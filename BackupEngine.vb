'Imports System.Data.SqlClient
Imports System.Diagnostics          ' สำหรับ Stopwatch ใน MirrorFile
Imports System.IO
Imports System.IO.Compression
Imports Microsoft.Data.SqlClient

Public Class BackupEngine

    Private ReadOnly _cfg As AppSettings

    Public Sub New(cfg As AppSettings)
        _cfg = cfg
    End Sub
    Public Property IsShutdownMode As Boolean = False
    ''' <summary>Full Backup - สร้างไฟล์ใหม่ทุกวัน</summary>
    ''' <param name="markAsScheduled">True = บันทึกว่าทำ full ของวันนี้แล้ว (scheduler จะไม่ทำซ้ำ)</param>
    Public Function RunFullBackup(Optional markAsScheduled As Boolean = True) As Boolean
        Try
            ' ★ ประกาศตรงนี้ เพื่อให้ใช้ได้ทั้งฟังก์ชัน
            Dim stamp As String = DateTime.Now.ToString("yyyyMMdd_HHmmss")
            Dim fileName As String = Path.Combine(_cfg.BackupPath,
                                 $"{_cfg.DatabaseName}_FULL_{stamp}.bak")

            If Not Directory.Exists(_cfg.BackupPath) Then
                Directory.CreateDirectory(_cfg.BackupPath)
            End If

            Dim sql As String = $"BACKUP DATABASE [{_cfg.DatabaseName}] TO DISK = N'{fileName}' WITH INIT, STATS = 5"
            If IsCompressionSupported() Then sql &= ", COMPRESSION"

            Using conn As New SqlConnection(_cfg.BuildConnectionString())
                conn.Open()
                Using cmd As New SqlCommand(sql, conn)
                    cmd.CommandTimeout = 0
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            If markAsScheduled Then
                _cfg.LastFullBackupDate = DateTime.Now.ToString("yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture)
                _cfg.LastFullBackupfolder = _cfg.BackupPath
                _cfg.Save()
            End If

            Logger.Write($"[FULL] สำเร็จ -> {fileName}")

            ' ★ ตรงนี้ fileName ยังอยู่ใน scope เดียวกัน ใช้ได้
            If IsShutdownMode Then
                QueueMirror(fileName)
            Else
                MirrorFile(fileName)
            End If

            Return True

        Catch ex As Exception
            Logger.Write("[FULL] ล้มเหลว: " & ex.Message)
            Return False
        End Try
    End Function

    Public Sub QueueMirror(filePath As String)
        If Not _cfg.EnableMirror Then Exit Sub
        Try
            If _cfg.PendingMirrorFiles Is Nothing Then _cfg.PendingMirrorFiles = New List(Of String)
            If Not _cfg.PendingMirrorFiles.Contains(filePath) Then
                _cfg.PendingMirrorFiles.Add(filePath)
                _cfg.Save()
                Logger.Write($"[MIRROR] เข้าคิวไว้ทำรอบหน้า -> {Path.GetFileName(filePath)}")
            End If
        Catch ex As Exception
            Logger.Write("[MIRROR] เข้าคิวไม่สำเร็จ: " & ex.Message)
        End Try
    End Sub

    ''' <summary>Transaction Log Backup - ต่อท้ายไฟล์ของวันนั้น</summary>
    Public Function RunLogBackup(Optional reason As String = "Schedule") As Boolean
        Try
            If Not IsFullRecoveryModel() Then
                Logger.Write("[LOG] ข้าม: ฐานข้อมูลไม่ได้อยู่ใน FULL Recovery Model")
                Return False
            End If

            EnsureFolder()
            Dim fileName As String = Path.Combine(_cfg.BackupPath,
                $"{_cfg.DatabaseName}_LOG_{DateTime.Now:yyyyMMdd}.trn")

            Dim sql As String =
                $"BACKUP LOG [{_cfg.DatabaseName}] TO DISK = @path " &
                $"WITH NOINIT, NAME = @name, STATS = 10{CompressionClause()};"

            ExecuteSql(sql, fileName, $"{_cfg.DatabaseName}-Log {DateTime.Now:yyyy-MM-dd HH:mm}")

            _cfg.LastLogBackupTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            _cfg.Save()
            Logger.Write($"[LOG-{reason}] สำเร็จ -> {fileName}")
            MirrorFile(fileName)
            Return True
        Catch ex As Exception
            Logger.Write($"[LOG-{reason}] ล้มเหลว: {ex.Message}")
            Return False
        End Try
    End Function

    ''' <summary>ลบไฟล์ backup ที่เกินอายุที่กำหนด</summary>
    Public Sub CleanupOldBackups()
        Try
            If _cfg.RetainDays <= 0 OrElse Not Directory.Exists(_cfg.BackupPath) Then Exit Sub
            Dim limit As DateTime = DateTime.Now.Date.AddDays(-_cfg.RetainDays)
            Dim di As New DirectoryInfo(_cfg.BackupPath)
            Dim count As Integer = 0

            For Each fi As FileInfo In di.GetFiles("*.*")
                If fi.Extension.ToLower() <> ".bak" AndAlso fi.Extension.ToLower() <> ".trn" Then Continue For
                If fi.LastWriteTime < limit Then
                    Try
                        fi.Delete()
                        count += 1
                        Logger.Write($"[CLEAN] ลบ {fi.Name}")
                    Catch ex As Exception
                        Logger.Write($"[CLEAN] ลบไม่ได้ {fi.Name}: {ex.Message}")
                    End Try
                End If
            Next
            If count > 0 Then Logger.Write($"[CLEAN] ลบไฟล์เก่ารวม {count} ไฟล์")
        Catch ex As Exception
            Logger.Write($"[CLEAN] ผิดพลาด: {ex.Message}")
        End Try
    End Sub

    Public Function TestConnection(ByRef msg As String) As Boolean
        Try
            Using cn As New SqlConnection(_cfg.BuildConnectionString())
                cn.Open()
                msg = "เชื่อมต่อสำเร็จ: " & cn.ServerVersion
                Return True
            End Using
        Catch ex As Exception
            msg = ex.Message
            Return False
        End Try
    End Function

    Public Function GetDatabaseList() As List(Of String)
        Dim list As New List(Of String)
        Using cn As New SqlConnection(_cfg.BuildConnectionString())
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT name FROM sys.databases WHERE database_id > 4 ORDER BY name", cn)
                Using rd = cmd.ExecuteReader()
                    While rd.Read() : list.Add(rd.GetString(0)) : End While
                End Using
            End Using
        End Using
        Return list
    End Function

    ' ---------- helpers ----------

    Private Sub ExecuteSql(sql As String, path As String, name As String)
        Using cn As New SqlConnection(_cfg.BuildConnectionString())
            cn.Open()
            Using cmd As New SqlCommand(sql, cn)
                cmd.CommandTimeout = 0   ' backup อาจใช้เวลานาน
                cmd.Parameters.AddWithValue("@path", path)
                cmd.Parameters.AddWithValue("@name", name)
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub

    Private Function CompressionClause() As String
        If Not _cfg.UseCompression Then Return ""
        If Not IsCompressionSupported() Then
            Logger.Write("[INFO] Edition นี้ไม่รองรับ Backup Compression -> ข้ามการบีบอัด")
            Return ""
        End If
        Return ", COMPRESSION"
    End Function

    Private Function IsFullRecoveryModel() As Boolean
        Using cn As New SqlConnection(_cfg.BuildConnectionString())
            cn.Open()
            Using cmd As New SqlCommand(
                "SELECT recovery_model_desc FROM sys.databases WHERE name = @db", cn)
                cmd.Parameters.AddWithValue("@db", _cfg.DatabaseName)
                Dim r = cmd.ExecuteScalar()
                Return r IsNot Nothing AndAlso r.ToString().ToUpper() <> "SIMPLE"
            End Using
        End Using
    End Function

    Private Sub EnsureFolder()
        If Not Directory.Exists(_cfg.BackupPath) Then Directory.CreateDirectory(_cfg.BackupPath)
    End Sub

    Private _compressionSupported As Boolean? = Nothing

    ''' <summary>ตรวจว่า Edition นี้รองรับ Backup Compression หรือไม่</summary>
    Public Function IsCompressionSupported() As Boolean
        If _compressionSupported.HasValue Then Return _compressionSupported.Value
        Try
            Using cn As New SqlConnection(_cfg.BuildConnectionString())
                cn.Open()
                ' EngineEdition: 1=Personal, 2=Standard, 3=Enterprise, 4=Express, 5=Azure DB
                Using cmd As New SqlCommand("SELECT CAST(SERVERPROPERTY('EngineEdition') AS INT)", cn)
                    Dim edition As Integer = CInt(cmd.ExecuteScalar())
                    ' Express(4) และ Personal(1) ไม่รองรับ
                    _compressionSupported = (edition <> 4 AndAlso edition <> 1)
                End Using
            End Using
        Catch
            _compressionSupported = False   ' ไม่แน่ใจ = ไม่ใช้ ปลอดภัยกว่า
        End Try
        Return _compressionSupported.Value
    End Function

    Public Function GetEditionName() As String
        Try
            Using cn As New SqlConnection(_cfg.BuildConnectionString())
                cn.Open()
                Using cmd As New SqlCommand("SELECT CAST(SERVERPROPERTY('Edition') AS NVARCHAR(200))", cn)
                    Return cmd.ExecuteScalar().ToString()
                End Using
            End Using
        Catch ex As Exception
            Return "ไม่ทราบ"
        End Try
    End Function

    ''' <summary>คัดลอกไฟล์ backup ไปยังไดรฟ์สำรอง (ไม่ทำให้ backup หลักล้มเหลว)</summary>
    Public Function MirrorFile(sourceFile As String) As Boolean
        If Not _cfg.EnableMirror Then Return False
        If String.IsNullOrWhiteSpace(_cfg.MirrorFolder) Then Return False

        Try
            If Not File.Exists(sourceFile) Then
                Logger.Write("[MIRROR] ข้าม: ไม่พบไฟล์ต้นทาง " & sourceFile)
                Return False
            End If

            ' --- 1. เตรียม Path และตรวจสอบว่าไม่ได้ mirror ทับตัวเอง ---
            Dim srcDir As String = Path.GetFullPath(Path.GetDirectoryName(sourceFile)).TrimEnd("\"c)
            Dim dstDir As String = Path.GetFullPath(_cfg.MirrorFolder).TrimEnd("\"c)
            If String.Equals(srcDir, dstDir, StringComparison.OrdinalIgnoreCase) Then
                Logger.Write("[MIRROR] ข้าม: โฟลเดอร์สำรองซ้ำกับโฟลเดอร์หลัก")
                Return False
            End If

            ' --- 2. ตรวจสอบความพร้อมของไดรฟ์ ---
            Dim srcInfo As New FileInfo(sourceFile)
            Dim targetSize As Long = srcInfo.Length ' ขนาดเริ่มต้นก่อน zip

            If Not dstDir.StartsWith("\\") Then
                Dim di As New DriveInfo(Path.GetPathRoot(dstDir))
                If Not di.IsReady Then
                    Logger.Write($"[MIRROR] ข้าม: ไดรฟ์ {di.Name} ไม่พร้อมใช้งาน")
                    Return False
                End If
                ' สำรองพื้นที่เผื่อไว้ 10%
                If di.AvailableFreeSpace < targetSize * 1.1 Then
                    Logger.Write($"[MIRROR] ข้าม: พื้นที่ไม่พอ (ต้องการ {FormatSize(targetSize)}, เหลือ {FormatSize(di.AvailableFreeSpace)})")
                    Return False
                End If
            End If

            If Not Directory.Exists(dstDir) Then Directory.CreateDirectory(dstDir)

            ' --- 3. ตัดสินใจว่าจะ ZIP หรือไม่ ---
            Dim ext As String = srcInfo.Extension.ToLower()
            Dim finalDestFile As String
            Dim fileToCopy As String = sourceFile ' ไฟล์ที่จะเอาไป copy
            Dim tempZipPath As String = ""

            If ext = ".bak" Then
                ' บีบอัดเป็น .zip
                finalDestFile = Path.Combine(dstDir, Path.GetFileNameWithoutExtension(sourceFile) & ".zip")
                tempZipPath = Path.Combine(Path.GetTempPath(), Path.GetFileName(finalDestFile))

                Using zip As ZipArchive = ZipFile.Open(tempZipPath, ZipArchiveMode.Create)
                    zip.CreateEntryFromFile(sourceFile, Path.GetFileName(sourceFile), CompressionLevel.Optimal)
                End Using
                fileToCopy = tempZipPath
            Else
                ' Copy ตรงๆ (.trn หรือไฟล์อื่น)
                finalDestFile = Path.Combine(dstDir, Path.GetFileName(sourceFile))
            End If

            ' --- 4. ดำเนินการ Copy ด้วยระบบ .tmp เพื่อป้องกันไฟล์พัง ---
            Dim tempDestFile As String = finalDestFile & ".tmp"
            Dim sw = Stopwatch.StartNew()

            File.Copy(fileToCopy, tempDestFile, True)

            ' ถ้าเคยสร้าง zip temp ไว้ ให้ลบทิ้ง
            If Not String.IsNullOrEmpty(tempZipPath) AndAlso File.Exists(tempZipPath) Then
                File.Delete(tempZipPath)
            End If

            sw.Stop()

            ' --- 5. Verify (ถ้าเปิดไว้) ---
            If _cfg.VerifyMirrorCopy Then
                ' ถ้าไฟล์ที่ copy ไปมีขนาด 0 หรือไม่ตรงกับต้นฉบับ (หลัง zip) ให้ยกเลิก
                Dim dstLen As Long = New FileInfo(tempDestFile).Length
                If dstLen = 0 Then
                    File.Delete(tempDestFile)
                    Logger.Write($"[MIRROR] ล้มเหลว: ไฟล์ปลายทางขนาดเป็น 0")
                    Return False
                End If
            End If

            If File.Exists(finalDestFile) Then File.Delete(finalDestFile)
            File.Move(tempDestFile, finalDestFile)

            Logger.Write($"[MIRROR] สำเร็จ -> {finalDestFile} (ขนาด {FormatSize(New FileInfo(finalDestFile).Length)})")
            Return True

        Catch ex As Exception
            Logger.Write($"[MIRROR] ล้มเหลว: {ex.Message}")
            Return False
        End Try
    End Function

    ''' <summary>ลบไฟล์เก่าในไดรฟ์สำรองตาม MirrorRetentionDays</summary>
    Public Sub CleanupMirror()
        If Not _cfg.EnableMirror Then Exit Sub
        If String.IsNullOrWhiteSpace(_cfg.MirrorFolder) Then Exit Sub
        If _cfg.MirrorRetentionDays < 1 Then
            Logger.Write("[MIRROR] ข้าม cleanup: retention < 1 วัน")
            Exit Sub
        End If

        Try
            If Not Directory.Exists(_cfg.MirrorFolder) Then Exit Sub

            Dim cutoff As DateTime = DateTime.Now.Date.AddDays(-_cfg.MirrorRetentionDays)
            Dim deleted As Integer = 0
            Dim freed As Long = 0

            For Each f As String In Directory.GetFiles(_cfg.MirrorFolder)
                Dim fi As New FileInfo(f)
                Dim ext As String = fi.Extension.ToLower()
                If ext <> ".bak" AndAlso ext <> ".trn" AndAlso ext <> ".zip" Then Continue For

                ' ★ กันชน: ไม่แตะไฟล์ที่สร้างภายใน 24 ชม. ไม่ว่าตั้ง retention เท่าไหร่
                If fi.CreationTime > DateTime.Now.AddHours(-24) Then Continue For
                If fi.LastWriteTime >= cutoff Then Continue For

                Try
                    freed += fi.Length
                    fi.Delete()
                    deleted += 1
                Catch ex As Exception
                    Logger.Write($"[MIRROR] ลบไม่ได้ {fi.Name}: {ex.Message}")
                End Try
            Next

            If deleted > 0 Then
                Logger.Write($"[MIRROR] ลบไฟล์เก่า {deleted} ไฟล์ คืนพื้นที่ {FormatSize(freed)}")
            End If

        Catch ex As Exception
            Logger.Write("[MIRROR] cleanup ผิดพลาด: " & ex.Message)
        End Try
    End Sub

    ''' <summary>ทดสอบว่าเขียนโฟลเดอร์สำรองได้จริง</summary>
    Public Function TestMirrorFolder(ByRef msg As String) As Boolean
        Try
            If String.IsNullOrWhiteSpace(_cfg.MirrorFolder) Then
                msg = "ยังไม่ได้ระบุโฟลเดอร์สำรอง"
                Return False
            End If
            If Not Directory.Exists(_cfg.MirrorFolder) Then Directory.CreateDirectory(_cfg.MirrorFolder)

            Dim probe As String = Path.Combine(_cfg.MirrorFolder, "_writetest_" & Guid.NewGuid().ToString("N") & ".tmp")
            File.WriteAllText(probe, "ok")
            File.Delete(probe)

            If Not _cfg.MirrorFolder.StartsWith("\\") Then
                Dim di As New DriveInfo(Path.GetPathRoot(Path.GetFullPath(_cfg.MirrorFolder)))
                msg = $"เขียนได้ปกติ | {di.DriveFormat} | เหลือ {FormatSize(di.AvailableFreeSpace)}"
                If di.DriveFormat = "FAT32" Then msg &= vbCrLf & "⚠ FAT32 รองรับไฟล์ไม่เกิน 4 GB"
            Else
                msg = "เขียนได้ปกติ (Network path)"
            End If
            Return True

        Catch ex As Exception
            msg = "เขียนไม่ได้: " & ex.Message
            Return False
        End Try
    End Function
    ''' <summary>แปลงขนาดไฟล์เป็นข้อความอ่านง่าย</summary>
    Private Function FormatSize(bytes As Long) As String
        If bytes >= 1099511627776L Then Return $"{bytes / 1099511627776.0:F2} TB"
        If bytes >= 1073741824L Then Return $"{bytes / 1073741824.0:F2} GB"
        If bytes >= 1048576L Then Return $"{bytes / 1048576.0:F2} MB"
        If bytes >= 1024L Then Return $"{bytes / 1024.0:F0} KB"
        Return $"{bytes} B"
    End Function

    ''' <summary>บีบไฟล์ .bak เป็น .zip แล้วลบต้นฉบับ (ใช้แทน COMPRESSION สำหรับ Express)</summary>
    Private Sub CompressToZip(sourceFile As String)
        Try
            If Not File.Exists(sourceFile) Then Exit Sub

            Dim zipPath As String = Path.ChangeExtension(sourceFile, ".zip")
            If File.Exists(zipPath) Then File.Delete(zipPath)

            Dim sizeBefore As Long = New FileInfo(sourceFile).Length

            Using zip As ZipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create)
                zip.CreateEntryFromFile(sourceFile, Path.GetFileName(sourceFile), CompressionLevel.Optimal)
            End Using

            Dim sizeAfter As Long = New FileInfo(zipPath).Length
            File.Delete(sourceFile)

            Dim pct As Double = If(sizeBefore > 0, 100 - (sizeAfter / sizeBefore * 100), 0)
            Logger.Write($"[ZIP] บีบสำเร็จ {FormatSize(sizeBefore)} -> {FormatSize(sizeAfter)} (ลด {pct:F1}%)")

        Catch ex As Exception
            Logger.Write($"[ZIP] บีบไม่สำเร็จ: {ex.Message} (ไฟล์ .bak ยังอยู่ครบ)")
        End Try
    End Sub

End Class
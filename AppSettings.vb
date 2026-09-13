Imports System.IO
Imports System.Xml.Serialization
Imports Microsoft.Data.SqlClient
<Serializable()>
Public Class AppSettings

    Public Property ServerName As String = "localhost"
    Public Property DatabaseName As String = ""
    Public Property UseWindowsAuth As Boolean = True
    Public Property UserName As String = ""
    Public Property Password As String = ""

    ' เวลาเริ่ม Full Backup ประจำวัน (เก็บเป็น string HH:mm)
    Public Property FullBackupTime As String = "02:00"
    Public Property EnableLogBackup As Boolean = True
    Public Property LogIntervalMinutes As Integer = 60
    Public Property BackupOnShutdown As Boolean = True

    Public Property BackupPath As String = "C:\SQLBackup"
    Public Property RetainDays As Integer = 7
    Public Property UseCompression As Boolean = False 'ใช้กับ express
    Public Property ZipAfterBackup As Boolean = True
    ' สถานะภายใน (ไม่ต้องแก้เอง)
    Public Property LastFullBackupfolder As String = ""
    Public Property LastFullBackupDate As String = ""
    Public Property LastLogBackupTime As String = ""

    Private Shared ReadOnly ConfigFile As String =
        Path.Combine(Application.StartupPath, "BackupSettings.xml")
    ''' <summary>เปิด/ปิดการสำรองซ้ำไปไดรฟ์ที่ 2</summary>
    Public Property EnableMirror As Boolean = False

    ''' <summary>โฟลเดอร์ปลายทางสำรอง เช่น E:\SQLBackup หรือ \\NAS\Backup</summary>
    Public Property MirrorFolder As String = ""

    ''' <summary>จำนวนวันที่เก็บย้อนหลังในไดรฟ์สำรอง (แยกจากตัวหลักได้)</summary>
    Public Property MirrorRetentionDays As Integer = 14

    ''' <summary>ตรวจสอบขนาดไฟล์หลังคัดลอกเสร็จ</summary>
    Public Property VerifyMirrorCopy As Boolean = True
    ''' <summary>ไฟล์ที่ยังไม่ได้ mirror (ค้างจากตอน shutdown)</summary>
    Public Property PendingMirrorFiles As New List(Of String)


    Public Shared Function Load() As AppSettings
        Try
            If File.Exists(ConfigFile) Then
                Dim ser As New XmlSerializer(GetType(AppSettings))
                Using fs As New FileStream(ConfigFile, FileMode.Open, FileAccess.Read)
                    Return CType(ser.Deserialize(fs), AppSettings)
                End Using
            End If
        Catch ex As Exception
            Logger.Write("โหลด config ไม่สำเร็จ: " & ex.Message)
        End Try
        Return New AppSettings()
    End Function

    Public Sub Save()
        Try
            Dim ser As New XmlSerializer(GetType(AppSettings))
            Using fs As New FileStream(ConfigFile, FileMode.Create, FileAccess.Write)
                ser.Serialize(fs, Me)
            End Using
        Catch ex As Exception
            Logger.Write("บันทึก config ไม่สำเร็จ: " & ex.Message)
        End Try
    End Sub

    Public Function BuildConnectionString() As String
        Dim sb As New SqlConnectionStringBuilder()

        sb.DataSource = ServerName

        sb.InitialCatalog = If(String.IsNullOrWhiteSpace(DatabaseName), "master", DatabaseName)


        If UseWindowsAuth Then

            sb.IntegratedSecurity = True

        Else

            sb.UserID = UserName

            sb.Password = Password
        End If


        ' ★ 2 บรรทัดนี้จำเป็นสำหรับ SQL Server ในเครือข่ายภายใน

        sb.Encrypt = True                      ' เข้ารหัสการเชื่อมต่อ

        sb.TrustServerCertificate = True       ' ยอมรับ self-signed cert


        sb.ConnectTimeout = 30

        sb.ApplicationName = "SqlBackupTool"

        Return sb.ConnectionString
    End Function

End Class

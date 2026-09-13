Imports System.IO
Imports Microsoft.Win32

Public Class Form1

    Private cfg As AppSettings
    Private engine As BackupEngine
    Private isRunning As Boolean = False
    Private shutdownHandled As Boolean = False

    Private Const WM_QUERYENDSESSION As Integer = &H11
    Private Const WM_ENDSESSION As Integer = &H16

    ' ================= Form Load / Close =================

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        AddHandler Logger.OnLog, AddressOf AppendLog
        AddHandler SystemEvents.SessionEnding, AddressOf OnSessionEnding

        cfg = AppSettings.Load()
        engine = New BackupEngine(cfg)
        LoadSettingsToUI()

        NotifyIcon1.Icon = Me.Icon
        Logger.Write("โปรแกรมเริ่มทำงาน")

        ' เริ่มตัวจับเวลาอัตโนมัติทันทีถ้าตั้งค่าครบ
        If cfg.DatabaseName <> "" Then
            NotifyIcon1.Visible = True
            Me.WindowState = FormWindowState.Minimized
            NotifyIcon1.ShowBalloonTip(2000, "SQL Backup Tool",
                "โปรแกรมยังทำงานอยู่เบื้องหลัง", ToolTipIcon.Info)
            StartScheduler()

        End If
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        ' ปิดเครื่อง / Log off -> backup log ก่อน
        If e.CloseReason = CloseReason.WindowsShutDown Then
            DoShutdownBackup()
        ElseIf e.CloseReason = CloseReason.UserClosing AndAlso isRunning Then
            ' ย่อลง tray แทนการปิด
            e.Cancel = True
            Me.Hide()
            NotifyIcon1.ShowBalloonTip(2000, "SQL Backup Tool",
                "โปรแกรมยังทำงานอยู่เบื้องหลัง", ToolTipIcon.Info)
            Return
        End If
        RemoveHandler SystemEvents.SessionEnding, AddressOf OnSessionEnding
        NotifyIcon1.Visible = False
    End Sub

    ' ================= ตรวจจับ Shutdown =================

    Protected Overrides Sub WndProc(ByRef m As Message)
        If m.Msg = WM_QUERYENDSESSION OrElse m.Msg = WM_ENDSESSION Then
            DoShutdownBackup()
        End If
        MyBase.WndProc(m)
    End Sub

    Private Sub OnSessionEnding(sender As Object, e As SessionEndingEventArgs)
        DoShutdownBackup()
    End Sub

    Private Sub DoShutdownBackup()
        If shutdownHandled Then Exit Sub
        shutdownHandled = True
        If cfg.BackupOnShutdown AndAlso cfg.DatabaseName <> "" Then
            Logger.Write("ตรวจพบการปิดเครื่อง -> เริ่ม Log Backup")
            engine.RunLogBackup("Shutdown")
        End If
    End Sub

    ' ================= Scheduler =================

    Private Async Sub tmrScheduler_Tick(sender As Object, e As EventArgs) Handles tmrScheduler.Tick
        ' --- ถ้ายังทำงานค้างอยู่ ให้ข้ามรอบนี้ไปเลย ---
        If Not TryAcquire() Then Exit Sub

        Try
            Dim now As DateTime = DateTime.Now
            Dim today As String = now.ToString("yyyy-MM-dd", Globalization.CultureInfo.InvariantCulture)

            Dim schedule As DateTime
            If Not DateTime.TryParse(cfg.FullBackupTime, schedule) Then Exit Sub
            Dim todayTarget As New DateTime(now.Year, now.Month, now.Day, schedule.Hour, schedule.Minute, 0)

            Dim folderChanged As Boolean =
            Not String.Equals(cfg.LastFullBackupFolder?.TrimEnd("\"c),
                              cfg.BackupPath?.TrimEnd("\"c),
                              StringComparison.OrdinalIgnoreCase)

            ' --- 1) Full Backup (ทำใน background thread) ---
            If (cfg.LastFullBackupDate <> today OrElse folderChanged) AndAlso now >= todayTarget Then
                SetUiBusy(True, "กำลังทำ Full Backup...")
                Await Task.Run(Sub()
                                   If engine.RunFullBackup() Then
                                       engine.CleanupOldBackups()
                                       engine.CleanupMirror()
                                   End If
                               End Sub)
                SetUiBusy(False, "")
                UpdateStatus(todayTarget)
                Exit Try          ' ข้าม log รอบนี้ ให้รอบหน้าทำ
            End If

            ' --- 2) Log Backup ---
            If cfg.EnableLogBackup Then
                Dim last As DateTime
                If DateTime.TryParse(cfg.LastLogBackupTime, last) Then
                    If now.Subtract(last).TotalMinutes >= cfg.LogIntervalMinutes Then
                        SetUiBusy(True, "กำลังทำ Log Backup...")
                        Await Task.Run(Sub() engine.RunLogBackup("Schedule"))
                        SetUiBusy(False, "")
                    End If
                Else
                    cfg.LastLogBackupTime = now.ToString("yyyy-MM-dd HH:mm:ss", Globalization.CultureInfo.InvariantCulture)
                    cfg.Save()
                End If
            End If

            UpdateStatus(todayTarget)

        Catch ex As Exception
            Logger.Write("[SCHED] ผิดพลาด: " & ex.Message)
        Finally
            Release()          ' ★ ต้องปลดล็อกเสมอ ไม่ว่าจะสำเร็จหรือ error
        End Try
    End Sub

    Private Sub SetUiBusy(busy As Boolean, msg As String)
        btnBackupNow.Enabled = Not busy
        btnLogNow.Enabled = Not busy
        btnSave.Enabled = Not busy
        If busy Then
            lblNextRun.Text = msg
            lstLog.Text = "SQL Backup Tool - " & msg
        Else
            lstLog.Text = "SQL Backup Tool"
        End If
    End Sub

    Private Sub UpdateStatus(todayTarget As DateTime)
        Dim nextFull As DateTime = If(cfg.LastFullBackupDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                      todayTarget.AddDays(1), todayTarget)
        lblNextRun.Text = $"สถานะ : กำลังทำงาน   |   Full ครั้งถัดไป : {nextFull:dd/MM/yyyy HH:mm}   |   " &
                          $"Log ล่าสุด : {If(cfg.LastLogBackupTime = "", "-", cfg.LastLogBackupTime)}"
    End Sub

    ' ================= Buttons =================

    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        If Not ValidateInput() Then Exit Sub
        SaveSettingsFromUI()
        StartScheduler()
    End Sub

    Private Sub StartScheduler()
        isRunning = True
        tmrScheduler.Start()
        btnStart.Enabled = False
        btnStop.Enabled = True
        SetInputEnabled(False)
        Logger.Write("เริ่มตัวจับเวลาอัตโนมัติแล้ว")
        engine.CleanupOldBackups()
    End Sub

    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        isRunning = False
        tmrScheduler.Stop()
        btnStart.Enabled = True
        btnStop.Enabled = False
        SetInputEnabled(True)
        lblNextRun.Text = "สถานะ : หยุดทำงาน"
        Logger.Write("หยุดตัวจับเวลา")
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not ValidateInput() Then Exit Sub
        SaveSettingsFromUI()
        MessageBox.Show("บันทึกการตั้งค่าเรียบร้อย", "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Async Sub btnBackupNow_Click(sender As Object, e As EventArgs) Handles btnBackupNow.Click
        If Not TryAcquire() Then
            MessageBox.Show("ระบบกำลังสำรองข้อมูลอยู่ กรุณารอสักครู่", "กำลังทำงาน",
                        MessageBoxButtons.OK, MessageBoxIcon.Information)
            Exit Sub
        End If

        Try
            SetUiBusy(True, "กำลังทำ Full Backup...")
            Await Task.Run(Sub()
                               If engine.RunFullBackup(markAsScheduled:=False) Then
                                   engine.CleanupOldBackups()
                                   engine.CleanupMirror()
                               End If
                           End Sub)
        Finally
            SetUiBusy(False, "")
            Release()
        End Try
    End Sub

    Private Sub btnLogNow_Click(sender As Object, e As EventArgs) Handles btnLogNow.Click
        If Not ValidateInput() Then Exit Sub
        SaveSettingsFromUI()
        Cursor = Cursors.WaitCursor
        engine.RunLogBackup("Manual")
        Cursor = Cursors.Default
    End Sub

    Private Sub btnTestConnection_Click(sender As Object, e As EventArgs) Handles btnTestConnection.Click
        SaveSettingsFromUI()
        Dim msg As String = ""
        If engine.TestConnection(msg) Then
            CheckServerCapability()
            MessageBox.Show(msg, "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show(msg, "เชื่อมต่อไม่สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub btnLoadDb_Click(sender As Object, e As EventArgs) Handles btnLoadDb.Click
        Try
            SaveSettingsFromUI()
            Dim current As String = cboDatabase.Text
            cboDatabase.DataSource = engine.GetDatabaseList()
            cboDatabase.Text = current
            CheckServerCapability()
        Catch ex As Exception
            MessageBox.Show("โหลดรายชื่อไม่สำเร็จ: " & ex.Message, "ผิดพลาด",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnBrowse_Click(sender As Object, e As EventArgs) Handles btnBrowse.Click
        If FolderBrowserDialog1.ShowDialog = DialogResult.OK Then
            txtBackupPath.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub chkWindowsAuth_CheckedChanged(sender As Object, e As EventArgs) Handles chkWindowsAuth.CheckedChanged
        txtUser.Enabled = Not chkWindowsAuth.Checked
        txtPassword.Enabled = Not chkWindowsAuth.Checked
    End Sub

    Private Sub NotifyIcon1_DoubleClick(sender As Object, e As EventArgs) Handles NotifyIcon1.DoubleClick
        Me.Show()
        Me.WindowState = FormWindowState.Normal
        Me.BringToFront()
    End Sub

    ' ================= Helpers =================

    Private Sub LoadSettingsToUI()
        txtServer.Text = cfg.ServerName
        cboDatabase.Text = cfg.DatabaseName
        chkWindowsAuth.Checked = cfg.UseWindowsAuth
        txtUser.Text = cfg.UserName
        txtPassword.Text = cfg.Password
        dtpFullTime.Value = DateTime.Parse(cfg.FullBackupTime)
        chkEnableLog.Checked = cfg.EnableLogBackup
        numLogInterval.Value = cfg.LogIntervalMinutes
        chkBackupOnShutdown.Checked = cfg.BackupOnShutdown
        txtBackupPath.Text = cfg.BackupPath
        numRetainDays.Value = cfg.RetainDays
        chkCompression.Checked = cfg.UseCompression
        txtUser.Enabled = Not chkWindowsAuth.Checked
        txtPassword.Enabled = Not chkWindowsAuth.Checked
        chkMirror.Checked = cfg.EnableMirror
        txtMirrorFolder.Text = cfg.MirrorFolder
        numMirrorRetention.Value = Math.Max(1, cfg.MirrorRetentionDays)
        chkVerifyMirror.Checked = cfg.VerifyMirrorCopy
    End Sub

    Private Sub SaveSettingsFromUI()
        cfg.ServerName = txtServer.Text.Trim()
        cfg.DatabaseName = cboDatabase.Text.Trim()
        cfg.UseWindowsAuth = chkWindowsAuth.Checked
        cfg.UserName = txtUser.Text.Trim()
        cfg.Password = txtPassword.Text
        cfg.FullBackupTime = dtpFullTime.Value.ToString("HH:mm")
        cfg.EnableLogBackup = chkEnableLog.Checked
        cfg.LogIntervalMinutes = CInt(numLogInterval.Value)
        cfg.BackupOnShutdown = chkBackupOnShutdown.Checked
        cfg.BackupPath = txtBackupPath.Text.Trim()
        cfg.RetainDays = CInt(numRetainDays.Value)
        cfg.UseCompression = chkCompression.Checked
        cfg.EnableMirror = chkMirror.Checked
        cfg.MirrorFolder = txtMirrorFolder.Text.Trim()
        cfg.MirrorRetentionDays = CInt(numMirrorRetention.Value)
        cfg.VerifyMirrorCopy = chkVerifyMirror.Checked
        cfg.Save()
    End Sub

    Private Function ValidateInput() As Boolean
        If txtServer.Text.Trim() = "" Then
            MessageBox.Show("กรุณาระบุชื่อ Server") : Return False
        End If
        If cboDatabase.Text.Trim() = "" Then
            MessageBox.Show("กรุณาระบุชื่อฐานข้อมูล") : Return False
        End If
        If txtBackupPath.Text.Trim() = "" Then
            MessageBox.Show("กรุณาระบุโฟลเดอร์เก็บไฟล์") : Return False
        End If
        Return True
    End Function

    Private Sub SetInputEnabled(state As Boolean)
        grpConnection.Enabled = state
        grpSchedule.Enabled = state
        grpStorage.Enabled = state
    End Sub

    Private Sub AppendLog(message As String)
        If lstLog.InvokeRequired Then
            lstLog.Invoke(Sub() AppendLog(message))
            Return
        End If
        lstLog.Items.Insert(0, message)
        If lstLog.Items.Count > 500 Then lstLog.Items.RemoveAt(lstLog.Items.Count - 1)
    End Sub
    Private Sub CheckServerCapability()
        Try
            Dim edition As String = engine.GetEditionName()
            If engine.IsCompressionSupported() Then
                chkCompression.Enabled = True
                chkCompression.Text = "ใช้ Backup Compression"
            Else
                chkCompression.Checked = False
                chkCompression.Enabled = False
                chkCompression.Text = "Backup Compression (Edition นี้ไม่รองรับ)"
            End If
            Logger.Write($"[INFO] SQL Server Edition: {edition}")
        Catch
            ' ยังต่อไม่ติด ปล่อยผ่าน
        End Try
    End Sub
    Private Sub btnBrowseMirror_Click(sender As Object, e As EventArgs) Handles btnBrowseMirror.Click
        Using fbd As New FolderBrowserDialog()
            fbd.Description = "เลือกโฟลเดอร์สำรอง (ไดรฟ์ที่ 2)"
            If fbd.ShowDialog() = DialogResult.OK Then txtMirrorFolder.Text = fbd.SelectedPath
        End Using
    End Sub

    Private Sub btnTestMirror_Click(sender As Object, e As EventArgs) Handles btnTestMirror.Click
        cfg.MirrorFolder = txtMirrorFolder.Text.Trim()
        Dim msg As String = ""
        If engine.TestMirrorFolder(msg) Then
            MessageBox.Show(msg, "ทดสอบสำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            MessageBox.Show(msg, "ทดสอบไม่ผ่าน", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End If
    End Sub

    Private Sub chkMirror_CheckedChanged(sender As Object, e As EventArgs) Handles chkMirror.CheckedChanged
        txtMirrorFolder.Enabled = chkMirror.Checked
        btnBrowseMirror.Enabled = chkMirror.Checked
        btnTestMirror.Enabled = chkMirror.Checked
        numMirrorRetention.Enabled = chkMirror.Checked
        chkVerifyMirror.Enabled = chkMirror.Checked
    End Sub
    Private _busy As Integer = 0      ' 0 = ว่าง, 1 = กำลังทำงาน


    ''' <summary>จองสิทธิ์ทำงาน - คืน True ถ้าจองได้</summary>

    Private Function TryAcquire() As Boolean

        Return Threading.Interlocked.CompareExchange(_busy, 1, 0) = 0

    End Function


    Private Sub Release()

        Threading.Interlocked.Exchange(_busy, 0)

    End Sub

End Class
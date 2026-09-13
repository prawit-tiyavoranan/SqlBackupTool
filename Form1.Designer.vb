<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        components = New ComponentModel.Container()
        grpConnection = New GroupBox()
        lblServer = New Label()
        txtServer = New TextBox()
        btnTestConnection = New Button()
        lblDatabase = New Label()
        cboDatabase = New ComboBox()
        btnLoadDb = New Button()
        chkWindowsAuth = New CheckBox()
        lblUser = New Label()
        txtUser = New TextBox()
        txtPassword = New TextBox()
        grpSchedule = New GroupBox()
        lblFullTime = New Label()
        dtpFullTime = New DateTimePicker()
        chkEnableLog = New CheckBox()
        lblLogInterval = New Label()
        numLogInterval = New NumericUpDown()
        chkBackupOnShutdown = New CheckBox()
        grpStorage = New GroupBox()
        btnTestMirror = New Button()
        lblPath = New Label()
        chkVerifyMirror = New CheckBox()
        txtBackupPath = New TextBox()
        btnBrowse = New Button()
        Label1 = New Label()
        lblRetain = New Label()
        numRetainDays = New NumericUpDown()
        txtMirrorFolder = New TextBox()
        chkCompression = New CheckBox()
        chkMirror = New CheckBox()
        btnBrowseMirror = New Button()
        numMirrorRetention = New NumericUpDown()
        Label2 = New Label()
        btnSave = New Button()
        btnStart = New Button()
        btnStop = New Button()
        btnBackupNow = New Button()
        btnLogNow = New Button()
        lblNextRun = New Label()
        lstLog = New ListBox()
        tmrScheduler = New Timer(components)
        NotifyIcon1 = New NotifyIcon(components)
        FolderBrowserDialog1 = New FolderBrowserDialog()
        grpConnection.SuspendLayout()
        grpSchedule.SuspendLayout()
        CType(numLogInterval, ComponentModel.ISupportInitialize).BeginInit()
        grpStorage.SuspendLayout()
        CType(numRetainDays, ComponentModel.ISupportInitialize).BeginInit()
        CType(numMirrorRetention, ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()
        ' 
        ' grpConnection
        ' 
        grpConnection.Controls.Add(lblServer)
        grpConnection.Controls.Add(txtServer)
        grpConnection.Controls.Add(btnTestConnection)
        grpConnection.Controls.Add(lblDatabase)
        grpConnection.Controls.Add(cboDatabase)
        grpConnection.Controls.Add(btnLoadDb)
        grpConnection.Controls.Add(chkWindowsAuth)
        grpConnection.Controls.Add(lblUser)
        grpConnection.Controls.Add(txtUser)
        grpConnection.Controls.Add(txtPassword)
        grpConnection.Location = New Point(12, 11)
        grpConnection.Name = "grpConnection"
        grpConnection.Size = New Size(660, 145)
        grpConnection.TabIndex = 0
        grpConnection.TabStop = False
        grpConnection.Text = "การเชื่อมต่อ SQL Server"
        ' 
        ' lblServer
        ' 
        lblServer.AutoSize = True
        lblServer.Location = New Point(18, 30)
        lblServer.Name = "lblServer"
        lblServer.Size = New Size(45, 15)
        lblServer.TabIndex = 0
        lblServer.Text = "Server :"
        ' 
        ' txtServer
        ' 
        txtServer.Location = New Point(120, 26)
        txtServer.Name = "txtServer"
        txtServer.Size = New Size(220, 23)
        txtServer.TabIndex = 1
        ' 
        ' btnTestConnection
        ' 
        btnTestConnection.Location = New Point(500, 24)
        btnTestConnection.Name = "btnTestConnection"
        btnTestConnection.Size = New Size(140, 26)
        btnTestConnection.TabIndex = 2
        btnTestConnection.Text = "ทดสอบการเชื่อมต่อ"
        btnTestConnection.UseVisualStyleBackColor = True
        ' 
        ' lblDatabase
        ' 
        lblDatabase.AutoSize = True
        lblDatabase.Location = New Point(18, 62)
        lblDatabase.Name = "lblDatabase"
        lblDatabase.Size = New Size(57, 15)
        lblDatabase.TabIndex = 3
        lblDatabase.Text = "ฐานข้อมูล :"
        ' 
        ' cboDatabase
        ' 
        cboDatabase.FormattingEnabled = True
        cboDatabase.Location = New Point(120, 59)
        cboDatabase.Name = "cboDatabase"
        cboDatabase.Size = New Size(220, 23)
        cboDatabase.TabIndex = 4
        ' 
        ' btnLoadDb
        ' 
        btnLoadDb.Location = New Point(350, 58)
        btnLoadDb.Name = "btnLoadDb"
        btnLoadDb.Size = New Size(120, 24)
        btnLoadDb.TabIndex = 5
        btnLoadDb.Text = "โหลดรายชื่อ DB"
        btnLoadDb.UseVisualStyleBackColor = True
        ' 
        ' chkWindowsAuth
        ' 
        chkWindowsAuth.AutoSize = True
        chkWindowsAuth.Checked = True
        chkWindowsAuth.CheckState = CheckState.Checked
        chkWindowsAuth.Location = New Point(120, 90)
        chkWindowsAuth.Name = "chkWindowsAuth"
        chkWindowsAuth.Size = New Size(172, 19)
        chkWindowsAuth.TabIndex = 6
        chkWindowsAuth.Text = "ใช้ Windows Authentication"
        chkWindowsAuth.UseVisualStyleBackColor = True
        ' 
        ' lblUser
        ' 
        lblUser.AutoSize = True
        lblUser.Location = New Point(18, 117)
        lblUser.Name = "lblUser"
        lblUser.Size = New Size(70, 15)
        lblUser.TabIndex = 7
        lblUser.Text = "User / Pass :"
        ' 
        ' txtUser
        ' 
        txtUser.Location = New Point(120, 113)
        txtUser.Name = "txtUser"
        txtUser.Size = New Size(140, 23)
        txtUser.TabIndex = 8
        ' 
        ' txtPassword
        ' 
        txtPassword.Location = New Point(270, 113)
        txtPassword.Name = "txtPassword"
        txtPassword.PasswordChar = "●"c
        txtPassword.Size = New Size(140, 23)
        txtPassword.TabIndex = 9
        ' 
        ' grpSchedule
        ' 
        grpSchedule.Controls.Add(lblFullTime)
        grpSchedule.Controls.Add(dtpFullTime)
        grpSchedule.Controls.Add(chkEnableLog)
        grpSchedule.Controls.Add(lblLogInterval)
        grpSchedule.Controls.Add(numLogInterval)
        grpSchedule.Controls.Add(chkBackupOnShutdown)
        grpSchedule.Location = New Point(12, 164)
        grpSchedule.Name = "grpSchedule"
        grpSchedule.Size = New Size(325, 253)
        grpSchedule.TabIndex = 1
        grpSchedule.TabStop = False
        grpSchedule.Text = "ตารางเวลา"
        ' 
        ' lblFullTime
        ' 
        lblFullTime.AutoSize = True
        lblFullTime.Location = New Point(18, 32)
        lblFullTime.Name = "lblFullTime"
        lblFullTime.Size = New Size(96, 15)
        lblFullTime.TabIndex = 0
        lblFullTime.Text = "เวลา Full Backup :"
        ' 
        ' dtpFullTime
        ' 
        dtpFullTime.Format = DateTimePickerFormat.Time
        dtpFullTime.Location = New Point(160, 28)
        dtpFullTime.Name = "dtpFullTime"
        dtpFullTime.ShowUpDown = True
        dtpFullTime.Size = New Size(140, 23)
        dtpFullTime.TabIndex = 1
        ' 
        ' chkEnableLog
        ' 
        chkEnableLog.AutoSize = True
        chkEnableLog.Checked = True
        chkEnableLog.CheckState = CheckState.Checked
        chkEnableLog.Location = New Point(21, 66)
        chkEnableLog.Name = "chkEnableLog"
        chkEnableLog.Size = New Size(135, 19)
        chkEnableLog.TabIndex = 2
        chkEnableLog.Text = "เปิดใช้งาน Log Backup"
        chkEnableLog.UseVisualStyleBackColor = True
        ' 
        ' lblLogInterval
        ' 
        lblLogInterval.AutoSize = True
        lblLogInterval.Location = New Point(38, 96)
        lblLogInterval.Name = "lblLogInterval"
        lblLogInterval.Size = New Size(65, 15)
        lblLogInterval.TabIndex = 3
        lblLogInterval.Text = "ทุก ๆ (นาที) :"
        ' 
        ' numLogInterval
        ' 
        numLogInterval.Location = New Point(160, 93)
        numLogInterval.Maximum = New Decimal(New Integer() {1440, 0, 0, 0})
        numLogInterval.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        numLogInterval.Name = "numLogInterval"
        numLogInterval.Size = New Size(90, 23)
        numLogInterval.TabIndex = 4
        numLogInterval.Value = New Decimal(New Integer() {60, 0, 0, 0})
        ' 
        ' chkBackupOnShutdown
        ' 
        chkBackupOnShutdown.AutoSize = True
        chkBackupOnShutdown.Checked = True
        chkBackupOnShutdown.CheckState = CheckState.Checked
        chkBackupOnShutdown.Location = New Point(21, 126)
        chkBackupOnShutdown.Name = "chkBackupOnShutdown"
        chkBackupOnShutdown.Size = New Size(212, 19)
        chkBackupOnShutdown.TabIndex = 5
        chkBackupOnShutdown.Text = "Backup Log เมื่อปิดเครื่อง (Shutdown)"
        chkBackupOnShutdown.UseVisualStyleBackColor = True
        ' 
        ' grpStorage
        ' 
        grpStorage.Controls.Add(btnTestMirror)
        grpStorage.Controls.Add(lblPath)
        grpStorage.Controls.Add(chkVerifyMirror)
        grpStorage.Controls.Add(txtBackupPath)
        grpStorage.Controls.Add(btnBrowse)
        grpStorage.Controls.Add(Label1)
        grpStorage.Controls.Add(lblRetain)
        grpStorage.Controls.Add(numRetainDays)
        grpStorage.Controls.Add(txtMirrorFolder)
        grpStorage.Controls.Add(chkCompression)
        grpStorage.Controls.Add(chkMirror)
        grpStorage.Controls.Add(btnBrowseMirror)
        grpStorage.Controls.Add(numMirrorRetention)
        grpStorage.Controls.Add(Label2)
        grpStorage.Location = New Point(347, 164)
        grpStorage.Name = "grpStorage"
        grpStorage.Size = New Size(325, 253)
        grpStorage.TabIndex = 2
        grpStorage.TabStop = False
        grpStorage.Text = "ที่เก็บไฟล์ / อายุไฟล์"
        ' 
        ' btnTestMirror
        ' 
        btnTestMirror.Location = New Point(226, 193)
        btnTestMirror.Name = "btnTestMirror"
        btnTestMirror.Size = New Size(93, 24)
        btnTestMirror.TabIndex = 7
        btnTestMirror.Text = "ทดสอบเขียน"
        btnTestMirror.UseVisualStyleBackColor = True
        ' 
        ' lblPath
        ' 
        lblPath.AutoSize = True
        lblPath.Location = New Point(18, 18)
        lblPath.Name = "lblPath"
        lblPath.Size = New Size(87, 15)
        lblPath.TabIndex = 0
        lblPath.Text = "โฟลเดอร์เก็บไฟล์ :"
        ' 
        ' chkVerifyMirror
        ' 
        chkVerifyMirror.AutoSize = True
        chkVerifyMirror.Checked = True
        chkVerifyMirror.CheckState = CheckState.Checked
        chkVerifyMirror.Location = New Point(21, 225)
        chkVerifyMirror.Name = "chkVerifyMirror"
        chkVerifyMirror.Size = New Size(158, 19)
        chkVerifyMirror.TabIndex = 6
        chkVerifyMirror.Text = "ตรวจสอบขนาดไฟล์หลัง copy"
        chkVerifyMirror.UseVisualStyleBackColor = True
        ' 
        ' txtBackupPath
        ' 
        txtBackupPath.Location = New Point(21, 39)
        txtBackupPath.Name = "txtBackupPath"
        txtBackupPath.Size = New Size(240, 23)
        txtBackupPath.TabIndex = 1
        ' 
        ' btnBrowse
        ' 
        btnBrowse.Location = New Point(267, 38)
        btnBrowse.Name = "btnBrowse"
        btnBrowse.Size = New Size(40, 24)
        btnBrowse.TabIndex = 2
        btnBrowse.Text = "..."
        btnBrowse.UseVisualStyleBackColor = True
        ' 
        ' Label1
        ' 
        Label1.AutoSize = True
        Label1.Location = New Point(18, 145)
        Label1.Name = "Label1"
        Label1.Size = New Size(114, 15)
        Label1.TabIndex = 0
        Label1.Text = "โฟลเดอร์เก็บไฟล์สำรอง :"
        ' 
        ' lblRetain
        ' 
        lblRetain.AutoSize = True
        lblRetain.Location = New Point(18, 71)
        lblRetain.Name = "lblRetain"
        lblRetain.Size = New Size(107, 15)
        lblRetain.TabIndex = 3
        lblRetain.Text = "เก็บไฟล์ย้อนหลัง (วัน) :"
        ' 
        ' numRetainDays
        ' 
        numRetainDays.Location = New Point(185, 68)
        numRetainDays.Maximum = New Decimal(New Integer() {365, 0, 0, 0})
        numRetainDays.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        numRetainDays.Name = "numRetainDays"
        numRetainDays.Size = New Size(90, 23)
        numRetainDays.TabIndex = 4
        numRetainDays.Value = New Decimal(New Integer() {7, 0, 0, 0})
        ' 
        ' txtMirrorFolder
        ' 
        txtMirrorFolder.Location = New Point(21, 166)
        txtMirrorFolder.Name = "txtMirrorFolder"
        txtMirrorFolder.Size = New Size(240, 23)
        txtMirrorFolder.TabIndex = 1
        ' 
        ' chkCompression
        ' 
        chkCompression.AutoSize = True
        chkCompression.Location = New Point(21, 95)
        chkCompression.Name = "chkCompression"
        chkCompression.Size = New Size(153, 19)
        chkCompression.TabIndex = 5
        chkCompression.Text = "ใช้ Backup Compression"
        chkCompression.UseVisualStyleBackColor = True
        ' 
        ' chkMirror
        ' 
        chkMirror.AutoSize = True
        chkMirror.Checked = True
        chkMirror.CheckState = CheckState.Checked
        chkMirror.Location = New Point(21, 124)
        chkMirror.Name = "chkMirror"
        chkMirror.Size = New Size(125, 19)
        chkMirror.TabIndex = 5
        chkMirror.Text = "Copyไป drive สำรอง"
        chkMirror.UseVisualStyleBackColor = True
        ' 
        ' btnBrowseMirror
        ' 
        btnBrowseMirror.Location = New Point(267, 165)
        btnBrowseMirror.Name = "btnBrowseMirror"
        btnBrowseMirror.Size = New Size(40, 24)
        btnBrowseMirror.TabIndex = 2
        btnBrowseMirror.Text = "..."
        btnBrowseMirror.UseVisualStyleBackColor = True
        ' 
        ' numMirrorRetention
        ' 
        numMirrorRetention.Location = New Point(130, 195)
        numMirrorRetention.Maximum = New Decimal(New Integer() {365, 0, 0, 0})
        numMirrorRetention.Minimum = New Decimal(New Integer() {1, 0, 0, 0})
        numMirrorRetention.Name = "numMirrorRetention"
        numMirrorRetention.Size = New Size(90, 23)
        numMirrorRetention.TabIndex = 4
        numMirrorRetention.Value = New Decimal(New Integer() {14, 0, 0, 0})
        ' 
        ' Label2
        ' 
        Label2.AutoSize = True
        Label2.Location = New Point(21, 198)
        Label2.Name = "Label2"
        Label2.Size = New Size(107, 15)
        Label2.TabIndex = 3
        Label2.Text = "เก็บไฟล์ย้อนหลัง (วัน) :"
        ' 
        ' btnSave
        ' 
        btnSave.Location = New Point(12, 426)
        btnSave.Name = "btnSave"
        btnSave.Size = New Size(110, 32)
        btnSave.TabIndex = 3
        btnSave.Text = "บันทึกค่า"
        btnSave.UseVisualStyleBackColor = True
        ' 
        ' btnStart
        ' 
        btnStart.Location = New Point(130, 426)
        btnStart.Name = "btnStart"
        btnStart.Size = New Size(110, 32)
        btnStart.TabIndex = 4
        btnStart.Text = "เริ่มทำงาน"
        btnStart.UseVisualStyleBackColor = True
        ' 
        ' btnStop
        ' 
        btnStop.Enabled = False
        btnStop.Location = New Point(248, 426)
        btnStop.Name = "btnStop"
        btnStop.Size = New Size(110, 32)
        btnStop.TabIndex = 5
        btnStop.Text = "หยุด"
        btnStop.UseVisualStyleBackColor = True
        ' 
        ' btnBackupNow
        ' 
        btnBackupNow.Location = New Point(366, 426)
        btnBackupNow.Name = "btnBackupNow"
        btnBackupNow.Size = New Size(145, 32)
        btnBackupNow.TabIndex = 6
        btnBackupNow.Text = "Full Backup ทันที"
        btnBackupNow.UseVisualStyleBackColor = True
        ' 
        ' btnLogNow
        ' 
        btnLogNow.Location = New Point(527, 426)
        btnLogNow.Name = "btnLogNow"
        btnLogNow.Size = New Size(145, 32)
        btnLogNow.TabIndex = 7
        btnLogNow.Text = "Log Backup ทันที"
        btnLogNow.UseVisualStyleBackColor = True
        ' 
        ' lblNextRun
        ' 
        lblNextRun.BorderStyle = BorderStyle.FixedSingle
        lblNextRun.Location = New Point(12, 466)
        lblNextRun.Name = "lblNextRun"
        lblNextRun.Padding = New Padding(6, 5, 0, 0)
        lblNextRun.Size = New Size(660, 28)
        lblNextRun.TabIndex = 8
        lblNextRun.Text = "สถานะ : หยุดทำงาน"
        ' 
        ' lstLog
        ' 
        lstLog.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lstLog.FormattingEnabled = True
        lstLog.HorizontalScrollbar = True
        lstLog.ItemHeight = 15
        lstLog.Location = New Point(12, 497)
        lstLog.Name = "lstLog"
        lstLog.Size = New Size(660, 154)
        lstLog.TabIndex = 9
        ' 
        ' tmrScheduler
        ' 
        tmrScheduler.Interval = 20000
        ' 
        ' NotifyIcon1
        ' 
        NotifyIcon1.Text = "SQL Backup Tool"
        NotifyIcon1.Visible = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 15F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(684, 673)
        Controls.Add(lstLog)
        Controls.Add(lblNextRun)
        Controls.Add(btnLogNow)
        Controls.Add(btnBackupNow)
        Controls.Add(btnStop)
        Controls.Add(btnStart)
        Controls.Add(btnSave)
        Controls.Add(grpStorage)
        Controls.Add(grpSchedule)
        Controls.Add(grpConnection)
        MinimumSize = New Size(700, 527)
        Name = "Form1"
        StartPosition = FormStartPosition.CenterScreen
        Text = "SQL Server Backup Tool"
        grpConnection.ResumeLayout(False)
        grpConnection.PerformLayout()
        grpSchedule.ResumeLayout(False)
        grpSchedule.PerformLayout()
        CType(numLogInterval, ComponentModel.ISupportInitialize).EndInit()
        grpStorage.ResumeLayout(False)
        grpStorage.PerformLayout()
        CType(numRetainDays, ComponentModel.ISupportInitialize).EndInit()
        CType(numMirrorRetention, ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
    End Sub

    Friend WithEvents grpConnection As System.Windows.Forms.GroupBox
    Friend WithEvents lblServer As System.Windows.Forms.Label
    Friend WithEvents txtServer As System.Windows.Forms.TextBox
    Friend WithEvents btnTestConnection As System.Windows.Forms.Button
    Friend WithEvents lblDatabase As System.Windows.Forms.Label
    Friend WithEvents cboDatabase As System.Windows.Forms.ComboBox
    Friend WithEvents btnLoadDb As System.Windows.Forms.Button
    Friend WithEvents chkWindowsAuth As System.Windows.Forms.CheckBox
    Friend WithEvents lblUser As System.Windows.Forms.Label
    Friend WithEvents txtUser As System.Windows.Forms.TextBox
    Friend WithEvents txtPassword As System.Windows.Forms.TextBox
    Friend WithEvents grpSchedule As System.Windows.Forms.GroupBox
    Friend WithEvents lblFullTime As System.Windows.Forms.Label
    Friend WithEvents dtpFullTime As System.Windows.Forms.DateTimePicker
    Friend WithEvents chkEnableLog As System.Windows.Forms.CheckBox
    Friend WithEvents lblLogInterval As System.Windows.Forms.Label
    Friend WithEvents numLogInterval As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkBackupOnShutdown As System.Windows.Forms.CheckBox
    Friend WithEvents grpStorage As System.Windows.Forms.GroupBox
    Friend WithEvents lblPath As System.Windows.Forms.Label
    Friend WithEvents txtBackupPath As System.Windows.Forms.TextBox
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents lblRetain As System.Windows.Forms.Label
    Friend WithEvents numRetainDays As System.Windows.Forms.NumericUpDown
    Friend WithEvents chkCompression As System.Windows.Forms.CheckBox
    Friend WithEvents btnSave As System.Windows.Forms.Button
    Friend WithEvents btnStart As System.Windows.Forms.Button
    Friend WithEvents btnStop As System.Windows.Forms.Button
    Friend WithEvents btnBackupNow As System.Windows.Forms.Button
    Friend WithEvents btnLogNow As System.Windows.Forms.Button
    Friend WithEvents lblNextRun As System.Windows.Forms.Label
    Friend WithEvents lstLog As System.Windows.Forms.ListBox
    Friend WithEvents tmrScheduler As System.Windows.Forms.Timer
    Friend WithEvents NotifyIcon1 As System.Windows.Forms.NotifyIcon
    Friend WithEvents FolderBrowserDialog1 As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents Label1 As Label
    Friend WithEvents txtMirrorFolder As TextBox
    Friend WithEvents btnBrowseMirror As Button
    Friend WithEvents Label2 As Label
    Friend WithEvents numMirrorRetention As NumericUpDown
    Friend WithEvents chkMirror As CheckBox
    Friend WithEvents chkVerifyMirror As CheckBox
    Friend WithEvents btnTestMirror As Button

End Class
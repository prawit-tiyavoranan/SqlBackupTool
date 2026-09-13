Imports System.IO

Public Module Logger

    Public Event OnLog(message As String)

    Private ReadOnly _lock As New Object()

    Public Sub Write(message As String)
        Dim line As String = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}  {message}"
        Try
            Dim logDir As String = Path.Combine(Application.StartupPath, "Logs")
            If Not Directory.Exists(logDir) Then Directory.CreateDirectory(logDir)

            Dim logPath As String = Path.Combine(logDir, $"app_{DateTime.Now:yyyyMMdd}.log")

            SyncLock _lock
                File.AppendAllText(logPath, line & Environment.NewLine)
            End SyncLock
        Catch
            ' ไม่ให้ error ของ log ทำให้โปรแกรมล้ม
        End Try
        RaiseEvent OnLog(line)
    End Sub

End Module
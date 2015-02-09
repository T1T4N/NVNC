Imports System
Imports System.Threading
Imports NVNC
Module Module1
    Private Sub IniciarVnc()
        Try
            Dim myVnc As New VncServer("1234", 5900, "Test")
            myVnc.Start()

        Catch ex As Exception
            Trace.WriteLine(ex.Message)
            Thread.Sleep(1000)
        End Try
    End Sub
    Sub Main()
        Dim t2 As Thread
        'port is closed, must open it
        t2 = New Thread(AddressOf IniciarVnc)
        t2.IsBackground = True
        t2.Priority = ThreadPriority.Highest
        t2.Start()
        Console.ReadLine()
    End Sub
End Module

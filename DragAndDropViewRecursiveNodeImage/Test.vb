Imports System.Runtime.Serialization.Formatters.Binary
Imports System.IO

Public Class Test
    Public Shared Sub test1()
        Dim n1 As New RNI.Node(123, 0)
        Dim r As New RNI(New List(Of RNI.Node) From {n1}, n1, "Example")
        Console.WriteLine(Dir(""))
        Using fs As New FileStream("test.rni", FileMode.Create)
            Dim fmt As New BinaryFormatter()
            fmt.Serialize(fs, r)
        End Using
    End Sub
End Class

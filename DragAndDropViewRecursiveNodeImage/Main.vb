Imports System.Windows.Forms

Module Main
    <STAThread>
    Sub Main()
        Test.test1()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New Form1())
    End Sub
End Module
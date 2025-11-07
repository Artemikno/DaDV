Imports System.Drawing.Imaging
Imports System.Drawing.Text
Imports System.IO
Imports System.Runtime.CompilerServices

Public Class Form1
    Private image As Image
    Private xOffset As Integer = 0
    Private yOffset As Integer = 0
    Private imageSize As Double = 1
    Private isMouseDown As Boolean
    Private mouseStartingPoint As Point
    Private isTheImageThere As Boolean
    Private didTheMouseMove As Boolean
    Private context As BufferedGraphicsContext
    Private buffer As BufferedGraphics
    Private zoomCursor = New Cursor(New IO.MemoryStream(My.Resources.zoomcursor_uncolored))
    Private WithEvents PictureBox1 As New DoublePanel

    Private Sub CenteredZoom(zoomFactor As Double)
        Dim oldSize = imageSize
        If Math.Sign(zoomFactor) = -1 Then
            imageSize /= Math.Abs(zoomFactor)
        Else
            imageSize *= zoomFactor
        End If
        xOffset += CInt((0 - xOffset) * (1 - imageSize / oldSize))
        yOffset += CInt((0 - yOffset) * (1 - imageSize / oldSize))
        PictureBox1.Invalidate()
    End Sub

    Private Sub Form1_DragEnter(sender As Object, e As DragEventArgs) Handles MyBase.DragEnter
        e.Effect = DragDropEffects.Move
    End Sub

    Private Sub Form1_DragDrop(sender As Object, e As DragEventArgs) Handles MyBase.DragDrop
        Dim files As Array = e.Data.GetData(DataFormats.FileDrop)
        If Not IsDBNull(files) Then
            isTheImageThere = True
            image = Image.FromFile(files(0))
            Me.Text = files(0)
            PictureBox1.Invalidate()
            Using bitmap As New Bitmap(files(0).ToString())
                Using ms As New MemoryStream()
                    Dim iconSize As New Size(32, 32)
                    Using resizedBitmap As New Bitmap(bitmap, iconSize)
                        resizedBitmap.Save(ms, ImageFormat.Png)
                        ms.Seek(0, SeekOrigin.Begin)
                        Me.Icon = Icon.FromHandle(resizedBitmap.GetHicon())
                    End Using
                End Using
            End Using
        End If
    End Sub

    Private Sub PictureBox1_Paint(sender As Object, e As PaintEventArgs) Handles PictureBox1.Paint
        buffer.Graphics.Clear(PictureBox1.BackColor)
        If isTheImageThere Then
            buffer.Graphics.DrawImage(image, CInt(PictureBox1.Width \ 2 - image.Width * imageSize \ 2 + xOffset), CInt(PictureBox1.Height \ 2 - image.Height * imageSize \ 2 + yOffset), CInt(image.Width * imageSize), CInt(image.Height * imageSize))
        Else
            buffer.Graphics.DrawString(String.Concat("The image", vbNewLine, "will be here"), New Font("Comic Sans MS", CSng(16 * imageSize), FontStyle.Bold), Brushes.Black, CInt(PictureBox1.Width / 2 - 132.5208 * imageSize / 2 + xOffset), CInt(PictureBox1.Height / 2 - 62.125 * imageSize / 2 + yOffset))
        End If
        buffer.Graphics.DrawString(String.Concat(String.Concat("X: ", xOffset), String.Concat(" Y: ", yOffset), String.Concat(" Zoom: ", imageSize)), New Font("Comic Sans MS", 16, FontStyle.Regular), Drawing.Brushes.Black, 0, (PictureBox1.Size.Height - 32.39583))
        buffer.Render(e.Graphics)
    End Sub

    Private Sub PictureBox1_MouseDown(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseDown
        isMouseDown = True
        mouseStartingPoint.X = e.X
        mouseStartingPoint.Y = e.Y
        didTheMouseMove = False
        Me.Cursor = zoomCursor
    End Sub

    Private Sub PictureBox1_MouseMove(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseMove
        If isMouseDown Then
            Me.Cursor = Cursors.SizeAll
            xOffset += (e.X - mouseStartingPoint.X)
            yOffset += (e.Y - mouseStartingPoint.Y)
            mouseStartingPoint.X = e.X
            mouseStartingPoint.Y = e.Y
            didTheMouseMove = True
            PictureBox1.Invalidate()
        Else
            Me.Cursor = Cursors.Default
        End If
    End Sub

    Private Sub PictureBox1_MouseUp(sender As Object, e As MouseEventArgs) Handles PictureBox1.MouseUp
        isMouseDown = False
        If Not didTheMouseMove Then
            If e.Button = MouseButtons.Left Then
                CenteredZoom(1.1)
            ElseIf e.Button = MouseButtons.Right Then
                CenteredZoom(-1.1)
            End If
        End If
        PictureBox1.Invalidate()
    End Sub

    Private Sub PictureBox1_Resize(sender As Object, e As EventArgs) Handles PictureBox1.Resize
        If PictureBox1.Width > 0 And PictureBox1.Height > 0 Then
            If buffer IsNot Nothing Then
                buffer.Dispose()
            End If
            If context IsNot Nothing Then
                buffer = context.Allocate(PictureBox1.CreateGraphics(), PictureBox1.DisplayRectangle)
                buffer.Graphics.InterpolationMode = Drawing2D.InterpolationMode.Low
                buffer.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighSpeed
            End If
        End If
        PictureBox1.Invalidate()
    End Sub

    Private Sub InToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles InToolStripMenuItem.Click
        CenteredZoom(1.1)
        PictureBox1.Invalidate()
    End Sub

    Private Sub OutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles OutToolStripMenuItem.Click
        CenteredZoom(-1.1)
        PictureBox1.Invalidate()
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PictureBox1.Dock = DockStyle.Fill
        Me.Controls.Add(PictureBox1)
        context = BufferedGraphicsManager.Current
        If PictureBox1.Width > 0 And PictureBox1.Height > 0 Then
            If buffer IsNot Nothing Then
                buffer.Dispose()
            End If
            buffer = context.Allocate(PictureBox1.CreateGraphics(), PictureBox1.DisplayRectangle)
            buffer.Graphics.InterpolationMode = Drawing2D.InterpolationMode.NearestNeighbor
            buffer.Graphics.SmoothingMode = Drawing2D.SmoothingMode.HighSpeed
        End If
        Dim args As String() = Environment.GetCommandLineArgs()

        If args.Length > 1 Then
            Dim filePath As String = args(1)
            Try
                isTheImageThere = True
                image = Image.FromFile(filePath)
                Me.Text = filePath
                PictureBox1.Invalidate()
                Using bitmap As New Bitmap(filePath)
                    Using ms As New MemoryStream()
                        Dim iconSize As New Size(32, 32)
                        Using resizedBitmap As New Bitmap(bitmap, iconSize)
                            resizedBitmap.Save(ms, ImageFormat.Png)
                            ms.Seek(0, SeekOrigin.Begin)
                            Me.Icon = Icon.FromHandle(resizedBitmap.GetHicon())
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                MessageBox.Show("Error loading the image: " & ex.Message)
            End Try
        End If
    End Sub

    Private Sub PictureBox1_Scroll(sender As Object, e As ScrollEventArgs) Handles PictureBox1.Scroll
        imageSize += e.NewValue - e.OldValue
        PictureBox1.Invalidate()
    End Sub

    Private Sub ResetToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ResetToolStripMenuItem.Click
        xOffset = 0
        yOffset = 0
        PictureBox1.Invalidate()
    End Sub

    Private Sub ResetToolStripMenuItem1_Click(sender As Object, e As EventArgs) Handles ResetToolStripMenuItem1.Click
        imageSize = 1
        PictureBox1.Invalidate()
    End Sub
End Class

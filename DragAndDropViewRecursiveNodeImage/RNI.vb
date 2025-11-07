Imports System.Drawing.Imaging
Imports System.IO
Imports System.Runtime.Remoting
Imports System.Runtime.Serialization

<Serializable>
Public Class RNI : Implements ICloneable, IDisposable, ISerializable
    Public Tag As Object
    Public Base As Node
    Private disposedValue As Boolean
    Public nodes As List(Of Node)
    <NonSerialized>
    Public getFromIdImage As Node.getFromIdTemplate = Function(id As Integer) As Node
                                                          Return nodes.Item(id)
                                                      End Function

    <Serializable>
    Public Class Node : Implements ISerializable
        Public id As Integer = -1
        Public Nodes As List(Of Integer) = Nothing
        Public Pixel As Integer = Nothing
        Public Divisions As Integer = 2
        Public getFromId As getFromIdTemplate
        Public Delegate Function getFromIdTemplate(id As Integer) As Node

        Public Sub Draw(g As Graphics, drawRect As Rectangle, resolution As Integer)
            If resolution <= 0 Then
                Return
            End If
            If Not (Nodes Is Nothing OrElse Nodes.Count = 0) Then
                If Not IsNothing(Nodes) Then
                    For i As Integer = 1 To 2 ^ Divisions
                        For j As Integer = 1 To 2 ^ Divisions
                            getFromId(Nodes.ElementAt(i * (2 ^ Divisions) + j)).Draw(g, New Rectangle(j, i * (2 ^ Divisions), drawRect.Width / Divisions, drawRect.Height / Divisions), resolution - 1)
                        Next
                    Next
                End If
            Else
                g.FillRectangle(New SolidBrush(Color.FromArgb(Pixel)), drawRect)
            End If
        End Sub

        Public Sub New(Pixel As Integer, id As Integer)
            Me.id = id
            Me.Pixel = Pixel
        End Sub

        Public Sub New(Nodes As List(Of Integer), Divisions As Integer, id As Integer, getFromId As getFromIdTemplate)
            Me.getFromId = getFromId
            Me.id = id
            Me.Nodes = Nodes
            Me.Divisions = Divisions
        End Sub

        Public Sub New(Nodes As List(Of Integer), id As Integer, getFromId As getFromIdTemplate)
            Me.getFromId = getFromId
            Me.id = id
            Me.Nodes = Nodes
        End Sub

        Protected Sub New(info As SerializationInfo, context As StreamingContext)
            id = info.GetInt32("id")
            Nodes = info.GetValue("nodes", GetType(List(Of Node)))
            Pixel = info.GetInt32("pixel")
            Divisions = info.GetInt32("divs")
            getFromId = info.GetValue("getfromid", GetType(Func(Of Integer, Node)))
        End Sub

        Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
            info.AddValue("id", id)
            info.AddValue("nodes", Nodes, GetType(List(Of Node)))
            info.AddValue("pixel", Pixel)
            info.AddValue("divs", Divisions)
            info.AddValue("getfromid", getFromId, GetType(Func(Of Integer, Node)))
        End Sub
    End Class

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    Public Overrides Function Equals(obj As Object) As Boolean
        Dim rNI = TryCast(obj, RNI)
        Return rNI IsNot Nothing AndAlso
               EqualityComparer(Of Object).Default.Equals(Tag, rNI.Tag) AndAlso
               EqualityComparer(Of List(Of Node)).Default.Equals(nodes, rNI.nodes) AndAlso
               EqualityComparer(Of Node).Default.Equals(Base, rNI.Base)
    End Function

    Public Overrides Function ToString() As String
        Return MyBase.ToString()
    End Function

    Public Overrides Function GetHashCode() As Integer
        Return MyBase.GetHashCode()
    End Function

    Public Sub New(nodes As List(Of Node), base As Node, tag As Object)
        Me.nodes = nodes
        Me.Base = base
        Me.Tag = tag
    End Sub
    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        nodes = info.GetValue("Nodes", GetType(List(Of Node)))
        Base = info.GetValue("Base", GetType(Node))
        Tag = info.GetValue("Tag", GetType(Object))
    End Sub

    Public Function Clone() As Object Implements ICloneable.Clone
        Throw New NotImplementedException()
    End Function

    Public Shared Function FromFile(fileName As String) As RNI
        Using fs As New FileStream(fileName, FileMode.Open)
            Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Return DirectCast(formatter.Deserialize(fs), RNI)
        End Using
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects)
                If nodes IsNot Nothing Then
                    nodes.Clear()
                    nodes = Nothing
                End If
                Tag = Nothing
                Base = Nothing
                getFromIdImage = Nothing
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
            ' TODO: set large fields to null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
    ' Protected Overrides Sub Finalize()
    '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

    Public Sub GetObjectData(info As SerializationInfo, context As StreamingContext) Implements ISerializable.GetObjectData
        info.AddValue("Tag", Tag)
        info.AddValue("Base", Base, GetType(Node))
        info.AddValue("Nodes", nodes, GetType(List(Of Node)))
    End Sub
End Class

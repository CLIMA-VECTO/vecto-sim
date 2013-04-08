Imports System.Collections.Generic
Imports VECTO.DelaunayTrigSimple     '<<<< Verweis auf Namespace ersetzen!

Public Class cDelaunayMap

    Public ptDim As Integer

    Public ptList As List(Of dPoint)
    Private lDT As List(Of dTriangle)
    Private planes As List(Of Double())

    Public DualMode As Boolean
    Private ptListXZ As List(Of dPoint)
    Private planesXZ As List(Of Double())
    Private lDTXZ As List(Of dTriangle)


    Public Sub New()
        ptList = New List(Of dPoint)
        ptListXZ = New List(Of dPoint)
        DualMode = False
    End Sub

    Public Sub AddPoints(ByVal X As Double, ByVal Y As Double, ByVal Z As Double)
        ptList.Add(New dPoint(X, Y, Z))
        If DualMode Then ptListXZ.Add(New dPoint(X, Z, Y))
    End Sub

    Public Function Triangulate() As Boolean
        Dim tr As dTriangle
        Dim DT As dTriangulation

        ptDim = ptList.Count - 1

        'XY-triangulation
        Try
            DT = New dTriangulation
            lDT = DT.Triangulate(ptList)
        Catch ex As Exception
            Return False
        End Try

        planes = New List(Of Double())

        For Each tr In lDT
            planes.Add(GetPlane(tr))
        Next



        '#If DEBUG Then
        '        Debug.Print("x,y,z,x,y,z")
        '        For Each tr In lDT
        '            Debug.Print(tr.P1.X & "," & tr.P1.Y & "," & tr.P1.Z & "," & tr.P2.X & "," & tr.P2.Y & "," & tr.P2.Z)
        '            Debug.Print(tr.P3.X & "," & tr.P3.Y & "," & tr.P3.Z & "," & tr.P2.X & "," & tr.P2.Y & "," & tr.P2.Z)
        '            Debug.Print(tr.P1.X & "," & tr.P1.Y & "," & tr.P1.Z & "," & tr.P3.X & "," & tr.P3.Y & "," & tr.P3.Z)
        '        Next
        '#End If



        'XZ-triangulation
        If DualMode Then

            If ptDim <> ptListXZ.Count - 1 Then Return False

            Try
                DT = New dTriangulation
                lDTXZ = DT.Triangulate(ptListXZ)
            Catch ex As Exception
                Return False
            End Try

            planesXZ = New List(Of Double())

            For Each tr In lDTXZ
                planesXZ.Add(GetPlane(tr))
            Next

        End If

        Return True

    End Function

    'XY => Z Interpolation
    Public Function Intpol(ByVal x As Double, ByVal y As Double) As Double
        Dim j As Integer
        Dim l0 As Double()
        Dim tr As dTriangle

        j = -1

        For Each tr In lDT
            j += 1
            If IsInside(tr, x, y) Then
                l0 = planes(j)
                Return (l0(3) - x * l0(0) - y * l0(1)) / l0(2)
            End If
        Next

        'ERROR: Extrapolation

#If DEBUG Then
        Debug.Print(x & "," & y)
#End If

        Throw New ArgumentException("Extrapolation not possible!")


        Return Nothing

    End Function

    'XZ => Y Interpolation
    Public Function IntpolXZ(ByVal x As Double, ByVal z As Double) As Double
        Dim j As Integer
        Dim l0 As Double()
        Dim tr As dTriangle

        If DualMode Then

            j = -1

            For Each tr In lDTXZ
                j += 1
                If IsInside(tr, x, z) Then
                    l0 = planesXZ(j)
                    Return (l0(3) - x * l0(0) - z * l0(1)) / l0(2)
                End If
            Next

            'ERROR: Extrapolation
            Throw New ArgumentException("Extrapolation not possible!")
            Return Nothing

        Else

            'ERROR: Extrapolation
            Throw New ArgumentException("XZ Interpolation not configured")
            Return Nothing

        End If
    End Function

    Private Function GetPlane(ByRef tr As dTriangle) As Double()
        Dim AB As dPoint
        Dim AC As dPoint
        Dim cross As dPoint
        Dim l(3) As Double
        Dim pt1 As dPoint
        Dim pt2 As dPoint
        Dim pt3 As dPoint

        pt1 = tr.P1
        pt2 = tr.P2
        pt3 = tr.P3

        AB = New dPoint(pt2.X - pt1.X, pt2.Y - pt1.Y, pt2.Z - pt1.Z)
        AC = New dPoint(pt3.X - pt1.X, pt3.Y - pt1.Y, pt3.Z - pt1.Z)

        cross = New dPoint(AB.Y * AC.Z - AB.Z * AC.Y, AB.Z * AC.X - AB.X * AC.Z, AB.X * AC.Y - AB.Y * AC.X)

        l(0) = cross.X
        l(1) = cross.Y
        l(2) = cross.Z

        l(3) = pt1.X * cross.X + pt1.Y * cross.Y + pt1.Z * cross.Z

        Return l

    End Function

    Private Function IsInside(ByRef tr As dTriangle, ByVal xges As Double, ByVal yges As Double) As Boolean
        Dim v0(1) As Double
        Dim v1(1) As Double
        Dim v2(1) As Double
        Dim dot00 As Double
        Dim dot01 As Double
        Dim dot02 As Double
        Dim dot11 As Double
        Dim dot12 As Double
        Dim invDenom As Double
        Dim u As Double
        Dim v As Double
        Dim pt1 As dPoint
        Dim pt2 As dPoint
        Dim pt3 As dPoint

        pt1 = tr.P1
        pt2 = tr.P2
        pt3 = tr.P3

        'Quelle: http://www.blackpawn.com/texts/pointinpoly/default.html  (Barycentric Technique)

        ' Compute vectors        
        v0(0) = pt3.X - pt1.X
        v0(1) = pt3.Y - pt1.Y

        v1(0) = pt2.X - pt1.X
        v1(1) = pt2.Y - pt1.Y

        v2(0) = xges - pt1.X
        v2(1) = yges - pt1.Y

        ' Compute dot products
        dot00 = v0(0) * v0(0) + v0(1) * v0(1)
        dot01 = v0(0) * v1(0) + v0(1) * v1(1)
        dot02 = v0(0) * v2(0) + v0(1) * v2(1)
        dot11 = v1(0) * v1(0) + v1(1) * v1(1)
        dot12 = v1(0) * v2(0) + v1(1) * v2(1)

        ' Compute barycentric coordinates
        invDenom = 1 / (dot00 * dot11 - dot01 * dot01)
        u = (dot11 * dot02 - dot01 * dot12) * invDenom
        v = (dot00 * dot12 - dot01 * dot02) * invDenom

        ' Check if point is in triangle
        Return (u >= 0) And (v >= 0) And (u + v <= 1.00000000001)




    End Function

End Class


Namespace DelaunayTrigSimple

    Public Class dPoint
        Public X As Double
        Public Y As Double
        Public Z As Double

        Public Sub New(ByVal xd As Double, ByVal yd As Double, ByVal zd As Double)
            X = xd
            Y = yd
            Z = zd
        End Sub

        Public Shared Operator =(left As dPoint, right As dPoint) As Boolean

            'If DirectCast(left, Object) = DirectCast(right, Object) Then
            '    Return True
            'End If

            'If (DirectCast(left, Object) Is Nothing) OrElse (DirectCast(right, Object) Is Nothing) Then
            '    Return False
            'End If

            ' Just compare x and y here...
            If left.X <> right.X Then
                Return False
            End If

            If left.Y <> right.Y Then
                Return False
            End If

            Return True

        End Operator

        Public Shared Operator <>(left As dPoint, right As dPoint) As Boolean
            Return Not (left = right)
        End Operator


    End Class

    Public Class dTriangle
        Public P1 As dPoint
        Public P2 As dPoint
        Public P3 As dPoint

        Public Sub New(ByRef pp1 As dPoint, ByRef pp2 As dPoint, ByRef pp3 As dPoint)
            P1 = pp1
            P2 = pp2
            P3 = pp3
        End Sub

        Public Function ContainsInCircumcircle(pt As dPoint) As Double
            Dim ax As Double = Me.P1.X - pt.X
            Dim ay As Double = Me.P1.Y - pt.Y
            Dim bx As Double = Me.P2.X - pt.X
            Dim by As Double = Me.P2.Y - pt.Y
            Dim cx As Double = Me.P3.X - pt.X
            Dim cy As Double = Me.P3.Y - pt.Y
            Dim det_ab As Double = ax * by - bx * ay
            Dim det_bc As Double = bx * cy - cx * by
            Dim det_ca As Double = cx * ay - ax * cy
            Dim a_squared As Double = ax * ax + ay * ay
            Dim b_squared As Double = bx * bx + by * by
            Dim c_squared As Double = cx * cx + cy * cy

            Return a_squared * det_bc + b_squared * det_ca + c_squared * det_ab

        End Function

        Public Function SharesVertexWith(triangle As dTriangle) As Boolean
            If Me.P1.X = triangle.P1.X AndAlso Me.P1.Y = triangle.P1.Y Then
                Return True
            End If
            If Me.P1.X = triangle.P2.X AndAlso Me.P1.Y = triangle.P2.Y Then
                Return True
            End If
            If Me.P1.X = triangle.P3.X AndAlso Me.P1.Y = triangle.P3.Y Then
                Return True
            End If

            If Me.P2.X = triangle.P1.X AndAlso Me.P2.Y = triangle.P1.Y Then
                Return True
            End If
            If Me.P2.X = triangle.P2.X AndAlso Me.P2.Y = triangle.P2.Y Then
                Return True
            End If
            If Me.P2.X = triangle.P3.X AndAlso Me.P2.Y = triangle.P3.Y Then
                Return True
            End If

            If Me.P3.X = triangle.P1.X AndAlso Me.P3.Y = triangle.P1.Y Then
                Return True
            End If
            If Me.P3.X = triangle.P2.X AndAlso Me.P3.Y = triangle.P2.Y Then
                Return True
            End If
            If Me.P3.X = triangle.P3.X AndAlso Me.P3.Y = triangle.P3.Y Then
                Return True
            End If

            Return False
        End Function

    End Class

    Public Class dEdge
        Public StartPoint As dPoint
        Public EndPoint As dPoint

        Public Sub New(ByRef p1 As dPoint, ByRef p2 As dPoint)
            StartPoint = p1
            EndPoint = p2
        End Sub

        Public Shared Operator =(left As dEdge, right As dEdge) As Boolean
            'If DirectCast(left, Object) = DirectCast(right, Object) Then
            '    Return True
            'End If

            'If (DirectCast(left, Object) Is Nothing) Or (DirectCast(right, Object) Is Nothing) Then
            '    Return False
            'End If

            Return ((left.StartPoint = right.StartPoint AndAlso left.EndPoint = right.EndPoint) OrElse (left.StartPoint = right.EndPoint AndAlso left.EndPoint = right.StartPoint))
        End Operator

        Public Shared Operator <>(left As dEdge, right As dEdge) As Boolean
            Return Not (left = right)
        End Operator


    End Class

    Public Class dTriangulation

        Public Function Triangulate(triangulationPoints As List(Of dPoint)) As List(Of dTriangle)
            If triangulationPoints.Count < 3 Then
                Throw New ArgumentException("Can not triangulate less than three vertices!")
            End If

            ' The triangle list
            Dim triangles As New List(Of dTriangle)()



            ' The "supertriangle" which encompasses all triangulation points.
            ' This triangle initializes the algorithm and will be removed later.
            Dim superTriangle As dTriangle = Me.SuperTriangle(triangulationPoints)
            triangles.Add(superTriangle)

            ' Include each point one at a time into the existing triangulation
            For i As Integer = 0 To triangulationPoints.Count - 1
                ' Initialize the edge buffer.
                Dim EdgeBuffer As New List(Of dEdge)()

                ' If the actual vertex lies inside the circumcircle, then the three edges of the 
                ' triangle are added to the edge buffer and the triangle is removed from list.                             
                For j As Integer = triangles.Count - 1 To 0 Step -1
                    Dim t As dTriangle = triangles(j)
                    If t.ContainsInCircumcircle(triangulationPoints(i)) > 0 Then
                        EdgeBuffer.Add(New dEdge(t.P1, t.P2))
                        EdgeBuffer.Add(New dEdge(t.P2, t.P3))
                        EdgeBuffer.Add(New dEdge(t.P3, t.P1))
                        triangles.RemoveAt(j)
                    End If
                Next

                ' Remove duplicate edges. This leaves the convex hull of the edges.
                ' The edges in this convex hull are oriented counterclockwise!
                For j As Integer = EdgeBuffer.Count - 2 To 0 Step -1
                    For k As Integer = EdgeBuffer.Count - 1 To j + 1 Step -1
                        If EdgeBuffer(j) = EdgeBuffer(k) Then
                            EdgeBuffer.RemoveAt(k)
                            EdgeBuffer.RemoveAt(j)
                            k -= 1
                            Continue For
                        End If
                    Next
                Next

                ' Generate new counterclockwise oriented triangles filling the "hole" in
                ' the existing triangulation. These triangles all share the actual vertex.
                For j As Integer = 0 To EdgeBuffer.Count - 1
                    triangles.Add(New dTriangle(EdgeBuffer(j).StartPoint, EdgeBuffer(j).EndPoint, triangulationPoints(i)))
                Next
            Next

            ' We don't want the supertriangle in the triangulation, so
            ' remove all triangles sharing a vertex with the supertriangle.
            For i As Integer = triangles.Count - 1 To 0 Step -1
                If triangles(i).SharesVertexWith(superTriangle) Then
                    triangles.RemoveAt(i)
                End If
            Next

            ' Return the triangles
            Return triangles
        End Function




        Private Function SuperTriangle(triangulationPoints As List(Of dPoint)) As dTriangle
            Dim M As Double = triangulationPoints(0).X

            ' get the extremal x and y coordinates
            For i As Integer = 1 To triangulationPoints.Count - 1
                Dim xAbs As Double = Math.Abs(triangulationPoints(i).X)
                Dim yAbs As Double = Math.Abs(triangulationPoints(i).Y)
                If xAbs > M Then
                    M = xAbs
                End If
                If yAbs > M Then
                    M = yAbs
                End If
            Next

            ' make a triangle
            Dim sp1 As New dPoint(10 * M, 0, 0)
            Dim sp2 As New dPoint(0, 10 * M, 0)
            Dim sp3 As New dPoint(-10 * M, -10 * M, 0)

            Return New dTriangle(sp1, sp2, sp3)
        End Function

    End Class

End Namespace

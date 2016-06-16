'*******************************************************************************
'  Copyright 2009-2012 dataweb GmbH
'  This file is part of the NShape framework.
'  NShape is free software: you can redistribute it and/or modify it under the 
'  terms of the GNU General Public License as published by the Free Software 
'  Foundation, either version 3 of the License, or (at your option) any later 
'  version.
'  NShape is distributed in the hope that it will be useful, but WITHOUT ANY
'  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
'  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
'  You should have received a copy of the GNU General Public License along with 
'  NShape. If not, see <http://www.gnu.org/licenses/>.
'*******************************************************************************

Imports Dataweb.NShape
Imports Dataweb.NShape.Advanced
Imports Dataweb.NShape.GeneralShapes


Public Class MainForm
	Dim moveCnt As Integer = 0
	Const stoneSize = 100


	Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
		' Prepare project
		Project.Name = "API DEMO"
		Project.Create()
		Project.AddLibraryByName("Dataweb.NShape.GeneralShapes", False)

		' Prepare diagram
		Dim diagram As New Diagram("Diagram")
		diagram.Width = 3 * stoneSize
		diagram.Height = 3 * stoneSize
		diagram.BackgroundImage = New NamedImage(My.Resources.Background, "Background")
		diagram.BackgroundImageLayout = ImageLayoutMode.Tile

		' Prepare visual styles
		' Create a fill style for the stones
		Dim fillstyle As New FillStyle
		fillstyle.AdditionalColorStyle = Project.Design.ColorStyles.Transparent
		fillstyle.BaseColorStyle = Project.Design.ColorStyles.Black
		fillstyle.FillMode = FillMode.Gradient
		' Create character style for the stone's labels
		Dim characterStyle As New CharacterStyle
		characterStyle.ColorStyle = New ColorStyle("White", Color.White)
		characterStyle.Size = 36
		characterStyle.FontName = "Tahoma"
		characterStyle.Style = Drawing.FontStyle.Bold
		' Create line style for the stone's outlines
		Dim lineStyle As New LineStyle
		lineStyle.ColorStyle = New ColorStyle("White", Color.White)

		' Find shape type for creating the stones
		Dim shapeType As ShapeType
		shapeType = Project.ShapeTypes("RoundedBox")

		Dim r, c As Integer
		For r = 0 To 2
			For c = 0 To 2
				Dim p As Point = New Point((stoneSize / 2) + (c * stoneSize), (stoneSize / 2) + (r * stoneSize))

				If Not (r = 2 And c = 2) Then
					' Create a new stone and set up its visual appearance
					Dim stone As RoundedBox = shapeType.CreateInstance()
					stone.FillStyle = fillstyle
					stone.CharacterStyle = characterStyle
					stone.LineStyle = lineStyle
					' Store the stone's original field assignment in the stone's Tag property
					stone.Tag = GetField(p)
					' Set the stone's size and label
					stone.Width = stoneSize
					stone.Height = stoneSize
					stone.Text = (CInt(stone.Tag) + 1).ToString()
					' Move the stone to its position
					stone.MoveTo(p.X, p.Y)
					' Add stone to the diagram
					diagram.Shapes.Add(stone)
				Else
					' Do nothing
				End If
			Next
		Next
		' Display the diagram
		Display.Diagram = diagram
		' Fit display into form bounds
		ZoomDiagramToForm()
		' Shuffle stone positions
		Shuffle()
	End Sub


	Private Sub Shuffle()
		' Reset move counter
		moveCnt = 0
		' Shuffle stone positions
		Dim Randomizer As New Random(DateTime.Now.Millisecond)
		' Build a list of stones
		Dim stones As New List(Of Shape)(9)
		For i As Integer = 0 To 8
			stones.Add(GetStone(i))
		Next
		For newField As Integer = 0 To 8
			' Pick a random stone out of the list...
			Dim idx As Integer
			idx = Randomizer.Next(1, stones.Count) - 1
			Dim stone As Shape = stones(idx)
			' ... and move it to the current field
			If Not stone Is Nothing Then
				Dim p As Point = GetCoordinates(newField)
				stone.MoveTo(p.X, p.Y)
			End If
			' Remove the stone from the list
			stones.RemoveAt(idx)
		Next
	End Sub


	Private Function GetField(stone As Shape) As Integer
		' Calculate the field of the stone from its coordinates
		If stone Is Nothing Then Return -1
		Dim pos As Point = Point.Empty
		pos.Offset(stone.X, stone.Y)
		Return GetField(pos)
	End Function


	Private Function GetField(position As Point) As Integer
		' Calculate the field of the given coordinates
		Dim row, col As Integer
		col = CInt(Math.Floor(position.X / 100.0F))
		row = CInt(Math.Floor(position.Y / 100.0F))
		Dim field As Integer = col + (row * 3)
		If field < 0 Or field > 8 Then
			Return -1
		Else
			Return field
		End If
	End Function


	Private Function GetCoordinates(field As Integer) As Point
		Dim row, col As Integer
		row = CInt(Math.Floor(field / 3.0F))
		col = field Mod 3
		Dim result As Point
		result.Offset((col * stoneSize) + (stoneSize / 2), (row * stoneSize) + (stoneSize / 2))
		Return result
	End Function


	Private Function GetStone(field As Integer) As Shape
		' Get the stone located in the given field
		Dim pos As Point
		pos = GetCoordinates(field)
		Return GetStone(pos)
	End Function


	Private Function GetStone(position As Point) As Shape
		' Get the stone located at the given coordinates
		For Each shape As Shape In Display.Diagram.Shapes.FindShapes(position.X, position.Y, ControlPointCapabilities.Reference, 0, Nothing)
			If shape.X = position.X And shape.Y = position.Y Then
				Return shape
			End If
		Next
		Return Nothing
	End Function


	Private Sub CheckMoves(ByVal stone As Shape)
		' Get the field of the given stone
		Dim field As Integer = GetField(stone)
		' Get diagram bounds for field validity check
		Dim diagramBounds As Rectangle
		diagramBounds.Location = Point.Empty
		diagramBounds.Width = Display.Diagram.Width
		diagramBounds.Height = Display.Diagram.Height

		' Check for the next free field to move to
		Dim targetField As Integer = -1
		Dim p As Point
		' Check field above
		p = GetCoordinates(field - 3)
		If diagramBounds.Contains(p) And GetStone(p) Is Nothing Then
			targetField = field - 3
		End If
		' Check field left
		p = GetCoordinates(field - 1)
		If diagramBounds.Contains(p) And GetStone(p) Is Nothing Then
			Debug.Assert(targetField < 0)
			targetField = field - 1
		End If
		' Check field right
		p = GetCoordinates(field + 1)
		If diagramBounds.Contains(p) And GetStone(p) Is Nothing Then
			Debug.Assert(targetField < 0)
			targetField = field + 1
		End If
		' Check field below
		p = GetCoordinates(field + 3)
		If diagramBounds.Contains(p) And GetStone(p) Is Nothing Then
			Debug.Assert(targetField < 0)
			targetField = field + 3
		End If

		' If a valid target field was found, move the stone
		If targetField >= 0 Then
			MoveStone(stone, field, targetField)
		End If

		' Check if the game is won after this move
		CheckWon()
	End Sub


	Private Sub CheckWon()
		' Check if all stones are on the fields they are supposed to be
		Dim won As Boolean = True
		For i As Integer = 0 To 7
			Dim stone = GetStone(i)
			If stone Is Nothing Then
				won = False
			Else
				If Not i.Equals(stone.Tag) Then
					won = False
				End If
			End If
			If won = False Then Exit For
		Next
		If won And moveCnt > 0 Then
			MessageBox.Show(String.Format("Game solved with {0} moves. {1}Congratulations!", moveCnt, vbCrLf))
		End If
	End Sub


	Private Sub MoveStone(ByVal stone As Shape, ByVal fromField As Integer, ByVal toField As Integer)
		MoveStone(stone, GetCoordinates(fromField), GetCoordinates(toField))
	End Sub


	Private Sub MoveStone(ByVal stone As Shape, ByVal fromPos As Point, ByVal toPos As Point)
		moveCnt += 1

		Dim stepping As Integer = 0
		Dim steps As Integer = 10

		Dim distanceX As Integer = toPos.X - fromPos.X
		Dim distanceY As Integer = toPos.Y - fromPos.Y

		If distanceX <> 0 Then
			stepping = distanceX / steps
			While stone.X <> toPos.X
				stone.X += stepping
				Application.DoEvents()
			End While
		ElseIf distanceY <> 0 Then
			stepping = distanceY / steps
			While stone.Y <> toPos.Y
				stone.Y += stepping
				Application.DoEvents()
			End While
		End If
		If stone.X <> toPos.X Or stone.Y <> toPos.Y Then
			stone.MoveTo(toPos.X, toPos.Y)
		End If
	End Sub


	Private Sub ZoomDiagramToForm()
		Dim zoom As Single
		If Not Display Is Nothing And Not Display.Diagram Is Nothing Then
			zoom = Math.Min(Display.Bounds.Width / CSng(Display.Diagram.Width), Display.Bounds.Height / CSng(Display.Diagram.Height))
			If (zoom > 0) Then
				Display.ZoomLevel = zoom * 100
			Else
				Display.ZoomLevel = 1
			End If
		End If
	End Sub


	Private Sub Display_ShapeClick(sender As System.Object, e As Dataweb.NShape.Controllers.DiagramPresenterShapeClickEventArgs) Handles Display.ShapeClick
		CheckMoves(e.Shape)
	End Sub


	Private Sub Display_ShapeDoubleClick(sender As System.Object, e As Dataweb.NShape.Controllers.DiagramPresenterShapeClickEventArgs) Handles Display.ShapeDoubleClick
		CheckMoves(e.Shape)
	End Sub


	Private Sub ToolStripButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripButton2.Click
		Shuffle()
	End Sub


	Private Sub Form1_Resize(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Resize
		ZoomDiagramToForm()
	End Sub


	Private Sub MaximumQualityToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MaximumQualityToolStripMenuItem.Click
		Display.RenderingQualityHighQuality = RenderingQuality.HighQuality
		Display.HighQualityRendering = True
		HighQualityToolStripMenuItem.Checked = False
		MediumQualityToolStripMenuItem.Checked = False
		LowQualityToolStripMenuItem.Checked = False
	End Sub


	Private Sub HighQualityToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HighQualityToolStripMenuItem.Click
		Display.RenderingQualityHighQuality = RenderingQuality.HighQuality
		Display.HighQualityRendering = True
		MaximumQualityToolStripMenuItem.Checked = False
		MediumQualityToolStripMenuItem.Checked = False
		LowQualityToolStripMenuItem.Checked = False
	End Sub


	Private Sub DefaultQualityToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MediumQualityToolStripMenuItem.Click
		Display.RenderingQualityLowQuality = RenderingQuality.DefaultQuality
		Display.HighQualityRendering = False
		MaximumQualityToolStripMenuItem.Checked = False
		HighQualityToolStripMenuItem.Checked = False
		LowQualityToolStripMenuItem.Checked = False
	End Sub


	Private Sub LowQualityToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LowQualityToolStripMenuItem.Click
		Display.RenderingQualityLowQuality = RenderingQuality.LowQuality
		Display.HighQualityRendering = False
		MaximumQualityToolStripMenuItem.Checked = False
		HighQualityToolStripMenuItem.Checked = False
		MediumQualityToolStripMenuItem.Checked = False
	End Sub

End Class

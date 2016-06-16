<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
	Inherits System.Windows.Forms.Form

	'Form overrides dispose to clean up the component list.
	<System.Diagnostics.DebuggerNonUserCode()> _
	Protected Overrides Sub Dispose(ByVal disposing As Boolean)
		Try
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
		Finally
			MyBase.Dispose(disposing)
		End Try
	End Sub

	'Required by the Windows Form Designer
	Private components As System.ComponentModel.IContainer

	'NOTE: The following procedure is required by the Windows Form Designer
	'It can be modified using the Windows Form Designer.  
	'Do not modify it using the code editor.
	<System.Diagnostics.DebuggerStepThrough()> _
	Private Sub InitializeComponent()
		Me.components = New System.ComponentModel.Container()
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
		Dim RoleBasedSecurityManager2 As Dataweb.NShape.RoleBasedSecurityManager = New Dataweb.NShape.RoleBasedSecurityManager()
		Me.DiagramSetController = New Dataweb.NShape.Controllers.DiagramSetController()
		Me.Project = New Dataweb.NShape.Project(Me.components)
		Me.CachedRepository = New Dataweb.NShape.Advanced.CachedRepository()
		Me.ToolStripContainer1 = New System.Windows.Forms.ToolStripContainer()
		Me.Display = New Dataweb.NShape.WinFormsUI.Display()
		Me.ToolStrip1 = New System.Windows.Forms.ToolStrip()
		Me.ToolStripButton2 = New System.Windows.Forms.ToolStripButton()
		Me.ToolStripDropDownButton1 = New System.Windows.Forms.ToolStripDropDownButton()
		Me.MaximumQualityToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
		Me.HighQualityToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
		Me.MediumQualityToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
		Me.LowQualityToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
		Me.ToolStripContainer1.ContentPanel.SuspendLayout()
		Me.ToolStripContainer1.TopToolStripPanel.SuspendLayout()
		Me.ToolStripContainer1.SuspendLayout()
		Me.ToolStrip1.SuspendLayout()
		Me.SuspendLayout()
		'
		'DiagramSetController
		'
		Me.DiagramSetController.ActiveTool = Nothing
		Me.DiagramSetController.Project = Me.Project
		'
		'Project
		'
		Me.Project.AutoGenerateTemplates = True
		Me.Project.LibrarySearchPaths = CType(resources.GetObject("Project.LibrarySearchPaths"), System.Collections.Generic.IList(Of String))
		Me.Project.Name = ""
		Me.Project.Repository = Me.CachedRepository
		RoleBasedSecurityManager2.CurrentRole = Dataweb.NShape.StandardRole.Administrator
		RoleBasedSecurityManager2.CurrentRoleName = "Administrator"
		Me.Project.SecurityManager = RoleBasedSecurityManager2
		'
		'CachedRepository
		'
		Me.CachedRepository.ProjectName = ""
		Me.CachedRepository.Store = Nothing
		Me.CachedRepository.Version = 0
		'
		'ToolStripContainer1
		'
		'
		'ToolStripContainer1.ContentPanel
		'
		Me.ToolStripContainer1.ContentPanel.Controls.Add(Me.Display)
		Me.ToolStripContainer1.ContentPanel.Size = New System.Drawing.Size(412, 412)
		Me.ToolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill
		Me.ToolStripContainer1.Location = New System.Drawing.Point(0, 0)
		Me.ToolStripContainer1.Name = "ToolStripContainer1"
		Me.ToolStripContainer1.Size = New System.Drawing.Size(412, 437)
		Me.ToolStripContainer1.TabIndex = 1
		Me.ToolStripContainer1.Text = "ToolStripContainer1"
		'
		'ToolStripContainer1.TopToolStripPanel
		'
		Me.ToolStripContainer1.TopToolStripPanel.Controls.Add(Me.ToolStrip1)
		'
		'Display
		'
		Me.Display.AllowDrop = True
		Me.Display.AutoScroll = True
		Me.Display.BackColorGradient = System.Drawing.SystemColors.Control
		Me.Display.DiagramSetController = Me.DiagramSetController
		Me.Display.Dock = System.Windows.Forms.DockStyle.Fill
		Me.Display.GridColor = System.Drawing.Color.White
		Me.Display.ImeMode = System.Windows.Forms.ImeMode.NoControl
		Me.Display.Location = New System.Drawing.Point(0, 0)
		Me.Display.Name = "Display"
		Me.Display.PropertyController = Nothing
		Me.Display.SelectionHilightColor = System.Drawing.Color.Firebrick
		Me.Display.SelectionInactiveColor = System.Drawing.Color.Gray
		Me.Display.SelectionInteriorColor = System.Drawing.Color.WhiteSmoke
		Me.Display.SelectionNormalColor = System.Drawing.Color.DarkGreen
		Me.Display.ShowDefaultContextMenu = False
		Me.Display.IsGridVisible = False
		Me.Display.ShowScrollBars = False
		Me.Display.Size = New System.Drawing.Size(412, 412)
		Me.Display.SnapToGrid = False
		Me.Display.TabIndex = 0
		Me.Display.ToolPreviewBackColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(119, Byte), Integer), CType(CType(136, Byte), Integer), CType(CType(153, Byte), Integer))
		Me.Display.ToolPreviewColor = System.Drawing.Color.FromArgb(CType(CType(96, Byte), Integer), CType(CType(70, Byte), Integer), CType(CType(130, Byte), Integer), CType(CType(180, Byte), Integer))
		'
		'ToolStrip1
		'
		Me.ToolStrip1.Dock = System.Windows.Forms.DockStyle.None
		Me.ToolStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripButton2, Me.ToolStripDropDownButton1})
		Me.ToolStrip1.Location = New System.Drawing.Point(3, 0)
		Me.ToolStrip1.Name = "ToolStrip1"
		Me.ToolStrip1.Size = New System.Drawing.Size(188, 25)
		Me.ToolStrip1.TabIndex = 0
		'
		'ToolStripButton2
		'
		Me.ToolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
		Me.ToolStripButton2.Image = CType(resources.GetObject("ToolStripButton2.Image"), System.Drawing.Image)
		Me.ToolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta
		Me.ToolStripButton2.Name = "ToolStripButton2"
		Me.ToolStripButton2.Size = New System.Drawing.Size(69, 22)
		Me.ToolStripButton2.Text = "New Game"
		'
		'ToolStripDropDownButton1
		'
		Me.ToolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text
		Me.ToolStripDropDownButton1.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.MaximumQualityToolStripMenuItem, Me.HighQualityToolStripMenuItem, Me.MediumQualityToolStripMenuItem, Me.LowQualityToolStripMenuItem})
		Me.ToolStripDropDownButton1.Image = CType(resources.GetObject("ToolStripDropDownButton1.Image"), System.Drawing.Image)
		Me.ToolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta
		Me.ToolStripDropDownButton1.Name = "ToolStripDropDownButton1"
		Me.ToolStripDropDownButton1.Size = New System.Drawing.Size(107, 22)
		Me.ToolStripDropDownButton1.Text = "Graphics Quality"
		'
		'MaximumQualityToolStripMenuItem
		'
		Me.MaximumQualityToolStripMenuItem.CheckOnClick = True
		Me.MaximumQualityToolStripMenuItem.Name = "MaximumQualityToolStripMenuItem"
		Me.MaximumQualityToolStripMenuItem.Size = New System.Drawing.Size(128, 22)
		Me.MaximumQualityToolStripMenuItem.Text = "Maximum"
		'
		'HighQualityToolStripMenuItem
		'
		Me.HighQualityToolStripMenuItem.Checked = True
		Me.HighQualityToolStripMenuItem.CheckOnClick = True
		Me.HighQualityToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
		Me.HighQualityToolStripMenuItem.Name = "HighQualityToolStripMenuItem"
		Me.HighQualityToolStripMenuItem.Size = New System.Drawing.Size(128, 22)
		Me.HighQualityToolStripMenuItem.Text = "High"
		'
		'MediumQualityToolStripMenuItem
		'
		Me.MediumQualityToolStripMenuItem.CheckOnClick = True
		Me.MediumQualityToolStripMenuItem.Name = "MediumQualityToolStripMenuItem"
		Me.MediumQualityToolStripMenuItem.Size = New System.Drawing.Size(128, 22)
		Me.MediumQualityToolStripMenuItem.Text = "Medium"
		'
		'LowQualityToolStripMenuItem
		'
		Me.LowQualityToolStripMenuItem.CheckOnClick = True
		Me.LowQualityToolStripMenuItem.Name = "LowQualityToolStripMenuItem"
		Me.LowQualityToolStripMenuItem.Size = New System.Drawing.Size(128, 22)
		Me.LowQualityToolStripMenuItem.Text = "Low"
		'
		'MainForm
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(412, 437)
		Me.Controls.Add(Me.ToolStripContainer1)
		Me.Name = "MainForm"
		Me.Text = "NShape Simple Shuffle Game"
		Me.ToolStripContainer1.ContentPanel.ResumeLayout(False)
		Me.ToolStripContainer1.TopToolStripPanel.ResumeLayout(False)
		Me.ToolStripContainer1.TopToolStripPanel.PerformLayout()
		Me.ToolStripContainer1.ResumeLayout(False)
		Me.ToolStripContainer1.PerformLayout()
		Me.ToolStrip1.ResumeLayout(False)
		Me.ToolStrip1.PerformLayout()
		Me.ResumeLayout(False)

	End Sub
	Friend WithEvents Display As Dataweb.NShape.WinFormsUI.Display
	Friend WithEvents ToolStripContainer1 As System.Windows.Forms.ToolStripContainer
	Friend WithEvents ToolStrip1 As System.Windows.Forms.ToolStrip
	Friend WithEvents ToolStripButton2 As System.Windows.Forms.ToolStripButton
	Friend WithEvents Project As Dataweb.NShape.Project
	Friend WithEvents DiagramSetController As Dataweb.NShape.Controllers.DiagramSetController
	Friend WithEvents CachedRepository As Dataweb.NShape.Advanced.CachedRepository
	Friend WithEvents ToolStripDropDownButton1 As System.Windows.Forms.ToolStripDropDownButton
	Friend WithEvents MaximumQualityToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
	Friend WithEvents HighQualityToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
	Friend WithEvents MediumQualityToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
	Friend WithEvents LowQualityToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

End Class

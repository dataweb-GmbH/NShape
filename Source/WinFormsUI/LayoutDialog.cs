/**************************************************************************************************
  Copyright 2009-2022 dataweb GmbH
  This file is part of the NShape framework.
  NShape is free software: you can redistribute it and/or modify it under the 
  terms of the GNU General Public License as published by the Free Software 
  Foundation, either version 3 of the License, or (at your option) any later 
  version.
  NShape is distributed in the hope that it will be useful, but WITHOUT ANY
  WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR 
  A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
  You should have received a copy of the GNU General Public License along with 
  NShape. If not, see <http://www.gnu.org/licenses/>.
***************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;
using Dataweb.NShape.Layouters;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// A dialog used for layouting shapes on a diagram.
	/// </summary>
	[ToolboxItem(false)]
	public partial class LayoutDialog : Form {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.LayoutDialog" />.
		/// </summary>
		public LayoutDialog() {
			InitializeComponent();
			Icon = System.Drawing.Icon.ExtractAssociatedIcon(this.GetType().Assembly.Location);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.LayoutDialog" />.
		/// </summary>
		public LayoutDialog(ILayouter layouter)
			: this() {
			if (layouter == null) throw new ArgumentNullException("layouter");

			int panelIdx = -1;
			if (layouter is ExpansionLayouter) {
				panelIdx = algorithmListBox.Items.IndexOf("Expansion");
				horizontalCompressionTrackBar.Value = ((ExpansionLayouter)layouter).HorizontalCompression;
				verticalCompressionTrackBar.Value = ((ExpansionLayouter)layouter).VerticalCompression;
			} else if (layouter is GridLayouter) {
				panelIdx = algorithmListBox.Items.IndexOf("Alignment");
				columnDistanceTrackBar.Value = ((GridLayouter)layouter).CoarsenessX;
				rowDistanceTrackBar.Value = ((GridLayouter)layouter).CoarsenessY;
			} else if (layouter is RepulsionLayouter) {
				panelIdx = algorithmListBox.Items.IndexOf("Clusters");
				repulsionStrengthTrackBar.Value = ((RepulsionLayouter)layouter).Repulsion;
				repulsionRangeTrackBar.Value = ((RepulsionLayouter)layouter).RepulsionRange;
				attractionStrengthTrackBar.Value = ((RepulsionLayouter)layouter).SpringRate;
			} else if (layouter is FlowLayouter) {
				panelIdx = algorithmListBox.Items.IndexOf("Flow");
				bottomUpRadioButton.Checked = ((FlowLayouter)layouter).Direction == FlowLayouter.FlowDirection.BottomUp;
				leftToRightRadioButton.Checked = ((FlowLayouter)layouter).Direction == FlowLayouter.FlowDirection.LeftToRight;
				topDownRadioButton.Checked = ((FlowLayouter)layouter).Direction == FlowLayouter.FlowDirection.TopDown;
				rightToLeftRadioButton.Checked = ((FlowLayouter)layouter).Direction == FlowLayouter.FlowDirection.RightToLeft;
				flowLayerDistanceTrackBar.Value = ((FlowLayouter)layouter).LayerDistance;
				flowRowDistanceTrackBar.Value = ((FlowLayouter)layouter).RowDistance;
			}

			this._layouter = layouter;
			algorithmListBox.SelectedIndex = panelIdx;
		}


		/// <summary>
		/// Raised when the layout changed.
		/// </summary>
		public event EventHandler LayoutChanged;


		/// <summary>
		/// Provides access to a <see cref="T:Dataweb.NShape.Project" />.
		/// </summary>
		public Project Project {
			get { return _project; }
			set {
				if (_project != null) UnregisterProjectEvents(_project);
				_project = value;
				if (_project != null) RegisterProjectEvents(_project);
			}
		}


		/// <summary>
		/// Specifies the diagram of the layouted shapes.
		/// </summary>
		public Diagram Diagram {
			get { return _diagram; }
			set {
				_selectedShapes.Clear();
				_diagram = value;
			}
		}


		/// <summary>
		/// Specifies the shapes to be layouted.
		/// </summary>
		public IEnumerable<Shape> SelectedShapes {
			get { return _selectedShapes; }
			set {
				_selectedShapes.Clear();
				_selectedShapes.AddRange(value);
			}
		}


		#region [Private] Methods

		private void PrepareLayouter() {
			switch ((string)_currentPanel.Tag) {
				case "Expansion": // Distribution
					if (_layouter == null || !(_layouter is ExpansionLayouter))
						_layouter = new ExpansionLayouter(Project);
					ExpansionLayouter dl = (ExpansionLayouter)_layouter;
					dl.HorizontalCompression = horizontalCompressionTrackBar.Value;
					dl.VerticalCompression = verticalCompressionTrackBar.Value;
					break;
				case "Alignment":
					if (_layouter == null || !(_layouter is GridLayouter))
						_layouter = new GridLayouter(Project);
					GridLayouter gl = (GridLayouter)_layouter;
					gl.CoarsenessX = columnDistanceTrackBar.Value;
					gl.CoarsenessY = rowDistanceTrackBar.Value;
					/* gl.ColumnDistance = columnDistanceTrackBar.Value;
					gl.RowDistance = rowDistanceTrackBar.Value; */
					break;
				case "Clusters":
					if (_layouter == null || !(_layouter is RepulsionLayouter))
						_layouter = new RepulsionLayouter(Project);
					RepulsionLayouter rl = (RepulsionLayouter)_layouter;
					// The default distance between connected elements should be 100 display units.
					// The default distance between unconnected elements should be 300 display units.
					rl.Friction = 0; // 300;
					rl.Repulsion = repulsionStrengthTrackBar.Value;
					rl.RepulsionRange = repulsionRangeTrackBar.Value;
					rl.SpringRate = attractionStrengthTrackBar.Value;
					// Two unconnected elements at the same position should move to their default distance 
					// within two steps
					rl.Mass = 50;
					break;
				case "Flow":
					if (_layouter == null || !(_layouter is FlowLayouter))
						_layouter = new FlowLayouter(Project);
					FlowLayouter fl = (FlowLayouter)_layouter;
					if (bottomUpRadioButton.Checked) fl.Direction = FlowLayouter.FlowDirection.BottomUp;
					else if (leftToRightRadioButton.Checked) fl.Direction = FlowLayouter.FlowDirection.LeftToRight;
					else if (topDownRadioButton.Checked) fl.Direction = FlowLayouter.FlowDirection.TopDown;
					else if (rightToLeftRadioButton.Checked) fl.Direction = FlowLayouter.FlowDirection.RightToLeft;
					fl.LayerDistance = flowLayerDistanceTrackBar.Value;
					fl.RowDistance = flowRowDistanceTrackBar.Value;
					break;
				default:
					Debug.Assert(false);
					break;
			}
		}


		private void SetShapes() {
			if (_diagram != null) {
				_layouter.AllShapes = _diagram.Shapes;
				if (_selectedShapes.Count == 0) _layouter.Shapes = _diagram.Shapes;
				else _layouter.Shapes = _selectedShapes;
			}
		}


		private void DoPreview(int operation, bool fit) {
			try {
				if ((operation == 1 || operation == 4) && _lastOperation == operation
				&& _project.History.IsNextUndoCommand(_lastCommand))
					_project.History.Undo();
				SetShapes();
				_layouter.Prepare();
				_layouter.Execute(10);
				if (fit) _layouter.Fit(0, 0, _diagram.Size.Width, _diagram.Size.Height);
				_lastCommand = _layouter.CreateLayoutCommand();
				_lastOperation = operation;
				_project.ExecuteCommand(_lastCommand);
				OnLayoutChanged();
			} catch (NShapeException exc) {
				MessageBox.Show(exc.Message, "Cannot Layout");
			}
		}


		// Returns false, if there are no more steps to execute.
		private bool DoPreviewStep(int operation) {
			bool undo = _lastOperation == operation && _project.History.IsNextUndoCommand(_lastCommand);
			bool finished = !_layouter.ExecuteStep();
			_lastCommand = _layouter.CreateLayoutCommand();
			_lastOperation = operation;
			if (undo) _project.History.Undo();
			_project.ExecuteCommand(_lastCommand);
			OnLayoutChanged();
			return !finished;
		}


		private void PreviewIfImmediate() {
			if (immediateRadioButton.Checked) {
				PrepareLayouter();
				DoPreview(1, false);
			}
		}


		private void DisplayCurrentPanel() {
			Panel newPanel = null;
			foreach (Control c in Controls)
				if (c is Panel && (string)c.Tag == (string)algorithmListBox.SelectedItem) {
					newPanel = (Panel)c;
					break;
				}
			if (newPanel == null) {
				MessageBox.Show("This algorithm is currently not supported.");
			} else {
				newPanel.Show();
				// Hide all others
				foreach (Control c in Controls)
					if (c is Panel && c != newPanel) c.Hide();
				_currentPanel = newPanel;
			}
		}


		private void OnLayoutChanged() {
			if (LayoutChanged != null) LayoutChanged(this, EventArgs.Empty);
		}


		private void StartAnimatedPreview() {
			UnregisterProjectEvents(_project);
			previewButton.Text = "Running";
			layoutTimer.Interval = animationInterval;
			layoutTimer.Start();
		}


		private void FinishAnimatedPreview() {
			layoutTimer.Stop();
			previewButton.Text = "Execute";
			UpdateUndoRedoButtons();
			RegisterProjectEvents(_project);
		}


		private void RegisterProjectEvents(Project project) {
			project.History.CommandAdded += History_CommandAdded;
			project.History.CommandsExecuted += History_CommandsExecuted;
		}


		private void UnregisterProjectEvents(Project project) {
			project.History.CommandAdded -= History_CommandAdded;
			project.History.CommandsExecuted -= History_CommandsExecuted;
		}


		private void UpdateUndoRedoButtons() {
			undoButton.Enabled = (_project != null && _project.History.UndoCommandCount > 0);
			redoButton.Enabled = (_project != null && _project.History.RedoCommandCount > 0);
		}

		#endregion


		#region [Private] Form Event Handlers

		private void LayoutControlForm_Load(object sender, EventArgs e) {
			if (algorithmListBox.SelectedIndex < 0)
				algorithmListBox.SelectedIndex = 0;

			// Update labels
			gridColumnDistanceLabel.Text = columnDistanceTrackBar.Value.ToString() + " %";
			gridRowDistanceLabel.Text = rowDistanceTrackBar.Value.ToString() + " %";
			attractionStrengthLabel.Text = attractionStrengthTrackBar.Value.ToString();
			repulsionStrengthLabel.Text = repulsionStrengthTrackBar.Value.ToString();
			repulsionRangeLabel.Text = repulsionRangeTrackBar.Value.ToString();
			horizontalCompressionLabel.Text = horizontalCompressionTrackBar.Value.ToString();
			verticalCompressionLabel.Text = verticalCompressionTrackBar.Value.ToString();
		}


		private void LayoutControlForm_Shown(object sender, EventArgs e) {
			DisplayCurrentPanel();
			UpdateUndoRedoButtons();
		}


		private void applyButton_Click(object sender, EventArgs e) {
			// This way we "forget" that the last operation was an immediate one or a step.
			_lastOperation = 0;
		}


		private void centerButton_Click(object sender, EventArgs e) {
			PrepareLayouter();
			SetShapes();
			_layouter.Prepare();
			_layouter.Fit(0, 0, _diagram.Size.Width, _diagram.Size.Height);
			_lastCommand = _layouter.CreateLayoutCommand();
			_lastOperation = 0;
			_project.ExecuteCommand(_lastCommand);
			OnLayoutChanged();
		}


		private void algorithmListBox_SelectedIndexChanged(object sender, EventArgs e) {
			DisplayCurrentPanel();
		}


		private void layoutTimer_Tick(object sender, EventArgs e) {
			if (!DoPreviewStep(3)) FinishAnimatedPreview();
		}


		private void columnDistanceTrackBar_ValueChanged(object sender, EventArgs e) {
			gridColumnDistanceLabel.Text = columnDistanceTrackBar.Value.ToString();
			PreviewIfImmediate();
		}


		private void rowDistanceTrackBar_ValueChanged(object sender, EventArgs e) {
			gridRowDistanceLabel.Text = rowDistanceTrackBar.Value.ToString();
			PreviewIfImmediate();
		}


		private void attractionStrengthTrackBar_ValueChanged(object sender, EventArgs e) {
			attractionStrengthLabel.Text = attractionStrengthTrackBar.Value.ToString();
			PreviewIfImmediate();
		}


		private void repulsionStrengthTrackBar_ValueChanged(object sender, EventArgs e) {
			repulsionStrengthLabel.Text = repulsionStrengthTrackBar.Value.ToString();
			PreviewIfImmediate();
		}


		private void repulsionRangeTrackBar_ValueChanged(object sender, EventArgs e) {
			repulsionRangeLabel.Text = repulsionRangeTrackBar.Value.ToString();
			PreviewIfImmediate();
		}


		private void flowLayerDistanceTrackBar_ValueChanged(object sender, EventArgs e) {
			flowLayerDistanceLabel.Text = flowLayerDistanceTrackBar.Value.ToString();
			PreviewIfImmediate();
		}


		private void flowRowDistanceTrackBar_ValueChanged(object sender, EventArgs e) {
			flowRowDistanceLabel.Text = flowRowDistanceTrackBar.Value.ToString();
			PreviewIfImmediate();
		}


		private void horizontalCompressionTrackBar_ValueChanged(object sender, EventArgs e) {
			horizontalCompressionLabel.Text = horizontalCompressionTrackBar.Value.ToString();
			if (keepAspectRationCheckBox.Checked) verticalCompressionTrackBar.Value = horizontalCompressionTrackBar.Value;
			PreviewIfImmediate();
		}


		private void verticalCompressionTrackBar_ValueChanged(object sender, EventArgs e) {
			verticalCompressionLabel.Text = verticalCompressionTrackBar.Value.ToString();
			if (keepAspectRationCheckBox.Checked) horizontalCompressionTrackBar.Value = verticalCompressionTrackBar.Value;
			PreviewIfImmediate();
		}


		private void keepAspectRationCheckBox_CheckedChanged(object sender, EventArgs e) {
			if (keepAspectRationCheckBox.Checked) {
				verticalCompressionTrackBar.Value = horizontalCompressionTrackBar.Value;
				PreviewIfImmediate();
			}
		}


		private void previewButton_Click(object sender, EventArgs e) {
			try {
				if (layoutTimer.Enabled)
					FinishAnimatedPreview();
				else {
					PrepareLayouter();
					if (fastRadioButton.Checked) {
						DoPreview(2, true);
					} else if (animatedRadioButton.Checked) {
						SetShapes();
						_layouter.Prepare();
						StartAnimatedPreview();
					} else if (stepRadioButton.Checked) {
						// TODO 2: This will not take into account, if selection changes between steps
						// But that would be difficult to handle anyway.
						if (_lastOperation != 4 || !_project.History.IsNextUndoCommand(_lastCommand)) {
							SetShapes();
							_layouter.Prepare();
						}
						DoPreviewStep(4);
					} else {
						Debug.Fail("Unexpected option");
						DoPreview(2, true);
					}
				}
			} catch (Exception exc) {
				MessageBox.Show(exc.Message, "Error During Layout");
			}
		}


		private void closeButton_Click(object sender, EventArgs e) {
			if (Modal) DialogResult = DialogResult.OK;
			else Close();
		}


		private void immediateRadioButton_CheckedChanged(object sender, EventArgs e) {
			previewButton.Enabled = !immediateRadioButton.Checked;
		}


		private void undoButton_Click(object sender, EventArgs e) {
			try {
				_project.History.Undo();
			} catch (Exception exc) {
				MessageBox.Show(exc.Message, "Cannot Undo");
			}
		}


		private void redoButton_Click(object sender, EventArgs e) {
			try {
				_project.History.Redo();
			} catch (Exception exc) {
				MessageBox.Show(exc.Message, "Cannot Redo");
			}
		}


		private void History_CommandsExecuted(object sender, CommandsEventArgs e) {
			UpdateUndoRedoButtons();
		}


		private void History_CommandAdded(object sender, CommandEventArgs e) {
			UpdateUndoRedoButtons();
		}

		#endregion


		#region Fields

#if DEBUG
		const int animationInterval = 50;
#else
		const int animationInterval = 50;
#endif

		private Project _project;
		private Diagram _diagram;
		private List<Shape> _selectedShapes = new List<Shape>(100);

		private Panel _currentPanel;
		private ILayouter _layouter;
		private ICommand _lastCommand;
		// 1 == immediate, 2 = fast, 3 = animated, 4 = step
		private int _lastOperation;

		#endregion

	}
}
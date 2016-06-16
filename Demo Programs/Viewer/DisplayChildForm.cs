/******************************************************************************
  Copyright 2009-2012 dataweb GmbH
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
******************************************************************************/

using System.Drawing;
using System.Windows.Forms;
using Dataweb.NShape.WinFormsUI;


namespace NShapeViewer {

	public partial class DisplayChildForm : Form {
		
		public DisplayChildForm() {
			InitializeComponent();
		}


		public Display Display {
			get { return display; }
		}

		
		#region Child form implementation

		private void ClearDiagramInfoTexts() {
			// Clear window title
			Text = string.Empty;

			// Clear status bar labels
			shapeCntLabel.Text = string.Empty;
			zoomLabel.Text = string.Empty;
			mousePosLabel.Text = string.Empty;
			topLeftLabel.Text = string.Empty;
			bottomRightLabel.Text = string.Empty;

		}


		private void UpdateWindowTitle() {
			if (display.Diagram != null)
				Text = string.Format(windowTitleFormatStr, display.Diagram.Name, display.ZoomLevel);
		}


		private void UpdateDisplayedAreaText() {
			Rectangle r;
			display.ControlToDiagram(display.DrawBounds, out r);
			topLeftLabel.Text = string.Format(pointFormatStr, r.Left, r.Top);
			bottomRightLabel.Text = string.Format(pointFormatStr, r.Right, r.Bottom);
			zoomLabel.Text = string.Format(percentFormatStr, display.ZoomLevel);
		}


		private void UpdateDiagramInfoText() {
			if (display.Diagram != null) {
				int cnt = display.Diagram.Shapes.Count;
				shapeCntLabel.Text = string.Format(shapeCntFormatStr, cnt, (cnt != 1) ? "s" : "");
				sizeLabel.Text = string.Format(sizeFormatStr, display.Diagram.Width, display.Diagram.Height);
			}
		}

		#endregion


		#region NShape Display event handler implementations

		private void display_DiagramChanged(object sender, System.EventArgs e) {
			UpdateWindowTitle();
			UpdateDiagramInfoText();
			UpdateDisplayedAreaText();
		}

		
		private void display_DiagramChanging(object sender, System.EventArgs e) {
			ClearDiagramInfoTexts();
		}

		
		private void DisplayChildForm_Shown(object sender, System.EventArgs e) {
			UpdateWindowTitle();
			UpdateDiagramInfoText();
			UpdateDisplayedAreaText();
		}


		private void display_ShapeClick(object sender, Dataweb.NShape.Controllers.DiagramPresenterShapeClickEventArgs e) {
			// nothing to do
		}


		private void display_ShapeDoubleClick(object sender, Dataweb.NShape.Controllers.DiagramPresenterShapeClickEventArgs e) {
			// nothing to do
		}


		private void display_ZoomChanged(object sender, System.EventArgs e) {
			UpdateWindowTitle();
			UpdateDisplayedAreaText();
		}


		private void display_Scroll(object sender, ScrollEventArgs e) {
			UpdateDisplayedAreaText();
		}


		private void display_SizeChanged(object sender, System.EventArgs e) {
			UpdateDisplayedAreaText();
		}


		private void display_MouseMove(object sender, MouseEventArgs e) {
			int x, y;
			display.ControlToDiagram(e.X, e.Y, out x, out y);
			mousePosLabel.Text = string.Format(pointFormatStr, x, y);
		}

		#endregion


		private const string windowTitleFormatStr = "{0} ({1} %)";
		private const string shapeCntFormatStr = "{0} Shape{1}";
		private const string percentFormatStr = "{0} %";
		private const string pointFormatStr = "{0}, {1}";
		private const string sizeFormatStr = "{0} x {1}";

	}
}

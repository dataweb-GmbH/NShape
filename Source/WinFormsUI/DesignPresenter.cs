/******************************************************************************
  Copyright 2009-2019 dataweb GmbH
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

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// A DesignPresenter user control.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(DesignPresenter), "DesignPresenter.bmp")]
	public partial class DesignPresenter : UserControl, IDisplayService {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.DesignPresenter" />.
		/// </summary>
		public DesignPresenter() {
			SetStyle(ControlStyles.ResizeRedraw
				| ControlStyles.AllPaintingInWmPaint
				| ControlStyles.OptimizedDoubleBuffer
				| ControlStyles.SupportsTransparentBackColor
				, true);
			UpdateStyles();

			// Initialize Components
			InitializeComponent();
			_infoGraphics = Graphics.FromHwnd(Handle);
			
			this._matrix = new Matrix();
			this._formatterFlags = 0 | StringFormatFlags.NoWrap;
			this._formatter = new StringFormat(_formatterFlags);
			this._formatter.Trimming = StringTrimming.EllipsisCharacter;
			this._formatter.Alignment = StringAlignment.Center;
			this._formatter.LineAlignment = StringAlignment.Center;

			propertyGrid.Site = this.Site;
			styleListBox.BackColor = SelectedItemColor;
		}


		/// <summary>
		/// Finalizer of Dataweb.NShape.WinFormsUI.DesignPresenter
		/// </summary>
		~DesignPresenter() {
			_infoGraphics.Dispose();
			_infoGraphics = null;
		}


		#region IDisplayService Members

		/// <override></override>
		void IDisplayService.Invalidate(int x, int y, int width, int height) { /* nothing to do */ }

		/// <override></override>
		void IDisplayService.Invalidate(Rectangle rectangle) { /* nothing to do */ }

		/// <override></override>
		void IDisplayService.NotifyBoundsChanged() { /* nothing to do */ }
		
		/// <override></override>
		Graphics IDisplayService.InfoGraphics {
			get { return _infoGraphics; }
		}

		/// <override></override>
		IFillStyle IDisplayService.HintBackgroundStyle {
			get {
				if (Project != null && Project.IsOpen)
					return Project.Design.FillStyles.White;
				else return null;
			}
		}

		/// <override></override>
		ILineStyle IDisplayService.HintForegroundStyle {
			get {
				if (Project != null && Project.IsOpen)
					return Project.Design.LineStyles.Normal;
				else return null;
			}
		}

		#endregion


		#region [Public] Events

		/// <summary>
		/// Raised when a design was selected.
		/// </summary>
		public event EventHandler DesignSelected;

		/// <summary>
		/// Raised when a style was selected.
		/// </summary>
		public event EventHandler StyleSelected;

		#endregion


		#region [Public] Properties: DesignPresenter

		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		[Browsable(true)]
		public new string ProductVersion {
			get { return base.ProductVersion; }
		}


		/// <summary>
		/// Provides access to a <see cref="T:Dataweb.NShape.Project" />.
		/// </summary>
		[CategoryNShape()]
		public Project Project {
			get { return (_designController == null) ? null : _designController.Project; }
		}


		/// <summary>
		/// The design controller component
		/// </summary>
		[CategoryNShape()]
		public DesignController DesignController {
			get { return _designController; }
			set {
				if (_designController != null) UnregisterDesignControllerEventHandlers();
				_designController = value;
				if (_designController != null) RegisterDesignControllerEventHandlers();
			}
		}


		/// <summary>
		/// The selected design.
		/// </summary>
		[Browsable(false)]
		public Design SelectedDesign {
			get { return _selectedDesign; }
			set {
				if (_selectedDesign != value) {
					SelectedStyle = null;
					_selectedDesign = value;
					
					StyleUITypeEditor.Design = _selectedDesign;
					InitializeStyleCollectionList();
				}
				if (DesignSelected != null) DesignSelected(this, EventArgs.Empty);
			}
		}


		/// <summary>
		/// The selected style
		/// </summary>
		[Browsable(false)]
		public StyleCategory SelectedStyleCategory {
			get { return styleListBox.StyleCategory; }
			set {
				if (styleListBox.StyleCategory != value) {
					switch (value) {
						case StyleCategory.CapStyle:
							styleCollectionListBox.SelectedIndex = capStylesItemIdx;
							break;
						case StyleCategory.CharacterStyle:
							styleCollectionListBox.SelectedIndex = charStylesItemIdx;
							break;
						case StyleCategory.ColorStyle:
							styleCollectionListBox.SelectedIndex = colorStylesItemIdx;
							break;
						case StyleCategory.FillStyle:
							styleCollectionListBox.SelectedIndex = fillStylesItemIdx;
							break;
						case StyleCategory.LineStyle:
							styleCollectionListBox.SelectedIndex = lineStylesItemIdx;
							break;
						case StyleCategory.ParagraphStyle:
							styleCollectionListBox.SelectedIndex = paragraphStylesItemIdx;
							break;
						default: throw new NShapeUnsupportedValueException(value);
					}
				}
			}
		}


		/// <summary>
		/// The selected style
		/// </summary>
		[Browsable(false)]
		public Style SelectedStyle {
			get { return _selectedStyle; }
			private set {
				if (_selectedStyle != value) {
					_selectedStyle = value;
					if (propertyController != null) propertyController.SetObject(0, _selectedStyle);
					if (StyleSelected != null) StyleSelected(this, EventArgs.Empty);
				}
			}
		}

		#endregion


		#region [Public] Properties: Visuals

		/// <summary>
		/// Background color of inactive items
		/// </summary>
		public Color InactiveItemBackgroundColor {
			get {
				return _backgroundColor;
			}
			set {
				if (_backgroundBrush != null) {
					_backgroundBrush.Dispose();
					_backgroundBrush = null;
				}
				_backgroundColor = value;
			}
		}


		/// <summary>
		/// Fill color of highlighted items
		/// </summary>
		public Color HighlightedItemColor {
			get { return _highlightedItemColor; }
			set {
				if (_highlightedItemBrush != null) {
					_highlightedItemBrush.Dispose();
					_highlightedItemBrush = null;
				}
				_highlightedItemColor = value;
			}
		}


		/// <summary>
		/// Fill color of selected items
		/// </summary>
		public Color SelectedItemColor {
			get { return _selectedItemColor; }
			set {
				if (_selectedItemBrush != null) {
					_selectedItemBrush.Dispose();
					_selectedItemBrush = null;
				}
				_selectedItemColor = value;
			}
		}


		/// <summary>
		/// Border color of inactive items
		/// </summary>
		public Color InactiveItemBorderColor {
			get { return _itemBorderColor; }
			set {
				if (_itemBorderPen != null) {
					_itemBorderPen.Dispose();
					_itemBorderPen = null;
				}
				_itemBorderColor = value;
			}
		}


		/// <summary>
		/// Fill color of focused items
		/// </summary>
		public Color FocusedItemColor {
			get { return _focusBackgroundColor; }
			set {
				if (_focusBackgroundBrush != null) {
					_focusBackgroundBrush.Dispose();
					_focusBackgroundBrush = null;
				}
				_focusBackgroundColor = value;
			}
		}


		/// <summary>
		/// Border color of focused items
		/// </summary>
		public Color FocusBorderColor {
			get { return _focusBorderColor; }
			set {
				if (_selectedBorderPen != null) {
					_selectedBorderPen.Dispose();
					_selectedBorderPen = null;
				}
				_focusBorderColor = value;
			}
		}


		/// <summary>
		/// Text color of selected items
		/// </summary>
		public Color SelectedItemTextColor {
			get { return _selectedTextColor; }
			set {
				if (_selectedTextBrush != null) {
					_selectedTextBrush.Dispose();
					_selectedTextBrush = null;
				}
				_selectedTextColor = value;
			}
		}


		/// <summary>
		/// Text color of selected items
		/// </summary>
		public Color InactiveItemTextColor {
			get { return _itemTextColor; }
			set {
				if (_itemTextBrush != null) {
					_itemTextBrush.Dispose();
					_itemTextBrush = null;
				}
				_itemTextColor = value;
			}
		}


		/// <summary>
		/// Specifies if Items should be highlighted.
		/// </summary>
		public bool HighlightItems {
			get { return _highlightItems; }
			set { _highlightItems = value; }
		}

		#endregion


		#region [Public] Methods

		/// <summary>
		/// Creates a new design.
		/// </summary>
		public void CreateDesign() {
			_designController.CreateDesign();
		}


		/// <summary>
		/// Deletes the selected design.
		/// </summary>
		public void DeleteSelectedDesign() {
			if (_selectedDesign != Project.Design) {
				_designController.DeleteDesign(_selectedDesign);
				SelectedDesign = Project.Design;
			}
		}


		/// <summary>
		/// Create an new style.
		/// </summary>
		public void CreateStyle() {
			_designController.CreateStyle(_selectedDesign, styleListBox.StyleCategory);
		}


		/// <summary>
		/// Delete the selected style.
		/// </summary>
		public void DeleteSelectedStyle() {
			if (_designController.Project.Repository.IsStyleInUse(_selectedStyle)) {
				string msgTxt = string.Format(Dataweb.NShape.WinFormsUI.Properties.Resources.MessageFmt_Style0IsStillInUse, _selectedStyle.Title);
				MessageBox.Show(this, msgTxt, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Warning);
			} else _designController.DeleteStyle(_selectedDesign, _selectedStyle);
		}


		/// <summary>
		///  Activates the selected design as the project's design.
		/// </summary>
		/// <param name="design"></param>
		public void ActivateDesign(Design design) {
			if (Project.Design != design) Project.ApplyDesign(design);
		}

		#endregion


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}


		private void InitializeStyleCollectionList() {
			styleCollectionListBox.Items.Clear();
			styleCollectionListBox.Items.Insert(colorStylesItemIdx, "Color Styles");
			styleCollectionListBox.Items.Insert(fillStylesItemIdx, "Fill Styles");
			styleCollectionListBox.Items.Insert(lineStylesItemIdx, "Line Styles");
			styleCollectionListBox.Items.Insert(capStylesItemIdx, "Line Cap Styles");
			styleCollectionListBox.Items.Insert(charStylesItemIdx, "Character Styles");
			styleCollectionListBox.Items.Insert(paragraphStylesItemIdx, "Paragraph Styles");

			styleCollectionListBox.SelectedIndex = 0;
			styleCollectionListBox.Invalidate();
		}


		#region [Private] Methods: (Un)Register events

		private void RegisterDesignControllerEventHandlers() {
			if (_designController != null) {
				_designController.Initialized += designController_Initialized;
				_designController.Uninitialized += designController_Uninitialized;
				_designController.DesignCreated += designController_DesignCreated;
				_designController.DesignChanged += designController_DesignChanged;
				_designController.DesignDeleted += designController_DesignDeleted;
				_designController.StyleCreated += designController_StyleCreated;
				_designController.StyleChanged += designController_StyleChanged;
				_designController.StyleDeleted += designController_StyleDeleted;
			}
		}


		private void UnregisterDesignControllerEventHandlers() {
			if (_designController != null) {
				_designController.Initialized -= designController_Initialized;
				_designController.Uninitialized -= designController_Uninitialized;
				_designController.DesignCreated -= designController_DesignCreated;
				_designController.DesignChanged -= designController_DesignChanged;
				_designController.DesignDeleted -= designController_DesignDeleted;
				_designController.StyleCreated -= designController_StyleCreated;
				_designController.StyleChanged -= designController_StyleChanged;
				_designController.StyleDeleted -= designController_StyleDeleted;
			}
		}

		#endregion


		#region [Private] Methods: DesignController event handler implementations

		private void designController_Initialized(object sender, EventArgs e) {
			propertyController.Project = Project;
			InitializeStyleCollectionList();
			SelectedDesign = _designController.Project.Design;
		}


		private void designController_Uninitialized(object sender, EventArgs e) {
			_selectedStyle = null;
			_selectedDesign = null;
			styleListBox.Items.Clear();
			// Only perform a CancalSetproperty if the probject still exists, otherwise the call will fail.
			if (propertyController.Project != null)
				propertyController.CancelSetProperty();
		}


		private void designController_StyleCreated(object sender, StyleEventArgs e) {
			if (!styleListBox.Items.Contains(e.Style)) {
				styleListBox.SuspendLayout();
				styleListBox.Items.Add(e.Style);
				styleListBox.SelectedItem = e.Style;
				styleListBox.ResumeLayout();
			}
		}


		private void designController_StyleChanged(object sender, StyleEventArgs e) {
			int idx = styleListBox.Items.IndexOf(e.Style);
			if (idx >= 0) {
				bool isSelectedStyle = (styleListBox.SelectedItem == e.Style);
				styleListBox.SuspendLayout();
				styleListBox.Items.RemoveAt(idx);
				styleListBox.Items.Insert(idx, e.Style);
				if (isSelectedStyle)
					styleListBox.SelectedItem = e.Style;
				styleListBox.ResumeLayout();
			}
			StyleUITypeEditor.Design = e.Design;
			if (propertyGrid.SelectedObject == e.Style)
				propertyGrid.Refresh();
		}


		private void designController_StyleDeleted(object sender, StyleEventArgs e) {
			if (styleListBox.Items.Contains(e.Style)) {
				propertyGrid.SuspendLayout();
				styleListBox.SuspendLayout();

				if (propertyController != null) {
					if (propertyController.GetSelectedObject(0) == e.Style)
						propertyController.SetObject(0, null);
				}
				// remove deleted item and select the previous one
				int idx = styleListBox.Items.IndexOf(e.Style);
				styleListBox.Items.RemoveAt(idx);
				--idx;
				if (idx < 0 && styleListBox.Items.Count > 0)
					idx = 0;
				styleListBox.SelectedIndex = idx;

				styleListBox.ResumeLayout();
				propertyGrid.ResumeLayout();
			}
		}


		private void designController_DesignCreated(object sender, DesignEventArgs e) {
			// nothing to do
		}


		private void designController_DesignChanged(object sender, DesignEventArgs e) {
			// nothing to do
		}


		private void designController_DesignDeleted(object sender, DesignEventArgs e) {
			// nothing to do
		}

		#endregion


		#region [Private] Methods: Event handler implementations

		private void styleCollectionListBox_SelectedIndexChanged(object sender, System.EventArgs e) {
			//propertyGrid.SelectedObject = null;
			if (propertyController != null) propertyController.SetObject(0, null);
			//
			styleListBox.SuspendLayout();
			styleListBox.SelectedItem = null;
			styleListBox.Items.Clear();
			// Assigning a Design here results in items beeing created
			styleListBox.StyleSet = _selectedDesign;
			switch (styleCollectionListBox.SelectedIndex) {
				case -1:
					//nothing to do
					break;
				case capStylesItemIdx: styleListBox.StyleCategory = StyleCategory.CapStyle; break;
				case charStylesItemIdx: styleListBox.StyleCategory = StyleCategory.CharacterStyle; break;
				case colorStylesItemIdx: styleListBox.StyleCategory = StyleCategory.ColorStyle; break;
				case fillStylesItemIdx: styleListBox.StyleCategory = StyleCategory.FillStyle; break;
				case lineStylesItemIdx: styleListBox.StyleCategory = StyleCategory.LineStyle; break;
				case paragraphStylesItemIdx: styleListBox.StyleCategory = StyleCategory.ParagraphStyle; break;
				default: throw new NShapeException("Unexpected value.");
			}
			if (styleListBox.Items.Count > 0) styleListBox.SelectedIndex = 0;
		}


		private void styleListBox_SelectedIndexChanged(object sender, EventArgs e) {
			SelectedStyle = styleListBox.SelectedStyle;
		}

	
		private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e) {
		}

		#endregion


		#region Fields
		private Graphics _infoGraphics;

		private DesignController _designController = null;
		private Design _selectedDesign = null;
		private Style _selectedStyle = null;

		private bool _highlightItems = true;
		// Colors
		private Color _backgroundColor = Color.FromKnownColor(KnownColor.Control);
		private Color _highlightedItemColor = Color.FromKnownColor(KnownColor.ControlLightLight);
		private Color _selectedItemColor = Color.FromKnownColor(KnownColor.Window);
		private Color _selectedItemBorderColor = Color.FromKnownColor(KnownColor.ControlDarkDark);
		private Color _itemBorderColor = Color.FromKnownColor(KnownColor.Window);
		private Color _focusBackgroundColor = Color.Beige;
		private Color _focusBorderColor = Color.FromArgb(128, Color.Beige);
		private Color _itemTextColor = Color.FromKnownColor(KnownColor.ControlDarkDark);
		private Color _selectedTextColor = Color.FromKnownColor(KnownColor.ControlText);
		// Pens and Brushes
		private Brush _backgroundBrush;
		private Brush _highlightedItemBrush;
		private Brush _selectedItemBrush;
		private Brush _itemTextBrush;
		private Brush _selectedTextBrush;
		private Brush _focusBackgroundBrush;
		private Pen _itemBorderPen;
		private Pen _selectedBorderPen;
		// Buffers
		private Matrix _matrix;
		private StringFormat _formatter;
		private StringFormatFlags _formatterFlags;

		const int noneSelectedItemIdx = -1;
		const int colorStylesItemIdx = 0;
		const int fillStylesItemIdx = 1;
		const int lineStylesItemIdx = 2;
		const int capStylesItemIdx = 3;
		const int charStylesItemIdx = 4;
		const int paragraphStylesItemIdx = 5;
		#endregion
	}
}
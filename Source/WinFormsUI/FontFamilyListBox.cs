/******************************************************************************
  Copyright 2009-2021 dataweb GmbH
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Design;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// List box for displaying available font families including preview.
	/// </summary>
	[ToolboxItem(false)]
	public partial class FontFamilyListBox : ListBox {
		
		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.FontFamilyListBox" />.
		/// </summary>
		public FontFamilyListBox(IWindowsFormsEditorService editorService) {
			InitializeComponent();
			Construct(editorService);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.FontFamilyListBox" />.
		/// </summary>
		public FontFamilyListBox(IWindowsFormsEditorService editorService, IContainer container) {
			container.Add(this);
			InitializeComponent();
			Construct(editorService);
		}


		#region [Public] Properties

		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		[Browsable(true)]
		public new string ProductVersion {
			get { return base.ProductVersion; }
		}


		/// <summary>
		/// Specifies if 
		/// </summary>
		[CategoryBehavior()]
		public bool HighlightItems {
			get { return _highlightItems; }
			set { _highlightItems = value; }
		}


		/// <summary>
		/// Specifies the background color for normal items.
		/// </summary>
		public Color ItemBackgroundColor {
			get { return _itemBackgroundColor; }
			set {
				if (_itemBackgroundBrush != null) {
					_itemBackgroundBrush.Dispose();
					_itemBackgroundBrush = null;
				}
				_itemBackgroundColor = value;
			}
		}


		/// <summary>
		/// Specifies the background color for highlighted items.
		/// </summary>
		public Color ItemHighlightedColor {
			get { return _itemHighlightedColor; }
			set {
				if (_itemHighlightedBrush != null) {
					_itemHighlightedBrush.Dispose();
					_itemHighlightedBrush = null;
				}
				_itemHighlightedColor = value;
			}
		}


		/// <summary>
		/// Specifies the background color for selected items.
		/// </summary>
		public Color ItemSelectedColor {
			get { return _itemSelectedColor; }
			set {
				if (_itemSelectedBrush != null) {
					_itemSelectedBrush.Dispose();
					_itemSelectedBrush = null;
				}
				_itemSelectedColor = value;
			}
		}


		/// <summary>
		/// Specifies the background color of focused items.
		/// </summary>
		public Color ItemFocusedColor {
			get { return _itemFocusedColor; }
			set {
				if (_itemFocusedBrush != null) {
					_itemFocusedBrush.Dispose();
					_itemFocusedBrush = null;
				}
				_itemFocusedColor = value;
			}
		}


		/// <summary>
		/// Specifies the border color for focused items.
		/// </summary>
		public Color FocusBorderColor {
			get { return _focusBorderColor; }
			set {
				if (_focusBorderPen != null) {
					_focusBorderPen.Dispose();
					_focusBorderPen = null;
				}
				_focusBorderColor = value;
			}
		}


		/// <summary>
		/// Spacifies the border color for normal items.
		/// </summary>
		public Color ItemBorderColor {
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
		/// Specifies the text color of all items.
		/// </summary>
		public Color TextColor {
			get { return _textColor; }
			set {
				if (_textBrush != null) {
					_textBrush.Dispose();
					_textBrush = null;
				}
				_textColor = value;
			}
		}

		#endregion


		#region [Protected] Methods: Overrides

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				// dispose drawing stuff
				_formatter.Dispose();
				foreach (Font font in _fonts)
					font.Dispose();
				_fonts.Clear();

				if (components != null)
					components.Dispose();
			}
			base.Dispose(disposing);
		}

	
		/// <override></override>
		protected override void OnKeyUp(KeyEventArgs e) {
			base.OnKeyUp(e);
			if (e.KeyData == Keys.Return || e.KeyData == Keys.Space) {
				ExecuteSelection();
			}
		}


		/// <override></override>
		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left) {
				for (int i = Items.Count - 1; i >= 0; --i) {
					if (Geometry.RectangleContainsPoint(GetItemRectangle(i), e.Location)) {
						ExecuteSelection();
						break;
					}
				}
			}
		}


		/// <override></override>
		protected override void OnMeasureItem(MeasureItemEventArgs e) {
			if (e.Index >= 0) {
				e.ItemHeight = (int)Math.Ceiling(_fonts[e.Index].GetHeight(e.Graphics));
				e.ItemWidth = Width;
			}
			else base.OnMeasureItem(e);
		}


		/// <override></override>
		protected override void OnDrawItem(DrawItemEventArgs e) {
			_itemBounds = e.Bounds;
			_itemBounds.Inflate(-3, -1);

			// Draw Item Background and Border
			e.Graphics.FillRectangle(ItemBackgroundBrush, _itemBounds);
			if (_itemBorderColor != Color.Transparent)
				e.Graphics.DrawRectangle(ItemBorderPen, _itemBounds);

			// Draw Selection and/or Focus markers
			if ((e.State & DrawItemState.Selected) != 0)
				e.Graphics.FillRectangle(ItemSelectedBrush, _itemBounds);
			if ((e.State & DrawItemState.Focus) != 0) {
				if (_itemFocusedColor != Color.Transparent)
					e.Graphics.FillRectangle(ItemFocusedBrush, _itemBounds);
				if (FocusBorderColor != Color.Transparent)
					e.Graphics.DrawRectangle(FocusBorderPen, _itemBounds);
			}
			else if (HighlightItems && (e.State & DrawItemState.HotLight) != 0)
				if (ItemHighlightedColor != Color.Transparent)
					e.Graphics.FillRectangle(ItemHighlightedBrush, _itemBounds);

			e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			if (e.Index >= 0) {
				Font font = _fonts[e.Index];
				e.Graphics.DrawString(font.FontFamily.Name, font, Brushes.Black, _itemBounds, _formatter);
			}
			else base.OnDrawItem(e);
		}

		#endregion


		#region [Private] Properties: Pens and Brushes

		private Brush ItemBackgroundBrush {
			get {
				if (_itemBackgroundBrush == null)
					_itemBackgroundBrush = new SolidBrush(ItemBackgroundColor);
				return _itemBackgroundBrush;
			}
		}


		private Brush ItemHighlightedBrush {
			get {
				if (_itemHighlightedBrush == null)
					_itemHighlightedBrush = new SolidBrush(_itemHighlightedColor);
				return _itemHighlightedBrush;
			}
		}


		private Brush ItemSelectedBrush {
			get {
				if (_itemSelectedBrush == null)
					_itemSelectedBrush = new SolidBrush(_itemSelectedColor);
				return _itemSelectedBrush;
			}
		}


		private Brush TextBrush {
			get {
				if (_textBrush == null)
					_textBrush = new SolidBrush(_textColor);
				return _textBrush;
			}
		}


		private Brush ItemFocusedBrush {
			get {
				if (_itemFocusedBrush == null)
					_itemFocusedBrush = new SolidBrush(ItemFocusedColor);
				return _itemFocusedBrush;
			}
		}


		private Pen FocusBorderPen {
			get {
				if (_focusBorderPen == null) {
					_focusBorderPen = new Pen(_focusBorderColor);
					_focusBorderPen.Alignment = PenAlignment.Inset;
				}
				return _focusBorderPen;
			}
		}


		private Pen ItemBorderPen {
			get {
				if (_itemBorderPen == null) {
					_itemBorderPen = new Pen(_itemBorderColor);
					_itemBorderPen.Alignment = PenAlignment.Inset;
				}
				return _itemBorderPen;
			}
		}

		#endregion


		#region [Private] Methods

		private void Construct(IWindowsFormsEditorService editorService) {
			if (editorService == null) throw new ArgumentNullException("editorService");
			this._editorService = editorService;

			this.IntegralHeight = false;
			this.DrawMode = DrawMode.OwnerDrawVariable;
			this.SelectionMode = SelectionMode.One;
			this.DoubleBuffered = true;

			_formatter.Alignment = StringAlignment.Near;
			_formatter.LineAlignment = StringAlignment.Near;
			int fontSize = 10;
			foreach (FontFamily fontFamily in FontFamily.Families) {
				Font font = null;
				if (fontFamily.IsStyleAvailable(FontStyle.Regular))
					font = new Font(fontFamily.Name, fontSize, FontStyle.Regular);
				else if (fontFamily.IsStyleAvailable(FontStyle.Italic))
					font = new Font(fontFamily.Name, fontSize, FontStyle.Italic);
				else if (fontFamily.IsStyleAvailable(FontStyle.Bold))
					font = new Font(fontFamily.Name, fontSize, FontStyle.Bold);
				else if (fontFamily.IsStyleAvailable(FontStyle.Strikeout))
					font = new Font(fontFamily.Name, fontSize, FontStyle.Strikeout);
				else if (fontFamily.IsStyleAvailable(FontStyle.Underline))
					font = new Font(fontFamily.Name, fontSize, FontStyle.Underline);
				else
					font = Font;
				_fonts.Add(font);
			}
		}


		private void ExecuteSelection() {
			if (_editorService != null) _editorService.CloseDropDown();
		}

		#endregion


		#region Fields

		private IWindowsFormsEditorService _editorService;
		private bool _highlightItems = true;
		
		// drawing stuff
		private List<Font> _fonts = new List<Font>(FontFamily.Families.Length);
		private StringFormat _formatter = new StringFormat();
		
		private const int margin = 2;
		private Rectangle _itemBounds = Rectangle.Empty;

		private Color _itemBackgroundColor = Color.FromKnownColor(KnownColor.Window);
		private Color _itemHighlightedColor = Color.FromKnownColor(KnownColor.HighlightText);
		private Color _itemSelectedColor = Color.FromKnownColor(KnownColor.MenuHighlight);
		private Color _textColor = Color.FromKnownColor(KnownColor.WindowText);
		private Color _itemFocusedColor = Color.Transparent;
		private Color _focusBorderColor = Color.Transparent;
		private Color _itemBorderColor = Color.Transparent;

		private Brush _itemBackgroundBrush;
		private Brush _itemHighlightedBrush;
		private Brush _itemSelectedBrush;
		private Brush _itemFocusedBrush;
		private Brush _textBrush;
		private Pen _itemBorderPen;
		private Pen _focusBorderPen;

		#endregion
	}
}

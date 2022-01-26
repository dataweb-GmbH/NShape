/******************************************************************************
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
******************************************************************************/

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// A vertical tab control component.
	/// </summary>
	[ToolboxItem(false)]
	public partial class VerticalTabControl : ListBox {
		
		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.VerticalTabControl" />.
		/// </summary>
		public VerticalTabControl() {
			InitializeComponent();
			SetStyle(ControlStyles.AllPaintingInWmPaint |
						ControlStyles.OptimizedDoubleBuffer |
						ControlStyles.ResizeRedraw,
						true);
			UpdateStyles();
			IntegralHeight = false;

			this._formatterFlags = 0 | StringFormatFlags.NoWrap;
			this._formatter = new StringFormat(_formatterFlags);
			this._formatter.Trimming = StringTrimming.EllipsisCharacter;
			this._formatter.Alignment = StringAlignment.Center;
			this._formatter.LineAlignment = StringAlignment.Center;
		}


		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		[Browsable(true)]
		public new string ProductVersion {
			get { return base.ProductVersion; }
		}


		#region [Public] Properties: Colors

		/// <summary>
		/// Specifies the background color for inactive items.
		/// </summary>
		[CategoryAppearance()]
		public Color InactiveItemBackgroundColor {
			get { return BackColor; }
			set {
				if (_backgroundBrush != null) {
					_backgroundBrush.Dispose();
					_backgroundBrush = null;
				}
				BackColor = value;
			}
		}


		/// <summary>
		/// Specifies the fill color of highlighted items.
		/// </summary>
		[CategoryAppearance()]
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
		/// Specifies the fill color of selected items.
		/// </summary>
		[CategoryAppearance()]
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
		/// Specifies the border color of inactive items.
		/// </summary>
		[CategoryAppearance()]
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
		/// Specifies the background color of focussed items.
		/// </summary>
		[CategoryAppearance()]
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
		/// Specifies the border color of focussed items.
		/// </summary>
		[CategoryAppearance()]
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
		/// Specifies the text color of selected items.
		/// </summary>
		[CategoryAppearance()]
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
		/// Specifies the text color of inactive items.
		/// </summary>
		[CategoryAppearance()]
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

		#endregion


		#region [Protected] Overridden Methods

		/// <override></override>
		protected override void OnMeasureItem(MeasureItemEventArgs e) {
			base.OnMeasureItem(e);
			e.ItemWidth = Width;
			e.ItemHeight = 50;
		}


		/// <override></override>
		protected override void OnDrawItem(DrawItemEventArgs e) {
			base.OnDrawItem(e);

			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

			Rectangle itemBounds = Rectangle.Empty;
			itemBounds.X = e.Bounds.X + 3;
			itemBounds.Y = e.Bounds.Y + 1;
			itemBounds.Width = e.Bounds.Width - itemBounds.X;
			itemBounds.Height = e.Bounds.Height - 2;

			e.Graphics.FillRectangle(BackgroundBrush, e.Bounds);
			if ((e.State & DrawItemState.Selected) != 0) {
				e.Graphics.FillRectangle(SelectedItemBrush, itemBounds.Left, itemBounds.Top, e.Bounds.Width, itemBounds.Height);
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Right - 1, e.Bounds.Top, itemBounds.Right - 1, itemBounds.Top);
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Right - 1, itemBounds.Bottom, itemBounds.Right - 1, e.Bounds.Bottom);

				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Left, itemBounds.Top, itemBounds.Left, itemBounds.Bottom);
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Left, itemBounds.Top, itemBounds.Right, itemBounds.Top);
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Left, itemBounds.Bottom, itemBounds.Right, itemBounds.Bottom);
				e.Graphics.DrawString(Items[e.Index].ToString(), Font, SelectedTextBrush, itemBounds, _formatter);

				e.Graphics.FillRectangle(BackgroundBrush, 0, Items.Count * e.Bounds.Height, itemBounds.Right, Height);
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Right - 1, Items.Count * e.Bounds.Height, itemBounds.Right - 1, Height);
			}
			else {
				e.Graphics.DrawLine(FocusBorderPen, itemBounds.Right - 1, e.Bounds.Top, itemBounds.Right - 1, e.Bounds.Bottom);
				if (e.Index >= 0 && Items.Count > 0)
					e.Graphics.DrawString(Items[e.Index].ToString(), Font, ItemTextBrush, itemBounds, _formatter);
			}
		}


		/// <override></override>
		protected override void OnPaintBackground(PaintEventArgs pevent) {
			base.OnPaintBackground(pevent);
		}


		/// <override></override>
		protected override void OnPaint(PaintEventArgs e) {
			//e.Graphics.FillRectangle(BackgroundBrush, Bounds);
			base.OnPaint(e);
		}

		#endregion


		#region [Private] Properties: Pens and Brushes

		private Brush BackgroundBrush {
			get {
				if (_backgroundBrush == null)
					_backgroundBrush = new SolidBrush(InactiveItemBackgroundColor);
				return _backgroundBrush;
			}
		}


		private Brush HighlightedItemBrush {
			get {
				if (_highlightedItemBrush == null)
					_highlightedItemBrush = new SolidBrush(_highlightedItemColor);
				return _highlightedItemBrush;
			}
		}


		private Brush SelectedItemBrush {
			get {
				if (_selectedItemBrush == null)
					_selectedItemBrush = new SolidBrush(_selectedItemColor);
				return _selectedItemBrush;
			}
		}


		private Brush ItemTextBrush {
			get {
				if (_itemTextBrush == null)
					_itemTextBrush = new SolidBrush(_itemTextColor);
				return _itemTextBrush;
			}
		}


		private Brush SelectedTextBrush {
			get {
				if (_selectedTextBrush == null)
					_selectedTextBrush = new SolidBrush(_selectedTextColor);
				return _selectedTextBrush;
			}
		}


		private Brush FocusBackgroundBrush {
			get {
				if (_focusBackgroundBrush == null)
					_focusBackgroundBrush = new SolidBrush(_focusBackgroundColor);
				return _focusBackgroundBrush;
			}
		}


		private Pen ItemBorderPen {
			get {
				if (_itemBorderPen == null)
					_itemBorderPen = new Pen(_itemBorderColor);
				return _itemBorderPen;
			}
		}


		private Pen FocusBorderPen {
			get {
				if (_selectedBorderPen == null)
					_selectedBorderPen = new Pen(_selectedItemBorderColor);
				return _selectedBorderPen;
			}
		}

		#endregion


		#region Fields
		private StringFormat _formatter;
		private StringFormatFlags _formatterFlags;
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
		#endregion
	}
}

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

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// RichTextBox control used for editing captions of shapes on the diagram.
	/// </summary>
	[ToolboxItem(false)]
	public partial class InPlaceTextBox : RichTextBox {

		/// <summary>
		/// Creates a new instance of Dataweb.NShape.WinFormsUI.InPlaceTextBox.
		/// </summary>
		public InPlaceTextBox(IDiagramPresenter owner, ICaptionedShape shape, int captionIndex, string currentText)
			: this(owner, shape, captionIndex, currentText, null) {
		}


		/// <summary>
		/// Creates a new instance of Dataweb.NShape.WinFormsUI.InPlaceTextBox.
		/// </summary>
		public InPlaceTextBox(IDiagramPresenter owner, ICaptionedShape shape, int captionIndex, string currentText, string newText) {
			// Construct control and set Text
			Construct(owner, shape, captionIndex, currentText, newText);
			if (string.IsNullOrEmpty(newText)) {
				// Preselect the whole text if the user has not started typing yet
				SelectAll();
			} else {
				// Set the types text and place the cursor at the end of the text
				SelectionStart = Text.Length;
			}
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


		///// <override></override>
		//public new Rectangle Bounds {
		//   get { return drawBounds; }
		//   private set {
		//      drawBounds = value;
				
		//      // RichTextbox ignores the Padding property, 
		//      // so we shrink the whole control to maintain the correct padding
		//      value.Inflate(-paragraphStyle.Padding.Horizontal, 0); //-paragraphStyle.Padding.Vertical);
		//      value.Offset(paragraphStyle.Padding.Left, 0); //paragraphStyle.Padding.Top);

		//      SuspendLayout();
		//      this.Size = value.Size;
		//      this.Location = value.Location;
		//      ResumeLayout();
		//   }
		//}


		///// <override></override>
		//public new int Width {
		//   get { return drawBounds.Width; }
		//   private set { base.Width = value - paragraphStyle.Padding.Horizontal; }
		//}


		///// <override></override>
		//public new int Height {
		//   get { return drawBounds.Height; }
		//   private set { base.Height = value - paragraphStyle.Padding.Vertical; }
		//}


		/// <summary>
		/// Specifies the text's alignment
		/// </summary>
		public ContentAlignment TextAlignment {
			get { return ConvertToContentAlignment(SelectionAlignment); }
			set {
				_contentAlignment = value;
				try {
					SuspendLayout();

					SelectAll();
					SelectionAlignment = ConvertToHorizontalAlignment(value);
					DeselectAll();
					SelectionStart = Text.Length;
				} finally {
					ResumeLayout();
				}
			}
		}


		/// <summary>
		/// Specifies the initial, unmodified text.
		/// </summary>
		public string OriginalText {
			get { return _originalText; }
			private set { _originalText = value; }
		}
		
		
		/// <summary>
		/// Specifies the text to be edited.
		/// </summary>
		public override string Text {
			get { return base.Text; }
			set {
				base.Text = value;
				InvalidateEx();
			}
		}


		/// <summary>
		/// Extended version of the Invalidate method, invalidates the control itself and its parent control.
		/// </summary>
		public void InvalidateEx() {
			if (Parent != null)
				Parent.Invalidate(Bounds, true);
			else
				Invalidate();
		}

		#endregion


		#region [Protected] Methods

		/// <override></override>
		protected override CreateParams CreateParams {
			get {
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x00000020; //WS_EX_TRANSPARENT
				return cp;
			}
		}


		/// <override></override>
		protected override Padding DefaultPadding {
			get { return _defaultPadding; }
		}


		/// <override></override>
		protected override void OnFontChanged(EventArgs e) {
			base.OnFontChanged(e);
			InvalidateEx();
		}


		/// <override></override>
		protected override void OnMove(EventArgs e) {
			InvalidateEx();
			base.OnMove(e);
			DoUpdateBounds();
			InvalidateEx();
		}


		/// <override></override>
		protected override void OnParentChanged(EventArgs e) {
			base.OnParentChanged(e);
			InvalidateEx();
		}


		/// <override></override>
		protected override void OnResize(EventArgs eventargs) {
			InvalidateEx();
			base.OnResize(eventargs);
			DoUpdateBounds();
			InvalidateEx();
		}


		/// <override></override>
		protected override void OnHScroll(EventArgs e) {
			base.OnHScroll(e);
			InvalidateEx();
		}


		/// <override></override>
		protected override void OnVScroll(EventArgs e) {
			base.OnVScroll(e);
			InvalidateEx();
		}


		/// <override></override>
		protected override void NotifyInvalidate(Rectangle invalidatedArea) {
			base.NotifyInvalidate(invalidatedArea);
		}


		///// <override></override>
		//protected override void OnInvalidated(InvalidateEventArgs e) {
		//	if (!_invalidating) {
		//		try {
		//			_invalidating = true;
		//
		//			if (Parent != null) {
		//				if (_owner != Parent) { }
		//				Rectangle rect = e.InvalidRect;
		//				rect.Offset(Left, Top);
		//				Parent.Invalidate(rect, true);
		//			}
		//			base.OnInvalidated(e);
		//		} finally {
		//			_invalidating = false;
		//		}
		//	} else { }
		//}


		/// <override></override>
		protected override void OnTextChanged(EventArgs e) {
			try {
				_owner.SuspendUpdate();
				base.OnTextChanged(e);
				_shape.SetCaptionText(_captionIndex, Text);
				DoUpdateBounds();
			} finally {
				_owner.ResumeUpdate();
				InvalidateEx();
			}
		}


		/// <override></override>
		protected override void OnSelectionChanged(EventArgs e) {
			base.OnSelectionChanged(e);
			InvalidateEx();
		}


		/// <override></override>
		protected override void OnVisibleChanged(EventArgs e) {
			base.OnVisibleChanged(e);			
			InvalidateEx();
		}

		#endregion


		#region [Private] Methods

		private void Construct(IDiagramPresenter owner, ICaptionedShape shape, int captionIndex, string currentText, string newText) {
			if (owner == null) throw new ArgumentNullException(nameof(owner));
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			if (captionIndex < 0 || captionIndex >= shape.CaptionCount) throw new ArgumentOutOfRangeException(nameof(captionIndex));
			// Set control styles
			// Caution:
			// Even though the docs of "SupportsTransparentBackColor" says "Transparency will be simulated only if the ControlStyles.UserPaint 
			// bit is set to true", we have to omit this flag for the RichTextBox, otherwise the text will *not* be drawn!
			SetStyle(ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
			UpdateStyles();

			// Set text
			OriginalText = currentText;
			Text = newText ?? currentText;

			// Set caption / style specific properties
			this._owner = owner;
			this._shape = shape;
			this._captionIndex = captionIndex;

			// Set general properties
			this.BackColor = Color.Transparent;	// Does not matter - see CreateParams()
			this.AutoScrollOffset = Point.Empty;
			this.ScrollBars = RichTextBoxScrollBars.None;
			this.BorderStyle = BorderStyle.None;
			// Set Styles here because the ParagraphStyle is needed for resizing
			_characterStyle = shape.GetCaptionCharacterStyle(captionIndex);
			_paragraphStyle = shape.GetCaptionParagraphStyle(captionIndex);

			// Set base' members
			SuspendLayout();
			try {
				this.WordWrap = _paragraphStyle.WordWrap;
				this.Font = ToolCache.GetFont(_characterStyle);
				this.ZoomFactor = owner.ZoomFactor;

				// Get line height
				_lineHeight = CalcTextSize("Iq").Height;
				DoUpdateBounds();

				SelectAll();
				SelectionAlignment = ConvertToHorizontalAlignment(_paragraphStyle.Alignment);
				DeselectAll();

				VisibleChanged += InPlaceTextBox_VisibleChanged;
			} finally {
				ResumeLayout();
			}
			OnPaddingChanged(EventArgs.Empty);
		}


		private Size CalcTextSize(string text) {
			Size textSize = Geometry.InvalidSize;
			using (Graphics gfx = Graphics.FromHwnd(Handle))
				textSize = TextRenderer.MeasureText(gfx, text, Font);
			_owner.DiagramToControl(textSize, out textSize);
			return textSize;
		}


		/// <summary>
		/// Measures the text (using the WinForms TextRenderer which measures slightly different than our TextMeasurer,
		/// more specific for WinForms controls.
		/// </summary>
		private Size CalcTextSize(string text, Size proposedSize, IParagraphStyle paragraphStyle) {
			Debug.Assert(!text.Contains(Environment.NewLine));
			Size result = TextRenderer.MeasureText(((IDisplayService)_owner).InfoGraphics, text, Font, proposedSize, GetTextFormatFlags(paragraphStyle));
			_owner.DiagramToControl(result, out result);
			return result;
		}


		private void InPlaceTextBox_VisibleChanged(object sender, EventArgs e) {
			if (Visible) {
				VisibleChanged -= InPlaceTextBox_VisibleChanged;
				Focus();

				//SelectAll();
				//SelectionAlignment = ConvertToHorizontalAlignment(_paragraphStyle.Alignment);
				//DeselectAll();

				InvalidateEx();
			}
		}


		private void DoUpdateBounds() {
			Debug.Assert(_shape != null);
			Debug.Assert(_paragraphStyle != null);
			Debug.Assert(_captionIndex >= 0);

			// Get rotated caption bounds from the shape
			Point tl = Point.Empty, tr = Point.Empty, bl = Point.Empty, br = Point.Empty;
			_shape.GetCaptionBounds(_captionIndex, out tl, out tr, out br, out bl);
			// Calculate unrotated caption bounds
			float angle;
			Point center = Geometry.VectorLinearInterpolation(tl, br, 0.5f);
			angle = Geometry.RadiansToDegrees(Geometry.Angle(tl.X, tl.Y, tr.X, tr.Y));
			tl = Geometry.RotatePoint(center, -angle, tl);
			br = Geometry.RotatePoint(center, -angle, br);
			
			// Calculate unrotated layout area for the text
			Rectangle layoutArea  = Rectangle.Empty;
			layoutArea.Location = tl;
			layoutArea.Width = br.X - tl.X;
			layoutArea.Height = br.Y - tl.Y;
			// Measure the current size of the text
			Size textSize = TextMeasurer.MeasureText(Text, Font, layoutArea.Size, _paragraphStyle);
			//Size textSize = CalcTextSize(Text, layoutArea.Size, _paragraphStyle);

			// Transform layout area and text size to control coordinates
			_owner.DiagramToControl(layoutArea, out layoutArea);
			_owner.DiagramToControl(textSize, out textSize);
			
			// Calculate new bounds for the text editor based on the shape's caption layout rectangle
			Rectangle newBounds = Rectangle.Empty;
			newBounds.X = layoutArea.X;
			newBounds.Width = layoutArea.Width;
			newBounds.Height = Math.Max(textSize.Height, _lineHeight);
			// Move text editor depending on the vertical text alignment
			switch (_paragraphStyle.Alignment) {
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomRight:
					newBounds.Y = layoutArea.Bottom - newBounds.Height;
					break;
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleRight:
					newBounds.Y = layoutArea.Y + (int)Math.Round((layoutArea.Height - newBounds.Height) / 2f);
					break;
				case ContentAlignment.TopCenter:
				case ContentAlignment.TopLeft:
				case ContentAlignment.TopRight:
					newBounds.Y = layoutArea.Y;
					break;
				default:
					newBounds.Y = layoutArea.Y;
					Debug.Fail(string.Format("Unhandled {0} '{1}'.", _paragraphStyle.Alignment.GetType().Name, _paragraphStyle.Alignment));
					break;
			}
			// Correct textbox size by padding
			newBounds.X += _paragraphStyle.Padding.Left;
			newBounds.Y += _paragraphStyle.Padding.Top;
			newBounds.Width -= _paragraphStyle.Padding.Horizontal;
			newBounds.Height -= _paragraphStyle.Padding.Vertical;
			newBounds.Inflate(1, 1);

			// Update control bounds 
			// (No need to Invalidate here, this is done by OnResize())
			try {
				SuspendLayout();
				Bounds = newBounds;
				_defaultPadding = new Padding(_paragraphStyle.Padding.Left, _paragraphStyle.Padding.Top, _paragraphStyle.Padding.Right, _paragraphStyle.Padding.Bottom);
			} finally {
				ResumeLayout();
			}
		}


		private HorizontalAlignment ConvertToHorizontalAlignment(ContentAlignment contentAlignment) {
			switch (contentAlignment) {
				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter:
					return HorizontalAlignment.Center;
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft:
					return HorizontalAlignment.Left;
				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight:
					return HorizontalAlignment.Right;
				default: throw new NShapeUnsupportedValueException(typeof(ContentAlignment), contentAlignment);
			}
		}


		private ContentAlignment ConvertToContentAlignment(HorizontalAlignment horizontalAlignment) {
			switch (horizontalAlignment) {
				case HorizontalAlignment.Center:
					switch(_contentAlignment) {
						case ContentAlignment.BottomCenter:
							return ContentAlignment.BottomCenter;
						case ContentAlignment.MiddleCenter:
							return ContentAlignment.MiddleCenter;
						case ContentAlignment.TopCenter:
							return ContentAlignment.TopCenter;
						default: throw new NShapeUnsupportedValueException(typeof(ContentAlignment), _contentAlignment);
					}
				case HorizontalAlignment.Left:
					switch(_contentAlignment) {
						case ContentAlignment.BottomLeft:
							return ContentAlignment.BottomLeft;
						case ContentAlignment.MiddleLeft:
							return ContentAlignment.MiddleLeft;
						case ContentAlignment.TopLeft:
							return ContentAlignment.TopLeft;
						default: throw new NShapeUnsupportedValueException(typeof(ContentAlignment), _contentAlignment);
					}
				case HorizontalAlignment.Right:
					switch(_contentAlignment) {
						case ContentAlignment.BottomRight:
							return ContentAlignment.BottomRight;
						case ContentAlignment.MiddleRight:
							return ContentAlignment.MiddleRight;
						case ContentAlignment.TopRight:
							return ContentAlignment.TopRight;
						default: throw new NShapeUnsupportedValueException(typeof(ContentAlignment), _contentAlignment);
					}
				default: throw new NShapeUnsupportedValueException(typeof(HorizontalAlignment), horizontalAlignment);
			}
		}


		/// <summary>
		/// Returns a <see cref="T:System.Drawing.StringFormat"/> from the given <see cref="T:Dataweb.NShape.IParagraphStyle"/>.
		/// </summary>
		private static TextFormatFlags GetTextFormatFlags(IParagraphStyle paragraphStyle) {
			if (paragraphStyle == null) throw new ArgumentNullException(nameof(paragraphStyle));

			TextFormatFlags textFormat = TextFormatFlags.TextBoxControl;
			switch (paragraphStyle.Alignment) {
				case ContentAlignment.BottomLeft:
					textFormat |= (TextFormatFlags.Bottom | TextFormatFlags.Left);
					break;
				case ContentAlignment.BottomCenter:
					textFormat |= (TextFormatFlags.Bottom | TextFormatFlags.HorizontalCenter);
					break;
				case ContentAlignment.BottomRight:
					textFormat |= (TextFormatFlags.Bottom | TextFormatFlags.Right);
					break;
				case ContentAlignment.MiddleLeft:
					textFormat |= (TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
					break;
				case ContentAlignment.MiddleCenter:
					textFormat |= (TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
					break;
				case ContentAlignment.MiddleRight:
					textFormat |= (TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
					break;
				case ContentAlignment.TopLeft:
					textFormat |= (TextFormatFlags.Top | TextFormatFlags.Left);
					break;
				case ContentAlignment.TopCenter:
					textFormat |= (TextFormatFlags.Top | TextFormatFlags.HorizontalCenter);
					break;
				case ContentAlignment.TopRight:
					textFormat |= (TextFormatFlags.Top | TextFormatFlags.Right);
					break;
				default:
					throw new Exception(string.Format("Unexpected ContentAlignment value '{0}'.", paragraphStyle.Alignment));
			}
			if (paragraphStyle.WordWrap)
				textFormat |= TextFormatFlags.WordBreak;
			switch (paragraphStyle.Trimming) {
				case StringTrimming.EllipsisCharacter:
					textFormat |= TextFormatFlags.EndEllipsis;
					break;
				case StringTrimming.EllipsisPath:
					textFormat |= TextFormatFlags.PathEllipsis;
					break;
				case StringTrimming.EllipsisWord:
					textFormat |= TextFormatFlags.WordEllipsis;
					break;
			}

			return textFormat;
		}

		#endregion


		#region Fields
		private IDiagramPresenter _owner;
		private ICaptionedShape _shape;
		private int _captionIndex;
		
		private string _originalText = string.Empty;
		private IParagraphStyle _paragraphStyle;
		private ICharacterStyle _characterStyle;

		private int _lineHeight;

		private Padding _defaultPadding = Padding.Empty;
		private ContentAlignment _contentAlignment;
		private TextPaddingTypeConverter _paddingConverter = new TextPaddingTypeConverter();
		#endregion
	}

}

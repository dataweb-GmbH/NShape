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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;
using System.Runtime.InteropServices;

using Dataweb.Utilities;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Displays a bitmap in the diagram.
	/// </summary>
	public class PictureBase : RectangleBase {

		/// <summary>ControlPointId of the top left control point.</summary>
		public const int TopLeftControlPoint = 1;
		/// <summary>ControlPointId of the top center control point.</summary>
		public const int TopCenterControlPoint = 2;
		/// <summary>ControlPointId of the top right control point.</summary>
		public const int TopRightControlPoint = 3;
		/// <summary>ControlPointId of the middle left control point.</summary>
		public const int MiddleLeftControlPoint = 4;
		/// <summary>ControlPointId of the middle right control point.</summary>
		public const int MiddleRightControlPoint = 5;
		/// <summary>ControlPointId of the bottom left control point.</summary>
		public const int BottomLeftControlPoint = 6;
		/// <summary>ControlPointId of the bottom center control point.</summary>
		public const int BottomCenterControlPoint = 7;
		/// <summary>ControlPointId of the bottom right control point.</summary>
		public const int BottomRightControlPoint = 8;
		/// <summary>ControlPointId of the center control point.</summary>
		public const int MiddleCenterControlPoint = 9;


		#region [Public] Properties

		/// <ToBeCompleted></ToBeCompleted>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_PictureBase_Image")]
		[LocalizedDescription("PropDesc_PictureBase_Image")]
		[RequiredPermission(Permission.Present)]
		[Editor("Dataweb.NShape.WinFormsUI.NamedImageUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
		public NamedImage Image {
			get { return _image; }
			set {
				GdiHelpers.DisposeObject(ref _brushImage);
				if (NamedImage.IsNullOrEmpty(value))
					_image = null;
				else _image = value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_PictureBase_ImageLayout")]
		[LocalizedDescription("PropDesc_PictureBase_ImageLayout")]
		[PropertyMappingId(PropertyIdImageLayout)]
		[RequiredPermission(Permission.Present)]
		public ImageLayoutMode ImageLayout {
			get { return _imageLayout; }
			set {
				_imageLayout = value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_PictureBase_GrayScale")]
		[LocalizedDescription("PropDesc_PictureBase_GrayScale")]
		[PropertyMappingId(PropertyIdImageGrayScale)]
		[RequiredPermission(Permission.Present)]
		public bool GrayScale {
			get { return _imageGrayScale; }
			set {
				_imageGrayScale = value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_PictureBase_GammaCorrection")]
		[LocalizedDescription("PropDesc_PictureBase_GammaCorrection")]
		[PropertyMappingId(PropertyIdImageGamma)]
		[RequiredPermission(Permission.Present)]
		public float GammaCorrection {
			get { return _imageGamma; }
			set {
				if (value <= 0) throw new ArgumentOutOfRangeException(Dataweb.NShape.Properties.Resources.MessageTxt_ValueHasToBeGreaterThanZero);
				_imageGamma = value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_PictureBase_Transparency")]
		[LocalizedDescription("PropDesc_PictureBase_Transparency")]
		[PropertyMappingId(PropertyIdImageTransparency)]
		[RequiredPermission(Permission.Present)]
		public byte Transparency {
			get { return _imageTransparency; }
			set {
				if (value < 0 || value > 100) throw new ArgumentOutOfRangeException(Dataweb.NShape.Properties.Resources.MessageTxt_ValueHasToBeBetween0And100);
				_imageTransparency = value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_PictureBase_TransparentColor")]
		[LocalizedDescription("PropDesc_PictureBase_TransparentColor")]
		[PropertyMappingId(PropertyIdImageTransparentColor)]
		[RequiredPermission(Permission.Present)]
		public Color TransparentColor {
			get { return _transparentColor; }
			set {
				_transparentColor = value;
				InvalidateDrawCache();
				Invalidate();
			}
		}

		#endregion


		#region [Public] Methods

		/// <ToBeCompleted></ToBeCompleted>
		public void FitShapeToImageSize() {
			if (_image != null) {
				Width = _image.Width + (Width - _imageBounds.Width);
				Height = _image.Height + (Height - _imageBounds.Height);
			}
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new PictureBase(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is PictureBase) {
				PictureBase src = (PictureBase)source;
				if (!NamedImage.IsNullOrEmpty(src.Image))
					Image = src.Image.Clone();
				_imageGrayScale = src.GrayScale;
				_imageLayout = src.ImageLayout;
				_imageGamma = src.GammaCorrection;
				_imageTransparency = src.Transparency;
				_transparentColor = src.TransparentColor;
				_compressionQuality = src._compressionQuality;
			}
		}


		/// <override></override>
		public override void MakePreview(IStyleSet styleSet) {
			base.MakePreview(styleSet);
			_isPreview = true;
			GdiHelpers.DisposeObject(ref _imageAttribs);
			GdiHelpers.DisposeObject(ref _imageBrush);
			//if (!NamedImage.IsNullOrEmpty(image) && BrushImage != null)
			//   image.Image = BrushImage;
		}


		/// <override></override>
		public override ControlPointId HitTest(int x, int y, ControlPointCapabilities controlPointCapability, int range) {
			return base.HitTest(x, y, controlPointCapability, range);
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			UpdateDrawCache();
			Pen pen = ToolCache.GetPen(LineStyle, null, null);
			Brush brush = ToolCache.GetTransformedBrush(FillStyle, BoundingRectangleUnrotated, Center, Angle);
			//Brush brush = ToolCache.GetTransformedBrush(FillStyle, GetBoundingRectangle(true), Center, Angle);
			graphics.FillPath(brush, Path);

			Debug.Assert(_imageAttribs != null);
			Debug.Assert(Geometry.IsValid(_imageBounds));
			if (_image != null && _image.Image is Metafile) {
				GdiHelpers.DrawImage(graphics, _image.Image, _imageAttribs, _imageLayout, _imageBounds, Geometry.TenthsOfDegreeToDegrees(Angle), Center);
			} else {
				Debug.Assert(_imageBrush != null);
				graphics.FillPolygon(_imageBrush, _imageDrawBounds);
			}
			DrawCaption(graphics);
			graphics.DrawPath(pen, Path);
			if (Children.Count > 0) foreach (Shape s in Children) s.Draw(graphics);
		}

		#endregion


		#region IEntity Members

		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			_imageLayout = (ImageLayoutMode)reader.ReadByte();
			_imageTransparency = reader.ReadByte();
			_imageGamma = reader.ReadFloat();
			_compressionQuality = reader.ReadByte();
			_imageGrayScale = reader.ReadBool();
			string name = reader.ReadString();
			Image img = reader.ReadImage();
			if (name != null && img != null)
				_image = new NamedImage(img, name);
			_transparentColor = Color.FromArgb(reader.ReadInt32());
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteByte((byte)_imageLayout);
			writer.WriteByte(_imageTransparency);
			writer.WriteFloat(_imageGamma);
			writer.WriteByte(_compressionQuality);
			writer.WriteBool(_imageGrayScale);
			if (NamedImage.IsNullOrEmpty(_image)) {
				writer.WriteString(string.Empty);
				writer.WriteImage(null);
			} else {
				writer.WriteString(_image.Name);
				object imgTag = _image.Image.Tag;
				_image.Image.Tag = _image;
				writer.WriteImage(_image.Image);
				_image.Image.Tag = imgTag;
			}
			writer.WriteInt32(_transparentColor.ToArgb());
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.PictureBase" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in RectangleBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("ImageLayout", typeof(byte));
			yield return new EntityFieldDefinition("ImageTransparency", typeof(byte));
			yield return new EntityFieldDefinition("ImageGammaCorrection", typeof(float));
			yield return new EntityFieldDefinition("ImageCompressionQuality", typeof(byte));
			yield return new EntityFieldDefinition("ConvertToGrayScale", typeof(bool));
			yield return new EntityFieldDefinition("ImageFileName", typeof(string));
			yield return new EntityFieldDefinition("Image", typeof(Image));
			yield return new EntityFieldDefinition("ImageTransparentColor", typeof(int));
		}

		#endregion


		#region [Protected] Overridden Methods

		/// <ToBeCompleted></ToBeCompleted>
		protected internal PictureBase(ShapeType shapeType, Template template)
			: base(shapeType, template) {
			Construct();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal PictureBase(ShapeType shapeType, IStyleSet styleSet)
			: base(shapeType, styleSet) {
			Construct();
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			Size txtSize = Size.Empty;
			txtSize.Width = Width;
			txtSize.Height = Height;
			string txt = string.IsNullOrEmpty(Text) ? "Ip" : Text;
			if (DisplayService != null)
				txtSize = TextMeasurer.MeasureText(DisplayService.InfoGraphics, txt, CharacterStyle, txtSize, ParagraphStyle);
			else txtSize = TextMeasurer.MeasureText(txt, CharacterStyle, txtSize, ParagraphStyle);

			captionBounds = Rectangle.Empty;
			captionBounds.Width = Width;
			captionBounds.Height = Math.Min(Height, txtSize.Height);
			captionBounds.X = (int)Math.Round(-(Width / 2f));
			captionBounds.Y = (int)Math.Round((Height / 2f) - captionBounds.Height);
		}


		/// <override></override>
		protected override void InvalidateDrawCache() {
			base.InvalidateDrawCache();
			GdiHelpers.DisposeObject(ref _imageAttribs);
			GdiHelpers.DisposeObject(ref _imageBrush);
			_imageBounds = Geometry.InvalidRectangle;
		}


		/// <override></override>
		protected override void RecalcDrawCache() {
			base.RecalcDrawCache();
			_imageBounds.Width = Width;
			_imageBounds.Height = Height;
			_imageBounds.X = (int)Math.Round(-Width / 2f);
			_imageBounds.Y = (int)Math.Round(-Height / 2f);
			if (!string.IsNullOrEmpty(Text)) {
				Rectangle r;
				CalcCaptionBounds(0, out r);
				_imageBounds.Height -= r.Height;
			}
			_imageDrawBounds[0].X = _imageBounds.Left; _imageDrawBounds[0].Y = _imageBounds.Top;
			_imageDrawBounds[1].X = _imageBounds.Right; _imageDrawBounds[1].Y = _imageBounds.Top;
			_imageDrawBounds[2].X = _imageBounds.Right; _imageDrawBounds[2].Y = _imageBounds.Bottom;
			_imageDrawBounds[3].X = _imageBounds.Left; _imageDrawBounds[3].Y = _imageBounds.Bottom;

			if (_imageAttribs == null)
				_imageAttribs = GdiHelpers.GetImageAttributes(_imageLayout, _imageGamma, _imageTransparency, _imageGrayScale, _isPreview, _transparentColor);

			Image bitmapImg = null;
			if (_image == null || _image.Image == null)
				bitmapImg = Properties.Resources.DefaultBitmapLarge;
			else if (_image.Image is Bitmap)
				bitmapImg = Image.Image;
			if (bitmapImg is Bitmap && _imageBrush == null) {
				if (_isPreview)
					_imageBrush = GdiHelpers.CreateTextureBrush(bitmapImg, Width, Height, _imageAttribs);
				else _imageBrush = GdiHelpers.CreateTextureBrush(bitmapImg, _imageAttribs);
				// Transform texture Brush
				Point imageCenter = Point.Empty;
				imageCenter.Offset((int)Math.Round(_imageBounds.Width / 2f), (int)Math.Round(_imageBounds.Height / 2f));
				if (Angle != 0) imageCenter = Geometry.RotatePoint(Point.Empty, Geometry.TenthsOfDegreeToDegrees(Angle), imageCenter);
				GdiHelpers.TransformTextureBrush(_imageBrush, _imageLayout, _imageBounds, imageCenter, Geometry.TenthsOfDegreeToDegrees(Angle));
			}
		}


		/// <override></override>
		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);
			if (Geometry.IsValid(_imageBounds)) {
				_imageBounds.Offset(deltaX, deltaY);
				Matrix.TransformPoints(_imageDrawBounds);
				if (_imageBrush != null) {
					float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
					Point p = Point.Empty;
					p.X = (int)Math.Round(_imageBounds.X + (_imageBounds.Width / 2f));
					p.Y = (int)Math.Round(_imageBounds.Y + (_imageBounds.Height / 2f));
					p = Geometry.RotatePoint(Center, angleDeg, p);
					GdiHelpers.TransformTextureBrush(_imageBrush, _imageLayout, _imageBounds, p, angleDeg);
				}
			}
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);

				Rectangle shapeRect = Rectangle.Empty;
				shapeRect.Offset(left, top);
				shapeRect.Width = Width;
				shapeRect.Height = Height;

				Path.Reset();
				Path.StartFigure();
				Path.AddRectangle(shapeRect);
				Path.CloseFigure();
				return true;
			} else return false;
		}

		#endregion

		
		/// <override></override>
		protected override void ProcessExecModelPropertyChange(IModelMapping propertyMapping) {
			switch (propertyMapping.ShapePropertyId) {
				//case PropertyIdImage: break;
				case PropertyIdImageLayout:
					ImageLayout = (ImageLayoutMode)propertyMapping.GetInteger();
					break;
				case PropertyIdImageGrayScale:
					GrayScale = (propertyMapping.GetInteger() != 0);
					break;
				case PropertyIdImageGamma:
					//GammaCorrection = Math.Max(0.00000001f, Math.Abs(propertyMapping.GetFloat()));
					GammaCorrection = propertyMapping.GetFloat();
					break;
				case PropertyIdImageTransparency:
					checked {
						Transparency = (byte)propertyMapping.GetInteger();
					}
					break;
				case PropertyIdImageTransparentColor:
					TransparentColor = Color.FromArgb(propertyMapping.GetInteger());
					break;
				default:
					base.ProcessExecModelPropertyChange(propertyMapping);
					break;
			}
		}


		//private Image BrushImage {
		//   get {
		//      if (brushImage == null
		//         && !NamedImage.IsNullOrEmpty(image)
		//         && (image.Width >= 2 * Width || image.Height >= 2 * Height))
		//            brushImage = GdiHelpers.GetBrushImage(image.Image, Width, Height);
		//      return brushImage;
		//   }
		//}


		private void Construct() {
			// this fillStyle holds the image of the shape
			_image = null;
			_imageGrayScale = false;
			_compressionQuality = 100;
			_imageGamma = 1;
			_imageLayout = ImageLayoutMode.Fit;
			_imageTransparency = 0;
		}


		#region Fields

		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdImage = 9;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdImageLayout = 10;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdImageGrayScale = 11;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdImageGamma = 12;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdImageTransparency = 13;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdImageTransparentColor = 14;

		private ImageAttributes _imageAttribs = null;
		private TextureBrush _imageBrush = null;
		private Rectangle _imageBounds = Geometry.InvalidRectangle;
		private Point[] _imageDrawBounds = new Point[4];
		private bool _isPreview = false;
		private Size _fullImageSize = Size.Empty;
		private Size _currentImageSize = Size.Empty;
		private Image _brushImage;

		private NamedImage _image;
		private ImageLayoutMode _imageLayout = ImageLayoutMode.Fit;
		private byte _imageTransparency = 0;
		private float _imageGamma = 1.0f;
		private bool _imageGrayScale = false;
		private byte _compressionQuality = 100;
		private Color _transparentColor = Color.Empty;
		
		#endregion
	}


	/// <summary>
	/// Abstract base class for shapes that draw themselves using a bitmap or
	/// meta file.
	/// </summary>
	public class ImageBasedShape : ShapeBase, IPlanarShape, ICaptionedShape {

		/// <summary>
		/// Provides constants for the control point id's of the shape.
		/// </summary>
		public class ControlPointIds {
			/// <summary>ControlPointId of the top left control point.</summary>
			public const int TopLeftControlPoint = 1;
			/// <summary>ControlPointId of the center control point.</summary>
			public const int MiddleCenterControlPoint = 2;
			/// <summary>ControlPointId of the bottom center connection point.</summary>
			public const int BottomCenterConnectionPoint = 3;
			/// <summary>ControlPointId of the bottom center control point.</summary>
			public const int BottomCenterControlPoint = 8;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal ImageBasedShape(ShapeType shapeType, Template template,
			string resourceBaseName, Assembly resourceAssembly)
			: base(shapeType, template) {
			Construct(resourceBaseName, resourceAssembly);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal ImageBasedShape(ShapeType shapeType, IStyleSet styleSet,
			string resourceBaseName, Assembly resourceAssembly)
			: base(shapeType, styleSet) {
			Construct(resourceBaseName, resourceAssembly);
		}


		/// <override></override>
		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			_width = _image.Width;
			_height = _image.Height;
			Fit(0, 0, 100, 100);
			_charStyle = styleSet.CharacterStyles.Normal;
			_paragraphStyle = styleSet.ParagraphStyles.Title;
			_fillStyle = styleSet.FillStyles.Transparent;
		}


		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);

			if (source is IPlanarShape) {
				IPlanarShape src = (IPlanarShape)source;
				// Copy regular properties
				this._angle = src.Angle;
				// Copy templated properties
				this._fillStyle = (Template != null && src.FillStyle == ((IPlanarShape)Template.Shape).FillStyle) ? null : src.FillStyle;
			}
			if (source is ICaptionedShape) {
				// Copy as many captions as possible. Leave the rest untouched.
				int ownCaptionCnt = CaptionCount;
				int srcCaptionCnt = ((ICaptionedShape)source).CaptionCount;
				int cnt = Math.Min(ownCaptionCnt, srcCaptionCnt);
				for (int i = 0; i < cnt; ++i) {
					this.SetCaptionText(i, ((ICaptionedShape)source).GetCaptionText(i));
					this.SetCaptionCharacterStyle(i, ((ICaptionedShape)source).GetCaptionCharacterStyle(i));
					this.SetCaptionParagraphStyle(i, ((ICaptionedShape)source).GetCaptionParagraphStyle(i));
				}
			}
			if (source is ImageBasedShape) {
				_width = ((ImageBasedShape)source)._width;
				_height = ((ImageBasedShape)source)._height;
				if (((ImageBasedShape)source)._image != null)
					_image = (Image)((ImageBasedShape)source)._image.Clone();
			} else {
				Rectangle r = source.GetBoundingRectangle(true);
				Fit(r.X, r.Y, r.Width, r.Height);
			}
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new ImageBasedShape(Type, this.Template, _resourceName, _resourceAssembly);
			result.CopyFrom(this);
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryData()]
		[LocalizedDisplayName("PropName_ICaptionedShape_Text")]
		[LocalizedDescription("PropDesc_ICaptionedShape_Text")]
		[PropertyMappingId(PropertyIdText)]
		[RequiredPermission(Permission.Data)]
		[Editor("Dataweb.NShape.WinFormsUI.TextUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
		public string Text {
			get { return _caption.Text; }
			set { _caption.Text = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_ICaptionedShape_CharacterStyle")]
		[LocalizedDescription("PropDesc_ICaptionedShape_CharacterStyle")]
		[PropertyMappingId(PropertyIdCharacterStyle)]
		[RequiredPermission(Permission.Present)]
		public ICharacterStyle CharacterStyle {
			get { return _charStyle ?? ((ICaptionedShape)Template.Shape).GetCaptionCharacterStyle(0); }
			set {
				_charStyle = (Template != null && value == ((ICaptionedShape)Template.Shape).GetCaptionCharacterStyle(0)) ? null : value;
				_caption.InvalidatePath();
				Invalidate();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_ICaptionedShape_ParagraphStyle")]
		[LocalizedDescription("PropDesc_ICaptionedShape_ParagraphStyle")]
		[RequiredPermission(Permission.Present)]
		[PropertyMappingId(PropertyIdParagraphStyle)]
		public IParagraphStyle ParagraphStyle {
			get { return _paragraphStyle ?? ((ICaptionedShape)Template.Shape).GetCaptionParagraphStyle(0); }
			set {
				_paragraphStyle = (Template != null && value == ((ICaptionedShape)Template.Shape).GetCaptionParagraphStyle(0)) ? null : value;
				_caption.InvalidatePath();
				Invalidate();
			}
		}


		/// <override></override>
		public override void MakePreview(IStyleSet styleSet) {
			base.MakePreview(styleSet);
			_charStyle = styleSet.GetPreviewStyle(CharacterStyle);
			_paragraphStyle = styleSet.GetPreviewStyle(ParagraphStyle);
			_fillStyle = styleSet.GetPreviewStyle(FillStyle);
		}


		/// <override></override>
		public override bool HasStyle(IStyle style) {
			if (IsStyleAffected(CharacterStyle, style) || IsStyleAffected(ParagraphStyle, style))
				return true;
			else return base.HasStyle(style);
		}


		/// <override></override>
		public override RelativePosition CalculateRelativePosition(int x, int y) {
			if (!Geometry.IsValid(x, y)) throw new ArgumentOutOfRangeException("x / y");
			RelativePosition result = RelativePosition.Empty;
			result.A = (int)Math.Round(((x - X) / (float)this._width) * 1000);
			result.B = (int)Math.Round(((y - Y) / (float)this._height) * 1000);
			return result;
		}


		/// <override></override>
		public override Point CalculateAbsolutePosition(RelativePosition relativePosition) {
			if (relativePosition == RelativePosition.Empty) throw new ArgumentOutOfRangeException("relativePosition");
			Point result = Point.Empty;
			result.X = (int)Math.Round((relativePosition.A / 1000f) * _width) + X;
			result.Y = (int)Math.Round((relativePosition.B / 1000f) * _height) + Y;
			return result;
		}


		/// <override></override>
		public override Point CalculateNormalVector(int x, int y) {
			if (!ContainsPoint(x, y)) throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageFmt_Coordinates0AreOutside1, new Point(x, y), Type.FullName);
			return Geometry.CalcNormalVectorOfRectangle(x - (_width / 2), y - (_height / 2), x + (_width / 2), y + (_height / 2), x, y, 100);
		}


		/// <override></override>
		protected override Rectangle CalculateBoundingRectangle(bool tight) {
			// All controlPoints of the imagebased shape are inside the shape's bounds
			// so the tight fitting bounding rectangle equals the loose bounding rectangle
			Rectangle result = Rectangle.Empty;
			result.X = _x - _width / 2;
			result.Y = _y - _height / 2;
			result.Width = _width;
			result.Height = _height;
			ShapeUtils.InflateBoundingRectangle(ref result, LineStyle); 
			return result;
		}


		/// <override></override>
		public override int X {
			get { return _x; }
			set {
				int origValue = _x;
				if (!MoveTo(value, Y)) {
					MoveTo(origValue, Y);
					throw new InvalidOperationException(string.Format(Properties.Resources.MessageFmt_ShapeCannotMoveTo0, new Point(value, Y)));
				}
			}
		}


		/// <override></override>
		public override int Y {
			get { return _y; }
			set {
				int origValue = _y;
				if (!MoveTo(X, value)) {
					MoveTo(X, origValue);
					throw new InvalidOperationException(string.Format(Properties.Resources.MessageFmt_ShapeCannotMoveTo0, new Point(X, value)));
				}
			}
		}

		
		///// <override></override>
		//public override bool MoveByCore(int deltaX, int deltaY) {
		//    return true;
		//}


		/// <override></override>
		public override void Fit(int x, int y, int width, int height) {
			if (height - _ch < _minH) _ch = 0;
			if (width * _image.Height <= (height - _ch) * _image.Width) {
				// Höhe ist verhältnismäßig größer
				this._height = height;
				if (this._height < _ch + _minH) this._width = this._height * _image.Width / _image.Height;
				else this._width = (this._height - _ch) * _image.Width / _image.Height;
			} else {
				this._width = width;
				this._height = (this._width * _image.Height) / _image.Width + _ch;
			}
			this._x = (x + width) / 2;
			this._y = (y + height) / 2;
			_captionUpdated = false;
			InvalidateDrawCache();
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int fromX, int fromY) {
			Point result = Geometry.InvalidPoint;
			if (fromX != _x || fromY != _y)
				result = Geometry.IntersectLineWithRectangle(fromX, fromY, _x, _y, _x - _width / 2, _y - _height / 2, _x - _width / 2 + _width, _y - _height / 2 + _height);
			if (!Geometry.IsValid(result)) {
				result.X = _x; result.Y = _y;
			}
			return result;
		}


		/// <override></override>
		protected internal override int ControlPointCount {
			get { return 4; }
		}


		/// <override></override>
		public override Point GetControlPointPosition(ControlPointId controlPointId) {
			Point result = Point.Empty;
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
					// Links oben
					result.X = _x - _width / 2;
					result.Y = _y - _height / 2;
					break;
				case ControlPointId.Reference:
				case ControlPointIds.MiddleCenterControlPoint:
					// Referenzpunkt
					result.X = _x;
					result.Y = _y;
					break;
				case ControlPointIds.BottomCenterConnectionPoint:
				    result.X = _x;
				    result.Y = _y - _height / 2 + _height - _ch;
				    break;
				case ControlPointIds.BottomCenterControlPoint:
					result.X = _x;
					result.Y = _y - _height / 2 + _height;
					break;
				default:
					Debug.Fail("Unsupported control point id");
					break;
			}
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			bool result;
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
					result = (controlPointCapability & ControlPointCapabilities.Resize) != 0;
					break;
				case ControlPointId.Reference:
				case ControlPointIds.MiddleCenterControlPoint:
					result = ((controlPointCapability & ControlPointCapabilities.Reference) != 0
						|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId)));
					break;
				case ControlPointIds.BottomCenterConnectionPoint:
					result = (controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId);
					break;
				default:
					result = base.HasControlPointCapability(controlPointId, controlPointCapability);
					break;
			}
			return result;
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			UpdateDrawCache();
			if (_height >= _ch + _minH) {
				graphics.DrawImage(_image, _x - _width / 2, Y - _width / 2, _width, _height - _ch);
				if (_caption.IsVisible)
					_caption.Draw(graphics, CharacterStyle, ParagraphStyle);
			} else {
				graphics.DrawImage(_image, _x - _width / 2, Y - _width / 2, _width, _height);
			}
			base.Draw(graphics);
		}


		/// <override></override>
		public override void DrawOutline(Graphics graphics, Pen pen) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (pen == null) throw new ArgumentNullException("pen");
			UpdateDrawCache();
			base.DrawOutline(graphics, pen);
			graphics.DrawRectangle(pen, _x - _width / 2, _y - _height / 2, _width, _height);
		}


		/// <override></override>
		public override void Invalidate() {
			if (!SuspendingInvalidation) {
				if (DisplayService != null)
					DisplayService.Invalidate(_x - _width / 2, _y - _height / 2, _width, _height);
				base.Invalidate();
			}
		}


		#region IPlanarShape Members

		/// <ToBeCompleted></ToBeCompleted>
		[CategoryLayout()]
		[LocalizedDisplayName("PropName_IPlanarShape_Angle")]
		[LocalizedDescription("PropDesc_IPlanarShape_Angle")]
		[PropertyMappingId(PropertyIdAngle)]
		[RequiredPermission(Permission.Layout)]
		public int Angle {
			get { return _angle; }
			set { _angle = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_IPlanarShape_FillStyle")]
		[LocalizedDescription("PropDesc_IPlanarShape_FillStyle")]
		[PropertyMappingId(PropertyIdFillStyle)]
		[RequiredPermission(Permission.Present)]
		public virtual IFillStyle FillStyle {
			get { return _fillStyle ?? ((IPlanarShape)Template.Shape).FillStyle; }
			set {
				_fillStyle = (Template != null && value == ((IPlanarShape)Template.Shape).FillStyle) ? null : value;
				Invalidate();
			}
		}

		#endregion


		#region ICaptionedShape Members

		/// <override></override>
		public int CaptionCount {
			get { return 1; }
		}


		/// <override></override>
		public bool GetCaptionTextBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			Point location = Point.Empty;
			location.Offset(X, Y);
			Rectangle captionBounds = Rectangle.Empty;
			captionBounds.X = -_width / 2;
			captionBounds.Y = -_height / 2 + _height - _ch;
			captionBounds.Width = _width;
			captionBounds.Height = _ch;
			captionBounds = _caption.CalculateTextBounds(captionBounds, CharacterStyle, ParagraphStyle, DisplayService);
			Geometry.TransformRectangle(location, Angle, captionBounds, out topLeft, out topRight, out bottomRight, out bottomLeft);
			return true;
		}


		/// <override></override>
		public bool GetCaptionBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			Point location = Point.Empty;
			location.Offset(X, Y);
			Rectangle captionBounds = Rectangle.Empty;
			captionBounds.X = -_width / 2;
			captionBounds.Y = -_height / 2 + _height - _ch;
			captionBounds.Width = _width;
			captionBounds.Height = _ch;
			Geometry.TransformRectangle(location, Angle, captionBounds, out topLeft, out topRight, out bottomRight, out bottomLeft);
			return true;
		}


		/// <override></override>
		public Rectangle GetCaptionTextBounds(int index) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			Rectangle captionBounds = Rectangle.Empty;
			captionBounds.X = -_width / 2;
			captionBounds.Y = -_height / 2 + _height - _ch;
			captionBounds.Width = _width;
			captionBounds.Height = _ch;
			return _caption.CalculateTextBounds(captionBounds, CharacterStyle, ParagraphStyle, DisplayService);
		}


		/// <override></override>
		public Rectangle GetCaptionBounds(int index) {
			Rectangle result = Rectangle.Empty;
			result.X = -_width / 2;
			result.Y = -_height / 2 + _height - _ch;
			result.Width = _width;
			result.Height = _ch;
			return result;
		}


		/// <override></override>
		public string GetCaptionText(int index) {
			return _caption.Text;
		}


		/// <override></override>
		public ICharacterStyle GetCaptionCharacterStyle(int index) {
			return CharacterStyle;
		}


		/// <override></override>
		public IParagraphStyle GetCaptionParagraphStyle(int index) {
			return ParagraphStyle;
		}


		/// <override></override>
		public void SetCaptionText(int index, string text) {
			_caption.Text = text;
			this._captionUpdated = false;
		}


		/// <override></override>
		public void SetCaptionCharacterStyle(int index, ICharacterStyle characterStyle) {
			CharacterStyle = characterStyle;
		}


		/// <override></override>
		public void SetCaptionParagraphStyle(int index, IParagraphStyle paragraphStyle) {
			ParagraphStyle = paragraphStyle;
		}


		/// <override></override>
		public int FindCaptionFromPoint(int x, int y) {
			return Geometry.RectangleContainsPoint(this._x - this._width / 2, this._y - this._height / 2 + this._height - this._ch, this._width, this._ch, x, y) ? 0 : -1;
		}


		/// <override></override>
		public void ShowCaptionText(int index) {
			_caption.IsVisible = true;
			Invalidate();
		}


		/// <override></override>
		public void HideCaptionText(int index) {
			_caption.IsVisible = false;
			Invalidate();
		}


		#endregion


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.ImageBasedShape" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition epd in ShapeBase.GetPropertyDefinitions(version))
				yield return epd;
			yield return new EntityFieldDefinition("FillStyle", typeof(object));
			yield return new EntityFieldDefinition("CharacterStyle", typeof(object));
			yield return new EntityFieldDefinition("ParagraphStyle", typeof(object));
			yield return new EntityFieldDefinition("Text", typeof(string));
			yield return new EntityFieldDefinition("Width", typeof(int));
			yield return new EntityFieldDefinition("Height", typeof(int));
		}


		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			_fillStyle = reader.ReadFillStyle();
			_charStyle = reader.ReadCharacterStyle();
			_paragraphStyle = reader.ReadParagraphStyle();

			string txt = reader.ReadString();
			if (_caption == null) _caption = new Caption(txt);
			else _caption.Text = txt;
			_width = reader.ReadInt32();
			_height = reader.ReadInt32();
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteStyle(_fillStyle);
			writer.WriteStyle(_charStyle);
			writer.WriteStyle(ParagraphStyle);

			writer.WriteString(Text);
			writer.WriteInt32(_width);
			writer.WriteInt32(_height);
		}


		/// <override></override>
		protected override void DeleteCore(IRepositoryWriter writer, int version) {
			// Nothing to do
		}

		#endregion


		/// <override></override>
		protected override ControlPointId GetControlPointId(int index) {
			switch (index) {
				case 0: return ControlPointIds.TopLeftControlPoint;
				case 1: return ControlPointIds.MiddleCenterControlPoint;
				case 2: return ControlPointIds.BottomCenterConnectionPoint;
				case 3: return ControlPointIds.BottomCenterControlPoint;
				default: throw new NShapeException("Unsupported control point index.");
			}
		}


		/// <override></override>
		protected override bool ContainsPointCore(int x, int y) {
			return Geometry.RectangleContainsPoint(this._x - this._width / 2, this._y - this._height / 2, this._width, this._height, x, y);
		}


		/// <override></override>
		protected override bool IntersectsWithCore(int x, int y, int width, int height) {
			bool result = Geometry.RectangleIntersectsWithRectangle(this._x - this._width / 2, this._y - this._height / 2, this._width, this._height, x, y, width, height);
			return result;
		}


		/// <override></override>
		protected override bool MoveByCore(int deltaX, int deltaY) {
			//base.MoveByCore(deltaX, deltaY);
			this._x += deltaX;
			this._y += deltaY;
			_transformation.Reset();
			_transformation.Translate(deltaX, deltaY);
			_caption.TransformPath(_transformation);
			return true;
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
			bool result;
			if (pointId == ControlPointId.Reference || pointId == ControlPointIds.MiddleCenterControlPoint) 
				result = MoveByCore(deltaX, deltaY);
			else {
				int oldX = _x, oldY = _y, oldW = _width, oldH = _height;
				int newX, newY, newW, newH;
				switch (pointId) {
					case ControlPointIds.TopLeftControlPoint:
						newX = _x;
						newY = _y;
						newW = oldW - deltaX;
						newH = oldH - deltaY;
						result = (deltaX == deltaY);
						int dx, dy;
						if (!Geometry.MoveRectangleTopLeft(_width, _height, deltaX, deltaY, 1, 0, modifiers | ResizeModifiers.MaintainAspect, out dx, out dy, out newW, out newH))
							result = false;
						break;
					case ControlPointIds.BottomCenterControlPoint:
						newH = oldH + deltaY;
						if (newH < _minH) newH = _minH;
						newW = (newH - _ch) * _image.Width / _image.Height;
						if (newW < _minW) {
							newW = _minW;
							newH = newW * _image.Height / _image.Width + _ch;
						}
						newX = oldX;
						newY = oldY - oldH / 2 + newH / 2;
						result = false;
						break;
					default:
						newW = _width; newH = _height; newX = _x; newY = _y;
						result = true;
						break;
				}
				_x = newX; _y = newY; _width = newW; _height = newH;
				_captionUpdated = false;
			}
			return result;
		}


		/// <override></override>
		protected override bool RotateCore(int angle, int x, int y) {
			if (x != X || y != Y) {
				int toX = X;
				int toY = Y;
				Geometry.RotatePoint(x, y, angle, ref toX, ref toY);
				MoveTo(toX, toY);
			}
			return false;
		}


		/// <override></override>
		protected override void InvalidateDrawCache() {
			base.InvalidateDrawCache();
			_caption.InvalidatePath();
		}


		/// <override></override>
		protected override void RecalcDrawCache() {
			UpdateDrawCache();
		}


		/// <override></override>
		protected override void UpdateDrawCache() {
			if (DrawCacheIsInvalid) {
				_ch = CharacterStyle.Size + 4;
				_caption.CalculatePath(-_width / 2, -_height / 2 + -_ch, _width, _ch, CharacterStyle, ParagraphStyle);
				TransformDrawCache(_x, _y + _height, 0, _x, _y);
				_captionUpdated = true;
			}
		}


		/// <override></override>
		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			_transformation.Reset();
			_transformation.Translate(deltaX, deltaY);
			_caption.TransformPath(_transformation);
		}


		private void Construct(string resourceBaseName, Assembly resourceAssembly) {
			if (resourceBaseName == null) throw new ArgumentNullException("resourceBaseName");
			System.IO.Stream stream = resourceAssembly.GetManifestResourceStream(resourceBaseName);
			if (stream == null) throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_0IsNotAValidResourceIn1, resourceBaseName, resourceAssembly), "resourceBaseName");
			_image = Image.FromStream(stream);
			if (_image == null) throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_0IsNotAValidImageResource, resourceBaseName), "resourceBaseName");
			this._resourceName = resourceBaseName;
			this._resourceAssembly = resourceAssembly;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdAngle = 2;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdFillStyle = 3;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdText = 4;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdCharacterStyle = 5;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdParagraphStyle = 6;

		/// <ToBeCompleted></ToBeCompleted>
		protected const int _minW = 10;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int _minH = 10;

		/// <ToBeCompleted></ToBeCompleted>
		protected int _x, _y; // Position of reference point in the center
		/// <ToBeCompleted></ToBeCompleted>
		protected int _width, _height; // Size of shape
		/// <ToBeCompleted></ToBeCompleted>
		protected int _angle;
		/// <ToBeCompleted></ToBeCompleted>
		protected Image _image;
		//
		// these are needed for calling the constructor when cloning
		/// <ToBeCompleted></ToBeCompleted>
		protected string _resourceName;
		/// <ToBeCompleted></ToBeCompleted>
		protected Assembly _resourceAssembly;
		//
		// Caption fields
		/// <ToBeCompleted></ToBeCompleted>
		protected bool _captionUpdated = false;
		/// <ToBeCompleted></ToBeCompleted>
		protected int _ch; // Height of the caption
		/// <ToBeCompleted></ToBeCompleted>
		protected Caption _caption = new Caption(string.Empty);

		private ICharacterStyle _charStyle;
		private IParagraphStyle _paragraphStyle;
		private IFillStyle _fillStyle;
		private Matrix _transformation = new Matrix();
	}


	/// <summary>
	/// Provides a metafile whose color and line color can be customized.
	/// </summary>
	public class CustomizableMetaFile : ImageBasedShape {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new CustomizableMetaFile(Type, this.Template, _resourceName, _resourceAssembly);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			if (source is CustomizableMetaFile) {
				ReplaceColor = ((CustomizableMetaFile)source).ReplaceColor;
			}
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (!_captionUpdated) UpdateDrawCache();
			_replaceColorFound = false;
			Rectangle dstBounds = Rectangle.Empty;
			dstBounds.Offset((_x - _width / 2), (_y - _height / 2));
			dstBounds.Width = _width;
			dstBounds.Height = _height - _ch;

			//// Workaround: Create a buffer image to draw
			//if (bufferImage == null) bufferImage = CreateImage();
			//MetafileHeader header = bufferImage.GetMetafileHeader();
			//graphics.DrawImage(bufferImage, dstBounds, header.Bounds.X, header.Bounds.Y, header.Bounds.Width, header.Bounds.Height, GraphicsUnit.Pixel, imageAttribs);
			if (_imageAttribs == null) 
				UpdateImageAttributes();
			//MetafileHeader header = ((Metafile)image).GetMetafileHeader();
			//graphics.DrawImage(((Metafile)image), dstBounds, header.Bounds.X, header.Bounds.Y, header.Bounds.Width, header.Bounds.Height, GraphicsUnit.Pixel, imageAttribs);
			GdiHelpers.DrawImage(graphics, _image, _imageAttribs, ImageLayoutMode.Fit, dstBounds);
			
			if (_caption != null) _caption.Draw(graphics, CharacterStyle, ParagraphStyle);
		}


		/// <override></override>
		public override void MakePreview(IStyleSet styleSet) {
			base.MakePreview(styleSet);
			_isPreview = true;
			GdiHelpers.DisposeObject(ref _imageAttribs);
			UpdateImageAttributes();
		}


		/// <override></override>
		public override bool HasStyle(IStyle style) {
			if (IsStyleAffected(FillStyle, style))
				return true;
			else return base.HasStyle(style);
		}


		/// <override></override>
		public override IFillStyle FillStyle {
			get { return base.FillStyle; }
			set {
				_replaceColorFound = false;
				base.FillStyle = value;
				UpdateImageAttributes();
				Invalidate();
			}
		}


		/// <override></override>
		protected override void UpdateDrawCache() {
			base.UpdateDrawCache();
			UpdateImageAttributes();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal CustomizableMetaFile(ShapeType shapeType, Template template,
			string resourceBaseName, Assembly resourceAssembly)
			: base(shapeType, template, resourceBaseName, resourceAssembly) {
			Construct();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal CustomizableMetaFile(ShapeType shapeType, IStyleSet styleSet,
			string resourceBaseName, Assembly resourceAssembly)
			: base(shapeType, styleSet, resourceBaseName, resourceAssembly) {
			Construct();
		}


		/// <override></override>
		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Debug.Assert(_image is Metafile);
			_metafileDataSize = 20;
			_metafileData = new byte[_metafileDataSize];

			LineStyle = styleSet.LineStyles.Normal;
			FillStyle = styleSet.FillStyles.Red;
		}


		private void Construct() {
			//replaceBrushCallback = new Graphics.EnumerateMetafileProc(ReplaceBrushCallbackProc);
			_findReplaceColorCallback = new Graphics.EnumerateMetafileProc(FindReplaceColorCallbackProc);
			FindRemapColor();
		}


		// This does not work with x64/AnyCPU binaries on x64 systems any more due to breaking changes:
		// See https://connect.microsoft.com/VisualStudio/feedback/details/523540/metafile-playrecord-does-not-play-all-emf-records-as-expected
		[Obsolete]
		private bool ReplaceBrushCallbackProc(EmfPlusRecordType recordType, int flags, int dataSize, IntPtr data, PlayRecordCallback callbackData) {
			try {
				if (data == IntPtr.Zero)
					((Metafile)_image).PlayRecord(recordType, flags, 0, null);
				else {
					if (dataSize > _metafileDataSize) {
						_metafileDataSize = dataSize;
						_metafileData = new byte[_metafileDataSize];
					}
					// Copy the unmanaged record to a managed byte buffer that can be used by PlayRecord.
					Marshal.Copy(data, _metafileData, 0, dataSize);
					// Adjust the color
					switch (recordType) {
						case EmfPlusRecordType.EmfCreateBrushIndirect:
							// This type of record only appears in WMF and 'classic' EMF files, not in EMF+ files.
							// Get color of current brush
							if (!_replaceColorFound) {
								_metafileData[8] = FillStyle.BaseColorStyle.Color.R;
								_metafileData[9] = FillStyle.BaseColorStyle.Color.G;
								_metafileData[10] = FillStyle.BaseColorStyle.Color.B;
								_metafileData[11] = FillStyle.BaseColorStyle.Color.A;
								_replaceColorFound = true;
							}
							break;
						default: /* nothing to do */ break;
					}
					((Metafile)_image).PlayRecord(recordType, flags, dataSize, _metafileData);
				}
			} catch (Exception exc) {
				Console.WriteLine("Error while playing metafile record: {0}", exc.Message);
			}
			return true;
		}


		private bool FindReplaceColorCallbackProc(EmfPlusRecordType recordType, int flags, int dataSize, IntPtr data, PlayRecordCallback callbackData) {
			switch (recordType) {
				case EmfPlusRecordType.EmfCreateBrushIndirect:
					if (!_replaceColorFound) {
						// Copy the unmanaged record to a managed byte buffer that can be used by PlayRecord.
						if (dataSize > _metafileDataSize) {
							_metafileDataSize = dataSize;
							_metafileData = new byte[_metafileDataSize];
						}
						Marshal.Copy(data, _metafileData, 0, dataSize);
						// Get the RGB color components of the brush
						_replaceColor = Color.FromArgb(_metafileData[8], _metafileData[9], _metafileData[10]);
						_replaceColorFound = true;
					}
					break;
				
				default:
					// Nothing to do
					break;
			}
			// No need to play the record as we only search a color to replace
			//((Metafile)image).PlayRecord(recordType, flags, dataSize, dataArray);    //note that every record gets played even if the dataArray is empty
			return true;
		}


		// This is the official sample code from MSDN documentation. Does not work on x64 systems. See comment on CreateImage()
		private bool MetafileCallback(EmfPlusRecordType recordType, int flags, int dataSize, IntPtr data, PlayRecordCallback callbackData) {
			byte[] dataArray = null;
			if (data != IntPtr.Zero) {
				// Copy the unmanaged record to a managed byte buffer 
				// that can be used by PlayRecord.
				dataArray = new byte[dataSize];
				Marshal.Copy(data, dataArray, 0, dataSize);
			}
			((Metafile)_image).PlayRecord(recordType, flags, dataSize, dataArray);
			return true;
		}


		// Due to a bug in the .NET wrapper of GDI+, records using CreateBrushIndirect are not played on x64 systems.
		// On x86 systems, these records can be played if the record's buffer is enlarged by 8 bytes but we don't
		// want to rely on this.
		// So we use EnumerateMetafile for searching a color to replace and do the color replacement via the color 
		// remapping functionality of the ImageAttributes class.
		private Metafile CreateImage() {
			// Create MetaFile and graphics context
			Metafile metaFile = null;
			System.IO.MemoryStream stream = new System.IO.MemoryStream();
			using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero)) {
				IntPtr hdc = gfx.GetHdc();
				try {
					MetafileHeader header = ((Metafile)_image).GetMetafileHeader();
					EmfType emfType;
					switch (header.Type) {
						case MetafileType.EmfPlusDual: emfType = EmfType.EmfPlusDual; break;
						case MetafileType.EmfPlusOnly: emfType = EmfType.EmfPlusOnly; break;
						default: emfType = EmfType.EmfOnly; break;
					}
					metaFile = new Metafile(stream, hdc, header.Bounds, MetafileFrameUnit.Pixel,
						emfType, "CustomizableMetafile Buffer Image");
				} finally {
					gfx.ReleaseHdc(hdc);
				}
			}

			UpdateImageAttributes();
			using (Graphics gfx = Graphics.FromImage(metaFile)) {
				GdiHelpers.ApplyGraphicsSettings(gfx, RenderingQuality.HighQuality);
				MetafileHeader header = ((Metafile)_image).GetMetafileHeader();
				GdiHelpers.DrawImage(gfx, _image, _imageAttribs, ImageLayoutMode.Fit, header.Bounds);
				// Deactivated, see comment above
				//gfx.EnumerateMetafile((Metafile)image, header.Bounds, header.Bounds, GraphicsUnit.Pixel, replaceBrushCallback, IntPtr.Zero, imageAttribs);
			}
			if (stream != null) {
				stream.Dispose();
				stream = null;
			}
			return metaFile;
		}


		private void FindRemapColor() {
			using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero)) {
				MetafileHeader header = ((Metafile)_image).GetMetafileHeader();
				gfx.EnumerateMetafile((Metafile)_image, header.Bounds, header.Bounds, GraphicsUnit.Pixel, _findReplaceColorCallback, IntPtr.Zero);
			}
		}


		private void UpdateImageAttributes() {
			GdiHelpers.DisposeObject(ref _imageAttribs);
			_imageAttribs = GdiHelpers.GetImageAttributes(ImageLayoutMode.Fit, _isPreview);
			_imageAttribs.ClearRemapTable();

			// As long as we cannot set/unset the color (available in next version), we map every color...
			//if (ReplaceColor != Color.Empty) {
				_colorReplaceMap.OldColor = ReplaceColor;
				_colorReplaceMap.NewColor = FillStyle.BaseColorStyle.Color;
				ColorMap[] colorMap = { _colorReplaceMap };
				_imageAttribs.SetRemapTable(colorMap);
			//}
		}


		/// <summary>
		/// Specifies the color of the meta file to replace with the base color of the specified fill style.
		/// </summary>
		private Color ReplaceColor {
			get { return (_privateReplaceColor != Color.Empty) ? _privateReplaceColor : _replaceColor; }
			set {
				_privateReplaceColor = value;
				UpdateImageAttributes();
				Invalidate();
			}
		}


		private Graphics.EnumerateMetafileProc _findReplaceColorCallback;
		private bool _replaceColorFound;
		private Color _privateReplaceColor = Color.Empty;
		private Color _replaceColor = Color.Empty;
		private ColorMap _colorReplaceMap = new ColorMap();

		// Buffer for metafile record's data
		private byte[] _metafileData = null; // Data buffer for meta file drawing
		private int _metafileDataSize = 0; // Allocated size for meta file data

		private bool _isPreview = false;
		private ImageAttributes _imageAttribs;
	}

}
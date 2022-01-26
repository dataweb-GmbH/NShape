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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices;
using Dataweb.NShape.Advanced;


namespace Dataweb.NShape {

	#region ***   Enums, Structs and Delegates   ***

	/// <summary>
	/// Specifies the category of a style.
	/// </summary>
	public enum StyleCategory {
		/// <summary>Specifies a line cap style.</summary>
		CapStyle,
		/// <summary>Specifies a character style.</summary>
		CharacterStyle,
		/// <summary>Specifies a color style.</summary>
		ColorStyle,
		/// <summary>Specifies a fill style.</summary>
		FillStyle,
		/// <summary>Specifies a line style.</summary>
		LineStyle,
		/// <summary>Specifies a paragraph style.</summary>
		ParagraphStyle
	}


	/// <summary>
	/// Specifies the layout of an image inside its display bounds.
	/// </summary>
	public enum ImageLayoutMode { 
		/// <summary>The image is displayed unscaled and aligned to the upper left corner.</summary>
		Original, 
		/// <summary>The image is displayed unscaled and centered.</summary>
		Center,
		/// <summary>The image is stretched to the display bounds.</summary>
		Stretch, 
		/// <summary>The image is fitted into the display bounds maintaining the aspect ratio of the image.</summary>
		Fit, 
		/// <summary>The image is displayed as tiles.</summary>
		Tile, 
		/// <summary>The image is displayed as tiles where one tile will be centered.</summary>
		CenterTile, 
		/// <summary>The image is displayed as tiles. Tiles are flipped so the tile bounds match each other.</summary>
		FlipTile 
	}


	/// <summary>
	/// Specifies the fill mode of a <see cref="T:Dataweb.NShape.IFillStyle" />.
	/// </summary>
	public enum FillMode {
		/// <summary>The area is filled with a color.</summary>
		Solid, 
		/// <summary>The area is filled with a color gradient.</summary>
		Gradient, 
		/// <summary>The area is filled with a pattern.</summary>
		Pattern, 
		/// <summary>The area is filled with an image.</summary>
		Image 
	}


	/// <summary>
	/// Specifies the shape of a line cap.
	/// </summary>
	public enum CapShape { 
		/// <summary>No line cap (equals 'Round').</summary>
		None, 
		/// <summary>A triangle shaped arrow cap.</summary>
		ClosedArrow, 
		/// <summary>A V-shaped arrow cap.</summary>
		OpenArrow, 
		/// <summary>A circular line cap.</summary>
		Circle, 
		/// <summary>A triangle shaped cap. The triangle's base line is located at the line's cap.</summary>
		Triangle, 
		/// <summary>A rhombical line cap.</summary>
		Diamond, 
		/// <summary>A quadratic line cap.</summary>
		Square, 
		/// <summary>A circular line cap. The circle's center is located at the line's cap.</summary>
		CenteredCircle, 
		/// <summary>A half circle shapes line cap. The circle's center is located at the line's cap.</summary>
		CenteredHalfCircle,
		/// <summary>A rounded line cap.</summary>
		Round,
		/// <summary>A flattened line cap.</summary>
		Flat,
		/// <summary>A peaked line cap</summary>
		Peak
	}


	/// <summary>
	/// Specifies the dashes of a line.
	/// </summary>
	public enum DashType { 
		/// <summary>Specifies a solid line.</summary>
		Solid,
		/// <summary>Specifies a line consisting of dashes.</summary>
		Dash,
		/// <summary>Specifies a line consisting of dots.</summary>
		Dot,
		/// <summary>Specifies a line consisting of a repeating pattern of dash-dot.</summary>
		DashDot,
		/// <summary>Specifies a line consisting of a repeating pattern of dash-dot-dot.</summary>
		DashDotDot 
	}


	/// <summary>
	/// Represents padding or margin information of a layouted text.
	/// </summary>
	[TypeConverter("Dataweb.NShape.WinFormsUI.TextPaddingTypeConverter, Dataweb.NShape.WinFormsUI")]
	[Serializable, StructLayout(LayoutKind.Sequential)]
	public struct TextPadding : IEquatable<TextPadding> {

		/// <summary>
		/// Provides a <see cref="T:Dataweb.NShape.TextPadding" /> object with no padding.
		/// </summary>
		public static readonly TextPadding Empty;


		/// <summary>
		/// Tests whether two specified <see cref="T:Dataweb.NShape.TextPadding" /> objects are equivalent.
		/// </summary>
		public static bool operator ==(TextPadding a, TextPadding b) {
			return (a.Left == b.Left
				&& a.Top == b.Top
				&& a.Right == b.Right
				&& a.Bottom == b.Bottom);
		}


		/// <summary>
		/// Tests whether two specified <see cref="T:Dataweb.NShape.TextPadding" /> objects are not equivalent.
		/// </summary>
		public static bool operator !=(TextPadding a, TextPadding b) {
			return !(a == b);
		}


		/// <summary>
		/// Generates a hash code for the current <see cref="T:Dataweb.NShape.TextPadding" />.
		/// </summary>
		public override int GetHashCode() {
			return base.GetHashCode();
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.TextPadding" />.
		/// </summary>
		public TextPadding(int all) {
			if (all < 0) throw new ArgumentOutOfRangeException("all");
			this._all = true;
			this._left = this._top = this._right = this._bottom = all;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.TextPadding" />.
		/// </summary>
		public TextPadding(int left, int top, int right, int bottom) {
			if (left < 0) throw new ArgumentOutOfRangeException("left");
			if (top < 0) throw new ArgumentOutOfRangeException("top");
			if (right < 0) throw new ArgumentOutOfRangeException("right");
			if (bottom < 0) throw new ArgumentOutOfRangeException("bottom");
			this._all = false;
			this._left = left;
			this._top = top;
			this._right = right;
			this._bottom = bottom;
			CheckAll();
		}


		/// <summary>
		/// Gets or sets the padding value for the left edge.
		/// </summary>
		[RefreshProperties(RefreshProperties.All)]
		public int Left {
			get { return _left; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("value");
				if (_all || _left != value) {
					_left = value;
					CheckAll();
				}
			}
		}


		/// <summary>
		/// Gets or sets the padding value for the top edge.
		/// </summary>
		[RefreshProperties(RefreshProperties.All)]
		public int Top {
			get { return _all ? _left : _top; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("value");
				if (_all || _top != value) {
					_top = value;
					CheckAll();
				}
			}
		}


		/// <summary>
		/// Gets or sets the padding value for the right edge.
		/// </summary>
		[RefreshProperties(RefreshProperties.All)]
		public int Right {
			get { return _all ? _left : _right; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("value");
				if (_all || _right != value) {
					_right = value;
					CheckAll();
				}
			}
		}


		/// <summary>
		/// Gets or sets the padding value for the bottom edge.
		/// </summary>
		[RefreshProperties(RefreshProperties.All)]
		public int Bottom {
			get { return _all ? _left : _bottom; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("value");
				if (_all || _bottom != value) {
					_bottom = value;
					CheckAll();
				}
			}
		}


		/// <summary>
		/// Gets or sets the padding value for all edges.
		/// </summary>
		[RefreshProperties(RefreshProperties.All)]
		public int All {
			get { return _all ? _left : -1; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("value");
				_left = _top = _right = _bottom = value;
				CheckAll();
			}
		}


		/// <summary>
		/// Gets the combined padding for the right and left edges.
		/// </summary>
		[Browsable(false)]
		public int Horizontal {
			get { return _left + _right; }
		}


		/// <summary>
		/// Gets the combined padding for the top and bottom edges.
		/// </summary>
		[Browsable(false)]
		public int Vertical {
			get { return _top + _bottom; }
		}


		/// <override></override>
		public override bool Equals(object obj) {
			return (obj is TextPadding && ((TextPadding)obj) == this);
		}


		/// <override></override>
		public bool Equals(TextPadding other) {
			return other == this;
		}

		
		static TextPadding() {
			Empty._left =
			Empty._top =
			Empty._right =
			Empty._bottom = 0;
			Empty._all = true;
		}


		private void CheckAll() {
			_all = (_left == _top && _left == _right && _left == _bottom);
		}


		private int _left, _top, _right, _bottom;
		private bool _all;
	}


	/// <summary>
	/// Returns the style of the same type with the same name if there is one in the design's style collection.
	/// </summary>
	public delegate IStyle FindStyleCallback(IStyle style);


	/// <ToBeCompleted></ToBeCompleted>
	public delegate Style CreatePreviewStyleCallback(IStyle style);

	#endregion


	#region ***   Type Description Classes   ***

	/// <summary>
	/// A type description provider that calls type converters and/or ui type editors registered with the TypeDescriptorRegistrar.
	/// </summary>
	public class StyleTypeDescriptionProvider : TypeDescriptionProvider {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.StyleTypeDescriptionProvider" />.
		/// </summary>
		public StyleTypeDescriptionProvider()
			: base(TypeDescriptor.GetProvider(typeof(Style))) {
		}


		/// <override></override>
		public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance) {
			return new StyleTypeDescriptor(base.GetTypeDescriptor(objectType, instance));
		}

	}


	/// <ToBeCompleted></ToBeCompleted>
	public class StyleTypeDescriptor : CustomTypeDescriptor {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.StyleTypeDescriptor" />.
		/// </summary>
		public StyleTypeDescriptor(ICustomTypeDescriptor parent)
			: base(parent) {
		}


		/// <override></override>
		public override object GetEditor(Type editorBaseType) {
			return TypeDescriptorRegistrar.GetRegisteredUITypeEditor(editorBaseType) ?? base.GetEditor(editorBaseType);
		}


		/// <override></override>
		public override TypeConverter GetConverter() {
			return TypeDescriptorRegistrar.GetRegisteredTypeConverter(typeof(IStyle)) ?? base.GetConverter();
		}

	}

	#endregion


	#region ***   Style Interfaces   ***

	/// <summary>
	/// Provides read-only access to a styles that define the appearance of shapes.
	/// </summary>
	[TypeConverter("Dataweb.NShape.WinFormsUI.StyleTypeConverter, Dataweb.NShape.WinFormsUI")]
	public interface IStyle : IEntity, IDisposable {
		
		/// <summary>Specifies the culture independent name of the style. Used for identifying it within a style collection. Has to be unique inside a style collection.</summary>
		[Browsable(false)]
		string Name { get; }
		
		/// <summary>Specifies the culture dependent title of the style.</summary>
		string Title { get; }
		
		/// <override></override>
		string ToString();
	}


	/// <summary>
	/// Provides read-only access to a style that defines the appearance of line caps.
	/// </summary>
	[Editor("Dataweb.NShape.WinFormsUI.StyleUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface ICapStyle : IStyle {
		
		/// <summary>Specifies the shape of the line cap.</summary>
		CapShape CapShape { get; }

		/// <summary>Specifies the diameter of the cap.</summary>
		short CapSize { get; }
		
		/// <summary>Specifies the interior fill color of the cap.</summary>
		IColorStyle ColorStyle { get; }

	}


	/// <summary>
	/// Provides read-only access to a style that defines the appearance of text.
	/// </summary>
	[Editor("Dataweb.NShape.WinFormsUI.StyleUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface ICharacterStyle : IStyle {

		/// <summary>Specifies the name of the font family.</summary>
		string FontName { get; }

		/// <summary>Specifies the <see cref="T:System.Drawing.FontFamily" />.</summary>
		FontFamily FontFamily { get; }

		/// <summary>Specifies the size of the font in points.</summary>
		float SizeInPoints { get; }

		/// <summary>Specifies the size of the font in world coordinates.</summary>
		int Size { get; }

		/// <summary>Specifies the style of the text's characters, such as bold or italic.</summary>
		FontStyle Style { get; }

		/// <summary>Specifies the text color.</summary>
		IColorStyle ColorStyle { get; }
	}


	/// <summary>
	/// Provides read-only access to a style that defines colors.
	/// </summary>
	[Editor("Dataweb.NShape.WinFormsUI.StyleUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface IColorStyle : IStyle {

		/// <summary>Specifies the color value of the color.</summary>
		Color Color { get; }
		
		/// <summary>Specifies transparency in percentage. Valid value range is 0 to 100.</summary>
		byte Transparency { get; }
		
		/// <summary>Specifies if the color value should be converted to a gray scale value.</summary>
		bool ConvertToGray { get; }
	}


	/// <summary>
	/// Provides read-only access to a style that defines the filling of a shape.
	/// </summary>
	[Editor("Dataweb.NShape.WinFormsUI.StyleUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface IFillStyle : IStyle {

		/// <summary>
		/// Specifies the base color of the fill style. 
		/// Depending on the FillMode this means 
		/// the fill color (<see cref="E:Dataweb.NShape.FillMode.Solid" />), 
		/// the color of a gradient's lower right (<see cref="E:Dataweb.NShape.FillMode.Gradient" />) or 
		/// a pattern's background color (<see cref="E:Dataweb.NShape.FillMode.Pattern" />).
		/// </summary>
		IColorStyle BaseColorStyle { get; }

		/// <summary>
		/// Specifies the additional color of the fill style. 
		/// Depending on the FillMode this means the color of the gradient's upper left or the fore color of a pattern.
		/// </summary>
		IColorStyle AdditionalColorStyle { get; }

		/// <summary>
		/// Specifies the fill mode.
		/// </summary>
		FillMode FillMode { get; }

		/// <summary>
		/// Specifies the fill pattern, applies only when FillMode is set to FillMode.Pattern.
		/// </summary>
		HatchStyle FillPattern { get; }

		/// <summary>
		/// The angle of the color gradient in degrees. 45 means from upper left to lower right.
		/// </summary>
		short GradientAngle { get; }

		/// <summary>
		/// Specifies if the colors or the image should be converted into grayscale.
		/// </summary>
		bool ConvertToGrayScale { get; }

		/// <summary>
		/// Defines the image of a fill style. Only applies when FillMode is set to <see cref="E:Dataweb.NShape.FillMode.Image" />.
		/// </summary>
		NamedImage Image { get; }

		/// <summary>
		/// Defines the layout of the image inside its bounds. Only applies when FillMode is set to <see cref="E:Dataweb.NShape.FillMode.Image" />.
		/// </summary>
		ImageLayoutMode ImageLayout { get; }

		/// <summary>
		/// Defines the transparency of the image. Only applies when FillMode is set to <see cref="E:Dataweb.NShape.FillMode.Image" />.
		/// </summary>
		byte ImageTransparency { get; }

		/// <summary>
		/// Defines the gamma correction of the image. Only applies when FillMode is set to <see cref="E:Dataweb.NShape.FillMode.Image" />.
		/// </summary>
		float ImageGammaCorrection { get; }
	}


	/// <summary>
	/// Provides read-only access to a style that the appearance of lines and outlines.
	/// </summary>
	[Editor("Dataweb.NShape.WinFormsUI.StyleUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface ILineStyle : IStyle {

		/// <summary>Specifies the thickness of the line in display units.</summary>
		int LineWidth { get; }

		/// <summary>Specifies the color of the line and the line cap's outline.</summary>
		IColorStyle ColorStyle { get; }

		/// <summary>Specifies whether the line is dashed.</summary>
		DashType DashType { get; }

		/// <summary>Specifies the shape of the dashes' ends.</summary>
		DashCap DashCap { get; }

		/// <summary>Specifies the pattern of the dashes/dots (in display units).</summary>
		float[] DashPattern { get; }

		/// <summary>Specifies the shape of the line's corners.</summary>
		LineJoin LineJoin { get; }
	}


	/// <summary>
	/// Provides read-only access to a style that defines the layout of text.
	/// </summary>
	[Editor("Dataweb.NShape.WinFormsUI.StyleUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
	public interface IParagraphStyle : IStyle {

		/// <summary>Specifies the alignment of the text inside a given area.</summary>
		ContentAlignment Alignment { get; }

		/// <summary>Specifies whether and how text should be truncated if the text's layout area is too small.</summary>
		StringTrimming Trimming { get; }

		/// <summary>Specifies the margin between the layout area's bounds and the text.</summary>
		TextPadding Padding { get; }

		/// <summary>Specifies whether text should be wrapped automatically if the text does not fit into the layout area.</summary>
		bool WordWrap { get; }

	}

	#endregion


	#region ***   StandardStyle Definitions   ***

	/// <summary>
	/// Base class for StandardStyleNames. 
	/// Implements all methods. Derived classes only have to define 
	/// public readonly string fields named like the standard style name.
	/// </summary>
	public abstract class StandardStyleNames {

		/// <summary>
		/// Base constructor of all derived StandardStyleNames.
		/// Initializes all public readonly string fields with their field names
		/// and creates the names string array.
		/// </summary>
		protected StandardStyleNames() {
			FieldInfo[] fieldInfos = this.GetType().GetFields();
			Array.Resize<string>(ref _names, fieldInfos.Length);
			int idx = -1;
			foreach (FieldInfo fieldInfo in fieldInfos) {
				if (fieldInfo.IsInitOnly && fieldInfo.IsPublic &&
					fieldInfo.FieldType == typeof(string)) {
					_names[++idx] = fieldInfo.Name;
					fieldInfo.SetValue(this, fieldInfo.Name);
				} else { }
			}
			if (idx + 1 < _names.Length) Array.Resize<string>(ref _names, idx + 1);
		}


		/// <summary>
		/// Provides index based access to the items of the collection.
		/// </summary>
		public string this[int index] {
			get {
				if (index >= _names.Length) throw new IndexOutOfRangeException("index");
				return _names[index];
			}
		}


		/// <summary>
		/// Specifies the number of items in the collection.
		/// </summary>
		public int Count {
			get { return _names.Length; }
		}


		/// <summary>
		/// Returns true if the given name equals any of the items in the collection.
		/// </summary>
		public bool EqualsAny(string name) {
			Debug.Assert(_names != null);
			if (name == Style.EmptyStyleName) return true;
			foreach (string n in _names)
				if (n.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					return true;
			return false;
		}


		private string[] _names;
	}


	/// <summary>
	/// Defines the standard cap style names.
	/// </summary>
	public sealed class StandardCapStyleNames : StandardStyleNames {
		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("This name will be removed in future versions. Use ArrowClosed instead.")]
		public readonly string Arrow;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string ArrowClosed;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string ArrowOpen;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string None;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Special1;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Special2;
	}


	/// <summary>
	/// Defines the standard character style names.
	/// </summary>
	public sealed class StandardCharacterStyleNames : StandardStyleNames {
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Caption;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Heading1;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Heading2;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Heading3;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Normal;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Subtitle;
	}


	/// <summary>
	/// Defines the standard color style names.
	/// </summary>
	public sealed class StandardColorStyleNames : StandardStyleNames {
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Background;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Black;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Blue;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Gray;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Green;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Highlight;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string HighlightText;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string LightBlue;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string LightGray;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string LightGreen;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string LightRed;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string LightYellow;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Red;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Text;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Transparent;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string White;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Yellow;
	}


	/// <summary>
	/// Defines the standard fill style names.
	/// </summary>
	public sealed class StandardFillStyleNames : StandardStyleNames {
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Black;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Blue;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Green;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Red;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Transparent;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string White;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Yellow;
	}


	/// <summary>
	/// Defines the standard line style names.
	/// </summary>
	public sealed class StandardLineStyleNames : StandardStyleNames {
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Blue;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Dashed;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Dotted;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Green;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Highlight;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string HighlightDashed;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string HighlightDotted;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string HighlightThick;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string None;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Normal;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Red;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Special1;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Special2;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Thick;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Yellow;
	}


	/// <summary>
	/// Defines the standard paragraph style names.
	/// </summary>
	public sealed class StandardParagraphStyleNames : StandardStyleNames {
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Label;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Text;
		/// <ToBeCompleted></ToBeCompleted>
		public readonly string Title;
	}

	#endregion


	#region ***   Style Classes   ***

	/// <summary>
	/// Provides a base class for <see cref="T:Dataweb.NShape.IStyle" /> implementations.
	/// </summary>
	[TypeDescriptionProvider(typeof(TypeDescriptionProviderDg))]
	public abstract class Style : IStyle, IEquatable<IStyle>, IEquatable<Style>, INotifyPropertyChanged {

		/// <override></override>
		[CategoryGeneral()]
		[LocalizedDisplayName("PropName_Style_Name")]
		[LocalizedDescription("PropDesc_Style_Name")]
		[RequiredPermission(Permission.Designs)]
		public string Name {
			get { return _name; }
			set {
				if (string.IsNullOrEmpty(value) || value == EmptyStyleName) throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_0IsNotAValidStyleName, value));
				if (!_renameable) throw new InvalidOperationException(Properties.Resources.MessageTxt_StandardStylesMustNotBeRenamed);
				else if (IsStandardName(value)) throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_0IsAStandardStyleName, value));
				if (_name != value) {
					_name = value;
					OnPropertyChanged("Name");
				}
			}
		}


		/// <override></override>
		[CategoryGeneral()]
		[LocalizedDisplayName("PropName_Style_Title")]
		[LocalizedDescription("PropDesc_Style_Title")]
		[RequiredPermission(Permission.Designs)]
		public string Title {
			get { return string.IsNullOrEmpty(_title) ? _name : _title; }
			set {
				if (value == _name || string.IsNullOrEmpty(value))
					_title = null;
				else _title = value;
				OnPropertyChanged("Title");
			}
		}


		/// <summary>
		/// Copies all properties from the given <see cref="T:Dataweb.NShape.IStyle" />.
		/// </summary>
		/// <param name="style">Specifies the copy source.</param>
		/// <param name="findStyleCallback">A callback method that obtains a style.</param>
		public virtual void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style == null) throw new ArgumentNullException("style");
			if (findStyleCallback == null) throw new ArgumentNullException("findStyleCallback");
			if (this.Name != style.Name) this.Name = style.Name;
			this.Title = style.Title;
		}


		/// <override></override>
		public override string ToString() {
			return Title;
		}


		/// <override></override>
		public bool Equals(IStyle other) {
			return other == this;
		}


		/// <override></override>
		public bool Equals(Style other) {
			return other == this;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Style" />.
		/// </summary>
		protected Style()
			: this(string.Empty) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Style" />.
		/// </summary>
		protected Style(string name) {
			this._renameable = !IsStandardName(name);
			this._name = name;
			this._title = string.Empty;
		}


		/// <summary>
		/// Tests if the given name is a standard style name.
		/// </summary>
		protected abstract bool IsStandardName(string name);


		/// <summary>
		/// Fires an INotityPropertyChanged.PropertyCHanged event.
		/// </summary>
		protected void OnPropertyChanged(string propertyName) {
			if (_propertyChangedHandler != null) 
				_propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
		}

		
		/// <summary>
		/// Defines the name of the style meaning 'None' / 'Not Set' / 'Not Initialized'.
		/// </summary>
		protected internal const string EmptyStyleName = "0";

		
		/// <summary>
		/// Defines the title of the style meaning 'None' / 'Not Set' / 'Not Initialized'.
		/// </summary>
		protected internal const string EmptyStyleTitle = "None";


		#region IDisposable Members

		/// <override></override>
		public abstract void Dispose();

		#endregion


		#region INotifyPropertyChanged Members
		
		/// <override></override>
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
			add { _propertyChangedHandler += value; }
			remove { _propertyChangedHandler -= value; }
		}

		#endregion


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Style" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
			if (version >= 3) yield return new EntityFieldDefinition("Title", typeof(string));
		}


		/// <override></override>
		[Browsable(false)]
		public virtual object Id {
			get { return _id; }
		}


		/// <override></override>
		public virtual void AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this._id != null) {
				//throw new InvalidOperationException("Style has already an id.");
			}else 
			this._id = id;
		}


		/// <override></override>
		public virtual void LoadFields(IRepositoryReader reader, int version) {
			if (reader == null) throw new ArgumentNullException("reader");
			_name = reader.ReadString();
			if (version >= 3) _title = reader.ReadString();
			_renameable = !IsStandardName(_name);
		}


		/// <override></override>
		public virtual void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (reader == null) throw new ArgumentNullException("reader");
			// nothing to do
		}


		/// <override></override>
		public virtual void SaveFields(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			writer.WriteString(_name);
			if (version >= 3) writer.WriteString(_title);
		}


		/// <override></override>
		public virtual void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (writer == null) throw new ArgumentNullException("writer");
			// nothing to do
		}


		/// <override></override>
		public virtual void Delete(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}

		#endregion


		/// <summary>
		/// Finalizer of <see cref="T:Dataweb.NShape.Style" />.
		/// </summary>
		~Style() {
			Dispose();
		}


		#region Fields

		private object _id = null;
		private string _name = null;
		private string _title = null;
		private bool _renameable = true;
		private PropertyChangedEventHandler _propertyChangedHandler;

		#endregion

	}


	/// <summary>
	/// Provides the definition of line cap.
	/// </summary>
	public sealed class CapStyle : Style, ICapStyle, IEquatable<ICapStyle>, IEquatable<CapStyle> {

		/// <summary>
		/// This static read-only field represents a default and not initialized cap style.
		/// </summary>
		public static readonly CapStyle Default;


		/// <summary>
		/// Provides the <see cref="T:Dataweb.NShape.StandardCapStyleNames" />.
		/// </summary>
		public static StandardCapStyleNames StandardNames;


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.CapStyle" />.
		/// </summary>
		public CapStyle()
			: base() { }


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.CapStyle" />.
		/// </summary>
		public CapStyle(string name)
			: base(name) { }


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.CapStyle" />.
		/// </summary>
		public CapStyle(string name, CapShape capShape, IColorStyle colorStyle)
			: base(name) {
			this.CapShape = capShape;
			this.ColorStyle = colorStyle;
		}


		#region IDisposable Members

		/// <override></override>
		public override void Dispose() {
			if (_colorStyle != null) {
				_colorStyle.Dispose();
				_colorStyle = null;
			}
		}

		#endregion


		#region IEntity Members

		/// <override></override>
		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			CapShape = (CapShape)reader.ReadByte();
			CapSize = reader.ReadInt16();
			ColorStyle = (IColorStyle)reader.ReadColorStyle();
		}


		/// <override></override>
		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteByte((byte)_capShape);
			writer.WriteInt16(_capSize);
			writer.WriteStyle(_colorStyle);
		}


		/// <summary>
		/// The entity type name of <see cref="T:Dataweb.NShape.CapStyle" />.
		/// </summary>
		public static string EntityTypeName {
			get { return "Core.CapStyle"; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.CapStyle" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("CapShape", typeof(byte));
			yield return new EntityFieldDefinition("CapSize", typeof(short));
			yield return new EntityFieldDefinition("ColorStyle", typeof(object));
		}

		#endregion


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_CapStyle_CapShape")]
		[LocalizedDescription("PropDesc_CapStyle_CapShape")]
		[RequiredPermission(Permission.Designs)]
		public CapShape CapShape {
			get { return _capShape; }
			set { _capShape = value; }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_CapStyle_CapSize")]
		[LocalizedDescription("PropDesc_CapStyle_CapSize")]
		[RequiredPermission(Permission.Designs)]
		public short CapSize {
			get { return _capSize; }
			set {
				if (value < 0) throw new ArgumentException(Properties.Resources.MessageTxt_ValueHasToBeGreaterThanZero);
				_capSize = value;
			}
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_CapStyle_ColorStyle")]
		[LocalizedDescription("PropDesc_CapStyle_ColorStyle")]
		[RequiredPermission(Permission.Designs)]
		public IColorStyle ColorStyle {
			get { return _colorStyle ?? Dataweb.NShape.ColorStyle.Empty; }
			set {
				if (value == Dataweb.NShape.ColorStyle.Empty)
					_colorStyle = null;
				else _colorStyle = value;
			}
		}


		/// <override></override>
		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is CapStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);
				this.CapShape = ((CapStyle)style).CapShape;
				this.CapSize = ((CapStyle)style).CapSize;
				IColorStyle colorStyle = (IColorStyle)findStyleCallback(((CapStyle)style).ColorStyle);
				if (colorStyle != null) this.ColorStyle = colorStyle;
				else this.ColorStyle = ((CapStyle)style).ColorStyle;
			} else throw new NShapeException("Style is not of the required type.");
		}


		/// <override></override>
		public bool Equals(ICapStyle other) {
			return other == this;
		}


		/// <override></override>
		public bool Equals(CapStyle other) {
			return other == this;
		}


		/// <override></override>
		protected override bool IsStandardName(string name) {
			return StandardNames.EqualsAny(name);
		}


		static CapStyle() {
			StandardNames = new StandardCapStyleNames();

			Default = new CapStyle(EmptyStyleName);
			Default.CapShape = CapShape.None;
			Default.CapSize = 1;
			Default.ColorStyle = Dataweb.NShape.ColorStyle.Empty;
		}


		private int GetCapShapePointCount(CapShape capShape) {
			switch (capShape) {
				case CapShape.OpenArrow:
				case CapShape.ClosedArrow:
				case CapShape.Triangle:
					return 3;
				case CapShape.Circle:
				case CapShape.Diamond:
				case CapShape.Square:
					return 4;
				case CapShape.None:
				case CapShape.Flat:
				case CapShape.Peak:
				case CapShape.Round:
					return 0;
				default:
					throw new NShapeUnsupportedValueException(capShape);
			}
		}


		#region Fields

		private CapShape _capShape = CapShape.None;
		private short _capSize = 10;
		private IColorStyle _colorStyle = null;

		#endregion
	}


	/// <summary>
	/// Provides the definition of text appearance.
	/// </summary>
	public sealed class CharacterStyle : Style, ICharacterStyle, IEquatable<ICharacterStyle>, IEquatable<CharacterStyle> {

		/// <summary>
		/// This static read-only field represents a default and not initialized character style.
		/// </summary>
		public static readonly CharacterStyle Default;


		/// <summary>
		/// Provides the <see cref="T:Dataweb.NShape.StandardCharacterStyleNames" />.
		/// </summary>
		public static StandardCharacterStyleNames StandardNames;


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.CharacterStyle" />.
		/// </summary>
		public CharacterStyle()
			: base() {
			Construct();
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.CharacterStyle" />.
		/// </summary>
		public CharacterStyle(string name)
			: base(name) {
			Construct();
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.CharacterStyle" />.
		/// </summary>
		public CharacterStyle(string name, float sizeInPoints, IColorStyle colorStyle)
			: base(name) {
			this.ColorStyle = colorStyle;
			this._fontSizeInPoints = sizeInPoints;
			Construct();
		}


		#region IDisposable Members

		/// <override></override>
		public override void Dispose() {
			if (_colorStyle != null) {
				_colorStyle.Dispose();
				_colorStyle = null;
			}
		}

		#endregion


		#region IEntity Members

		/// <override></override>
		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			_fontFamily = FindFontFamily(reader.ReadString());
			_fontSizeInPoints = reader.ReadInt32() / 100f;
			_fontSize = Geometry.PointToPixel(_fontSizeInPoints, dpi);
			_fontStyle = (FontStyle)reader.ReadByte();
			ColorStyle = (IColorStyle)reader.ReadColorStyle();	// Set Property!
		}


		/// <override></override>
		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteString(_fontFamily.Name);
			writer.WriteInt32((int)(100 * _fontSizeInPoints));
			writer.WriteByte((byte)_fontStyle);
			writer.WriteStyle(_colorStyle);
		}


		/// <summary>
		/// The type name of <see cref="T:Dataweb.NShape.CharacterStyle" />.
		/// </summary>
		public static string EntityTypeName {
			get { return "Core.CharacterStyle"; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.CharacterStyle" />.
		/// </summary>
		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Dataweb.NShape.Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("FontName", typeof(string));
			yield return new EntityFieldDefinition("Size", typeof(int));
			yield return new EntityFieldDefinition("Decoration", typeof(byte));
			yield return new EntityFieldDefinition("ColorStyle", typeof(object));
		}

		#endregion


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_CharStyle_ColorStyle")]
		[LocalizedDescription("PropDesc_CharStyle_ColorStyle")]
		[RequiredPermission(Permission.Designs)]
		public IColorStyle ColorStyle {
			get { return _colorStyle ?? Dataweb.NShape.ColorStyle.Empty; }
			set {
				if (value == Dataweb.NShape.ColorStyle.Empty)
					_colorStyle = null;
				else _colorStyle = value;
			}
		}


		/// <override></override>
		[Browsable(false)]
		[CategoryAppearance()]
		[RequiredPermission(Permission.Designs)]
		public FontFamily FontFamily {
			get { return _fontFamily; }
			set { _fontFamily = value; }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_CharStyle_FontName")]
		[LocalizedDescription("PropDesc_CharStyle_FontName")]
		[RequiredPermission(Permission.Designs)]
		[Editor("Dataweb.NShape.WinFormsUI.FontFamilyUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
		public string FontName {
			get { return _fontFamily.Name; }
			set { _fontFamily = FindFontFamily(value); }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_CharStyle_Size")]
		[LocalizedDescription("PropDesc_CharStyle_Size")]
		[RequiredPermission(Permission.Designs)]
		public int Size {
			get { return _fontSize; }
			set {
				_fontSize = value;
				_fontSizeInPoints = Geometry.PixelToPoint(value, dpi);
			}
		}


		/// <summary>
		/// Font Size in Point (1/72 Inch)
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_CharStyle_SizeInPoints")]
		[LocalizedDescription("PropDesc_CharStyle_SizeInPoints")]
		[RequiredPermission(Permission.Designs)]
		public float SizeInPoints {
			get { return _fontSizeInPoints; }
			set {
				_fontSizeInPoints = value;
				_fontSize = Geometry.PointToPixel(value, dpi);
			}
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_CharStyle_Style")]
		[LocalizedDescription("PropDesc_CharStyle_Style")]
		[RequiredPermission(Permission.Designs)]
		public FontStyle Style {
			get { return _fontStyle; }
			set {
				if (value == FontStyle.Regular)
					_fontStyle = FontStyle.Regular;
				else
					_fontStyle = _fontStyle ^ value;
			}
		}


		/// <override></override>
		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is CharacterStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);

				IColorStyle colorStyle = (IColorStyle)findStyleCallback(((CharacterStyle)style).ColorStyle);
				if (colorStyle != null) this.ColorStyle = colorStyle;
				else this.ColorStyle = ((CharacterStyle)style).ColorStyle;

				this.FontName = ((CharacterStyle)style).FontName;
				this.SizeInPoints = ((CharacterStyle)style).SizeInPoints;
				this.Style = ((CharacterStyle)style).Style;
			} else throw new NShapeException("Style is not of the required type.");
		}


		/// <override></override>
		public bool Equals(ICharacterStyle other) {
			return other == this;
		}


		/// <override></override>
		public bool Equals(CharacterStyle other) {
			return other == this;
		}


		/// <override></override>
		protected override bool IsStandardName(string name) {
			return CharacterStyle.StandardNames.EqualsAny(name);
		}


		static CharacterStyle() {
			StandardNames = new StandardCharacterStyleNames();
			using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero))
				dpi = gfx.DpiY;

			Default = new CharacterStyle(EmptyStyleName);
			Default.ColorStyle = Dataweb.NShape.ColorStyle.Empty;
			Default.SizeInPoints = 10;
			Default.Style = FontStyle.Regular;
		}


		private void Construct() {
			// Get the system's default GenericSansSerif font family
			_fontFamily = FindFontFamily(string.Empty);
			_fontSize = Geometry.PointToPixel(_fontSizeInPoints, dpi);
		}


		private FontFamily FindFontFamily(string fontName) {
			FontFamily result = null;
			if (!string.IsNullOrEmpty(fontName)) {
				FontFamily[] families = FontFamily.Families;
				foreach (FontFamily ff in families) {
					if (ff.Name.Equals(fontName, StringComparison.InvariantCultureIgnoreCase)) {
						result = ff;
						break;
					}
				}
			}
			return result ?? FontFamily.GenericSansSerif;
		}


		#region Fields
		private static readonly float dpi;

		private float _fontSizeInPoints = 8.25f;
		private int _fontSize;
		private FontStyle _fontStyle = 0;
		private FontFamily _fontFamily = null;
		private IColorStyle _colorStyle = null;
		#endregion
	}

	
	/// <summary>
	/// Provides the definition of a color.
	/// </summary>
	public sealed class ColorStyle : Style, IColorStyle, IEquatable<IColorStyle>, IEquatable<ColorStyle> {

		/// <summary>
		/// This static read-only field represents a default and not initialized color style.
		/// </summary>
		public static readonly IColorStyle Empty;


		/// <summary>
		/// Provides the <see cref="T:Dataweb.NShape.StandardColorStyleNames" />.
		/// </summary>
		public static StandardColorStyleNames StandardNames;


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.ColorStyle" />.
		/// </summary>
		public ColorStyle()
			: this(string.Empty, Color.Empty) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.ColorStyle" />.
		/// </summary>
		public ColorStyle(string name)
			: this(name, Color.Empty) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.ColorStyle" />.
		/// </summary>
		public ColorStyle(string name, Color color)
			: base(name) {
			Construct(color, AlphaToTransparency(color.A));
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.ColorStyle" />.
		/// </summary>
		public ColorStyle(string name, Color color, byte transparencyPercentage)
			: base(name) {
				Construct(color, transparencyPercentage);
		}


		#region IDisposable Members

		/// <override></override>
		public override void Dispose() {
			// nothing to do
		}

		#endregion


		#region IEntity Members

		/// <override></override>
		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			_color = Color.FromArgb(reader.ReadInt32());
			_transparency = reader.ReadByte();
			if (version >= 3) _convertToGray = reader.ReadBool();
		}


		/// <override></override>
		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt32(_color.ToArgb());
			writer.WriteByte(_transparency);
			if (version >= 3) writer.WriteBool(_convertToGray);	
		}


		/// <summary>
		/// The entity type name of <see cref="T:Dataweb.NShape.ColorStyle" />.
		/// </summary>
		public static string EntityTypeName {
			get { return "Core.ColorStyle"; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.ColorStyle" />.
		/// </summary>
		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("Color", typeof(Color));
			yield return new EntityFieldDefinition("Transparency", typeof(byte));
			if (version >= 3) yield return new EntityFieldDefinition("ConvertToGray", typeof(bool));
		}

		#endregion


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_ColorStyle_Color")]
		[LocalizedDescription("PropDesc_ColorStyle_Color")]
		[RequiredPermission(Permission.Designs)]
		public Color Color {
			get { return _color; }
			set {
				_color = value;
				_transparency = AlphaToTransparency(_color.A);
			}
		}


		/// <summary>
		/// Specifies if the color should be converted into a grayscale value.
		/// </summary>
		[Browsable(false)]
		[CategoryAppearance()]
		[RequiredPermission(Permission.Designs)]
		public bool ConvertToGray {
			get { return _convertToGray; }
			set { _convertToGray = value; }
		}


		/// <summary>
		/// Indicates the transparency in percent (0-100).
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_ColorStyle_Transparency")]
		[LocalizedDescription("PropDesc_ColorStyle_Transparency")]
		[RequiredPermission(Permission.Designs)]
		public byte Transparency {
			get { return _transparency; }
			set {
				if (value < 0 || value > 100) throw new ArgumentException(Properties.Resources.MessageTxt_ValueHasToBeBetween0And100);
				_transparency = value;
				_color = Color.FromArgb(TransparencyToAlpha(_transparency), _color);
			}
		}


		/// <override></override>
		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is ColorStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);
				this.Color = ((ColorStyle)style).Color;
				this.Transparency = ((ColorStyle)style).Transparency;
				this.ConvertToGray = ((ColorStyle)style).ConvertToGray;
			} else throw new NShapeException("Style is not of the required type.");
		}


		/// <override></override>
		public bool Equals(IColorStyle other) {
			return other == this;
		}


		/// <override></override>
		public bool Equals(ColorStyle other) {
			return other == this;
		}


		/// <override></override>
		protected override bool IsStandardName(string name) {
			return StandardNames.EqualsAny(name);
		}


		static ColorStyle() {
			StandardNames = new StandardColorStyleNames();
			Empty = new ColorStyle(EmptyStyleName, Color.Empty) { Title = EmptyStyleTitle };
		}


		private void Construct(Color color, byte transparency) {
			if (transparency < 0 || transparency > 100)
				throw new ArgumentOutOfRangeException("transparency", Properties.Resources.MessageTxt_ValueHasToBeBetween0And100);
			this._transparency = transparency;
			this._color = Color.FromArgb(TransparencyToAlpha(transparency), color);
		}


		private byte AlphaToTransparency(byte alpha) {
			return (byte)(100 - Math.Round(alpha / 2.55f));
		}


		private byte TransparencyToAlpha(byte transparency) {
			if (transparency < 0 || transparency > 100)
				throw new ArgumentOutOfRangeException(Dataweb.NShape.Properties.Resources.MessageTxt_ValueHasToBeBetween0And100);
			return Convert.ToByte(255 - (transparency * 2.55f));
		}


		#region Fields
		private Color _color = Color.White;
		private byte _transparency = 0;
		private bool _convertToGray = false;
		#endregion
	}


	/// <summary>
	/// Provides the definition of filling.
	/// </summary>
	public sealed class FillStyle : Style, IFillStyle, IEquatable<IFillStyle>, IEquatable<FillStyle> {

		/// <summary>
		/// This static read-only field represents a default and not initialized fill style.
		/// </summary>
		public static readonly FillStyle Empty;


		/// <summary>
		/// Provides the <see cref="T:Dataweb.NShape.StandardFillStyleNames" />.
		/// </summary>
		public static StandardFillStyleNames StandardNames;


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.FillStyle" />.
		/// </summary>
		public FillStyle()
			: this(string.Empty) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.FillStyle" />.
		/// </summary>
		public FillStyle(string name)
			: this(name, ColorStyle.Empty, ColorStyle.Empty) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.FillStyle" />.
		/// </summary>
		public FillStyle(string name, IColorStyle baseColorStyle, IColorStyle additionalColorStyle)
			: base(name) {
			Construct(baseColorStyle, additionalColorStyle);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.FillStyle" />.
		/// </summary>
		public FillStyle(string name, NamedImage image)
			: base(name) {
			Construct(image);
		}


		#region IDisposable Members

		/// <override></override>
		public override void Dispose() {
			if (_baseColorStyle != null) {
				_baseColorStyle.Dispose();
				_baseColorStyle = null;
			}
			if (_additionalColorStyle != null) {
				_additionalColorStyle.Dispose();
				_additionalColorStyle = null;
			}
			if (_image != null) {
				_image.Dispose();
				_image = null;
			}
		}

		#endregion


		#region IEntity Members

		/// <override></override>
		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			BaseColorStyle = (IColorStyle)reader.ReadColorStyle();	// Set property!
			AdditionalColorStyle = (IColorStyle)reader.ReadColorStyle();	// Set property!
			_fillMode = (FillMode)reader.ReadByte();
			_fillPattern = (HatchStyle)reader.ReadByte();
			if (version >= 3)  _convertToGrayScale = reader.ReadBool();
			_imageLayout = (ImageLayoutMode)reader.ReadByte();
			_imageTransparency = reader.ReadByte();
			_imageGamma = reader.ReadFloat();
			_imageCompressionQuality = reader.ReadByte();
			string imgName = reader.ReadString();
			Image img = reader.ReadImage();
			if (img != null) _image = new NamedImage(img, imgName);
		}


		/// <override></override>
		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteStyle(_baseColorStyle);
			writer.WriteStyle(_additionalColorStyle);
			writer.WriteByte((byte)_fillMode);
			writer.WriteByte((byte)_fillPattern);
			if (version >= 3) writer.WriteBool(_convertToGrayScale);
			writer.WriteByte((byte)_imageLayout);
			writer.WriteByte(_imageTransparency);
			writer.WriteFloat(_imageGamma);
			writer.WriteByte(_imageCompressionQuality);
			if (NamedImage.IsNullOrEmpty(_image)) {
				writer.WriteString(string.Empty);
				writer.WriteImage(null);
			} else {
				writer.WriteString(_image.Name);
				object imgTag = _image.Image.Tag;
				_image.Image.Tag = _image.Name;
				writer.WriteImage(_image.Image);
				_image.Image.Tag = imgTag;
			}
		}


		/// <summary>
		/// The entity type name of <see cref="T:Dataweb.NShape.FillStyle" />.
		/// </summary>
		public static string EntityTypeName {
			get { return "Core.FillStyle"; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.FillStyle" />.
		/// </summary>
		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("BaseColorStyle", typeof(object));
			yield return new EntityFieldDefinition("AdditionalColorStyle", typeof(object));
			yield return new EntityFieldDefinition("FillMode", typeof(byte));
			yield return new EntityFieldDefinition("FillPattern", typeof(byte));
			if (version >= 3) yield return new EntityFieldDefinition("ConvertToGrayScale", typeof(bool));
			yield return new EntityFieldDefinition("ImageLayout", typeof(byte));
			yield return new EntityFieldDefinition("ImageTransparency", typeof(byte));
			yield return new EntityFieldDefinition("ImageGammaCorrection", typeof(float));
			yield return new EntityFieldDefinition("ImageCompressionQuality", typeof(byte));
			yield return new EntityFieldDefinition("ImageFileName", typeof(string));
			yield return new EntityFieldDefinition("Image", typeof(Image));
		}

		#endregion


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_FillStyle_AdditionalColorStyle")]
		[LocalizedDescription("PropDesc_FillStyle_AdditionalColorStyle")]
		[RequiredPermission(Permission.Designs)]
		public IColorStyle AdditionalColorStyle {
			get { return _additionalColorStyle ?? Dataweb.NShape.ColorStyle.Empty; }
			set {
				if (value == Dataweb.NShape.ColorStyle.Empty)
					_additionalColorStyle = null;
				else _additionalColorStyle = value;
			}
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_FillStyle_BaseColorStyle")]
		[LocalizedDescription("PropDesc_FillStyle_BaseColorStyle")]
		[RequiredPermission(Permission.Designs)]
		public IColorStyle BaseColorStyle {
			get { return _baseColorStyle ?? Dataweb.NShape.ColorStyle.Empty; }
			set {
				if (value == Dataweb.NShape.ColorStyle.Empty)
					_baseColorStyle = null;
				else _baseColorStyle = value;
			}
		}


		/// <summary>
		/// If true, the Image is shown as grayscale image
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_FillStyle_ConvertToGrayScale")]
		[LocalizedDescription("PropDesc_FillStyle_ConvertToGrayScale")]
		[RequiredPermission(Permission.Designs)]
		public bool ConvertToGrayScale {
			get { return _convertToGrayScale; }
			set { _convertToGrayScale = value; }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_FillStyle_FillMode")]
		[LocalizedDescription("PropDesc_FillStyle_FillMode")]
		[RequiredPermission(Permission.Designs)]
		public FillMode FillMode {
			get { return _fillMode; }
			set { _fillMode = value; }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_FillStyle_FillPattern")]
		[LocalizedDescription("PropDesc_FillStyle_FillPattern")]
		[RequiredPermission(Permission.Designs)]
		public HatchStyle FillPattern {
			get { return _fillPattern; }
			set { _fillPattern = value; }
		}


		/// <override></override>
		[Browsable(true)]
		public short GradientAngle {
			get { return _gradientAngle; }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_FillStyle_Image")]
		[LocalizedDescription("PropDesc_FillStyle_Image")]
		[RequiredPermission(Permission.Designs)]
		[Editor("Dataweb.NShape.WinFormsUI.NamedImageUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
		public NamedImage Image {
			get { return _image; }
			set { _image = value; }
		}


		/// <summary>
		/// Quality setting in percentage when compressing the image with a non-lossless encoder.
		/// </summary>
		[Browsable(false)]
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_FillStyle_ImageCompressionQuality")]
		[LocalizedDescription("PropDesc_FillStyle_ImageCompressionQuality")]
		[RequiredPermission(Permission.Designs)]
		public byte ImageCompressionQuality {
			get { return _imageCompressionQuality; }
			set {
				if (value < 0 || value > 100) throw new NShapeException(Properties.Resources.MessageTxt_ValueHasToBeBetween0And100);
				_imageCompressionQuality = value;
			}
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_FillStyle_ImageGammaCorrection")]
		[LocalizedDescription("PropDesc_FillStyle_ImageGammaCorrection")]
		[RequiredPermission(Permission.Designs)]
		public float ImageGammaCorrection {
			get { return _imageGamma; }
			set {
				if (value <= 0) throw new ArgumentException(Properties.Resources.MessageTxt_ValueHasToBeGreaterThanZero);
				_imageGamma = value; 
			}
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_FillStyle_ImageLayout")]
		[LocalizedDescription("PropDesc_FillStyle_ImageLayout")]
		[RequiredPermission(Permission.Designs)]
		public ImageLayoutMode ImageLayout {
			get { return _imageLayout; }
			set { _imageLayout = value; }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_FillStyle_ImageTransparency")]
		[LocalizedDescription("PropDesc_FillStyle_ImageTransparency")]
		[RequiredPermission(Permission.Designs)]
		public byte ImageTransparency {
			get { return _imageTransparency; }
			set {
				if (value < 0 || value > 100) throw new ArgumentException(Properties.Resources.MessageTxt_ValueHasToBeBetween0And100);
				_imageTransparency = value;
			}
		}


		/// <override></override>
		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is FillStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);
				IColorStyle colorStyle;
				if (((FillStyle)style).AdditionalColorStyle != null) {
					colorStyle = (IColorStyle)findStyleCallback(((FillStyle)style).AdditionalColorStyle);
					if (colorStyle != null) this.AdditionalColorStyle = colorStyle;
					else this.AdditionalColorStyle = ((FillStyle)style).AdditionalColorStyle;
				}
				if (((FillStyle)style).BaseColorStyle != null) {
					colorStyle = (IColorStyle)findStyleCallback(((FillStyle)style).BaseColorStyle);
					if (colorStyle != null) this.BaseColorStyle = colorStyle;
					else this.BaseColorStyle = ((FillStyle)style).BaseColorStyle;
				}

				this.ConvertToGrayScale = ((FillStyle)style).ConvertToGrayScale;
				this.FillMode = ((FillStyle)style).FillMode;
				this.FillPattern = ((FillStyle)style).FillPattern;
				if (this.Image != null) this.Image.Dispose();
				this.Image = ((FillStyle)style).Image;
				this.ImageCompressionQuality = ((FillStyle)style).ImageCompressionQuality;
				this.ImageGammaCorrection = ((FillStyle)style).ImageGammaCorrection;
				this.ImageLayout = ((FillStyle)style).ImageLayout;
				this.ImageTransparency = ((FillStyle)style).ImageTransparency;
			} else throw new NShapeException("Style is not of the required Type.");
		}


		/// <override></override>
		public bool Equals(IFillStyle other) {
			return other == this;
		}


		/// <override></override>
		public bool Equals(FillStyle other) {
			return other == this;
		}


		/// <override></override>
		protected override bool IsStandardName(string name) {
			return FillStyle.StandardNames.EqualsAny(name);
		}


		private void Construct(IColorStyle baseColorStyle, IColorStyle additionalColorStyle) {
			if (baseColorStyle == null) throw new ArgumentNullException("baseColorStyle");
			if (additionalColorStyle == null) throw new ArgumentNullException("additionalColorStyle");
			this.BaseColorStyle = baseColorStyle;
			this.AdditionalColorStyle = additionalColorStyle;
		}


		private void Construct(NamedImage image) {
			if (image == null) throw new ArgumentNullException("image");
			this._image = image;
		}


		static FillStyle() {
			StandardNames = new StandardFillStyleNames();

			Empty = new FillStyle(EmptyStyleName);
			Empty.AdditionalColorStyle = ColorStyle.Empty;
			Empty.BaseColorStyle = ColorStyle.Empty;
			Empty.ConvertToGrayScale = false;
			Empty.FillMode = FillMode.Solid;
			Empty.FillPattern = HatchStyle.Cross;
			Empty.Image = null;
			Empty.ImageLayout = ImageLayoutMode.Original;
		}


		#region Fields

		// Color and Pattern Stuff
		private IColorStyle _baseColorStyle = null;
		private IColorStyle _additionalColorStyle = null;
		private FillMode _fillMode = FillMode.Gradient;
		private HatchStyle _fillPattern = HatchStyle.BackwardDiagonal;
		private short _gradientAngle = 45;
		// Image Stuff
		private NamedImage _image = null;
		private ImageLayoutMode _imageLayout = ImageLayoutMode.CenterTile;
		private byte _imageTransparency = 0;
		private float _imageGamma = 1f;
		private byte _imageCompressionQuality = 100;
		private bool _convertToGrayScale = false;

		#endregion
	}


	/// <summary>
	/// Provides the definition of lines and outlines.
	/// </summary>
	public sealed class LineStyle : Style, ILineStyle, IEquatable<ILineStyle>, IEquatable<LineStyle> {

		/// <summary>
		/// This static read-only field represents a default and not initialized line style.
		/// </summary>
		public static readonly LineStyle Empty;


		/// <summary>
		/// Provides the <see cref="T:Dataweb.NShape.StandardLineStyleNames" />.
		/// </summary>
		public static StandardLineStyleNames StandardNames;


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.LineStyle" />.
		/// </summary>
		public LineStyle(string name, int lineWidth, IColorStyle colorStyle)
			: base(name) {
			this._lineWidth = lineWidth;
			this._colorStyle = colorStyle;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.LineStyle" />.
		/// </summary>
		public LineStyle(string name)
			: base(name) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.LineStyle" />.
		/// </summary>
		public LineStyle()
			: base() {
		}


		#region IDisposable Members

		/// <override></override>
		public override void Dispose() {
			if (_colorStyle != null) {
				_colorStyle.Dispose();
				_colorStyle = null;
			}
			if (_dashPattern != null)
				_dashPattern = null;
		}

		#endregion


		#region IPersistable Members

		/// <override></override>
		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			LineWidth = reader.ReadInt32();
			DashType = (DashType)reader.ReadByte();
			DashCap = (DashCap)reader.ReadByte();			// set property instead of member var in order to create DashPattern array
			LineJoin = (LineJoin)reader.ReadByte();
			ColorStyle = (IColorStyle)reader.ReadColorStyle();	// Set property!
		}


		/// <override></override>
		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt32(_lineWidth);
			writer.WriteByte((byte)_dashStyle);
			writer.WriteByte((byte)_dashCap);
			writer.WriteByte((byte)_lineJoin);
			writer.WriteStyle(_colorStyle);
		}


		/// <summary>
		/// The entity type name of <see cref="T:Dataweb.NShape.LineStyle" />.
		/// </summary>
		public static string EntityTypeName {
			get { return "Core.LineStyle"; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.LineStyle" />.
		/// </summary>
		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("LineWidth", typeof(int));
			yield return new EntityFieldDefinition("DashType", typeof(byte));
			yield return new EntityFieldDefinition("DashCap", typeof(byte));
			yield return new EntityFieldDefinition("LineJoin", typeof(byte));
			yield return new EntityFieldDefinition("ColorStyle", typeof(object));
		}

		#endregion


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_LineStyle_ColorStyle")]
		[LocalizedDescription("PropDesc_LineStyle_ColorStyle")]
		[RequiredPermission(Permission.Designs)]
		public IColorStyle ColorStyle {
			get { return _colorStyle ?? Dataweb.NShape.ColorStyle.Empty; }
			set {
				if (value == Dataweb.NShape.ColorStyle.Empty)
					_colorStyle = null;
				else _colorStyle = value;
			}
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_LineStyle_DashCap")]
		[LocalizedDescription("PropDesc_LineStyle_DashCap")]
		[RequiredPermission(Permission.Designs)]
		public DashCap DashCap {
			get { return _dashCap; }
			set { _dashCap = value; }
		}


		/// <override></override>
		[Browsable(false)]
		public float[] DashPattern {
			get { return _dashPattern; }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_LineStyle_DashType")]
		[LocalizedDescription("PropDesc_LineStyle_DashType")]
		[RequiredPermission(Permission.Designs)]
		public DashType DashType {
			get { return _dashStyle; }
			set {
				_dashStyle = value;
				switch (_dashStyle) {
					case DashType.Solid:
						_dashPattern = new float[0];
						break;
					case DashType.Dash:
						_dashPattern = new float[2] { lineDashLen, lineDashSpace };
						break;
					case DashType.DashDot:
						_dashPattern = new float[4] { lineDashLen, lineDashSpace, lineDotLen, lineDashSpace };
						break;
					case DashType.DashDotDot:
						_dashPattern = new float[6] { lineDashLen, lineDashSpace, lineDotLen, lineDotSpace, lineDotLen, lineDashSpace };
						break;
					case DashType.Dot:
						_dashPattern = new float[2] { lineDotLen, lineDotSpace };
						break;
					default:
						throw new NShapeUnsupportedValueException(_dashStyle);
				}
			}
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_LineStyle_LineJoin")]
		[LocalizedDescription("PropDesc_LineStyle_LineJoin")]
		[RequiredPermission(Permission.Designs)]
		public LineJoin LineJoin {
			get { return _lineJoin; }
			set { _lineJoin = value; }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_LineStyle_LineWidth")]
		[LocalizedDescription("PropDesc_LineStyle_LineWidth")]
		[RequiredPermission(Permission.Designs)]
		public int LineWidth {
			get { return _lineWidth; }
			set {
				if (value <= 0) throw new NShapeException(Properties.Resources.MessageTxt_ValueHasToBeGreaterThanZero);
				_lineWidth = value;
			}
		}


		/// <override></override>
		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is LineStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);
				IColorStyle colorStyle = (IColorStyle)findStyleCallback(((LineStyle)style).ColorStyle);
				if (colorStyle != null) this.ColorStyle = colorStyle;
				else this.ColorStyle = ((LineStyle)style).ColorStyle;

				this.DashCap = ((LineStyle)style).DashCap;
				this.DashType = ((LineStyle)style).DashType;
				this.LineJoin = ((LineStyle)style).LineJoin;
				this.LineWidth = ((LineStyle)style).LineWidth;
			} else throw new NShapeException("Style is not of the required type.");
		}


		/// <override></override>
		public bool Equals(ILineStyle other) {
			return other == this;
		}


		/// <override></override>
		public bool Equals(LineStyle other) {
			return other == this;
		}


		/// <override></override>
		protected override bool IsStandardName(string name) {
			return LineStyle.StandardNames.EqualsAny(name);
		}


		static LineStyle() {
			StandardNames = new StandardLineStyleNames();

			Empty = new LineStyle(EmptyStyleName);
			Empty.ColorStyle = Dataweb.NShape.ColorStyle.Empty;
			Empty.DashCap = DashCap.Round;
			Empty.DashType = DashType.Solid;
			Empty.LineJoin = LineJoin.Round;
			Empty.LineWidth = 1;
		}


		#region Fields

		private int _lineWidth = 1;
		private IColorStyle _colorStyle = null;
		private DashType _dashStyle = DashType.Solid;
		private DashCap _dashCap = DashCap.Round;
		private LineJoin _lineJoin = LineJoin.Round;
		private float[] _dashPattern = new float[0];
		// dashpattern defs
		private const float lineDashSpace = 2f;
		private const float lineDashLen = 5f;
		private const float lineDotSpace = 1f;
		private const float lineDotLen = 1f;

		#endregion
	}


	/// <summary>
	/// Provides the definition of text layout.
	/// </summary>
	public sealed class ParagraphStyle : Style, IParagraphStyle, IEquatable<IParagraphStyle>, IEquatable<ParagraphStyle> {

		/// <summary>
		/// This static read-only field represents a default and not initialized paragraph style.
		/// </summary>
		public static readonly ParagraphStyle Empty;


		/// <summary>
		/// Provides the <see cref="T:Dataweb.NShape.StandardParagraphStyleNames" />.
		/// </summary>
		public static StandardParagraphStyleNames StandardNames;


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.ParagraphStyle" />.
		/// </summary>
		public ParagraphStyle(string name)
			: base(name) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.ParagraphStyle" />.
		/// </summary>
		public ParagraphStyle()
			: base() {
		}


		#region IDisposable Members

		/// <override></override>
		public override void Dispose() {
			// nothing to do
		}

		#endregion


		#region IEntity Members

		/// <override></override>
		public override void LoadFields(IRepositoryReader reader, int version) {
			base.LoadFields(reader, version);
			Alignment = (ContentAlignment)reader.ReadInt16();
			Trimming = (StringTrimming)reader.ReadByte();
			Padding = new TextPadding(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
			WordWrap = reader.ReadBool();
		}


		/// <override></override>
		public override void SaveFields(IRepositoryWriter writer, int version) {
			base.SaveFields(writer, version);
			writer.WriteInt16((short)_alignment);
			writer.WriteByte((byte)_trimming);
			writer.WriteInt32(_padding.Left);
			writer.WriteInt32(_padding.Top);
			writer.WriteInt32(_padding.Right);
			writer.WriteInt32(_padding.Bottom);
			writer.WriteBool(_wordWrap);
		}


		/// <summary>
		/// The entity type name of <see cref="T:Dataweb.NShape.ParagraphStyle" />.
		/// </summary>
		public static string EntityTypeName {
			get { return "Core.ParagraphStyle"; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.ParagraphStyle" />.
		/// </summary>
		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in Style.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("Alignment", typeof(short));
			yield return new EntityFieldDefinition("Trimming", typeof(byte));
			yield return new EntityFieldDefinition("PaddingLeft", typeof(int));
			yield return new EntityFieldDefinition("PaddingTop", typeof(int));
			yield return new EntityFieldDefinition("PaddingRight", typeof(int));
			yield return new EntityFieldDefinition("PaddingBottom", typeof(int));
			yield return new EntityFieldDefinition("WordWrap", typeof(bool));
		}

		#endregion


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_ParagraphStyle_Alignment")]
		[LocalizedDescription("PropDesc_ParagraphStyle_Alignment")]
		[RequiredPermission(Permission.Designs)]
		public ContentAlignment Alignment {
			get { return _alignment; }
			set { _alignment = value; }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_ParagraphStyle_Trimming")]
		[LocalizedDescription("PropDesc_ParagraphStyle_Trimming")]
		[RequiredPermission(Permission.Designs)]
		public StringTrimming Trimming {
			get { return _trimming; }
			set { _trimming = value; }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_ParagraphStyle_Padding")]
		[LocalizedDescription("PropDesc_ParagraphStyle_Padding")]
		[RequiredPermission(Permission.Designs)]
		public TextPadding Padding {
			get { return _padding; }
			set { _padding = value; }
		}


		/// <override></override>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_ParagraphStyle_WordWrap")]
		[LocalizedDescription("PropDesc_ParagraphStyle_WordWrap")]
		[RequiredPermission(Permission.Designs)]
		public bool WordWrap {
			get { return _wordWrap; }
			set { _wordWrap = value; }
		}


		/// <override></override>
		public override void Assign(IStyle style, FindStyleCallback findStyleCallback) {
			if (style is ParagraphStyle) {
				// Delete GDI+ objects based on the current style
				ToolCache.NotifyStyleChanged(this);

				base.Assign(style, findStyleCallback);
				this.Alignment = ((ParagraphStyle)style).Alignment;
				this.Padding = ((ParagraphStyle)style).Padding;
				this.Trimming = ((ParagraphStyle)style).Trimming;
				this.WordWrap = ((ParagraphStyle)style).WordWrap;
			} else throw new NShapeException("Style is not of the required type.");
		}


		/// <override></override>
		public bool Equals(IParagraphStyle other) {
			return other == this;
		}


		/// <override></override>
		public bool Equals(ParagraphStyle other) {
			return other == this;
		}


		/// <override></override>
		protected override bool IsStandardName(string name) {
			return ParagraphStyle.StandardNames.EqualsAny(name);
		}


		static ParagraphStyle() {
			StandardNames = new StandardParagraphStyleNames();

			Empty = new ParagraphStyle(EmptyStyleName);
			Empty.Alignment = ContentAlignment.MiddleCenter;
			Empty.Padding = TextPadding.Empty;
			Empty.Trimming = StringTrimming.None;
			Empty.WordWrap = true;
		}


		#region Fields

		private ContentAlignment _alignment = ContentAlignment.MiddleCenter;
		private StringTrimming _trimming = StringTrimming.None;
		private TextPadding _padding = TextPadding.Empty;
		private bool _wordWrap = true;

		#endregion
	}

	#endregion


	#region ***   StyleCollection Classes   ***

	/// <summary>
	/// A collection of <see cref="T:Dataweb.NShape.IStyle" /> sorted by name.
	/// </summary>
	public abstract class StyleCollection<TStyle, TStyleInterface> : IEnumerable<TStyleInterface>
		where TStyle : class, TStyleInterface 
		where TStyleInterface : class, IStyle
	{

		/// <summary>
		/// Initialize a new instance of <see cref="T:Dataweb.NShape.StyleCollection`1" />.
		/// </summary>
		public StyleCollection() {
			Construct(-1);
		}


		/// <summary>
		/// Initialize a new instance of <see cref="T:Dataweb.NShape.StyleCollection`1" />.
		/// </summary>
		public StyleCollection(int capacity) {
			Construct(capacity);
		}


		#region IEnumerable<TStyleInterface> Members

		/// <override></override>
		public IEnumerator<TStyleInterface> GetEnumerator() {
			return StyleCollectionEnumerator.Create(this);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return StyleCollectionEnumerator.Create(this);
		}

		#endregion


		#region [Public] Properties

		/// <summary>
		/// Provides index based direct access to the items of the collection.
		/// </summary>
		/// <param name="index">Zero-based index.</param>
		public TStyle this[int index] {
			get { return internalList.Values[index].Style; }
		}


		/// <summary>
		/// Provides index based direct access to the items of the collection.
		/// </summary>
		/// <param name="name">The name of the style.</param>
		public TStyle this[string name] {
			get { return internalList[name].Style; }
		}


		/// <summary>
		/// Returns the number of items in the collection.
		/// </summary>
		public int Count {
			get { return internalList.Count; }
		}

		#endregion


		#region [Public] Methods

		/// <ToBeCompleted></ToBeCompleted>
		public TStyle GetPreviewStyle(TStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			// If the style has been renamed, the preview style won't be found by name,
			// so we have to iterate through the elements and compare by reference.
			TStyle result = null;
			foreach (KeyValuePair<string, StylePair<TStyle>> item in internalList) {
				if (item.Value.Style == style) {
					result = item.Value.PreviewStyle;
					break;
				}
			}
			if (result == null) throw new KeyNotFoundException();
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public TStyle GetPreviewStyle(string styleName) {
			if (styleName == null) throw new ArgumentNullException("styleName");
			return internalList[styleName].PreviewStyle;
		}


		/// <override></override>
		public void Add(TStyle style, TStyle previewStyle) {
			if (style == null) throw new ArgumentNullException("style");
			if (previewStyle == null) throw new ArgumentNullException("previewStyle");
			internalList.Add(style.Name, new StylePair<TStyle>(style, previewStyle));
			RegisterEventHandler(style);
		}

		
		/// <override></override>
		public void Clear() {
			foreach (KeyValuePair<string, StylePair<TStyle>> item in internalList) {
				IStyle baseStyle = item.Value.Style;
				IStyle previewStyle = item.Value.PreviewStyle;
				if (baseStyle != null) baseStyle.Dispose();
				if (previewStyle != null) previewStyle.Dispose();
				baseStyle = previewStyle = null;
			}
			internalList.Clear();
		}


		/// <summary>
		/// Returns true if the collection contains the given style itself or, in case the given 
		/// style is a preview style, the style for the given preview style.
		/// </summary>
		public bool Contains(TStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			foreach (KeyValuePair<string, StylePair<TStyle>> item in internalList) {
				if (item.Value.Style == style || item.Value.PreviewStyle == style)
					return true;
			}
			return false;
		}


		/// <override></override>
		public bool Contains(string name) {
			if (name == null) throw new ArgumentNullException("name");
			return internalList.ContainsKey(name);
		}


		/// <summary>
		/// Returns true if the collection contains a preview style for the given style or, in 
		/// case the given style is a preview style, the given preview style itself.
		/// </summary>
		public bool ContainsPreviewStyle(TStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			foreach (KeyValuePair<string, StylePair<TStyle>> item in internalList) {
				if (item.Value.Style == style)
					return (item.Value.PreviewStyle != null);
				else if (item.Value.PreviewStyle == style)
					return true;
			}
			return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool ContainsPreviewStyle(string name) {
			if (name == null) throw new ArgumentNullException("name");
			if (internalList.ContainsKey(name))
				return (internalList[name].PreviewStyle != null);
			else return false;
		}


		/// <override></override>
		public int IndexOf(TStyle item) {
			if (item == null) throw new ArgumentNullException("item");
			foreach (StylePair<TStyle> stylePair in internalList.Values) {
				if (stylePair.Style == item || stylePair.PreviewStyle == item)
					return internalList.IndexOfValue(stylePair);
			}
			return -1;
		}


		/// <override></override>
		public int IndexOf(string styleName) {
			if (styleName == null) throw new ArgumentNullException("styleName");
			return internalList.IndexOfKey(styleName);
		}


		/// <override></override>
		public bool Remove(TStyle style) {
			if (style == null) throw new ArgumentNullException("item");
			UnregisterEventHandler(style);
			return internalList.Remove(style.Name);
		}


		/// <override></override>
		public bool Remove(string styleName) {
			UnregisterEventHandler(internalList[styleName].Style);
			return internalList.Remove(styleName);
		}


		/// <override></override>
		public void RemoveAt(int index) {
			string key = internalList.Keys[index];
			UnregisterEventHandler(internalList[key].Style);
			internalList.Remove(key);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public abstract bool IsStandardStyle(TStyle style);


		/// <ToBeCompleted></ToBeCompleted>
		public void SetPreviewStyle(string baseStyleName, TStyle value) {
			if (baseStyleName == null) throw new ArgumentNullException("baseStyle");
			if (value == null) throw new ArgumentNullException("value");
			internalList[baseStyleName].PreviewStyle = value;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SetPreviewStyle(TStyle baseStyle, TStyle value) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			if (value == null) throw new ArgumentNullException("value");
			internalList[baseStyle.Name].PreviewStyle = value;
		}

		#endregion


		/// <ToBeCompleted></ToBeCompleted>
		protected void SetStyle(TStyle style, TStyle previewStyle) {
			if (style == null) throw new ArgumentNullException("style");
			if (previewStyle == null) throw new ArgumentNullException("previewStyle");
			internalList[style.Name].Style = style;
			internalList[style.Name].PreviewStyle = previewStyle;
		}


		private void StyleCollection_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == "Name" && sender is TStyle) {
				TStyle style = (TStyle)sender;
				string key = null;
				StylePair<TStyle> value = null;
				foreach (KeyValuePair<string, StylePair<TStyle>> item in internalList) {
					if (item.Value.Style == style || item.Value.PreviewStyle == style) {
						key = item.Key;
						value = item.Value;
						break;
					}
				}
				Debug.Assert(key != null && value != null);
				// Once we have found the old key, remove and add the items from the 
				// internal list (without re-registering the PropertyChanged event).
				internalList.Remove(key);
				internalList.Add(style.Name, value);
			}
		}


		private void RegisterEventHandler(TStyle style) {
			// Register for the style's propertyChanged event in order to get notified about name changes
			if (style is INotifyPropertyChanged)
				((INotifyPropertyChanged)style).PropertyChanged += new PropertyChangedEventHandler(StyleCollection_PropertyChanged);
		}


		private void UnregisterEventHandler(TStyle style) {
			// Register for the style's propertyChanged event in order to get notified about name changes
			if (style is INotifyPropertyChanged)
				((INotifyPropertyChanged)style).PropertyChanged -= new PropertyChangedEventHandler(StyleCollection_PropertyChanged);
		}


		private void Construct(int capacity) {
			if (capacity > 0)
				internalList = new SortedList<string, StylePair<TStyle>>(capacity);
			else internalList = new SortedList<string, StylePair<TStyle>>();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected class StylePair<T> where T : class, IStyle {

			/// <ToBeCompleted></ToBeCompleted>
			public StylePair(T baseStyle, T previewStyle) {
				this.Style = baseStyle;
				this.PreviewStyle = previewStyle;
			}

			/// <ToBeCompleted></ToBeCompleted>
			public T Style;

			/// <ToBeCompleted></ToBeCompleted>
			public T PreviewStyle;

		}


		/// <summary>
		/// Generic enumerator implementation for the generic StyleCollection.
		/// Iterates through the styles of the StyleCollection.
		/// </summary>
		protected struct StyleCollectionEnumerator : IEnumerator<TStyleInterface> {

			/// <summary>Represents an empty enumerator.</summary>
			public static readonly StyleCollectionEnumerator Empty;

			/// <summary>Constructs a new enumerator instance</summary>
			public static StyleCollectionEnumerator Create(StyleCollection<TStyle, TStyleInterface> collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				StyleCollectionEnumerator result = StyleCollectionEnumerator.Empty;
				result._collection = collection;
				result._enumerator = collection.internalList.GetEnumerator();
				return result;
			}

			/// <summary>Constructs a new enumerator instance</summary>
			public StyleCollectionEnumerator(StyleCollection<TStyle, TStyleInterface> collection) {
				if (collection == null) throw new ArgumentNullException("collection");
				this._collection = collection;
				this._enumerator = collection.internalList.GetEnumerator();
			}


			#region IEnumerator<TStyleInterface> Members

			/// <override></override>
			public TStyleInterface Current {
				get { return (TStyleInterface)_enumerator.Current.Value.Style; }
			}

			#endregion


			#region IDisposable Members

			/// <override></override>
			public void Dispose() {
				_collection = null;
			}

			#endregion


			#region IEnumerator Members

			/// <override></override>
			object IEnumerator.Current {
				get { return Current; }
			}

			/// <override></override>
			public bool MoveNext() {
				return _enumerator.MoveNext();
			}

			/// <override></override>
			public void Reset() {
				_enumerator.MoveNext();
			}

			#endregion

			
			static StyleCollectionEnumerator() {
				Empty._collection = null;
				Empty._enumerator = null;
			}

			private StyleCollection<TStyle, TStyleInterface> _collection;
			private IEnumerator<KeyValuePair<string, StylePair<TStyle>>> _enumerator;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected SortedList<string, StylePair<TStyle>> internalList = null;
	}


	/// <summary>
	/// A collection of <see cref="T:Dataweb.NShape.CapStyle" /> sorted by name.
	/// </summary>
	public class CapStyleCollection : StyleCollection<CapStyle, ICapStyle>, ICapStyles {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.CapStyleCollection" />.
		/// </summary>
		public CapStyleCollection()
			: base(CapStyle.StandardNames.Count) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.CapStyleCollection" />.
		/// </summary>
		public CapStyleCollection(int capacity)
			: base(capacity) {
		}


		/// <override></override>
		public override bool IsStandardStyle(CapStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return CapStyle.StandardNames.EqualsAny(style.Name);
		}


		#region ICapStyles Members

		ICapStyle ICapStyles.this[string name] {
			get { return internalList[name].Style; }
		}


		/// <override></override>
		public ICapStyle None {
			get { return internalList[CapStyle.StandardNames.None].Style; }
		}


		/// <override></override>
		[Obsolete("This property will be removed in future versions. Use ArrowClosed instead.")]
		public ICapStyle Arrow {
			get { return ClosedArrow; }
		}


		/// <override></override>
		public ICapStyle ClosedArrow {
			get { return internalList[CapStyle.StandardNames.ArrowClosed].Style; }
		}


		/// <override></override>
		public ICapStyle OpenArrow {
			get { return internalList[CapStyle.StandardNames.ArrowOpen].Style; }
		}


		/// <override></override>
		public ICapStyle Special1 {
			get { return internalList[CapStyle.StandardNames.Special1].Style; }
		}


		/// <override></override>
		public ICapStyle Special2 {
			get { return internalList[CapStyle.StandardNames.Special2].Style; }
		}

		#endregion

	}


	/// <summary>
	/// A collection of <see cref="T:Dataweb.NShape.CharacterStyle" /> sorted by name.
	/// </summary>
	public class CharacterStyleCollection : StyleCollection<CharacterStyle, ICharacterStyle>, ICharacterStyles {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.CharacterStyleCollection" />.
		/// </summary>
		public CharacterStyleCollection()
			: base(CharacterStyle.StandardNames.Count) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.CharacterStyleCollection" />.
		/// </summary>
		public CharacterStyleCollection(int capacity)
			: base(capacity) {
		}


		/// <override></override>
		public override bool IsStandardStyle(CharacterStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return CharacterStyle.StandardNames.EqualsAny(style.Name);
		}


		#region ICharacterStyles Members

		ICharacterStyle ICharacterStyles.this[string name] {
			get { return this[name]; }
		}


		/// <override></override>
		public ICharacterStyle Normal {
			get { return internalList[CharacterStyle.StandardNames.Normal].Style; }
		}


		/// <override></override>
		public ICharacterStyle Caption {
			get { return internalList[CharacterStyle.StandardNames.Caption].Style; }
		}


		/// <override></override>
		public ICharacterStyle Subtitle {
			get { return internalList[CharacterStyle.StandardNames.Subtitle].Style; }
		}


		/// <override></override>
		public ICharacterStyle Heading3 {
			get { return internalList[CharacterStyle.StandardNames.Heading3].Style; }
		}


		/// <override></override>
		public ICharacterStyle Heading2 {
			get { return internalList[CharacterStyle.StandardNames.Heading2].Style; }
		}


		/// <override></override>
		public ICharacterStyle Heading1 {
			get { return internalList[CharacterStyle.StandardNames.Heading1].Style; }
		}

		#endregion

	}

	
	/// <summary>
	/// A collection of <see cref="T:Dataweb.NShape.ColorStyle" /> sorted by name.
	/// </summary>
	public class ColorStyleCollection : StyleCollection<ColorStyle, IColorStyle>, IColorStyles {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.ColorStyleCollection" />.
		/// </summary>
		public ColorStyleCollection()
			: base(ColorStyle.StandardNames.Count) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.ColorStyleCollection" />.
		/// </summary>
		public ColorStyleCollection(int capacity)
			: base(capacity) {
		}


		/// <override></override>
		public override bool IsStandardStyle(ColorStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return ColorStyle.StandardNames.EqualsAny(style.Name);
		}


		#region IColorStyles Members

		IColorStyle IColorStyles.this[string name] {
			get { return this[name]; }
		}

		/// <override></override>
		public IColorStyle Transparent {
			get { return internalList[ColorStyle.StandardNames.Transparent].Style; }
		}

		/// <override></override>
		public IColorStyle Background {
			get { return internalList[ColorStyle.StandardNames.Background].Style; }
		}

		/// <override></override>
		public IColorStyle Highlight {
			get { return internalList[ColorStyle.StandardNames.Highlight].Style; }
		}

		/// <override></override>
		public IColorStyle Text {
			get { return internalList[ColorStyle.StandardNames.Text].Style; }
		}

		/// <override></override>
		public IColorStyle HighlightText {
			get { return internalList[ColorStyle.StandardNames.HighlightText].Style; }
		}

		/// <override></override>
		public IColorStyle Black {
			get { return internalList[ColorStyle.StandardNames.Black].Style; }
		}

		/// <override></override>
		public IColorStyle White {
			get { return internalList[ColorStyle.StandardNames.White].Style; }
		}

		/// <override></override>
		public IColorStyle Gray {
			get { return internalList[ColorStyle.StandardNames.Gray].Style; }
		}

		/// <override></override>
		public IColorStyle LightGray {
			get { return internalList[ColorStyle.StandardNames.LightGray].Style; }
		}

		/// <override></override>
		public IColorStyle Red {
			get { return internalList[ColorStyle.StandardNames.Red].Style; }
		}

		/// <override></override>
		public IColorStyle LightRed {
			get { return internalList[ColorStyle.StandardNames.LightRed].Style; }
		}

		/// <override></override>
		public IColorStyle Blue {
			get { return internalList[ColorStyle.StandardNames.Blue].Style; }
		}

		/// <override></override>
		public IColorStyle LightBlue {
			get { return internalList[ColorStyle.StandardNames.LightBlue].Style; }
		}

		/// <override></override>
		public IColorStyle Green {
			get { return internalList[ColorStyle.StandardNames.Green].Style; }
		}

		/// <override></override>
		public IColorStyle LightGreen {
			get { return internalList[ColorStyle.StandardNames.LightGreen].Style; }
		}

		/// <override></override>
		public IColorStyle Yellow {
			get { return internalList[ColorStyle.StandardNames.Yellow].Style; }
		}

		/// <override></override>
		public IColorStyle LightYellow {
			get { return internalList[ColorStyle.StandardNames.LightYellow].Style; }
		}

		#endregion

	}


	/// <summary>
	/// A collection of <see cref="T:Dataweb.NShape.FillStyle" /> sorted by name.
	/// </summary>
	public class FillStyleCollection : StyleCollection<FillStyle, IFillStyle>, IFillStyles {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.FillStyleCollection" />.
		/// </summary>
		public FillStyleCollection()
			: base(FillStyle.StandardNames.Count) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.FillStyleCollection" />.
		/// </summary>
		public FillStyleCollection(int capacity)
			: base(capacity) {
		}


		/// <override></override>
		public override bool IsStandardStyle(FillStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return FillStyle.StandardNames.EqualsAny(style.Name);
		}


		#region IFillStyles Members

		IFillStyle IFillStyles.this[string name] {
			get { return this[name]; }
		}

		/// <override></override>
		public IFillStyle Transparent {
			get { return internalList[FillStyle.StandardNames.Transparent].Style; }
		}

		/// <override></override>
		public IFillStyle Black {
			get { return internalList[FillStyle.StandardNames.Black].Style; }
		}

		/// <override></override>
		public IFillStyle White {
			get { return internalList[FillStyle.StandardNames.White].Style; }
		}

		/// <override></override>
		public IFillStyle Red {
			get { return internalList[FillStyle.StandardNames.Red].Style; }
		}

		/// <override></override>
		public IFillStyle Blue {
			get { return internalList[FillStyle.StandardNames.Blue].Style; }
		}

		/// <override></override>
		public IFillStyle Green {
			get { return internalList[FillStyle.StandardNames.Green].Style; }
		}

		/// <override></override>
		public IFillStyle Yellow {
			get { return internalList[FillStyle.StandardNames.Yellow].Style; }
		}

		#endregion

	}


	/// <summary>
	/// A collection of <see cref="T:Dataweb.NShape.LineStyle" /> sorted by name.
	/// </summary>
	public class LineStyleCollection : StyleCollection<LineStyle, ILineStyle>, ILineStyles {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.LineStyleCollection" />.
		/// </summary>
		public LineStyleCollection()
			: base(LineStyle.StandardNames.Count) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.LineStyleCollection" />.
		/// </summary>
		public LineStyleCollection(int capacity)
			: base(capacity) {
		}


		/// <override></override>
		public override bool IsStandardStyle(LineStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return LineStyle.StandardNames.EqualsAny(style.Name);
		}


		#region ILineStyles Members

		ILineStyle ILineStyles.this[string name] {
			get { return this[name]; }
		}

		/// <override></override>
		public ILineStyle None {
			get { return internalList[LineStyle.StandardNames.None].Style; }
		}

		/// <override></override>
		public ILineStyle Normal {
			get { return internalList[LineStyle.StandardNames.Normal].Style; }
		}

		/// <override></override>
		public ILineStyle Thick {
			get { return internalList[LineStyle.StandardNames.Thick].Style; }
		}

		/// <override></override>
		public ILineStyle Dotted {
			get { return internalList[LineStyle.StandardNames.Dotted].Style; }
		}

		/// <override></override>
		public ILineStyle Dashed {
			get { return internalList[LineStyle.StandardNames.Dashed].Style; }
		}

		/// <override></override>
		public ILineStyle Highlight {
			get { return internalList[LineStyle.StandardNames.Highlight].Style; }
		}

		/// <override></override>
		public ILineStyle HighlightThick {
			get { return internalList[LineStyle.StandardNames.HighlightThick].Style; }
		}

		/// <override></override>
		public ILineStyle HighlightDotted {
			get { return internalList[LineStyle.StandardNames.HighlightDotted].Style; }
		}

		/// <override></override>
		public ILineStyle HighlightDashed {
			get { return internalList[LineStyle.StandardNames.HighlightDashed].Style; }
		}

		/// <override></override>
		public ILineStyle Red {
			get { return internalList[LineStyle.StandardNames.Red].Style; }
		}

		/// <override></override>
		public ILineStyle Blue {
			get { return internalList[LineStyle.StandardNames.Blue].Style; }
		}

		/// <override></override>
		public ILineStyle Green {
			get { return internalList[LineStyle.StandardNames.Green].Style; }
		}

		/// <override></override>
		public ILineStyle Yellow {
			get { return internalList[LineStyle.StandardNames.Yellow].Style; }
		}

		/// <override></override>
		public ILineStyle Special1 {
			get { return internalList[LineStyle.StandardNames.Special1].Style; }
		}

		/// <override></override>
		public ILineStyle Special2 {
			get { return internalList[LineStyle.StandardNames.Special2].Style; }
		}

		#endregion

	}


	/// <summary>
	/// A collection of <see cref="T:Dataweb.NShape.ParagraphStyle" /> sorted by name.
	/// </summary>
	public class ParagraphStyleCollection : StyleCollection<ParagraphStyle, IParagraphStyle>, IParagraphStyles {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.ParagraphStyleCollection" />.
		/// </summary>
		public ParagraphStyleCollection()
			: base(ParagraphStyle.StandardNames.Count) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.ParagraphStyleCollection" />.
		/// </summary>
		public ParagraphStyleCollection(int capacity)
			: base(capacity) {
		}


		/// <override></override>
		public override bool IsStandardStyle(ParagraphStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			return ParagraphStyle.StandardNames.EqualsAny(style.Name);
		}


		#region IParagraphStyles Members

		IParagraphStyle IParagraphStyles.this[string name] {
			get { return this[name]; }
		}

		/// <override></override>
		public IParagraphStyle Label {
			get { return internalList[ParagraphStyle.StandardNames.Label].Style; }
		}

		/// <override></override>
		public IParagraphStyle Text {
			get { return internalList[ParagraphStyle.StandardNames.Text].Style; }
		}

		/// <override></override>
		public IParagraphStyle Title {
			get { return internalList[ParagraphStyle.StandardNames.Title].Style; }
		}

		#endregion

	}

	#endregion

}
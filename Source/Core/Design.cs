/******************************************************************************
  Copyright 2009-2017 dataweb GmbH
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
using System.Drawing.Drawing2D;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape {

	/// <summary>
	/// Provides a collection of styles implementing <see cref="T:Dataweb.NShape.ICapStyle" /> and direct access to the standard styles.
	/// </summary>
	public interface ICapStyles : IEnumerable<ICapStyle> {

		/// <summary>
		/// Provides read only access to the collections elements by the style name.
		/// </summary>
		ICapStyle this[string name] { get; }

		/// <summary>No line cap.</summary>
		ICapStyle None { get; }

		/// <summary>Arrow shaped line cap.</summary>
		[Obsolete("This property will be removed in future versions. Use ClosedArrow instead.")]
		ICapStyle Arrow { get; }

		/// <summary>Arrow shaped line cap.</summary>
		ICapStyle ClosedArrow { get; }

		/// <summary>Arrow shaped line cap.</summary>
		ICapStyle OpenArrow { get; }

		/// <summary>Special line cap #1.</summary>
		ICapStyle Special1 { get; }

		/// <summary>Special line cap #2.</summary>
		ICapStyle Special2 { get; }

	}


	/// <summary>
	/// Provides a collection of styles implementing <see cref="T:Dataweb.NShape.ICharacterStyle" /> and direct access to the standard styles.
	/// </summary>
	public interface ICharacterStyles : IEnumerable<ICharacterStyle> {

		/// <summary>
		/// Provides read only access to the collections elements by the style name.
		/// </summary>
		ICharacterStyle this[string name] { get; }

		/// <summary>Character style for captions.</summary>
		ICharacterStyle Caption { get; }

		/// <summary>Character style for level 1 (top level) headings.</summary>
		ICharacterStyle Heading1 { get; }

		/// <summary>Character style for level 2 headings.</summary>
		ICharacterStyle Heading2 { get; }

		/// <summary>Character style for level 3 headings.</summary>
		ICharacterStyle Heading3 { get; }

		/// <summary>Character style for normal text.</summary>
		ICharacterStyle Normal { get; }

		/// <summary>Character style for subtitles.</summary>
		ICharacterStyle Subtitle { get; }

	}


	/// <summary>
	/// Provides a collection of styles implementing <see cref="T:Dataweb.NShape.IColorStyle" /> and direct access to the standard styles.
	/// </summary>
	public interface IColorStyles : IEnumerable<IColorStyle> {

		/// <summary>
		/// Provides read only access to the collections elements by the style name.
		/// </summary>
		IColorStyle this[string name] { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle Background { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle Black { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle Blue { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle Gray { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle Green { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle Highlight { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle HighlightText { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle LightBlue { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle LightGray { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle LightGreen { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle LightRed { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle LightYellow { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle Red { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle Text { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle Transparent { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle White { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IColorStyle Yellow { get; }

	}


	/// <summary>
	/// Provides a collection of styles implementing <see cref="T:Dataweb.NShape.IFillStyle" /> and direct access to the standard styles.
	/// </summary>
	public interface IFillStyles : IEnumerable<IFillStyle> {

		/// <summary>
		/// Provides read only access to the collections elements by the style name.
		/// </summary>
		IFillStyle this[string name] { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IFillStyle Black { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IFillStyle Blue { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IFillStyle Green { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IFillStyle Red { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IFillStyle Transparent { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IFillStyle White { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IFillStyle Yellow { get; }

	}


	/// <summary>
	/// Provides a collection of styles implementing <see cref="T:Dataweb.NShape.ILineStyle" /> and direct access to the standard styles.
	/// </summary>
	public interface ILineStyles : IEnumerable<ILineStyle> {

		/// <summary>
		/// Provides read only access to the collections elements by the style name.
		/// </summary>
		ILineStyle this[string name] { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle Blue { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle Dashed { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle Dotted { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle Green { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle Highlight { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle HighlightDashed { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle HighlightDotted { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle HighlightThick { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle None { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle Normal { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle Red { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle Special1 { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle Special2 { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle Thick { get; }

		/// <ToBeCompleted></ToBeCompleted>
		ILineStyle Yellow { get; }

	}


	/// <summary>
	/// Provides a collection of styles implementing <see cref="T:Dataweb.NShape.IParagraphStyle" /> and direct access to the standard styles.
	/// </summary>
	public interface IParagraphStyles : IEnumerable<IParagraphStyle> {

		/// <summary>
		/// Provides read only access to the collections elements by the style name.
		/// </summary>
		IParagraphStyle this[string name] { get; }

		// Standard Style Properties
		/// <ToBeCompleted></ToBeCompleted>
		IParagraphStyle Label { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IParagraphStyle Text { get; }

		/// <ToBeCompleted></ToBeCompleted>
		IParagraphStyle Title { get; }

	}


	/// <summary>
	/// Defines a set of styles.
	/// </summary>
	public interface IStyleSet {

		/// <summary>Provides access to a style collection implementing the <see cref="T:Dataweb.NShape.ICapStyles" /> interface.</summary>
		ICapStyles CapStyles { get; }

		/// <summary>Provides access to a style collection implementing the <see cref="T:Dataweb.NShape.ICharacterStyles" /> interface.</summary>
		ICharacterStyles CharacterStyles { get; }

		/// <summary>Provides access to a style collection implementing the <see cref="T:Dataweb.NShape.IColorStyles" /> interface.</summary>
		IColorStyles ColorStyles { get; }

		/// <summary>Provides access to a style collection implementing the <see cref="T:Dataweb.NShape.IFillStyles" /> interface.</summary>
		IFillStyles FillStyles { get; }

		/// <summary>Provides access to a style collection implementing the <see cref="T:Dataweb.NShape.ILineStyles" /> interface.</summary>
		ILineStyles LineStyles { get; }

		/// <summary>Provides access to a style collection implementing the <see cref="T:Dataweb.NShape.IParagraphStyles" /> interface.</summary>
		IParagraphStyles ParagraphStyles { get; }

		/// <summary>Gets a preview style associated with the given style.</summary>
		ICapStyle GetPreviewStyle(ICapStyle colorStyle);

		/// <summary>Gets a preview style associated with the given style.</summary>
		ICharacterStyle GetPreviewStyle(ICharacterStyle colorStyle);

		/// <summary>Gets a preview style associated with the given style.</summary>
		IColorStyle GetPreviewStyle(IColorStyle colorStyle);

		/// <summary>Gets a preview style associated with the given style.</summary>
		IFillStyle GetPreviewStyle(IFillStyle fillStyle);

		/// <summary>Gets a preview style associated with the given style.</summary>
		ILineStyle GetPreviewStyle(ILineStyle lineStyle);

		/// <summary>Gets a preview style associated with the given style.</summary>
		IParagraphStyle GetPreviewStyle(IParagraphStyle colorStyle);

	}


	/// <summary>
	/// Defines a set of styles for shapes.
	/// </summary>
	[TypeDescriptionProvider(typeof(TypeDescriptionProviderDg))]
	public class Design : IStyleSet, IEntity {

		/// <summary>
		/// Creates an empty <see cref="T:Dataweb.NShape.Design" /> for subsequent loading from the <see cref="T:Dataweb.NShape.Advanced.IRepository" />.
		/// </summary>
		internal Design() {
			_name = string.Empty;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Design" />. It already includes standard styles.
		/// </summary>
		public Design(string name)
			: this() {
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			this._name = name;
			CreateStandardStyles();
		}


		#region [Explicit] IEntity Members

		/// <summary>
		/// The entity type name of <see cref="T:Dataweb.NShape.Design" />.
		/// </summary>
		public static string EntityTypeName {
			get { return "Core.Design"; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Design" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
			if (version >= 4) yield return new EntityFieldDefinition("Title", typeof(string));
			yield return new EntityFieldDefinition("Description", typeof(string));
		}


		object IEntity.Id {
			get { return _id; }
		}


		void IEntity.AssignId(object id) {
			if (id == null)
				throw new ArgumentNullException("id");
			if (this._id != null)
				throw new InvalidOperationException("Design has already an id.");
			this._id = id;
		}


		void IEntity.LoadFields(IRepositoryReader reader, int version) {
			_name = reader.ReadString();
			if (version >= 4) _title = reader.ReadString();
			_description = reader.ReadString();
		}


		void IEntity.SaveFields(IRepositoryWriter writer, int version) {
			writer.WriteString(_name);
			if (version >= 4) writer.WriteString(_title);
			writer.WriteString(_description);
		}


		void IEntity.LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			// nothing to do
		}


		void IEntity.SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			// nothing to do
		}


		void IEntity.Delete(IRepositoryWriter writer, int version) {
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}

		#endregion


		#region [Public] IStyleSet Members

		/// <ToBeCompleted></ToBeCompleted>
		public ICapStyle GetPreviewStyle(ICapStyle capStyle) {
			if (capStyle == null) throw new ArgumentNullException("capStyle");
			return _capStyles.GetPreviewStyle(capStyle.Name);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ICharacterStyle GetPreviewStyle(ICharacterStyle characterStyle) {
			if (characterStyle == null) throw new ArgumentNullException("characterStyle");
			return _characterStyles.GetPreviewStyle(characterStyle.Name);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IColorStyle GetPreviewStyle(IColorStyle colorStyle) {
			if (colorStyle == null) throw new ArgumentNullException("colorStyle");
			return _colorStyles.GetPreviewStyle(colorStyle.Name);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IFillStyle GetPreviewStyle(IFillStyle fillStyle) {
			if (fillStyle == null) throw new ArgumentNullException("fillStyle");
			return _fillStyles.GetPreviewStyle(fillStyle.Name);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ILineStyle GetPreviewStyle(ILineStyle lineStyle) {
			if (lineStyle == null) throw new ArgumentNullException("lineStyle");
			return _lineStyles.GetPreviewStyle(lineStyle.Name);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IParagraphStyle GetPreviewStyle(IParagraphStyle paragraphStyle) {
			if (paragraphStyle == null) throw new ArgumentNullException("paragraphStyle");
			return _paragraphStyles.GetPreviewStyle(paragraphStyle.Name);
		}


		ICapStyles IStyleSet.CapStyles {
			get { return _capStyles; }
		}


		ICharacterStyles IStyleSet.CharacterStyles {
			get { return _characterStyles; }
		}


		IColorStyles IStyleSet.ColorStyles {
			get { return _colorStyles; }
		}


		IFillStyles IStyleSet.FillStyles {
			get { return _fillStyles; }
		}


		ILineStyles IStyleSet.LineStyles {
			get { return _lineStyles; }
		}


		IParagraphStyles IStyleSet.ParagraphStyles {
			get { return _paragraphStyles; }
		}

		#endregion


		#region [Public] Properties

		/// <summary>
		/// The name of the <see cref="T:Dataweb.NShape.Design" />.
		/// </summary>
		public string Name {
			get { return _name; }
			set { _name = value; }
		}


		/// <summary>
		/// The title of the <see cref="T:Dataweb.NShape.Design" />.
		/// </summary>
		public string Title {
			get { return string.IsNullOrEmpty(_title) ? _name : _title; }
			set {
				if (value == _name || string.IsNullOrEmpty(value))
					_title = null;
				else _title = value;
			}
		}


		/// <summary>
		/// Returns all <see cref="T:Dataweb.NShape.IStyle" /> instances stored in the <see cref="T:Dataweb.NShape.Design" /> regardless of the style category.
		/// </summary>
		public IEnumerable<IStyle> Styles {
			get {
				foreach (IStyle s in _colorStyles) yield return s;
				foreach (IStyle s in _capStyles) yield return s;
				foreach (IStyle s in _lineStyles) yield return s;
				foreach (IStyle s in _fillStyles) yield return s;
				foreach (IStyle s in _characterStyles) yield return s;
				foreach (IStyle s in _paragraphStyles) yield return s;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CapStyleCollection CapStyles {
			get { return _capStyles; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CharacterStyleCollection CharacterStyles {
			get { return _characterStyles; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ColorStyleCollection ColorStyles {
			get { return _colorStyles; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public FillStyleCollection FillStyles {
			get { return _fillStyles; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LineStyleCollection LineStyles {
			get { return _lineStyles; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ParagraphStyleCollection ParagraphStyles {
			get { return _paragraphStyles; }
		}

		#endregion


		#region [Public] Methods

		/// <override></override>
		public override string ToString() {
			return Title;
		}


		/// <summary>
		/// Clears all style collections of the design
		/// </summary>
		public void Clear() {
			// Clear user defined styles
			_paragraphStyles.Clear();
			_lineStyles.Clear();
			_characterStyles.Clear();
			_fillStyles.Clear();
			_capStyles.Clear();
			_colorStyles.Clear();
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool ContainsStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			if (style is CapStyle)
				return _capStyles.Contains((CapStyle)style);
			else if (style is CharacterStyle)
				return _characterStyles.Contains((CharacterStyle)style);
			else if (style is ColorStyle)
				return _colorStyles.Contains((ColorStyle)style);
			else if (style is FillStyle)
				return _fillStyles.Contains((FillStyle)style);
			else if (style is LineStyle)
				return _lineStyles.Contains((LineStyle)style);
			else if (style is ParagraphStyle)
				return _paragraphStyles.Contains((ParagraphStyle)style);
			else throw new NShapeUnsupportedValueException(style);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool IsStandardStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			if (style is CapStyle)
				return _capStyles.IsStandardStyle((CapStyle)style);
			else if (style is CharacterStyle)
				return _characterStyles.IsStandardStyle((CharacterStyle)style);
			else if (style is ColorStyle)
				return _colorStyles.IsStandardStyle((ColorStyle)style);
			else if (style is FillStyle)
				return _fillStyles.IsStandardStyle((FillStyle)style);
			else if (style is LineStyle)
				return _lineStyles.IsStandardStyle((LineStyle)style);
			else if (style is ParagraphStyle)
				return _paragraphStyles.IsStandardStyle((ParagraphStyle)style);
			else throw new NShapeUnsupportedValueException(style);
		}


		/// <summary>
		/// Returns the style of the same type with the same name if there is one in the design's style collection.
		/// </summary>
		public IStyle FindMatchingStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			if (style is ColorStyle) {
				if (_colorStyles.Contains(style.Name))
					return _colorStyles[style.Name];
				else return null;
			} else if (style is CapStyle) {
				if (_capStyles.Contains(style.Name))
					return _capStyles[style.Name];
				else return null;
			} else if (style is FillStyle) {
				if (_fillStyles.Contains(style.Name))
					return _fillStyles[style.Name];
				else return null;
			} else if (style is CharacterStyle) {
				if (_characterStyles.Contains(style.Name))
					return _characterStyles[style.Name];
				else return null;
			} else if (style is LineStyle) {
				if (_lineStyles.Contains(style.Name))
					return _lineStyles[style.Name];
				else return null;
			} else if (style is ParagraphStyle) {
				if (_paragraphStyles.Contains(style.Name))
					return _paragraphStyles[style.Name];
				else return null;
			} else throw new NShapeUnsupportedValueException(style);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IStyle FindStyleByName(string name, Type styleType) {
			if (name == null) throw new ArgumentNullException("name");
			if (styleType == null) throw new ArgumentNullException("styleType");
			return DoFindStyleByName(name, styleType);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void AddStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertValidStyle(style);
			if (style is CapStyle) {
				_capStyles.Add((CapStyle)style, CreatePreviewStyle((ICapStyle)style));
			} else if (style is CharacterStyle) {
				_characterStyles.Add((CharacterStyle)style, CreatePreviewStyle((ICharacterStyle)style));
			} else if (style is ColorStyle) {
				_colorStyles.Add((ColorStyle)style, CreatePreviewStyle((IColorStyle)style));
			} else if (style is FillStyle) {
				_fillStyles.Add((FillStyle)style, CreatePreviewStyle((IFillStyle)style));
			} else if (style is LineStyle) {
				_lineStyles.Add((LineStyle)style, CreatePreviewStyle((ILineStyle)style));
			} else if (style is ParagraphStyle) {
				_paragraphStyles.Add((ParagraphStyle)style, CreatePreviewStyle((IParagraphStyle)style));
			} else throw new NShapeUnsupportedValueException(style);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void RemoveStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			if (style is CapStyle)
				_capStyles.Remove((CapStyle)style);
			else if (style is CharacterStyle)
				_characterStyles.Remove((CharacterStyle)style);
			else if (style is ColorStyle)
				_colorStyles.Remove((ColorStyle)style);
			else if (style is FillStyle)
				_fillStyles.Remove((FillStyle)style);
			else if (style is LineStyle)
				_lineStyles.Remove((LineStyle)style);
			else if (style is ParagraphStyle)
				_paragraphStyles.Remove((ParagraphStyle)style);
			else throw new NShapeUnsupportedValueException(style);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void RemoveStyle(string name, Type styleType) {
			if (name == null) throw new ArgumentNullException("name");
			if (styleType == null) throw new ArgumentNullException("styleType");
			if (styleType == typeof(CapStyle))
				_capStyles.Remove(name);
			else if (styleType == typeof(CharacterStyle))
				_characterStyles.Remove(name);
			else if (styleType == typeof(ColorStyle))
				_colorStyles.Remove(name);
			else if (styleType == typeof(FillStyle))
				_fillStyles.Remove(name);
			else if (styleType == typeof(LineStyle))
				_lineStyles.Remove(name);
			else if (styleType == typeof(ParagraphStyle))
				_paragraphStyles.Remove(name);
			else throw new NShapeUnsupportedValueException(styleType);
		}


		/// <summary>
		/// Assigns the given style to the existing style with the same name. 
		/// If there is not style with such a projectName, a new style is created.
		/// This method also takes care about preview styles.
		/// </summary>
		/// <param name="style">The style that should be assigned to an existing style.</param>
		/// <returns>Returns true if an existring style was assigned and false if there was no matching style.</returns>
		public bool AssignStyle(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertValidStyle(style);
			bool styleFound = ContainsStyle(style);
			if (styleFound) {
				Type styleType = style.GetType();
				Style existingStyle = DoFindStyleByName(_name, styleType);
				existingStyle.Assign(style, this.FindMatchingStyle);
				CreateAndSetPreviewStyle(existingStyle);
			} else {
				Style newStyle = new CapStyle(style.Name);
				newStyle.Assign(style, this.FindMatchingStyle);
				AddStyle(newStyle);
			}
			return styleFound;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CapStyle CreatePreviewStyle(ICapStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			CapStyle result = new CapStyle(baseStyle.Name);
			result.Title = baseStyle.Name + _previewNameSuffix;
			result.CapShape = baseStyle.CapShape;
			result.CapSize = baseStyle.CapSize;
			if (baseStyle.ColorStyle != null)
				result.ColorStyle = CreatePreviewStyle(baseStyle.ColorStyle);
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ColorStyle CreatePreviewStyle(IColorStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			ColorStyle result = new ColorStyle(baseStyle.Name);
			result.Title = baseStyle.Name + _previewNameSuffix;
			result.Color = baseStyle.Color;
			result.Transparency = GetPreviewTransparency(baseStyle.Transparency);
			result.ConvertToGray = _previewAsGrayScale;
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public FillStyle CreatePreviewStyle(IFillStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			FillStyle result = new FillStyle(baseStyle.Name, ColorStyle.Empty, ColorStyle.Empty);
			result.Title = baseStyle.Name + _previewNameSuffix;
			if (baseStyle.AdditionalColorStyle != null)
				result.AdditionalColorStyle = CreatePreviewStyle(baseStyle.AdditionalColorStyle);
			if (baseStyle.BaseColorStyle != null)
				result.BaseColorStyle = CreatePreviewStyle(baseStyle.BaseColorStyle);
			result.ConvertToGrayScale = _previewAsGrayScale;
			result.FillMode = baseStyle.FillMode;
			result.FillPattern = baseStyle.FillPattern;

			int newSize = 512;
			if (baseStyle.Image != null && (baseStyle.Image.Width > 2 * newSize || baseStyle.Image.Height > newSize)) {
				float scale = Geometry.CalcScaleFactor(
					baseStyle.Image.Width,
					baseStyle.Image.Height,
					baseStyle.Image.Width / Math.Max(1, (baseStyle.Image.Width / newSize)),
					baseStyle.Image.Height / Math.Max(1, (baseStyle.Image.Height / newSize)));
				int width = (int)Math.Round(baseStyle.Image.Width * scale);
				int height = (int)Math.Round(baseStyle.Image.Height * scale);
				NamedImage namedImg = new NamedImage();
				namedImg.Image = baseStyle.Image.Image.GetThumbnailImage(width, height, null, IntPtr.Zero);
				namedImg.Name = baseStyle.Image.Name;
				result.Image = namedImg;
			} else result.Image = baseStyle.Image;

			result.ImageGammaCorrection = baseStyle.ImageGammaCorrection;
			result.ImageLayout = baseStyle.ImageLayout;
			result.ImageTransparency = GetPreviewTransparency(baseStyle.ImageTransparency);
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CharacterStyle CreatePreviewStyle(ICharacterStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			CharacterStyle result = new CharacterStyle(baseStyle.Name);
			result.Title = baseStyle.Name + _previewNameSuffix;
			if (baseStyle.ColorStyle != null)
				result.ColorStyle = CreatePreviewStyle(baseStyle.ColorStyle);
			result.FontName = baseStyle.FontName;
			result.SizeInPoints = baseStyle.SizeInPoints;
			result.Style = baseStyle.Style;
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LineStyle CreatePreviewStyle(ILineStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			LineStyle result = new LineStyle(baseStyle.Name);
			result.Title = baseStyle.Name + _previewNameSuffix;
			if (baseStyle.ColorStyle != null)
				result.ColorStyle = CreatePreviewStyle(baseStyle.ColorStyle);
			result.DashCap = baseStyle.DashCap;
			result.DashType = baseStyle.DashType;
			result.LineJoin = baseStyle.LineJoin;
			result.LineWidth = baseStyle.LineWidth;
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ParagraphStyle CreatePreviewStyle(IParagraphStyle baseStyle) {
			if (baseStyle == null) throw new ArgumentNullException("baseStyle");
			ParagraphStyle result = new ParagraphStyle(baseStyle.Name);
			result.Title = baseStyle.Name + _previewNameSuffix;
			result.Alignment = baseStyle.Alignment;
			result.Padding = baseStyle.Padding;
			result.Trimming = baseStyle.Trimming;
			return result;
		}

		#endregion


		internal static bool PreviewsAsGrayScale {
			get { return _previewAsGrayScale; }
		}


		internal static Byte GetPreviewTransparency(byte baseTransparency) {
			int result = baseTransparency + (int)Math.Round((100 - baseTransparency) * _previewTransparencyFactor);
			if (result < 0) result = 0;
			else if (result > 100) result = 100;
			return Convert.ToByte(result);
		}


		internal void CreateStandardStyles() {
			// Create standard styles
			CreateStandardColorStyles();
			CreateStandardCapStyles();
			CreateStandardCharacterStyles();
			CreateStandardFillStyles();
			CreateStandardLineStyles();
			CreateStandardParagraphStyles();
		}


		private Style DoFindStyleByName(string name, Type styleType) {
			if (IsOfType(styleType, typeof(ICapStyle)))
				return _capStyles.Contains(name) ? _capStyles[name] : null;
			else if (IsOfType(styleType, typeof(ICharacterStyle)))
				return _characterStyles.Contains(name) ? _characterStyles[name] : null;
			else if (IsOfType(styleType, typeof(IColorStyle)))
				return _colorStyles.Contains(name) ? _colorStyles[name] : null;
			else if (IsOfType(styleType, typeof(IFillStyle)))
				return _fillStyles.Contains(name) ? _fillStyles[name] : null;
			else if (IsOfType(styleType, typeof(ILineStyle)))
				return _lineStyles.Contains(name) ? _lineStyles[name] : null;
			else if (styleType == typeof(ParagraphStyle))
				return _paragraphStyles.Contains(name) ? _paragraphStyles[name] : null;
			else throw new NShapeException("Unexpected style type '{0}'.", styleType.Name);
		}


		private bool IsOfType(Type objectType, Type targetType) {
			return objectType == targetType
				|| objectType.IsSubclassOf(targetType)
				|| objectType.GetInterface(targetType.Name, true) != null;
		}


		private void CreateAndSetPreviewStyle(Style baseStyle) {
			if (baseStyle is CapStyle) {
				CapStyle style = (CapStyle)baseStyle;
				CapStyle previewStyle = CreatePreviewStyle(style);
				_capStyles.SetPreviewStyle(style, previewStyle);
			} else if (baseStyle is CharacterStyle) {
				CharacterStyle style = (CharacterStyle)baseStyle;
				CharacterStyle previewStyle = CreatePreviewStyle(style);
				_characterStyles.SetPreviewStyle(style, previewStyle);
			} else if (baseStyle is ColorStyle) {
				ColorStyle style = (ColorStyle)baseStyle;
				ColorStyle previewStyle = CreatePreviewStyle(style);
				_colorStyles.SetPreviewStyle(style, previewStyle);
			} else if (baseStyle is FillStyle) {
				FillStyle style = (FillStyle)baseStyle;
				FillStyle previewStyle = CreatePreviewStyle(style);
				_fillStyles.SetPreviewStyle(style, previewStyle);
			} else if (baseStyle is LineStyle) {
				LineStyle style = (LineStyle)baseStyle;
				LineStyle previewStyle = CreatePreviewStyle(style);
				_lineStyles.SetPreviewStyle(style, previewStyle);
			} else if (baseStyle is ParagraphStyle) {
				ParagraphStyle style = (ParagraphStyle)baseStyle;
				ParagraphStyle previewStyle = CreatePreviewStyle(style);
				_paragraphStyles.SetPreviewStyle(style, previewStyle);
			} else throw new NShapeUnsupportedValueException(baseStyle);
		}


		#region Assert style validity

		private void AssertValidStyle(IStyle style) {
			if (style is ICapStyle) AssertValidStyle((ICapStyle)style);
			else if (style is ICharacterStyle) AssertValidStyle((ICharacterStyle)style);
			else if (style is IColorStyle) AssertValidStyle((IColorStyle)style);
			else if (style is IFillStyle) AssertValidStyle((IFillStyle)style);
			else if (style is ILineStyle) AssertValidStyle((ILineStyle)style);
			else if (style is IParagraphStyle) AssertValidStyle((IParagraphStyle)style);
			else Debug.Fail("Unhandled style class!");
		}


		private void AssertValidStyle(ICapStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertStyleExists(style, style.ColorStyle);
		}


		private void AssertValidStyle(ICharacterStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertStyleExists(style, style.ColorStyle);
		}


		private void AssertValidStyle(IColorStyle style) {
			if (style == null) throw new ArgumentNullException("style");
		}


		private void AssertValidStyle(IFillStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			if (style.FillMode == FillMode.Gradient || style.FillMode == FillMode.Pattern) {
				AssertStyleNotEmpty(style, style.AdditionalColorStyle);
				AssertStyleExists(style, style.AdditionalColorStyle);
			}
			if (style.FillMode != FillMode.Image) {
				AssertStyleNotEmpty(style, style.BaseColorStyle);
				AssertStyleExists(style, style.BaseColorStyle);
			}
		}


		private void AssertValidStyle(ILineStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertStyleExists(style, style.ColorStyle);
		}


		private void AssertValidStyle(IParagraphStyle style) {
			if (style == null) throw new ArgumentNullException("style");
		}


		private void AssertStyleNotEmpty(IStyle styleToAdd, IStyle requiredStyle) {
			if (requiredStyle.Name == Style.EmptyStyleName)
				throw new NShapeException(GetErrorMessageStyleIsEmpty(styleToAdd, requiredStyle));
		}


		private void AssertStyleExists(IStyle styleToAdd, IStyle requiredStyle) {
			if (requiredStyle.Name != Style.EmptyStyleName && !ContainsStyle(requiredStyle))
				throw new NShapeException(GetErrorMessageStyleDoesNotExist(styleToAdd, requiredStyle));
		}


		private string GetErrorMessageCannotAddStyle(IStyle styleToAdd) {
			return string.Format(
				Dataweb.NShape.Properties.Resources.MessageFmt_CannotAdd01ToDesign2, 
				styleToAdd.GetType().Name, styleToAdd.Title, this.Name
			);
		}


		private string GetErrorMessageStyleIsEmpty(IStyle styleToAdd, IStyle requiredStyle) {
			return string.Format(
				Dataweb.NShape.Properties.Resources.MessageFmt_0Used1IsEmptyNotDefined,
				GetErrorMessageCannotAddStyle(styleToAdd), requiredStyle.GetType().Name, requiredStyle.Title
			);
		}


		private string GetErrorMessageStyleDoesNotExist(IStyle styleToAdd, IStyle requiredStyle) {
			return string.Format(
				Dataweb.NShape.Properties.Resources.MessageFmt_0Used12DoesNotExistInDesign3, 
				GetErrorMessageCannotAddStyle(styleToAdd), requiredStyle.GetType().Name, requiredStyle.Title, this.Name
			);
		}

		#endregion


		#region Creating Standard Styles

		private void CreateStandardColorStyles() {
			ColorStyle colorStyle;

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Background, Color.Silver);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Black, Color.Black);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Blue, Color.SteelBlue);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Gray, Color.Gray);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Green, Color.SeaGreen);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Highlight, Color.DarkOrange);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.HighlightText, Color.Navy);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.LightBlue, Color.LightSteelBlue);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.LightGray, Color.LightGray);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.LightGreen, Color.DarkSeaGreen);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.LightRed, Color.LightSalmon);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.LightYellow, Color.LightGoldenrodYellow);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Red, Color.Firebrick);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Text, Color.Black);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Transparent, Color.Transparent);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.White, Color.White);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));

			colorStyle = new ColorStyle(ColorStyle.StandardNames.Yellow, Color.Gold);
			_colorStyles.Add(colorStyle, CreatePreviewStyle(colorStyle));
		}


		private void CreateStandardCapStyles() {
			CapStyle capStyle;

			capStyle = new CapStyle(CapStyle.StandardNames.None);
			capStyle.CapShape = CapShape.None;
			capStyle.CapSize = 12;
			capStyle.ColorStyle = _colorStyles.White;
			_capStyles.Add(capStyle, CreatePreviewStyle(capStyle));

			capStyle = new CapStyle(CapStyle.StandardNames.ArrowClosed);
			capStyle.CapShape = CapShape.ClosedArrow;
			capStyle.CapSize = 12;
			capStyle.ColorStyle = _colorStyles.Black;
			_capStyles.Add(capStyle, CreatePreviewStyle(capStyle));

			capStyle = new CapStyle(CapStyle.StandardNames.ArrowOpen);
			capStyle.CapShape = CapShape.OpenArrow;
			capStyle.CapSize = 12;
			capStyle.ColorStyle = _colorStyles.Black;
			_capStyles.Add(capStyle, CreatePreviewStyle(capStyle));

			capStyle = new CapStyle(CapStyle.StandardNames.Special1);
			capStyle.CapShape = CapShape.Circle;
			capStyle.CapSize = 6;
			capStyle.ColorStyle = _colorStyles.White;
			_capStyles.Add(capStyle, CreatePreviewStyle(capStyle));

			capStyle = new CapStyle(CapStyle.StandardNames.Special2);
			capStyle.CapShape = CapShape.Diamond;
			capStyle.CapSize = 6;
			capStyle.ColorStyle = _colorStyles.White;
			_capStyles.Add(capStyle, CreatePreviewStyle(capStyle));
		}


		private void CreateStandardCharacterStyles() {
			CharacterStyle charStyle;

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Caption);
			charStyle.ColorStyle = _colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 10;
			charStyle.Style = FontStyle.Regular;
			_characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Heading1);
			charStyle.ColorStyle = _colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 36;
			charStyle.Style = FontStyle.Bold;
			_characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Heading2);
			charStyle.ColorStyle = _colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 24;
			charStyle.Style = FontStyle.Bold;
			_characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Heading3);
			charStyle.ColorStyle = _colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 16;
			charStyle.Style = FontStyle.Bold;
			_characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Normal);
			charStyle.ColorStyle = _colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 12;
			charStyle.Style = FontStyle.Regular;
			_characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));

			charStyle = new CharacterStyle(CharacterStyle.StandardNames.Subtitle);
			charStyle.ColorStyle = _colorStyles.Text;
			charStyle.FontName = "Tahoma";
			charStyle.SizeInPoints = 12;
			charStyle.Style = FontStyle.Bold;
			_characterStyles.Add(charStyle, CreatePreviewStyle(charStyle));
		}


		private void CreateStandardFillStyles() {
			FillStyle fillStyle;

			fillStyle = new FillStyle(FillStyle.StandardNames.Black, _colorStyles.Black, _colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			_fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.Blue, _colorStyles.Blue, _colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			_fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.Green, _colorStyles.Green, _colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			_fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.Red, _colorStyles.Red, _colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			_fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.Transparent, _colorStyles.Transparent, _colorStyles.Transparent);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			_fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.White, _colorStyles.White, _colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			_fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));

			fillStyle = new FillStyle(FillStyle.StandardNames.Yellow, _colorStyles.Yellow, _colorStyles.White);
			fillStyle.ConvertToGrayScale = false;
			fillStyle.FillMode = FillMode.Gradient;
			fillStyle.FillPattern = HatchStyle.BackwardDiagonal;
			_fillStyles.Add(fillStyle, CreatePreviewStyle(fillStyle));
		}


		private void CreateStandardLineStyles() {
			LineStyle lineStyle;

			lineStyle = new LineStyle(LineStyle.StandardNames.Blue);
			lineStyle.ColorStyle = _colorStyles.Blue;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Dashed);
			lineStyle.ColorStyle = _colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dash;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Dotted);
			lineStyle.ColorStyle = _colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Green);
			lineStyle.ColorStyle = _colorStyles.Green;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Highlight);
			lineStyle.ColorStyle = _colorStyles.Highlight;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.HighlightDashed);
			lineStyle.ColorStyle = _colorStyles.Highlight;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dash;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.HighlightDotted);
			lineStyle.ColorStyle = _colorStyles.Highlight;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Dot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.HighlightThick);
			lineStyle.ColorStyle = _colorStyles.Highlight;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 3;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.None);
			lineStyle.ColorStyle = _colorStyles.Transparent;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Normal);
			lineStyle.ColorStyle = _colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Red);
			lineStyle.ColorStyle = _colorStyles.Red;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Special1);
			lineStyle.ColorStyle = _colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.DashDot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Special2);
			lineStyle.ColorStyle = _colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.DashDotDot;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Thick);
			lineStyle.ColorStyle = _colorStyles.Black;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 3;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));

			lineStyle = new LineStyle(LineStyle.StandardNames.Yellow);
			lineStyle.ColorStyle = _colorStyles.Yellow;
			lineStyle.DashCap = DashCap.Round;
			lineStyle.DashType = DashType.Solid;
			lineStyle.LineJoin = LineJoin.Round;
			lineStyle.LineWidth = 1;
			_lineStyles.Add(lineStyle, CreatePreviewStyle(lineStyle));
		}


		private void CreateStandardParagraphStyles() {
			ParagraphStyle paragraphStyle;

			paragraphStyle = new ParagraphStyle(ParagraphStyle.StandardNames.Label);
			paragraphStyle.Alignment = ContentAlignment.MiddleLeft;
			paragraphStyle.Padding = new TextPadding(3);
			paragraphStyle.Trimming = StringTrimming.EllipsisCharacter;
			paragraphStyle.WordWrap = false;
			_paragraphStyles.Add(paragraphStyle, CreatePreviewStyle(paragraphStyle));

			paragraphStyle = new ParagraphStyle(ParagraphStyle.StandardNames.Text);
			paragraphStyle.Alignment = ContentAlignment.TopLeft;
			paragraphStyle.Padding = new TextPadding(3);
			paragraphStyle.Trimming = StringTrimming.EllipsisCharacter;
			paragraphStyle.WordWrap = true;
			_paragraphStyles.Add(paragraphStyle, CreatePreviewStyle(paragraphStyle));

			paragraphStyle = new ParagraphStyle(ParagraphStyle.StandardNames.Title);
			paragraphStyle.Alignment = ContentAlignment.MiddleCenter;
			paragraphStyle.Padding = new TextPadding(3);
			paragraphStyle.Trimming = StringTrimming.EllipsisCharacter;
			paragraphStyle.WordWrap = true;
			_paragraphStyles.Add(paragraphStyle, CreatePreviewStyle(paragraphStyle));

		}

		#endregion


		#region Fields

		// parameters for preview style creation
		private static bool _previewAsGrayScale = true;
		private static float _previewTransparencyFactor = 0.66f;

		private object _id = null;
		private string _name = string.Empty;
		private string _title = null;
		private string _description = string.Empty;

		// Style collections
		private CapStyleCollection _capStyles = new CapStyleCollection();
		private CharacterStyleCollection _characterStyles = new CharacterStyleCollection();
		private ColorStyleCollection _colorStyles = new ColorStyleCollection();
		private FillStyleCollection _fillStyles = new FillStyleCollection();
		private LineStyleCollection _lineStyles = new LineStyleCollection();
		private ParagraphStyleCollection _paragraphStyles = new ParagraphStyleCollection();
		private const string _previewNameSuffix = " (Preview)";

		#endregion
	}

}
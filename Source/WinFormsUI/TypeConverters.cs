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
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.WinFormsUI {

	#region TypeConverters

	/// <summary>
	/// Converts all types of System.String and collections of System.String to System.String and collections of System.String.
	/// </summary>
	public class TextTypeConverter : TypeConverter {

		/// <override></override>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string))
				return true;
			if (destinationType == typeof(string[]))
				return true;
			if (destinationType == typeof(IEnumerable<string>))
				return true;
			if (destinationType == typeof(IList<string>))
				return true;
			if (destinationType == typeof(IReadOnlyCollection<string>))
				return true;
			if (destinationType == typeof(ICollection<string>))
				return true;
			return base.CanConvertTo(context, destinationType);
		}


		/// <override></override>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string)) {
				if (value != null) {
					if (culture == null) culture = CultureInfo.CurrentCulture;
					string separator = culture.TextInfo.ListSeparator + " ";

					string result = string.Empty;
					foreach (string line in ((IEnumerable<string>)value)) {
						if (result.Length > 0) result += separator;
						result += line;
					}
					return result;
				}
			} else if (destinationType == typeof(string[]))
				return value as IEnumerable<string>;
			else if (destinationType == typeof(IEnumerable<string>))
				return value as IEnumerable<string>;
			else if (destinationType == typeof(IList<string>))
				return value as IEnumerable<string>;
			else if (destinationType == typeof(ICollection<string>))
				return value as IEnumerable<string>;
			return base.ConvertTo(context, culture, value, destinationType);
		}


		/// <override></override>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			if (sourceType == typeof(string))
				return true;
			if (sourceType == typeof(string[]))
				return true;
			if (sourceType == typeof(IEnumerable<string>))
				return true;
			if (sourceType == typeof(IList<string>))
				return true;
			if (sourceType == typeof(IReadOnlyCollection<string>))
				return true;
			if (sourceType == typeof(ICollection<string>))
				return true;
			return base.CanConvertFrom(context, sourceType);
		}


		/// <override></override>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			if (value == null) return null;
			List<string> lines = new List<string>();
			if (value is string) {
				if (culture == null) culture = CultureInfo.CurrentCulture;
				string separator = culture.TextInfo.ListSeparator + " ";
				lines.AddRange(((string)value).Split(new string[] { separator }, StringSplitOptions.None));
			} else if (value is string[]
				|| value is IEnumerable<string>)
				lines.AddRange((IEnumerable<string>)value);

			if (context.Instance is string)
				return ConvertTo(context, culture, lines, typeof(string));
			else if (context.Instance is string[])
				return lines.ToArray();
			else return lines;
		}

	}


	/// <summary>
	/// Converts a Dataweb.NShape.Advanced.NamedImage to a System.Drawing.Image type or a System.String.
	/// </summary>
	public class NamedImageTypeConverter : TypeConverter {

		/// <override></override>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)) return true;
			if (destinationType == typeof(Image)) return true;
			if (destinationType == typeof(Bitmap)) return true;
			if (destinationType == typeof(Metafile)) return true;
			return base.CanConvertTo(context, destinationType);
		}


		/// <override></override>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (value != null && value is NamedImage) {
				NamedImage val = (NamedImage)value;
				if (destinationType == typeof(string))
					return val.Name;
				else if (destinationType == typeof(Bitmap))
					return (Bitmap)val.Image;
				else if (destinationType == typeof(Metafile))
					return (Metafile)val.Image;
				else if (destinationType == typeof(Image))
					return val.Image;
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}

	}


	/// <summary>
	/// Converts a Dataweb.NShape.IStyle type to a System.String type.
	/// </summary>
	public class StyleTypeConverter : TypeConverter {

		/// <override></override>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)) return true;
			return base.CanConvertTo(context, destinationType);
		}


		/// <override></override>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string) && value is IStyle) return ((IStyle)value).Title;
			return base.ConvertTo(context, culture, value, destinationType);
		}

	}


	/// <summary>
	/// Converts a System.Drawing.FontFamily type to a System.String type.
	/// </summary>
	public class FontFamilyTypeConverter : TypeConverter {

		/// <override></override>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)) return true;
			return base.CanConvertTo(context, destinationType);
		}


		/// <override></override>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == typeof(string) && value != null)
				return ((FontFamily)value).Name;
			return base.ConvertTo(context, culture, value, destinationType);
		}

	}


	/// <summary>
	/// Converts a Dataweb.NShape.TextPadding type to a System.Windows.Forms.Padding or a System.String type and vice versa.
	/// </summary>
	public class TextPaddingTypeConverter : TypeConverter {

		/// <override></override>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			return ((sourceType == typeof(string))
					|| (sourceType == typeof(Padding))
					|| base.CanConvertFrom(context, sourceType));
		}


		/// <override></override>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)
				|| destinationType == typeof(Padding)
				|| destinationType == typeof(InstanceDescriptor))
				return true;
			else return base.CanConvertTo(context, destinationType);
		}


		/// <override></override>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			TextPadding result = TextPadding.Empty;
			if (value is string) {
				string valueStr = value as string;
				if (valueStr == null) return base.ConvertFrom(context, culture, value);

				valueStr = valueStr.Trim();
				if (valueStr.Length == 0) return null;

				if (culture == null) culture = CultureInfo.CurrentCulture;
				char ch = culture.TextInfo.ListSeparator[0];
				string[] strArray = valueStr.Split(new char[] { ch });
				int[] numArray = new int[strArray.Length];
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
				for (int i = 0; i < numArray.Length; i++)
					numArray[i] = (int)converter.ConvertFromString(context, culture, strArray[i]);

				if (numArray.Length == 1)
					result.All = numArray[0];
				else if (numArray.Length == 4) {
					result.Left = numArray[0];
					result.Top = numArray[1];
					result.Right = numArray[2];
					result.Bottom = numArray[3];
				} else throw new ArgumentException();
			} else if (value is Padding) {
				Padding padding = (Padding)value;
				result.Left = padding.Left;
				result.Top = padding.Top;
				result.Right = padding.Right;
				result.Bottom = padding.Bottom;
			} else return base.ConvertFrom(context, culture, value);
			return result;
		}


		/// <override></override>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == null) throw new ArgumentNullException("destinationType");
			if (value is TextPadding) {
				if (destinationType == typeof(string)) {
					TextPadding txtPadding = (TextPadding)value;
					if (destinationType == typeof(string)) {
						if (culture == null) culture = CultureInfo.CurrentCulture;

						string separator = culture.TextInfo.ListSeparator + " ";
						TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
						string[] strArray = new string[4];
						strArray[0] = converter.ConvertToString(context, culture, txtPadding.Left);
						strArray[1] = converter.ConvertToString(context, culture, txtPadding.Top);
						strArray[2] = converter.ConvertToString(context, culture, txtPadding.Right);
						strArray[3] = converter.ConvertToString(context, culture, txtPadding.Bottom);
						return string.Join(separator, strArray);
					}
					if (destinationType == typeof(InstanceDescriptor)) {
						if (txtPadding.All < 0) {
							return new InstanceDescriptor(
								typeof(TextPadding).GetConstructor(new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }),
									new object[] { txtPadding.Left, txtPadding.Top, txtPadding.Right, txtPadding.Bottom });
						} else {
							return new InstanceDescriptor(
								typeof(TextPadding).GetConstructor(new Type[] { typeof(int) }), new object[] { txtPadding.All }
							);
						}
					}
				} else if (destinationType == typeof(Padding)) {
					Padding paddingResult = Padding.Empty;
					if (value != null) {
						TextPadding val = (TextPadding)value;
						paddingResult.Left = val.Left;
						paddingResult.Top = val.Top;
						paddingResult.Right = val.Right;
						paddingResult.Bottom = val.Bottom;
					}
					return paddingResult;
				}
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}


		/// <override></override>
		public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues) {
			if (context == null) throw new ArgumentNullException("context");
			if (propertyValues == null) throw new ArgumentNullException("propertyValues");

			TextPadding txtPadding = (TextPadding)context.PropertyDescriptor.GetValue(context.Instance);
			TextPadding result = TextPadding.Empty;
			int all = (int)propertyValues["All"];
			if (txtPadding.All != all) result.All = all;
			else {
				result.Left = (int)propertyValues["Left"];
				result.Top = (int)propertyValues["Top"];
				result.Right = (int)propertyValues["Right"];
				result.Bottom = (int)propertyValues["Bottom"];
			}
			return result;
		}


		/// <override></override>
		public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) {
			return true;
		}


		/// <override></override>
		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) {
			return TypeDescriptor.GetProperties(typeof(TextPadding), attributes).Sort(
				new string[] { "All", "Left", "Top", "Right", "Bottom" }
			);
		}


		/// <override></override>
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) {
			return true;
		}

	}


	/// <summary>
	/// Converts a <see cref="T:Dataweb.NShape.IStyle" /> to a <see cref="T:System.String" />.
	/// </summary>
	public class LayerTypeConverter : TypeConverter {

		/// <override></override>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
			return ((sourceType == typeof(string))
					|| (sourceType == typeof(int))
					|| (sourceType == typeof(LayerIds))
					|| base.CanConvertFrom(context, sourceType));
		}


		/// <override></override>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
			if (destinationType == typeof(string)
				|| (destinationType == typeof(int))
				|| (destinationType == typeof(LayerIds))
				|| destinationType == typeof(InstanceDescriptor))
				return true;
			else return base.CanConvertTo(context, destinationType);
		}


		/// <override></override>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
			// Determine the property's value type (Property HomeLayer: int, Property Layers/SupplementalLayers: LayerIds)
			Type propertyType = context.PropertyDescriptor.PropertyType;
			if (value is string || value == null) {
				if (propertyType == typeof(int))
					return ConvertToLayerId(context, (string)value);
				else if (propertyType == typeof(LayerIds))
					return ConvertToLayerIds(context, (string)value);
				else throw new NotSupportedException();
			} else if (value is int) {
				if (propertyType == typeof(LayerIds))
					return Layer.ConvertToLayerIds((int)value);
				else if (propertyType == typeof(string))
					return ConvertToLayerName(context, (int)value);
				else throw new NotSupportedException();
			} else if (value is LayerIds) {
				if (propertyType == typeof(int))
					return Layer.ConvertToLayerId((LayerIds)value);
				else if (propertyType == typeof(string))
					return ConvertToLayerName(context, (LayerIds)value);
				else throw new NotSupportedException();
			} else throw new NotSupportedException();
		}


		/// <override></override>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
			if (destinationType == null) throw new ArgumentNullException("destinationType");
			if (value is string || value == null) {
				if (destinationType == typeof(int))
					return ConvertToLayerId(context, (string)value);
				else if (destinationType == typeof(LayerIds))
					return ConvertToLayerIds(context, (string)value);
				else throw new NotSupportedException();
			} else if (value is int) {
				if (destinationType == typeof(LayerIds))
					return Layer.ConvertToLayerIds((int)value);
				else if (destinationType == typeof(string))
					return ConvertToLayerName(context, (int)value);
				else throw new NotSupportedException();
			} else if (value is LayerIds) {
				if (destinationType == typeof(int))
					return Layer.ConvertToLayerId((LayerIds)value);
				else if (destinationType == typeof(string)) {
					string result = String.Empty;
					foreach (int id in LayerHelper.GetAllLayerIds((LayerIds)value))
						result += string.Format("{0}{1}",
							String.IsNullOrEmpty(result) ? String.Empty : ListSeparatorChar + " ", 
							ConvertToLayerName(context, id)
						);
					return result;
				} else throw new NotSupportedException();
			} else throw new NotSupportedException();
		}


		private LayerIds ConvertToLayerIds(ITypeDescriptorContext context, string value) {
			if (string.IsNullOrEmpty(value))
				return LayerIds.None;

			LayerIds result;
			if (value.Contains(ListSeparatorChar)) {
				// Convert multiple values
				result = LayerIds.None;
				foreach (String val in value.Split(','))
					result |= ConvertToLayerIds(context, val.Trim());
			} else {
				// First, try interpreting as Name
				Diagram diagram = GetDiagram(context);
				Layer layer = diagram.Layers.FindLayer(value);
				if (layer != null)
					result = Layer.ConvertToLayerIds(layer.LayerId);
				else {
					// Afterwards, try converting to integer
					int layerId;
					if (int.TryParse(value, out layerId))
						result = Layer.ConvertToLayerIds(layerId);
					else {
						// Finally, check for LayerIds
						result = (LayerIds)Enum.Parse(typeof(LayerIds), value);
					}
				}
			}
			return result;
		}


		private int ConvertToLayerId(ITypeDescriptorContext context, string value) {
			if (string.IsNullOrEmpty(value))
				return Layer.NoLayerId;

			int result;
			if (value.Contains(ListSeparatorChar)) {
				// Converting to list of integer values is not supported
				throw new NotSupportedException();
			} else {
				// First, try converting to integer
				if (!int.TryParse(value, out result)) {
					// Converion successful, nothing else to do
				} else {
					try {
						// Afterwards, check for LayerIds
						result = Layer.ConvertToLayerId((LayerIds)Enum.Parse(typeof(LayerIds), value));
					} catch (Exception) {
						// Finally, try interpreting as Name/Title
						Layer layer = ConvertToLayer(context, value);
						if (layer == null) throw new ArgumentException();
						result = layer.LayerId;
					}
				}
			}
			return result;
		}


		private string ConvertToLayerName(ITypeDescriptorContext context, int value) {
			if (value == Layer.NoLayerId)
				return null;
			Layer layer = ConvertToLayer(context, value);
			if (layer == null) throw new ArgumentException();
			return layer.Name;
		}


		private string ConvertToLayerName(ITypeDescriptorContext context, LayerIds value) {
			if (value == LayerIds.None)
				return null;
			Layer layer = ConvertToLayer(context, value);
			if (layer == null) throw new ArgumentException();
			return layer.Name;
		}


		private Layer ConvertToLayer(ITypeDescriptorContext context, string value) {
			// Try interpreting as Name/Title
			Diagram diagram = GetDiagram(context);
			if (diagram == null) throw new ArgumentException();
			// Search by name
			Layer layer = diagram.Layers.FindLayer(value);
			if (layer == null) {
				// Search for title
				foreach (Layer l in diagram.Layers)
					if (string.Compare(l.Title, value, true) == 0) {
						layer = l;
						break;
					}
			}
			return layer;
		}


		private Layer ConvertToLayer(ITypeDescriptorContext context, int value) {
			// Try interpreting as Name/Title
			Diagram diagram = GetDiagram(context);
			if (diagram == null) throw new ArgumentException();
			return diagram.Layers[value];
		}


		private Layer ConvertToLayer(ITypeDescriptorContext context, LayerIds value) {
			// Try interpreting as Name/Title
			Diagram diagram = GetDiagram(context);
			if (diagram == null) throw new ArgumentException();
			return diagram.Layers[value];
		}


		private Diagram GetDiagram(ITypeDescriptorContext context) {
			if (context.Instance is object[]) {
				Shape shape = ((object[])context.Instance)[0] as Shape;
				return (shape != null) ? shape.Diagram : null;
			} else
				return (context.Instance is Shape) ? ((Shape)context.Instance).Diagram : null;
		}


		private const string ListSeparatorChar = ",";
	}

	#endregion

}

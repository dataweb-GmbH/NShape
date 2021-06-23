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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.WinFormsUI {

	#region UITypeEditors

	/// <summary>
	/// NShape UI type editor for choosing a character style's font from a drop down list.
	/// </summary>
	public class FontFamilyUITypeEditor : UITypeEditor {

		/// <override></override>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context != null && context.Instance != null && provider != null) {
				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null) {
					using (FontFamilyListBox listBox = new FontFamilyListBox(edSvc)) {
						listBox.BorderStyle = BorderStyle.None;
						listBox.IntegralHeight = false;
						listBox.Items.Clear();

						int cnt = FontFamily.Families.Length;
						for (int i = 0; i < cnt; ++i) {
							FontFamily family = FontFamily.Families[i];
							listBox.Items.Add(family.Name);
							if (family == value)
								listBox.SelectedIndex = listBox.Items.Count - 1;
						}
						edSvc.DropDownControl(listBox);
						if (listBox.SelectedItem != null)
							value = listBox.SelectedItem.ToString();
					}
				}
			}
			return value;
		}


		/// <override></override>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null && context.Instance != null)
				return UITypeEditorEditStyle.DropDown;
			return base.GetEditStyle(context);
		}


		/// <override></override>
		public override bool GetPaintValueSupported(ITypeDescriptorContext context) {
			return false;
		}


		/// <override></override>
		public override void PaintValue(PaintValueEventArgs e) {
			if (e != null && e.Value != null) {
				if (formatter.Alignment != StringAlignment.Center) formatter.Alignment = StringAlignment.Center;
				if (formatter.LineAlignment != StringAlignment.Near) formatter.LineAlignment = StringAlignment.Near;

				GdiHelpers.ApplyGraphicsSettings(e.Graphics, RenderingQuality.HighQuality);
				using (Font font = new Font(e.Value.ToString(), e.Bounds.Height, FontStyle.Regular, GraphicsUnit.Pixel))
					e.Graphics.DrawString(e.Value.ToString(), font, Brushes.Black, (RectangleF)e.Bounds, formatter);
				GdiHelpers.ApplyGraphicsSettings(e.Graphics, RenderingQuality.DefaultQuality);

				base.PaintValue(e);
			}
		}


		/// <override></override>
		public override bool IsDropDownResizable {
			get { return true; }
		}

		StringFormat formatter = new StringFormat();
	}


	/// <summary>
	/// NShape UI type editor for editing flagable enums.
	/// </summary>
	public class FlagEnumUITypeEditor : UITypeEditor {

		/// <override></override>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (provider != null) {
				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null && context.Instance != null) {
					using (CheckedListBox checkedListBox = new CheckedListBox()) {
						checkedListBox.BorderStyle = BorderStyle.None;
						checkedListBox.IntegralHeight = false;
						checkedListBox.CheckOnClick = true;
						checkedListBox.SelectionMode = SelectionMode.One;
						checkedListBox.SelectedIndexChanged += CheckedListBox_SelectedIndexChanged;
						checkedListBox.ItemCheck += CheckedListBox_ItemCheck;

						// Add existing layers and check shape's layers
						int intValue = (int)value;
						Type enumType = context.PropertyDescriptor.PropertyType;
						Array enumValues = Enum.GetValues(enumType);
						for (int i = 0; i < enumValues.Length; ++i) {
							int enumVal = (int)enumValues.GetValue(i);
							if (checkedListBox.Items.Count < enumValues.Length)
								checkedListBox.Items.Insert(i, EnumDisplayItem.Create(enumVal, Enum.GetName(enumType, enumVal)));
							EnumDisplayItem item = (EnumDisplayItem)checkedListBox.Items[i];
							bool isChecked = ((item.EnumValue & intValue) == item.EnumValue) && item.EnumValue != 0;
							checkedListBox.SetItemChecked(i, isChecked);
						}

						edSvc.DropDownControl(checkedListBox);
						// Get selected enum flags
						intValue = 0;
						foreach (EnumDisplayItem item in checkedListBox.CheckedItems)
							intValue |= item.EnumValue;
						// Update property value
						value = intValue;
					}
				}
			}
			return value;
		}


		private void CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e) {
			//throw new NotImplementedException();
		}


		/// <override></override>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null && context.Instance != null)
				return UITypeEditorEditStyle.DropDown;
			return base.GetEditStyle(context);
		}


		/// <override></override>
		public override bool GetPaintValueSupported(ITypeDescriptorContext context) {
			return false;
		}


		/// <override></override>
		public override bool IsDropDownResizable {
			get { return true; }
		}


		private void CheckedListBox_SelectedIndexChanged(object sender, EventArgs e) {
			if (sender is CheckedListBox checkedListBox) {
				//int idx = checkedListBox.SelectedIndex;
				//for (int i = checkedListBox.Items.Count - 1; i >= 0; --i)
				//	if (i != idx && checkedListBox.GetItemChecked(i)) {
				//		checkedListBox.SetItemChecked(i, false);
				//	}
			}
		}


		private class EnumDisplayItem {

			public static EnumDisplayItem Create(int enumValue, string enumTitle) {
				EnumDisplayItem result = new EnumDisplayItem();
				result.EnumValue = enumValue;
				result.EnumTitle = enumTitle;
				return result;
			}

			public override string ToString() {
				return EnumTitle;
			}

			public int EnumValue { get; set; }

			public string EnumTitle { get; set; }

		}

	}


	/// <summary>
	/// NShape UI type editor for adding/removing shapes to/from layers.
	/// </summary>
	public class LayerUITypeEditor : UITypeEditor {

		/// <override></override>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context != null && context.Instance != null && provider != null) {
				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null && context.Instance is Shape) {
					Shape shape = (Shape)context.Instance;
					using (CheckedListBox listBox = new CheckedListBox()) {
						bool isHomeLayerProperty = (context.PropertyDescriptor.Name == "HomeLayer");

						listBox.BorderStyle = BorderStyle.None;
						listBox.IntegralHeight = false;
						listBox.CheckOnClick = true;
						listBox.SelectionMode = SelectionMode.One;
						if (isHomeLayerProperty) {
							listBox.SelectedIndexChanged += (sender, e) => {
								int idx = listBox.SelectedIndex;
								for (int i = listBox.Items.Count - 1; i >= 0; --i)
									if (i != idx && listBox.GetItemChecked(i))
										listBox.SetItemChecked(i, false);
							};
						}

						// Add existing layers and check shape's layers
						listBox.Items.Clear();
						foreach (Layer l in shape.Diagram.Layers) {
							int idx = listBox.Items.Count;
							// Add layer item
							if (Layer.IsCombinable(l.LayerId) || isHomeLayerProperty) {
								listBox.Items.Insert(idx, LayerDisplayItem.Create(l.LayerId, l.Name));
								// Set check state
								if (isHomeLayerProperty)
									listBox.SetItemChecked(idx, shape.HomeLayer == l.LayerId);
								else
									listBox.SetItemChecked(idx, (shape.SupplementalLayers & Layer.ConvertToLayerIds(l.LayerId)) != 0);
							}
						}

						edSvc.DropDownControl(listBox);
						// Get selected layer ids
						List<int> shapeLayerIds = new List<int>();
						foreach (LayerDisplayItem layerItem in listBox.CheckedItems)
							shapeLayerIds.Add(layerItem.LayerId);

						// Set property value
						if (isHomeLayerProperty) {
							System.Diagnostics.Debug.Assert(shapeLayerIds.Count <= 1);
							value = (shapeLayerIds.Count > 0) ? shapeLayerIds[0] : Layer.NoLayerId;
						} else {
							value = Layer.ConvertToLayerIds(shapeLayerIds);
						}
					}
				}
			}
			return value;
		}


		/// <override></override>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null && context.Instance != null)
				return UITypeEditorEditStyle.DropDown;
			return base.GetEditStyle(context);
		}


		/// <override></override>
		public override bool GetPaintValueSupported(ITypeDescriptorContext context) {
			return false;
		}


		///// <override></override>
		//public override void PaintValue(PaintValueEventArgs e) {
		//    if (e != null && e.Value is int) {
		//        ITypeDescriptorContext context = e.Context;
		//        if (context != null && context.Instance is Shape) {
		//            Shape shape = (Shape)context.Instance;

		//            int homeLayer = (int)e.Value;
		//            e.Value = shape.Diagram.Layers[homeLayer].Title;
		//        }
		//    } else
		//        base.PaintValue(e);
		//}


		/// <override></override>
		public override bool IsDropDownResizable {
			get { return true; }
		}


		private struct LayerDisplayItem {

			public static readonly LayerDisplayItem Empty;

			public static LayerDisplayItem Create(int layerId, string layerTitle) {
				LayerDisplayItem result = LayerDisplayItem.Empty;
				result.LayerId = layerId;
				result.LayerTitle = layerTitle;
				return result;
			}

			public override string ToString() {
				return string.Format("{0:D2}: {1}", LayerId, LayerTitle);
			}

			public int LayerId;
			public string LayerTitle;

			static LayerDisplayItem() {
				Empty.LayerId = Layer.NoLayerId;
				Empty.LayerTitle = String.Empty;
			}
		}

	}


	/// <summary>
	/// NShape UI type editor for editing properties of type System.String or a System.String collection type.
	/// </summary>
	public class TextUITypeEditor : UITypeEditor {

		/// <override></override>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context != null && context.Instance != null && provider != null) {
				if (context.PropertyDescriptor.PropertyType == typeof(string)) {
					string valueStr = string.IsNullOrEmpty((string)value) ? string.Empty : (string)value;
					using (TextEditorDialog stringEditor = new TextEditorDialog(valueStr)) {
						if (stringEditor.ShowDialog() == DialogResult.OK)
							value = stringEditor.ResultText;
					}
				} else if (context.PropertyDescriptor.PropertyType == typeof(string[])) {
					string[] valueArr = (value != null) ? (string[])value : new string[0];
					using (TextEditorDialog stringEditor = new TextEditorDialog(valueArr)) {
						if (stringEditor.ShowDialog() == DialogResult.OK)
							value = stringEditor.ResultText;
					}
				} else if (context.PropertyDescriptor.PropertyType == typeof(IEnumerable<string>)
					|| context.PropertyDescriptor.PropertyType == typeof(IList<string>)
					|| context.PropertyDescriptor.PropertyType == typeof(ICollection<string>)
					|| context.PropertyDescriptor.PropertyType == typeof(List<string>)) {
					IEnumerable<string> values = (value != null) ? (IEnumerable<string>)value : new string[0];
					using (TextEditorDialog stringEditor = new TextEditorDialog(values)) {
						if (stringEditor.ShowDialog() == DialogResult.OK)
							value = new List<string>(stringEditor.Lines);
					}
				}
			}
			return value;
		}


		/// <override></override>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null && context.Instance != null)
				return UITypeEditorEditStyle.Modal;
			else return base.GetEditStyle(context);
		}

	}


	/// <summary>
	/// NShape UI type editor for choosing an image and assinging a name.
	/// </summary>
	public class NamedImageUITypeEditor : UITypeEditor {

		/// <override></override>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context != null && context.Instance != null && provider != null) {
				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null) {
					ImageEditor imageEditor = null;
					try {
						if (value is NamedImage && value != null)
							imageEditor = new ImageEditor((NamedImage)value);
						else imageEditor = new ImageEditor();

						if (imageEditor.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
							value = imageEditor.Result;
						}
					} finally {
						if (imageEditor != null) imageEditor.Dispose();
						imageEditor = null;
					}
				}
			}
			return value;
		}


		/// <override></override>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null && context.Instance != null)
				return UITypeEditorEditStyle.Modal;
			return base.GetEditStyle(context);
		}


		/// <override></override>
		public override bool GetPaintValueSupported(ITypeDescriptorContext context) {
			return true;
		}


		/// <override></override>
		public override void PaintValue(PaintValueEventArgs e) {
			base.PaintValue(e);
			if (e != null && e.Value is NamedImage) {
				try {
					NamedImage imageValue = (NamedImage)e.Value;
					if (imageValue.Image != null) {
						// Create a thumbnail image
						GraphicsUnit gfxUnit = GraphicsUnit.Pixel;
						Rectangle imgBounds = Rectangle.Round(imageValue.Image.GetBounds(ref gfxUnit));
						Rectangle bounds = CalcDestinationBounds(imageValue.Image, imgBounds, Rectangle.FromLTRB(0, 0, 100, 100));
						using (Image img = imageValue.Image.GetThumbnailImage(bounds.Width, bounds.Height, null, IntPtr.Zero)) {
							Rectangle srcRect = Rectangle.Empty;
							srcRect.X = 0;
							srcRect.Y = 0;
							srcRect.Width = img.Width;
							srcRect.Height = img.Height;

							float highestRatio = (float)Math.Round(
								Math.Max((double)(e.Bounds.Width - (e.Bounds.X + e.Bounds.X)) / (double)img.Width,
								(double)(e.Bounds.Height - (e.Bounds.Y + e.Bounds.Y)) / (double)img.Height)
								, 6);
							Rectangle dstRect = Rectangle.Empty;
							dstRect.Width = (int)Math.Round(srcRect.Width * highestRatio);
							dstRect.Height = (int)Math.Round(srcRect.Height * highestRatio);
							dstRect.X = e.Bounds.X + (int)Math.Round((float)(e.Bounds.Width - dstRect.Width) / 2);
							dstRect.Y = e.Bounds.Y + (int)Math.Round((float)(e.Bounds.Height - dstRect.Height) / 2);

							// Apply HighQuality rendering settings to avoid false-color images when using
							// certain image formats (e.g. JPG with 24 bits color depth) on x64 OSes
							// Revert to default settings afterwards in order to avoid other graphical glitches
							GdiHelpers.ApplyGraphicsSettings(e.Graphics, RenderingQuality.HighQuality);
							e.Graphics.DrawImage(img, dstRect, srcRect, GraphicsUnit.Pixel);
							GdiHelpers.ApplyGraphicsSettings(e.Graphics, RenderingQuality.DefaultQuality);
						}
					}
				} catch (Exception exc) {
					// Ignore errors while drawing
					Debug.Print(string.Format("Error in {0}.PaintValue: {1}", GetType().Name, exc.Message));
				}
			}
		}


		private Rectangle CalcDestinationBounds(Image image, Rectangle sourceBounds, Rectangle destinationBounds) {
			float highestRatio = (float)Math.Round(
					Math.Max((double)(destinationBounds.Width - (destinationBounds.X + destinationBounds.X)) / (double)image.Width,
					(double)(destinationBounds.Height - (destinationBounds.Y + destinationBounds.Y)) / (double)image.Height)
					, 6);
			Rectangle result = Rectangle.Empty;
			result.Width = (int)Math.Round(sourceBounds.Width * highestRatio);
			result.Height = (int)Math.Round(sourceBounds.Height * highestRatio);
			result.X = destinationBounds.X + (int)Math.Round((float)(destinationBounds.Width - result.Width) / 2);
			result.Y = destinationBounds.Y + (int)Math.Round((float)(destinationBounds.Height - result.Height) / 2);
			return result;
		}

	}


	/// <summary>
	/// NShape UI type editor for choosing a directory.
	/// </summary>
	public class DirectoryUITypeEditor : UITypeEditor {

		/// <override></override>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (context != null && context.Instance != null && provider != null) {
				IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (edSvc != null) {
					using (FolderBrowserDialog dlg = new FolderBrowserDialog()) {
						if (value is DirectoryInfo)
							dlg.SelectedPath = ((DirectoryInfo)value).FullName;
						else if (value is string)
							dlg.SelectedPath = (string)value;
						dlg.ShowNewFolderButton = true;

						if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
							value = dlg.SelectedPath;
					}
				}
			}
			return value;
		}


		/// <override></override>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null && context.Instance != null)
				return UITypeEditorEditStyle.Modal;
			return base.GetEditStyle(context);
		}


		/// <override></override>
		public override bool GetPaintValueSupported(ITypeDescriptorContext context) {
			return false;
		}

	}


	/// <summary>
	/// NShape UI type editor for choosing a style from a drop down list.
	/// </summary>
	public class StyleUITypeEditor : UITypeEditor {

		/// <summary>
		/// The <see cref="T:Dataweb.NShape.Project" /> providing the <see cref="T:Dataweb.NShape.Design" /> for the <see cref="T:Dataweb.NShape.WinFormsUI.StyleUITypeEditor" />.
		/// </summary>
		public static Project Project {
			set {
				if (_projectBuffer != value) {
					_projectBuffer = value;
					_designBuffer = _projectBuffer.Design;
				} else {
					bool replaceDesign = (_designBuffer == null);
					if (!replaceDesign) {
						replaceDesign = true;
						// Ensure that the current design is part in the repository:
						foreach (Design design in _projectBuffer.Repository.GetDesigns()) {
							if (design == _designBuffer) {
								replaceDesign = false;
								break;
							}
						}
					}
					if (replaceDesign) _designBuffer = _projectBuffer.Design;
				}
			}
		}


		/// <summary>
		/// The design providing styles for the <see cref="T:Dataweb.NShape.WinFormsUI.StyleUITypeEditor" />.
		/// </summary>
		public static Design Design {
			set { _designBuffer = value; }
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.StyleUITypeEditor" />.
		/// </summary>
		public StyleUITypeEditor() {
			if (_designBuffer == null) {
				string msg = string.Format("{0} is not set. Set the static property {1}.Design to a reference of the current design before creating the UI type editor.", typeof(Design).Name, GetType().Name);
				throw new NShapeInternalException(msg);
			}
			this._project = _projectBuffer;
			this._design = _designBuffer;
		}


		/// <override></override>
		public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value) {
			if (provider != null) {
#if DEBUG
				if (!(context.PropertyDescriptor is PropertyDescriptorDg))
					System.Diagnostics.Debug.Print("### The given PropertyDescriptor for {0} is not of type {1}.", value, typeof(PropertyDescriptorDg).Name);
				else System.Diagnostics.Debug.Print("### PropertyDescriptor is of type {1}.", value, typeof(PropertyDescriptorDg).Name);
#endif

				IWindowsFormsEditorService editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
				if (editorService != null) {
					// fetch current Design (if changed)
					if (_designBuffer != null && _designBuffer != _design)
						_design = _designBuffer;

					// Examine edited instances and determine whether the list item "Default Style" should be displayed.
					bool showItemDefaultStyle = false;
					bool showItemOpenEditor = false;
					if (context.Instance is Shape) {
						showItemDefaultStyle = ((Shape)context.Instance).Template != null;
						showItemOpenEditor = _project.SecurityManager.IsGranted(Permission.Designs);
					} else if (context.Instance is ICapStyle) {
						showItemDefaultStyle = true;
					} else if (context.Instance is object[]) {
						object[] objArr = (object[])context.Instance;
						int cnt = objArr.Length;
						showItemDefaultStyle = true;
						showItemOpenEditor = _project.SecurityManager.IsGranted(Permission.Designs);
						foreach (object obj in objArr) {
							Shape shape = obj as Shape;
							if (shape == null || shape.Template == null) {
								showItemDefaultStyle = false;
								showItemOpenEditor = false;
								break;
							}
						}
					}

					StyleListBox styleListBox = null;
					try {
						Type styleType = null;
						if (value == null) {
							if (context.PropertyDescriptor.PropertyType == typeof(ICapStyle))
								styleType = typeof(CapStyle);
							else if (context.PropertyDescriptor.PropertyType == typeof(IColorStyle))
								styleType = typeof(ColorStyle);
							else if (context.PropertyDescriptor.PropertyType == typeof(IFillStyle))
								styleType = typeof(FillStyle);
							else if (context.PropertyDescriptor.PropertyType == typeof(ICharacterStyle))
								styleType = typeof(CharacterStyle);
							else if (context.PropertyDescriptor.PropertyType == typeof(ILineStyle))
								styleType = typeof(LineStyle);
							else if (context.PropertyDescriptor.PropertyType == typeof(IParagraphStyle))
								styleType = typeof(ParagraphStyle);
							else throw new NShapeUnsupportedValueException(context.PropertyDescriptor.PropertyType);

							if (_project != null)
								styleListBox = new StyleListBox(editorService, _project, _design, styleType, showItemDefaultStyle, showItemOpenEditor);
							else styleListBox = new StyleListBox(editorService, _design, styleType, showItemDefaultStyle, showItemOpenEditor);
						} else {
							if (_project != null)
								styleListBox = new StyleListBox(editorService, _project, _design, value as Style, showItemDefaultStyle, showItemOpenEditor);
							else styleListBox = new StyleListBox(editorService, _design, value as Style, showItemDefaultStyle, showItemOpenEditor);
						}

						editorService.DropDownControl(styleListBox);
						if (styleListBox.SelectedItem is IStyle)
							value = styleListBox.SelectedItem;
						else value = null;
					} finally {
						if (styleListBox != null) styleListBox.Dispose();
						styleListBox = null;
					}
				}
			}
			return value;
		}


		/// <override></override>
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) {
			if (context != null && context.Instance != null)
				return UITypeEditorEditStyle.DropDown;
			else return base.GetEditStyle(context);
		}


		/// <override></override>
		public override bool GetPaintValueSupported(ITypeDescriptorContext context) {
			return true;
		}


		/// <override></override>
		public override void PaintValue(PaintValueEventArgs e) {
			//base.PaintValue(e);
			if (e != null && e.Value != null) {
				Rectangle previewRect = Rectangle.Empty;
				previewRect.X = e.Bounds.X + 1;
				previewRect.Y = e.Bounds.Y + 1;
				previewRect.Width = (e.Bounds.Right - 1) - (e.Bounds.Left + 1);
				previewRect.Height = (e.Bounds.Bottom - 1) - (e.Bounds.Top + 1);

				// Set new GraphicsModes
				Matrix origTransform = e.Graphics.Transform;
				GdiHelpers.ApplyGraphicsSettings(e.Graphics, RenderingQuality.HighQuality);

				// Draw value
				if (e.Value is ICapStyle)
					DrawStyleItem(e.Graphics, previewRect, (ICapStyle)e.Value);
				else if (e.Value is ICharacterStyle)
					DrawStyleItem(e.Graphics, previewRect, (ICharacterStyle)e.Value);
				else if (e.Value is IColorStyle)
					DrawStyleItem(e.Graphics, previewRect, (IColorStyle)e.Value);
				else if (e.Value is IFillStyle)
					DrawStyleItem(e.Graphics, previewRect, (IFillStyle)e.Value);
				else if (e.Value is ILineStyle)
					DrawStyleItem(e.Graphics, previewRect, (ILineStyle)e.Value);
				else if (e.Value is IParagraphStyle)
					DrawStyleItem(e.Graphics, previewRect, (IParagraphStyle)e.Value);

				// Restore original values
				GdiHelpers.ApplyGraphicsSettings(e.Graphics, RenderingQuality.DefaultQuality);
				e.Graphics.Transform = origTransform;
			}
		}


		/// <override></override>
		public override bool IsDropDownResizable {
			get { return true; }
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, ICapStyle capStyle) {
			Pen capPen = ToolCache.GetPen(_design.LineStyles.Normal, null, capStyle);
			float scale = 1;
			if (capStyle.CapSize + 2 >= previewBounds.Height) {
				scale = Geometry.CalcScaleFactor(capStyle.CapSize + 2, capStyle.CapSize + 2, previewBounds.Width - 2, previewBounds.Height - 2);
				gfx.ScaleTransform(scale, scale);
			}
			int startX, endX, y;
			startX = previewBounds.Left;
			if (capPen.StartCap == LineCap.Custom)
				startX += (int)Math.Round(capStyle.CapSize - capPen.CustomStartCap.BaseInset);
			startX = (int)Math.Round(startX / scale);
			endX = previewBounds.Right;
			if (capPen.EndCap == LineCap.Custom)
				endX -= (int)Math.Round(capStyle.CapSize - capPen.CustomEndCap.BaseInset);
			endX = (int)Math.Round(endX / scale);
			y = (int)Math.Round((previewBounds.Y + ((float)previewBounds.Height / 2)) / scale);
			gfx.DrawLine(capPen, startX, y, endX, y);
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, ICharacterStyle charStyle) {
			Brush fontBrush = ToolCache.GetBrush(charStyle.ColorStyle);
			Font font = ToolCache.GetFont(charStyle);
			int height = Geometry.PointToPixel(charStyle.SizeInPoints, gfx.DpiY);
			float scale = Geometry.CalcScaleFactor(height, height, previewBounds.Width, previewBounds.Height);
			gfx.ScaleTransform(scale, scale);
			RectangleF layoutRect = RectangleF.Empty;
			layoutRect.X = 0;
			layoutRect.Y = 0;
			layoutRect.Width = (float)previewBounds.Width / scale;
			layoutRect.Height = (float)previewBounds.Height / scale;
			gfx.DrawString(string.Format("{0} {1} pt", charStyle.FontName, charStyle.SizeInPoints),
				font, fontBrush, layoutRect, _formatter);
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, IColorStyle colorStyle) {
			Brush brush = ToolCache.GetBrush(colorStyle);
			gfx.FillRectangle(brush, previewBounds);
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, IFillStyle fillStyle) {
			Brush fillBrush = ToolCache.GetBrush(fillStyle);
			// Transform
			if (fillBrush is LinearGradientBrush) {
				float srcGradLen = ((LinearGradientBrush)fillBrush).Rectangle.Width;
				float dstGradLen = previewBounds.Width / (float)Math.Cos(Geometry.DegreesToRadians(fillStyle.GradientAngle));
				float scale = dstGradLen / srcGradLen;
				((LinearGradientBrush)fillBrush).ResetTransform();
				((LinearGradientBrush)fillBrush).ScaleTransform(scale, scale);
				((LinearGradientBrush)fillBrush).RotateTransform(fillStyle.GradientAngle);
			} else if (fillBrush is TextureBrush) {
				if (fillStyle.ImageLayout == ImageLayoutMode.Stretch) {
					float scaleX = (float)previewBounds.Width / ((TextureBrush)fillBrush).Image.Width;
					float scaleY = (float)previewBounds.Height / ((TextureBrush)fillBrush).Image.Height;
					((TextureBrush)fillBrush).ScaleTransform(scaleX, scaleY);
				} else {
					float scale = Geometry.CalcScaleFactor(((TextureBrush)fillBrush).Image.Width, ((TextureBrush)fillBrush).Image.Height, previewBounds.Width, previewBounds.Height);
					((TextureBrush)fillBrush).ScaleTransform(scale, scale);
					((TextureBrush)fillBrush).TranslateTransform((((TextureBrush)fillBrush).Image.Width * scale) / 2, (((TextureBrush)fillBrush).Image.Height * scale) / 2);
				}
			}
			// Draw
			if (fillBrush != Brushes.Transparent) gfx.FillRectangle(fillBrush, previewBounds);
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, ILineStyle lineStyle) {
			Pen linePen = ToolCache.GetPen(lineStyle, null, null);
			int height = lineStyle.LineWidth + 2;
			bool scalePen = (height > previewBounds.Height);
			if (scalePen) {
				float scale = Geometry.CalcScaleFactor(height, height, previewBounds.Width, previewBounds.Height);
				linePen.ScaleTransform(scale, scale);
			}
			gfx.DrawLine(linePen, previewBounds.X, previewBounds.Y + (previewBounds.Height / 2), previewBounds.Right, previewBounds.Y + (previewBounds.Height / 2));
			if (scalePen) linePen.ResetTransform();
		}


		private void DrawStyleItem(Graphics gfx, Rectangle previewBounds, IParagraphStyle paragraphStyle) {
			StringFormat stringFormat = ToolCache.GetStringFormat(paragraphStyle);
			Rectangle r = Rectangle.Empty;
			r.X = previewBounds.X + paragraphStyle.Padding.Left;
			r.Y = previewBounds.Y + paragraphStyle.Padding.Top;
			r.Width = (previewBounds.Width * 10) - (paragraphStyle.Padding.Left + paragraphStyle.Padding.Right);
			r.Height = (previewBounds.Height * 10) - (paragraphStyle.Padding.Top + paragraphStyle.Padding.Bottom);

			// Transform
			float scale = Geometry.CalcScaleFactor(r.Width, r.Height, previewBounds.Width, previewBounds.Height);
			gfx.ScaleTransform(scale, scale);

			// Draw
			gfx.DrawString(_previewText, Control.DefaultFont, Brushes.Black, r, stringFormat);
		}


		static StyleUITypeEditor() {
			_previewText = "This is the first line of the sample text."
				+ Environment.NewLine + "This is line 2 of the text."
				+ Environment.NewLine + "Line 3 of the text.";
		}


		#region Fields

		private static Project _projectBuffer;
		private static Design _designBuffer;
		private static readonly string _previewText;
		private Project _project;
		private Design _design;
		private StringFormat _formatter = new StringFormat(StringFormatFlags.NoWrap);

		#endregion
	}

	#endregion

}

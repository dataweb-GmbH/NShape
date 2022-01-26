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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;
using System.ComponentModel;
using System.IO;

namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// Dialog for exporting a diagram to a graphics file.
	/// </summary>
	[ToolboxItem(false)]
	public partial class ExportDiagramDialog : Form {
		
		/// <summary>
		/// Initializes a new Instance of <see cref="T:Dataweb.NShape.WinFormsUI.ExportDiagramDialog" />.
		/// </summary>
		public ExportDiagramDialog(IDiagramPresenter diagramPresenter) {
			InitializeComponent();
			Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);

			if (diagramPresenter == null) throw new ArgumentNullException("diagramPresenter");
			this._diagramPresenter = diagramPresenter;
			InitializeDialog();
		}


		#region [Private] Methods

		private void InitializeDialog() {
			dpiComboBox.Items.Clear();
			dpiComboBox.Items.Add(72);
			dpiComboBox.Items.Add(150);
			dpiComboBox.Items.Add(300);
			dpiComboBox.Items.Add(600);

			Graphics infoGfx = _diagramPresenter.Diagram.DisplayService.InfoGraphics;
			for (int i = dpiComboBox.Items.Count - 1; i >= 0; --i) {
				System.Diagnostics.Debug.Assert(dpiComboBox.Items[i] is int);
				if ((int)dpiComboBox.Items[i] < infoGfx.DpiY) {
					dpiComboBox.Items.Insert(i + 1, (int)infoGfx.DpiY);
					dpiComboBox.SelectedIndex = i + 1;
					break;
				} else if (i == 0) {
					dpiComboBox.Items.Insert(i, (int)infoGfx.DpiY);
					dpiComboBox.SelectedIndex = i;
				}
			}
			 
			colorLabel.BackColor = Color.White;

			backColorCheckBox.Checked = false;
			marginUpDown.Value = 0;
			emfPlusRadioButton.Checked = true;
			toFileRadioButton.Checked = true;
			if (_diagramPresenter.SelectedShapes.Count > 0) {
				exportSelectedRadioButton.Enabled =
				exportSelectedRadioButton.Checked = true;
			} else {
				exportSelectedRadioButton.Enabled = false;
				exportAllRadioButton.Checked = true;
			}
			visibleLayersOnlyCheckBox.Checked = (!EnumerationHelper.IsEmpty(_diagramPresenter.HiddenLayerIds));

			EnableOkButton();
		}


		private void RefreshPreview() {
			DeleteImage();
			previewPanel.Invalidate();
		}


		private void CreateImage() {
			if (_image != null) _image.Dispose();
			try {
				// Check if image dimensions are valid
				Graphics infoGfx = _diagramPresenter.Diagram.DisplayService.InfoGraphics;
				int imgWidth = (int)Math.Round((_dpi / infoGfx.DpiX) * _diagramPresenter.Diagram.Width);
				int imgHeight = (int)Math.Round((_dpi / infoGfx.DpiY) * _diagramPresenter.Diagram.Height);
				bool createImage = true;
				if (Math.Min(imgWidth, imgHeight) <= 0 || (imgWidth * imgHeight) > Diagram.MaxBitmapPixelCount) {
					string msgTxt = string.Format(
						"The selected resolution would result in a {0}x{1} pixels bitmap which may be too big when running in 32 bit mode. {2}Do you want to continue?", 
						imgWidth, imgHeight, Environment.NewLine);
					DialogResult res = MessageBox.Show(this, msgTxt, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
					createImage = (res == DialogResult.Yes);
				} 
				if (createImage) {
					Cursor = Cursors.WaitCursor;
					try {
						_image = _diagramPresenter.Diagram.CreateImage(_imageFormat,
							_shapes,
							visibleLayersOnlyCheckBox.Checked ? _diagramPresenter.GetVisibleLayerIds() : null,
							_margin,
							withBackgroundCheckBox.Checked,
							_backgroundColor,
							_dpi);
						if (_image != null) {
							GraphicsUnit unit = GraphicsUnit.Display;
							_imageBounds = Rectangle.Round(_image.GetBounds(ref unit));
						}
					} catch (Exception exc) {
						MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					} finally {
						Cursor = Cursors.Default;
					}
				}
			} catch (Exception exc) {
				MessageBox.Show(this, string.Format("Error while creating image: {0}", exc.Message), "Error while creating image", MessageBoxButtons.OK, MessageBoxIcon.Error);
				_image = new Bitmap(1, 1);
			}
		}


		private void UpdateFileExtension() {
			if (!string.IsNullOrEmpty(_filePath)) {
				string currExt = Path.GetExtension(_filePath);
				string newExt = GetFileExtension(_imageFormat);
				if (string.IsNullOrEmpty(currExt))
					filePathTextBox.Text = _filePath + newExt;
				else {
					bool updateExtension = true;
					if (!IsStandardExtension(currExt)) {
						string msgTxt = string.Format("Do you want the file extension to be updated to '{0}'?", newExt);
						updateExtension = (MessageBox.Show(this, msgTxt, "Update Extension", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes);
					}
					if (updateExtension) {
						if (string.Compare(currExt, newExt, StringComparison.InvariantCultureIgnoreCase) != 0)
							SetFilePath(_filePath.Replace(currExt, newExt));
					}
				}
			}
		}


		private bool IsStandardExtension(string extension) {
			return (string.Compare(extension, fileExtBMP, StringComparison.InvariantCultureIgnoreCase) == 0
				|| string.Compare(extension, fileExtEMF, StringComparison.InvariantCultureIgnoreCase) == 0
				|| string.Compare(extension, fileExtGIF, StringComparison.InvariantCultureIgnoreCase) == 0
				|| string.Compare(extension, fileExtJPEG, StringComparison.InvariantCultureIgnoreCase) == 0
				|| string.Compare(extension, fileExtJPG, StringComparison.InvariantCultureIgnoreCase) == 0
				|| string.Compare(extension, fileExtPNG, StringComparison.InvariantCultureIgnoreCase) == 0
				|| string.Compare(extension, fileExtSVG, StringComparison.InvariantCultureIgnoreCase) == 0
				|| string.Compare(extension, fileExtTIFF, StringComparison.InvariantCultureIgnoreCase) == 0);
		}
		
		
		private string GetFileExtension(ImageFileFormat imageFileFormat) {
			switch (imageFileFormat) {
				case ImageFileFormat.Bmp: return fileExtBMP;
				case ImageFileFormat.Emf:
				case ImageFileFormat.EmfPlus: return fileExtEMF;
				case ImageFileFormat.Gif: return fileExtGIF;
				case ImageFileFormat.Jpeg: return fileExtJPG;
				case ImageFileFormat.Png: return fileExtPNG;
				case ImageFileFormat.Svg: return fileExtSVG;
				case ImageFileFormat.Tiff: return fileExtTIFF;
				default: return string.Empty;
			}
		}


		private void SetFilePath(string path) {
			_filePath = path;
			if (filePathTextBox.Text != _filePath) {
				filePathTextBox.Text = _filePath;
				_filePathChanged = true;
			}
			EnableOkButton();
		}


		private bool CanOverwriteFile(string path) {
			if (File.Exists(path)) {
				string msgTxt = string.Format("The file '{0}' already exists. Do you want to overwrite it?", path);
				DialogResult res = MessageBox.Show(this, msgTxt, "Overwrite file", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				return (res == System.Windows.Forms.DialogResult.Yes);
			} else return true;
		}


		private void ExportImage() {
			// Update file extension
			if (string.IsNullOrEmpty(Path.GetExtension(_filePath)))
				SetFilePath(_filePath + GetFileExtension(_imageFormat));
			// Check if the file path changed since last export
			if (_filePathChanged && !CanOverwriteFile(_filePath))
				return;

			if (_image == null) CreateImage();
			if (_image != null) {
				switch (_imageFormat) {
					case ImageFileFormat.Emf:
					case ImageFileFormat.EmfPlus:
						if (_exportToClipboard) {
							try {
								ClipboardHelper.OpenClipboard(this.Handle);
								ClipboardHelper.EmptyClipboard();
								ClipboardHelper.AddEnhMetafileToClipboard((Metafile)_image.Clone());
							} finally {
								ClipboardHelper.CloseClipboard();
							}
						} else 
							GdiHelpers.SaveImage(_image, _filePath, _imageFormat, _compressionQuality);
						break;
					case ImageFileFormat.Bmp:
					case ImageFileFormat.Gif:
					case ImageFileFormat.Jpeg:
					case ImageFileFormat.Png:
					case ImageFileFormat.Tiff:
						if (_exportToClipboard)
							Clipboard.SetImage((Image)_image.Clone());
						else GdiHelpers.SaveImage(_image, _filePath, _imageFormat, _compressionQuality);
						break;
					case ImageFileFormat.Svg:
						throw new NotImplementedException();
					default: throw new NShapeUnsupportedValueException(_imageFormat);
				}
			}
			_filePathChanged = false;
		}


		private void DeleteImage() {
			if (_image != null) _image.Dispose();
			_image = null;
		}


		private void SetBackgroundColor(Color color) {
			_backgroundColor = color;
			if (_colorLabelFrontBrush != null) _colorLabelFrontBrush.Dispose();
			_colorLabelFrontBrush = null;
			colorLabel.Invalidate();
			RefreshPreview();
		}


		private void EnableOkButton() {
			exportButton.Enabled =
			okButton.Enabled = (_exportToClipboard || !string.IsNullOrEmpty(_filePath));
		}


		private void EnableFileSelection() {
			filePathTextBox.Enabled =
			browseButton.Enabled = !_exportToClipboard;
			EnableResolutionAndQualitySelection();
		}


		private void EnableResolutionAndQualitySelection() {
			bool enable;
			switch (_imageFormat) {
				case ImageFileFormat.EmfPlus:
				case ImageFileFormat.Emf:
				case ImageFileFormat.Svg:
					enable = false; break;
				case ImageFileFormat.Bmp:
				case ImageFileFormat.Gif:
				case ImageFileFormat.Jpeg:
				case ImageFileFormat.Png:
				case ImageFileFormat.Tiff:
					enable = true; break;
				default: 
					enable = false; break;
			}
			dpiLabel.Enabled =
			dpiComboBox.Enabled = enable;
			// Quality has only an effect on JPG images
			qualityLabel.Enabled =
			qualityTrackBar.Enabled = (_imageFormat == ImageFileFormat.Jpeg);
		}

		#endregion


		#region [Private] "File Format Options" event handler implementations

		private void emfPlusRadioButton_CheckedChanged(object sender, EventArgs e) {
			if (emfPlusRadioButton.Checked) {
				_imageFormat = ImageFileFormat.EmfPlus;
				descriptionLabel.Text = emfPlusDescription;
				UpdateFileExtension();
				EnableResolutionAndQualitySelection();
				RefreshPreview();
			}
		}


		private void emfRadioButton_CheckedChanged(object sender, EventArgs e) {
			if (emfRadioButton.Checked) {
				_imageFormat = ImageFileFormat.Emf;
				descriptionLabel.Text = emfDescription;
				UpdateFileExtension();
				EnableResolutionAndQualitySelection();
				RefreshPreview();
			}
		}


		private void pngRadioButton_CheckedChanged(object sender, EventArgs e) {
			if (pngRadioButton.Checked) {
				_imageFormat = ImageFileFormat.Png;
				descriptionLabel.Text = pngDescription;
				UpdateFileExtension();
				EnableResolutionAndQualitySelection();
				RefreshPreview();
			}
		}


		private void jpgRadioButton_CheckedChanged(object sender, EventArgs e) {
			if (jpgRadioButton.Checked) {
				_imageFormat = ImageFileFormat.Jpeg;
				descriptionLabel.Text = jpgDescription;
				UpdateFileExtension();
				EnableResolutionAndQualitySelection();
				backColorCheckBox.Checked = true;
				RefreshPreview();
			}
		}


		private void bmpRadioButton_CheckedChanged(object sender, EventArgs e) {
			if (bmpRadioButton.Checked) {
				_imageFormat = ImageFileFormat.Bmp;
				descriptionLabel.Text = bmpDescription;
				UpdateFileExtension();
				EnableResolutionAndQualitySelection();
				backColorCheckBox.Checked = true;
				RefreshPreview();
			}
		}

		#endregion


		#region [Private] "Export Options" event handler implementations
		
		private void toClipboardRadioButton_CheckedChanged(object sender, EventArgs e) {
			_exportToClipboard = true;
			EnableFileSelection();
			EnableOkButton();
		}


		private void toFileRadioButton_CheckedChanged(object sender, EventArgs e) {
			_exportToClipboard = false;
			EnableFileSelection();
			EnableOkButton();
		}


		private void filePathTextBox_TextChanged(object sender, EventArgs e) {
			if (filePathTextBox.Text != _filePath)
				SetFilePath(filePathTextBox.Text);
		}

	
		private void browseButton_Click(object sender, EventArgs e) {
			string fileFilter = null;
			switch (_imageFormat) {
				case ImageFileFormat.Bmp: fileFilter = "Bitmap Picture Files|*.bmp|All Files|*.*"; break;
				case ImageFileFormat.EmfPlus: 
				case ImageFileFormat.Emf: fileFilter = "Enhanced Meta Files|*.emf|All Files|*.*"; break;
				case ImageFileFormat.Gif: fileFilter = "Graphics Interchange Format Files|*.gif|All Files|*.*"; break;
				case ImageFileFormat.Jpeg: fileFilter = "Joint Photographic Experts Group (JPEG) Files|*.jpeg;*.jpg|All Files|*.*"; break;
				case ImageFileFormat.Png: fileFilter = "Portable Network Graphics Files|*.png|All Files|*.*"; break;
				case ImageFileFormat.Tiff: fileFilter = "Tagged Image File Format Files|*.tiff;*.tif|All Files|*.*"; break;
				default: throw new NShapeUnsupportedValueException(_imageFormat);
			}
			string fileName = string.Empty;
			saveFileDialog.FileName = _filePath;
			saveFileDialog.Filter = fileFilter;
			saveFileDialog.AddExtension = true;
			saveFileDialog.DefaultExt = GetFileExtension(_imageFormat);
			saveFileDialog.SupportMultiDottedExtensions = true;
			saveFileDialog.OverwritePrompt = false;
			if (saveFileDialog.ShowDialog() == DialogResult.OK) {
				if (string.Compare(fileName, saveFileDialog.FileName) != 0) {
					if (CanOverwriteFile(saveFileDialog.FileName)) {
						fileName = saveFileDialog.FileName;
						if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
							fileName += GetFileExtension(_imageFormat);
						// Update file path
						SetFilePath(fileName);
						_filePathChanged = false;
					}
				}
			}
		}


		private void dpiComboBox_SelectedValueChanged(object sender, EventArgs e) {
		}


		private void dpiComboBox_SelectedIndexChanged(object sender, EventArgs e) {
		}


		private void dpiComboBox_TextChanged(object sender, EventArgs e) {
			int value;
			if (!int.TryParse(dpiComboBox.Text, out value)) {
				if (!string.IsNullOrEmpty(dpiComboBox.Text))
					dpiComboBox.Text = string.Empty;
				value = -1;
			} else {
				if (previewCheckBox.Checked) {
					// Deactivate preview if image would be very large
					// ToDo: Create image with a worker thread
					Graphics infoGfx = _diagramPresenter.Diagram.DisplayService.InfoGraphics;
					int imgWidth = (int)Math.Round((value / infoGfx.DpiX) * _diagramPresenter.Diagram.Width);
					int imgHeight = (int)Math.Round((value / infoGfx.DpiY) * _diagramPresenter.Diagram.Height);
					if (Math.Max(imgWidth, imgHeight) > _deactivatePreviewDimension) {
						string msg = string.Format("The resulting image will have {0}x{1} pixels and drawing a preview of this image might be slow on some machines.\nDo you wand to deactivate the preview option?", 
															imgWidth, imgHeight);
						DialogResult result = MessageBox.Show(this, msg, "Deactivate preview?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
						switch (result) {
							case DialogResult.Cancel:
								dpiComboBox.Text = _dpi.ToString();
								value = _dpi;
								break;
							case DialogResult.Yes:
								previewCheckBox.Checked = false;
								break;
							case DialogResult.No:
								// Update dimensions that are ok for the user
								_deactivatePreviewDimension = Math.Max(imgWidth, imgHeight);
								break;
							default: Debug.Fail("Unhandled switch case!"); break;
						}
					}
				}
			}
			// Set value and refresh preview image
			if (value > 0 && value != _dpi) {
				_dpi = value;
				RefreshPreview();
			}
		}


		private void qualityTrackBar_ValueChanged(object sender, EventArgs e) {
			_compressionQuality = (byte)qualityTrackBar.Value;
		}
		
		#endregion


		#region [Private] "Content Options" event handler implementations

		private void exportSelectedRadioButton_CheckedChanged(object sender, EventArgs e) {
			_shapes = _diagramPresenter.SelectedShapes;
			RefreshPreview();
		}


		private void exportAllRadioButton_CheckedChanged(object sender, EventArgs e) {
			_shapes = _diagramPresenter.Diagram.Shapes;
			RefreshPreview();
		}


		private void exportDiagramRadioButton_CheckedChanged(object sender, EventArgs e) {
			_shapes = null;
			RefreshPreview();
		}


		private void visibleLayersOnlyCheckBox_CheckedChanged(object sender, EventArgs e) {
			RefreshPreview();
		}


		private void withBackgroundCheckBox_CheckedChanged(object sender, EventArgs e) {
			RefreshPreview();
		}


		private void marginUpDown_ValueChanged(object sender, EventArgs e) {
			_margin = (int)marginUpDown.Value;
			RefreshPreview();
		}


		private void marginUpDown_KeyUp(object sender, KeyEventArgs e) {
			marginUpDown_ValueChanged(sender, EventArgs.Empty);
		}


		private void backColorCheckBox_CheckedChanged(object sender, EventArgs e) {
			if (backColorCheckBox.Checked) 
				SetBackgroundColor(colorLabel.BackColor);
			else SetBackgroundColor(Color.Transparent);
		}


		private void selectBackColor_Click(object sender, EventArgs e) {
			colorDialog.Color = _backgroundColor;
			colorDialog.SolidColorOnly = false;
			colorDialog.AllowFullOpen = true;
			colorDialog.AnyColor = true;
			if (colorDialog.ShowDialog(this) == DialogResult.OK) {
				colorLabel.BackColor = colorDialog.Color;
				if (backColorCheckBox.Checked) SetBackgroundColor(colorLabel.BackColor);
				else backColorCheckBox.Checked = true;
			}
		}
		
		#endregion


		#region [Private] Event handler implementations

		private void previewCheckBox_CheckedChanged(object sender, EventArgs e) {
			RefreshPreview();
		}

	
		private void previewPanel_Paint(object sender, PaintEventArgs e) {
			if (previewCheckBox.Checked) {
				try {
					// Apply graphics settings
					GdiHelpers.ApplyGraphicsSettings(e.Graphics, RenderingQuality.MaximumQuality);
					// Create image
					if (_image == null) CreateImage();
					if (_image != null) {
						if (_imgAttribs == null) _imgAttribs = GdiHelpers.GetImageAttributes(_imageLayout);
						// Draw image
						Rectangle bounds = previewPanel.ClientRectangle;
						GdiHelpers.DrawImage(e.Graphics, _image, _imgAttribs, _imageLayout, bounds);
					}
				} catch (Exception exc) {
					string errMsg = string.Format("Error while drawing preview image: {0}{1}Preview option checkbox will be disabled.", 
						exc.Message, Environment.NewLine);
					MessageBox.Show(this, errMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					previewCheckBox.Checked = false;
				}
			}
		}


		private void backColorLabel_Paint(object sender, PaintEventArgs e) {
			// Draw a pattern in order to make transparency of colors visible
			if (_colorLabelBackBrush == null)
				_colorLabelBackBrush = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.White, Color.Black);
			if (_colorLabelFrontBrush == null) _colorLabelFrontBrush = new SolidBrush(_backgroundColor);
			e.Graphics.FillRectangle(_colorLabelBackBrush, e.ClipRectangle);
			e.Graphics.FillRectangle(_colorLabelFrontBrush, e.ClipRectangle);
		}

	
		private void exportButton_Click(object sender, EventArgs e) {
			ExportImage();
		}


		private void okButton_Click(object sender, EventArgs e) {
			ExportImage();
			DialogResult = DialogResult.OK;
			if (!Modal) Close();
			DeleteImage();
		}


		private void cancelButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			if (!Modal) Close();
			DeleteImage();
		}

		#endregion


		static ExportDiagramDialog() {
			emfPlusDescription = "Windows Enhanced Metafile Plus Dual (*" + fileExtEMF + ")" + Environment.NewLine
			+ "Creates a high quality vector image file supporting transparency, translucency and anti-aliasing. The Emf Plus Dual file format is backwards compatible with the classic Emf format.";
			emfDescription = "Windows Enhanced Metafile (*" + fileExtEMF + ")" + Environment.NewLine
				+ "Creates a low quality vector image file supporting transparency and (emulated) translucency.";
			pngDescription = "Portable Network graphics (*" + fileExtPNG + ")" + Environment.NewLine
				+ "Creates a bitmap image file supporting transparency. The Png file format provides medium but lossless compression.";
			jpgDescription = "Joint Photographic Experts Group (*" + fileExtJPG + ")" + Environment.NewLine
				+ "Creates a compressed bitmap image file. The Jpg file format does not support transparency. It provides an adjustable (lossy) compression.";
			bmpDescription = "Bitmap (*" + fileExtBMP + ")" + Environment.NewLine
				+ "Creates an uncompressed bitmap image file. The Bmp file format does not support transparency.";
			_deactivatePreviewDimension = 3000;
		}


		#region Fields

		private const string fileExtBMP = ".bmp";
		private const string fileExtEMF = ".emf";
		private const string fileExtGIF = ".gif";
		private const string fileExtJPG = ".jpg";
		private const string fileExtJPEG = ".jpeg";
		private const string fileExtPNG = ".png";
		private const string fileExtSVG = ".svg";
		private const string fileExtTIFF = ".tiff";

		private static readonly string emfPlusDescription;
		private static readonly string emfDescription;
		private static readonly string pngDescription;
		private static readonly string jpgDescription;
		private static readonly string bmpDescription;
		
		// Store in static field in order to preserve the decision as long as the application is running
		private static int _deactivatePreviewDimension;

		// Rendering stuff
		private const ImageLayoutMode _imageLayout = ImageLayoutMode.Fit;
		private Image _image = null;
		private Rectangle _imageBounds = Rectangle.Empty;

		// Image content stuff
		private int _margin;
		private int _dpi;
		private Color _backgroundColor = Color.Transparent;
		private IEnumerable<Shape> _shapes;

		// Export stuff
		private ImageFileFormat _imageFormat;
		private bool _exportToClipboard;
		private byte _compressionQuality = 75;
		private string _filePath = null;
		private bool _filePathChanged = false;

		// Fields
		private IDiagramPresenter _diagramPresenter = null;
		private ImageAttributes _imgAttribs = null;
		private Brush _colorLabelBackBrush = null;
		private Brush _colorLabelFrontBrush = null;

		#endregion

	}
}

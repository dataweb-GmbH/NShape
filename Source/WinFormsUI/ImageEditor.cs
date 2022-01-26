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
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// UI type editor for properties of type NamedImage.
	/// </summary>
	public partial class ImageEditor : Form {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.ImageEditor" />.
		/// </summary>
		public ImageEditor() {
			InitializeComponent();
			Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.ImageEditor" />.
		/// </summary>
		public ImageEditor(string fileName)
			: this() {
			if (fileName == null) throw new ArgumentNullException("fileName");
			_resultImage.Load(fileName);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.ImageEditor" />.
		/// </summary>
		public ImageEditor(Image image, string path)
			: this() {
			if (image == null) throw new ArgumentNullException("image");
			if (path == null) throw new ArgumentNullException("name");
			_resultImage.Image = (Image)image.Clone();
			_resultImage.Name = path;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.ImageEditor" />.
		/// </summary>
		public ImageEditor(Image image)
			: this(image, string.Empty) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.WinFormsUI.ImageEditor" />.
		/// </summary>
		public ImageEditor(NamedImage namedImage) {
			InitializeComponent();
			if (namedImage == null) throw new ArgumentNullException("namedImage");
			if (namedImage.Image != null)
				_resultImage.Image = (Image)namedImage.Image.Clone();
			_resultImage.FilePath = namedImage.FilePath;
			_resultImage.Name = namedImage.Name;
		}


		/// <summary>
		/// Specifies the image selected by the user.
		/// </summary>
		public NamedImage Result {
			get { return _resultImage; }
		}
		
		
		/// <override></override>
		protected override void OnShown(EventArgs e) {
			base.OnShown(e);
			DisplayResult();
		}


		/// <override></override>
		protected override void OnFormClosed(FormClosedEventArgs e) {
			base.OnFormClosed(e);
			pictureBox.Image = null;
		}


		private Image Image {
			get { return _resultImage.Image; }
			set {
				_resultImage.Image = value;
				DisplayResult();
			}
		}


		private bool ReplaceImageName {
			get { return (nameTextBox.Text == Path.GetFileNameWithoutExtension(_resultImage.FilePath)); }
		}


		private void ClearAll() {
			this.SuspendLayout();
			_resultImage.Name = null;
			_resultImage.FilePath = null;
			if (_resultImage.Image != null)
				_resultImage.Image.Dispose();
			_resultImage.Image = null;
			DisplayResult();
			this.ResumeLayout();
		}


		private void DisplayResult() {
			this.SuspendLayout();
			pictureBox.Image = _resultImage.Image;
			pathTextBox.Text = _resultImage.FilePath;
			nameTextBox.Text = _resultImage.Name;
			this.ResumeLayout();
		}


		private void UpdateImage(String imageFilePath) {
			this.SuspendLayout();

			if (ReplaceImageName)
				_resultImage.Name = Path.GetFileNameWithoutExtension(imageFilePath);
			_resultImage.Image = null;
			_resultImage.Load(imageFilePath);

			DisplayResult();
			this.ResumeLayout();
		}


		private void okButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.OK;
			if (!Modal) Close();
		}

		
		private void cancelButton_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
			if (!Modal) Close();
		}

		
		private void clearButton_Click(object sender, EventArgs e) {
			ClearAll();
		}

		
		private void browseButton_Click(object sender, EventArgs e) {
			openFileDialog.Filter = "Image Files|*.Bmp;*.Emf;*.Exif;*.Gif;*.Ico;*.Jpg;*.Jpeg;*.Png;*.Tiff;*.Wmf|All files (*.*)|*.*";
			if (nameTextBox.Text != string.Empty)
				openFileDialog.InitialDirectory = Path.GetDirectoryName(nameTextBox.Text);
			
			if (openFileDialog.ShowDialog(this) == DialogResult.OK)
				UpdateImage(openFileDialog.FileName);
		}


		private void nameTextBox_TextChanged(object sender, EventArgs e) {
			_resultImage.Name = nameTextBox.Text;
		}


		private void pathTextBox_TextChanged(object sender, EventArgs e) {
			bool updateImage = (string.Compare(pathTextBox.Text, _resultImage.FilePath, StringComparison.InvariantCultureIgnoreCase) != 0);
			_resultImage.FilePath = pathTextBox.Text;

			if (updateImage) {
				if (_resultImage.Image != null)
					_resultImage.Dispose();
				pictureBox.Image = null;

				_resultImage.Image = null;
				if (!string.IsNullOrEmpty(pathTextBox.Text) && File.Exists(pathTextBox.Text))
					UpdateImage(pathTextBox.Text);
			}
		}


		private NamedImage _resultImage = new NamedImage();

	}

}
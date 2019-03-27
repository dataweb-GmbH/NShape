using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Dataweb.NShape.WinFormsUI {

	/// <summary>
	/// A MessageBox like input dialog.
	/// </summary>
	public partial class InputBox : Form {

		/// <ToBeCompleted></ToBeCompleted>
		public InputBox(string prompt, string title) {
			InitializeComponent();
			promptLabel.Text = prompt;
			this.Text = title;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InputBox(string prompt) 
		:this(prompt, String.Empty) {			
		}


		/// <ToBeCompleted></ToBeCompleted>
		public string InputText {
			get {return inputTextBox.Text;}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static string Show(Control parent, string title, string prompt) {
			string result = null;

			InputBox inputBox = new InputBox(prompt, title);
			if (parent != null) {
				inputBox.Parent = parent;
				inputBox.StartPosition = FormStartPosition.CenterParent;
			} else
				inputBox.StartPosition = FormStartPosition.CenterScreen;
			DialogResult res = inputBox.ShowDialog();
			if (res == DialogResult.OK)
				result = inputBox.InputText;
			
			return result;
		}

	}

}

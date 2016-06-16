/******************************************************************************
  Copyright 2009-2012 dataweb GmbH
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
using System.Xml;


namespace Dataweb.Xml {

	public class XmlScanner {

		public XmlScanner(string fileName) {
			reader = new XmlTextReader(fileName);
		}


		/// <summary>
		/// Reads the next element on the same level as the current element.
		/// </summary>
		/// <returns></returns>
		public bool ReadElement() {
			bool result = reader.Read();
			while (result && reader.NodeType == XmlNodeType.Whitespace)
				result = reader.Read();
			if (reader.NodeType == XmlNodeType.EndElement)
				result = false;
			return result;
		}


		public bool ReadElement(string name) {
			bool result = ReadElement();
			if (result)
				if (reader.Name != name) throw new Exception("Unexpected XML tag.");
			return result;
		}


		/// <summary>
		/// Reads the next attribute of the current element
		/// </summary>
		/// <returns></returns>
		public bool ReadAttribute() {
			return reader.MoveToNextAttribute();
		}


		/// <summary>
		/// Reads the first child of the current element.
		/// </summary>
		/// <returns></returns>
		public bool ReadChild() {
			bool result = reader.Read();
			while (result && reader.NodeType == XmlNodeType.Whitespace)
				result = reader.Read();
			if (reader.NodeType == XmlNodeType.EndElement)
				result = false;
			return result;
		}


		public bool ReadChild(string name) {
			bool result = ReadChild();
			if (reader.Name != name) throw new Exception("Unexpected XML tag.");
			return result;
		}


		/// <summary>
		/// Reads an end tag
		/// </summary>
		public bool ReadParent() {
			bool result = true;
			while (result && reader.NodeType == XmlNodeType.Whitespace)
				result = reader.Read();
			if (!result || reader.NodeType != XmlNodeType.EndElement)
				throw new Exception("End tag expected");
			result = reader.Read();
			while (result && reader.NodeType == XmlNodeType.Whitespace)
				result = reader.Read();
			return result;
		}


		public string Name {
			get { return reader.Name; }
		}


		public int IntValue {
			get { return int.Parse(reader.Value); }
		}


		public string StringValue {
			get { return reader.Value; }
		}


		public void Close() {
			reader.Close();
			reader = null;
		}

		
		private XmlReader reader;
	}

}

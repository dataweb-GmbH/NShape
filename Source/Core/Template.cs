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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape {

	/// <summary>
	/// Combines a shape and a model object to form a sample for shape creation.
	/// </summary>
	public class Template : IEntity {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.Template" />.
		/// </summary>
		public Template(string name, Shape shape) {
			if (name == null) throw new ArgumentNullException("name");
			if (shape == null) throw new ArgumentNullException("shape");
			this._name = name;
			this._shape = shape;
		}


		/// <override></override>
		public override string ToString() {
			return Title;
		}


		/// <summary>
		/// Creates a new <see cref="T:Dataweb.NShape.Advanced.Template" /> that is a copy of the current instance.
		/// </summary>
		public Template Clone() {
			Template result = new Template();
			result.CopyFrom(this);
			return result;
		}


		/// <summary>
		/// Copies all properties and fields from the source template to this template.
		/// The shape and the model obejcts will be cloned.
		/// </summary>
		public void CopyFrom(Template source) {
			if (source == null) throw new ArgumentNullException("source");

			this._name = source._name;
			this._title = source._title;
			this._description = source._description;
			
			// Clone or copy shape
			if (this._shape == null)	this._shape = ShapeDuplicator.CloneShapeAndModelObject(source._shape);
			else this._shape.CopyFrom(source._shape);	// Template will be copied although this is not desirable

			// copy connection point mapping
			this._connectionPointMappings.Clear();
			foreach (KeyValuePair<ControlPointId, TerminalId> item in source._connectionPointMappings)
				this._connectionPointMappings.Add(item.Key, item.Value);

			// copy property mapping
			this._propertyMappings.Clear();
			foreach (KeyValuePair<int, IModelMapping> item in source._propertyMappings)
				this._propertyMappings.Add(item.Key, item.Value.Clone());
		}


		/// <summary>
		/// Gets or sets an object that provides additional data.
		/// </summary>
		public object Tag {
			get { return _tag; }
			set { _tag = value; }
		}
		
		
		/// <summary>
		/// Specifies the culture independent name.
		/// </summary>
		public string Name {
			get { return _name; }
			set { _name = value; }
		}


		/// <summary>
		/// Specifies the culture dependent display name.
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
		/// A descriptive text for the template.
		/// </summary>
		public string Description {
			get { return _description; }
			set { _description = value; }
		}


		/// <summary>
		/// Defines the shape for this template. If the template contains a ModelObject, it will also become the shape's ModelObject.
		/// </summary>
		/// <remarks>Replacing the shape of a template with templated shapes results in 
		/// errors, if the templated shapes are not updated accordingly.</remarks>
		public Shape Shape {
			get { return _shape; }
			set {
				if (_shape != null) {
				    if (_shape.ModelObject != null && value != null && value.ModelObject != null) {
				        // If both shapes have ModelObejct instances assigned, 
				        // try to keep as many mappings as possible
				        // ToDo: try to copy property mappings
				        CopyTerminalMappings(_shape.ModelObject, value.ModelObject);
				    } else {
				        // Delete all mappings to restore default behavior
				        UnmapAllProperties();
				        UnmapAllTerminals();
				    }
				}
				_shape = value;
			}
		}


		/// <summary>
		/// Creates a new shape from this template.
		/// </summary>
		public Shape CreateShape() {
			return CreateShape<Shape>();
		}


		/// <summary>
		/// Creates a new shape from this template.
		/// </summary>
		public TShape CreateShape<TShape>() where TShape: Shape {
			TShape result = (TShape)_shape.Type.CreateInstance(this);
			if (_shape.ModelObject != null)
				ShapeDuplicator.CloneModelObjectOnly(result);
			return result;
		}


		/// <summary>
		/// Creates a thumbnail of the template shape.
		/// </summary>
		/// <param name="size">Size of tumbnail in pixels</param>
		/// <param name="margin">Size of margin around shape in pixels</param>
		public Image CreateThumbnail(int size, int margin) {
			return CreateThumbnail(size, margin, Color.White);
		}


		/// <summary>
		/// Creates a thumbnail of the template shape.
		/// </summary>
		/// <param name="size">Size of tumbnail in pixels</param>
		/// <param name="margin">Size of margin around shape in pixels</param>
		/// <param name="transparentColor">Specifies a color that will be rendered transparent.</param>
		public Image CreateThumbnail(int size, int margin, Color transparentColor) {
			Image bmp = new Bitmap(size, size, PixelFormat.Format32bppPArgb);
			using (Shape shapeClone = Shape.Clone())
				shapeClone.DrawThumbnail(bmp, margin, transparentColor);
			return bmp;
		}


		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		public IEnumerable<MenuItemDef> GetMenuItemDefs() {
			yield break;
		}


		#region Visualization Mapping

		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<IModelMapping> GetPropertyMappings() {
			return _propertyMappings.Values;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IModelMapping GetPropertyMapping(int modelPropertyId) {
			IModelMapping result = null;
			_propertyMappings.TryGetValue(modelPropertyId, out result);
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void MapProperties(IModelMapping propertyMapping) {
			if (propertyMapping == null) throw new ArgumentNullException("propertyMapping");
			if (_propertyMappings.ContainsKey(propertyMapping.ModelPropertyId))
				_propertyMappings[propertyMapping.ModelPropertyId] = propertyMapping;
			else
				_propertyMappings.Add(propertyMapping.ModelPropertyId, propertyMapping);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void UnmapProperties(IModelMapping propertyMapping) {
			if (propertyMapping == null) throw new ArgumentNullException("propertyMapping");
			_propertyMappings.Remove(propertyMapping.ModelPropertyId);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void UnmapAllProperties() {
			_propertyMappings.Clear();
		}

		#endregion


		#region Terminal Mapping

		/// <ToBeCompleted></ToBeCompleted>
		public TerminalId GetMappedTerminalId(ControlPointId connectionPointId) {
			// If there is a mapping, return it.
			TerminalId result;
			if (_connectionPointMappings.TryGetValue(connectionPointId, out result))
				return result;
			else {
				// if there is no mapping, return default values:
				if (_shape != null) {
					// - If the given point is no connectionPoint
					if (!_shape.HasControlPointCapability(connectionPointId, ControlPointCapabilities.Connect | ControlPointCapabilities.Glue))
						return TerminalId.Invalid;
					// - If a shape is set but no ModelObject, all connectionPoints are activated by default
					else if (_shape.ModelObject == null) return TerminalId.Generic;
					else return TerminalId.Invalid;
				} else return TerminalId.Invalid;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public string GetMappedTerminalName(ControlPointId connectionPointId) {
			TerminalId terminalId = GetMappedTerminalId(connectionPointId);
			if (terminalId == TerminalId.Invalid)
				return null;
			else {
				if (_shape.ModelObject != null)
					return _shape.ModelObject.Type.GetTerminalName(terminalId);
				else return _activatedTag;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void MapTerminal(TerminalId terminalId, ControlPointId connectionPointId) {
			// check if terminalId and connectionPointId are valid values
			if (_shape == null)
				throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_TemplateHasNoShape);
			if (!_shape.HasControlPointCapability(connectionPointId, ControlPointCapabilities.Glue | ControlPointCapabilities.Connect))
				throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageFmt_ControlPoint0IsNotAValidGlueOrConnectionPoint, connectionPointId);
			//
			if (_connectionPointMappings.ContainsKey(connectionPointId))
				_connectionPointMappings[connectionPointId] = terminalId;
			else
				_connectionPointMappings.Add(connectionPointId, terminalId);
		}


		/// <summary>
		/// Clears all mappings between the shape's connection points and the model's terminals.
		/// </summary>
		public void UnmapAllTerminals() {
			_connectionPointMappings.Clear();
		}

		#endregion


		#region IEntity Members

		/// <summary>
		/// The entity type name of <see cref="T:Dataweb.NShape.Advanced.Template" />.
		/// </summary>
		public static string EntityTypeName { get { return _entityTypeName; } }


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.Template" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
			// Program versions 1.0.0 through 1.0.3 store the template's title and description for repository version 2.
			// Since program version 1.0.4 (which introduced repository version 3), the template's title and description 
			// was no longer stored for repository version 2.
			// Program versions 2.2.x restores backwards compatibility with these early program versions while maintaining 
			// backwards compatibility with versions 1.0.4 through 2.x at the same time by implementing appropriate checks 
			// in the XmlStore.
			yield return new EntityFieldDefinition("Title", typeof(string));
			yield return new EntityFieldDefinition("Description", typeof(string));
			yield return new EntityInnerObjectsDefinition(_connectionPtMappingName + "s", _connectionPtMappingName, _connectionPtMappingAttrNames, _connectionPtMappingAttrTypes);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public object Id {
			get { return _id; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this._id != null) throw new InvalidOperationException("Template has already an id.");
			this._id = id;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SaveFields(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			writer.WriteString(_name);
			// See comment in GetPropertyDefinitions()
			writer.WriteString(_title);
			writer.WriteString(_description);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void LoadFields(IRepositoryReader reader, int version) {
			if (reader == null) throw new ArgumentNullException("reader");
			_name = reader.ReadString();
			// See comment in GetPropertyDefinitions()
			_title = reader.ReadString();
			_description = reader.ReadString();
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (writer == null) throw new ArgumentNullException("writer");
			if (propertyName == "ConnectionPointMappings") {
				// Save ConnectionPoint mappings
				writer.BeginWriteInnerObjects();
				foreach (KeyValuePair<ControlPointId, TerminalId> item in _connectionPointMappings) {
					writer.BeginWriteInnerObject();
					writer.WriteInt32((int)item.Key);
					writer.WriteInt32((int)item.Value);
					writer.EndWriteInnerObject();
				}
				writer.EndWriteInnerObjects();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (reader == null) throw new ArgumentNullException("reader");
			if (propertyName == "ConnectionPointMappings") {
				// load ConnectionPoint mappings			
				reader.BeginReadInnerObjects();
				while (reader.BeginReadInnerObject()) {
					ControlPointId connectionPointId = reader.ReadInt32();
					TerminalId terminalId = reader.ReadInt32();
					// The following is the essence of MapTerminal without the checks.
					if (_connectionPointMappings.ContainsKey(connectionPointId))
						_connectionPointMappings[connectionPointId] = terminalId;
					else
						_connectionPointMappings.Add(connectionPointId, terminalId);
					reader.EndReadInnerObject();
				}
				reader.EndReadInnerObjects();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Delete(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}

		#endregion


		// Used to create templates for loading.
		internal Template() {
		}


		private int CountControlPoints(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			int result = 0;
			foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.All))
				++result;
			return result;
		}


		/// <summary>
		/// Checks if the mappings between ConnectionPoints and Terminals can be reused
		/// </summary>
		private void CopyTerminalMappings(IModelObject oldModelObject, IModelObject newModelObject) {
			if (oldModelObject == null) throw new ArgumentNullException("oldModelObject");
			if (newModelObject == null) throw new ArgumentNullException("newModelObject");
			foreach (KeyValuePair<ControlPointId, TerminalId> item in _connectionPointMappings) {
				if (item.Value == TerminalId.Invalid) continue;
				string oldTerminalName = oldModelObject.Type.GetTerminalName(item.Value);
				string newTerminalName = newModelObject.Type.GetTerminalName(item.Value);
				if (oldTerminalName != newTerminalName)
					_connectionPointMappings[item.Key] = TerminalId.Invalid;
			}
		}


		#region Fields

		private static string _entityTypeName = "Core.Template";
		private static string _connectionPtMappingName = "ConnectionPointMapping";
		
		private static string[] _connectionPtMappingAttrNames = new string[] { "PointId", "TerminalId" };
		private static Type[] _connectionPtMappingAttrTypes = new Type[] { typeof(int), typeof(int) };		

		private const string _deactivatedTag = "Deactivated";
		private const string _activatedTag = "Activated";

		private object _id = null;
		private string _name;
		private string _title;
		private string _description;
		private Shape _shape;
		private object _tag;
		
		private Dictionary<ControlPointId, TerminalId> _connectionPointMappings = new Dictionary<ControlPointId, TerminalId>();
		private SortedList<int, IModelMapping> _propertyMappings = new SortedList<int, IModelMapping>();
		
		#endregion
	}

}
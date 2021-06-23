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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Defines the interface between the NShape framework and the model objects represented by a complete diagram.
	/// </summary>
	public interface IDiagramModelObject : IEntity, ISecurityDomainObject {

		/// <summary>
		/// Name of the model object. Is unique with siblings.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Type of this model object
		/// </summary>
		DiagramModelObjectType Type { get; }

		/// <summary>
		/// Creates a copy of this model object.
		/// </summary>
		/// <remarks>Composite objects are also cloned, references to aggregated objects are just copied.</remarks>
		IDiagramModelObject Clone();

		/// <summary>
		/// Retrieves the attached diagram.
		/// </summary>
		Diagram Diagram { get; }

		/// <summary>
		/// Attaches to a diagram.
		/// </summary>
		void AttachDiagram(Diagram diagram);

		/// <summary>
		/// Detaches from a diagram.
		/// </summary>
		void DetachDiagram();

		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		IEnumerable<MenuItemDef> GetMenuItemDefs();
	}


	/// <summary>
	/// Represents a model object type.
	/// </summary>
	public abstract class DiagramModelObjectType {

		/// <summary>
		/// Constructs a model object type.
		/// </summary>
		public DiagramModelObjectType(string name, string libraryName, string categoryTitle, CreateDiagramModelObjectDelegate createDiagramModelObjectDelegate,
			GetPropertyDefinitionsDelegate getPropertyDefinitionsDelegate) {
			if (name == null) throw new ArgumentNullException("name");
			if (!Project.IsValidName(name)) throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_0IsNotAValidModelObjectTypeName, name));
			if (libraryName == null) throw new ArgumentNullException("libraryName");
			if (!Project.IsValidName(libraryName)) throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_0IsNotAValidLibraryName, libraryName));
			if (createDiagramModelObjectDelegate == null) throw new ArgumentNullException("createDiagramModelObjectDelegate");
			if (getPropertyDefinitionsDelegate == null) throw new ArgumentNullException("getPropertyDefinitionsDelegate");
			//
			_name = name;
			_libraryName = libraryName;
			_categoryTitle = categoryTitle;
			_createModelObjectDelegate = createDiagramModelObjectDelegate;
			_getPropertyDefinitionsDelegate = getPropertyDefinitionsDelegate;
		}


		/// <summary>
		/// Specifies the language invariant name of the model object type.
		/// </summary>
		public string Name {
			get { return _name; }
		}


		/// <summary>
		/// Indicates the name of the library where the model object type is implemented.
		/// </summary>
		public string LibraryName {
			get { return _libraryName; }
		}


		/// <summary>
		/// Specifies the full language invariant name of the model object type.
		/// </summary>
		public string FullName {
			get { return string.Format("{0}.{1}", _libraryName, _name); }
		}


		/// <summary>
		/// Specifies the culture depending description of the model type.
		/// </summary>
		public string Description {
			get { return _description; }
			set { _description = value; }
		}


		/// <summary>
		/// Indicates the default for the culture depending category name.
		/// </summary>
		public string DefaultCategoryTitle {
			get { return _categoryTitle; }
		}


		/// <summary>
		/// Creates a model object instance of this type.
		/// </summary>
		public IDiagramModelObject CreateInstance() {
			return _createModelObjectDelegate(this);
		}


		/// <summary>
		/// Creates a model object instance of this type.
		/// </summary>
		public TDiagramModelObject CreateInstance<TDiagramModelObject>() where TDiagramModelObject : IDiagramModelObject {
			return (TDiagramModelObject)_createModelObjectDelegate(this);
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.ModelObjectType" />.
		/// </summary>
		public IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			return _getPropertyDefinitionsDelegate(version);
		}


		internal string GetDefaultName() {
			return string.Format("{0} {1}", _name, ++_nameCounter);
		}


		#region Fields

		private string _name;
		private string _libraryName;
		private string _description;
		private string _categoryTitle = string.Empty;
		private CreateDiagramModelObjectDelegate _createModelObjectDelegate;
		private GetPropertyDefinitionsDelegate _getPropertyDefinitionsDelegate;

		private int _nameCounter = 0;

		#endregion
	}


	/// <summary>
	/// Defines a read-only collection of model object types.
	/// </summary>
	public interface IReadOnlyDiagramModelObjectTypeCollection : IReadOnlyCollection<DiagramModelObjectType> {

		/// <ToBeCompleted></ToBeCompleted>
		DiagramModelObjectType this[string diagramModelObjectTypeName] { get; }

	}


	/// <summary>
	/// Manages a list of diagram model object types.
	/// </summary>
	public class DiagramModelObjectTypeCollection : IReadOnlyDiagramModelObjectTypeCollection {

		internal DiagramModelObjectTypeCollection() {
		}


		/// <summary>
		/// Adds a model object type to the collection.
		/// </summary>
		/// <param name="diagramModelObjectType"></param>
		public void Add(DiagramModelObjectType diagramModelObjectType) {
			if (diagramModelObjectType == null) throw new ArgumentNullException("diagramModelObjectType");
			_diagramModelObjectTypes.Add(diagramModelObjectType.FullName, diagramModelObjectType);
		}


		/// <summary>
		/// Removes a model object type from the collection.
		/// </summary>
		/// <param name="modelObjectType"></param>
		public bool Remove(ModelObjectType modelObjectType) {
			if (modelObjectType == null) throw new ArgumentNullException("modelObjectType");
			return _diagramModelObjectTypes.Remove(modelObjectType.FullName);
		}


		/// <summary>
		/// Retrieves the model object type with the given name.
		/// </summary>
		/// <param name="typeName">Either a full (i.e. including the namespace) or partial model object type name</param>
		/// <returns>ModelObjectTypes object type with given name.</returns>
		public DiagramModelObjectType GetDiagramModelObjectType(string typeName) {
			if (typeName == null) throw new ArgumentNullException("typeName");
			DiagramModelObjectType result = null;
			if (!_diagramModelObjectTypes.TryGetValue(typeName, out result)) {
				foreach (KeyValuePair<string, DiagramModelObjectType> item in _diagramModelObjectTypes) {
					// If no matching type name was found, check if the given type projectName was a type projectName without namespace
					if (string.Compare(item.Value.Name, typeName, StringComparison.InvariantCultureIgnoreCase) == 0) {
						if (result == null) result = item.Value;
						else throw new ArgumentException(Properties.Resources.MessageFmt_TheModelObjectType0IsAmbiguous, typeName);
					}
				}
			}
			if (result == null)
				throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_ModelObjectType0WasNotRegistered, typeName));
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DiagramModelObjectType this[string diagramModelObjectTypeName] {
			get { return GetDiagramModelObjectType(diagramModelObjectTypeName); }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int Count {
			get { return _diagramModelObjectTypes.Count; }
		}


		internal bool IsModelObjectTypeRegistered(DiagramModelObjectType diagramModelObjectType) {
			return _diagramModelObjectTypes.ContainsKey(diagramModelObjectType.FullName);
		}


		internal void Clear() {
			_diagramModelObjectTypes.Clear();
		}


		#region IEnumerable<Type> Members

		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerator<DiagramModelObjectType> GetEnumerator() {
			return _diagramModelObjectTypes.Values.GetEnumerator();
		}

		#endregion


		#region IEnumerable Members

		/// <ToBeCompleted></ToBeCompleted>
		IEnumerator IEnumerable.GetEnumerator() {
			return _diagramModelObjectTypes.Values.GetEnumerator();
		}

		#endregion


		#region ICollection Members

		/// <ToBeCompleted></ToBeCompleted>
		public void CopyTo(Array array, int index) {
			if (array == null) throw new ArgumentNullException("array");
			_diagramModelObjectTypes.Values.CopyTo((DiagramModelObjectType[])array, index);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool IsSynchronized {
			get { return false; }
		}


		/// <override></override>
		public object SyncRoot {
			get {
				if (_syncRoot == null)
					Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		#endregion


		#region Fields

		// Key = ModelObjectType.FullName, Value = ModelObjectType
		private Dictionary<string, DiagramModelObjectType> _diagramModelObjectTypes = new Dictionary<string, DiagramModelObjectType>();
		private object _syncRoot = null;

		#endregion

	}


	/// <summary>
	/// Represents the method that is called to create a model object.
	/// </summary>
	/// <status>reviewed</status>
	public delegate IDiagramModelObject CreateDiagramModelObjectDelegate(DiagramModelObjectType diagramModelObjectType);


	/// <ToBeCompleted></ToBeCompleted>
	[TypeDescriptionProvider(typeof(TypeDescriptionProviderDg))]
	public class GenericDiagramModelObjectType : DiagramModelObjectType {

		/// <ToBeCompleted></ToBeCompleted>
		public GenericDiagramModelObjectType(string name, string namespaceName, string categoryTitle, CreateDiagramModelObjectDelegate createDiagramModelObjectDelegate,
			GetPropertyDefinitionsDelegate getPropertyDefinitionsDelegate)
			: base(name, namespaceName, categoryTitle, createDiagramModelObjectDelegate, getPropertyDefinitionsDelegate) {
		}

	}


	/// <summary>
	/// Base class for diagram model objects implementing naming, model hierarchy and diagram management.
	/// </summary>
	/// <remarks>DiagramModelObjectTypes objects can inherit from this class but need not.</remarks>
	[TypeDescriptionProvider(typeof(TypeDescriptionProviderDg))]
	public abstract class DiagramModelObjectBase : IDiagramModelObject, IEntity {

		/// <ToBeCompleted></ToBeCompleted>
		protected internal DiagramModelObjectBase(DiagramModelObjectBase source) {
			_id = null;
			_diagramModelObjectType = source.Type;
			_name = source.Name;
		}


		#region IModelObject Members

		/// <ToBeCompleted></ToBeCompleted>
		[LocalizedDisplayName("PropName_IModelObject_Name")]
		[LocalizedDescription("PropDesc_IModelObject_Name")]
		public virtual string Name {
			get { return _name; }
			set { _name = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		[LocalizedDisplayName("PropName_IModelObject_Type")]
		[LocalizedDescription("PropDesc_IModelObject_Type")]
		public DiagramModelObjectType Type {
			get { return _diagramModelObjectType; }
		}


		/// <override></override>
		[Browsable(false)]
		public Diagram Diagram {
			get { return _diagram; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public abstract IDiagramModelObject Clone();


		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		public abstract IEnumerable<MenuItemDef> GetMenuItemDefs();


		/// <override></override>
		public void AttachDiagram(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			//if (_diagram != null) throw new NShapeException(Properties.Resources.MessageFmt_01IsAlreadyAttachedToADiagram, Type.Name, Name);
			if (diagram.ModelObject != this) diagram.ModelObject = this;
			else _diagram = diagram;
		}


		/// <override></override>
		public void DetachDiagram() {
			//if (_diagram == null) throw new NShapeException(Properties.Resources.MessageFmt_01IsNotAttachedToADiagram, Type.Name, Name);
			if (_diagram != null)
				_diagram.ModelObject = null;
		}

		#endregion


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.ModelObjectBase" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
			yield return new EntityFieldDefinition("SecurityDomainName", typeof(string));
		}


		// unique id of object, does never change
		object IEntity.Id {
			get { return _id; }
		}


		void IEntity.AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this._id != null) throw new InvalidOperationException("Model object has already an id.");
			this._id = id;
		}


		void IEntity.LoadFields(IRepositoryReader reader, int version) {
			if (reader == null) throw new ArgumentNullException("reader");
			LoadFieldsCore(reader, version);
		}


		void IEntity.LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (reader == null) throw new ArgumentNullException("reader");
			LoadInnerObjectsCore(propertyName, reader, version);
		}


		void IEntity.SaveFields(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			SaveFieldsCore(writer, version);
		}


		void IEntity.SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			if (propertyName == null) throw new ArgumentNullException("propertyName");
			if (writer == null) throw new ArgumentNullException("writer");
			SaveInnerObjectsCore(propertyName, writer, version);
		}


		void IEntity.Delete(IRepositoryWriter writer, int version) {
			if (writer == null) throw new ArgumentNullException("writer");
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition) {
					writer.DeleteInnerObjects();
				}
			}
		}

		#endregion


		/// <summary>
		/// Indicates the name of the security domain this shape belongs to.
		/// </summary>
		[CategoryGeneral()]
		[LocalizedDisplayName("PropName_Diagram_SecurityDomainName")]
		[LocalizedDescription("PropDesc_ModelObject_SecurityDomainName")]
		[RequiredPermission(Permission.Security)]
		public abstract char SecurityDomainName { get; set; }


		/// <ToBeCompleted></ToBeCompleted>
		protected internal DiagramModelObjectBase(DiagramModelObjectType diagramModelObjectType) {
			if (diagramModelObjectType == null) throw new ArgumentNullException("ModelObjectType");
			this._diagramModelObjectType = diagramModelObjectType;
			this._name = diagramModelObjectType.GetDefaultName();
		}


		#region [Protected] IEntity implementation

		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void LoadFieldsCore(IRepositoryReader reader, int version) {
			_name = reader.ReadString();
			SecurityDomainName = reader.ReadChar();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void LoadInnerObjectsCore(string propertyName, IRepositoryReader reader, int version) {
			// nothing to do here
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void SaveFieldsCore(IRepositoryWriter writer, int version) {
			writer.WriteString(_name);
			writer.WriteChar(SecurityDomainName);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void SaveInnerObjectsCore(string propertyName, IRepositoryWriter writer, int version) {
			// nothing to do here
		}

		#endregion


		#region Fields


		private const string _persistentTypeName = "DiagramModelObject";

		private object _id = null;
		private DiagramModelObjectType _diagramModelObjectType = null;
		private string _name = string.Empty;
		private Diagram _diagram = null;

		#endregion

	}


	/// <summary>
	/// DiagramModelObjectTypes object with configurable number and type of properties.
	/// </summary>
	public class GenericDiagramModelObject : DiagramModelObjectBase {

		/// <ToBeCompleted></ToBeCompleted>
		public static GenericDiagramModelObject CreateInstance(DiagramModelObjectType diagramModelObjectType) {
			if (diagramModelObjectType == null) throw new ArgumentNullException("diagramModelObjectType");
			return new GenericDiagramModelObject(diagramModelObjectType);
		}


		/// <override></override>
		public override IDiagramModelObject Clone() {
			return new GenericDiagramModelObject(this);
		}


		/// <override></override>
		public override char SecurityDomainName {
			get { return _securityDomainName; }
			set {
				if (value < 'A' || value > 'Z')
					throw new ArgumentOutOfRangeException("SecurityDomainName", Dataweb.NShape.Properties.Resources.MessageTxt_TheDomainQualifierHasToBeAnUpperCaseANSILetterAZ);
				_securityDomainName = value;
			}
		}


		/// <override></override>
		public override IEnumerable<MenuItemDef> GetMenuItemDefs() {
			yield break;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal GenericDiagramModelObject(DiagramModelObjectType diagramModelObjectType)
			: base(diagramModelObjectType) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal GenericDiagramModelObject(GenericDiagramModelObject source)
			: base(source) {
		}


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.GenericModelObject" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in DiagramModelObjectBase.GetPropertyDefinitions(version))
				yield return pi;
		}


		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
		}


		#endregion


		#region Fields

		private char _securityDomainName = 'A';

		#endregion
	}

}

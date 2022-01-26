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
using System.Threading;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Defines the interface between the NShape framework and the model objects.
	/// </summary>
	public interface IModelObject : IEntity, ISecurityDomainObject {

		/// <summary>
		/// Name of the model object. Is unique with siblings.
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Type of this model object
		/// </summary>
		ModelObjectType Type { get; }

		/// <summary>
		/// Owning model object, can be null if this object is a root object.
		/// </summary>
		IModelObject Parent { get; set; }

		/// <summary>
		/// Creates a copy of this model object.
		/// </summary>
		/// <remarks>Composite objects are also cloned, references to aggregated objects are just copied.</remarks>
		IModelObject Clone();

		/// <summary>
		/// Connects the model object with another one.
		/// </summary>
		void Connect(TerminalId ownTerminalId, IModelObject otherModelObject, TerminalId otherTerminalId);

		/// <summary>
		/// Disconnects the model object from another one.
		/// </summary>
		void Disconnect(TerminalId ownTerminalId, IModelObject otherModelObject, TerminalId otherTerminalId);

		/// <summary>
		/// Retrieves the attached shapes.
		/// </summary>
		IEnumerable<Shape> Shapes { get; }

		/// <summary>
		/// Returns the number of attached shapes.
		/// </summary>
		int ShapeCount { get; }

		/// <summary>
		/// Attaches an observing shape.
		/// </summary>
		/// <param name="shape"></param>
		void AttachShape(Shape shape);

		/// <summary>
		/// Detaches an observing shape.
		/// </summary>
		/// <param name="shape"></param>
		void DetachShape(Shape shape);

		/// <summary>
		/// Retrieves the integer value of a field.
		/// </summary>
		/// <param name="propertyId"></param>
		/// <returns></returns>
		int GetInteger(int propertyId);

		/// <summary>
		/// Retrieves the float value of a field.
		/// </summary>
		/// <param name="propertyId"></param>
		/// <returns></returns>
		float GetFloat(int propertyId);

		/// <summary>
		/// Retrieves the string value of a field.
		/// </summary>
		/// <param name="propertyId"></param>
		/// <returns></returns>
		string GetString(int propertyId);

		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		IEnumerable<MenuItemDef> GetMenuItemDefs();

	}


	/// <summary>
	/// Represents a model object type.
	/// </summary>
	public abstract class ModelObjectType {

		/// <summary>
		/// Constructs a model object type.
		/// </summary>
		public ModelObjectType(string name, string libraryName, string categoryTitle, CreateModelObjectDelegate createModelObjectDelegate,
			GetPropertyDefinitionsDelegate getPropertyDefinitionsDelegate) {
			if (name == null) throw new ArgumentNullException("name");
			if (!Project.IsValidName(name)) throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_0IsNotAValidModelObjectTypeName, name));
			if (libraryName == null) throw new ArgumentNullException("libraryName");
			if (!Project.IsValidName(libraryName)) throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_0IsNotAValidLibraryName, libraryName));
			if (createModelObjectDelegate == null) throw new ArgumentNullException("createModelObjectDelegate");
			if (getPropertyDefinitionsDelegate == null) throw new ArgumentNullException("getPropertyDefinitionsDelegate");
			//
			_name = name;
			_libraryName = libraryName;
			_categoryTitle = categoryTitle;
			_createModelObjectDelegate = createModelObjectDelegate;
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
		public IModelObject CreateInstance() {
			return _createModelObjectDelegate(this);
		}


		/// <summary>
		/// Creates a model object instance of this type.
		/// </summary>
		public TModelObject CreateInstance<TModelObject>() where TModelObject : IModelObject {
			return (TModelObject)_createModelObjectDelegate(this);
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.ModelObjectType" />.
		/// </summary>
		public IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			return _getPropertyDefinitionsDelegate(version);
		}


		/// <summary>
		/// Indicates largest available terminal id for this type.
		/// </summary>
		public abstract TerminalId MaxTerminalId { get; }


		/// <summary>
		/// Retreives the name of a terminal.
		/// </summary>
		public abstract string GetTerminalName(TerminalId terminalId);

		/// <summary>
		/// Retrieves the id of a terminal.
		/// </summary>
		public abstract TerminalId FindTerminalId(string terminalName);


		internal string GetDefaultName() {
			return string.Format("{0} {1}", _name, ++_nameCounter);
		}


		#region Fields

		private string _name;
		private string _libraryName;
		private string _description;
		private string _categoryTitle = string.Empty;
		private CreateModelObjectDelegate _createModelObjectDelegate;
		private GetPropertyDefinitionsDelegate _getPropertyDefinitionsDelegate;

		private int _nameCounter = 0;

		#endregion
	}


	/// <summary>
	/// Represents the method that is called to create a model object.
	/// </summary>
	/// <param name="modelObjectType"></param>
	/// <returns></returns>
	/// <status>reviewed</status>
	public delegate IModelObject CreateModelObjectDelegate(ModelObjectType modelObjectType);


	/// <ToBeCompleted></ToBeCompleted>
	[TypeDescriptionProvider(typeof(TypeDescriptionProviderDg))]
	public class GenericModelObjectType : ModelObjectType {

		/// <ToBeCompleted></ToBeCompleted>
		public GenericModelObjectType(string name, string namespaceName, string categoryTitle, CreateModelObjectDelegate createModelObjectDelegate,
			GetPropertyDefinitionsDelegate getPropertyDefinitionsDelegate, TerminalId maxTerminalId)
			: base(name, namespaceName, categoryTitle, createModelObjectDelegate, getPropertyDefinitionsDelegate) {
			this._maxTerminalId = maxTerminalId;
			_terminals.Add(TerminalId.Generic, "Generic Terminal");
			for (int i = 1; i <= maxTerminalId; ++i)
				_terminals.Add(i, "Terminal " + Convert.ToString(i));
		}


		/// <override></override>
		public override TerminalId MaxTerminalId {
			get { return _maxTerminalId; }
		}


		/// <override></override>
		public override string GetTerminalName(TerminalId terminalId) {
			if (terminalId < 0 || terminalId > _maxTerminalId) throw new ArgumentOutOfRangeException("terminalId");
			string result;
			if (_terminals.TryGetValue(terminalId, out result)) return result;
			else throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageFmt_NoTerminalNameFoundForTerminal0, terminalId);
		}


		/// <override></override>
		public override TerminalId FindTerminalId(string terminalName) {
			if (string.IsNullOrEmpty(terminalName)) return TerminalId.Invalid;
			foreach (KeyValuePair<TerminalId, string> item in _terminals) {
				if (item.Value.Equals(terminalName, StringComparison.InvariantCultureIgnoreCase))
					return item.Key;
			}
			return TerminalId.Invalid;
		}


		#region Fields

		private TerminalId _maxTerminalId;
		private Dictionary<TerminalId, string> _terminals = new Dictionary<TerminalId, string>();

		#endregion
	}


	/// <summary>
	/// Defines a read-only collection of model object types.
	/// </summary>
	public interface IReadOnlyModelObjectTypeCollection : IReadOnlyCollection<ModelObjectType> {

		/// <ToBeCompleted></ToBeCompleted>
		ModelObjectType this[string modelObjectTypeName] { get; }

	}


	/// <summary>
	/// Manages a list of model object types.
	/// </summary>
	public class ModelObjectTypeCollection : IReadOnlyModelObjectTypeCollection {

		internal ModelObjectTypeCollection() {
		}


		/// <summary>
		/// Adds a model object type to the collection.
		/// </summary>
		/// <param name="modelObjectType"></param>
		public void Add(ModelObjectType modelObjectType) {
			if (modelObjectType == null) throw new ArgumentNullException("modelObjectType");
			_modelObjectTypes.Add(modelObjectType.FullName, modelObjectType);
		}


		/// <summary>
		/// Removes a model object type from the collection.
		/// </summary>
		/// <param name="modelObjectType"></param>
		public bool Remove(ModelObjectType modelObjectType) {
			if (modelObjectType == null) throw new ArgumentNullException("modelObjectType");
			return _modelObjectTypes.Remove(modelObjectType.FullName);
		}


		/// <summary>
		/// Retrieves the model object type with the given name.
		/// </summary>
		/// <param name="typeName">Either a full (i.e. including the namespace) or partial model object type name</param>
		/// <returns>ModelObjectTypes object type with given name.</returns>
		public ModelObjectType GetModelObjectType(string typeName) {
			if (typeName == null) throw new ArgumentNullException("typeName");
			ModelObjectType result = null;
			if (!_modelObjectTypes.TryGetValue(typeName, out result)) {
				foreach (KeyValuePair<string, ModelObjectType> item in _modelObjectTypes) {
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
		public ModelObjectType this[string modelObjectTypeName] {
			get { return GetModelObjectType(modelObjectTypeName); }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int Count {
			get { return _modelObjectTypes.Count; }
		}


		internal bool IsModelObjectTypeRegistered(ModelObjectType modelObjectType) {
			return _modelObjectTypes.ContainsKey(modelObjectType.FullName);
		}


		internal void Clear() {
			_modelObjectTypes.Clear();
		}


		#region IEnumerable<Type> Members

		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerator<ModelObjectType> GetEnumerator() {
			return _modelObjectTypes.Values.GetEnumerator();
		}

		#endregion


		#region IEnumerable Members

		/// <ToBeCompleted></ToBeCompleted>
		IEnumerator IEnumerable.GetEnumerator() {
			return _modelObjectTypes.Values.GetEnumerator();
		}

		#endregion


		#region ICollection Members

		/// <ToBeCompleted></ToBeCompleted>
		public void CopyTo(Array array, int index) {
			if (array == null) throw new ArgumentNullException("array");
			_modelObjectTypes.Values.CopyTo((ModelObjectType[])array, index);
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
		private Dictionary<string, ModelObjectType> _modelObjectTypes = new Dictionary<string, ModelObjectType>();
		private object _syncRoot = null;

		#endregion

	}


	/// <summary>
	/// Base class for model objects implementing naming, model hierarchy and shape management.
	/// </summary>
	/// <remarks>ModelObjectTypes objects can inherit from this class but need not.</remarks>
	[TypeDescriptionProvider(typeof(TypeDescriptionProviderDg))]
	public abstract class ModelObjectBase : IModelObject, IEntity {

		/// <ToBeCompleted></ToBeCompleted>
		protected internal ModelObjectBase(ModelObjectBase source) {
			_id = null;
			_modelObjectType = source.Type;
			_name = _modelObjectType.GetDefaultName();
			_parent = source.Parent;
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
		public ModelObjectType Type {
			get { return _modelObjectType; }
		}


		/// <summary>
		/// Parent of the model objects. Only the root object has no parent. Sometimes
		/// temporary objects have no parent and are therefore orphaned. E.g. when cloning
		/// model objects the clones do not have parents.
		/// </summary>
		[Browsable(false)]
		public virtual IModelObject Parent {
			get { return _parent; }
			set { _parent = value; }
		}


		/// <override></override>
		[Browsable(false)]
		public virtual IEnumerable<Shape> Shapes {
			get { return (IEnumerable<Shape>)_shapes ?? EmptyEnumerator<Shape>.Empty; }
		}


		/// <override></override>
		[Browsable(false)]
		public int ShapeCount {
			get { return (_shapes != null) ? _shapes.Count : 0; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public abstract IModelObject Clone();


		/// <ToBeCompleted></ToBeCompleted>
		public virtual int GetInteger(int propertyId) {
			throw new NShapeException(Properties.Resources.MessageFmt_NoIntegerPropertyWithPropertyId0Found, propertyId);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public virtual float GetFloat(int propertyId) {
			throw new NShapeException(Properties.Resources.MessageFmt_NoFloatPropertyWithPropertyId0Found, propertyId);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public virtual string GetString(int propertyId) {
			throw new NShapeException(Properties.Resources.MessageFmt_NoStringPropertyWithPropertyId0Found, propertyId);
		}


		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		public abstract IEnumerable<MenuItemDef> GetMenuItemDefs();


		/// <ToBeCompleted></ToBeCompleted>
		public abstract void Connect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId);


		/// <ToBeCompleted></ToBeCompleted>
		public abstract void Disconnect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId);


		/// <override></override>
		public void AttachShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (_shapes == null) _shapes = new List<Shape>(1);
			else if (_shapes.Contains(shape)) throw new NShapeException(Properties.Resources.MessageFmt_01IsAlreadyAttachedToThisShape, Type.Name, Name);
			if (shape.ModelObject != this) shape.ModelObject = this;
			else _shapes.Add(shape);
		}


		/// <override></override>
		public void DetachShape(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			//if (_shapes == null) throw new NShapeException(Properties.Resources.MessageFmt_01IsNotAttachedToAnyShape, Type.Name, Name);
			if (_shapes != null) {
				int idx = _shapes.IndexOf(shape);
				if (idx < 0)
					//throw new NShapeException(Properties.Resources.MessageFmt_01IsNotAttachedToThisShape, Type.Name, Name);
					return;
				if (shape.ModelObject == this) shape.ModelObject = null;
				else _shapes.RemoveAt(idx);
			}
		}

		#endregion


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.ModelObjectBase" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
			if (version >= 4) yield return new EntityFieldDefinition("SecurityDomainName", typeof(string));
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
		protected internal ModelObjectBase(ModelObjectType modelObjectType) {
			if (modelObjectType == null) throw new ArgumentNullException("ModelObjectType");
			this._modelObjectType = modelObjectType;
			this._name = modelObjectType.GetDefaultName();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void OnPropertyChanged(int propertyId) {
			foreach (Shape shape in Shapes)
				shape.NotifyModelChanged(propertyId);
		}


		#region [Protected] IEntity implementation

		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void LoadFieldsCore(IRepositoryReader reader, int version) {
			_name = reader.ReadString();
			if (version >= 4) SecurityDomainName = reader.ReadChar();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void LoadInnerObjectsCore(string propertyName, IRepositoryReader reader, int version) {
			// nothing to do here
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void SaveFieldsCore(IRepositoryWriter writer, int version) {
			writer.WriteString(_name);
			if (version >= 4) writer.WriteChar(SecurityDomainName);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void SaveInnerObjectsCore(string propertyName, IRepositoryWriter writer, int version) {
			// nothing to do here
		}

		#endregion


		/// <ToBeCompleted></ToBeCompleted>
		protected int TerminalCount {
			get { return _terminalCount; }
			set { _terminalCount = value; }
		}


		#region Fields


		private const string _persistentTypeName = "ModelObject";

		private int _terminalCount;

		private object _id = null;
		private ModelObjectType _modelObjectType = null;
		private string _name = string.Empty;
		private IModelObject _parent = null;
		private List<Shape> _shapes = null;

		#endregion

	}


	/// <summary>
	/// ModelObjectTypes object with configurable number and type of properties.
	/// </summary>
	public class GenericModelObject : ModelObjectBase {

		/// <ToBeCompleted></ToBeCompleted>
		public static GenericModelObject CreateInstance(ModelObjectType modelObjectType) {
			if (modelObjectType == null) throw new ArgumentNullException("modelObjectType");
			return new GenericModelObject(modelObjectType);
		}


		/// <override></override>
		public override IModelObject Clone() {
			return new GenericModelObject(this);
		}


		/// <override></override>
		public override int GetInteger(int propertyId) {
			if (propertyId == PropertyIdIntegerValue) return IntegerValue;
			else return base.GetInteger(propertyId);
		}


		/// <override></override>
		public override float GetFloat(int propertyId) {
			if (propertyId == PropertyIdFloatValue) return FloatValue;
			else return base.GetFloat(propertyId);
		}


		/// <override></override>
		public override string GetString(int propertyId) {
			if (propertyId == PropertyIdStringValue) return StringValue;
			else return base.GetString(propertyId);
		}


		/// <ToBeCompleted></ToBeCompleted>
		[PropertyMappingId(PropertyIdIntegerValue)]
		[Description("An integer value. This value is represented by the assigned Shape.")]
		public int IntegerValue {
			get { return _integerValue; }
			set {
				_integerValue = value;
				OnPropertyChanged(PropertyIdIntegerValue);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[PropertyMappingId(PropertyIdFloatValue)]
		[Description("A floating point number value. This value is represented by the assigned Shape.")]
		public float FloatValue {
			get { return _floatValue; }
			set {
				_floatValue = value;
				OnPropertyChanged(PropertyIdFloatValue);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[PropertyMappingId(PropertyIdStringValue)]
		[Description("A string value. This value is represented by the assigned Shape.")]
		public string StringValue {
			get { return _stringValue; }
			set {
				_stringValue = value;
				OnPropertyChanged(PropertyIdStringValue);
			}
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
			//yield return new NotImplementedAction("Set State");
			yield break;
		}


		/// <override></override>
		public override void Connect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId) {
			throw new NotImplementedException();
		}


		/// <override></override>
		public override void Disconnect(TerminalId ownTerminalId, IModelObject targetConnector, TerminalId targetTerminalId) {
			throw new NotImplementedException();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal GenericModelObject(ModelObjectType modelObjectType)
			: base(modelObjectType) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal GenericModelObject(GenericModelObject source)
			: base(source) {
		}


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.GenericModelObject" />.
		/// </summary>
		new public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in ModelObjectBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("IntegerValue", typeof(int));
			yield return new EntityFieldDefinition("FloatValue", typeof(float));
			yield return new EntityFieldDefinition("StringValue", typeof(string));
		}


		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			_integerValue = reader.ReadInt32();
			_floatValue = reader.ReadFloat();
			_stringValue = reader.ReadString();
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteInt32(_integerValue);
			writer.WriteFloat(_floatValue);
			writer.WriteString(_stringValue);
		}


		#endregion


		#region Fields

		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdGenericValue = 1;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdStringValue = 2;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdIntegerValue = 3;
		/// <ToBeCompleted></ToBeCompleted>
		protected const int PropertyIdFloatValue = 4;

		private int _integerValue;
		private float _floatValue;
		private string _stringValue;
		private char _securityDomainName = 'A';

		#endregion
	}

}

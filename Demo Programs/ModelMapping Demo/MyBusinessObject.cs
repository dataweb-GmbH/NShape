using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using System.Diagnostics;
using Dataweb.NShape.GeneralShapes;
using Dataweb.NShape.WinFormsUI;

namespace ModelMapping_Demo
{


	/// <summary>
	/// This test class has 3 properties that can be bound to shape properties.
	/// In order to enable a property binding, we have to add a PropertyMappingId 
	/// attribute to each property that should be mapped to a shape property.
	/// </summary>
	public class MyBusinessObject : IModelObject
	{

		#region [public] PropertyId constants 

		public const int BooleanPropertyId = 1;
		public const int IntegerPropertyId = 2;
		public const int FloatPropertyId = 3;
		public const int StringPropertyId = 4;

		#endregion


		public MyBusinessObject(ModelObjectType type)
		{
			this._modelObjectType = type;
		}


		#region [Public] Properties

		[PropertyMappingId(BooleanPropertyId)]
		public bool MyBooleanProperty {
			get { return _boolProperty; }
			set {
				_boolProperty = value;
				OnPropertyChanged(BooleanPropertyId);
			}
		}


		[PropertyMappingId(IntegerPropertyId)]
		public int MyIntegerProperty {
			get { return _intProperty; }
			set {
				_intProperty = value;
				OnPropertyChanged(IntegerPropertyId);
			}
		}


		[PropertyMappingId(FloatPropertyId)]
		public float MyFloatProperty {
			get { return _floatProperty; }
			set {
				_floatProperty = value;
				OnPropertyChanged(FloatPropertyId);
			}
		}


		[PropertyMappingId(StringPropertyId)]
		public string MyStringProperty {
			get { return _stringProperty; }
			set {
				_stringProperty = value;
				OnPropertyChanged(StringPropertyId);
			}
		}

		#endregion


		#region IModelObject Implementation

		public void AttachShape(Shape shape)
		{
			if (_shapes.Contains(shape)) throw new InvalidOperationException("Model object is already attached with the given shape.");
			_shapes.Add(shape);
		}


		public IModelObject Clone()
		{
			MyBusinessObject clone = new MyBusinessObject(this._modelObjectType);
			clone.Name = this.Name;
			clone.MyBooleanProperty = this.MyBooleanProperty;
			clone.MyFloatProperty = this.MyFloatProperty;
			clone.MyIntegerProperty = this.MyIntegerProperty;
			clone.MyStringProperty = this.MyStringProperty;
			return clone;
		}


		public void DetachShape(Shape shape)
		{
			_shapes.Remove(shape);
		}


		public float GetFloat(int propertyId)
		{
			if (propertyId == FloatPropertyId)
				return MyFloatProperty;
			else {
				throw new ArgumentException("Invalid property id.");
			}
		}


		public int GetInteger(int propertyId)
		{
			if (propertyId == BooleanPropertyId)
				return MyBooleanProperty ? 1 : 0;
			else if (propertyId == IntegerPropertyId)
				return MyIntegerProperty;
			else {
				throw new ArgumentException("Invalid property id.");
			}
		}


		public string GetString(int propertyId)
		{
			if (propertyId == StringPropertyId)
				return MyStringProperty;
			else {
				throw new ArgumentException("Invalid property id.");
			}
		}


		public IEnumerable<MenuItemDef> GetMenuItemDefs()
		{
			yield break;
		}


		public int ShapeCount {
			get { return _shapes.Count; }
		}

		public IEnumerable<Shape> Shapes {
			get { return _shapes; }
		}


		public ModelObjectType Type {
			get { return _modelObjectType; }
		}


		public char SecurityDomainName {
			get { return 'A'; }
		}

		#endregion


		#region IModelObject Members (Not implemented: Not needed for this simple demo)

		public void Connect(TerminalId ownTerminalId, IModelObject otherModelObject, TerminalId otherTerminalId)
		{
			throw new NotImplementedException();
		}

		public void Disconnect(TerminalId ownTerminalId, IModelObject otherModelObject, TerminalId otherTerminalId)
		{
			throw new NotImplementedException();
		}

		public string Name {
			get;
			set;
		}

		public IModelObject Parent {
			get { return null; }
			set {
				// Not implemented - Do nothing
			}
		}

		#endregion

		#region IEntity implementation

		public object Id {
			get { return _id; }
		}


		public void AssignId(object id)
		{
			if (this._id != null) throw new InvalidOperationException("An id is already assigned.");
			this._id = id;
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.GenericModelObject" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version)
		{
			yield return new EntityFieldDefinition("Name", typeof(string));
			yield return new EntityFieldDefinition("MyBooleanProperty", typeof(bool));
			yield return new EntityFieldDefinition("MyIntegerProperty", typeof(int));
			yield return new EntityFieldDefinition("MyFloatProperty", typeof(float));
			yield return new EntityFieldDefinition("MyStringProperty", typeof(string));
		}


		public void Delete(IRepositoryWriter writer, int version)
		{
			// Nothing to do here.
		}


		public void LoadFields(IRepositoryReader reader, int version)
		{
			Name = reader.ReadString();
			MyBooleanProperty = reader.ReadBool();
			MyIntegerProperty = reader.ReadInt32();
			MyFloatProperty = reader.ReadFloat();
			MyStringProperty = reader.ReadString();
		}


		public void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version)
		{
			// Nothing to do here.
		}


		public void SaveFields(IRepositoryWriter writer, int version)
		{
			writer.WriteString(Name);
			writer.WriteBool(MyBooleanProperty);
			writer.WriteInt32(MyIntegerProperty);
			writer.WriteFloat(MyFloatProperty);
			writer.WriteString(MyStringProperty);
		}


		public void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version)
		{
			// Nothing to do here.
		}

		#endregion


		/// <summary>
		/// Notify all attached shapes that the specified property has changed
		/// </summary>
		/// <param name="propertyId"></param>
		protected void OnPropertyChanged(int propertyId)
		{
			for (int i = _shapes.Count - 1; i >= 0; --i)
				_shapes[i].NotifyModelChanged(propertyId);
		}


		// Fields
		private List<Shape> _shapes = new List<Shape>();
		private object _id = null;
		private bool _boolProperty;
		private int _intProperty;
		private float _floatProperty;
		private string _stringProperty;
		private ModelObjectType _modelObjectType;
	}

}
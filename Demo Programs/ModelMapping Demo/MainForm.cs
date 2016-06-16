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


namespace ModelMapping_Demo {

	public partial class MainForm : Form {
		
		public MainForm() {
			InitializeComponent();

			// Initialize project with a shape library
			project.AddLibrary(typeof(ThickArrow).Assembly, false);
			// Add this application as model object library
			project.AddLibrary(GetType().Assembly, false);

			project.Name = "Model Mapping Demo";
			project.Create();

			// Create an instance of the test object (that implements IModelObject)
			Template template = project.Repository.GetTemplate("ThickArrow");
			MyBusinessObject model =  (MyBusinessObject)project.ModelObjectTypes["MyBusinessObjectType"].CreateInstance();
			InitPropertyMappings(template, model);

			// Create a diagram with a shape that is bound to a business object
			const string diagramName = "Demo Diagram";
			Diagram diagram = new Diagram(diagramName);

			// Create a shape from the prepared template
			ThickArrow shape = (ThickArrow)project.Repository.GetTemplate("ThickArrow").CreateShape();
			shape.MoveTo(diagram.Width / 2, diagram.Height / 2);
			shape.Width = 120;
			shape.HeadWidth = 60;
			shape.Height = 100;
			
			diagram.Shapes.Add(shape);
			project.Repository.Insert(shape.ModelObject);
			project.Repository.InsertAll(diagram);

			display1.LoadDiagram(diagramName);
			
			// Store model object to modify
			businessObj = shape.ModelObject as MyBusinessObject;
			// Update start values
			businessObj.MyBooleanProperty = checkBox.Checked;
			businessObj.MyIntegerProperty = (int)intTrackBar.Value;
			businessObj.MyFloatProperty = (float)floatTrackBar.Value;
		}


		private void InitPropertyMappings(Template template, MyBusinessObject modelObj) {
			Debug.Assert(template != null && template.Shape != null && modelObj != null);

			// Assign model object to template shape
			template.Shape.ModelObject = modelObj;

			// Delete and clear all property mappings 
			// (not really necessary in this case)
			project.Repository.Delete(template.GetPropertyMappings());
			template.UnmapAllProperties();

			// Prepare property mappings (One mapping per property!):
			//
			// Float to Style 
			// Change color depending on the integer property
			StyleModelMapping lineStyleMapping = new StyleModelMapping(1, MyBusinessObject.FloatPropertyId, StyleModelMapping.MappingType.FloatStyle);
			lineStyleMapping.AddValueRange(float.MinValue, project.Design.LineStyles.None);
			lineStyleMapping.AddValueRange(16.66f, project.Design.LineStyles.Dotted);
			lineStyleMapping.AddValueRange(33.33f, project.Design.LineStyles.Special2);
			lineStyleMapping.AddValueRange(50.00f, project.Design.LineStyles.Special1);
			lineStyleMapping.AddValueRange(66.66f, project.Design.LineStyles.Dashed);
			lineStyleMapping.AddValueRange(83.33f, project.Design.LineStyles.Normal);
			template.MapProperties(lineStyleMapping);
			// Save mapping
			project.Repository.Insert(lineStyleMapping, template);
			//
			// Integer to Style
			// Change outline thickness depending on the boolean property
			StyleModelMapping fillStyleMapping = new StyleModelMapping(3, MyBusinessObject.BooleanPropertyId, StyleModelMapping.MappingType.IntegerStyle);
			fillStyleMapping.AddValueRange(0, project.Design.FillStyles.Green);
			fillStyleMapping.AddValueRange(1, project.Design.FillStyles.Red);
			template.MapProperties(fillStyleMapping);
			// Save mapping
			project.Repository.Insert(fillStyleMapping, template);
			//
			// Integer to Integer
			// Change shape's angle depending on the integer property 
			// Note: Value is modified by a slope factor of 10 because angle is specified in tenths of degrees.
			NumericModelMapping angleMapping = new NumericModelMapping(2, MyBusinessObject.IntegerPropertyId, NumericModelMapping.MappingType.IntegerInteger, 0, 10);
			template.MapProperties(angleMapping);
			// Save mapping
			project.Repository.Insert(angleMapping, template);

			// Add model object and update template
			project.Repository.Insert(modelObj);
			project.Repository.Update(template);
		}


		#region [Private] UI event handler implementations

		private void checkBox_CheckedChanged(object sender, EventArgs e) {
			Debug.Assert(businessObj != null);
			businessObj.MyBooleanProperty = checkBox.Checked;
		}


		private void intTrackBar_Scroll(object sender, EventArgs e) {
			Debug.Assert(businessObj != null);
			businessObj.MyIntegerProperty = (int)intTrackBar.Value;
		}


		private void floatTrackBar_Scroll(object sender, EventArgs e) {
			Debug.Assert(businessObj != null);
			businessObj.MyFloatProperty = (float)floatTrackBar.Value;
		}
		
		#endregion


		// Fields
		private MyBusinessObject businessObj = null;
	}


	/// <summary>
	/// This test class has 3 properties that can be bound to shape properties.
	/// In order to enable a property binding, we have to add a PropertyMappingId 
	/// attribute to each property that should be mapped to a shape property.
	/// </summary>
	public class MyBusinessObject : IModelObject {

		#region [public] PropertyId constants 

		public const int BooleanPropertyId = 1;
		public const int IntegerPropertyId = 2;
		public const int FloatPropertyId = 3;
		public const int StringPropertyId = 4;

		#endregion


		public MyBusinessObject(ModelObjectType type) {
			this.modelObjectType = type;
		}


		#region [Public] Properties

		[PropertyMappingId(BooleanPropertyId)]
		public bool MyBooleanProperty {
			get { return boolProperty; }
			set { 
				boolProperty = value;
				OnPropertyChanged(BooleanPropertyId);
			}
		}


		[PropertyMappingId(IntegerPropertyId)]
		public int MyIntegerProperty {
			get { return intProperty; }
			set {
				intProperty = value;
				OnPropertyChanged(IntegerPropertyId);
				OnPropertyChanged(5);
			}
		}


		[PropertyMappingId(FloatPropertyId)]
		public float MyFloatProperty {
			get { return floatProperty; }
			set {
				floatProperty = value;
				OnPropertyChanged(FloatPropertyId);
			}
		}


		[PropertyMappingId(StringPropertyId)]
		public string MyStringProperty {
			get { return stringProperty; }
			set {
				stringProperty = value;
				OnPropertyChanged(StringPropertyId);
			}
		}

		#endregion


		#region IModelObject Implementation

		public void AttachShape(Shape shape) {
			if (shapes.Contains(shape)) throw new InvalidOperationException("Model object is already attached with the given shape.");
			shapes.Add(shape);
		}


		public IModelObject Clone() {
			MyBusinessObject clone = new MyBusinessObject(this.modelObjectType);
			clone.MyBooleanProperty = this.MyBooleanProperty;
			clone.MyFloatProperty = this.MyFloatProperty;
			clone.MyIntegerProperty = this.MyIntegerProperty;
			return clone;
		}


		public void DetachShape(Shape shape) {
			shapes.Remove(shape);
		}


		public float GetFloat(int propertyId) {
			if (propertyId == FloatPropertyId)
				return MyFloatProperty;
			else {
				throw new ArgumentException("Invalid property id.");
			}
		}


		public int GetInteger(int propertyId) {
			if (propertyId == BooleanPropertyId)
				return MyBooleanProperty ? 1 : 0;
			else if (propertyId == IntegerPropertyId)
				return MyIntegerProperty;
			else {
				throw new ArgumentException("Invalid property id.");
			}
		}
		

		public string GetString(int propertyId) {
			if (propertyId == StringPropertyId)
				return MyStringProperty;
			else {
				throw new ArgumentException("Invalid property id.");
			}
		}


		public IEnumerable<MenuItemDef> GetMenuItemDefs() {
			yield break;
		}


		public int ShapeCount {
			get { return shapes.Count; }
		}

		public IEnumerable<Shape> Shapes {
			get { return shapes; }
		}


		public ModelObjectType Type {
			get { return modelObjectType; }
		}


		public void AssignId(object id) {
			if (this.id != null) throw new InvalidOperationException("An id is already assigned.");
			this.id = id;
		}


		public object Id {
			get { return id; }
		}


		public char SecurityDomainName {
			get { return 'A'; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.GenericModelObject" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("MyBooleanProperty", typeof(bool));
			yield return new EntityFieldDefinition("MyIntegerProperty", typeof(int));
			yield return new EntityFieldDefinition("MyFloatProperty", typeof(float));
			yield return new EntityFieldDefinition("MyStringProperty", typeof(string));
		}

		#endregion


		#region IModelObject Members (Not implemented: Not needed for this simple demo)

		public void Connect(TerminalId ownTerminalId, IModelObject otherModelObject, TerminalId otherTerminalId) {
			throw new NotImplementedException();
		}

		public void Disconnect(TerminalId ownTerminalId, IModelObject otherModelObject, TerminalId otherTerminalId) {
			throw new NotImplementedException();
		}

		public string Name {
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public IModelObject Parent {
			get { return null; }
			set {
				// Not implemented - Do nothing
			}
		}

		public void Delete(IRepositoryWriter writer, int version) {
			throw new NotImplementedException();
		}

		public void LoadFields(IRepositoryReader reader, int version) {
			throw new NotImplementedException();
		}

		public void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			throw new NotImplementedException();
		}

		public void SaveFields(IRepositoryWriter writer, int version) {
			throw new NotImplementedException();
		}

		public void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			throw new NotImplementedException();
		}

		#endregion


		/// <summary>
		/// Notify all attached shapes that the specified property has changed
		/// </summary>
		/// <param name="propertyId"></param>
		protected void OnPropertyChanged(int propertyId) {
			for (int i = shapes.Count - 1; i >= 0; --i)
				shapes[i].NotifyModelChanged(propertyId);
		}


		// Fields
		private List<Shape> shapes = new List<Shape>();
		private object id = null;
		private bool boolProperty;
		private int intProperty;
		private float floatProperty;
		private string stringProperty;
		private ModelObjectType modelObjectType;
	}


	/// <summary>
	/// The Libraryinitializer is needed for registering the IModelObject implementations with the NShape framework.
	/// </summary>
	public static class NShapeLibraryInitializer {

		public static void Initialize(IRegistrar registrar) {
			registrar.RegisterLibrary(namespaceName, preferredRepositoryVersion);
			registrar.RegisterModelObjectType(new GenericModelObjectType("MyBusinessObjectType", namespaceName, categoryTitle,
				(ModelObjectType type) => { return new MyBusinessObject(type); }, MyBusinessObject.GetPropertyDefinitions, 0));
		}


		#region Fields

		private const string namespaceName = "ModelMappingDemo";
		private const string categoryTitle = "Model Mapping Demo";
		private const int preferredRepositoryVersion = 1;

		#endregion
	}

}

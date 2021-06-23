using System;
using System.Diagnostics;
using System.Windows.Forms;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.GeneralShapes;


namespace ModelMapping_Demo
{

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
			MyBusinessObject model =  project.ModelObjectTypes["MyBusinessObject"].CreateInstance<MyBusinessObject>();
			InitPropertyMappings(template, model);

			// Create a diagram with a shape that is bound to a business object
			const string diagramName = "Demo Diagram";
			Diagram diagram = new Diagram(diagramName);
			diagram.ModelObject = project.DiagramModelObjectTypes["MyDiagramBusinessObject"].CreateInstance<MyDiagramBusinessObject>();

			// Create a shape from the prepared template
			ThickArrow shape = project.Repository.GetTemplate("ThickArrow").CreateShape<ThickArrow>();
			shape.MoveTo(diagram.Width / 2, diagram.Height / 2);
			shape.Width = 120;
			shape.HeadWidth = 60;
			shape.Height = 100;
			
			diagram.Shapes.Add(shape);
			project.Repository.Insert(shape.ModelObject);
			project.Repository.InsertAll(diagram);

			display1.LoadDiagram(diagramName);
			
			// Store model object to modify
			_businessObj = shape.ModelObject as MyBusinessObject;
			// Update start values
			_businessObj.MyBooleanProperty = checkBox.Checked;
			_businessObj.MyIntegerProperty = (int)intTrackBar.Value;
			_businessObj.MyFloatProperty = (float)floatTrackBar.Value;

			// Display model object properties in UI
			propertyGrid1.SelectedObject = display1.Diagram.ModelObject;
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
			//
			// Integer to String
			FormatModelMapping textMapping = new FormatModelMapping(4, MyBusinessObject.StringPropertyId, FormatModelMapping.MappingType.StringString); //, "Angle: {0}°");
			template.MapProperties(textMapping);
			// Save mapping
			project.Repository.Insert(textMapping, template);

			// Add model object and update template
			project.Repository.Insert(modelObj, template);
			project.Repository.Update(template);
		}


		private string GetText(MyBusinessObject modelObj) {
			return string.Format("{0}°", modelObj.MyIntegerProperty);
		}


		#region [Private] UI event handler implementations

		private void checkBox_CheckedChanged(object sender, EventArgs e) {
			Debug.Assert(_businessObj != null);
			_businessObj.MyBooleanProperty = checkBox.Checked;
			_businessObj.MyStringProperty = GetText(_businessObj);
		}


		private void intTrackBar_Scroll(object sender, EventArgs e) {
			Debug.Assert(_businessObj != null);
			_businessObj.MyIntegerProperty = (int)intTrackBar.Value;
			_businessObj.MyStringProperty = GetText(_businessObj);
		}


		private void floatTrackBar_Scroll(object sender, EventArgs e) {
			Debug.Assert(_businessObj != null);
			_businessObj.MyFloatProperty = (float)floatTrackBar.Value;
			_businessObj.MyStringProperty = GetText(_businessObj);
		}

		#endregion


		// Fields
		private MyBusinessObject _businessObj = null;


		private void saveButton_Click(object sender, EventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog() {
				DefaultExt = ".mmdf",
				Filter = "Model Demo Files|*.mmdf",
				AddExtension = true
			};
			if (dlg.ShowDialog(this) == DialogResult.OK) {
				string directory = System.IO.Path.GetDirectoryName(dlg.FileName);
				string fileName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
				string fileExt = System.IO.Path.GetExtension(dlg.FileName);

				//project.Repository.SaveChanges();
				project.Name = fileName;
				XmlStore store = new XmlStore(directory, fileExt);
				if (project.Repository is CachedRepository repository) {
					repository.Store = store;
					if (store.Exists())
						store.Erase();
					store.Create(repository);
					project.Repository.SaveChanges();
					//repository.Store.Create(repository);
				}
			}
		}

	}

}

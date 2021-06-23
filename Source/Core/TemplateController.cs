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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;


namespace Dataweb.NShape.Controllers {

	/// <summary>
	/// A non-visual component for editing templates. 
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(TemplateController), "TemplateController.bmp")]
	public class TemplateController : Component, IDisplayService {

		/// <summary>
		/// Creates a new <see cref="T:Dataweb.NShape.Controllers.TemplateController" /> instance
		/// </summary>
		public TemplateController() {
			_infoGraphics = Graphics.FromHwnd(IntPtr.Zero);
		}


		/// <summary>
		/// Creates and initializes a new <see cref="T:Dataweb.NShape.Controllers.TemplateController" /> instance
		/// </summary>
		public TemplateController(Project project, Template template)
			: this() {
			if (project == null) throw new ArgumentNullException("project");
			Initialize(project, template);
		}


		/// <ToBeCompleted></ToBeCompleted>
		~TemplateController() {
			_infoGraphics.Dispose();
			_infoGraphics = null;
		}


		#region IDisplayService Members

		/// <override></override>
		void IDisplayService.Invalidate(int x, int y, int width, int height) {
			// nothing to do
		}


		/// <override></override>
		void IDisplayService.Invalidate(Rectangle rectangle) {
			// nothing to do
		}


		/// <override></override>
		void IDisplayService.NotifyBoundsChanged() { 
			// nothing to do
		}


		/// <override></override>
		Graphics IDisplayService.InfoGraphics {
			get { return _infoGraphics; }
		}


		IFillStyle IDisplayService.HintBackgroundStyle {
			get {
				if (Project != null && Project.IsOpen)
					return Project.Design.FillStyles.White;
				else return null;
			}
		}


		ILineStyle IDisplayService.HintForegroundStyle {
			get {
				if (Project != null && Project.IsOpen)
					return Project.Design.LineStyles.Normal;
				else return null;
			}
		}

		#endregion


		#region [Public] Events

		/// <summary>
		/// Raised when the <see cref="T:Dataweb.NShape.Controllers.TemplateController" /> initializes.
		/// </summary>
		public event EventHandler<TemplateControllerInitializingEventArgs> Initializing;

		/// <summary>
		/// Raised when changes are applied.
		/// </summary>
		public event EventHandler ApplyingChanges;

		/// <summary>
		/// Raised when changes are discarded.
		/// </summary>
		public event EventHandler DiscardingChanges;

		/// <summary>
		/// Raised when ever a property of the template (such as name, title or description) was changed.
		/// </summary>
		public event EventHandler TemplateModified;

		/// <summary>
		/// Raised when ever a property of the template's shape was changed.
		/// </summary>
		public event EventHandler TemplateShapeModified;

		/// <summary>
		/// Raised when ever a property of the template's model object was changed.
		/// </summary>
		public event EventHandler TemplateModelObjectModified;

		/// <summary>
		/// Raised when the template's shape is replaced by another shape.
		/// </summary>
		public event EventHandler<TemplateControllerTemplateShapeReplacedEventArgs> TemplateShapeChanged;

		/// <summary>
		/// Raised when the template's ModelObject is replaced by another ModelObject
		/// </summary>
		public event EventHandler<TemplateControllerModelObjectReplacedEventArgs> TemplateModelObjectChanged;

		/// <summary>
		/// Raised when the property mapping between shape and ModelObject was created or changed
		/// </summary>
		public event EventHandler<TemplateControllerPropertyMappingChangedEventArgs> TemplateShapePropertyMappingSet;

		/// <summary>
		/// Raised when the property mapping between shape and ModelObject was deleted
		/// </summary>
		public event EventHandler<TemplateControllerPropertyMappingChangedEventArgs> TemplateShapePropertyMappingDeleted;

		/// <summary>
		/// Raised when ConnectionPoints were enabled/disabled or mapped to other Terminals of the underlying ModelObject
		/// </summary>
		public event EventHandler<TemplateControllerPointMappingChangedEventArgs> TemplateShapeControlPointMappingChanged;

		#endregion


		#region [Public] Properties

		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		/// <summary>
		/// Provides access to a <see cref="T:Dataweb.NShape.Project" />.
		/// </summary>
		[CategoryNShape()]
		public Project Project {
			get { return _project; }
			set {
				if (_project != null) UnregisterProjectEvents();
				_project = value;
				if (_project != null) {
					RegisterProjectEvents();
					if (!_isInitializing && _project.IsOpen)
						Initialize(_project, OriginalTemplate);
				}
			}
		}


		/// <summary>
		/// Specified whether the <see cref="T:Dataweb.NShape.Controllers.TemplateController" /> edits an existing or creates a new template.
		/// </summary>
		[Browsable(false)]
		public TemplateControllerEditMode EditMode {
			get { return _editMode; }
		}


		/// <summary>
		/// A list of all shapes available.
		/// </summary>
		[Browsable(false)]
		public IReadOnlyCollection<Shape> Shapes {
			get { return _shapes; }
		}


		/// <summary>
		/// A list of all model objects available.
		/// </summary>
		[Browsable(false)]
		public IReadOnlyCollection<IModelObject> ModelObjects {
			get { return _modelObjects; }
		}


		/// <summary>
		/// A clone of the original template. This template will be modified. 
		/// When applying the changes, it will be copied into the original template property-by-property .
		/// </summary>
		[Browsable(false)]
		public Template WorkTemplate {
			get { return _workTemplate; }
		}


		/// <summary>
		/// The original template. Remains unchanged until applying changes.
		/// </summary>
		[Browsable(false)]
		public Template OriginalTemplate {
			get { return _originalTemplate; }
		}


		/// <summary>
		/// Specifies whether the <see cref="T:Dataweb.NShape.Controllers.TemplateController" /> was initialized completly.
		/// </summary>
		[Browsable(false)]
		public bool IsInitialized {
			get { return _isInitialized; }
		}

		#endregion


		#region [Public] Methods

		/// <summary>
		/// Calling this method initializes the <see cref="T:Dataweb.NShape.Controllers.TemplateController" />.
		/// </summary>
		public void Initialize(Template template) {
			if (_project == null) throw new ArgumentNullException("Property 'Project' is not set.");
			Initialize(Project, template);
		}


		/// <summary>
		/// Calling this method initializes the <see cref="T:Dataweb.NShape.Controllers.TemplateController" />.
		/// </summary>
		public void Initialize(Project project, Template template) {
			if (_isInitializing) {
				Debug.Fail("Already initializing");
				return;
			}
			try {
				_isInitializing = true;
				if (project == null) throw new ArgumentNullException("project");
				if (this._project != project) Project = project;

#if DEBUG_DIAGNOSTICS
				if (template != null)
					template.Tag = template.Name;
#endif

				// Check if there are shape types supporting templating
				bool templateSupportingShapeTypeFound = false;
				foreach (ShapeType shapeType in project.ShapeTypes) {
					if (shapeType.SupportsAutoTemplates) {
						templateSupportingShapeTypeFound = true;
						break;
					}
				}
				if (!templateSupportingShapeTypeFound) throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageTxt_NoTemplateSupportingShapeTypesFound);

				// Create a copy of the template
				if (template != null) {
					_editMode = TemplateControllerEditMode.EditTemplate;
					_originalTemplate = template;
					_workTemplate = new Template(_originalTemplate.Name, _originalTemplate.Shape.Clone());
					_workTemplate.CopyFrom(_originalTemplate);
					_workTemplate.Shape.DisplayService = this;
				} else {
					// Create a new Template
					_editMode = TemplateControllerEditMode.CreateTemplate;
					_originalTemplate = null;

					// As a shape is mandatory for every template, find a shape first
					Shape shape = FindFirstShapeOfType(typeof(IPlanarShape));
					if (shape == null) shape = FindFirstShapeOfType(typeof(Shape)); // if no planar shape was found, get the first one
					int templateCnt = 1;
					foreach (Template t in project.Repository.GetTemplates()) ++templateCnt;
					_workTemplate = new Template(string.Format("Template {0}", templateCnt), shape);
					shape.DisplayService = this;
				}

				// Disable all controls if the user has not the appropriate access rights
				if (!project.SecurityManager.IsGranted(Permission.Templates)) {
					// ToDo: implement access right restrictions
				}

				InitShapeList();
				InitModelObjectList();
				_isInitialized = true;

				if (Initializing != null) {
					TemplateControllerInitializingEventArgs eventArgs = new TemplateControllerInitializingEventArgs(_editMode, template);
					Initializing(this, eventArgs);
				}
			} finally {
				_isInitializing = false;
			}
		}


		/// <summary>
		/// Rename the current template.
		/// </summary>
		/// <param name="name"></param>
		public void SetTemplateName(string name) {
			if (_workTemplate.Name != name) {
				string oldName = _workTemplate.Name;
				_workTemplate.Name = name;
				TemplateWasChanged = true;

				if (TemplateModified != null) TemplateModified(this, EventArgs.Empty);
			}
		}


		/// <summary>
		/// Change the current template's title.
		/// </summary>
		public void SetTemplateTitle(string title) {
			if (_workTemplate.Title != title) {
				string oldTitle = _workTemplate.Title;
				_workTemplate.Title = title;
				TemplateWasChanged = true;

				if (TemplateModified != null) TemplateModified(this, EventArgs.Empty);
			}
		}


		/// <summary>
		/// Change the current template's description.
		/// </summary>
		public void SetTemplateDescription(string description) {
			if (_workTemplate.Description != description) {
				string oldDescription = _workTemplate.Name;
				_workTemplate.Description = description;
				TemplateWasChanged = true;

				if (TemplateModified != null) TemplateModified(this, EventArgs.Empty);
			}
		}
		

		/// <summary>
		/// Set the given shape as the template's shape.
		/// </summary>
		public void SetTemplateShape(Shape newShape) {
			if (newShape == null) throw new ArgumentNullException("newShape");
			// buffer the current template shape
			Shape oldShape = _workTemplate.Shape;
			if (oldShape != null)
				oldShape.Invalidate();
			
			// set the new template shape
			newShape.DisplayService = this;
			newShape.Invalidate();
			_workTemplate.Shape = newShape;

			TemplateWasChanged = true;
			if (TemplateShapeChanged != null) {
				_shapeReplacedEventArgs.Template = _workTemplate;
				_shapeReplacedEventArgs.OldTemplateShape = oldShape;
				_shapeReplacedEventArgs.NewTemplateShape = newShape;
				TemplateShapeChanged(this, _shapeReplacedEventArgs);
			}
		}


		/// <summary>
		/// Set the given Modelobject as the template's ModelObject
		/// </summary>
		public void SetTemplateModel(IModelObject newModelObject) {
			if (_workTemplate.Shape == null) throw new NShapeException(Properties.Resources.MessageTxt_TemplateHasNoShape);
			IModelObject oldModelObject = _workTemplate.Shape.ModelObject;
			if (oldModelObject != null) {
				// ToDo: Implement ModelObject.CopyFrom()
				//newModelObject.CopyFrom(oldModelObject);
			}
			_workTemplate.UnmapAllTerminals();
			_workTemplate.Shape.ModelObject = newModelObject;
			TemplateWasChanged = true;

			if (TemplateModelObjectChanged != null) {
				_modelObjectReplacedEventArgs.Template = _workTemplate;
				_modelObjectReplacedEventArgs.OldModelObject = oldModelObject;
				_modelObjectReplacedEventArgs.NewModelObject = newModelObject;
				TemplateModelObjectChanged(this, _modelObjectReplacedEventArgs);
			}
			if (TemplateModelObjectChanged != null)
				TemplateModelObjectChanged(this, null);
		}


		/// <summary>
		/// Define a new model-to-shape property mapping.
		/// </summary>
		public void SetModelMapping(IModelMapping modelMapping) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			_workTemplate.MapProperties(modelMapping);
			TemplateWasChanged = true;
			if (TemplateShapePropertyMappingSet != null) {
				_modelMappingChangedEventArgs.Template = _workTemplate;
				_modelMappingChangedEventArgs.ModelMapping = modelMapping;
				TemplateShapePropertyMappingSet(this, _modelMappingChangedEventArgs);
			}
		}


		/// <summary>
		/// Deletes a model-to-shape property mapping
		/// </summary>
		/// <param name="modelMapping"></param>
		public void DeleteModelMapping(IModelMapping modelMapping) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			_workTemplate.UnmapProperties(modelMapping);
			TemplateWasChanged = true;
			if (TemplateShapePropertyMappingDeleted != null) {
				_modelMappingChangedEventArgs.Template = _workTemplate;
				_modelMappingChangedEventArgs.ModelMapping = modelMapping;
				TemplateShapePropertyMappingDeleted(this, _modelMappingChangedEventArgs);
			}
		}


		/// <summary>
		/// If the template has no Modelobject, this method enables/disables ConnectionPoints of the shape.
		/// If the template has a ModelObject, this method assigns a ModelObject terminal to a ConnectionPoint of the shape
		/// </summary>
		/// <param name="controlPointId">Id of the shape's ControlPoint</param>
		/// <param name="terminalId">Id of the Modelobject's Terminal. Pass -1 in order to clear the mapping.</param>
		public void SetTerminalConnectionPointMapping(ControlPointId controlPointId, TerminalId terminalId) {
			TerminalId oldTerminalId = _workTemplate.GetMappedTerminalId(controlPointId);
			_workTemplate.MapTerminal(terminalId, controlPointId);
			TemplateWasChanged = true;

			if (TemplateShapeControlPointMappingChanged != null) {
				_controlPointMappingChangedEventArgs.ControlPointId = controlPointId;
				_controlPointMappingChangedEventArgs.OldTerminalId = oldTerminalId;
				_controlPointMappingChangedEventArgs.NewTerminalId = terminalId;
				TemplateShapeControlPointMappingChanged(this, _controlPointMappingChangedEventArgs);
			}
		}


		/// <summary>
		/// Applies all changes made on the working template to the original template.
		/// </summary>
		public void ApplyChanges() {
			if (string.IsNullOrEmpty(_workTemplate.Name)) throw new NShapeException(Properties.Resources.MessageTxt_TheTemplateNameMustNotBeEmpty);
			if (TemplateWasChanged) {
				ICommand cmd = null;
				switch (_editMode) {
					case TemplateControllerEditMode.CreateTemplate:
						cmd = new CreateTemplateCommand(_workTemplate);
						_project.ExecuteCommand(cmd);
						// after inserting the template into the cache, the template becomes the new 
						// originalTemplate and a new workTemplate has to be cloned.
						// TemplateControllerEditMode is changed from Create to Edit so the user can continue editing the 
						// template until the template editor is closed
						_originalTemplate = _workTemplate;
						// ToDo: Set appropriate DisplayService
						_originalTemplate.Shape.DisplayService = null;
						_workTemplate = _originalTemplate.Clone();
						_editMode = TemplateControllerEditMode.EditTemplate;
						break;

					case TemplateControllerEditMode.EditTemplate:
						// set workTemplate.Shape's DisplayService to the original shape's DisplayService 
						// (typically the ToolSetController)
						_workTemplate.Shape.DisplayService = _originalTemplate.Shape.DisplayService;
						if (_workTemplate.Shape.Type != _originalTemplate.Shape.Type)
							cmd = new ExchangeTemplateShapeCommand(_originalTemplate, _workTemplate);
						else
							cmd = new CopyTemplateFromTemplateCommand(_originalTemplate, _workTemplate);
						_project.ExecuteCommand(cmd);
						break;

					default: throw new NShapeUnsupportedValueException(typeof(TemplateControllerEditMode), _editMode);
				}
				TemplateWasChanged = false;
				if (ApplyingChanges != null) ApplyingChanges(this, EventArgs.Empty);
			}
		}


		/// <summary>
		/// Discards all changes made to the working copy of the original template.
		/// </summary>
		public void DiscardChanges() {
			if (EditMode == TemplateControllerEditMode.CreateTemplate)
				Initialize(_project, null);
			else
				Initialize(_project, _originalTemplate);
			if (DiscardingChanges != null) DiscardingChanges(this, EventArgs.Empty);
		}


		/// <summary>
		/// Clears all buffers and objects used by the <see cref="T:Dataweb.NShape.Controllers.TemplateController" />
		/// </summary>
		public void Clear() {
			ClearShapeList();
			ClearModelObjectList();

			_workTemplate = null;
			_originalTemplate = null;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void NotifyTemplateShapeChanged() {
			_templateWasChanged = true;
			if (TemplateShapeModified != null) TemplateShapeModified(this, EventArgs.Empty);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void NotifyTemplateModelObjectChanged() {
			_templateWasChanged = true;
			if (TemplateModelObjectModified != null) TemplateModelObjectModified(this, EventArgs.Empty);
		}

		#endregion


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				Clear();
				_infoGraphics.Dispose();
			}
			base.Dispose(disposing);
		}


		private bool TemplateWasChanged {
			get { return _templateWasChanged; }
			set {
				if (_project.SecurityManager.IsGranted(Permission.Templates)) {
					_templateWasChanged = value;
					if (TemplateModified != null) TemplateModified(this, EventArgs.Empty);
				}
			}
		}


		#region [Private] Methods

		private void RegisterProjectEvents() {
			Debug.Assert(_project != null);
			if (_project != null) {
				_project.Closing += project_Closing;
				_project.Opened += project_Opened;
			}
		}


		private void UnregisterProjectEvents() {
			Debug.Assert(_project != null);
			if (_project != null) {
				_project.Closing -= project_Closing;
				_project.Opened -= project_Opened;
			}
		}


		private void project_Opened(object sender, EventArgs e) {
			
		}


		private void project_Closing(object sender, EventArgs e) {
			
		}


		private bool IsOfType(Type type, Type targetType) {
			return (type == targetType || type.IsSubclassOf(targetType) || type.GetInterface(targetType.Name, true) != null);
		}
		
		
		private void ClearShapeList() {
			//foreach (Shape shape in shapes)
			//   shape.Dispose();
			_shapes.Clear();
		}


		private void ClearModelObjectList() {
			//foreach (IModelObject modelObject in modelObjects)
			//   modelObject.Dispose();
			_modelObjects.Clear();
		}


		private void InitShapeList() {
			ClearShapeList();
			foreach (ShapeType shapeType in _project.ShapeTypes) {
				if (!shapeType.SupportsAutoTemplates) continue;
				Shape shape = shapeType.CreateInstance();
				shape.DisplayService = this;
				_shapes.Add(shape);
			}
		}


		private void InitModelObjectList() {
			ClearModelObjectList();
			foreach (ModelObjectType modelObjectType in _project.ModelObjectTypes) {
				IModelObject modelObject = modelObjectType.CreateInstance();
				_modelObjects.Add(modelObject);
			}
		}


		private Shape FindFirstShapeOfType(Type type) {
			foreach (ShapeType shapeType in _project.ShapeTypes) {
				if (!shapeType.SupportsAutoTemplates) continue;
				Shape shape = shapeType.CreateInstance();
				if (IsOfType(shape.GetType(), type)) {
					if (shape is IPlanarShape) return shape;
				} else return shape;
			}
			return null;
		}

		#endregion


		#region Fields

		// IDisplayService fields
		private Graphics _infoGraphics;
		// TemplateController fields
		private Project _project;

		private TemplateControllerEditMode _editMode;
		private Template _originalTemplate;
		private Template _workTemplate;
		private ReadOnlyList<Shape> _shapes = new ReadOnlyList<Shape>();
		private ReadOnlyList<IModelObject> _modelObjects = new ReadOnlyList<IModelObject>();
		private bool _templateWasChanged = false;
		private bool _isInitializing = false;
		private bool _isInitialized = false;
		// EventArgs buffers
		private TemplateControllerStringChangedEventArgs _stringChangedEventArgs 
			= new TemplateControllerStringChangedEventArgs(string.Empty, string.Empty);
		private TemplateControllerTemplateShapeReplacedEventArgs _shapeReplacedEventArgs 
			= new TemplateControllerTemplateShapeReplacedEventArgs(null, null, null);
		private TemplateControllerModelObjectReplacedEventArgs _modelObjectReplacedEventArgs 
			= new TemplateControllerModelObjectReplacedEventArgs(null, null, null);
		private TemplateControllerPointMappingChangedEventArgs _controlPointMappingChangedEventArgs 
			= new TemplateControllerPointMappingChangedEventArgs(null, ControlPointId.None, TerminalId.Invalid, TerminalId.Invalid);
		private TemplateControllerPropertyMappingChangedEventArgs _modelMappingChangedEventArgs
			= new TemplateControllerPropertyMappingChangedEventArgs(null, null);

		#endregion
	}


	/// <summary>
	/// Specifies the edit mode of a <see cref="T:Dataweb.NShape.Controllers.TemplateController" />.
	/// </summary>
	public enum TemplateControllerEditMode { 
		/// <summary>Compose a new template.</summary>
		CreateTemplate, 
		/// <summary>Modify an existing template.</summary>
		EditTemplate 
	};


	#region EventArgs

	/// <summary>
	/// Encapsulates parameters for an event raised when the <see cref="T:Dataweb.NShape.Controllers.TemplateController" /> is initialized.
	/// </summary>
	public class TemplateControllerInitializingEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public TemplateControllerInitializingEventArgs(TemplateControllerEditMode editMode, Template template) {
			this._editMode = editMode;
			this._template = template;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public TemplateControllerEditMode EditMode {
			get { return _editMode; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Template Template {
			get { return _template; }
		}


		private TemplateControllerEditMode _editMode;
		private Template _template;
	}


	/// <summary>
	/// Encapsulates parameters for an event raised when the name of the <see cref="T:Dataweb.NShape.Controllers.TemplateController" />'s template is modified.
	/// </summary>
	public class TemplateControllerStringChangedEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public TemplateControllerStringChangedEventArgs(string oldString, string newString) {
			this._oldString = oldString;
			this._newString = newString;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public string OldString {
			get { return _oldString; }
			internal set { _oldString = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public string NewString {
			get { return _newString; }
			internal set { _newString = value; }
		}


		private string _newString;
		private string _oldString;
	}


	/// <summary>
	/// Encapsulates parameters for a template-related <see cref="T:Dataweb.NShape.Controllers.TemplateController" /> event.
	/// </summary>
	public class TemplateControllerTemplateEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public TemplateControllerTemplateEventArgs(Template template) {
			this._template = template;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Template Template {
			get { return _template; }
			internal set { _template = value; }
		}


		private Template _template;
	}


	/// <summary>
	/// Encapsulates parameters for an event raised when the <see cref="T:Dataweb.NShape.Controllers.TemplateController" />'s template shape is replaced ba a shape of another Type.
	/// </summary>
	public class TemplateControllerTemplateShapeReplacedEventArgs : TemplateControllerTemplateEventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public TemplateControllerTemplateShapeReplacedEventArgs(Template template, Shape oldTemplateShape, Shape newTemplateShape)
			: base(template) {
			this._oldTemplateShape = oldTemplateShape;
			this._newTemplateShape = newTemplateShape;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Shape OldTemplateShape {
			get { return _oldTemplateShape; }
			internal set { _oldTemplateShape = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Shape NewTemplateShape {
			get { return _newTemplateShape; }
			internal set { _newTemplateShape = value; }
		}


		private Shape _oldTemplateShape;
		private Shape _newTemplateShape;
	}


	/// <summary>
	/// Encapsulates parameters for an event raised when the <see cref="T:Dataweb.NShape.Controllers.TemplateController" />'s template model object is replaced by a model object of another model object type.
	/// </summary>
	public class TemplateControllerModelObjectReplacedEventArgs : TemplateControllerTemplateEventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public TemplateControllerModelObjectReplacedEventArgs(Template template,
			IModelObject oldModelObject, IModelObject newModelObject)
			: base(template) {
			this._oldModelObject = oldModelObject;
			this._newModelObject = newModelObject;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IModelObject OldModelObject {
			get { return _oldModelObject; }
			internal set { _oldModelObject = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IModelObject NewModelObject {
			get { return _newModelObject; }
			internal set { _newModelObject = value; }
		}

		private IModelObject _oldModelObject;
		private IModelObject _newModelObject;
	}


	/// <summary>
	/// Encapsulates parameters for a <see cref="T:Dataweb.NShape.Controllers.TemplateController" /> event raised when 
	/// the mapping of a <see cref="T:Dataweb.NShape.Advanced.ControlPointId" /> to a <see cref="T:Dataweb.NShape.Advanced.TerminalId" /> is modified.
	/// </summary>
	public class TemplateControllerPropertyMappingChangedEventArgs : TemplateControllerTemplateEventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public TemplateControllerPropertyMappingChangedEventArgs(Template template, IModelMapping modelMapping)
			: base(template) {
			this._propertyMapping = modelMapping;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public IModelMapping ModelMapping {
			get { return _propertyMapping; }
			internal set { _propertyMapping = value; }
		}

		private IModelMapping _propertyMapping = null;
	}


	/// <summary>
	/// Encapsulates parameters for a <see cref="T:Dataweb.NShape.Controllers.TemplateController" /> event raised when the mapping of shape's properties to modeloject's properties is modified.
	/// </summary>
	public class TemplateControllerPointMappingChangedEventArgs : TemplateControllerTemplateEventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public TemplateControllerPointMappingChangedEventArgs(Template template, ControlPointId controlPointId, TerminalId oldTerminalId, TerminalId newTerminalId)
			: base(template) {
			this._controlPointId = controlPointId;
			this._oldTerminalId = oldTerminalId;
			this._newTerminalId = newTerminalId;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public ControlPointId ControlPointId {
			get { return _controlPointId; }
			internal set { _controlPointId = value; }
		}

		/// <ToBeCompleted></ToBeCompleted>
		public TerminalId OldTerminalId {
			get { return _oldTerminalId; }
			internal set { _oldTerminalId = value; }
		}

		/// <ToBeCompleted></ToBeCompleted>
		public TerminalId NewTerminalId {
			get { return _newTerminalId; }
			internal set { _newTerminalId = value; }
		}

		private ControlPointId _controlPointId;
		private TerminalId _oldTerminalId;
		private TerminalId _newTerminalId;
	}

	#endregion

}
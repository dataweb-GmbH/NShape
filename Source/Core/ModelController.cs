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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;


namespace Dataweb.NShape.Controllers {

	/// <ToBeCompleted></ToBeCompleted>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(ModelController), "ModelController.bmp")]
	public class ModelController : Component {

		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameDeleteModelObjects = "DeleteModelObjectsAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameCopyModelObjects = "CopyModelObjectsAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNamePasteModelObjects = "PasteModelObjectsAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameFindShapes = "FindShapesAction";


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.ModelController" />.
		/// </summary>
		public ModelController()
			: base() {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.ModelController" />.
		/// </summary>
		public ModelController(DiagramSetController diagramSetController)
			: this() {
			if (diagramSetController == null) throw new ArgumentNullException("diagramSetController");
			DiagramSetController = diagramSetController;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.ModelController" />.
		/// </summary>
		public ModelController(Project project)
			: this() {
			if (project == null) throw new ArgumentNullException("project");
			Project = project;
		}


		#region [Public] Events

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler Initialized;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler Uninitialized;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsCreated;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsChanged;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsDeleted;

		/// <summary>
		/// The Changed event will be raised whenever an object somehow related to a model object has changed.
		/// </summary>
		public event EventHandler Changed;

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
		[ReadOnly(true)]
		[CategoryNShape()]
		public Project Project {
			get { return (_diagramSetController == null) ? _project : _diagramSetController.Project; }
			set {
				if (_diagramSetController != null && _diagramSetController.Project != value) {
					string errMsg = string.Format(Properties.Resources.MessageFmt_A0IsAlreadyAssignedItsProjectWillBeUsed, _diagramSetController.GetType().Name);
					throw new InvalidOperationException(errMsg);
				}
				if (Project != value) {
					DetachProject();
					_project = value;
					AttachProject();
				}
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		[CategoryNShape()]
		public DiagramSetController DiagramSetController {
			get { return _diagramSetController; }
			set {
				if (Project != null) DetachProject();
				if (_diagramSetController != null) UnregisterDiagramSetControllerEvents();
				_diagramSetController = value;
				if (_diagramSetController != null) {
					RegisterDiagramSetControllerEvents();
					AttachProject();
				}
			}
		}

		#endregion


		#region [Public] Methods

		/// <ToBeCompleted></ToBeCompleted>
		public void CreateModelObject() {
			throw new NotImplementedException();
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void RenameModelObject(IModelObject modelObject, string newName) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			throw new NotImplementedException();
		}


		/// <summary>
		/// Deletes the given model obejcts and their attached shapes
		/// </summary>
		/// <param name="modelObjects"></param>
		public void DeleteModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			ICommand cmd = new DeleteModelObjectsCommand (modelObjects);
			Project.ExecuteCommand(cmd);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SetModelObjectParent(IModelObject modelObject, IModelObject parent) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			ICommand cmd = new SetModelObjectParentCommand(modelObject, parent);
			Project.ExecuteCommand(cmd);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Copy(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			_copyPasteBuffer.Clear();
			_copyPasteBuffer.Add(modelObject.Clone());
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Copy(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			_copyPasteBuffer.Clear();
			foreach (IModelObject mo in modelObjects)
				_copyPasteBuffer.Add(mo.Clone());
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Paste(IModelObject parent) {
			// Set parent
			for (int i = _copyPasteBuffer.Count - 1; i >= 0; --i)
				_copyPasteBuffer[i].Parent = parent;
			// Execute command
			ICommand command = new CreateModelObjectsCommand(_copyPasteBuffer);
			Project.ExecuteCommand(command);
			// Copy for next paste action
			for (int i = _copyPasteBuffer.Count - 1; i >= 0; --i)
				_copyPasteBuffer[i] = _copyPasteBuffer[i].Clone();
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<IModelObject> GetChildModelObjects(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			return Project.Repository.GetModelObjects(modelObject);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void FindShapes(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			if (_diagramSetController == null) throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_DiagramSetControllerIsNotSet);
			_diagramSetController.SelectModelObjects(modelObjects);
		}


		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		public IEnumerable<MenuItemDef> GetMenuItemDefs(IReadOnlyCollection<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			
			// New...
			// Rename
			yield return CreateDeleteModelObjectsAction(modelObjects);
			
			yield return new SeparatorMenuItemDef();

			yield return CreateCopyModelObjectsAction(modelObjects);
			// Cut
			yield return CreatePasteModelObjectsAction(modelObjects);

			yield return new SeparatorMenuItemDef();
			
			// Find model object...
			yield return CreateFindShapesAction(modelObjects);
		}

		#endregion


		#region [Private] Methods: (Un)Registering event handlers

		private void DetachProject() {
			if (Project != null) {
				UnregisterProjectEvents();
				_project = null;
			}
		}


		private void AttachProject() {
			if (Project != null) {
				// Register current project
				RegisterProjectEvents();
			}
		}


		private void RegisterDiagramSetControllerEvents() {
			Debug.Assert(_diagramSetController != null);
			_diagramSetController.ProjectChanged += diagramSetController_ProjectChanged;
			_diagramSetController.ProjectChanging += diagramSetController_ProjectChanging;
		}


		private void UnregisterDiagramSetControllerEvents() {
			Debug.Assert(_diagramSetController != null);
			_diagramSetController.ProjectChanged -= diagramSetController_ProjectChanged;
			_diagramSetController.ProjectChanging -= diagramSetController_ProjectChanging;
		}


		private void RegisterProjectEvents() {
			Debug.Assert(Project != null);

			// Register project events
			Project.Opened += project_ProjectOpen;
			Project.Closing += project_ProjectClosing;
			Project.Closed += project_ProjectClosed;
			
			// Register repository events
			if (Project.IsOpen) project_ProjectOpen(this, null);
		}


		private void UnregisterProjectEvents() {
			Debug.Assert(Project != null);

			// Unregister repository events
			if (Project.Repository != null) UnregisterRepositoryEvents();
			
			// Unregister project events
			Project.Opened -= project_ProjectOpen;
			Project.Closing -= project_ProjectClosing;
			Project.Closed -= project_ProjectClosed;
		}


		private void RegisterRepositoryEvents() {
			Debug.Assert(Project != null && Project.Repository != null);
			Project.Repository.ModelObjectsInserted += repository_ModelObjectsInserted;
			Project.Repository.ModelObjectsUpdated += repository_ModelObjectsUpdated;
			Project.Repository.ModelObjectsDeleted += repository_ModelObjectsDeleted;
			Project.Repository.TemplateInserted += repository_TemplateInserted;
			Project.Repository.TemplateUpdated += repository_TemplateUpdated;
			Project.Repository.TemplateDeleted += repository_TemplateDeleted;
			Project.Repository.TemplateShapeReplaced += repository_TemplateShapeReplaced;
		}


		private void UnregisterRepositoryEvents() {
			Debug.Assert(Project != null && Project.Repository != null);
			Project.Repository.ModelObjectsInserted -= repository_ModelObjectsInserted;
			Project.Repository.ModelObjectsUpdated -= repository_ModelObjectsUpdated;
			Project.Repository.ModelObjectsDeleted -= repository_ModelObjectsDeleted;
			Project.Repository.TemplateInserted -= repository_TemplateInserted;
			Project.Repository.TemplateUpdated -= repository_TemplateUpdated;
			Project.Repository.TemplateDeleted -= repository_TemplateDeleted;
			Project.Repository.TemplateShapeReplaced -= repository_TemplateShapeReplaced;
		}

		#endregion


		#region [Private] Methods: DiagramSetController event handler implementations

		private void diagramSetController_ProjectChanged(object sender, EventArgs e) {
			if (_diagramSetController.Project != null) AttachProject();
		}


		private void diagramSetController_ProjectChanging(object sender, EventArgs e) {
			if (_diagramSetController.Project != null) DetachProject();
		}

		#endregion


		#region [Private] Methods: Project event handler implementations

		private void project_ProjectOpen(object sender, EventArgs e) {
			RegisterRepositoryEvents();
			if (Initialized != null) Initialized(this, EventArgs.Empty);
		}


		private void project_ProjectClosing(object sender, EventArgs e) {
			UnregisterRepositoryEvents();
			if (Uninitialized != null) Uninitialized(this, EventArgs.Empty);
		}


		private void project_ProjectClosed(object sender, EventArgs e) {
			// nothing to do here
		}

		#endregion


		#region [Private] Methods: Repository event handler implementations

		private void repository_ModelObjectsInserted(object sender, RepositoryModelObjectsEventArgs e) {
			if (ModelObjectsCreated != null) ModelObjectsCreated(this, e);
		}


		private void repository_ModelObjectsUpdated(object sender, RepositoryModelObjectsEventArgs e) {
			if (ModelObjectsChanged != null) ModelObjectsChanged(this, e);
		}


		private void repository_ModelObjectsDeleted(object sender, RepositoryModelObjectsEventArgs e) {
			if (ModelObjectsDeleted != null) ModelObjectsDeleted(this, e);
		}


		private void repository_TemplateShapeReplaced(object sender, RepositoryTemplateShapeReplacedEventArgs e) {
			if (e.OldTemplateShape.ModelObject != null || e.NewTemplateShape.ModelObject != null)
				if (Changed != null) Changed(this, EventArgs.Empty);
		}


		private void repository_TemplateInserted(object sender, RepositoryTemplateEventArgs e) {
			if (e.Template.Shape.ModelObject != null)
				if (Changed != null) Changed(this, EventArgs.Empty);
		}


		private void repository_TemplateUpdated(object sender, RepositoryTemplateEventArgs e) {
			if (e.Template.Shape.ModelObject != null)
				if (Changed != null) Changed(this, EventArgs.Empty);
		}


		private void repository_TemplateDeleted(object sender, RepositoryTemplateEventArgs e) {
			if (e.Template.Shape.ModelObject != null)
				if (Changed != null) Changed(this, EventArgs.Empty);
		}

		#endregion


		#region [Private] Methods: Create actions

		private MenuItemDef CreateDeleteModelObjectsAction(IReadOnlyCollection<IModelObject> modelObjects) {
			string description;
			bool isFeasible;
			if (modelObjects != null && modelObjects.Count > 0) {
				isFeasible = true;
				description = (modelObjects.Count > 0) ? string.Format(Properties.Resources.MessageFmt_Delete0ModelObjects, modelObjects.Count) 
					: Properties.Resources.MessageFmt_DeleteModelObject;
				foreach (IModelObject modelObject in modelObjects) {
					foreach (IModelObject mo in Project.Repository.GetModelObjects(modelObject))
						if (mo.ShapeCount > 0) {
							isFeasible = false;
							description = Properties.Resources.MessageTxt_OneOrMoreChildModelObjectsAreAttachedToShapes;
						}
				}
			} else {
				isFeasible = false;
				description = Properties.Resources.MessageTxt_NoModelObjectsSelected;
			}

			return new DelegateMenuItemDef(MenuItemNameDeleteModelObjects, Properties.Resources.CaptionTxt_Delete, Properties.Resources.DeleteBtn,
				description, false, isFeasible, Permission.None,
				(a, p) => DeleteModelObjects(modelObjects));
		}


		private MenuItemDef CreateCopyModelObjectsAction(IReadOnlyCollection<IModelObject> modelObjects) {
			bool isFeasible = (modelObjects != null && modelObjects.Count > 0);
			string description;
			if (isFeasible)
				description = (modelObjects.Count > 1) ? string.Format(Properties.Resources.MessageFmt_Copy0ModelObjects, modelObjects.Count) : Properties.Resources.MessageTxt_CopyModelObject;
			else description = Properties.Resources.MessageTxt_NoModelObjectsSelected;
			return new DelegateMenuItemDef(MenuItemNameCopyModelObjects, Properties.Resources.CaptionTxt_Copy, Properties.Resources.CopyBtn, 
				description, false, isFeasible, Permission.None,
				(a, p) => Copy(modelObjects));
		}


		private MenuItemDef CreatePasteModelObjectsAction(IReadOnlyCollection<IModelObject> modelObjects) {
			bool isFeasible = (_copyPasteBuffer.Count > 0 && modelObjects.Count <= 1);
			string description;
			if (isFeasible)
				description = (_copyPasteBuffer.Count > 1) ? string.Format(Properties.Resources.MessageFmt_Paste0ModelObjects, _copyPasteBuffer.Count) : Properties.Resources.MessageTxt_PasteModelObject;
			else description = Properties.Resources.MessageTxt_NoModelObjectsCopied;
			
			IModelObject parent = null;
			foreach (IModelObject mo in modelObjects) {
				parent = mo;
				break;
			}
			return new DelegateMenuItemDef(MenuItemNamePasteModelObjects, Properties.Resources.CaptionTxt_Paste, Properties.Resources.PasteBtn, 
				description, false, isFeasible, Permission.None,
				(a, p) => Paste(parent));
		}


		private MenuItemDef CreateFindShapesAction(IReadOnlyCollection<IModelObject> modelObjects) {
			bool isFeasible = (_diagramSetController != null);
			string description = Properties.Resources.MessageTxt_FindAndSelectAllAssignedShapes;
			return new DelegateMenuItemDef(MenuItemNameFindShapes, Properties.Resources.MessageTxt_FindAssignedShapes, Properties.Resources.FindShapes, 
				description, false, isFeasible, Permission.None,
				(a, p) => FindShapes(modelObjects));
		}

		#endregion


		#region Fields

		private DiagramSetController _diagramSetController;
		private Project _project;
		private List<IModelObject> _copyPasteBuffer = new List<IModelObject>();

		#endregion
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class ModelObjectSelectedEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public ModelObjectSelectedEventArgs(IEnumerable<IModelObject> selectedModelObjects, bool ensureVisibility) {
			if (selectedModelObjects == null) throw new ArgumentNullException("selectedModelObjects");
			this._modelObjects = new List<IModelObject>(selectedModelObjects);
			this._ensureVisibility = ensureVisibility;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<IModelObject> SelectedModelObjects {
			get { return _modelObjects; }
			internal set {
				_modelObjects.Clear();
				if (value != null) _modelObjects.AddRange(value);
			}
		}

		/// <ToBeCompleted></ToBeCompleted>
		public bool EnsureVisibility {
			get { return _ensureVisibility; }
			internal set { _ensureVisibility = value; }
		}

		internal ModelObjectSelectedEventArgs() {
			_modelObjects = new List<IModelObject>();
			_ensureVisibility = false;
		}
		
		private List<IModelObject> _modelObjects;
		private bool _ensureVisibility;
	}

}
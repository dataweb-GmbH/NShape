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
using System.Drawing;
using System.Reflection;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;


namespace Dataweb.NShape.Controllers {
	
	/// <ToBeCompleted></ToBeCompleted>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(PropertyController), "PropertyController.bmp")]
	public class PropertyController : Component, IPropertyController {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.PropertyController" />.
		/// </summary>
		public PropertyController() {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.PropertyController" />.
		/// </summary>
		public PropertyController(Project project) {
			if (project == null) throw new ArgumentNullException(nameof(project));
			Project = project;
		}


		/// <summary>
		/// Finalizer of <see cref="T:Dataweb.NShape.Controllers.PropertyController" />.
		/// </summary>
		~PropertyController() {
			Project = null;
		}


		#region [Public] IPropertyController Events

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<PropertyControllerEventArgs> ObjectsSet;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<PropertyControllerEventArgs> SelectedObjectsChanging;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<PropertyControllerEventArgs> SelectedObjectsChanged;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<PropertyControllerPropertyChangedEventArgs> PropertyChanged;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler<PropertyControllerEventArgs> ObjectsModified;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler ProjectClosing;

		#endregion


		#region [Public] IPropertyController Properties

		/// <summary>
		/// Provides access to a <see cref="T:Dataweb.NShape.Project" />.
		/// </summary>
		[CategoryNShape()]
		public Project Project {
			get { return _project; }
			set {
				if (_project != null) {
					CancelSetProperty();
					if (_project.Repository != null) UnregisterRepositoryEvents();
					UnregisterProjectEvents();
				}
				_project = value;
				if (_project != null) {
					RegisterProjectEvents();
					if (_project.IsOpen) RegisterRepositoryEvents();
				}
			}
		}


		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		/// <summary>
		/// Specifies the display mode for properties that are not editable, in most cases due to insufficient permissions.
		/// </summary>
		[CategoryBehavior()]
		public NonEditableDisplayMode PropertyDisplayMode {
			get { return _propertyDisplayMode; }
			set { _propertyDisplayMode = value; }
		}

		#endregion


		#region [Public] IPropertyController Methods

		/// <ToBeCompleted></ToBeCompleted>
		public void SetPropertyValue(object obj, string propertyName, object oldValue, object newValue) {
			try {
				try {
					AssertProjectExists();
					// ToDo: 
					// If the objects are not of the same type but of the same base type (or interface), this method will fail.
					// So we have to get the common base type/interface in this case.
					if (_propertySetBuffer != null
						&& _propertySetBuffer.Objects.Count > 0
						&& !IsOfType(obj.GetType(), _propertySetBuffer.ObjectType))
						throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_AnotherTransactionIsPending);
					if (_propertySetBuffer == null) {
						PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
						if (obj is Shape)
							_propertySetBuffer = new PropertySetInfo<Shape>(propertyInfo, newValue);
						else if (obj is IModelObject)
							_propertySetBuffer = new PropertySetInfo<IModelObject>(propertyInfo, newValue);
						else if (obj is IDiagramModelObject)
							_propertySetBuffer = new PropertySetInfo<IDiagramModelObject>(propertyInfo, newValue);
						else if (obj is Diagram)
							_propertySetBuffer = new PropertySetInfo<Diagram>(propertyInfo, newValue);
						else if (obj is Layer)
							_propertySetBuffer = new PropertySetInfo<Layer>(propertyInfo, newValue);
						else if (obj is Design)
							_propertySetBuffer = new PropertySetInfo<Design>(propertyInfo, newValue);
						else if (obj is Style)
							_propertySetBuffer = new PropertySetInfo<Style>(propertyInfo, newValue);
						else
							throw new NotSupportedException(string.Format("PropertyController does not support objects of type '{0}'", obj.GetType().Name));
					}
					_propertySetBuffer.Objects.Add(obj);
					_propertySetBuffer.OldValues.Add(oldValue);

					// If all properties are set, commit changes.
					if (_selectedPrimaryObjects.Contains(obj)
						&& _propertySetBuffer.Objects.Count == _selectedPrimaryObjects.Count)
						CommitSetProperty();
					else if (_selectedSecondaryObjects.Contains(obj)
						&& _propertySetBuffer.Objects.Count == _selectedSecondaryObjects.Count)
						CommitSetProperty();
				} catch (TargetInvocationException exc) {
					if (exc.InnerException != null)
						throw exc.InnerException;
				}
			} catch (Exception) {
				CancelSetProperty();
				throw;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void CancelSetProperty() {
			AssertProjectExists();
			// Discard buffer
			_propertySetBuffer = null;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void CommitSetProperty() {
			AssertProjectExists();
			if (_propertySetBuffer != null) {
				ICommand command = null;
				if (_propertySetBuffer.Objects.Count != 0) {
					if (IsOfType(_propertySetBuffer.ObjectType, typeof(Shape))) {
						command = new ShapePropertySetCommand(
							ConvertEnumerator<Shape>.Create(_propertySetBuffer.Objects),
							_propertySetBuffer.PropertyInfo,
							_propertySetBuffer.OldValues,
							_propertySetBuffer.NewValue);
					} else if (IsOfType(_propertySetBuffer.ObjectType, typeof(IModelObject))) {
						command = new ModelObjectPropertySetCommand(
							ConvertEnumerator<IModelObject>.Create(_propertySetBuffer.Objects),
							_propertySetBuffer.PropertyInfo,
							_propertySetBuffer.OldValues,
							_propertySetBuffer.NewValue);
					} else if (IsOfType(_propertySetBuffer.ObjectType, typeof(IDiagramModelObject))) {
						command = new DiagramModelObjectPropertySetCommand(
							ConvertEnumerator<IDiagramModelObject>.Create(_propertySetBuffer.Objects),
								_propertySetBuffer.PropertyInfo,
								_propertySetBuffer.OldValues,
								_propertySetBuffer.NewValue);
					} else if (IsOfType(_propertySetBuffer.ObjectType, typeof(Diagram))) {
						command = new DiagramPropertySetCommand(
							ConvertEnumerator<Diagram>.Create(_propertySetBuffer.Objects),
							_propertySetBuffer.PropertyInfo,
							_propertySetBuffer.OldValues,
							_propertySetBuffer.NewValue);
					} else if (IsOfType(_propertySetBuffer.ObjectType, typeof(Layer))) {
						Diagram diagram = null;
						foreach (Diagram d in _project.Repository.GetDiagrams()) {
							foreach (Layer l in ConvertEnumerator<Layer>.Create(_propertySetBuffer.Objects)) {
								if (d.Layers.Contains(l)) {
									diagram = d;
									break;
								}
								if (diagram != null) break;
							}
						}
						command = new LayerPropertySetCommand(
							diagram,
							ConvertEnumerator<Layer>.Create(_propertySetBuffer.Objects),
							_propertySetBuffer.PropertyInfo,
							_propertySetBuffer.OldValues,
							_propertySetBuffer.NewValue);
					} else if (IsOfType(_propertySetBuffer.ObjectType, typeof(Design))) {
						command = new DesignPropertySetCommand(
							ConvertEnumerator<Design>.Create(_propertySetBuffer.Objects),
							_propertySetBuffer.PropertyInfo,
							_propertySetBuffer.OldValues,
							_propertySetBuffer.NewValue);
					} else if (IsOfType(_propertySetBuffer.ObjectType, typeof(Style))) {
						Design design = null;
						if (_project.Repository != null) {
							foreach (Design d in _project.Repository.GetDesigns()) {
								foreach (Style s in ConvertEnumerator<Style>.Create(_propertySetBuffer.Objects)) {
									if (d.ContainsStyle(s)) {
										design = d;
										break;
									}
								}
								if (design != null) break;
							}
						} else design = _project.Design;
						if (design != null) {
							command = new StylePropertySetCommand(
								design,
								ConvertEnumerator<Style>.Create(_propertySetBuffer.Objects),
								_propertySetBuffer.PropertyInfo,
								_propertySetBuffer.OldValues,
								_propertySetBuffer.NewValue);
						}
					} else throw new NotSupportedException();
				}

				if (command != null) {
					// Check if command execution is allowed
					Exception exc = command.CheckAllowed(Project.SecurityManager);
					if (exc != null) throw exc;
					//
					if (_updateRepository) Project.ExecuteCommand(command);
					else command.Execute();
					if (PropertyChanged != null) {
						int pageIndex;
						Hashtable selectedObjectsList;
						GetSelectedObjectList(_propertySetBuffer.Objects, out pageIndex, out selectedObjectsList);
						if (pageIndex >= 0) {
							PropertyControllerPropertyChangedEventArgs e = new PropertyControllerPropertyChangedEventArgs(
								pageIndex,
								_propertySetBuffer.Objects,
								_propertySetBuffer.PropertyInfo,
								_propertySetBuffer.OldValues,
								_propertySetBuffer.NewValue);
							PropertyChanged(this, e);
						}
					}
				} else 
					CancelSetProperty();
			}
			_propertySetBuffer = null;
		}

		#endregion


		#region [Public] Methods

		/// <ToBeCompleted></ToBeCompleted>
		public void SetObject(int pageIndex, object selectedObject) {
			AssertProjectExists();
			SetObject(pageIndex, selectedObject, true);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SetObject(int pageIndex, object selectedObject, bool isPersistent) {
			AssertProjectExists();
			OnSelectedObjectsChanging(pageIndex, selectedObject);

			Hashtable selectedObjectsList;
			GetSelectedObjectList(pageIndex, out selectedObjectsList);
			selectedObjectsList.Clear();
			_updateRepository = isPersistent;
			if (selectedObject != null) selectedObjectsList.Add(selectedObject, null);

			OnSelectedObjectsChanged(pageIndex, selectedObject);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SetObjects(int pageIndex, IEnumerable selectedObjects) {
			AssertProjectExists();
			SetObjects(pageIndex, selectedObjects, true);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SetObjects(int pageIndex, IEnumerable selectedObjects, bool arePersistent) {
			if (selectedObjects == null) throw new ArgumentNullException(nameof(selectedObjects));
			AssertProjectExists();
			OnSelectedObjectsChanging(pageIndex, selectedObjects);

			Hashtable selectedObjectsList;
			GetSelectedObjectList(pageIndex, out selectedObjectsList);
			selectedObjectsList.Clear();

			_updateRepository = arePersistent;
			foreach (object o in selectedObjects) {
				if (!selectedObjectsList.Contains(o))
					selectedObjectsList.Add(o, null);
			}

			OnSelectedObjectsChanged(pageIndex, selectedObjects);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public object GetSelectedObject(int pageIndex) {
			object result = null;
			Hashtable selectedObjects;
			GetSelectedObjectList(pageIndex, out selectedObjects);
			if (selectedObjects.Count > 0) {
				foreach (DictionaryEntry item in selectedObjects) {
					result = item.Key;
					break;
				}
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<object> GetSelectedObjects(int pageIndex) {
			Hashtable selectedObjects;
			GetSelectedObjectList(pageIndex, out selectedObjects);
			foreach (DictionaryEntry item in selectedObjects)
				yield return item.Key;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int GetSelectedObjectsCount(int pageIndex) {
			Hashtable selectedObjects;
			GetSelectedObjectList(pageIndex, out selectedObjects);
			return selectedObjects.Count;
		}

		#endregion


		#region [Protected] Methods

		/// <summary>Raises the SelectedObjectsChanging event.</summary>
		protected virtual void OnSelectedObjectsChanging(int pageIndex, object selectedObject)
		{
			if (SelectedObjectsChanging != null) SelectedObjectsChanging(this, new PropertyControllerEventArgs(pageIndex, selectedObject));
		}


		/// <summary>Raises the SelectedObjectsChanging event.</summary>
		protected virtual void OnSelectedObjectsChanging(int pageIndex, IEnumerable selectedObjects)
		{
			if (SelectedObjectsChanging != null) SelectedObjectsChanging(this, new PropertyControllerEventArgs(pageIndex, selectedObjects));
		}


		/// <summary>Raises the SelectedObjectsChanged event.</summary>
		/// <remarks>At the moment, this method also raises the obsolete ObjectsSet event.</remarks>
		protected virtual void OnSelectedObjectsChanged(int pageIndex, object selectedObject)
		{
			if (SelectedObjectsChanged != null) SelectedObjectsChanged(this, new PropertyControllerEventArgs(pageIndex, selectedObject));
			// ToDo: Remove in future versions
			if (ObjectsSet != null) ObjectsSet(this, new PropertyControllerEventArgs(pageIndex, selectedObject));
		}


		/// <summary>Raises the SelectedObjectsChanged event.</summary>
		/// <remarks>At the moment, this method also raises the obsolete ObjectsSet event.</remarks>
		protected virtual void OnSelectedObjectsChanged(int pageIndex, IEnumerable selectedObjects)
		{
			if (SelectedObjectsChanged != null) SelectedObjectsChanged(this, new PropertyControllerEventArgs(pageIndex, selectedObjects));
			// ToDo: Remove in future versions
			if (ObjectsSet != null) ObjectsSet(this, new PropertyControllerEventArgs(pageIndex, selectedObjects));
		}


		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing)
				UnregisterRepositoryEvents();
			base.Dispose(disposing);
		}

		#endregion


		#region [Private] Methods: Registering for events

		private void RegisterProjectEvents() {
			if (_project != null) {
				_project.Opened += project_ProjectOpen;
				_project.Closing += project_ProjectClosing;
				_project.Closed += project_ProjectClosed;
			}
		}


		private void RegisterRepositoryEvents() {
			if (_project != null && _project.Repository != null) {
				_project.Repository.StyleUpdated += repository_StyleUpdated;
				_project.Repository.DiagramUpdated += repository_DiagramUpdated;
				_project.Repository.ShapesUpdated += repository_ShapesUpdated;
				_project.Repository.ModelObjectsUpdated += repository_ModelObjectsUpdated;

				_project.Repository.StyleDeleted += Repository_StyleDeleted;
				_project.Repository.DiagramDeleted += Repository_DiagramDeleted;
				_project.Repository.ShapesDeleted += Repository_ShapesDeleted;
				_project.Repository.ModelObjectsDeleted += Repository_ModelObjectsDeleted;
			}
		}


		private void UnregisterProjectEvents() {
			if (_project != null) {
				_project.Opened -= project_ProjectOpen;
				_project.Closing -= project_ProjectClosing;
				_project.Closed -= project_ProjectClosed;
			}
		}


		private void UnregisterRepositoryEvents() {
			if (_project != null && _project.Repository != null) {
				_project.Repository.StyleUpdated -= repository_StyleUpdated;
				_project.Repository.DiagramUpdated -= repository_DiagramUpdated;
				_project.Repository.ShapesUpdated -= repository_ShapesUpdated;
				_project.Repository.ModelObjectsUpdated -= repository_ModelObjectsUpdated;

				_project.Repository.StyleDeleted -= Repository_StyleDeleted;
				_project.Repository.DiagramDeleted -= Repository_DiagramDeleted;
				_project.Repository.ShapesDeleted -= Repository_ShapesDeleted;
				_project.Repository.ModelObjectsDeleted -= Repository_ModelObjectsDeleted;
			}
		}

		#endregion


		#region [Private] Methods: Event Handler implementation

		private void project_ProjectOpen(object sender, EventArgs e) {
			RegisterRepositoryEvents();
		}


		private void project_ProjectClosing(object sender, EventArgs e) {
			if (ProjectClosing != null) ProjectClosing(this, EventArgs.Empty);
		}


		private void project_ProjectClosed(object sender, EventArgs e) {
			UnregisterRepositoryEvents();
		}


		private void repository_StyleUpdated(object sender, RepositoryStyleEventArgs e) {
			OnObjectModified(e.Style);
		}


		private void repository_DiagramUpdated(object sender, RepositoryDiagramEventArgs e) {
			OnObjectModified(e.Diagram);
		}


		private void repository_ModelObjectsUpdated(object sender, RepositoryModelObjectsEventArgs e) {
			OnObjectsModified(e.ModelObjects);
		}


		private void repository_ShapesUpdated(object sender, RepositoryShapesEventArgs e) {
			OnObjectsModified(e.Shapes);
		}


		private void Repository_ModelObjectsDeleted(object sender, RepositoryModelObjectsEventArgs e) {
			OnObjectsDeleted(e.ModelObjects);
		}


		private void Repository_ShapesDeleted(object sender, RepositoryShapesEventArgs e) {
			OnObjectsDeleted(e.Shapes);
		}


		private void Repository_DiagramDeleted(object sender, RepositoryDiagramEventArgs e) {
			OnObjectDeleted(e.Diagram);
		}


		private void Repository_StyleDeleted(object sender, RepositoryStyleEventArgs e) {
			OnObjectDeleted(e.Style);
		}

		#endregion


		private void AssertProjectExists() {
			if (Project == null) throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_PropertyProjectIsNotSet);
		}


		private bool IsOfType(Type sourceType, Type targetType) {
			return (sourceType == targetType
				|| sourceType.IsSubclassOf(targetType)
				|| sourceType.GetInterface(targetType.Name, true) != null);
		}


		private void GetSelectedObjectList(int pageIndex, out Hashtable selectedObjectsList) {
			selectedObjectsList = null;
			switch (pageIndex) {
				case 0: selectedObjectsList = _selectedPrimaryObjects; break;
				case 1: selectedObjectsList = _selectedSecondaryObjects; break;
				default: throw new ArgumentOutOfRangeException(nameof(pageIndex));
			}
		}


		private void GetSelectedObjectList(IEnumerable objs, out int pageIndex, out Hashtable selectedObjectsList) {
			pageIndex = -1;
			selectedObjectsList = null;
			foreach (object obj in objs) {
				if (_selectedPrimaryObjects.ContainsKey(obj)) {
					pageIndex = 0;
					selectedObjectsList = _selectedPrimaryObjects;
				} else if (_selectedSecondaryObjects.ContainsKey(obj)) {
					pageIndex = 1;
					selectedObjectsList = _selectedSecondaryObjects;
				}
				if (selectedObjectsList != null) break;
			}
		}


		private void GetSelectedObjectList(object obj, out int pageIndex, out Hashtable selectedObjectsList) {
			pageIndex = -1;
			selectedObjectsList = null;
			if (_selectedPrimaryObjects.ContainsKey(obj)) {
				pageIndex = 0;
				selectedObjectsList = _selectedPrimaryObjects;
			} else if (_selectedSecondaryObjects.ContainsKey(obj)) {
				pageIndex = 1;
				selectedObjectsList = _selectedSecondaryObjects;
			}
		}


		private void OnObjectsModified(IEnumerable objs) {
			if (ObjectsModified != null) {
				int pageIndex;
				Hashtable selectedObjectsList;
				GetSelectedObjectList(objs, out pageIndex, out selectedObjectsList);
				if (pageIndex < 0 || selectedObjectsList.Count == 0)
					return;

				ObjectsModified(this, new PropertyControllerEventArgs(pageIndex, selectedObjectsList.Keys));
			}
		}


		private void OnObjectModified(object obj) {
			if (ObjectsModified != null) {
				int pageIndex;
				Hashtable selectedObjectsList;
				GetSelectedObjectList(obj, out pageIndex, out selectedObjectsList);
				if (pageIndex < 0 || selectedObjectsList.Count == 0)
					return;

				ObjectsModified(this, new PropertyControllerEventArgs(pageIndex, selectedObjectsList.Keys));
			}
		}


		private void OnObjectsDeleted(IEnumerable objs) {
			if (ObjectsSet != null) {
				int pageIndex;
				Hashtable selectedObjectsList;
				GetSelectedObjectList(objs, out pageIndex, out selectedObjectsList);
				if (pageIndex < 0 || selectedObjectsList.Count == 0)
					return;

				// Remove all deleted objects and set the rest as new selection
				foreach (object o in objs) {
					if (selectedObjectsList.Contains(o))
						selectedObjectsList.Remove(o);
				}
				SetObjects(pageIndex, selectedObjectsList);
			}
		}


		private void OnObjectDeleted(object obj) {
			if (this.ObjectsSet != null) {
				int pageIndex;
				Hashtable selectedObjectsList;
				GetSelectedObjectList(obj, out pageIndex, out selectedObjectsList);
				if (pageIndex < 0 || selectedObjectsList.Count == 0)
					return;

				if (selectedObjectsList.Contains(obj)) {
					selectedObjectsList.Remove(obj);

					ObjectsSet(this, new PropertyControllerEventArgs(pageIndex, selectedObjectsList));
				}
			}
		}


		private abstract class PropertySetInfo {

			public PropertySetInfo(object newValue) {
				this._newValue = newValue;
			}

			public abstract IList Objects { get; }


			public List<object> OldValues {
				get { return _oldValues; }
			}


			public object NewValue {
				get { return _newValue; }
			}


			public abstract Type ObjectType { get; }


			public abstract PropertyInfo PropertyInfo { get; }


			private List<object> _oldValues = new List<object>();
			private object _newValue;
		}


		private class PropertySetInfo<TObject> : PropertySetInfo {

			public PropertySetInfo(PropertyInfo propertyInfo, object newValue)
				: base(newValue) {
				this.objects = new List<TObject>();
				this.propertyInfo = propertyInfo;
			}


			/// <override></override>
			public override IList Objects {
				get { return objects; }
			}


			/// <override></override>
			public override Type ObjectType {
				get { return typeof(TObject); }
			}


			/// <override></override>
			public override PropertyInfo PropertyInfo {
				get { return propertyInfo; }
			}


			private List<TObject> objects;
			private PropertyInfo propertyInfo;
		}


		#region Fields
		
		private Project _project;
		private bool _updateRepository;
		private Hashtable _selectedPrimaryObjects = new Hashtable();
		private Hashtable _selectedSecondaryObjects = new Hashtable();
		private PropertySetInfo _propertySetBuffer;
		private NonEditableDisplayMode _propertyDisplayMode = NonEditableDisplayMode.ReadOnly;
		
		#endregion
	}


	/// <summary>
	/// Specifies the display mode for properties that are not editable, in most cases due to insufficient permissions.
	/// </summary>
	public enum NonEditableDisplayMode {
		/// <summary>Equals 'ReadOnly'.</summary>
		Default,
		/// <summary>Non-editable properties will be shown as readonly properties. A reason is added to he property description.</summary>
		ReadOnly,
		/// <summary>Non-editable properties will be hidden.</summary>
		Hidden
	}



	/// <ToBeCompleted></ToBeCompleted>
	public interface IPropertyController {

		#region Events

		/// <summary>Raised after an object was set for editing.</summary>
		[Obsolete("Use SelectedObjectsChanged instead.")]
		event EventHandler<PropertyControllerEventArgs> ObjectsSet;

		/// <summary>Raised before objects are set for editing.</summary>
		event EventHandler<PropertyControllerEventArgs> SelectedObjectsChanging;

		/// <summary>Raised after objects were set for editing.</summary>
		event EventHandler<PropertyControllerEventArgs> SelectedObjectsChanged;


		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<PropertyControllerPropertyChangedEventArgs> PropertyChanged;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler<PropertyControllerEventArgs> ObjectsModified;

		/// <ToBeCompleted></ToBeCompleted>
		event EventHandler ProjectClosing;

		#endregion


		#region Properties

		/// <summary>
		/// Provides access to a <see cref="T:Dataweb.NShape.Project" />.
		/// </summary>
		Project Project { get; }

		/// <summary>
		/// Specifies the display mode for properties that are not editable, in most cases due to insufficient permissions.
		/// </summary>
		NonEditableDisplayMode PropertyDisplayMode { get; }

		#endregion


		#region Methods

		/// <ToBeCompleted></ToBeCompleted>
		void SetPropertyValue(object obj, string propertyName, object oldValue, object newValue);

		/// <ToBeCompleted></ToBeCompleted>
		void CancelSetProperty();

		/// <ToBeCompleted></ToBeCompleted>
		void CommitSetProperty();

		#endregion

	}


	#region EventArgs

	/// <ToBeCompleted></ToBeCompleted>
	public class PropertyControllerEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public PropertyControllerEventArgs(int pageIndex, IEnumerable objects) {
			SetObjects(objects);
			this._pageIndex = pageIndex;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public PropertyControllerEventArgs(int pageIndex, object obj)
		{
			SetObjects(SingleInstanceEnumerator<object>.Create(obj));
			this._pageIndex = pageIndex;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IReadOnlyCollection<object> Objects {
			get { return _objects; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public object[] GetObjectArray() {
			return _objects.ToArray();
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int PageIndex {
			get { return _pageIndex; }
			internal set { _pageIndex = value; }
		}


		/// <summary>
		/// Returns the common Type of all objects in the objects collection.
		/// </summary>
		public Type ObjectsType {
			get { return _commonType ?? typeof(object); }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal PropertyControllerEventArgs() { }


		/// <ToBeCompleted></ToBeCompleted>
		protected internal void SetObjects(IEnumerable objects) {
			this._objects.Clear();
			foreach (object obj in objects) {
				if (obj != null)
					_objects.Add(obj);
			}
			SetCommonType();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal void SetObjects(IEnumerable<Shape> objects) {
			this._objects.Clear();
			foreach (Shape s in objects)
				this._objects.Add(s);
			_commonType = typeof(Shape);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal void SetObjects(IEnumerable<IModelObject> objects) {
			this._objects.Clear();
			foreach (IModelObject m in objects)
				this._objects.Add(m);
			_commonType = typeof(IModelObject);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal void SetObject(object obj) {
			this._objects.Clear();
			if (obj != null) this._objects.Add(obj);
			SetCommonType();
		}


		private void SetCommonType() {
			// get Type of modifiedObjects
			_commonType = null;
			foreach (Object obj in _objects) {
				if (obj == null) continue;
				if (_commonType == null) {
					if (obj is Shape) 
						_commonType = typeof(Shape);
					else if (obj is IModelObject) 
						_commonType = typeof(IModelObject);
					else _commonType = obj.GetType();
				} else if (!obj.GetType().IsSubclassOf(_commonType)
							&& obj.GetType().GetInterface(_commonType.Name) == null) {
					_commonType = null;
					break;
				}
			}
		}


		private ReadOnlyList<object> _objects = new ReadOnlyList<object>();
		private int _pageIndex = -1;
		private Type _commonType = null;
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class PropertyControllerPropertyChangedEventArgs : PropertyControllerEventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public PropertyControllerPropertyChangedEventArgs(int pageIndex, IEnumerable modifiedObjects, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(pageIndex, modifiedObjects) {
			if (modifiedObjects == null) throw new ArgumentNullException(nameof(modifiedObjects));
			if (propertyInfo == null) throw new ArgumentNullException(nameof(propertyInfo));
			// store modifiedObjects
			this._oldValues = new List<object>(oldValues);
			this._newValue = newValue;
			this._propertyInfo = propertyInfo;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public IEnumerable<object> OldValues { get { return _oldValues; } }

		/// <ToBeCompleted></ToBeCompleted>
		public object NewValue { get { return _newValue; } }

		/// <ToBeCompleted></ToBeCompleted>
		public PropertyInfo PropertyInfo { get { return _propertyInfo; } }

		private List<object> _oldValues;
		private object _newValue;
		private PropertyInfo _propertyInfo;
	}

	#endregion


	/// <ToBeCompleted></ToBeCompleted>
	public delegate void SelectedObjectsChangedCallback(PropertyControllerPropertyChangedEventArgs propertyChangedEventArgs);

}

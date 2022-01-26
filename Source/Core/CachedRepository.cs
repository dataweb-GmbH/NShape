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
using System.Runtime.Serialization;
using System.Text;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Caches modifications to the cache and commits them to the data store
	/// during SaveChanges.
	/// </summary>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(CachedRepository), "CachedRepository.bmp")]
	public class CachedRepository : Component, IRepository, IStoreCache {

		/// <summary>
		/// The <see cref="T:Dataweb.NShape.Advanced.Store" /> containing the persistent data for this <see cref="T:Dataweb.NShape.Advanced.CachedRepository" />
		/// </summary>
		public Store Store {
			get { return _store; }
			set {
				// Assigning a store when no store exists is ok but not exchanging the stores when 
				// modifications are pending because the state of store implementations that write 
				// only changed entities would become inconsistent.
				if (_store != null) {
					if (IsOpen && IsModified) throw new NShapeException(ResourceStrings.MessageTxt_UnsavedRepositoryModificationsPending);
					_store.ProjectName = string.Empty;
				}
				_store = value;
				if (_store != null) _store.ProjectName = _projectName;
			}
		}

		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


#if DEBUG_DIAGNOSTICS
		/// <summary>
		/// Finds the owner of the given shape. For debugging purposes only!
		/// </summary>
		public IEntity FindOwner(Shape shape) {
			foreach (Diagram d in GetCachedEntities<Diagram>(_loadedDiagrams, _newDiagrams))
				if (d.Shapes.Contains(shape)) return d;
			foreach (Template t in GetCachedEntities<Template>(_loadedTemplates, _newTemplates))
				if (t.Shape == shape) return t;
			foreach (Shape s in GetCachedEntities<Shape>(_loadedShapes, _newShapes))
				if (s.Children.Contains(shape)) return s;
			return null;
		}
#endif


		#region IRepository Members

		/// <override></override>
		public int Version {
			get { return _version; }
			set {
				// ToDo: Check on first/last supported save/load version
				_version = value;
				if (_store != null) _store.Version = _version;
			}
		}


		/// <override></override>
		public bool CanModifyVersion {
			get { return (_store != null) ? _store.CanModifyVersion : false; }
		}


		/// <override></override>
		public string ProjectName {
			get { return _projectName; }
			set {
				_projectName = value;
				if (_store != null) _store.ProjectName = _projectName;
			}
		}


		/// <override></override>
		public bool IsModified {
			get { return _isModified; }
		}


		/// <override></override>
		public void AddEntityType(IEntityType entityType) {
			if (entityType == null) throw new ArgumentNullException("entityType");
			if (_entityTypes.ContainsKey(CalcElementName(entityType.FullName)))
				throw new NShapeException(ResourceStrings.MessageFmt_RepositoryAlreadyContainsAnEntityType0, entityType.FullName);
			foreach (KeyValuePair<string, IEntityType> item in _entityTypes) {
				if (item.Value.FullName.Equals(entityType.FullName, StringComparison.InvariantCultureIgnoreCase))
					throw new NShapeException(ResourceStrings.MessageFmt_RepositoryAlreadyContainsAnEntityType0, entityType.FullName);
			}
			// Calculate the XML element names for all entity identifiers
			entityType.ElementName = CalcElementName(entityType.FullName);
			foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions) {
				pi.ElementName = CalcElementName(pi.Name);
				if (pi is EntityInnerObjectsDefinition) {
					foreach (EntityPropertyDefinition fi in ((EntityInnerObjectsDefinition)pi).PropertyDefinitions)
						fi.ElementName = CalcElementName(fi.Name);
				}
			}
			_entityTypes.Add(entityType.ElementName, entityType);
		}


		/// <override></override>
		public void RemoveEntityType(string entityTypeName) {
			if (entityTypeName == null) throw new ArgumentNullException("entityTypeName");
			if (entityTypeName == string.Empty) throw new ArgumentException(ResourceStrings.MessageTxt_InvalidEntityTypeName);
			_entityTypes.Remove(CalcElementName(entityTypeName));
		}


		/// <override></override>
		public void RemoveAllEntityTypes() {
			_entityTypes.Clear();
		}


		/// <override></override>
		public void ReadVersion() {
			if (_store == null) throw new Exception(ResourceStrings.MessageTxt_PropertyStoreNotSet);
			// Do not test whether the store exists because the store itself will throw a more 
			// specific exception when opening the data source fails.
			//if (!store.Exists()) throw new NShapeException(ResourceStrings.MessageTxt_StoreDoesNotExist);
			_store.ReadVersion(this);
		}


		/// <override></override>
		public bool Exists() {
			return _store != null && _store.Exists();
		}


		/// <override></override>
		public virtual void Create() {
			AssertClosed();
			if (string.IsNullOrEmpty(_projectName)) throw new NShapeException(ResourceStrings.MessageTxt_ProjectNameNotDefined);
			_settings = new ProjectSettings();
			_newProjects.Add(_settings, _projectOwner);
			_projectDesign = new Design();
			_projectDesign.CreateStandardStyles();
			DoInsertDesign(_projectDesign, _settings, true);
			if (_store != null) {
				_store.Version = _version;
				_store.Create(this);
			}
			_isOpen = true;
		}


		/// <override></override>
		public void Open() {
			AssertClosed();
			if (string.IsNullOrEmpty(_projectName)) throw new NShapeException(ResourceStrings.MessageTxt_ProjectNameNotDefined);
			if (_store == null)
				throw new NShapeException(ResourceStrings.MessageTxt_RepositoryHasNoStoreAttachedOpenNotAllowed);
			_store.ProjectName = _projectName;
			_store.Open(this);
			DoLoadProjectAndDesign();
			_isOpen = true;
			_isModified = false;
		}

		/// <override></override>
		public virtual void Close() {
			if (_store != null) _store.Close(this);
			_isOpen = false;
			ClearBuffers();
			_isModified = false;
		}


		/// <override></override>
		public void Erase() {
			if (_store == null) throw new NShapeException(ResourceStrings.MessageTxt_RepositoryHasNoStoreAttached);
			_store.Erase();
		}


		/// <override></override>
		public void SaveChanges() {
			if (_store == null) throw new NShapeException(ResourceStrings.MessageTxt_RepositoryHasNoStoreAttached);
			_store.SaveChanges(this);
			AcceptAll();
		}


		/// <override></override>
		[Browsable(false)]
		public bool IsOpen {
			get { return _isOpen; }
		}


		/// <override></override>
		public int ObtainNewBottomZOrder(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			return diagram.Shapes.MinZOrder - 10;
		}


		/// <override></override>
		public int ObtainNewTopZOrder(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			return diagram.Shapes.MaxZOrder + 10;
		}


		/// <override></override>
		public bool IsShapeTypeInUse(ShapeType shapeType) {
			return IsShapeTypeInUse(SingleInstanceEnumerator<ShapeType>.Create(shapeType));
		}


		/// <override></override>
		public bool IsShapeTypeInUse(IEnumerable<ShapeType> shapeTypes) {
			if (shapeTypes == null) throw new ArgumentNullException("shapeTypes");
			AssertOpen();
			// Store shape types in a hash collection for better performance when searching huge amounts of shapes
			HashCollection<ShapeType> shapeTypeSet = new HashCollection<ShapeType>();
			foreach (ShapeType shapeType in shapeTypes)
				shapeTypeSet.Add(shapeType);
			if (shapeTypeSet.Count == 0) return false;

			// Check all new shapes if one of the shape types is used
			foreach (KeyValuePair<Shape, IEntity> keyValuePair in _newShapes) {
				// Skip template shapes as they are checked by the Project's RemoveLibrary method.
				if (keyValuePair.Value is Template) continue;
				if (shapeTypeSet.Contains(keyValuePair.Key.Type))
					return true;
			}
			// Check all loaded shapes if one of the shape types is used
			foreach (EntityBucket<Shape> entityBucket in _loadedShapes) {
				// Skip deleted shapes and template shapes as they are checked by the Project's RemoveLibrary method.
				if (entityBucket.State == ItemState.Deleted) continue;
				if (entityBucket.ObjectRef.Template == null) continue;
				if (shapeTypeSet.Contains(entityBucket.ObjectRef.Type))
					return true;
			}
			// Check all diagrams that are not loaded at the moment
			foreach (ShapeType shapeType in shapeTypeSet) {
				if (Store.CheckShapeTypeInUse(this, shapeType.FullName))
					return true;
			}
			return false;
		}


		/// <override></override>
		public bool IsModelObjectTypeInUse(ModelObjectType modelObjectType) {
			return IsModelObjectTypeInUse(SingleInstanceEnumerator<ModelObjectType>.Create(modelObjectType));
		}


		/// <override></override>
		public bool IsModelObjectTypeInUse(IEnumerable<ModelObjectType> modelObjectTypes) {
			if (modelObjectTypes == null) throw new ArgumentNullException("modelObjectTypes");
			AssertOpen();
			// Store model object types in a hash list for better performance when searching
			HashCollection<ModelObjectType> modelObjTypeSet = new HashCollection<ModelObjectType>();
			foreach (ModelObjectType modelObjectType in modelObjectTypes)
				modelObjTypeSet.Add(modelObjectType);
			if (modelObjTypeSet.Count == 0) return false;

			// Check all new model objects if one of the model object types is used
			foreach (KeyValuePair<IModelObject, IEntity> keyValuePair in _newTemplateModelObjects) {
				if (modelObjTypeSet.Contains(keyValuePair.Key.Type))
					return true;
			}
			// Check all loaded model objects if one of the model object types is used
			foreach (EntityBucket<IModelObject> entityBucket in _loadedTemplateModelObjects) {
				if (entityBucket.State == ItemState.Deleted) continue;
				if (modelObjTypeSet.Contains(entityBucket.ObjectRef.Type))
					return true;
			}
			// Check all new model objects if one of the model object types is used
			foreach (KeyValuePair<IModelObject, IEntity> keyValuePair in _newModelObjects) {
				if (modelObjTypeSet.Contains(keyValuePair.Key.Type))
					return true;
			}
			// Check all loaded model objects if one of the model object types is used
			foreach (EntityBucket<IModelObject> entityBucket in _loadedModelObjects) {
				if (entityBucket.State == ItemState.Deleted) continue;
				if (modelObjTypeSet.Contains(entityBucket.ObjectRef.Type))
					return true;
			}
			// A check on the database is not necessary here because models are always loaded completely (by design).
			return false;
		}


		/// <override></override>
		public bool IsDiagramModelObjectTypeInUse(DiagramModelObjectType diagramModelObjectType) {
			return IsDiagramModelObjectTypeInUse(SingleInstanceEnumerator<DiagramModelObjectType>.Create(diagramModelObjectType));
		}


		/// <override></override>
		public bool IsDiagramModelObjectTypeInUse(IEnumerable<DiagramModelObjectType> diagramModelObjectTypes) {
			if (diagramModelObjectTypes == null) throw new ArgumentNullException("diagramModelObjectTypes");
			AssertOpen();
			// Store model object types in a hash list for better performance when searching
			HashCollection<DiagramModelObjectType> diagramModelObjTypeSet = new HashCollection<DiagramModelObjectType>();
			foreach (DiagramModelObjectType diagramModelObjectType in diagramModelObjectTypes)
				diagramModelObjTypeSet.Add(diagramModelObjectType);
			if (diagramModelObjTypeSet.Count == 0) return false;

			// Check all new model objects if one of the model object types is used
			foreach (KeyValuePair<IDiagramModelObject, IEntity> keyValuePair in _newDiagramModelObjects) {
				if (diagramModelObjTypeSet.Contains(keyValuePair.Key.Type))
					return true;
			}
			// Check all loaded model objects if one of the model object types is used
			foreach (EntityBucket<IDiagramModelObject> entityBucket in _loadedDiagramModelObjects) {
				if (entityBucket.State == ItemState.Deleted) continue;
				if (diagramModelObjTypeSet.Contains(entityBucket.ObjectRef.Type))
					return true;
			}
			// A check on the database is not necessary here because models are always loaded completely (by design).
			return false;
		}


		#region Project

		/// <override></override>
		public ProjectSettings GetProject() {
			AssertOpen();
			return this._settings;
		}


		/// <override></override>
		public void Update() {
			AssertOpen();
			UpdateEntity<ProjectSettings>(_loadedProjects, _newProjects, _settings);
			if (ProjectUpdated != null) ProjectUpdated(this, GetProjectEventArgs(_settings));
		}


		/// <override></override>
		public void Delete() {
			AssertOpen();
			DeleteEntity<ProjectSettings>(_loadedProjects, _newProjects, _settings);
			if (ProjectDeleted != null) ProjectDeleted(this, GetProjectEventArgs(_settings));
		}


		/// <override></override>
		public event EventHandler<RepositoryProjectEventArgs> ProjectUpdated;

		/// <override></override>
		public event EventHandler<RepositoryProjectEventArgs> ProjectDeleted;

		#endregion


		#region Designs

		private void EnsureDesignsAreLoaded() {
			// Load instances from store if...
			// - there is a store,
			// - there are no loaded objects at all and 
			// - if the current project has an id (otherwise the project was not saved yet and there is nothing to load).
			if (_store != null && IsProjectLoaded && _loadedDesigns.Count <= 0)
				_store.LoadDesigns(this, null);
		}


		/// <override></override>
		public IEnumerable<Design> GetDesigns() {
			AssertOpen();
			EnsureDesignsAreLoaded();
			return GetCachedEntities(_loadedDesigns, _newDesigns);
		}


		/// <override></override>
		public Design GetDesign(object id) {
			Design result = null;
			AssertOpen();
			if (id == null) {
				// Return the project design
				result = _projectDesign;
			} else {
				EnsureDesignsAreLoaded();
				EntityBucket<Design> designBucket;
				if (!_loadedDesigns.TryGetValue(id, out designBucket))
					throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1NotFoundInRepository, ResourceStrings.Text_Design, id);
				if (designBucket.State == ItemState.Deleted)
					throw new NShapeException(ResourceStrings.MessageFmt_Entity01WasDeleted, ResourceStrings.Text_Design, designBucket.ObjectRef);
				result = designBucket.ObjectRef;
			}
			return result;
		}


		/// <override></override>
		public Design GetDesign(string name) {
			AssertOpen();
			if (string.IsNullOrEmpty(name))
				return _projectDesign;
			else {
				EnsureDesignsAreLoaded();
				foreach (Design d in GetCachedEntities<Design>(_loadedDesigns, _newDesigns))
					if (d.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
						return d;
				throw new ArgumentException(string.Format(ResourceStrings.MessageFmt_Entity01DoesNotExistInTheRepository, name));
			}
		}


		/// <override></override>
		public void Insert(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			AssertOpen();
			DoInsertDesign(design, _settings, false);
			if (DesignInserted != null) DesignInserted(this, GetDesignEventArgs(design));
		}


		/// <override></override>
		public void InsertAll(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			AssertOpen();
			DoInsertDesign(design, _settings, true);
			if (DesignInserted != null) DesignInserted(this, GetDesignEventArgs(design));
		}


		/// <override></override>
		public void Update(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			AssertOpen();
			AssertCanUpdate(design);
			UpdateEntity<Design>(_loadedDesigns, _newDesigns, design);
			if (DesignUpdated != null) DesignUpdated(this, GetDesignEventArgs(design));
		}


		/// <override></override>
		public void Delete(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			if (design == _projectDesign)
				throw new InvalidOperationException(ResourceStrings.MessageTxt_CurrentProjectDesignCannotBeDeleted);
			AssertOpen();
			DoDeleteDesign(design, false);
			if (DesignDeleted != null) DesignDeleted(this, GetDesignEventArgs(design));
		}


		/// <override></override>
		public void DeleteAll(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			if (design == _projectDesign)
				throw new InvalidOperationException(ResourceStrings.MessageTxt_CurrentProjectDesignCannotBeDeleted);
			AssertOpen();
			DoDeleteDesign(design, true);
			if (DesignDeleted != null) DesignDeleted(this, GetDesignEventArgs(design));
		}


		/// <override></override>
		public void Undelete(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			AssertOpen();
			DoUndeleteDesign(design, false);
			if (DesignInserted != null) DesignInserted(this, GetDesignEventArgs(design));
		}


		/// <override></override>
		public void UndeleteAll(Design design) {
			if (design == null) throw new ArgumentNullException("design");
			AssertOpen();
			DoUndeleteDesign(design, true);
			if (DesignInserted != null) DesignInserted(this, GetDesignEventArgs(design));
		}


		/// <override></override>
		public event EventHandler<RepositoryDesignEventArgs> DesignInserted;

		/// <override></override>
		public event EventHandler<RepositoryDesignEventArgs> DesignUpdated;

		/// <override></override>
		public event EventHandler<RepositoryDesignEventArgs> DesignDeleted;

		#endregion


		#region Styles

		/// <override></override>
		public bool IsStyleInUse(IStyle style) {
			return IsStyleInUse(style, EmptyEnumerator<IStyle>.Empty);
		}


		/// <override></override>
		public void Insert(Design design, IStyle style) {
			if (design == null) throw new ArgumentNullException("design");
			if (style == null) throw new ArgumentNullException("style");
			AssertOpen();
			AssertCanInsert(SingleInstanceEnumerator<IStyle>.Create(style));
			DoInsertStyle(design, style);
			if (StyleInserted != null) StyleInserted(this, GetStyleEventArgs(style));
		}


		/// <override></override>
		public void Update(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertOpen();
			AssertCanUpdate(style);
			DoUpdateStyle(style);
			if (StyleUpdated != null) StyleUpdated(this, GetStyleEventArgs(style));
		}


		/// <override></override>
		public void Delete(IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertOpen();
			AssertCanDelete(SingleInstanceEnumerator<IStyle>.Create(style));
			DeleteEntity<IStyle>(_loadedStyles, _newStyles, style);
			if (StyleDeleted != null) StyleDeleted(this, GetStyleEventArgs(style));
		}


		/// <override></override>
		public void Undelete(Design design, IStyle style) {
			if (style == null) throw new ArgumentNullException("style");
			AssertOpen();
			AssertCanUndelete(SingleInstanceEnumerator<IStyle>.Create(style));
			DoUndeleteStyle(design, style);
			if (StyleInserted != null) StyleInserted(this, GetStyleEventArgs(style));
		}


		/// <override></override>
		public event EventHandler<RepositoryStyleEventArgs> StyleInserted;

		/// <override></override>
		public event EventHandler<RepositoryStyleEventArgs> StyleUpdated;

		/// <override></override>
		public event EventHandler<RepositoryStyleEventArgs> StyleDeleted;

		#endregion


		#region  Model

		/// <override></override>
		public Model GetModel() {
			Model model = null;
			if (!TryGetModel(out model))
				throw new NShapeException(ResourceStrings.MessageTxt_AModelDoesNotExistInTheRepository);
			return model;
		}


		/// <override></override>
		public void Insert(Model model) {
			if (model == null) throw new ArgumentNullException("model");
			AssertOpen();
			AssertCanInsert(model);
			Model m = null;
			if (TryGetModel(out m))
				throw new NShapeException(ResourceStrings.MessageTxt_AModelAleadyExistsInTheRepository);
			InsertEntity<Model>(_newModels, model, GetProject());
			if (ModelInserted != null) ModelInserted(this, GetModelEventArgs(model));
		}


		/// <override></override>
		public void Update(Model model) {
			AssertOpen();
			AssertCanUpdate(model);
			UpdateEntity<Model>(_loadedModels, _newModels, model);
			if (ModelUpdated != null) ModelUpdated(this, GetModelEventArgs(model));
		}


		/// <override></override>
		public void Delete(Model model) {
			AssertOpen();
			AssertCanDelete(model);
			DeleteEntity<Model>(_loadedModels, _newModels, model);
			if (ModelDeleted != null) ModelDeleted(this, GetModelEventArgs(model));
		}


		/// <override></override>
		public void Undelete(Model model) {
			AssertOpen();
			AssertCanUndelete(model);
			UndeleteEntity<Model>(_loadedModels, model);
			if (ModelInserted != null) ModelInserted(this, GetModelEventArgs(model));
		}


		/// <override></override>
		public event EventHandler<RepositoryModelEventArgs> ModelInserted;

		/// <override></override>
		public event EventHandler<RepositoryModelEventArgs> ModelUpdated;

		/// <override></override>
		public event EventHandler<RepositoryModelEventArgs> ModelDeleted;

		#endregion


		#region ModelObjects

		private void EnsureModelObjectsAreLoaded(Model model) {
			// Load instances from store if...
			// - there is a store,
			// - there are no loaded objects at all and 
			// - if the current project has an id (otherwise the project was not saved yet and there is nothing to load).

			// TemplateModelObjects are stored in LoadedModelObjects, too.Due to this fact, we cannot simply test on _loadedModelObjects.Count <= 0...
			bool hasModelModelObjects = false;
			foreach (EntityBucket<IModelObject> mob in _loadedModelObjects) {
				if (mob.Owner is Template || mob.State != ItemState.Deleted) continue;
				hasModelModelObjects = true;
				break;
			}
			//
			if (_store != null && model != null && !hasModelModelObjects && model.Id != null) {
				// Load parent model objects
				_store.LoadModelModelObjects(this, model.Id);
				// Load child model objects
				// Caution: This recursive implementation is slow! Store implementation should load all model objects at once and do nothing in LoadChildModelObject!
				foreach (EntityBucket<IModelObject> mob in _loadedModelObjects)
					EnsureChildModelObjectsAreLoaded(mob.ObjectRef.Id);
			}
		}


		private void EnsureChildModelObjectsAreLoaded(object parentModelObjectId) {
			// Load instances from store if...
			// - there is a store,
			// - there are no loaded objects at all and 
			// - if the parent object has an id (otherwise the parent was not saved yet and there is nothing to load).
			if (_store != null && parentModelObjectId != null) {
				_store.LoadChildModelObjects(this, parentModelObjectId);

				// Load child model objects
				// Caution: This recursive implementation is slow! Store implementation should load all model objects at once and do nothing in LoadChildModelObject!
				EntityBucket<IModelObject> parent = _loadedModelObjects[parentModelObjectId];
				foreach (IModelObject childModelObject in GetModelObjects(parent.ObjectRef))
					EnsureChildModelObjectsAreLoaded(childModelObject.Id);
			}
		}


		/// <summary>
		/// Checks whether the given model object is still referenced by any object.
		/// </summary>
		public bool IsModelObjectInUse(IModelObject modelObject) {
			return IsModelObjectInUse(modelObject, EmptyEnumerator<IModelObject>.Empty);
		}


		/// <summary>
		/// Checks whether the given model object is still referenced by any object.
		/// </summary>
		private bool IsModelObjectInUse(IModelObject modelObject, IEnumerable<IModelObject> modelObjectsToDelete) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			EnsureModelIsLoaded();

			// Check if there are child model objects
			foreach (IModelObject m in GetModelObjects(modelObject)) {
				if (!Contains(modelObjectsToDelete, modelObject))
					return true;
			}
			// Check if model object is used by shapes
			foreach (KeyValuePair<Shape, IEntity> item in _newShapes)
				if (item.Key.ModelObject == modelObject)
					return true;
			foreach (EntityBucket<Shape> item in _loadedShapes) {
				if (item.State == ItemState.Deleted) continue;
				if (item.ObjectRef.ModelObject == modelObject)
					return true;
			}
			// Check if model object is used by objects that are not loaded
			if (modelObject.Id != null)
				return _store.CheckModelObjectInUse(this, modelObject.Id);
			return false;
		}


		// TODO 2: Should be similar to GetShape. Unify?
		/// <override></override>
		public IModelObject GetModelObject(object id) {
			if (id == null) throw new ArgumentNullException("id");
			AssertOpen();
			IModelObject result = null;
			EntityBucket<IModelObject> bucket;
			if (_loadedModelObjects.TryGetValue(id, out bucket))
				result = bucket.ObjectRef;
			else {
				// Load ModelObjects and try again
				Model model = GetModel();
				if (_store != null && model != null) _store.LoadModelModelObjects(this, model.Id);
				if (!_loadedModelObjects.TryGetValue(id, out bucket))
					throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1NotFoundInRepository, ResourceStrings.Text_ModelObject, id);
				if (bucket.State == ItemState.Deleted)
					throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1WasDeleted, ResourceStrings.Text_ModelObject, id);
				result = bucket.ObjectRef;
			}
			return result;
		}

		//public IModelObject GetModelObject(object id) {
		//	if (id == null) throw new ArgumentNullException("id");
		//	AssertOpen();
		//	if (TryGetModel(out Model model))
		//		EnsureModelObjectsAreLoaded(model);

		//	IModelObject result = null;
		//	EntityBucket<IModelObject> bucket;
		//	if (!_loadedModelObjects.TryGetValue(id, out bucket))
		//		throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1NotFoundInRepository, ResourceStrings.Text_ModelObject, id);
		//	if (bucket.State == ItemState.Deleted)
		//		throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1WasDeleted, ResourceStrings.Text_ModelObject, id);
		//	result = bucket.ObjectRef;
		//	return result;
		//}


		/// <override></override>
		public IEnumerable<IModelObject> GetModelObjects(IModelObject parent) {
			AssertOpen();

			//// Original version
			//if (_store != null && IsProjectLoaded) {
			//	if (parent == null) {
			//		if (_loadedModels.Count == 0) {
			//			Model model;
			//			if (TryGetModel(out model))
			//				_store.LoadModelModelObjects(this, model.Id);
			//		}
			//	} else if (parent.Id != null)
			//		_store.LoadChildModelObjects(this, ((IEntity)parent).Id);
			//}

			EnsureModelIsLoaded();

			//// Model is loaded completely!
			////if (parent == null) {
			////	Model model;
			////	if (TryGetModel(out model))
			////		EnsureModelObjectsAreLoaded(model);
			////} else if (parent.Id != null) {
			////	EnsureChildModelObjectsAreLoaded(parent.Id);
			////}

			foreach (EntityBucket<IModelObject> mob in _loadedModelObjects) {
				if (mob.State == ItemState.Deleted) continue;
				if (mob.ObjectRef.Parent == parent) {
					if ((parent != null) || (parent == null && mob.Owner is Model))
						yield return mob.ObjectRef;
				}
			}
			foreach (KeyValuePair<IModelObject, IEntity> item in _newModelObjects) {
				if (item.Key.Parent == parent) {
					if ((parent != null) || (parent == null && item.Value is Model))
						yield return item.Key;
				}
			}
		}


		/// <override></override>
		public void Insert(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObject);
			DoInsertModelObjects(SingleInstanceEnumerator<IModelObject>.Create(modelObject));
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void Insert(IModelObject modelObject, Template template) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObject);
			DoInsertModelObject(modelObject, template);
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void Insert(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObjects);
			DoInsertModelObjects(modelObjects);
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void UpdateOwner(IModelObject modelObject, IModelObject parent) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			DoUpdateModelObjectOwner(modelObject, parent);
			if (ModelObjectsUpdated != null) ModelObjectsUpdated(this, GetModelObjectsEventArgs(modelObject));
		}


		/// <override></override>
		public void UpdateOwner(IModelObject modelObject, Template template) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			if (template == null) throw new ArgumentNullException("template");
			DoUpdateModelObjectOwner(modelObject, template);
			if (ModelObjectsUpdated != null) ModelObjectsUpdated(this, GetModelObjectsEventArgs(modelObject));
		}


		/// <override></override>
		public void UpdateOwner(IModelObject modelObject, Model model) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			DoUpdateModelObjectOwner(modelObject, model);
			if (ModelObjectsUpdated != null) ModelObjectsUpdated(this, GetModelObjectsEventArgs(modelObject));
		}


		/// <override></override>
		public void Update(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObject);
			DoUpdateModelObject(modelObject);
			if (ModelObjectsUpdated != null) ModelObjectsUpdated(this, e);
		}


		/// <override></override>
		public void Update(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObjects);
			DoUpdateModelObjects(modelObjects);
			if (ModelObjectsUpdated != null) ModelObjectsUpdated(this, e);
		}


		/// <override></override>
		public void Delete(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObject);
			DoDeleteModelObject(modelObject);
			if (ModelObjectsDeleted != null) ModelObjectsDeleted(this, e);
		}


		/// <override></override>
		public void Delete(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObjects);
			DoDeleteModelObjects(modelObjects);
			if (ModelObjectsDeleted != null) ModelObjectsDeleted(this, e);
		}


		/// <override></override>
		public void Undelete(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObject);
			DoUndeleteModelObjects(SingleInstanceEnumerator<IModelObject>.Create(modelObject));
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void Undelete(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			RepositoryModelObjectsEventArgs e = GetModelObjectsEventArgs(modelObjects);
			DoUndeleteModelObjects(modelObjects);
			if (ModelObjectsInserted != null) ModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void Unload(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			AssertOpen();
			foreach (IModelObject mo in modelObjects) {
				// TODO 2: Should we allow to remove from new model objects?
				if (mo.Id == null) _newModelObjects.Remove(mo);
				else _loadedModelObjects.Remove(mo.Id);
			}
		}


		/// <override></override>
		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsInserted;

		/// <override></override>
		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsUpdated;

		/// <override></override>
		public event EventHandler<RepositoryModelObjectsEventArgs> ModelObjectsDeleted;

		#endregion


		#region DiagramModelObjects

		private void EnsureDiagramModelObjectsAreLoaded(Model model) {
			// Load instances from store if...
			// - there is a store,
			// - there are no loaded objects at all and 
			// - if the current project has an id (otherwise the project was not saved yet and there is nothing to load).
			if (_store != null && _loadedDiagramModelObjects.Count <= 0 && model != null && model.Id != null)
				_store.LoadDiagramModelObjects(this, model.Id);
		}


		/// <summary>
		/// Checks whether the given diagram model object is still referenced by any object.
		/// </summary>
		public bool IsDiagramModelObjectInUse(IDiagramModelObject diagramModelObject) {
			if (diagramModelObject == null) throw new ArgumentNullException("diagramModelObject");
			AssertOpen();
			Model model;
			if (TryGetModel(out model))
				EnsureDiagramModelObjectsAreLoaded(model);

			// Check if diagram model object is used by diagrams
			foreach (KeyValuePair<Diagram, IEntity> item in _newDiagrams)
				if (item.Key.ModelObject == diagramModelObject)
					return true;
			foreach (EntityBucket<Diagram> item in _loadedDiagrams) {
				if (item.State == ItemState.Deleted) continue;
				if (item.ObjectRef.ModelObject == diagramModelObject)
					return true;
			}
			// No need to check the database for objects that are not yet loaded because the diagram instances 
			// (along with their model objects) are loaded all at once.
			return false;
		}


		/// <override></override>
		public IDiagramModelObject GetDiagramModelObject(object diagramModelObjectid) {
			if (diagramModelObjectid == null) throw new ArgumentNullException("diagramModelObjectid");
			AssertOpen();
			Model model;
			if (TryGetModel(out model))
				EnsureDiagramModelObjectsAreLoaded(model);

			IDiagramModelObject result = null;
			EntityBucket<IDiagramModelObject> bucket;
			if (!_loadedDiagramModelObjects.TryGetValue(diagramModelObjectid, out bucket))
				throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1NotFoundInRepository, ResourceStrings.Text_DiagramModelObject, diagramModelObjectid);
			if (bucket.State == ItemState.Deleted)
				throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1WasDeleted, ResourceStrings.Text_DiagramModelObject, diagramModelObjectid);
			result = bucket.ObjectRef;
			return result;
		}


		/// <override></override>
		public void Insert(IDiagramModelObject diagramModelObject) {
			if (diagramModelObject == null) throw new ArgumentNullException("diagramModelObject");
			AssertOpen();
			RepositoryDiagramModelObjectsEventArgs e = GetDiagramModelObjectsEventArgs(diagramModelObject);
			DoInsertDiagramModelObjects(SingleInstanceEnumerator<IDiagramModelObject>.Create(diagramModelObject));
			if (DiagramModelObjectsInserted != null) DiagramModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void Insert(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			if (diagramModelObjects == null) throw new ArgumentNullException("diagramModelObjects");
			AssertOpen();
			RepositoryDiagramModelObjectsEventArgs e = GetDiagramModelObjectsEventArgs(diagramModelObjects);
			DoInsertDiagramModelObjects(diagramModelObjects);
			if (DiagramModelObjectsInserted != null) DiagramModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void Update(IDiagramModelObject diagramModelObject) {
			if (diagramModelObject == null) throw new ArgumentNullException("diagramModelObject");
			AssertOpen();
			RepositoryDiagramModelObjectsEventArgs e = GetDiagramModelObjectsEventArgs(diagramModelObject);
			DoUpdateDiagramModelObject(diagramModelObject);
			if (DiagramModelObjectsUpdated != null) DiagramModelObjectsUpdated(this, e);
		}


		/// <override></override>
		public void Update(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			if (diagramModelObjects == null) throw new ArgumentNullException("diagramModelObjects");
			AssertOpen();
			RepositoryDiagramModelObjectsEventArgs e = GetDiagramModelObjectsEventArgs(diagramModelObjects);
			DoUpdateDiagramModelObjects(diagramModelObjects);
			if (DiagramModelObjectsUpdated != null) DiagramModelObjectsUpdated(this, e);
		}


		/// <override></override>
		public void Delete(IDiagramModelObject diagramModelObject) {
			if (diagramModelObject == null) throw new ArgumentNullException("diagramModelObject");
			AssertOpen();
			RepositoryDiagramModelObjectsEventArgs e = GetDiagramModelObjectsEventArgs(diagramModelObject);
			DoDeleteDiagramModelObject(diagramModelObject);
			if (DiagramModelObjectsDeleted != null) DiagramModelObjectsDeleted(this, e);
		}


		/// <override></override>
		public void Delete(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			if (diagramModelObjects == null) throw new ArgumentNullException("diagramModelObjects");
			AssertOpen();
			RepositoryDiagramModelObjectsEventArgs e = GetDiagramModelObjectsEventArgs(diagramModelObjects);
			DoDeleteDiagramModelObjects(diagramModelObjects);
			if (DiagramModelObjectsDeleted != null) DiagramModelObjectsDeleted(this, e);
		}


		/// <override></override>
		public void Undelete(IDiagramModelObject diagramModelObject) {
			if (diagramModelObject == null) throw new ArgumentNullException("diagramModelObject");
			AssertOpen();
			RepositoryDiagramModelObjectsEventArgs e = GetDiagramModelObjectsEventArgs(diagramModelObject);
			DoUndeleteDiagramModelObjects(SingleInstanceEnumerator<IDiagramModelObject>.Create(diagramModelObject));
			if (DiagramModelObjectsInserted != null) DiagramModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void Undelete(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			if (diagramModelObjects == null) throw new ArgumentNullException("diagramModelObjects");
			AssertOpen();
			RepositoryDiagramModelObjectsEventArgs e = GetDiagramModelObjectsEventArgs(diagramModelObjects);
			DoUndeleteDiagramModelObjects(diagramModelObjects);
			if (DiagramModelObjectsInserted != null) DiagramModelObjectsInserted(this, e);
		}


		/// <override></override>
		public void Unload(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			if (diagramModelObjects == null) throw new ArgumentNullException("diagramModelObjects");
			AssertOpen();
			foreach (IDiagramModelObject mo in diagramModelObjects) {
				// TODO 2: Should we allow to remove from new diagram model objects?
				if (mo.Id == null) _newDiagramModelObjects.Remove(mo);
				else _loadedDiagramModelObjects.Remove(mo.Id);
			}
		}


		/// <override></override>
		public event EventHandler<RepositoryDiagramModelObjectsEventArgs> DiagramModelObjectsInserted;

		/// <override></override>
		public event EventHandler<RepositoryDiagramModelObjectsEventArgs> DiagramModelObjectsUpdated;

		/// <override></override>
		public event EventHandler<RepositoryDiagramModelObjectsEventArgs> DiagramModelObjectsDeleted;

		#endregion


		#region Templates

		private void EnsureTemplatesAreLoaded() {
			// Load instances from store if...
			// - there is a store,
			// - there are no loaded objects at all and 
			// - if the current project has an id (otherwise the project was not saved yet and there is nothing to load).
			if (_store != null && IsProjectLoaded && _loadedTemplates.Count <= 0)
				_store.LoadTemplates(this, ProjectId);
		}


		/// <override></override>
		public bool IsTemplateInUse(Template template) {
			return IsTemplateInUse(SingleInstanceEnumerator<Template>.Create(template));
		}


		/// <override></override>
		public bool IsTemplateInUse(IEnumerable<Template> templatesToCheck) {
			if (templatesToCheck == null) throw new ArgumentNullException("templatesToCheck");
			AssertOpen();
			// Store shape types in a hash list for better performance when searching huge amounts of shapes
			HashCollection<Template> templateSet = new HashCollection<Template>();
			foreach (Template template in templatesToCheck)
				templateSet.Add(template);
			if (templateSet.Count == 0) return false;

			// Check all new shapes if one of the shape types is used
			foreach (KeyValuePair<Shape, IEntity> keyValuePair in _newShapes) {
				// Skip template shapes (and their children) as they are checked by the Project's RemoveLibrary method.
				if (keyValuePair.Value is Template) continue;
				if (keyValuePair.Key.Template == null) continue;
				if (templateSet.Contains(keyValuePair.Key.Template))
					return true;
			}
			// Check all loaded shapes if one of the shape types is used
			foreach (EntityBucket<Shape> entityBucket in _loadedShapes) {
				// Skip deleted shapes and template shapes
				if (entityBucket.State == ItemState.Deleted) continue;
				if (entityBucket.ObjectRef.Template == null) continue;
				if (templateSet.Contains(entityBucket.ObjectRef.Template))
					return true;
			}
			// Check all diagrams that are not loaded at the moment
			foreach (Template template in templateSet) {
				if (template.Id == null) continue;
				if (Store.CheckTemplateInUse(this, template.Id))
					return true;
			}
			return false;
		}


		// We assume that this is only called once to load all existing templates.
		/// <override></override>
		public IEnumerable<Template> GetTemplates() {
			AssertOpen();
			EnsureTemplatesAreLoaded();

			return GetCachedEntities(_loadedTemplates, _newTemplates);
		}


		/// <override></override>
		public Template GetTemplate(object id) {
			if (id == null) throw new ArgumentNullException("id");
			AssertOpen();
			EnsureTemplatesAreLoaded();

			EntityBucket<Template> result = null;
			if (!_loadedTemplates.TryGetValue(id, out result))
				throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1NotFoundInRepository, ResourceStrings.Text_Template, id);
			if (result.State == ItemState.Deleted)
				throw new NShapeException(ResourceStrings.MessageFmt_Entity01WasDeleted, ResourceStrings.Text_Template, result.ObjectRef);
			return result.ObjectRef;
		}


		/// <override></override>
		public Template GetTemplate(string name) {
			if (name == null) throw new ArgumentNullException("name");
			AssertOpen();
			EnsureTemplatesAreLoaded();

			foreach (Template t in GetCachedEntities<Template>(_loadedTemplates, _newTemplates))
				if (t.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					return t;
			throw new ArgumentException(string.Format(ResourceStrings.MessageFmt_Entity01DoesNotExistInTheRepository, ResourceStrings.Text_Template, name));
		}


		/// <override></override>
		public void Insert(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			DoInsertTemplate(template, false);
			if (TemplateInserted != null) TemplateInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void InsertAll(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			DoInsertTemplate(template, true);
			if (TemplateInserted != null) TemplateInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void Update(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			UpdateEntity<Template>(_loadedTemplates, _newTemplates, template);
			if (TemplateUpdated != null) TemplateUpdated(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void ReplaceTemplateShape(Template template, Shape oldShape, Shape newShape) {
			if (template == null) throw new ArgumentNullException("template");
			if (oldShape == null) throw new ArgumentNullException("oldShape");
			if (newShape == null) throw new ArgumentNullException("newShape");
			AssertOpen();
			AssertCanUpdate(template);

			//DoInsertShape(newShape, template);
			UpdateEntity<Template>(_loadedTemplates, _newTemplates, template);
			// Insert/Undelete new shape
			DoUpdateTemplateModelObject(template);
			DoUpdateTemplateShape(template, true);
			// Delete old shape
			if (oldShape.ModelObject != null) oldShape.ModelObject.DetachShape(oldShape);
			DoDeleteShapes(SingleInstanceEnumerator<Shape>.Create(oldShape), true);

			if (TemplateShapeReplaced != null) TemplateShapeReplaced(this, new RepositoryTemplateShapeReplacedEventArgs(template, oldShape, newShape));
		}


		/// <override></override>
		public void Delete(Template template) {
			AssertOpen();
			DoDeleteTemplate(template, false);
			if (TemplateDeleted != null) TemplateDeleted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void DeleteAll(Template template) {
			AssertOpen();
			DoDeleteTemplate(template, true);
			if (TemplateDeleted != null) TemplateDeleted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void Undelete(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			DoUndeleteTemplate(template, false);
			if (TemplateInserted != null) TemplateInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void UndeleteAll(Template template) {
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			DoUndeleteTemplate(template, true);
			if (TemplateInserted != null) TemplateInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> TemplateInserted;

		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> TemplateUpdated;

		/// <override></override>
		public event EventHandler<RepositoryTemplateShapeReplacedEventArgs> TemplateShapeReplaced;

		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> TemplateDeleted;

		#endregion


		#region ModelMappings

		/// <override></override>
		public void Insert(IModelMapping modelMapping, Template template) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			DoInsertModelMappings(SingleInstanceEnumerator<IModelMapping>.Create(modelMapping), template);
			if (ModelMappingsInserted != null) ModelMappingsInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void Insert(IEnumerable<IModelMapping> modelMappings, Template template) {
			if (modelMappings == null) throw new ArgumentNullException("modelMappings");
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			DoInsertModelMappings(modelMappings, template);
			if (ModelMappingsInserted != null) ModelMappingsInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void Update(IModelMapping modelMapping) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			AssertOpen();
			DoUpdateModelMappings(SingleInstanceEnumerator<IModelMapping>.Create(modelMapping));
			if (ModelMappingsUpdated != null) ModelMappingsUpdated(this,
				GetTemplateEventArgs(GetModelMappingOwner(modelMapping)));
		}


		/// <override></override>
		public void Update(IEnumerable<IModelMapping> modelMappings) {
			if (modelMappings == null) throw new ArgumentNullException("modelMapping");
			AssertOpen();
			DoUpdateModelMappings(modelMappings);
			if (ModelMappingsUpdated != null) {
				foreach (IModelMapping modelMapping in modelMappings) {
					ModelMappingsUpdated(this, GetTemplateEventArgs(GetModelMappingOwner(modelMapping)));
					break;
				}
			}
		}


		/// <override></override>
		public void Delete(IModelMapping modelMapping) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			AssertOpen();
			DoDeleteModelMappings(SingleInstanceEnumerator<IModelMapping>.Create(modelMapping));
			if (ModelMappingsDeleted != null) {
				Template owner = GetModelMappingOwner(modelMapping);
				ModelMappingsDeleted(this, GetTemplateEventArgs(owner));
			}
		}


		/// <override></override>
		public void Delete(IEnumerable<IModelMapping> modelMappings) {
			if (modelMappings == null) throw new ArgumentNullException("modelMapping");
			AssertOpen();
			DoDeleteModelMappings(modelMappings);
			if (ModelMappingsDeleted != null) {
				foreach (IModelMapping modelMapping in modelMappings) {
					ModelMappingsDeleted(this, GetTemplateEventArgs(GetModelMappingOwner(modelMapping)));
					break;
				}
			}
		}


		/// <override></override>
		public void Undelete(IModelMapping modelMapping, Template template) {
			if (modelMapping == null) throw new ArgumentNullException("modelMapping");
			AssertOpen();
			DoUndeleteModelMappings(SingleInstanceEnumerator<IModelMapping>.Create(modelMapping), template);
			if (ModelMappingsInserted != null) ModelMappingsInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public void Undelete(IEnumerable<IModelMapping> modelMappings, Template template) {
			if (modelMappings == null) throw new ArgumentNullException("modelMapping");
			AssertOpen();
			DoUndeleteModelMappings(modelMappings, template);
			if (ModelMappingsInserted != null) ModelMappingsInserted(this, GetTemplateEventArgs(template));
		}


		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> ModelMappingsInserted;

		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> ModelMappingsUpdated;

		/// <override></override>
		public event EventHandler<RepositoryTemplateEventArgs> ModelMappingsDeleted;

		#endregion


		#region Diagrams

		private void EnsureDiagramsAreLoaded() {
			if (_store != null && IsProjectLoaded && _loadedDiagrams.Count <= 0)
				_store.LoadDiagrams(this, ProjectId);
		}


		/// <override></override>
		public IEnumerable<Diagram> GetDiagrams() {
			AssertOpen();
			EnsureDiagramsAreLoaded();

			return GetCachedEntities(_loadedDiagrams, _newDiagrams);
		}


		/// <override></override>
		public Diagram GetDiagram(object id) {
			if (id == null) throw new ArgumentNullException("id");
			AssertOpen();
			EnsureDiagramsAreLoaded();

			EntityBucket<Diagram> result = null;
			if (!_loadedDiagrams.TryGetValue(id, out result))
				throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1NotFoundInRepository, ResourceStrings.Text_Diagram, id);
			if (result.State == ItemState.Deleted)
				throw new NShapeException(ResourceStrings.MessageFmt_Entity01WasDeleted, ResourceStrings.Text_Diagram, result.ObjectRef);
			// Do *NOT* load diagram shapes here. The diagramController is responsible for 
			// loading the diagram shapes. Otherwise partial (per diagram) loading does not work.
			//store.LoadDiagramShapes(this, result.ObjectRef);
			return result.ObjectRef;
		}


		/// <override></override>
		public Diagram GetDiagram(string name) {
			if (name == null) throw new ArgumentNullException("name");
			AssertOpen();
			EnsureDiagramsAreLoaded();

			foreach (Diagram diagram in GetCachedEntities(_loadedDiagrams, _newDiagrams))
				if (string.Compare(diagram.Name, name, Comparison) == 0) {
					// Do *NOT* load diagram shapes here. The diagramController is responsible for 
					// loading the diagram shapes. Otherwise partial (per diagram) loading does not work.
					//store.LoadDiagramShapes(this, d);
					return diagram;
				}
			throw new ArgumentException(string.Format(ResourceStrings.MessageFmt_Entity01DoesNotExistInTheRepository, ResourceStrings.Text_Diagram, name));
		}


		/// <override></override>
		public void Insert(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			DoInsertDiagram(diagram, false);
			if (DiagramInserted != null) DiagramInserted(this, GetDiagramEventArgs(diagram));
		}


		/// <override></override>
		public void InsertAll(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			DoInsertDiagram(diagram, true);
			if (DiagramInserted != null) DiagramInserted(this, GetDiagramEventArgs(diagram));
		}


		/// <override></override>
		public void Update(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			AssertCanUpdate(diagram);
			UpdateEntity<Diagram>(_loadedDiagrams, _newDiagrams, diagram);
			if (DiagramUpdated != null) DiagramUpdated(this, GetDiagramEventArgs(diagram));
		}


		/// <override></override>
		public void Delete(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			DoDeleteDiagram(diagram, false);
			if (DiagramDeleted != null) DiagramDeleted(this, GetDiagramEventArgs(diagram));
		}


		/// <override></override>
		public void DeleteAll(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			DoDeleteDiagram(diagram, true);
			if (DiagramDeleted != null) DiagramDeleted(this, GetDiagramEventArgs(diagram));
		}


		/// <override></override>
		public void Undelete(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			DoUndeleteDiagram(diagram, false);
			if (DiagramInserted != null) DiagramInserted(this, GetDiagramEventArgs(diagram));
		}


		/// <override></override>
		public void UndeleteAll(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			DoUndeleteDiagram(diagram, true);
			if (DiagramInserted != null) DiagramInserted(this, GetDiagramEventArgs(diagram));
		}


		/// <override></override>
		public event EventHandler<RepositoryDiagramEventArgs> DiagramInserted;

		/// <override></override>
		public event EventHandler<RepositoryDiagramEventArgs> DiagramUpdated;

		/// <override></override>
		public event EventHandler<RepositoryDiagramEventArgs> DiagramDeleted;

		#endregion


		#region Shapes

		/// <override></override>
		public void GetDiagramShapes(Diagram diagram, params Rectangle[] rectangles) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (((IEntity)diagram).Id == null) return;
			AssertOpen();
			// For the time being, a diagram is either loaded or not. No partial diagram loading yet.
			if (diagram.Shapes.Count <= 0) {
				// Load missing shapes
				if (_store != null)
					_store.LoadDiagramShapes(this, diagram);
			}
		}


		/// <override></override>
		public void Insert(Shape shape, Diagram diagram) {
			if (shape == null) throw new ArgumentNullException("shape");
			AssertOpen();
			DoInsertShapes(SingleInstanceEnumerator<Shape>.Create(shape), diagram, false);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, diagram));
		}


		/// <override></override>
		public void Insert<TShape>(TShape shape, Diagram diagram) where TShape : Shape {
			Insert<Shape>(shape, diagram);
		}


		/// <override></override>
		public void Insert(Shape shape, Shape parentShape) {
			if (shape == null) throw new ArgumentNullException("shape");
			AssertOpen();
			DoInsertShapes(SingleInstanceEnumerator<Shape>.Create(shape), parentShape, false);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void Insert<TShape>(TShape shape, Shape parentShape) where TShape : Shape {
			Insert<Shape>(shape, parentShape);
		}


		/// <override></override>
		public void Insert(Shape shape, Template template) {
			if (shape == null) throw new ArgumentNullException("shape");
			AssertOpen();
			DoInsertShapes(shape, template, false);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void Insert<TShape>(TShape shape, Template template) where TShape : Shape {
			Insert<Shape>(shape, template);
		}


		/// <override></override>
		public void Insert(IEnumerable<Shape> shapes, Diagram diagram) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			DoInsertShapes(shapes, diagram, false);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shapes, diagram));
		}


		/// <override></override>
		public void Insert(IEnumerable<Shape> shapes, Shape parentShape) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (parentShape == null) throw new ArgumentNullException("parentShape");
			AssertOpen();
			DoInsertShapes(shapes, parentShape, false);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shapes, null));
		}


		/// <override></override>
		public void InsertAll(Shape shape, Diagram diagram) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			DoInsertShapes(SingleInstanceEnumerator<Shape>.Create(shape), diagram, true);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, diagram));
		}


		/// <override></override>
		public void InsertAll<TShape>(TShape shape, Diagram diagram) where TShape : Shape {
			InsertAll<Shape>(shape, diagram);
		}


		/// <override></override>
		public void InsertAll(Shape shape, Shape parentShape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (parentShape == null) throw new ArgumentNullException("parentShape");
			AssertOpen();
			DoInsertShapes(SingleInstanceEnumerator<Shape>.Create(shape), parentShape, true);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void InsertAll<TShape>(TShape shape, Shape parentShape) where TShape : Shape {
			InsertAll<Shape>(shape, parentShape);
		}


		/// <override></override>
		public void InsertAll(Shape shape, Template template) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (template == null) throw new ArgumentNullException("template");
			AssertOpen();
			DoInsertShapes(shape, template, true);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void InsertAll<TShape>(TShape shape, Template template) where TShape : Shape {
			InsertAll<Shape>(shape, template);
		}


		/// <override></override>
		public void InsertAll(IEnumerable<Shape> shapes, Diagram diagram) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			DoInsertShapes(shapes, diagram, true);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shapes, diagram));
		}


		/// <override></override>
		public void InsertAll(IEnumerable<Shape> shapes, Shape parentShape) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (parentShape == null) throw new ArgumentNullException("parentShape");
			AssertOpen();
			DoInsertShapes(shapes, parentShape, true);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shapes, null));
		}


		/// <override></override>
		public void Update(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			AssertOpen();
			DoUpdateShapes(SingleInstanceEnumerator<Shape>.Create(shape));
			if (ShapesUpdated != null) ShapesUpdated(this, GetShapesEventArgs(shape));
		}


		/// <override></override>
		public void Update<TShape>(TShape shape) where TShape : Shape {
			Update<Shape>(shape);
		}


		/// <override></override>
		public void Update(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			DoUpdateShapes(shapes);
			if (ShapesUpdated != null) ShapesUpdated(this, GetShapesEventArgs(shapes));
		}


		/// <override></override>
		public void UpdateOwner(Shape shape, Diagram diagram) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertOpen();
			AssertCanUpdate(SingleInstanceEnumerator<Shape>.Create(shape), diagram);
			DoUpdateShapeOwner(shape, diagram);
			if (ShapesUpdated != null) ShapesUpdated(this, GetShapesEventArgs(shape, diagram));
		}


		/// <override></override>
		public void UpdateOwner(Shape shape, Shape parent) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (parent == null) throw new ArgumentNullException("parent");
			AssertOpen();
			AssertCanUpdate(SingleInstanceEnumerator<Shape>.Create(shape), parent);
			DoUpdateShapeOwner(shape, parent);
			if (ShapesUpdated != null) ShapesUpdated(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void Delete(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			// Get event args before deleting because the owner of new shapes is not accessible
			// after deleting ( == removing) new shapes from the repository.
			AssertOpen();
			AssertCanDelete(SingleInstanceEnumerator<Shape>.Create(shape));
			RepositoryShapesEventArgs e = GetShapesEventArgs(shape);
			DoDeleteShapes(SingleInstanceEnumerator<Shape>.Create(shape), false);
			if (ShapesDeleted != null) ShapesDeleted(this, e);
		}


		/// <override></override>
		public void Delete<TShape>(TShape shape) where TShape : Shape {
			Delete<Shape>(shape);
		}


		/// <override></override>
		public void DeleteAll(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			AssertOpen();
			// Get event args before deleting because the owner of new shapes is not accessible
			// after deleting ( == removing) new shapes from the repository.
			RepositoryShapesEventArgs e = GetShapesEventArgs(shape);
			DoDeleteShapes(SingleInstanceEnumerator<Shape>.Create(shape), true);
			if (ShapesDeleted != null) ShapesDeleted(this, e);
		}


		/// <override></override>
		public void DeleteAll<TShape>(TShape shape) where TShape : Shape {
			DeleteAll<Shape>(shape);
		}


		/// <override></override>
		public void Delete(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			DoDeleteShapes(shapes, false);
			if (ShapesDeleted != null) ShapesDeleted(this, GetShapesEventArgs(shapes));
		}


		/// <override></override>
		public void DeleteAll(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			DoDeleteShapes(shapes, true);
			if (ShapesDeleted != null) ShapesDeleted(this, GetShapesEventArgs(shapes));
		}


		/// <override></override>
		public void Undelete(Shape shape, Diagram diagram) {
			if (shape == null) throw new ArgumentNullException("shape");
			AssertOpen();
			DoUndeleteShapes(shape, diagram, false);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape));
		}


		/// <override></override>
		public void Undelete<TShape>(TShape shape, Diagram diagram) where TShape : Shape {
			Undelete<Shape>(shape, diagram);
		}


		/// <override></override>
		public void UndeleteAll(Shape shape, Diagram diagram) {
			if (shape == null) throw new ArgumentNullException("shape");
			AssertOpen();
			DoUndeleteShapes(shape, diagram, true);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape));
		}


		/// <override></override>
		public void UndeleteAll<TShape>(TShape shape, Diagram diagram) where TShape : Shape {
			UndeleteAll<Shape>(shape, diagram);
		}


		/// <override></override>
		public void Undelete(Shape shape, Shape parent) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (parent == null) throw new ArgumentNullException("parentShape");
			AssertOpen();
			DoUndeleteShapes(shape, parent, false);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void Undelete<TShape>(TShape shape, Shape parent) where TShape : Shape {
			Undelete<Shape>(shape, parent);
		}


		/// <override></override>
		public void UndeleteAll(Shape shape, Shape parent) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (parent == null) throw new ArgumentNullException("parentShape");
			AssertOpen();
			DoUndeleteShapes(shape, parent, true);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void UndeleteAll<TShape>(TShape shape, Shape parent) where TShape : Shape {
			UndeleteAll<Shape>(shape, parent);
		}


		/// <override></override>
		public void Undelete(Shape shape, Template template) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (template == null) throw new ArgumentNullException("parentShape");
			AssertOpen();
			DoUndeleteShapes(shape, template, false);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void Undelete<TShape>(TShape shape, Template template) where TShape : Shape {
			Undelete<Shape>(shape, template);
		}


		/// <override></override>
		public void UndeleteAll(Shape shape, Template template) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (template == null) throw new ArgumentNullException("parentShape");
			AssertOpen();
			DoUndeleteShapes(shape, template, true);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shape, null));
		}


		/// <override></override>
		public void UndeleteAll<TShape>(TShape shape, Template template) where TShape : Shape {
			UndeleteAll<Shape>(shape, template);
		}


		/// <override></override>
		public void Undelete(IEnumerable<Shape> shapes, Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			DoUndeleteShapes(shapes, diagram, false);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shapes, diagram));
		}


		/// <override></override>
		public void UndeleteAll(IEnumerable<Shape> shapes, Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			DoUndeleteShapes(shapes, diagram, true);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shapes, diagram));
		}


		/// <override></override>
		public void Undelete(IEnumerable<Shape> shapes, Shape parent) {
			if (parent == null) throw new ArgumentNullException("parent");
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			DoUndeleteShapes(shapes, parent, false);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shapes));
		}


		/// <override></override>
		public void UndeleteAll(IEnumerable<Shape> shapes, Shape parent) {
			if (parent == null) throw new ArgumentNullException("parent");
			if (shapes == null) throw new ArgumentNullException("shapes");
			AssertOpen();
			DoUndeleteShapes(shapes, parent, true);
			if (ShapesInserted != null) ShapesInserted(this, GetShapesEventArgs(shapes));
		}


		/// <override></override>
		public void Unload(Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			DoUnloadShapes(diagram.Shapes);
		}


		/// <override></override>
		public event EventHandler<RepositoryShapesEventArgs> ShapesInserted;

		/// <override></override>
		public event EventHandler<RepositoryShapesEventArgs> ShapesUpdated;

		/// <override></override>
		public event EventHandler<RepositoryShapesEventArgs> ShapesDeleted;

		#endregion


		#region ShapeConnections

		/// <override></override>
		public void InsertConnection(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId) {
			if (activeShape == null) throw new ArgumentNullException("activeShape");
			if (passiveShape == null) throw new ArgumentNullException("passiveShape");
			AssertOpen();
			AssertCanInsert(activeShape, gluePointId, passiveShape, connectionPointId);
			ShapeConnection connection = DoInsertShapeConnection(activeShape, gluePointId, passiveShape, connectionPointId);
			if (ConnectionInserted != null) ConnectionInserted(this, GetShapeConnectionEventArgs(connection));
		}


		/// <override></override>
		public void DeleteConnection(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId) {
			if (activeShape == null) throw new ArgumentNullException("activeShape");
			if (passiveShape == null) throw new ArgumentNullException("passiveShape");
			AssertOpen();
			AssertCanDelete(activeShape, gluePointId, passiveShape, connectionPointId);
			ShapeConnection connection = DoDeleteShapeConnection(activeShape, gluePointId, passiveShape, connectionPointId);
			if (ConnectionDeleted != null) ConnectionDeleted(this, GetShapeConnectionEventArgs(connection));
		}


		/// <override></override>
		public event EventHandler<RepositoryShapeConnectionEventArgs> ConnectionInserted;

		/// <override></override>
		public event EventHandler<RepositoryShapeConnectionEventArgs> ConnectionDeleted;

		#endregion

		#endregion


		#region [Protected] ProjectOwner Class

		/// <summary>
		/// Serves as a parent entity for the project info.
		/// </summary>
		protected class ProjectOwner : IEntity {

			/// <override></override>
			public object Id;


			#region IEntity Members

			object IEntity.Id {
				get { return Id; }
			}

			void IEntity.AssignId(object id) {
				throw new NotImplementedException();
			}

			void IEntity.LoadFields(IRepositoryReader reader, int version) {
				throw new NotImplementedException();
			}

			void IEntity.LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
				throw new NotImplementedException();
			}

			void IEntity.SaveFields(IRepositoryWriter writer, int version) {
				throw new NotImplementedException();
			}

			void IEntity.SaveInnerObjects(string PropertyName, IRepositoryWriter writer, int version) {
				throw new NotImplementedException();
			}

			void IEntity.Delete(IRepositoryWriter writer, int version) {
				throw new NotImplementedException();
			}

			#endregion

		}

		#endregion


		#region [Explicit] IStoreCache Members Implementation

		ProjectSettings IStoreCache.Project {
			get { return _settings; }
		}


		void IStoreCache.SetRepositoryBaseVersion(int version) {
			this._version = version;
		}


		void IStoreCache.SetProjectOwnerId(object id) {
			_projectOwner.Id = id;
		}


		object IStoreCache.ProjectId {
			get { return ProjectId; }
		}


		string IStoreCache.ProjectName {
			get { return _projectName; }
		}


		int IStoreCache.RepositoryBaseVersion {
			get { return _version; }
		}


		Design IStoreCache.ProjectDesign {
			get { return _projectDesign; }
		}


		IEnumerable<IEntityType> IStoreCache.EntityTypes {
			get { return _entityTypes.Values; }
		}


		IEntityType IStoreCache.FindEntityTypeByElementName(string elementName) {
			if (!_entityTypes.ContainsKey(elementName))
				throw new ArgumentException(string.Format(ResourceStrings.MessageFmt_EntityTypeWithElementName0IsNotRegistered, elementName));
			return _entityTypes[elementName];
		}


		IEntityType IStoreCache.FindEntityTypeByName(string name) {
			return FindEntityType(name, true);
		}


		string IStoreCache.CalculateElementName(string entityTypeName) {
			return CachedRepository.CalcElementName(entityTypeName);

		}


		bool IStoreCache.ModelExists() {
			Model model;
			return TryGetModel(out model);
		}


		ICacheCollection<ProjectSettings> IStoreCache.LoadedProjects {
			get { return _loadedProjects; }
		}


		IEnumerable<KeyValuePair<ProjectSettings, IEntity>> IStoreCache.NewProjects {
			get { return _newProjects; }
		}


		ICacheCollection<Model> IStoreCache.LoadedModels {
			get { return _loadedModels; }
		}


		IEnumerable<KeyValuePair<Model, IEntity>> IStoreCache.NewModels {
			get { return _newModels; }
		}


		ICacheCollection<Design> IStoreCache.LoadedDesigns {
			get { return _loadedDesigns; }
		}


		IEnumerable<KeyValuePair<Design, IEntity>> IStoreCache.NewDesigns {
			get { return _newDesigns; }
		}


		ICacheCollection<Diagram> IStoreCache.LoadedDiagrams {
			get { return _loadedDiagrams; }
		}


		IEnumerable<KeyValuePair<Diagram, IEntity>> IStoreCache.NewDiagrams {
			get { return _newDiagrams; }
		}


		ICacheCollection<Shape> IStoreCache.LoadedShapes {
			get { return _loadedShapes; }
		}


		IEnumerable<KeyValuePair<Shape, IEntity>> IStoreCache.NewShapes {
			get { return _newShapes; }
		}


		ICacheCollection<IStyle> IStoreCache.LoadedStyles {
			get { return _loadedStyles; }
		}


		IEnumerable<KeyValuePair<IStyle, IEntity>> IStoreCache.NewStyles {
			get { return _newStyles; }
		}


		ICacheCollection<Template> IStoreCache.LoadedTemplates {
			get { return _loadedTemplates; }
		}


		IEnumerable<KeyValuePair<Template, IEntity>> IStoreCache.NewTemplates {
			get { return _newTemplates; }
		}


		ICacheCollection<IModelMapping> IStoreCache.LoadedModelMappings {
			get { return _loadedModelMappings; }
		}


		IEnumerable<KeyValuePair<IModelMapping, IEntity>> IStoreCache.NewModelMappings {
			get { return _newModelMappings; }
		}


		ICacheCollection<IModelObject> IStoreCache.LoadedModelObjects {
			get { return _loadedModelObjects; }
		}


		IEnumerable<KeyValuePair<IModelObject, IEntity>> IStoreCache.NewModelObjects {
			get { return _newModelObjects; }
		}


		ICacheCollection<IDiagramModelObject> IStoreCache.LoadedDiagramModelObjects {
			get { return _loadedDiagramModelObjects; }
		}


		IEnumerable<KeyValuePair<IDiagramModelObject, IEntity>> IStoreCache.NewDiagramModelObjects {
			get { return _newDiagramModelObjects; }
		}


		IEnumerable<ShapeConnection> IStoreCache.NewShapeConnections {
			get { return _newShapeConnections; }
		}


		IEnumerable<ShapeConnection> IStoreCache.DeletedShapeConnections {
			get { return _deletedShapeConnections; }
		}


		IStyle IStoreCache.GetProjectStyle(object id) {
			return _loadedStyles[id].ObjectRef;
		}


		Template IStoreCache.GetTemplate(object id) {
			return _loadedTemplates[id].ObjectRef;
		}


		Diagram IStoreCache.GetDiagram(object id) {
			return _loadedDiagrams[id].ObjectRef;
		}


		Shape IStoreCache.GetShape(object id) {
			return _loadedShapes[id].ObjectRef;
		}


		IModelObject IStoreCache.GetModelObject(object id) {
			return _loadedModelObjects[id].ObjectRef;
		}


		Design IStoreCache.GetDesign(object id) {
			return _loadedDesigns[id].ObjectRef;
		}

		#endregion


		#region [Private] Implementation

		private void DoLoadProjectAndDesign() {
			// Load the project, must be exactly one.
			_store.LoadProjects(this, FindEntityType(ProjectSettings.EntityTypeName, true));
			IEnumerator<EntityBucket<ProjectSettings>> projectEnumerator = _loadedProjects.Values.GetEnumerator();
			if (!projectEnumerator.MoveNext())
				throw new NShapeException(ResourceStrings.MessageFmt_Entity01DoesNotExistInTheRepository, ResourceStrings.Text_Project, _projectName);
			_settings = projectEnumerator.Current.ObjectRef;
			if (projectEnumerator.MoveNext())
				throw new NShapeException(ResourceStrings.MessageFmt_TwoProjectsNamed0FoundInRepository, _projectName);
			// Load the design, there must be exactly one returned
			_store.LoadDesigns(this, ProjectId);
			IEnumerator<EntityBucket<Design>> designEnumerator = _loadedDesigns.Values.GetEnumerator();
			if (!designEnumerator.MoveNext())
				throw new NShapeException(ResourceStrings.MessageTxt_ProjectStylesNotFound);
			_projectDesign = designEnumerator.Current.ObjectRef;
			if (designEnumerator.MoveNext()) {
				//throw new NShapeException("More than one project design found in repository.");
				// ToDo: Load additional designs
			}
		}


		private void AcceptAll() {
			AcceptEntities(_loadedProjects, _newProjects);
			AcceptEntities(_loadedDesigns, _newDesigns);
			AcceptEntities(_loadedStyles, _newStyles);
			AcceptEntities(_loadedTemplates, _newTemplates);
			AcceptEntities(_loadedModelMappings, _newModelMappings);
			AcceptEntities(_loadedModelObjects, _newModelObjects);
			AcceptEntities(_loadedDiagramModelObjects, _newDiagramModelObjects);
			AcceptEntities(_loadedModels, _newModels);
			AcceptEntities(_loadedDiagrams, _newDiagrams);
			AcceptEntities(_loadedShapes, _newShapes);
			_newShapeConnections.Clear();
			_deletedShapeConnections.Clear();
			_isModified = false;
		}


		private void ClearBuffers() {
			_projectDesign = null;

			ClearNewEntitiesBuffer(_newProjects);
			ClearNewEntitiesBuffer(_newDesigns);
			ClearNewEntitiesBuffer(_newStyles);
			ClearNewEntitiesBuffer(_newDiagrams);
			ClearNewEntitiesBuffer(_newTemplates);
			ClearNewEntitiesBuffer(_newShapes);
			ClearNewEntitiesBuffer(_newModels);
			ClearNewEntitiesBuffer(_newModelObjects);
			ClearNewEntitiesBuffer(_newDiagramModelObjects);
			ClearNewEntitiesBuffer(_newModelMappings);

			ClearLoadedEntitiesBuffer(_loadedProjects);
			ClearLoadedEntitiesBuffer(_loadedStyles);
			ClearLoadedEntitiesBuffer(_loadedDesigns);
			ClearLoadedEntitiesBuffer(_loadedDiagrams);
			ClearLoadedEntitiesBuffer(_loadedTemplates);
			ClearLoadedEntitiesBuffer(_loadedShapes);
			ClearLoadedEntitiesBuffer(_loadedModels);
			ClearLoadedEntitiesBuffer(_loadedModelObjects);
			ClearLoadedEntitiesBuffer(_loadedDiagramModelObjects);
			ClearLoadedEntitiesBuffer(_loadedModelMappings);
		}


		private void ClearNewEntitiesBuffer<TEntity>(Dictionary<TEntity, IEntity> newEntities) where TEntity : IEntity {
			DisposeEntities(newEntities.Keys);
			newEntities.Clear();
		}


		private void ClearLoadedEntitiesBuffer<TEntity>(LoadedEntities<TEntity> loadedEntities) where TEntity : IEntity {
			DisposeEntities(loadedEntities.Values);
			loadedEntities.Clear();
		}


		private void DisposeEntities<TEntity>(IEnumerable<TEntity> entities) where TEntity : IEntity {
			if (Array.IndexOf(typeof(TEntity).GetInterfaces(), typeof(IDisposable)) >= 0) {
				foreach (TEntity entity in entities) {
					IDisposable disposable = entity as IDisposable;
					if (disposable != null) disposable.Dispose();
				}
			}
		}


		private void DisposeEntities<TEntity>(IEnumerable<EntityBucket<TEntity>> entityBuckets) where TEntity : IEntity {
			if (Array.IndexOf(typeof(TEntity).GetInterfaces(), typeof(IDisposable)) >= 0) {
				foreach (EntityBucket<TEntity> entityBucket in entityBuckets) {
					IDisposable disposable = entityBucket.ObjectRef as IDisposable;
					if (disposable != null) disposable.Dispose();
				}
			}
		}


		/// <summary>
		/// Calculates an XML tag name for the given entity name.
		/// </summary>
		private static string CalcElementName(string entityName) {
			string result;
			// We remove the prefixes Core. and GeneralShapes.
			if (entityName.StartsWith("Core.")) result = entityName.Substring(5);
			else if (entityName.StartsWith("GeneralShapes.")) result = entityName.Substring(14);
			else result = entityName;
			// ReplaceRange invalid characters
			result = result.Replace(' ', '_');
			result = result.Replace('/', '_');
			result = result.Replace('<', '_');
			result = result.Replace('>', '_');
			// We replace Camel casing with underscores
			_stringBuilder.Length = 0;
			foreach (char c in result) {
				if (char.IsUpper(c)) {
					// Avoid multiple subsequent underscores
					if (_stringBuilder.Length > 0 && _stringBuilder[_stringBuilder.Length - 1] != '_')
						_stringBuilder.Append('_');
					_stringBuilder.Append(char.ToLowerInvariant(c));
				} else _stringBuilder.Append(c);
			}

			// We use namespace prefixes for the library names
			// Not yet, must use prefix plus name in order to do that
			// result = result.ReplaceRange('.', ':');
			return _stringBuilder.ToString();
		}


		private IEntityType FindEntityType(string entityTypeName, bool mustExist) {
			IEntityType result;
			_entityTypes.TryGetValue(CalcElementName(entityTypeName), out result);
			if (mustExist && result == null)
				throw new NShapeException(ResourceStrings.MessageFmt_EntityType0DoesNotExistInTheRepository, entityTypeName);
			return result;
		}

		#endregion


		#region [Private] Implementation (Generic Methods)

		private IEnumerable<TEntity> GetCachedEntities<TEntity>(LoadedEntities<TEntity> loadedEntities,
			IDictionary<TEntity, IEntity> newEntities) where TEntity : IEntity {
			foreach (EntityBucket<TEntity> eb in loadedEntities) {
				if (eb.State != ItemState.Deleted)
					yield return eb.ObjectRef;
			}
			foreach (KeyValuePair<TEntity, IEntity> item in newEntities)
				yield return item.Key;
		}


		private IEnumerable<TEntity> GetCachedEntities<TEntity>(LoadedEntities<TEntity> loadedEntities,
			IDictionary<TEntity, IEntity> newEntities, IEntity owner) where TEntity : IEntity {
			foreach (EntityBucket<TEntity> eb in loadedEntities) {
				if (eb.Owner == owner && eb.State != ItemState.Deleted)
					yield return eb.ObjectRef;
			}
			foreach (KeyValuePair<TEntity, IEntity> item in newEntities)
				if (item.Value == owner) yield return item.Key;
		}


		/// <summary>
		/// Inserts an entity into the internal cache and marks it as new.
		/// </summary>
		private void InsertEntity<TEntity>(Dictionary<TEntity, IEntity> newEntities,
			TEntity entity, IEntity owner) where TEntity : IEntity {
			if (entity.Id != null)
				throw new ArgumentException(ResourceStrings.MessageTxt_EntitiesWithAnIdCannotBeInsertedIntoTheRepository);
			newEntities.Add(entity, owner);
			_isModified = true;
		}


		/// <summary>
		/// Updates an entity in the internal cache and marks it as modified.
		/// </summary>
		private void UpdateEntity<TEntity>(Dictionary<object, EntityBucket<TEntity>> loadedEntities,
			Dictionary<TEntity, IEntity> newEntities, TEntity entity) where TEntity : IEntity {
			if (entity.Id == null) {
				if (!newEntities.ContainsKey(entity))
					throw new NShapeException(string.Format(ResourceStrings.MessageTxt_EntityNotFoundInRepository));
			} else {
				EntityBucket<TEntity> item;
				if (!loadedEntities.TryGetValue(entity.Id, out item))
					throw new NShapeException(ResourceStrings.MessageTxt_EntityNotFoundInRepository);
				if (item.State == ItemState.Deleted)
					throw new NShapeException(ResourceStrings.MessageTxt_EntityWasDeletedUndeleteBeforeModifying);
				item.State = ItemState.Modified;
			}
			_isModified = true;
		}


		/// <summary>
		/// Marks the entity for deletion from the data store. 
		/// Must be called after all children have been removed.
		/// </summary>
		private void DeleteEntity<TEntity>(Dictionary<object, EntityBucket<TEntity>> loadedEntities,
			Dictionary<TEntity, IEntity> newEntities, TEntity entity) where TEntity : IEntity {
			if (entity.Id == null) {
				if (!newEntities.ContainsKey(entity))
					throw new NShapeException(ResourceStrings.MessageTxt_EntityNotFoundInRepository);
				newEntities.Remove(entity);
			} else {
				EntityBucket<TEntity> item;
				if (!loadedEntities.TryGetValue(entity.Id, out item))
					throw new NShapeException(ResourceStrings.MessageTxt_EntityNotFoundInRepository);
				if (item.State == ItemState.Deleted)
					throw new NShapeException(ResourceStrings.MessageTxt_EntityIsAlreadyDeleted);
				item.State = ItemState.Deleted;
			}
			_isModified = true;
		}


		private void UndeleteEntity<TEntity>(Dictionary<object, EntityBucket<TEntity>> loadedEntities,
			TEntity entity) where TEntity : IEntity {
			if (entity.Id == null)
				throw new NShapeException(ResourceStrings.MessageTxt_AnEntityWithoutIdCannotBeUndeleted);
			else {
				EntityBucket<TEntity> item;
				if (!loadedEntities.TryGetValue(entity.Id, out item))
					throw new NShapeException(ResourceStrings.MessageTxt_EntityNotFoundInRepository);
				if (item.State != ItemState.Deleted)
					throw new NShapeException(ResourceStrings.MessageFmt_EntityWasNotDeletedBefore);
				item.State = ItemState.Modified;
			}
			_isModified = true;
		}


		private void UndeleteEntity<TEntity>(Dictionary<object, EntityBucket<TEntity>> loadedEntities,
			TEntity entity, IEntity owner) where TEntity : IEntity {
			if (entity.Id == null)
				throw new NShapeException(ResourceStrings.MessageTxt_AnEntityWithoutIdCannotBeUndeleted);
			else {
				EntityBucket<TEntity> item;
				if (!loadedEntities.TryGetValue(entity.Id, out item))
					loadedEntities.Add(entity.Id, new EntityBucket<TEntity>(entity, owner, ItemState.New));
				else {
					if (item.State != ItemState.Deleted)
						throw new NShapeException(ResourceStrings.MessageFmt_EntityWasNotDeletedBefore);
					item.State = ItemState.Modified;
					Debug.Assert(item.Owner == owner);
				}
			}
			_isModified = true;
		}


		private void AcceptEntities<EntityType>(Dictionary<object, EntityBucket<EntityType>> loadedEntities,
			Dictionary<EntityType, IEntity> newEntities) where EntityType : IEntity {
			// Remove deleted entities from loaded Entities
			List<object> deletedEntities = new List<object>(100);
			foreach (KeyValuePair<object, EntityBucket<EntityType>> ep in loadedEntities)
				if (ep.Value.State == ItemState.Deleted)
					deletedEntities.Add(ep.Key);
			foreach (object id in deletedEntities)
				loadedEntities.Remove(id);
			deletedEntities.Clear();
			deletedEntities = null;

			// Mark modified entities as original
			foreach (KeyValuePair<object, EntityBucket<EntityType>> item in loadedEntities) {
				if (item.Value.State != ItemState.Original) {
					Debug.Assert(loadedEntities[item.Key].State != ItemState.Deleted);
					loadedEntities[item.Key].State = ItemState.Original;
				}
			}

			// Move new entities from newEntities to loadedEntities
			foreach (KeyValuePair<EntityType, IEntity> ep in newEntities) {
				// Settings do not have a parent
				loadedEntities.Add(ep.Key.Id, new EntityBucket<EntityType>(ep.Key, ep.Value, ItemState.Original));
			}
			newEntities.Clear();
		}


		/// <summary>
		/// Defines a dictionary for loaded entity types.
		/// </summary>
		private class LoadedEntities<TEntity> : Dictionary<object, EntityBucket<TEntity>>,
			ICacheCollection<TEntity> where TEntity : IEntity {

			#region ICacheCollection<TEntity> Members

			public bool Contains(object id) {
				return ContainsKey(id);
			}


			public TEntity GetEntity(object id) {
				return this[id].ObjectRef;
			}


			public void Add(EntityBucket<TEntity> bucket) {
				Add(((IEntity)bucket.ObjectRef).Id, bucket);
			}

			#endregion


			#region IEnumerable<TEntity> Members

			// ToDo 3: Find a way to avoid the new keyword
			public new IEnumerator<EntityBucket<TEntity>> GetEnumerator() {
				foreach (EntityBucket<TEntity> eb in Values)
					yield return eb;
			}

			#endregion

		}


		private bool Contains<T>(IEnumerable<T> collection, T obj) {
			foreach (T item in collection)
				if (item != null && item.Equals(obj)) return true;
			return false;
		}

		#endregion


		#region [Private] Implemenation - Design and Styles

		private void DoInsertDesign(Design design, ProjectSettings projectData, bool withStyles) {
			AssertCanInsert(design);
			if (withStyles) AssertCanInsert(design.Styles);

			InsertEntity<Design>(_newDesigns, design, projectData);
			if (withStyles) {
				foreach (IStyle s in design.Styles)
					DoInsertStyle(design, s);
			}
			_isModified = true;
		}


		private void DoDeleteDesign(Design design, bool withStyles) {
			// First, delete all styles (ColorStyles least)
			if (withStyles) {
				AssertCanDelete(design.Styles);
				foreach (IStyle s in design.CapStyles)
					DoDeleteStyle(s);
				foreach (IStyle s in design.CharacterStyles)
					DoDeleteStyle(s);
				foreach (IStyle s in design.FillStyles)
					DoDeleteStyle(s);
				foreach (IStyle s in design.LineStyles)
					DoDeleteStyle(s);
				foreach (IStyle s in design.ParagraphStyles)
					DoDeleteStyle(s);
				foreach (IStyle s in design.ColorStyles)
					DoDeleteStyle(s);
			}
			AssertCanDelete(design);
			DeleteEntity<Design>(_loadedDesigns, _newDesigns, design);
		}


		private void DoUndeleteDesign(Design design, bool withStyles) {
			AssertCanUndelete(design);
			UndeleteEntity<Design>(_loadedDesigns, design);
			if (withStyles) {
				AssertCanUndelete(design.Styles);
				// Undelete styles (ColorStyles first)
				foreach (IStyle s in design.ColorStyles)
					UndeleteEntity<IStyle>(_loadedStyles, s, design);
				foreach (IStyle s in design.CapStyles)
					UndeleteEntity<IStyle>(_loadedStyles, s, design);
				foreach (IStyle s in design.CharacterStyles)
					UndeleteEntity<IStyle>(_loadedStyles, s, design);
				foreach (IStyle s in design.FillStyles)
					UndeleteEntity<IStyle>(_loadedStyles, s, design);
				foreach (IStyle s in design.LineStyles)
					UndeleteEntity<IStyle>(_loadedStyles, s, design);
				foreach (IStyle s in design.ParagraphStyles)
					UndeleteEntity<IStyle>(_loadedStyles, s, design);
			}
		}


		private void DoInsertStyle(Design design, IStyle style) {
			InsertEntity<IStyle>(_newStyles, style, design);
		}


		private void DoUpdateStyle(IStyle style) {
			UpdateEntity<IStyle>(_loadedStyles, _newStyles, style);
		}


		private void DoDeleteStyle(IStyle style) {
			DeleteEntity<IStyle>(_loadedStyles, _newStyles, style);
		}


		private void DoUndeleteStyle(Design design, IStyle style) {
			UndeleteEntity<IStyle>(_loadedStyles, style, design);
		}


		/// <summary>
		/// Retrieves the indicated project style, which is always loaded when the project is open.
		/// </summary>
		private IStyle GetProjectStyle(object id) {
			EntityBucket<IStyle> styleItem;
			if (!_loadedStyles.TryGetValue(id, out styleItem))
				throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1NotFoundInRepository, ResourceStrings.Text_Style, id);
			if (styleItem.State == ItemState.Deleted)
				throw new NShapeException(ResourceStrings.MessageFmt_Entity01WasDeleted, ResourceStrings.Text_Style, styleItem.ObjectRef);
			return styleItem.ObjectRef;
		}


		private bool UsesStyle(IStyle style, IStyle ownerStyle) {
			Debug.Assert(style != null);
			Debug.Assert(ownerStyle != null);
			if (ownerStyle is ICapStyle && ((ICapStyle)ownerStyle).ColorStyle == style)
				return true;
			else if (ownerStyle is ICharacterStyle && ((ICharacterStyle)ownerStyle).ColorStyle == style)
				return true;
			else if (ownerStyle is IFillStyle) {
				IFillStyle fillStyle = (IFillStyle)ownerStyle;
				if (fillStyle.AdditionalColorStyle == style) return true;
				if (fillStyle.BaseColorStyle == style) return true;
			} else if (ownerStyle is ILineStyle && ((ILineStyle)ownerStyle).ColorStyle == style)
				return true;
			return false;
		}


		private bool UsesStyle(IStyle style, IModelMapping modelMapping) {
			if (modelMapping is StyleModelMapping) {
				StyleModelMapping styleMapping = (StyleModelMapping)modelMapping;
				foreach (object range in styleMapping.ValueRanges) {
					if (range is int) {
						if (styleMapping[(int)range] == style) return true;
					} else if (range is float) {
						if (styleMapping[(float)range] == style) return true;
					}
				}
			}
			return false;
		}

		#endregion


		#region [Private] Implementation - Templates and ModelMappings


		private void DoInsertTemplate(Template template, bool withContent) {
			AssertCanInsert(template, withContent);
			InsertEntity<Template>(_newTemplates, template, GetProject());
			if (withContent) {
				// Insert template model object
				if (template.Shape.ModelObject != null)
					DoInsertModelObject(template.Shape.ModelObject, template);
				// Insert template shape
				DoInsertShapes(template.Shape, template, withContent);
				// Insert model property mappings
				if (template.Shape.ModelObject != null)
					DoInsertModelMappings(template.GetPropertyMappings(), template);
			}
		}


		private void DoUpdateTemplateShape(Template template, bool withContent) {
			// Insert / undelete / update shape
			if (((IEntity)template.Shape).Id == null && !_newShapes.ContainsKey(template.Shape))
				DoInsertShapes(template.Shape, template, withContent);
			else {
				if (CanUndeleteEntity<Shape>(template.Shape, _loadedShapes))
					DoUndeleteShapes(template.Shape, template, withContent);
				DoUpdateTemplateShapeWithContent(template.Shape);
			}
		}


		private void DoUpdateTemplateShapeWithContent(Shape shape) {
			AssertCanUpdate(GetShapes(SingleInstanceEnumerator<Shape>.Create(shape), true));
			UpdateEntity<Shape>(_loadedShapes, _newShapes, shape);
			// Update connections automatically will not work due to the fact that loaded connections 
			// are not stored and therefore we don't know which connections are really new.
			//DoUpdateShapeConnections(shape);

			if (shape.Children.Count > 0) {
				// Delete removed child shapes
				List<Shape> shapesToDelete = new List<Shape>();
				foreach (Shape childShape in GetCachedEntities<Shape>(_loadedShapes, _newShapes, shape))
					if (!shape.Children.Contains(childShape)) shapesToDelete.Add(childShape);
				DoDeleteShapes(shapesToDelete, true);

				// Update/Insert all existing child shapes
				foreach (Shape childShape in shape.Children) {
					object childId = ((IEntity)childShape).Id;
					if (CanUndeleteEntity(childShape, _loadedShapes))
						DoUndeleteShapes(childShape, shape, true);
					else if ((childId != null && _loadedShapes.Contains(childId)) || _newShapes.ContainsKey(childShape))
						DoUpdateTemplateShapeWithContent(childShape);
					else DoInsertShapes(SingleInstanceEnumerator<Shape>.Create(childShape), shape, true);
				}
			}
		}


		private void DoUpdateTemplateModelObject(Template template) {
			// Insert / undelete / update model object
			if (template.Shape.ModelObject != null) {
				IModelObject modelObject = template.Shape.ModelObject;
				if (modelObject.Id == null && !_newModelObjects.ContainsKey(modelObject))
					DoInsertModelObject(template.Shape.ModelObject, template);
				else {
					if (CanUndeleteEntity<IModelObject>(modelObject, _loadedModelObjects))
						DoUndeleteModelObject(modelObject);
					DoUpdateModelObject(template.Shape.ModelObject);
				}
			}
		}


		private void DoDeleteTemplate(Template template, bool withContent) {
			if (template == null) throw new ArgumentNullException("template");
			if (withContent) {
				// Delete template's model object
				IModelObject modelObject = template.Shape.ModelObject;
				if (modelObject != null) {
					// Detach shape and model object
					template.Shape.ModelObject = null;
					DoDeleteModelObject(modelObject);
				}
				// Delete the template's shape and model object
				//DoDeleteShapes(GetShapes(SingleInstanceEnumerator<Shape>.Create(template.Shape), true), withContent);
				DoDeleteShapes(SingleInstanceEnumerator<Shape>.Create(template.Shape), withContent);
				// Delete template's model mappings
				DoDeleteModelMappings(template.GetPropertyMappings());
			}
			// Delete the template
			AssertCanDelete(template);
			DeleteEntity<Template>(_loadedTemplates, _newTemplates, template);
			if (TemplateDeleted != null) TemplateDeleted(this, GetTemplateEventArgs(template));
		}


		private void DoUndeleteTemplate(Template template, bool withContent) {
			UndeleteEntity<Template>(_loadedTemplates, template);
			if (withContent) {
				DoUndeleteShapes(template.Shape, template, withContent);

				if (template.Shape.ModelObject != null) {
					DoUndeleteModelObject(template.Shape.ModelObject);
					Undelete(template.GetPropertyMappings(), template);
				}
			}
		}


		private void DoInsertModelMappings(IEnumerable<IModelMapping> modelMappings, Template template) {
			AssertCanInsert(modelMappings);
			foreach (IModelMapping modelMapping in modelMappings)
				InsertEntity<IModelMapping>(_newModelMappings, modelMapping, template);
		}


		private void DoUpdateModelMappings(IEnumerable<IModelMapping> modelMappings) {
			AssertCanUpdate(modelMappings);
			foreach (IModelMapping modelMapping in modelMappings)
				UpdateEntity<IModelMapping>(_loadedModelMappings, _newModelMappings, modelMapping);
		}


		private void DoDeleteModelMappings(IEnumerable<IModelMapping> modelMappings) {
			AssertCanDelete(modelMappings);
			foreach (IModelMapping modelMapping in modelMappings)
				DeleteEntity<IModelMapping>(_loadedModelMappings, _newModelMappings, modelMapping);
		}


		private void DoUndeleteModelMappings(IEnumerable<IModelMapping> modelMappings, Template owner) {
			AssertCanUndelete(modelMappings);
			foreach (IModelMapping modelMapping in modelMappings)
				UndeleteEntity<IModelMapping>(_loadedModelMappings, modelMapping, owner);
		}


		private Template GetModelMappingOwner(IModelMapping modelMapping) {
			Template owner = null;
			if (modelMapping.Id == null) {
				Debug.Assert(_newModelMappings.ContainsKey(modelMapping));
				Debug.Assert(_newModelMappings[modelMapping] is Template);
				owner = (Template)_newModelMappings[modelMapping];
			} else {
				Debug.Assert(_loadedModelMappings[modelMapping.Id].Owner is Template);
				owner = (Template)_loadedModelMappings[modelMapping.Id].Owner;
			}
			return owner;
		}

		#endregion


		#region [Private] Implementation - Diagrams, Shapes and ShapeConnections

		private void DoInsertDiagram(Diagram diagram, bool withContent) {
			AssertCanInsert(diagram);
			// Check for duplicate diagram names
			foreach (Diagram d in GetDiagrams()) {
				if (string.Compare(d.Name, diagram.Name, Comparison) == 0)
					throw new CachedRepositoryException(ResourceStrings.MessageFmt_Entity01AlreadyExists, ResourceStrings.Text_Diagram, diagram.Name);
			}

			InsertEntity<Diagram>(_newDiagrams, diagram, GetProject());
			if (withContent) {
				if (diagram.ModelObject != null) {
					AssertCanInsert(diagram.ModelObject);
					DoInsertDiagramModelObject(diagram.ModelObject);
				}
				AssertCanInsert(diagram.Shapes, diagram);
				DoInsertShapes(diagram.Shapes, diagram, withContent);
			}
		}


		private void DoDeleteDiagram(Diagram diagram, bool withContent) {
			if (withContent) {
				if (diagram.ModelObject != null) {
					AssertCanDelete(diagram.ModelObject);

				}
				AssertCanDelete(diagram.Shapes);
				// First, delete all shapes with their connections
				DoDeleteShapes(diagram.Shapes, withContent);
				if (ShapesDeleted != null) ShapesDeleted(this, GetShapesEventArgs(diagram.Shapes, diagram));
			}
			// Now we can delete the actual diagram
			AssertCanDelete(diagram);
			DeleteEntity<Diagram>(_loadedDiagrams, _newDiagrams, diagram);
		}


		private void DoUndeleteDiagram(Diagram diagram, bool withContent) {
			AssertCanUndelete(diagram);
			UndeleteEntity<Diagram>(_loadedDiagrams, diagram);
			if (withContent) {
				AssertCanUndelete(diagram.Shapes, diagram);
				// First, delete all shapes with their connections
				DoUndeleteShapes(diagram.Shapes, diagram, withContent);
			}
		}


		private void DoInsertShapes(IEnumerable<Shape> shapes, Shape parentShape, bool withContent) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (parentShape == null) throw new ArgumentNullException("parentShape");
			AssertCanInsert(shapes, parentShape);
			DoInsertShapesCore(shapes, parentShape, withContent);
		}


		private void DoInsertShapes(IEnumerable<Shape> shapes, Diagram diagram, bool withContent) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (diagram == null) throw new ArgumentNullException("diagram");
			AssertCanInsert(shapes, diagram);
			DoInsertShapesCore(shapes, diagram, withContent);
		}


		private void DoInsertShapes(Shape shape, Template template, bool withContent) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (template == null) throw new ArgumentNullException("template");
			AssertCanInsert(GetShapes(SingleInstanceEnumerator<Shape>.Create(shape), true), template);
			DoInsertShapesCore(SingleInstanceEnumerator<Shape>.Create(shape), template, withContent);
		}


		private void DoInsertShapesCore(IEnumerable<Shape> shapes, IEntity parentEntity, bool withContent) {
			foreach (Shape shape in shapes) {
				// This check will be done in CanInsert(Shape shape)
				if (shape.ModelObject != null)
					Debug.Assert(IsExistent(shape.ModelObject, _newModelObjects, _loadedModelObjects), "Shape has a model object that does not exist in the repository.");
				InsertEntity<Shape>(_newShapes, shape, parentEntity);
				if (withContent)
					DoInsertShapesCore(shape.Children.BottomUp, shape, withContent);
			}
			// Insert connections only when inserting with content
			if (withContent) {
				foreach (Shape shape in shapes) {
					// Insert only connectons of the active shapes
					foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Any, null))
						if (shape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue))
							DoInsertShapeConnection(shape, sci);
				}
			}
		}


		private void DoUpdateShapes(IEnumerable<Shape> shapes) {
			AssertCanUpdate(shapes);
			foreach (Shape shape in shapes)
				UpdateEntity<Shape>(_loadedShapes, _newShapes, shape);
		}


		private void DoUpdateShapeOwner(Shape shape, IEntity owner) {
			if (((IEntity)shape).Id == null) {
				_newShapes.Remove(shape);
				_newShapes.Add(shape, owner);
			} else {
				_loadedShapes[((IEntity)shape).Id].Owner = owner;
				_loadedShapes[((IEntity)shape).Id].State = ItemState.OwnerChanged;
			}
		}


		//private void DoDeleteShapes(Shape shape, bool withContent) {
		//    DoDeleteShapes(SingleInstanceEnumerator<Shape>.Create(shape), withContent);
		//}


		private void DoDeleteShapes(IEnumerable<Shape> shapes, bool withContent) {
			AssertCanDelete(GetShapes(shapes, withContent));
			DoDeleteShapesCore(shapes, withContent);
		}


		private void DoDeleteShapesCore(IEnumerable<Shape> shapes, bool withContent) {
			if (withContent) {
				// Delete the shape's children
				foreach (Shape shape in shapes) {
					DoDeleteShapesCore(shape.Children, withContent);
					if (withContent) {
						// Delete all connections of the deleted shapes
						foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Any, null))
							if (shape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue))
								DoDeleteShapeConnection(shape, sci);
					}
				}
			}
			// Delete the shape itself
			foreach (Shape shape in shapes)
				DeleteEntity<Shape>(_loadedShapes, _newShapes, shape);
		}


		private void DoUndeleteShapes(Shape shape, IEntity parentEntity, bool withContent) {
			AssertOpen();
			AssertCanUndelete(GetShapes(SingleInstanceEnumerator<Shape>.Create(shape), withContent), parentEntity);
			DoUndeleteShapes(SingleInstanceEnumerator<Shape>.Create(shape), parentEntity, withContent);
		}


		private void DoUndeleteShapes(IEnumerable<Shape> shapes, IEntity parentEntity, bool withContent) {
			AssertOpen();
			foreach (Shape shape in shapes) {
				UndeleteEntity<Shape>(_loadedShapes, shape);
				// Re-attach model object after undelete
				if (shape.ModelObject != null) {
					bool isAttached = false;
					foreach (Shape s in shape.ModelObject.Shapes)
						if (shape == s) {
							isAttached = true;
							break;
						}
					if (!isAttached) shape.ModelObject.AttachShape(shape);
				}
				if (withContent) {
					if (shape.Children.Count > 0)
						DoUndeleteShapes(shape.Children, shape, withContent);
					// Insert connections
					foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Any, null))
						if (shape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue))
							DoInsertShapeConnection(shape, sci);
				}
			}
		}


		private void DoUnloadShapes(IEnumerable<Shape> shapes) {
			// First unload the children then the parent.
			// TODO 2: Should we allow to remove from new shapes?
			foreach (Shape s in shapes) {
				DoUnloadShapes(s.Children);
				if (((IEntity)s).Id == null) _newShapes.Remove(s);
				else _loadedShapes.Remove(((IEntity)s).Id);
			}
		}


		private ShapeConnection DoInsertShapeConnection(Shape shape, ShapeConnectionInfo connectionInfo) {
			if (shape.HasControlPointCapability(connectionInfo.OwnPointId, ControlPointCapabilities.Glue))
				return DoInsertShapeConnection(shape, connectionInfo.OwnPointId, connectionInfo.OtherShape, connectionInfo.OtherPointId);
			else return DoInsertShapeConnection(connectionInfo.OtherShape, connectionInfo.OtherPointId, shape, connectionInfo.OwnPointId);
		}


		private ShapeConnection DoInsertShapeConnection(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId) {
			ShapeConnection connection;
			connection.ConnectorShape = activeShape;
			connection.GluePointId = gluePointId;
			connection.TargetShape = passiveShape;
			connection.TargetPointId = connectionPointId;

			// If the inserted connection is not in the list of deleted connections, it's a new 
			// connection and has to be added to the list of new connections.
			if (!_deletedShapeConnections.Remove(connection)) {
				if (!_newShapeConnections.Contains(connection))
					_newShapeConnections.Add(connection);
#if REPOSITORY_CHECK
				else {
					// The connection must have been inserted twice or not deleted previously
					throw new NShapeException(string.Format(ResourceStrings.MessageFmt_0AlreadyExistsInRepository, ResourceStrings.Text_Connection));
				}
#endif
			}
			_isModified = true;
			return connection;
		}


		private ShapeConnection DoDeleteShapeConnection(Shape shape, ShapeConnectionInfo connectionInfo) {
			if (shape.HasControlPointCapability(connectionInfo.OwnPointId, ControlPointCapabilities.Glue))
				return DoDeleteShapeConnection(shape, connectionInfo.OwnPointId, connectionInfo.OtherShape, connectionInfo.OtherPointId);
			else return DoDeleteShapeConnection(connectionInfo.OtherShape, connectionInfo.OtherPointId, shape, connectionInfo.OwnPointId);
		}


		private ShapeConnection DoDeleteShapeConnection(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId) {
			ShapeConnection connection;
			connection.ConnectorShape = activeShape;
			connection.GluePointId = gluePointId;
			connection.TargetShape = passiveShape;
			connection.TargetPointId = connectionPointId;

			// If the deleted connection is not in the list of new connections, it's a loaded 
			// connection and has to be added to the list of deleted connections
			if (!_newShapeConnections.Remove(connection)) {
				if (!_deletedShapeConnections.Contains(connection))
					_deletedShapeConnections.Add(connection);
#if REPOSITORY_CHECK
				else {
					// The connection must have been deleted twice or not inserted previously
					throw new NShapeException(string.Format(ResourceStrings.MessageFmt_0AlreadyDeletedFromTheRepository, ResourceStrings.Text_Connection));
				}
#endif
			}
			_isModified = true;
			return connection;
		}


		private bool IsShapeInRepository(Shape shape) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			IEntity shapeEntity = (IEntity)shape;
			if (shapeEntity.Id == null)
				return _newShapes.ContainsKey(shape);
			else return _loadedShapes.ContainsKey(shapeEntity.Id);
		}


		private bool IsShapeInCollection(Shape shape, IEnumerable<Shape> shapeCollection) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			foreach (Shape s in shapeCollection)
				if (s == shape) return true;
			return false;
		}


		/// <summary>
		/// Returns all shapes (child and parent shapes) in a 'flat' collection
		/// </summary>
		private IEnumerable<Shape> GetShapes(IEnumerable<Shape> shapes, bool withChildren) {
			foreach (Shape shape in shapes) {
				if (withChildren)
					foreach (Shape childShape in GetShapes(shape.Children, true))
						yield return childShape;
				yield return shape;
			}
		}

		#endregion


		#region [Private] Implementation - Model and IModelObjects

		private void EnsureModelIsLoaded() {
			// Load instances from store if...
			// - there is a store,
			// - there are no loaded objects at all and 
			// - if the current project has an id (otherwise the project was not saved yet and there is nothing to load).
			if (_store != null && IsProjectLoaded && _loadedModels.Count <= 0) {
				_store.LoadModel(this, ProjectId);

				// Get loaded model
				Model model = null;
				foreach (Model m in GetCachedEntities(_loadedModels, _newModels))
					model = m;
				Debug.Assert(model != null);

				// Load *all* model objects (including their child model objects!				 
				EnsureModelObjectsAreLoaded(model);
			}
		}


		private bool TryGetModel(out Model model) {
			AssertOpen();
			EnsureModelIsLoaded();
			model = null;
			foreach (Model m in GetCachedEntities(_loadedModels, _newModels)) {
				model = m;
				return true;
			}
			return false;
		}


		private EntityBucket<IModelObject> GetModelObjectItem(object id) {
			if (id == null) throw new NShapeException(ResourceStrings.MessageFmt_Entity0HasNoId, ResourceStrings.Text_ModelObject);
			EntityBucket<IModelObject> item;
			if (!_loadedModelObjects.TryGetValue(id, out item))
				throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1NotFoundInRepository, ResourceStrings.Text_ModelObject, id);
			return item;
		}


		private void DoInsertModelObjects(IEnumerable<IModelObject> modelObjects) {
			AssertCanInsert(modelObjects);
			IEntity model = GetModel();
			foreach (IModelObject modelObject in modelObjects) {
				IEntity owner = (modelObject.Parent != null) ? modelObject.Parent : model;
				InsertEntity<IModelObject>(_newModelObjects, modelObject, owner);
			}
		}


		private void DoInsertModelObject(IModelObject modelObject, Template template) {
			DoInsertModelObjects(SingleInstanceEnumerator<IModelObject>.Create(modelObject), template);
		}


		private void DoInsertModelObjects(IEnumerable<IModelObject> modelObjects, Template template) {
			AssertCanInsert(modelObjects);
			foreach (IModelObject modelObject in modelObjects)
				InsertEntity<IModelObject>(_newModelObjects, modelObject, template);
		}


		private void DoUpdateModelObjectOwner(IModelObject modelObject, IEntity owner) {
			Debug.Assert(owner == null || owner is Model || owner is Template || owner is IModelObject, "Invalid parent type.");
			if (((IEntity)modelObject).Id == null) {
				_newModelObjects.Remove(modelObject);
				_newModelObjects.Add(modelObject, owner ?? GetModel());
			} else {
				_loadedModelObjects[((IEntity)modelObject).Id].Owner = owner ?? GetModel();
				_loadedModelObjects[((IEntity)modelObject).Id].State = ItemState.OwnerChanged;
			}
		}


		private void DoUpdateModelObject(IModelObject modelObject) {
			DoUpdateModelObjects(SingleInstanceEnumerator<IModelObject>.Create(modelObject));
		}


		private void DoUpdateModelObjects(IEnumerable<IModelObject> modelObjects) {
			AssertCanUpdate(modelObjects);
			foreach (IModelObject modelObject in modelObjects)
				UpdateEntity<IModelObject>(_loadedModelObjects, _newModelObjects, modelObject);
		}


		private void DoDeleteModelObject(IModelObject modelObject) {
			DoDeleteModelObjects(SingleInstanceEnumerator<IModelObject>.Create(modelObject));
		}


		private void DoDeleteModelObjects(IEnumerable<IModelObject> modelObjects) {
			AssertCanDelete(modelObjects);
			foreach (IModelObject modelObject in modelObjects)
				DeleteEntity<IModelObject>(_loadedModelObjects, _newModelObjects, modelObject);
		}


		private void DoUndeleteModelObject(IModelObject modelObject) {
			DoUndeleteModelObjects(SingleInstanceEnumerator<IModelObject>.Create(modelObject));
		}


		private void DoUndeleteModelObjects(IEnumerable<IModelObject> modelObjects) {
			AssertCanUndelete(modelObjects);
			foreach (IModelObject modelObject in modelObjects)
				UndeleteEntity<IModelObject>(_loadedModelObjects, modelObject);
		}

		#endregion


		#region [Private] Implementation - DiagramModelObjects

		private EntityBucket<IDiagramModelObject> GetDiagramModelObjectItem(object id) {
			if (id == null) throw new NShapeException(ResourceStrings.MessageFmt_Entity0HasNoId, ResourceStrings.Text_DiagramModelObject);
			EntityBucket<IDiagramModelObject> item;
			if (!_loadedDiagramModelObjects.TryGetValue(id, out item))
				throw new NShapeException(ResourceStrings.MessageFmt_Entity0WithId1NotFoundInRepository, ResourceStrings.Text_DiagramModelObject, id);
			return item;
		}


		private void DoInsertDiagramModelObject(IDiagramModelObject diagramModelObject) {
			DoInsertDiagramModelObjects(SingleInstanceEnumerator<IDiagramModelObject>.Create(diagramModelObject));
		}


		private void DoInsertDiagramModelObjects(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			AssertCanInsert(diagramModelObjects);
			Model model = GetModel();
			foreach (IDiagramModelObject diagramModelObject in diagramModelObjects)
				InsertEntity<IDiagramModelObject>(_newDiagramModelObjects, diagramModelObject, model);
		}


		private void DoUpdateDiagramModelObject(IDiagramModelObject diagramModelObject) {
			DoUpdateDiagramModelObjects(SingleInstanceEnumerator<IDiagramModelObject>.Create(diagramModelObject));
		}


		private void DoUpdateDiagramModelObjects(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			AssertCanUpdate(diagramModelObjects);
			foreach (IDiagramModelObject diagramModelObject in diagramModelObjects)
				UpdateEntity<IDiagramModelObject>(_loadedDiagramModelObjects, _newDiagramModelObjects, diagramModelObject);
		}


		private void DoDeleteDiagramModelObject(IDiagramModelObject diagramModelObject) {
			DoDeleteDiagramModelObjects(SingleInstanceEnumerator<IDiagramModelObject>.Create(diagramModelObject));
		}


		private void DoDeleteDiagramModelObjects(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			AssertCanDelete(diagramModelObjects);
			foreach (IDiagramModelObject diagramModelObject in diagramModelObjects)
				DeleteEntity<IDiagramModelObject>(_loadedDiagramModelObjects, _newDiagramModelObjects, diagramModelObject);
		}


		private void DoUndeleteDiagramModelObject(IDiagramModelObject diagramModelObject) {
			DoUndeleteDiagramModelObjects(SingleInstanceEnumerator<IDiagramModelObject>.Create(diagramModelObject));
		}


		private void DoUndeleteDiagramModelObjects(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			AssertCanUndelete(diagramModelObjects);
			foreach (IDiagramModelObject diagramModelObject in diagramModelObjects)
				UndeleteEntity<IDiagramModelObject>(_loadedDiagramModelObjects, diagramModelObject);
		}

		#endregion


		#region [Private] Methods: Consistency checks

		#region Designs and Styles

		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanInsert(Design design) {
			if (!CanInsert(design, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUpdate(Design design) {
			if (!CanUpdate(design, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanDelete(Design design) {
			if (!CanDelete(design, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUndelete(Design design) {
			if (!CanUndeleteEntity(design, _loadedDesigns, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanInsert(IEnumerable<IStyle> styles) {
			if (!CanInsert(styles, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUpdate(IStyle style) {
			if (!CanUpdate(style, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanDelete(IEnumerable<IStyle> styles) {
			if (!CanDelete(styles, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUndelete(IEnumerable<IStyle> styles) {
			foreach (IStyle style in styles)
				if (!CanUndeleteEntity(style, _loadedStyles, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}

		#endregion


		#region Model and ModelObjects

		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanInsert(Model model) {
			if (!CanInsert(model, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUpdate(Model model) {
			if (!CanUpdate(model, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanDelete(Model model) {
			if (!CanDelete(model, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUndelete(Model model) {
			if (!CanUndeleteEntity(model, _loadedModels, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanInsert(IEnumerable<IModelObject> modelObjects) {
			if (!CanInsert(modelObjects, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUpdate(IEnumerable<IModelObject> modelObjects) {
			if (!CanUpdate(modelObjects, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanDelete(IEnumerable<IModelObject> modelObjects) {
			if (!CanDelete(modelObjects, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUndelete(IEnumerable<IModelObject> modelObjects) {
			foreach (IModelObject modelObject in modelObjects)
				if (!CanUndeleteEntity(modelObject, _loadedModelObjects, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}

		#endregion


		#region DiagramModelObject

		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanInsert(IDiagramModelObject diagramModelObject) {
			if (!CanInsertEntity(diagramModelObject, _newDiagramModelObjects, _loadedDiagramModelObjects, out _reasonText))
				throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanInsert(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			if (!CanInsert(diagramModelObjects, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUpdate(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			if (!CanUpdate(diagramModelObjects, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUpdate(IDiagramModelObject diagramModelObject) {
			if (!CanUpdateEntity(diagramModelObject, _newDiagramModelObjects, _loadedDiagramModelObjects, out _reasonText))
				throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanDelete(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			if (!CanDelete(diagramModelObjects, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanDelete(IDiagramModelObject diagramModelObject) {
			if (!CanDeleteEntity(diagramModelObject, _newDiagramModelObjects, _loadedDiagramModelObjects, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUndelete(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			foreach (IDiagramModelObject diagramModelObject in diagramModelObjects)
				if (!CanUndeleteEntity(diagramModelObject, _loadedDiagramModelObjects, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUndelete(IDiagramModelObject diagramModelObject) {
			if (!CanUndeleteEntity(diagramModelObject, _loadedDiagramModelObjects, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}

		#endregion


		#region Templates and ModelMappings

		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanInsert(Template template, bool checkContentCanInsert) {
			if (!CanInsert(template, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUpdate(Template template) {
			if (!CanUpdate(template, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanDelete(Template template) {
			if (!CanDelete(template, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanInsert(IEnumerable<IModelMapping> modelMappings) {
			if (!CanInsert(modelMappings, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUpdate(IEnumerable<IModelMapping> modelMappings) {
			if (!CanUpdate(modelMappings, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanDelete(IEnumerable<IModelMapping> modelMappings) {
			if (!CanDelete(modelMappings, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUndelete(IEnumerable<IModelMapping> modelMappings) {
			foreach (IModelMapping modelMapping in modelMappings)
				if (!CanUndeleteEntity(modelMapping, _loadedModelMappings, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}

		#endregion


		#region Diagram, Shapes and Connections

		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanInsert(Diagram diagram) {
			if (!CanInsert(diagram, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUpdate(Diagram diagram) {
			if (!CanUpdate(diagram, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanDelete(Diagram diagram) {
			if (!CanDelete(diagram, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUndelete(Diagram diagram) {
			if (!CanUndeleteEntity(diagram, _loadedDiagrams, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanInsert(IEnumerable<Shape> shapes, IEntity parent) {
			if (parent is Diagram diagram) {
				if (!IsExistent(diagram, _newDiagrams, _loadedDiagrams))
					throw new CachedRepositoryException(Dataweb.NShape.Properties.Resources.MessageTxt_DiagramNotFoundInRepository);
			} else if (parent is Template) {
				foreach (Shape s in shapes)
					if (!HasNoTemplate(s))
						throw new CachedRepositoryException(Dataweb.NShape.Properties.Resources.MessageTxt_ShapeOrOneOfItsChildShapesHasATemplate);
			} else if (parent is Shape parentShape) {
				if (!IsExistent(parentShape, _newShapes, _loadedShapes))
					throw new CachedRepositoryException(Dataweb.NShape.Properties.Resources.MessageTxt_ParentShapeNotFoundInRepository);
			}
			if (!CanInsert(shapes, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUpdate(IEnumerable<Shape> shapes) {
			if (!CanUpdate(shapes, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUpdate(IEnumerable<Shape> shapes, IEntity owner) {
			if (!CanUpdate(shapes, owner, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanDelete(IEnumerable<Shape> shapes) {
			if (!CanDelete(shapes, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUndelete(IEnumerable<Shape> shapes, IEntity owner) {
			if (owner is Diagram ownerDiagram) {
				if (!IsExistent(ownerDiagram, _newDiagrams, _loadedDiagrams))
					throw new CachedRepositoryException(Dataweb.NShape.Properties.Resources.MessageTxt_DiagramDoesNotExistInTheRepository);
			} else if (owner is Template ownerTemplate) {
				if (!IsExistent(ownerTemplate, _newTemplates, _loadedTemplates))
					throw new CachedRepositoryException(Dataweb.NShape.Properties.Resources.MessageTxt_TemplateDoesNotExistInTheRepository);
			} else if (owner is Shape ownerShape) {
				if (!IsExistent(ownerShape, _newShapes, _loadedShapes))
					throw new CachedRepositoryException(Dataweb.NShape.Properties.Resources.MessageTxt_ParentShapeDoesNotExistInTheRepository);
			}
			foreach (Shape shape in shapes) AssertCanUndelete(shape);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanUndelete(Shape shape) {
			if (!CanUndeleteEntity(shape, _loadedShapes, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanInsert(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId) {
			if (!CanInsert(activeShape, gluePointId, passiveShape, connectionPointId, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}


		[Conditional(REPOSITORY_CHECK_DEFINE)]
		private void AssertCanDelete(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId) {
			if (!CanDelete(activeShape, gluePointId, passiveShape, connectionPointId, out _reasonText)) throw new CachedRepositoryException(_reasonText);
		}

		#endregion


		#endregion


		#region [Private] Methods: Consistency checks implementation

		private void AssertOpen() {
			if (!_isOpen) throw new NShapeException(ResourceStrings.MessageTxt_RepositoryIsNotOpen);
			Debug.Assert(_settings != null && _projectDesign != null);
		}


		private void AssertClosed() {
			if (_isOpen) throw new NShapeException(ResourceStrings.MessageTxt_RepositoryIsAlreadyOpen);
		}


		private void AssertStoreExists() {
			if (_store == null) throw new NShapeException(ResourceStrings.MessageTxt_NoStoreComponentConnectedToTheRepository);
		}


		// Project
		// ToDo: Implement consistency check methods for project class


		#region Consistency check implementations for Design and Styles

		private bool CanInsert(Design design, out string reason) {
			reason = null;
			CanInsertEntity(design, _newDesigns, _loadedDesigns, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdate(Design design, out string reason) {
			reason = null;
			CanUpdateEntity(design, _newDesigns, _loadedDesigns, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanDelete(Design design, out string reason) {
			reason = null;
			CanDeleteEntity(design, _newDesigns, _loadedDesigns, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanInsert(IEnumerable<IStyle> styles, out string reason) {
			reason = null;
			foreach (IStyle style in styles)
				CanInsertEntity(style, _newStyles, _loadedStyles, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdate(IStyle style, out string reason) {
			reason = null;
			CanUpdateEntity(style, _newStyles, _loadedStyles, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanDelete(IEnumerable<IStyle> styles, out string reason) {
			reason = null;
			foreach (IStyle style in styles) {
				if (IsStyleInUse(style, styles))
					reason = string.Format(ResourceStrings.MessageTxt_Type0IsStillInUse, ResourceStrings.Text_Style);
				else if (!IsExistent(style, _newStyles, _loadedStyles))
					reason = string.Format(ResourceStrings.MessageFmt_Entity01DoesNotExistInTheRepository, ResourceStrings.Text_Style, style.Title);
				if (!string.IsNullOrEmpty(reason)) break;
			}
			return string.IsNullOrEmpty(reason);
		}


		private bool IsStyleInUse(IStyle style, IEnumerable<IStyle> stylesToDelete) {
			if (style == null) throw new ArgumentNullException("style");
			AssertOpen();
			// First, check Styles and StyleModelMappings as they are always loaded completly,
			// then check new shapes and laoded shapes 
			// Finally, the store has to check all objects that are not loaded:
			//
			// Check Styles
			if (style is IColorStyle) {
				// Styles only have to checked if the given style is a ColorStyle
				foreach (KeyValuePair<IStyle, IEntity> keyValuePair in _newStyles)
					if (UsesStyle(style, keyValuePair.Key)) {
						if (Contains(stylesToDelete, keyValuePair.Key)) continue;
						return true;
					}
				// Check all loaded styles if style is used
				foreach (EntityBucket<IStyle> entityBucket in _loadedStyles) {
					if (entityBucket.State == ItemState.Deleted) continue;
					if (UsesStyle(style, entityBucket.ObjectRef)) {
						if (Contains(stylesToDelete, entityBucket.ObjectRef)) continue;
						return true;
					}
				}
			}
			// Check StyleModelMappings
			foreach (KeyValuePair<IModelMapping, IEntity> keyValuePair in _newModelMappings)
				if (UsesStyle(style, keyValuePair.Key)) return true;
			foreach (EntityBucket<IModelMapping> entityBucket in _loadedModelMappings) {
				if (entityBucket.State == ItemState.Deleted) continue;
				if (UsesStyle(style, entityBucket.ObjectRef)) return true;
			}
			// Check all new shapes if style is used
			foreach (KeyValuePair<Shape, IEntity> keyValuePair in _newShapes)
				if (keyValuePair.Key.HasStyle(style)) return true;
			// Check all loaded shapes if style is used
			foreach (EntityBucket<Shape> entityBucket in _loadedShapes) {
				if (entityBucket.State == ItemState.Deleted) continue;
				if (entityBucket.ObjectRef.HasStyle(style)) return true;
			}
			// If the given style is not a new style, check if it is used in 
			// currently not loaded objects
			if (style.Id != null)
				return _store.CheckStyleInUse(this, style.Id);
			return false;
		}

		#endregion


		#region Consistency check implementations for Model and Model Objects

		private bool CanInsert(Model model, out string reason) {
			reason = null;
			CanInsertEntity(model, _newModels, _loadedModels, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdate(Model model, out string reason) {
			reason = null;
			CanUpdateEntity(model, _newModels, _loadedModels, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanDelete(Model model, out string reason) {
			reason = null;
			CanDeleteEntity(model, _newModels, _loadedModels, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanInsert(IEnumerable<IModelObject> modelObjects, out string reason) {
			reason = null;
			foreach (IModelObject modelObject in modelObjects)
				if (!CanInsertEntity(modelObject, _newModelObjects, _loadedModelObjects, out reason))
					break;
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdate(IEnumerable<IModelObject> modelObjects, out string reason) {
			reason = null;
			foreach (IModelObject modelObject in modelObjects)
				CanUpdateEntity(modelObject, _newModelObjects, _loadedModelObjects, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanDelete(IEnumerable<IModelObject> modelObjects, out string reason) {
			reason = null;
			foreach (IModelObject modelObject in modelObjects) {
				if (CanDeleteEntity(modelObject, _newModelObjects, _loadedModelObjects, out reason)) {
					// Check if model object is used by shapes
					bool usedByShapes = false;
					foreach (KeyValuePair<Shape, IEntity> item in _newShapes) {
						if (item.Key.ModelObject == modelObject) {
							usedByShapes = true;
							break;
						}
					}
					if (!usedByShapes) {
						foreach (EntityBucket<Shape> item in _loadedShapes) {
							if (item.State == ItemState.Deleted) continue;
							if (item.ObjectRef.ModelObject == modelObject) {
								usedByShapes = true;
								break;
							}
						}
					}
					if (usedByShapes) reason = ResourceStrings.MessageTxt_ModelObjectIsStillUsedByShapes;

					if (string.IsNullOrEmpty(reason)) {
						bool usedByModelObjects = false;
						if (_newModelObjects.ContainsValue(modelObject))
							usedByModelObjects = true;
						else {
							foreach (EntityBucket<IModelObject> item in _loadedModelObjects) {
								if (item.Owner == modelObject && item.State != ItemState.Deleted) {
									usedByModelObjects = true;
									break;
								}
							}
						}
						if (usedByModelObjects) reason = Properties.Resources.MessageTxt_ModelObjectIsStillUsedByModelObjects;
					}
					if (string.IsNullOrEmpty(reason))
						if (IsModelObjectInUse(modelObject)) reason = Properties.Resources.MessageTxt_ModelObjectIsInUse;
				}
			}
			return string.IsNullOrEmpty(reason);
		}


		private bool CanInsert(IEnumerable<IDiagramModelObject> diagramModelObjects, out string reason) {
			reason = null;
			foreach (IDiagramModelObject diagramModelObject in diagramModelObjects)
				if (!CanInsertEntity(diagramModelObject, _newDiagramModelObjects, _loadedDiagramModelObjects, out reason))
					break;
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdate(IEnumerable<IDiagramModelObject> diagramModelObjects, out string reason) {
			reason = null;
			foreach (IDiagramModelObject diagramModelObject in diagramModelObjects)
				CanUpdateEntity(diagramModelObject, _newDiagramModelObjects, _loadedDiagramModelObjects, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanDelete(IEnumerable<IDiagramModelObject> diagramModelObjects, out string reason) {
			reason = null;
			foreach (IDiagramModelObject diagramModelObject in diagramModelObjects) {
				if (CanDeleteEntity(diagramModelObject, _newDiagramModelObjects, _loadedDiagramModelObjects, out reason)) {
					if (IsDiagramModelObjectInUse(diagramModelObject) && string.IsNullOrEmpty(reason))
						reason = Properties.Resources.MessageTxt_DiagramModelObjectIsInUse;
				}
			}
			return string.IsNullOrEmpty(reason);
		}

		#endregion


		#region Consistency check implementations for Templates and ModelMappings

		private bool CanInsert(Template template, out string reason) {
			reason = null;
			CanInsertEntity(template, _newTemplates, _loadedTemplates, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdate(Template template, out string reason) {
			reason = null;
			CanUpdateEntity(template, _newTemplates, _loadedTemplates, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanDelete(Template template, out string reason) {
			reason = null;
			if (CanDeleteEntity(template, _newTemplates, _loadedTemplates, out reason)) {
				// Check if the template shape was deleted
				bool shapeDeleted = true;
				if (_newShapes.ContainsValue(template))
					shapeDeleted = false;
				else {
					foreach (EntityBucket<Shape> item in _loadedShapes) {
						if (item.Owner == template && item.State != ItemState.Deleted) {
							shapeDeleted = false;
							break;
						}
					}
				}
				if (!shapeDeleted) reason = Properties.Resources.MessageTxt_TemplatesShapeWasNotDeleted;

				// Check if model mappings were deleted
				bool modelMappingsDeleted = true;
				if (_newModelMappings.ContainsValue(template))
					modelMappingsDeleted = false;
				else {
					foreach (EntityBucket<IModelMapping> item in _loadedModelMappings) {
						if (item.Owner == template && item.State != ItemState.Deleted) {
							modelMappingsDeleted = false;
							break;
						}
					}
				}
				if (!modelMappingsDeleted) reason = Properties.Resources.MessageTxt_TemplatesModelMappingsWereNotDeleted;

				// Check if template is still in use
				if (IsTemplateInUse(template)) reason = Properties.Resources.MessageTxt_TemplateIsInUse;
			}
			return string.IsNullOrEmpty(reason);
		}


		private bool CanInsert(IEnumerable<IModelMapping> modelMappings, out string reason) {
			reason = null;
			foreach (IModelMapping modelMapping in modelMappings)
				if (!CanInsertEntity(modelMapping, _newModelMappings, _loadedModelMappings, out reason))
					break;
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdate(IEnumerable<IModelMapping> modelMappings, out string reason) {
			reason = null;
			foreach (IModelMapping modelMapping in modelMappings)
				if (!CanUpdateEntity(modelMapping, _newModelMappings, _loadedModelMappings, out reason))
					break;
			if (string.IsNullOrEmpty(reason))
				OwnerIsValid(modelMappings, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanDelete(IEnumerable<IModelMapping> modelMappings, out string reason) {
			reason = null;
			foreach (IModelMapping modelMapping in modelMappings)
				if (!CanDeleteEntity(modelMapping, _newModelMappings, _loadedModelMappings, out reason))
					break;
			if (string.IsNullOrEmpty(reason))
				OwnerIsValid(modelMappings, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool OwnerIsValid(IEnumerable<IModelMapping> modelMappings, out string reason) {
			reason = null;
			Template owner = null;
			foreach (IModelMapping modelMapping in modelMappings) {
				if (owner == null) owner = GetModelMappingOwner(modelMapping);
				else if (owner != GetModelMappingOwner(modelMapping)) {
					reason = Dataweb.NShape.Properties.Resources.MessageTxt_NotAllModelMappingsArePartOfTheSameTemplate;
					break;
				}
			}
			return string.IsNullOrEmpty(reason);
		}

		#endregion


		#region Consistency check implementations for Diagram, Shapes and Connections

		private bool CanInsert(Diagram diagram, out string reason) {
			reason = null;
			CanInsertEntity(diagram, _newDiagrams, _loadedDiagrams, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdate(Diagram diagram, out string reason) {
			reason = null;
			CanUpdateEntity(diagram, _newDiagrams, _loadedDiagrams, out reason);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanDelete(Diagram diagram, out string reason) {
			reason = null;

			// Check if the diagram's shapes were deleted
			bool shapesDeleted = true;
			if (_newShapes.ContainsValue(diagram))
				shapesDeleted = false;
			else {
				foreach (EntityBucket<Shape> item in _loadedShapes) {
					if (item.Owner == diagram && item.State != ItemState.Deleted) {
						shapesDeleted = false;
						break;
					}
				}
			}
			if (!shapesDeleted) reason = Dataweb.NShape.Properties.Resources.MessageTxt_TheDiagramsShapesAreNotDeleted;

			//if (diagram.Shapes.Count > 0)
			//    reason = string.Format("Diagram still contains {0} shapes", diagram.Shapes.Count);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanInsert(IEnumerable<Shape> shapes, out string reason) {
			reason = null;
			if (IsConnectedToOtherShapes(shapes, shapes, true))
				reason = Dataweb.NShape.Properties.Resources.MessageTxt_ShapeIsConnectedToShapesThatAreNotInTheRepository;
			if (!string.IsNullOrEmpty(reason)) {
				foreach (Shape shape in shapes) {
					if (IsExistent(shape, _newShapes, _loadedShapes)) {
						reason = Dataweb.NShape.Properties.Resources.MessageTxt_ShapeAlreadyExistsInTheRepository;
						break;
					}
					if (shape.ModelObject != null) {
						if (!IsExistent(shape.ModelObject, _newModelObjects, _loadedModelObjects)) {
							reason = Dataweb.NShape.Properties.Resources.MessageTxt_ShapeHasAModelObjectThatDoesNotExistInTheRepository;
							break;
						}
					}
				}
			}
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdate(IEnumerable<Shape> shapes, out string reason) {
			reason = null;
			foreach (Shape shape in shapes)
				if (!CanUpdate(shape, out reason))
					break;
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdate(IEnumerable<Shape> shapes, IEntity owner, out string reason) {
			reason = null;
			foreach (Shape shape in shapes) {
				if (CanUpdate(shape, out reason)) {
					if (IsOwner(shape, owner))
						reason = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_ShapeIsAlreadyOwnedBy0, owner.GetType().Name);
					if (owner is Template) {
						if (!HasNoTemplate(shape))
							reason = "Shape or one of its child shapes has a template. Template shapes may not refer to other templates.";
					}
				}
				if (!string.IsNullOrEmpty(reason)) break;
			}
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdate(Shape shape, out string reason) {
			reason = null;
			if (!IsExistent(shape, _newShapes, _loadedShapes))
				reason = "Shape does not exist in the repository or is already deleted.";
			if (shape.Children.Count > 0) {
				foreach (Shape childShape in GetShapes(shape.Children, true))
					if (!IsExistent(childShape, _newShapes, _loadedShapes)) {
						reason = "Shape's child shapes either do not exist, are deleted or assigned to the wrong parent in the repository.";
						break;
					}
			}
			return string.IsNullOrEmpty(reason);
		}


		private bool CanDelete(IEnumerable<Shape> shapes, out string reason) {
			reason = null;
			// ToDo: Optimize this
			IEnumerable<Shape> allShapes = GetShapes(shapes, true);
			// Check if shape has children that are not deleted
			if (HasOtherChildren(shapes, allShapes))
				reason = "Shape has still children that were not deleted.";
			// Check shape has connections to other shapes
			if (IsConnectedToOtherShapes(shapes, allShapes, false))
				reason = "Shape has still connections to other shapes.";
			foreach (Shape shape in shapes) {
				if (shape.ModelObject != null) {
					foreach (Shape s in shape.ModelObject.Shapes) {
						if (shape == s) {
							reason = "Shape is still attached to a model object.";
							break;
						}
					}
				}
			}

			//if (shape.Diagram != null) 
			//    reason = string.Format("Shape was not removed from diagram '{0}'.", shape.Diagram);
			//if (shape.Template != null)
			//    reason = string.Format("Shape was not removed from template '{0}'.", shape.Template);
			//if (shape.Parent != null)
			//    reason = "Shape was not removed from its parent.";
			//if (shape.Children.Count > 0)
			//    reason = string.Format("Shape has still {0} children.", shape.Children.Count);
			//if (shape.IsConnected(ControlPointId.Any, null) != ControlPointId.None)
			//    reason = "Shape is still connected to other shapes.";

			return string.IsNullOrEmpty(reason);
		}


		private bool IsOwner(Shape shape, IEntity owner) {
			bool isOwner = false;
			if (_newShapes.ContainsKey(shape))
				isOwner = (_newShapes[shape] == owner);
			else {
				object id = ((IEntity)shape).Id;
				if (_loadedShapes[id].Owner == owner && _loadedShapes[id].State != ItemState.Deleted)
					isOwner = true;
			}
			return isOwner;
		}


		private bool HasChildren(IEnumerable<Shape> shapesToCheck) {
			return HasOtherChildren(shapesToCheck, null);
		}


		private bool HasOtherChildren(IEnumerable<Shape> shapesToCheck, IEnumerable<Shape> allShapes) {
			// Check if shape has children that are not deleted
			bool hasChildren = false;
			foreach (Shape shape in shapesToCheck) {
				if (shape.Children.Count == 0) continue;
				foreach (Shape childShape in shape.Children) {
					if (allShapes != null && IsShapeInCollection(childShape, allShapes)) continue;
					if (IsExistent(childShape, _newShapes, _loadedShapes)) {
						hasChildren = true;
						break;
					}
				}
			}
			return hasChildren;
		}


		private bool IsConnectedToOtherShapes(IEnumerable<Shape> shapes, IEnumerable<Shape> allShapes, bool allowConnectionsToRepositoryMembers) {
			// Search for connections with shapes not contained in the given collection
			foreach (Shape shape in shapes) {
				foreach (ShapeConnectionInfo ci in shape.GetConnectionInfos(ControlPointId.Any, null)) {
					if (!shape.HasControlPointCapability(ci.OwnPointId, ControlPointCapabilities.Glue)) continue;
					if (allowConnectionsToRepositoryMembers && IsShapeInRepository(ci.OtherShape)) continue;
					if (IsShapeInCollection(ci.OtherShape, allShapes)) continue;
					return true;
				}
			}
			return false;
		}


		private bool HasNoTemplate(Shape shape) {
			Debug.Assert(shape != null);
			bool result = true;
			if (shape.Template != null) result = false;
			foreach (Shape childShape in shape.Children)
				if (!HasNoTemplate(childShape)) result = false;
			return result;
		}


		private bool CanInsert(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId, out string reason) {
			reason = null;
			if (!IsExistent(activeShape, _newShapes, _loadedShapes)) reason = "Active shape is not in the repository.";
			if (!IsExistent(passiveShape, _newShapes, _loadedShapes)) reason = "Passive shape is not in the repository.";
			if (!activeShape.HasControlPointCapability(gluePointId, ControlPointCapabilities.Glue)) reason = string.Format("Active shape's point {0} is not a glue point.", gluePointId);
			if (!passiveShape.HasControlPointCapability(connectionPointId, ControlPointCapabilities.Connect)) reason = string.Format("Passive shape's point {0} is not a connection point.", connectionPointId);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanDelete(Shape activeShape, ControlPointId gluePointId, Shape passiveShape, ControlPointId connectionPointId, out string reason) {
			reason = null;
			if (!IsExistent(activeShape, _newShapes, _loadedShapes)) reason = "Active shape is not in the repository.";
			if (!IsExistent(passiveShape, _newShapes, _loadedShapes)) reason = "Passive shape is not in the repository.";
			if (!activeShape.HasControlPointCapability(gluePointId, ControlPointCapabilities.Glue)) reason = string.Format("Active shape's point {0} is not a glue point.", gluePointId);
			if (!passiveShape.HasControlPointCapability(connectionPointId, ControlPointCapabilities.Connect)) reason = string.Format("Passive shape's point {0} is not a connection point.", connectionPointId);
			return string.IsNullOrEmpty(reason);
		}

		#endregion


		private bool CanInsertEntity<TEntity>(TEntity entity, IDictionary<TEntity, IEntity> newEntities, LoadedEntities<TEntity> loadedEntities, out string reason)
			where TEntity : IEntity {
			reason = null;
			if (entity.Id != null)
				reason = string.Format(ResourceStrings.MessageFmt_EntityAlreadyHasAnIdAssigned, entity.GetType().Name, entity);
			else if (IsExistent(entity, newEntities, loadedEntities))
				reason = string.Format(ResourceStrings.MessageFmt_EntityAlreadyExistsInTheRepository, entity.GetType().Name, entity);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUpdateEntity<TEntity>(TEntity entity, IDictionary<TEntity, IEntity> newEntities, LoadedEntities<TEntity> loadedEntities, out string reason)
			where TEntity : IEntity {
			reason = null;
			if (!IsExistent(entity, newEntities, loadedEntities))
				reason = string.Format(ResourceStrings.MessageFmt_EntityDoesNotExistInTheRepository, entity.GetType().Name, entity);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanDeleteEntity<TEntity>(TEntity entity, IDictionary<TEntity, IEntity> newEntities, LoadedEntities<TEntity> loadedEntities, out string reason)
			where TEntity : IEntity {
			reason = null;
			if (!IsExistent(entity, newEntities, loadedEntities))
				reason = string.Format(ResourceStrings.MessageFmt_EntityDoesNotExistInTheRepository, entity.GetType().Name, entity);
			return string.IsNullOrEmpty(reason);
		}


		private bool CanUndeleteEntity<TEntity>(TEntity entity, LoadedEntities<TEntity> loadedEntities, out string reason) where TEntity : IEntity {
			reason = null;
			if (!CanUndeleteEntity(entity, loadedEntities)) {
				reason = string.Format(ResourceStrings.MessageFmt_EntityCannotBeUndeleted, entity.GetType().Name, entity);
				return false;
			} else return true;
		}


		private bool CanUndeleteEntity<TEntity>(TEntity entity, LoadedEntities<TEntity> loadedEntities) where TEntity : IEntity {
			if (entity == null) throw new ArgumentNullException("entity");
			if (loadedEntities == null) throw new ArgumentNullException("loadedEntities");
			EntityBucket<TEntity> item = null;
			if (entity.Id != null && loadedEntities.TryGetValue(entity.Id, out item)) {
				return (item.State == ItemState.Deleted);
			} else return false;
		}


		private bool IsExistent<TEntity>(TEntity entity, IDictionary<TEntity, IEntity> newEntities, LoadedEntities<TEntity> loadedEntities)
			where TEntity : IEntity {
			bool isExistent = false;
			if (entity.Id == null)
				isExistent = newEntities.ContainsKey(entity);
			else if (loadedEntities.ContainsKey(entity.Id))
				isExistent = (loadedEntities[entity.Id].State != ItemState.Deleted);
			return isExistent;
		}


		private object ProjectId {
			get { return ((IEntity)_settings).Id; }
		}


		private bool IsProjectLoaded {
			get {
				if (_settings is IEntity settingsEntity)
					return (settingsEntity.Id != null);
				return false;
			}
		}

		#endregion


		#region [Private] Methods for retrieving EventArgs

		private RepositoryProjectEventArgs GetProjectEventArgs(ProjectSettings projectSettings) {
			return new RepositoryProjectEventArgs(projectSettings);
		}


		private RepositoryModelEventArgs GetModelEventArgs(Model model) {
			return new RepositoryModelEventArgs(model);
		}


		private RepositoryDesignEventArgs GetDesignEventArgs(Design design) {
			return new RepositoryDesignEventArgs(design);
		}


		private RepositoryStyleEventArgs GetStyleEventArgs(IStyle style) {
			return new RepositoryStyleEventArgs(style);
		}


		private RepositoryDiagramEventArgs GetDiagramEventArgs(Diagram diagram) {
			return new RepositoryDiagramEventArgs(diagram);
		}


		private RepositoryTemplateEventArgs GetTemplateEventArgs(Template template) {
			return new RepositoryTemplateEventArgs(template);
		}


		private RepositoryTemplateShapeReplacedEventArgs GetTemplateShapeExchangedEventArgs(Template template, Shape oldTemplateShape, Shape newTemplateShape) {
			return new RepositoryTemplateShapeReplacedEventArgs(template, oldTemplateShape, newTemplateShape);
		}


		private RepositoryShapesEventArgs GetShapesEventArgs(Shape shape) {
			Diagram diagram;
			if (((IEntity)shape).Id == null)
				diagram = _newShapes[shape] as Diagram;
			else
				diagram = _loadedShapes[((IEntity)shape).Id].Owner as Diagram;
			return new RepositoryShapesEventArgs(SingleInstanceEnumerator<Shape>.Create(shape), diagram);
		}


		private RepositoryShapesEventArgs GetShapesEventArgs(Shape shape, Diagram diagram) {
			return new RepositoryShapesEventArgs(SingleInstanceEnumerator<Shape>.Create(shape), diagram);
		}


		private RepositoryShapesEventArgs GetShapesEventArgs(IEnumerable<Shape> shapes, Diagram diagram) {
			return new RepositoryShapesEventArgs(shapes, diagram);
		}


		private RepositoryShapesEventArgs GetShapesEventArgs(IEnumerable<Shape> shapes) {
			Diagram diagram = null;
			foreach (Shape shape in shapes) {
				if (((IEntity)shape).Id == null) {
					if (_newShapes.ContainsKey(shape)) {
						Debug.Assert(diagram == null || diagram == _newShapes[shape]);
						diagram = _newShapes[shape] as Diagram;
					}
				} else if (_loadedShapes.ContainsKey(((IEntity)shape).Id)) {
					Debug.Assert(diagram == null || diagram == _loadedShapes[((IEntity)shape).Id].Owner as Diagram);
					diagram = _loadedShapes[((IEntity)shape).Id].Owner as Diagram;
				}
			}
			return new RepositoryShapesEventArgs(shapes, diagram);
		}


		private RepositoryModelObjectsEventArgs GetModelObjectsEventArgs(IModelObject modelObject) {
			return new RepositoryModelObjectsEventArgs(SingleInstanceEnumerator<IModelObject>.Create(modelObject));
		}


		private RepositoryModelObjectsEventArgs GetModelObjectsEventArgs(IEnumerable<IModelObject> modelObjects) {
			return new RepositoryModelObjectsEventArgs(modelObjects);
		}


		private RepositoryDiagramModelObjectsEventArgs GetDiagramModelObjectsEventArgs(IDiagramModelObject diagramModelObject) {
			return new RepositoryDiagramModelObjectsEventArgs(SingleInstanceEnumerator<IDiagramModelObject>.Create(diagramModelObject));
		}


		private RepositoryDiagramModelObjectsEventArgs GetDiagramModelObjectsEventArgs(IEnumerable<IDiagramModelObject> diagramModelObjects) {
			return new RepositoryDiagramModelObjectsEventArgs(diagramModelObjects);
		}


		private RepositoryShapeConnectionEventArgs GetShapeConnectionEventArgs(ShapeConnection connection) {
			return new RepositoryShapeConnectionEventArgs(connection);
		}

		#endregion


		private class ResourceStrings {
			internal const string MessageFmt_0AlreadyDeletedFromTheRepository = "{0} already deleted from the repository.";
			internal const string MessageFmt_0AlreadyExistsInRepository = "{0} already exists in the repository.";
			internal const string MessageFmt_Entity01AlreadyExists = "{0} '{1}' already exists.";
			internal const string MessageFmt_Entity01DoesNotExistInTheRepository = "{0} '{1}' does not exist in the repository.";
			internal const string MessageFmt_Entity01WasDeleted = "{0} '{1}' was deleted.";
			internal const string MessageFmt_Entity0HasNoId = "{0} has no id.";
			internal const string MessageFmt_Entity0WithId1NotFoundInRepository = "{0} with id '{1}' not found in repository.";
			internal const string MessageFmt_Entity0WithId1WasDeleted = "{0} with id '{1}' was deleted.";
			internal const string MessageFmt_EntityAlreadyExistsInTheRepository = "{0} '{1}' already exists in the repository.";
			internal const string MessageFmt_EntityAlreadyHasAnIdAssigned = "{0} '{1}' already has an Id assigned. Entities with Id cannot be inserted.";
			internal const string MessageFmt_EntityCannotBeUndeleted = "{0} '{1}' cannot be undeleted. It was not loaded from a store or it was not deleted.";
			internal const string MessageFmt_EntityDoesNotExistInTheRepository = "{0} '{1}' does not exist in the repository or is already deleted.";
			internal const string MessageFmt_EntityType0DoesNotExistInTheRepository = "Entity type '{0}' does not exist in the repository.";
			internal const string MessageFmt_EntityTypeWithElementName0IsNotRegistered = "An entity type with element name '{0}' is not registered.";
			internal const string MessageFmt_EntityWasNotDeletedBefore = "Entity was not deleted before. Only deleted entities can be undeleted.";
			internal const string MessageFmt_RepositoryAlreadyContainsAnEntityType0 = "The repository already contains an entity type called '{0}'.";
			internal const string MessageFmt_TwoProjectsNamed0FoundInRepository = "Two projects named '{0}' found in repository.";
			internal const string MessageTxt_AModelAleadyExistsInTheRepository = "A model aleady exists. More than one model per project is not supported.";
			internal const string MessageTxt_AModelDoesNotExistInTheRepository = "A model does not exist in the repository.";
			internal const string MessageTxt_AnEntityWithoutIdCannotBeUndeleted = "An entity without id cannot be undeleted.";
			internal const string MessageTxt_CurrentProjectDesignCannotBeDeleted = "Current project design cannot be deleted.";
			internal const string MessageTxt_EntitiesWithAnIdCannotBeInsertedIntoTheRepository = "Entities with an id cannot be inserted into the repository.";
			internal const string MessageTxt_EntityIsAlreadyDeleted = "Entity is already deleted.";
			internal const string MessageTxt_EntityNotFoundInRepository = "Entity not found in repository.";
			internal const string MessageTxt_EntityWasDeletedUndeleteBeforeModifying = "Entity was deleted before. Undelete the entity before modifying it.";
			internal const string MessageTxt_InvalidEntityTypeName = "Invalid entity type name.";
			internal const string MessageTxt_ModelObjectIsStillUsedByShapes = "Model object is still used by shapes.";
			internal const string MessageTxt_NoStoreComponentConnectedToTheRepository = "There is no store component connected to the repository.";
			internal const string MessageTxt_ProjectNameNotDefined = "No project name defined.";
			internal const string MessageTxt_ProjectStylesNotFound = "Project styles not found.";
			internal const string MessageTxt_PropertyStoreNotSet = "Property Store is not set.";
			internal const string MessageTxt_RepositoryHasNoStoreAttached = "Repository has no store attached.";
			internal const string MessageTxt_RepositoryHasNoStoreAttachedOpenNotAllowed = "Repository has no store attached. An in-memory repository must be created, not opened.";
			internal const string MessageTxt_RepositoryIsAlreadyOpen = "Repository is already open.";
			internal const string MessageTxt_RepositoryIsNotOpen = "Repository is not open.";
			internal const string MessageTxt_StoreDoesNotExist = "Store does not exist.";
			internal const string MessageTxt_Type0IsStillInUse = "{0} is still in use.";
			internal const string MessageTxt_UnsavedRepositoryModificationsPending = "There are unsaved modifications of the repository. Please save all changes first.";
			internal const string Text_Connection = "Connection";
			internal const string Text_Design = "Design";
			internal const string Text_Diagram = "Diagram";
			internal const string Text_Entity = "Entity";
			internal const string Text_DiagramModelObject = "Diagram Model Object";
			internal const string Text_ModelObject = "Model Object";
			internal const string Text_ModelObjects = "Model Objects";
			internal const string Text_Project = "Project";
			internal const string Text_Shapes = "Shapes";
			internal const string Text_Style = "Style";
			internal const string Text_Template = "Template";
		}


		#region [Private] Constants and Fields

		private const string REPOSITORY_CHECK_DEFINE = "REPOSITORY_CHECK";
		private const string DEBUG_DEFINE = "DEBUG";

		private const StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;

		// project info is an internal entity type
		private const string ProjectInfoEntityTypeName = "ProjectInfo";

		// Used to calculate the element entityTypeName
		static private StringBuilder _stringBuilder = new StringBuilder();

		/// <summary>
		/// True, when modfications have been done to any part of the projects since
		/// Open or SaveChanges. 
		/// </summary>
		private bool _isModified;

		// True, when Open was successfully called. Is identical to store.IsOpen if store 
		// is defined.
		private bool _isOpen;

		/// <summary>
		/// Reference to the open project for easier access.
		/// </summary>
		private ProjectSettings _settings;

		/// <summary>
		/// Indicates the pseudo design used to manage the styles of the project.
		/// This design is not entered in the designs or newDesigns dictionaries.
		/// </summary>
		private Design _projectDesign;

		private int _version;

		// Name of the project
		private string _projectName = string.Empty;

		// Store for cache data. Is null, if no store is assigned to open
		// cache, i.e. the cache is in-memory.
		private Store _store;

		// project needs an owner for the newProjects dictionary
		private ProjectOwner _projectOwner = new ProjectOwner();

		// Buffers
		private string _reasonText;

		// DirectoryName of registered entities
		private Dictionary<string, IEntityType> _entityTypes = new Dictionary<string, IEntityType>();

		// Containers for loaded objects
		private LoadedEntities<ProjectSettings> _loadedProjects = new LoadedEntities<ProjectSettings>();
		private LoadedEntities<Model> _loadedModels = new LoadedEntities<Model>();
		private LoadedEntities<Design> _loadedDesigns = new LoadedEntities<Design>();
		private LoadedEntities<IStyle> _loadedStyles = new LoadedEntities<IStyle>();
		private LoadedEntities<Diagram> _loadedDiagrams = new LoadedEntities<Diagram>();
		private LoadedEntities<Template> _loadedTemplates = new LoadedEntities<Template>();
		private LoadedEntities<IModelMapping> _loadedModelMappings = new LoadedEntities<IModelMapping>();
		private LoadedEntities<Shape> _loadedShapes = new LoadedEntities<Shape>();
		private LoadedEntities<IModelObject> _loadedTemplateModelObjects = new LoadedEntities<IModelObject>();
		private LoadedEntities<IModelObject> _loadedModelObjects = new LoadedEntities<IModelObject>();
		private LoadedEntities<IDiagramModelObject> _loadedDiagramModelObjects = new LoadedEntities<IDiagramModelObject>();

		// Containers for new entities
		// Stores the new entity as the key and its parent as the value.
		// (New objects do not yet have an id and are therefore not addressable in the dictionary.)
		private Dictionary<ProjectSettings, IEntity> _newProjects = new Dictionary<ProjectSettings, IEntity>();
		private Dictionary<Model, IEntity> _newModels = new Dictionary<Model, IEntity>();
		private Dictionary<Design, IEntity> _newDesigns = new Dictionary<Design, IEntity>();
		private Dictionary<IStyle, IEntity> _newStyles = new Dictionary<IStyle, IEntity>();
		private Dictionary<Diagram, IEntity> _newDiagrams = new Dictionary<Diagram, IEntity>();
		private Dictionary<Template, IEntity> _newTemplates = new Dictionary<Template, IEntity>();
		private Dictionary<IModelMapping, IEntity> _newModelMappings = new Dictionary<IModelMapping, IEntity>();
		private Dictionary<Shape, IEntity> _newShapes = new Dictionary<Shape, IEntity>();
		private Dictionary<IModelObject, IEntity> _newTemplateModelObjects = new Dictionary<IModelObject, IEntity>();
		private Dictionary<IModelObject, IEntity> _newModelObjects = new Dictionary<IModelObject, IEntity>();
		private Dictionary<IDiagramModelObject, IEntity> _newDiagramModelObjects = new Dictionary<IDiagramModelObject, IEntity>();
		private List<ShapeConnection> _newShapeConnections = new List<ShapeConnection>();
		private List<ShapeConnection> _deletedShapeConnections = new List<ShapeConnection>();

		#endregion

	}


	/// <summary>
	/// An exception class raised by the CachedRepository class to indicate a fatal error.
	/// </summary>
	public class CachedRepositoryException : NShapeInternalException {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInternalException" />.
		/// </summary>
		public CachedRepositoryException(string message)
			: base(message) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInternalException" />.
		/// </summary>
		public CachedRepositoryException(string message, Exception innerException)
			: base(message, innerException) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInternalException" />.
		/// </summary>
		public CachedRepositoryException(string format, params object[] args)
			: base(string.Format(format, args), (Exception)null) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NShapeInternalException" />.
		/// </summary>
		public CachedRepositoryException(string format, Exception innerException, params object[] args)
			: base(string.Format(format, args), innerException) {
		}


		/// <summary>
		/// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
		/// </summary>
		protected CachedRepositoryException(SerializationInfo info, StreamingContext context)
			: base(info, context) {
		}
	}


	#region EntityBucket<TObject> Class

	/// <summary>
	/// Specifies the state of a persistent entity stored in a <see cref="T:Dataweb.NShape.Advanced.CachedRepository" />
	/// </summary>
	public enum ItemState {
		/// <summary>The entity was not modified.</summary>
		Original,
		/// <summary>The entity was modified and not yet saved.</summary>
		Modified,
		/// <summary>The owner of the entity changed.</summary>
		OwnerChanged,
		/// <summary>The entity was deleted from the repository but not yet saved.</summary>
		Deleted,
		/// <summary>The entity is new.</summary>
		New
	};


	// EntityBucket is a reference type, because it is entered into dictionaries.
	// Modifying a field of a value type in a dictionary is not possible during
	// an enumeration, but we have to modify at least the State.
	/// <summary>
	/// Stores a reference to a loaded object together with its state.
	/// </summary>
	public class EntityBucket<TObject> {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.EntityBucket`1" />
		/// </summary>
		public EntityBucket(TObject obj, IEntity owner, ItemState state) {
			this.ObjectRef = obj;
			this.Owner = owner;
			this.State = state;
		}

		/// <summary>
		/// Gets the object stored in the <see cref="T:Dataweb.NShape.Advanced.EntityBucket`1" />
		/// </summary>
		public TObject ObjectRef;

		/// <summary>
		/// Gets the owner <see cref="T:Dataweb.NShape.Advanced.IEntity" />.
		/// </summary>
		public IEntity Owner;

		/// <summary>
		/// Gets the <see cref="T:Dataweb.NShape.Advanced.ItemState" />.
		/// </summary>
		public ItemState State;
	}

	#endregion


	#region ShapeConnection Struct

	/// <summary>Represents a connection between two shapes.</summary>
	public struct ShapeConnection : IEquatable<ShapeConnection> {

		/// <summary>Implementation of the equality operator.</summary>
		public static bool operator ==(ShapeConnection x, ShapeConnection y) {
			return (
				x.ConnectorShape == y.ConnectorShape
				&& x.TargetShape == y.TargetShape
				&& x.GluePointId == y.GluePointId
				&& x.TargetPointId == y.TargetPointId);
		}

		/// <summary>Implementation of the unequality operator.</summary>
		public static bool operator !=(ShapeConnection x, ShapeConnection y) { return !(x == y); }

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ShapeConnection" />.
		/// </summary>
		public ShapeConnection(Diagram diagram, Shape connectorShape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId) {
			this.ConnectorShape = connectorShape;
			this.GluePointId = gluePointId;
			this.TargetShape = targetShape;
			this.TargetPointId = targetPointId;
		}

		/// <override></override>
		public override bool Equals(object obj) {
			return obj is ShapeConnection && this == (ShapeConnection)obj;
		}

		/// <override></override>
		public bool Equals(ShapeConnection other) {
			return other == this;
		}

		/// <override></override>
		public override int GetHashCode() {
			int result = GluePointId.GetHashCode() ^ TargetPointId.GetHashCode();
			if (ConnectorShape != null) result ^= ConnectorShape.GetHashCode();
			if (TargetShape != null) result ^= TargetShape.GetHashCode();
			return result;
		}

		/// <summary>Represents an empty ShapeConnection.</summary>
		public static readonly ShapeConnection Empty;

		/// <summary>Gets or sets the active shape that will follow the passive shape when it moves.</summary>
		public Shape ConnectorShape;

		/// <summary>Gets or sets the glue control point id of the active shape.</summary>
		public ControlPointId GluePointId;

		/// <summary>Gets or sets the passive shape.</summary>
		public Shape TargetShape;

		/// <summary>Gets or sets the connection point id the active shape is connected to.</summary>
		public ControlPointId TargetPointId;


		static ShapeConnection() {
			Empty.ConnectorShape = null;
			Empty.GluePointId = ControlPointId.None;
			Empty.TargetShape = null;
			Empty.TargetPointId = ControlPointId.None;
		}
	}

	#endregion


	#region Delegates

	/// <summary>
	/// Defines a filter function for the loading methods.
	/// </summary>
	public delegate bool FilterDelegate<TEntity>(TEntity entity, IEntity owner);


	/// <summary>
	/// Retrieves the entity with the given id.
	/// </summary>
	/// <param name="pid"></param>
	/// <returns></returns>
	public delegate IEntity Resolver(object pid);

	#endregion


	#region RepositoryReader Class

	/// <summary>
	/// Cache reader for the cached cache.
	/// </summary>
	public abstract class RepositoryReader : IRepositoryReader, IDisposable {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.RepositoryReader" />.
		/// </summary>
		protected RepositoryReader(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			this._cache = cache;
		}


		#region [Public] IRepositoryReader Members

		/// <summary>
		/// Fetches the next set of inner objects and prepares them for reading.
		/// </summary>
		public abstract void BeginReadInnerObjects();


		/// <summary>
		/// Finishes reading the current set of inner objects.
		/// </summary>
		public abstract void EndReadInnerObjects();


		/// <summary>
		/// Fetches the next inner object in a set of inner object.
		/// </summary>
		public bool BeginReadInnerObject() {
			if (innerObjectsReader == null)
				return DoBeginObject();
			else return innerObjectsReader.BeginReadInnerObject();
		}


		/// <summary>
		/// Finishes reading an inner object.
		/// </summary>
		public abstract void EndReadInnerObject();


		/// <summary>
		/// Reads a boolean value from the data source.
		/// </summary>
		public bool ReadBool() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadBool();
			} else return innerObjectsReader.ReadBool();
		}


		/// <summary>
		/// Reads a byte value from the data source.
		/// </summary>
		public byte ReadByte() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadByte();
			} else return innerObjectsReader.ReadByte();
		}


		/// <summary>
		/// Reads a 16 bit integer value from the data source.
		/// </summary>
		/// <returns></returns>
		public short ReadInt16() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadInt16();
			} else return innerObjectsReader.ReadInt16();
		}


		/// <summary>
		/// Reads a 32 bit integer value from the data source.
		/// </summary>
		/// <returns></returns>
		public int ReadInt32() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadInt32();
			} else return innerObjectsReader.ReadInt32();
		}


		/// <summary>
		/// Reads a 64 bit integer value from the data source.
		/// </summary>
		/// <returns></returns>
		public long ReadInt64() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadInt64();
			} else return innerObjectsReader.ReadInt64();
		}


		/// <summary>
		/// Reads a single precision floating point number from the data source.
		/// </summary>
		/// <returns></returns>
		public float ReadFloat() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadFloat();
			} else return innerObjectsReader.ReadFloat();
		}


		/// <summary>
		/// Reads a double precision floating point number from the data source.
		/// </summary>
		/// <returns></returns>
		public double ReadDouble() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadDouble();
			} else return innerObjectsReader.ReadDouble();
		}


		/// <summary>
		/// Reads a character value.
		/// </summary>
		public char ReadChar() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadChar();
			} else return innerObjectsReader.ReadChar();
		}


		/// <summary>
		/// Reads a string value from the data source.
		/// </summary>
		public string ReadString() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadString();
			} else return innerObjectsReader.ReadString();
		}


		/// <summary>
		/// Reads a date and time value from the data source.
		/// </summary>
		public DateTime ReadDate() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadDate();
			} else return innerObjectsReader.ReadDate();
		}


		/// <summary>
		/// Reads an image value from the data source.
		/// </summary>
		public System.Drawing.Image ReadImage() {
			if (innerObjectsReader == null) {
				++PropertyIndex;
				ValidatePropertyIndex();
				return DoReadImage();
			} else return innerObjectsReader.ReadImage();
		}


		/// <summary>
		/// Reads a template from the data source.
		/// </summary>
		public Template ReadTemplate() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetTemplate(id);
		}


		/// <summary>
		/// Reads a shape from the data source.
		/// </summary>
		/// <returns></returns>
		public Shape ReadShape() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetShape(id);
		}


		/// <summary>
		/// Reads a model object from the data source.
		/// </summary>
		public IModelObject ReadModelObject() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetModelObject(id);
		}


		/// <summary>
		/// Reads a model object from the data source.
		/// </summary>
		public IDiagramModelObject ReadDiagramModelObject() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetDiagramModelObject(id);
		}


		/// <summary>
		/// Reads a design from the data source.
		/// </summary>
		public Design ReadDesign() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetDesign(id);
		}


		/// <summary>
		/// Reads a cap style from the data source.
		/// </summary>
		public ICapStyle ReadCapStyle() {
			if (innerObjectsReader == null) {
				object id = ReadId();
				if (id == null) return null;
				return (ICapStyle)_cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadCapStyle();
		}


		/// <summary>
		/// Reads a character style from the data source.
		/// </summary>
		public ICharacterStyle ReadCharacterStyle() {
			if (innerObjectsReader == null) {
				object id = ReadId();
				if (id == null) return null;
				return (ICharacterStyle)_cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadCharacterStyle();
		}


		/// <summary>
		/// Reads a color style from the data source.
		/// </summary>
		public IColorStyle ReadColorStyle() {
			if (innerObjectsReader == null) {
				object id = ReadId();
				if (id == null) return null;
				return (IColorStyle)_cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadColorStyle();
		}


		/// <summary>
		/// Reads a fill style from the data source.
		/// </summary>
		public IFillStyle ReadFillStyle() {
			if (innerObjectsReader == null) {
				object id = ReadId();
				if (id == null) return null;
				return (IFillStyle)_cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadFillStyle();
		}


		/// <summary>
		/// Reads a line style from the data source.
		/// </summary>
		public ILineStyle ReadLineStyle() {
			if (innerObjectsReader == null) {
				object id = ReadId();
				if (id == null) return null;
				IStyle style = _cache.GetProjectStyle(id);
				Debug.Assert(style is ILineStyle, string.Format("Style {0} is not a line style.", id));
				return (ILineStyle)style;
			} else return innerObjectsReader.ReadLineStyle();
		}


		/// <summary>
		/// Reads a paragraph stylefrom the data source.
		/// </summary>
		public IParagraphStyle ReadParagraphStyle() {
			if (innerObjectsReader == null) {
				object id = ReadId();
				if (id == null) return null;
				return (IParagraphStyle)_cache.GetProjectStyle(id);
			} else return innerObjectsReader.ReadParagraphStyle();
		}

		#endregion


		#region [Public] IDisposable Members

		/// <summary>
		/// Releases all allocated unmanaged or persistent resources.
		/// </summary>
		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion


		#region [Protected] Properties

		/// <summary>
		/// Indicates the current index in the list of property info of the entity type.
		/// </summary>
		protected internal int PropertyIndex {
			get { return _propertyIndex; }
			set { _propertyIndex = value; }
		}


		/// <summary>
		/// The IStoreCache that contains the data to read.
		/// </summary>
		protected IStoreCache Cache {
			get { return _cache; }
		}


		/// <summary>
		/// A read only collection of property info of the entity type to read.
		/// </summary>
		protected IEnumerable<EntityPropertyDefinition> PropertyInfos {
			get { return propertyInfos; }
		}


		/// <summary>
		/// When reading inner objects, this property stores the owner entity of the inner objects. Otherwise, this property is null/Nothing.
		/// </summary>
		protected IEntity Object {
			get { return _entity; }
			set { _entity = value; }
		}

		#endregion


		#region [Protected] Methods: Implementation

		/// <summary>
		/// Implementation of reading an id value. Reads an id or null, if no id exists.
		/// </summary>
		protected internal abstract object ReadId();


		/// <summary>
		/// Resets the repositoryReader for a sequence of reads of entities of the same type.
		/// </summary>
		internal virtual void ResetFieldReading(IEnumerable<EntityPropertyDefinition> propertyInfos) {
			if (propertyInfos == null) throw new ArgumentNullException("propertyInfos");
			this.propertyInfos.Clear();
			this.propertyInfos.AddRange(propertyInfos);
			_propertyIndex = int.MinValue;
		}


		/// <summary>
		/// Advances to the next object and prepares reading it.
		/// </summary>
		protected internal abstract bool DoBeginObject();


		/// <summary>
		/// Finishes reading an object.
		/// </summary>
		protected internal abstract void DoEndObject();


		/// <summary>
		/// Implementation of reading a boolean value.
		/// </summary>
		protected abstract bool DoReadBool();


		/// <summary>
		/// Implementation of reading a byte value.
		/// </summary>
		protected abstract byte DoReadByte();


		/// <summary>
		/// Implementation of reading a 16 bit integer number.
		/// </summary>
		protected abstract short DoReadInt16();


		/// <summary>
		/// Implementation of reading a 32 bit integer number.
		/// </summary>
		protected abstract int DoReadInt32();


		/// <summary>
		/// Implementation of reading a 64 bit integer number.
		/// </summary>
		protected abstract long DoReadInt64();


		/// <summary>
		/// Implementation of reading a single precision floating point number.
		/// </summary>
		protected abstract float DoReadFloat();


		/// <summary>
		/// Implementation of reading a double precision floating point number.
		/// </summary>
		protected abstract double DoReadDouble();


		/// <summary>
		/// Implementation of reading a character value.
		/// </summary>
		protected abstract char DoReadChar();


		/// <summary>
		/// Implementation of reading a string value.
		/// </summary>
		/// <returns></returns>
		protected abstract string DoReadString();


		/// <summary>
		/// Implementation of reading a date and time value.
		/// </summary>
		protected abstract DateTime DoReadDate();


		/// <summary>
		/// Implementation of reading an image.
		/// </summary>
		protected abstract System.Drawing.Image DoReadImage();


		/// <summary>
		/// Implementation of reading a template.
		/// </summary>
		protected Template DoReadTemplate() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetTemplate(id);
		}


		/// <summary>
		/// Implementation of reading a shape.
		/// </summary>
		protected Shape DoReadShape() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetShape(id);
		}


		/// <summary>
		/// Implementation of reading a model object.
		/// </summary>
		protected IModelObject DoReadModelObject() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetModelObject(id);
		}


		/// <summary>
		/// Implementation of reading a design.
		/// </summary>
		protected Design DoReadDesign() {
			object id = ReadId();
			if (id == null) return null;
			else return Cache.GetDesign(id);
		}


		/// <summary>
		/// Implementation of reading a cap style.
		/// </summary>
		protected ICapStyle DoReadCapStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (ICapStyle)_cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Implementation of reading a character style.
		/// </summary>
		protected ICharacterStyle DoReadCharacterStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (ICharacterStyle)_cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Implementation of reading a color style.
		/// </summary>
		protected IColorStyle DoReadColorStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (IColorStyle)_cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Implementation of reading a fill style.
		/// </summary>
		protected IFillStyle DoReadFillStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (IFillStyle)_cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Implementation of reading a line style.
		/// </summary>
		protected ILineStyle DoReadLineStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (ILineStyle)_cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Implementation of reading a paragraph style.
		/// </summary>
		protected IParagraphStyle DoReadParagraphStyle() {
			object id = ReadId();
			if (id == null) return null;
			else return (IParagraphStyle)_cache.GetProjectStyle(id);
		}


		/// <summary>
		/// Checks whether the current property index refers to a valid entity field.
		/// </summary>
		protected virtual void ValidatePropertyIndex() {
			// We cannot check propertyIndex < 0 because some readers use PropertyIndex == -1 for the id.
			if (_propertyIndex >= propertyInfos.Count)
				throw new NShapeException("An entity tries to read more properties from the repository than there are defined.");
		}


		/// <override></override>
		protected virtual void Dispose(bool disposing) {
			// Nothing to do
		}

		#endregion


		#region Fields

		/// <summary>
		/// A list of property info of the entity type to read.
		/// </summary>
		protected List<EntityPropertyDefinition> propertyInfos = new List<EntityPropertyDefinition>(20);

		/// <summary>
		/// When reading inner objects, this field holds the reader used for reading these inner objects.
		/// </summary>
		protected RepositoryReader innerObjectsReader;

		private IStoreCache _cache;
		private int _propertyIndex;
		// Used for loading innerObjects
		private IEntity _entity;

		#endregion
	}

	#endregion


	#region RepositoryWriter Class

	/// <summary>
	/// Offline RepositoryWriter
	/// </summary>
	public abstract class RepositoryWriter : IRepositoryWriter {

		/// <summary>
		/// Initializes a new Iinstance of RepositoryWriter
		/// </summary>
		protected RepositoryWriter(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			this._cache = cache;
		}


		#region [Public] IRepositoryWriter Members

		/// <summary>
		/// Fetches the next inner object in a set of inner object.
		/// </summary>
		public void BeginWriteInnerObject() {
			// Must be executed by the outer writer. Currently there is only one inner 
			// and one outer.
			DoBeginWriteInnerObject();
		}


		/// <summary>
		/// Fetches the next set of inner objects and prepares them for writing.
		/// </summary>
		public void BeginWriteInnerObjects() {
			if (InnerObjectsWriter != null)
				throw new InvalidOperationException("Call EndWriteInnerObjects before a new call to BeginWriteInnerObjects.");
			DoBeginWriteInnerObjects();
		}


		/// <summary>
		/// Finishes writing an inner object.
		/// </summary>
		public void EndWriteInnerObject() {
			// Must be executed by the outer writer. Currently there is only one inner 
			// and one outer.
			DoEndWriteInnerObject();
		}


		/// <summary>
		/// Finishes writing the current set of inner objects.
		/// </summary>
		public void EndWriteInnerObjects() {
			if (InnerObjectsWriter == null)
				throw new InvalidOperationException("BeginWriteInnerObjects has not been called.");
			DoEndWriteInnerObjects();
			InnerObjectsWriter = null;
		}


		/// <summary>
		/// Deletes the current set of inner objects.
		/// </summary>
		public void DeleteInnerObjects() {
			BeginWriteInnerObjects();
			EndWriteInnerObjects();
		}


		/// <summary>
		/// Writes an IEntity.Id value.
		/// </summary>
		/// <param name="id"></param>
		public void WriteId(object id) {
			if (InnerObjectsWriter == null) DoWriteId(id);
			else InnerObjectsWriter.WriteId(id);
		}


		/// <summary>
		/// Writes a boolean value.
		/// </summary>
		public void WriteBool(bool value) {
			if (InnerObjectsWriter == null) DoWriteBool(value);
			else InnerObjectsWriter.WriteBool(value);
		}


		/// <summary>
		/// Writes a byte value.
		/// </summary>
		public void WriteByte(byte value) {
			if (InnerObjectsWriter == null) DoWriteByte(value);
			else InnerObjectsWriter.WriteByte(value);
		}


		/// <summary>
		/// Writes a 16 bit integer value.
		/// </summary>
		public void WriteInt16(short value) {
			if (InnerObjectsWriter == null) DoWriteInt16(value);
			else InnerObjectsWriter.WriteInt16(value);
		}


		/// <summary>
		/// Writes a 32 bit integer value.
		/// </summary>
		public void WriteInt32(int value) {
			if (InnerObjectsWriter == null) DoWriteInt32(value);
			else InnerObjectsWriter.WriteInt32(value);
		}


		/// <summary>
		/// Writes a 64 bit integer value.
		/// </summary>
		public void WriteInt64(long value) {
			if (InnerObjectsWriter == null) DoWriteInt64(value);
			else InnerObjectsWriter.WriteInt64(value);
		}


		/// <summary>
		/// Writes a single precision floating point number.
		/// </summary>
		public void WriteFloat(float value) {
			if (InnerObjectsWriter == null) DoWriteFloat(value);
			else InnerObjectsWriter.WriteFloat(value);
		}


		/// <summary>
		/// Writes a double precision floating point number.
		/// </summary>
		public void WriteDouble(double value) {
			if (InnerObjectsWriter == null) DoWriteDouble(value);
			else InnerObjectsWriter.WriteDouble(value);
		}


		/// <summary>
		/// Writes a character value.
		/// </summary>
		public void WriteChar(char value) {
			if (InnerObjectsWriter == null) DoWriteChar(value);
			else InnerObjectsWriter.WriteChar(value);
		}


		/// <summary>
		/// Writes a string value.
		/// </summary>
		public void WriteString(string value) {
			if (InnerObjectsWriter == null) DoWriteString(value);
			else InnerObjectsWriter.WriteString(value);
		}


		/// <summary>
		/// Writes a date and time value.
		/// </summary>
		public void WriteDate(DateTime value) {
			if (InnerObjectsWriter == null) DoWriteDate(value);
			else InnerObjectsWriter.WriteDate(value);
		}


		/// <summary>
		/// Writes an image value.
		/// </summary>
		public void WriteImage(System.Drawing.Image image) {
			if (InnerObjectsWriter == null) DoWriteImage(image);
			else InnerObjectsWriter.WriteImage(image);
		}


		/// <summary>
		/// Writes a model object.
		/// </summary>
		public void WriteModelObject(IModelObject modelObject) {
			if (InnerObjectsWriter == null) DoWriteModelObject(modelObject);
			else InnerObjectsWriter.WriteModelObject(modelObject);
		}


		/// <summary>
		/// Writes a model object.
		/// </summary>
		public void WriteDiagramModelObject(IDiagramModelObject diagramModelObject) {
			if (InnerObjectsWriter == null) DoWriteDiagramModelObject(diagramModelObject);
			else InnerObjectsWriter.WriteDiagramModelObject(diagramModelObject);
		}


		/// <summary>
		/// Writes a style.
		/// </summary>
		public void WriteStyle(IStyle style) {
			if (InnerObjectsWriter == null) DoWriteStyle(style);
			else InnerObjectsWriter.WriteStyle(style);
		}


		/// <summary>
		/// Writes a template.
		/// </summary>
		public void WriteTemplate(Template template) {
			if (InnerObjectsWriter == null) DoWriteTemplate(template);
			else InnerObjectsWriter.WriteTemplate(template);
		}

		#endregion


		#region [Protected] Properties

		/// <summary>
		/// Indicates the current index in the list of property info of the entity type.
		/// </summary>
		protected internal int PropertyIndex {
			get { return _propertyIndex; }
			set { _propertyIndex = value; }
		}


		/// <summary>
		/// This property stores the entity that is currently written.
		/// When writing inner objects, this is the owner of the inner objects.
		/// </summary>
		protected IEntity Entity {
			get { return _entity; }
		}


		/// <summary>
		/// The IStoreCache that contains the data to read.
		/// </summary>
		protected IStoreCache Cache {
			get { return _cache; }
		}

		#endregion


		#region [Protected] Properties

		/// <summary>
		/// When reading inner objects, this field holds the reader used for reading these inner objects.
		/// </summary>
		protected RepositoryWriter InnerObjectsWriter { get; set; }

		// Description of the entity type currently writting
		/// <summary>
		/// A list of <see cref="T:Dataweb.NShape.Advanced.EntityPropertyDefinition" /> for the entity type.
		/// </summary>
		protected List<EntityPropertyDefinition> PropertyInfos {
			get { return _propertyInfos; }
			set { _propertyInfos = value; }
		}

		#endregion


		#region [Protected] Methods: Implementation

		/// <summary>
		/// Implementation of writing an IEntity.Id value.
		/// </summary>
		protected abstract void DoWriteId(object id);

		/// <summary>
		/// Implementation of writing a boolean value.
		/// </summary>
		protected abstract void DoWriteBool(bool value);

		/// <summary>
		/// Implementation of writing a byte value.
		/// </summary>
		protected abstract void DoWriteByte(byte value);

		/// <summary>
		/// Implementation of writing a 16 bit integer number.
		/// </summary>
		protected abstract void DoWriteInt16(short value);

		/// <summary>
		/// Implementation of writing a 32 bit integer number.
		/// </summary>
		protected abstract void DoWriteInt32(int value);

		/// <summary>
		/// Implementation of writing a 64 bit integer number.
		/// </summary>
		protected abstract void DoWriteInt64(long value);

		/// <summary>
		/// Implementation of writing a single precision floating point number.
		/// </summary>
		protected abstract void DoWriteFloat(float value);

		/// <summary>
		/// Implementation of writing a double precision floating point number.
		/// </summary>
		protected abstract void DoWriteDouble(double value);

		/// <summary>
		/// Implementation of writing a character value.
		/// </summary>
		protected abstract void DoWriteChar(char value);

		/// <summary>
		/// Implementation of writing a string value.
		/// </summary>
		protected abstract void DoWriteString(string value);

		/// <summary>
		/// Implementation of writing a date value.
		/// </summary>
		protected abstract void DoWriteDate(DateTime date);

		/// <summary>
		/// Implementation of writing an image.
		/// </summary>
		protected abstract void DoWriteImage(System.Drawing.Image image);

		/// <summary>
		/// Implementation of writing a diagram model object.
		/// </summary>
		protected virtual void DoWriteModelObject(IModelObject modelObject) {
			if (modelObject != null && modelObject.Id == null)
				throw new InvalidOperationException(string.Format("{0} '{1}' was not inserted in the repository.",
					modelObject.Type.FullName, modelObject.Name));
			Debug.Assert(modelObject == null || modelObject.Id != null);
			if (modelObject == null) WriteId(null);
			else WriteId(modelObject.Id);
		}

		/// <summary>
		/// Implementation of writing a diagram model object.
		/// </summary>
		protected virtual void DoWriteDiagramModelObject(IDiagramModelObject diagramModelObject) {
			if (diagramModelObject != null && diagramModelObject.Id == null)
				throw new InvalidOperationException(string.Format("{0} '{1}' was not inserted in the repository.",
					diagramModelObject.Type.FullName, diagramModelObject.Name));
			Debug.Assert(diagramModelObject == null || diagramModelObject.Id != null);
			if (diagramModelObject == null) WriteId(null);
			else WriteId(diagramModelObject.Id);
		}

		/// <summary>
		/// Implementation of writing a style.
		/// </summary>
		protected virtual void DoWriteStyle(IStyle style) {
			if (style != null && style.Id == null) throw new InvalidOperationException(
				 string.Format("{0} '{1}' was not inserted in the repository.", style.GetType().Name, style.Name));
			if (style == null) WriteId(null);
			else WriteId(style.Id);
		}

		/// <summary>
		/// Implementation of writing a template.
		/// </summary>
		protected virtual void DoWriteTemplate(Template template) {
			if (template != null && template.Id == null)
				throw new InvalidOperationException(string.Format("Template '{0}' was not inserted in the repository.", template.Name));
			if (template == null) WriteId(null);
			else WriteId(template.Id);
		}

		/// <summary>
		/// Implementation of BeginWriteInnerObjects.
		/// </summary>
		protected abstract void DoBeginWriteInnerObjects();

		/// <summary>
		/// Implementation of EndWriteInnerObjects.
		/// </summary>
		protected abstract void DoEndWriteInnerObjects();

		// Must be called upon the outer cache writer.
		/// <summary>
		/// Implementation of BeginWriteInnerObject.
		/// </summary>
		protected abstract void DoBeginWriteInnerObject();

		// Must be called upon the outer cache writer.
		/// <summary>
		/// Implementation of EndWriteInnerObject.
		/// </summary>
		protected abstract void DoEndWriteInnerObject();

		/// <summary>
		/// Implementation of DeleteInnerObjects.
		/// </summary>
		protected abstract void DoDeleteInnerObjects();


		/// <summary>
		/// Reinitializes the writer to work with given property infos.
		/// </summary>
		protected internal virtual void Reset(IEnumerable<EntityPropertyDefinition> propertyInfos) {
			if (propertyInfos == null) throw new ArgumentNullException("propertyInfos");
			PropertyInfos.Clear();
			PropertyInfos.AddRange(propertyInfos);
		}


		/// <summary>
		/// Specifies the entity to write next. Is null when going to write an inner object.
		/// </summary>
		/// <param name="entity"></param>
		protected internal virtual void Prepare(IEntity entity) {
			_entity = entity;
			// The first property is the internally written id.
			PropertyIndex = -2;
		}


		/// <summary>
		/// Commits inner object data to the data store.
		/// </summary>
		protected internal virtual void Finish() {
			// Nothing to do
		}

		#endregion


		#region Fields

		private List<EntityPropertyDefinition> _propertyInfos = new List<EntityPropertyDefinition>(20);

		private IStoreCache _cache;
		// Current entity to write. Null when writing an inner object
		private IEntity _entity;
		// Index of property currently being written
		private int _propertyIndex;
		#endregion
	}

	#endregion

}

/******************************************************************************
  Copyright 2009-2019 dataweb GmbH
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
using System.Reflection;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.Commands {

	/// <summary>
	/// Encapsulates a command.
	/// </summary>
	public interface ICommand {

		/// <summary>
		/// Executes the command.
		/// </summary>
		void Execute();

		/// <summary>
		/// Reverts the command.
		/// </summary>
		void Revert();

		/// <summary>
		/// Tests whether the required permissions for the command are granted.
		/// </summary>
		bool IsAllowed(ISecurityManager securityManager);

		/// <summary>
		/// Tests whether the required permissions for the command are granted.
		/// </summary>
		/// <returns>
		/// Returns a <see cref="T:Dataweb.NShape.NShapeSecurityException"/> if the required permission is not granted.
		/// The exception can be thrown or used for retrieving a detailed description.
		/// </returns>
		Exception CheckAllowed(ISecurityManager securityManager);

		/// <summary>
		/// Specifies the cache on which the command will be executed.
		/// </summary>
		IRepository Repository { get; set; }

		/// <summary>
		/// Describes the purpose of the command.
		/// </summary>
		string Description { get; }
	}


	#region Base Classes

	/// <summary>
	/// Base class for all commands.
	/// </summary>
	public abstract class Command : ICommand {

		/// <override></override>
		protected Command() {
			// nothing to do here
		}


		/// <override></override>
		protected Command(IRepository repository)
			: this() {
			this.repository = repository;
		}


		/// <override></override>
		public abstract void Execute();


		/// <override></override>
		public abstract void Revert();


		/// <override></override>
		public abstract Permission RequiredPermission { get; }


		/// <override></override>
		public bool IsAllowed(ISecurityManager securityManager) {
			Exception result = null;
			return CheckAllowedCore(securityManager, false, out result);
		}


		/// <override></override>
		public Exception CheckAllowed(ISecurityManager securityManager) {
			Exception result = null;
			CheckAllowedCore(securityManager, true, out result);
			return result;
		}


		/// <override></override>
		public IRepository Repository {
			get { return repository; }
			set { repository = value; }
		}


		/// <override></override>
		public virtual string Description {
			get { return description; }
			protected set { description = value; }
		}


		/// <summary>
		/// Checks whether execution of this command is permitted.
		/// </summary>
		/// <param name="securityManager">The project's security manager.</param>
		/// <param name="createException">Specifies whether an exception instance should be created if the execution is denied.</param>
		/// <param name="exception">The created exception (is wanted).</param>
		protected abstract bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception);


		/// <summary>
		/// Checks whether the given entity has to be undeleted instead of being inserted
		/// </summary>
		protected bool CanUndeleteEntity(IEntity entity) {
			if (entity == null) throw new ArgumentNullException();
			return entity.Id != null;
		}


		/// <summary>
		/// Checks whether the given entity has to be undeleted instead of being inserted
		/// </summary>
		protected bool CanInsertEntity(IEntity entity) {
			if (entity == null) throw new ArgumentNullException();
			return entity.Id == null;
		}


		/// <summary>
		/// Checks whether the given entity has to be undeleted instead of being inserted
		/// </summary>
		protected bool CanUndeleteEntities<TEntity>(IEnumerable<TEntity> entities) where TEntity : IEntity {
			if (entities == null) throw new ArgumentNullException();
			foreach (IEntity e in entities) return CanUndeleteEntity(e);
			return false;
		}


		private string description;
		private IRepository repository;

	}


	/// <summary>
	/// Base class for shape specific commands
	/// </summary>
	public abstract class ShapeCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		protected ShapeCommand(Shape shape)
			: this(null, shape) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected ShapeCommand(IRepository repository, Shape shape)
			: base(repository) {
			if (shape == null) throw new ArgumentNullException("shape");
			this.Shape = shape;
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, SecurityAccess.Modify, Shape);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Shape Shape { get; set; }

	}


	/// <summary>
	/// Base class for shape specific commands
	/// </summary>
	public abstract class ShapesCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		protected ShapesCommand(IEnumerable<Shape> shapes)
			: this(null, shapes) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected ShapesCommand(IRepository repository, IEnumerable<Shape> shapes)
			: base(repository) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.Shapes = new List<Shape>(shapes);
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, Shapes);
			exception = (!isGranted && createException) ? exception = new NShapeSecurityException(this) : null;
			return isGranted;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected List<Shape> Shapes { get; set; }

	}


	/// <summary>
	/// Base class for template specific commands
	/// </summary>
	public abstract class TemplateCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		protected TemplateCommand(IRepository repository)
			: base(repository) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected TemplateCommand(Template template)
			: this(null, template) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected TemplateCommand(IRepository repository, Template template)
			: base(repository) {
			if (template == null) throw new ArgumentNullException("template");
			this.Template = template;
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Templates; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("template");
			bool isGranted = securityManager.IsGranted(RequiredPermission);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Template Template { get; set; }

	}


	/// <summary>
	/// Base class for diagram specific commands
	/// </summary>
	public abstract class DiagramCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		protected DiagramCommand(Diagram diagram)
			: this(null, diagram) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected DiagramCommand(IRepository repository, Diagram diagram)
			: base(repository) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this.Diagram = diagram;
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, Diagram.SecurityDomainName);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Diagram Diagram { get; set; }

	}


	/// <summary>
	/// Base class for design specific commands
	/// </summary>
	public abstract class DesignCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		protected DesignCommand(Design design)
			: this(null, design) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected DesignCommand(IRepository repository, Design design)
			: base(repository) {
			if (design == null) throw new ArgumentNullException("design");
			this.Design = design;
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Designs; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Design Design { get; set; }

	}


	/// <summary>
	/// Base class for commands that need to disconnect connected shapes before the action may executed,
	/// e.g. a DeleteCommand has to disconnect the deleted shape before deleting it.
	/// </summary>
	public abstract class AutoDisconnectShapesCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		protected AutoDisconnectShapesCommand()
			: this(null) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected AutoDisconnectShapesCommand(IRepository repository)
			: base(repository) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void Disconnect(IList<Shape> shapes) {
			foreach (Shape shape in shapes)
			    Disconnect(shape);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void Disconnect(Shape shape) {
			// Check whether the shape has already been added (this is the case when re-doing the command)
			if (!connections.ContainsKey(shape))
				connections.Add(shape, new List<ShapeConnectionInfo>(shape.GetConnectionInfos(ControlPointId.Any, null)));
			// Disconnect shapes (if necessary)
			foreach (ShapeConnectionInfo sci in connections[shape]) {
				if (shape.IsConnected(sci.OwnPointId, sci.OtherShape) == ControlPointId.None)
					continue;
				if (shape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue)) {
					shape.Disconnect(sci.OwnPointId);
					Repository.DeleteConnection(shape, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
				} else {
					sci.OtherShape.Disconnect(sci.OtherPointId);
					Repository.DeleteConnection(sci.OtherShape, sci.OtherPointId, shape, sci.OwnPointId);
				}
				sci.OtherShape.Invalidate();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void Reconnect(IList<Shape> shapes) {
			// restore connections
			foreach (Shape shape in shapes)
				Reconnect(shape);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void Reconnect(Shape shape) {
			// restore connections
			if (connections.ContainsKey(shape)) {
				foreach (ShapeConnectionInfo sci in connections[shape]) {
					if (shape is ILinearShape) {
						shape.Connect(sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
						if (Repository != null) Repository.InsertConnection(shape, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
					} else {
						sci.OtherShape.Connect(sci.OtherPointId, shape, sci.OwnPointId);
						if (Repository != null) Repository.InsertConnection(sci.OtherShape, sci.OtherPointId, shape, sci.OwnPointId);
					}
				}
			} 
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Dictionary<Shape, List<ShapeConnectionInfo>> ShapeConnections { get { return connections; } }


		private Dictionary<Shape, List<ShapeConnectionInfo>> connections = new Dictionary<Shape, List<ShapeConnectionInfo>>();

	}


	///// <summary>
	///// Base class for inserting and removing shapes to a diagram and a cache
	///// </summary>
	//public abstract class InsertOrRemoveShapeCommand : AutoDisconnectShapesCommand {

	//   protected InsertOrRemoveShapeCommand(Diagram diagram)
	//      : base() {
	//      if (diagram == null) throw new ArgumentNullException("diagram");
	//      this.diagram = diagram;
	//   }


	//   protected void InsertShapes(LayerIds activeLayers) {
	//      DoInsertShapes(false, activeLayers);
	//   }


	//   protected void InsertShapes() {
	//      DoInsertShapes(true, LayerIds.None);
	//   }


	//   protected void RemoveShapes() {
	//      if (Shapes.Count == 0) throw new NShapeInternalException("No shapes set. Call SetShapes() before.");

	//      // disconnect all selectedShapes connected to the deleted shape(s)
	//      Disconnect(Shapes);

	//      if (Shapes.Count > 1) {
	//         diagram.Shapes.RemoveRange(Shapes);
	//         if (Repository != null) Repository.DeleteShapes(Shapes);
	//      } else {
	//         diagram.Shapes.Remove(Shapes[0]);
	//         if (Repository != null) Repository.DeleteShape(Shapes[0]);
	//      }
	//   }


	//   protected void SetShapes(Shape shape) {
	//      if (shape == null) throw new ArgumentNullException("shape");
	//      if (shapeLayers == null) shapeLayers = new List<LayerIds>(1);
	//      else this.shapeLayers.Clear();
	//      this.Shapes.Clear();

	//      if (!this.Shapes.Contains(shape)) {
	//         this.Shapes.Add(shape);
	//         this.shapeLayers.Add(shape.Layers);
	//      }
	//   }


	//   protected void SetShapes(IEnumerable<Shape> shapes) {
	//      SetShapes(shapes, true);
	//   }


	//   protected void SetShapes(IEnumerable<Shape> shapes, bool invertSortOrder) {
	//      if (shapes == null) throw new ArgumentNullException("shapes");
	//      this.Shapes.Clear();
	//      if (shapeLayers == null) shapeLayers = new List<LayerIds>();
	//      else this.shapeLayers.Clear();

	//      foreach (Shape shape in shapes) {
	//         if (!this.Shapes.Contains(shape)) {
	//            if (invertSortOrder) {
	//               this.Shapes.Insert(0, shape);
	//               this.shapeLayers.Insert(0, shape.Layers);
	//            } else {
	//               this.Shapes.Add(shape);
	//               this.shapeLayers.Add(shape.Layers);
	//            }
	//         }
	//      }
	//   }


	//   protected List<Shape> Shapes = new List<Shape>();


	//   protected static string DeleteDescription = "Delete {0} shape{1}";


	//   protected static string CreateDescription = "Create {0} shape{1}";


	//   private void DoInsertShapes(bool useOriginalLayers, LayerIds activeLayers) {
	//      int startIdx = Shapes.Count - 1;
	//      if (startIdx < 0) throw new NShapeInternalException("No shapes set. Call SetShapes() before.");

	//      if (Repository == null) throw new ArgumentNullException("Repository"); 
	//      for (int i = startIdx; i >= 0; --i) {
	//         //if (Shapes[i].ZOrder == 0) 
	//            Shapes[i].ZOrder = Repository.ObtainNewTopZOrder(diagram);
	//         diagram.Shapes.Add(Shapes[i]);
	//         diagram.AddShapeToLayers(Shapes[i], useOriginalLayers ? shapeLayers[i] : activeLayers);
	//      }
	//      if (startIdx == 0)
	//         Repository.InsertShape(Shapes[0], diagram);
	//      else
	//         Repository.InsertShapes(Shapes, diagram);

	//      // connect all selectedShapes that were previously connected to the shape(s)
	//      Reconnect(Shapes);
	//   }


	//   private Diagram diagram = null;
	//   private List<LayerIds> shapeLayers;
	//}


	/// <summary>Base class for inserting and removing layers to a diagram.</summary>
	public abstract class InsertOrRemoveLayerCommand : DiagramCommand {

		/// <ToBeCompleted></ToBeCompleted>
		protected InsertOrRemoveLayerCommand(Diagram diagram, string layerName)
			: this(null, diagram, layerName) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected InsertOrRemoveLayerCommand(IRepository repository, Diagram diagram, string layerName)
			: base(repository, diagram) {
			if (layerName == null) throw new ArgumentNullException("layerName");
			Layers = new List<Layer>(1);
			Layer l = this.Diagram.Layers.FindLayer(layerName);
			if (l == null) l = new Layer(layerName);
			Layers.Add(l);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected InsertOrRemoveLayerCommand(Diagram diagram, Layer layer)
			: this(null, diagram, layer) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected InsertOrRemoveLayerCommand(IRepository repository, Diagram diagram, Layer layer)
			: this(repository, diagram, SingleInstanceEnumerator<Layer>.Create(layer)) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected InsertOrRemoveLayerCommand(Diagram diagram, IEnumerable<Layer> layers)
			: this(null, diagram, layers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected InsertOrRemoveLayerCommand(IRepository repository, Diagram diagram, IEnumerable<Layer> layers)
			: base(repository, diagram) {
			this.Layers = new List<Layer>(layers);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void AddLayers() {
			for (int i = 0; i < Layers.Count; ++i)
				Diagram.Layers.Add(Layers[i]);
			if (Repository != null) Repository.Update(Diagram);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void RemoveLayers() {
			for (int i = 0; i < Layers.Count; ++i)
				Diagram.Layers.Remove(Layers[i]);
			if (Repository != null) Repository.Update(Diagram);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected List<Layer> Layers { get; set; }

	}


	/// <summary>Base class for inserting and removing model into a model and a repository.</summary>
	public abstract class InsertOrRemoveModelObjectsCommand : Command {

		/// <override></override>
		protected InsertOrRemoveModelObjectsCommand()
			: this(null) {
		}


		/// <override></override>
		protected InsertOrRemoveModelObjectsCommand(IRepository repository)
			: base(repository) {
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = true;
			foreach (KeyValuePair<IModelObject, AttachedObjects> pair in ModelObjects) {
				if (!IsAllowed(securityManager, pair.Value)) {
					isGranted = false;
					break;
				}
			}
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void SetModelObjects(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			Debug.Assert(this.ModelObjects.Count == 0);
			ModelObjects.Add(modelObject, new AttachedObjects(modelObject, Repository));
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void SetModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			Debug.Assert(this.ModelObjects.Count == 0);
			foreach (IModelObject modelObject in modelObjects)
				ModelObjects.Add(modelObject, new AttachedObjects(modelObject, Repository));
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void InsertModelObjects(bool insertShapes) {
			int cnt = ModelObjects.Count;
			if (cnt == 0) throw new NShapeInternalException("No ModelObjects set. Call SetModelObjects() before.");
			if (Repository != null) {
				if (CanUndeleteEntities(ModelObjects.Keys))
					Repository.Undelete(ModelObjects.Keys);
				else Repository.Insert(ModelObjects.Keys);
				foreach (KeyValuePair<IModelObject, AttachedObjects> item in ModelObjects)
					InsertAndAttachObjects(item.Key, item.Value, Repository, insertShapes);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void RemoveModelObjects(bool deleteShapes) {
			if (ModelObjects.Count == 0) throw new NShapeInternalException("No ModelObjects set. Call SetModelObjects() before.");
			if (Repository != null) {
				foreach (KeyValuePair<IModelObject, AttachedObjects> item in ModelObjects)
					DetachAndDeleteObjects(item.Value, Repository, deleteShapes);
				Repository.Delete(ModelObjects.Keys);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		internal Dictionary<IModelObject, AttachedObjects> ModelObjects = new Dictionary<IModelObject, AttachedObjects>();


		private void DetachAndDeleteObjects(AttachedObjects attachedObjects, IRepository repository, bool deleteShapes) {
			// Delete or detach shapes
			if (attachedObjects.Shapes.Count > 0) {
				if (deleteShapes) repository.DeleteAll(attachedObjects.Shapes);
				else {
					for (int sIdx = attachedObjects.Shapes.Count - 1; sIdx >= 0; --sIdx)
						attachedObjects.Shapes[sIdx].ModelObject = null;
					repository.Update(attachedObjects.Shapes);
				}
			}
			// Process children
			foreach (KeyValuePair<IModelObject, AttachedObjects> child in attachedObjects.Children)
				DetachAndDeleteObjects(child.Value, repository, deleteShapes);
			// Delete model object
			repository.Delete(attachedObjects.Children.Keys);
		}


		private void InsertAndAttachObjects(IModelObject modelObject, AttachedObjects attachedObjects, IRepository repository, bool insertShapes) {
			// Insert model objects
			if (CanUndeleteEntities(attachedObjects.Children.Keys))
				repository.Undelete(attachedObjects.Children.Keys);
			else repository.Insert(attachedObjects.Children.Keys);
			foreach (KeyValuePair<IModelObject, AttachedObjects> child in attachedObjects.Children) {
				InsertAndAttachObjects(child.Key, child.Value, repository, insertShapes);
				child.Key.Parent = modelObject;
			}
			repository.Update(attachedObjects.Children.Keys);
			// insert shapes
			if (attachedObjects.Shapes.Count > 0) {
				for (int sIdx = attachedObjects.Shapes.Count - 1; sIdx >= 0; --sIdx)
				    attachedObjects.Shapes[sIdx].ModelObject = modelObject;
				if (insertShapes)
					throw new NotImplementedException();
				else repository.Update(attachedObjects.Shapes);
			}
		}


		private bool IsAllowed(ISecurityManager securityManager, AttachedObjects attachedObjects) {
			if (attachedObjects == null) return true;
			foreach (AttachedObjects obj in attachedObjects.Children.Values) {
				if (!IsAllowed(securityManager, obj))
					return false;
			}
			return securityManager.IsGranted(RequiredPermission, attachedObjects.Shapes);
		}

	}


	/// <ToBeCompleted></ToBeCompleted>
	internal class AttachedObjects {

		/// <ToBeCompleted></ToBeCompleted>
		public AttachedObjects(IModelObject modelObject, IRepository repository) {
			shapes = new List<Shape>();
			children = new Dictionary<IModelObject, AttachedObjects>();
			Add(modelObject, repository);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public List<Shape> Shapes {
			get { return shapes; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Dictionary<IModelObject, AttachedObjects> Children {
			get { return children; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Add(IModelObject modelObject, IRepository repository) {
			DoAdd(this, modelObject, repository);
		}


		private void DoAdd(AttachedObjects attachedObjects, IModelObject modelObject, IRepository repository) {
			attachedObjects.Shapes.AddRange(modelObject.Shapes);
			foreach (IModelObject child in repository.GetModelObjects(modelObject))
				attachedObjects.Children.Add(child, new AttachedObjects(child, repository));
		}


		private List<Shape> shapes;
		private Dictionary<IModelObject, AttachedObjects> children;
	}


	/// <summary>
	/// Base class for inserting and removing shapes along with their model objects to a diagram and a repository.
	/// </summary>
	public abstract class InsertOrRemoveShapeCommand : AutoDisconnectShapesCommand {

		/// <ToBeCompleted></ToBeCompleted>
		protected enum SortOrder {
			/// <ToBeCompleted></ToBeCompleted>
			Unspecified,
			/// <ToBeCompleted></ToBeCompleted>
			TopDown,
			/// <ToBeCompleted></ToBeCompleted>
			BottomUp
		};


		/// <ToBeCompleted></ToBeCompleted>
		protected SortOrder GetCurrentSortOrder(IEnumerable<Shape> shapes) {
			SortOrder result = SortOrder.Unspecified;
			int lastZOrder = int.MinValue;
			foreach (Shape s in shapes) {
				if (lastZOrder == int.MinValue)
					lastZOrder = s.ZOrder;
				else {
					if (s.ZOrder == lastZOrder)
						continue;
					else if (s.ZOrder < lastZOrder)
						result = SortOrder.TopDown;
					else if (s.ZOrder > lastZOrder)
						result = SortOrder.BottomUp;
					break;
				}
			}
			if (result == SortOrder.Unspecified) 
				result = SortOrder.TopDown;
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected InsertOrRemoveShapeCommand(Diagram diagram)
			: this(null, diagram) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected InsertOrRemoveShapeCommand(IRepository repository, Diagram diagram)
			: base(repository) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this._diagram = diagram;
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, _shapes);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use InsertShapesAndModels(int homeLayer, LayerIds supplementalLayers) instead.")]
		protected void InsertShapesAndModels(LayerIds activeLayers) {
			InsertShapesAndModels(Layer.NoLayerId, activeLayers);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void InsertShapesAndModels(int homeLayer, LayerIds supplementalLayers) {
			DoInsertShapesAndModels(false, homeLayer, supplementalLayers);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void InsertShapesAndModels() {
			DoInsertShapesAndModels(true, Layer.NoLayerId, LayerIds.None);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void DeleteShapesAndModels() {
			if (Shapes.Count == 0) throw new NShapeInternalException("No shapes set. Call SetShapes() before.");
			if (Repository != null && ModelObjects != null && ModelObjects.Count > 0) {
				if (_modelsAndObjects == null) {
					_modelsAndObjects = new Dictionary<IModelObject, AttachedObjects>();
					foreach (IModelObject modelObject in ModelObjects)
						_modelsAndObjects.Add(modelObject, new AttachedObjects(modelObject, Repository));
				}
				foreach (KeyValuePair<IModelObject, AttachedObjects> item in _modelsAndObjects)
					DetachAndDeleteObjects(item.Value, Repository);
				Repository.Delete(_modelsAndObjects.Keys);
			}
			_diagram.Shapes.RemoveRange(Shapes);
			if (Repository != null) Repository.DeleteAll(Shapes);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void SetShape(Shape shape, bool withModelObject) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (_shapeLayers == null) _shapeLayers = new List<LayerInfo>(1);
			else this._shapeLayers.Clear();
			this.Shapes.Clear();

			if (!this.Shapes.Contains(shape)) {
				this.Shapes.Add(shape);
				this._shapeLayers.Add(LayerInfo.Create(shape.HomeLayer, shape.SupplementalLayers));

				if (shape.ModelObject != null && withModelObject)
					SetModelObject(shape.ModelObject);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void SetShapes(IEnumerable<Shape> shapes, SortOrder currentSortOrder, bool withModelObjects) {
			SetShapes(shapes, withModelObjects, currentSortOrder, false);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void SetShapes(IEnumerable<Shape> shapes, bool withModelObjects, SortOrder currentSortOrder, bool invertSortOrder) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.Shapes.Clear();
			if (this._shapeLayers == null) _shapeLayers = new List<LayerInfo>();
			else this._shapeLayers.Clear();

			foreach (Shape shape in shapes) {
				if (!this.Shapes.Contains(shape)) {
					LayerInfo layerInfo = LayerInfo.Create(shape.HomeLayer, shape.SupplementalLayers);
					if (currentSortOrder == SortOrder.BottomUp || (currentSortOrder == SortOrder.TopDown && invertSortOrder)) {
						this.Shapes.Insert(0, shape);
						this._shapeLayers.Insert(0, layerInfo);
					} else {
						Debug.Assert(currentSortOrder == SortOrder.TopDown || (currentSortOrder == SortOrder.BottomUp && invertSortOrder));
						this.Shapes.Add(shape);
						this._shapeLayers.Add(layerInfo);
					}
					if (shape.ModelObject != null && withModelObjects)
						SetModelObject(shape.ModelObject);
				}
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void SetModelObject(IModelObject modelObject) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			if (this._modelObjects == null) this._modelObjects = new List<IModelObject>();
			if (!this._modelObjects.Contains(modelObject)) this._modelObjects.Add(modelObject);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void SetModelObjects(IEnumerable<IModelObject> modelObjects) {
			if (modelObjects == null) throw new ArgumentNullException("modelObjects");
			if (this._modelObjects == null) this._modelObjects = new List<IModelObject>();
			foreach (IModelObject modelObject in modelObjects) {
				if (!this._modelObjects.Contains(modelObject))
					this._modelObjects.Add(modelObject);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void SetModelObjects(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			foreach (Shape shape in shapes)
				if (shape.ModelObject != null) SetModelObject(shape.ModelObject);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected List<Shape> Shapes {
			get { return _shapes; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected List<IModelObject> ModelObjects {
			get { return _modelObjects; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected string GetDescription(DescriptionType descType, Shape shape, bool withModelObject) {
			string modelString = string.Empty;
			if (withModelObject && shape.ModelObject != null)
				modelString = string.Format(WithModelsFormatStr, string.Empty, " " + shape.ModelObject.Type.Name);
			return string.Format(DescriptionFormatStr,
				GetDescTypeText(descType),
				shape.Type.Name,
				string.Empty,
				modelString);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected string GetDescription(DescriptionType descType, IEnumerable<Shape> shapes, bool withModelObjects) {
			int shapeCnt = 0, modelCnt = 0;
			foreach (Shape s in shapes) {
				++shapeCnt;
				if (s.ModelObject != null) ++modelCnt;
			}
			if (shapeCnt == 1)
				foreach (Shape s in shapes)
					return GetDescription(descType, s, withModelObjects);

			string modelString = string.Empty;
			if (withModelObjects && modelCnt > 0)
				modelString = string.Format(WithModelsFormatStr, modelCnt.ToString() + " ", modelCnt > 1 ? Dataweb.NShape.Properties.Resources.Text_PluralEndingModel : string.Empty);
			return string.Format(DescriptionFormatStr,
				GetDescTypeText(descType),
				shapeCnt,
				Dataweb.NShape.Properties.Resources.Text_PluralEndingShape,
				modelString);
		}


		private string GetDescTypeText(DescriptionType descType) {
			switch (descType) {
				case DescriptionType.Insert: return "Create";
				case DescriptionType.Delete: return "Delete";
				case DescriptionType.Cut: return "Cut";
				case DescriptionType.Paste: return "Paste";
				default:
					Debug.Fail("Unhandled switch case!");
					return descType.ToString();
			}
		}


		// Insert shapes only
		private void DoInsertShapes(bool useOriginalLayers, int homeLayer, LayerIds supplementalLayers) {
			if (Shapes.Count == 0) throw new NShapeInternalException("No shapes set. Call SetShapes() before.");
			for (int i = Shapes.Count - 1; i >= 0; --i) {
				//Shapes[i].ZOrder = Repository.ObtainNewTopZOrder(diagram);
				_diagram.Shapes.Add(Shapes[i]);
				_diagram.AddShapeToLayers(Shapes[i], 
					useOriginalLayers ? _shapeLayers[i].HomeLayer : homeLayer, 
					useOriginalLayers ? _shapeLayers[i].SupplementalLayers : supplementalLayers
				);
			}
			if (Repository != null) {
				// Insert shapes
				Repository.UndeleteAll(GetEntities<Shape>(Shapes, CanUndeleteEntity), _diagram);
				Repository.InsertAll(GetEntities<Shape>(Shapes, CanInsertEntity), _diagram);
			}
		}


		private IEnumerable<TEntity> GetEntities<TEntity>(IEnumerable<TEntity> entities, Predicate<TEntity> predicate) where TEntity : IEntity {
			foreach (TEntity entity in entities)
				if (predicate(entity)) yield return entity;
		}


		private void DoInsertShapesAndModels(bool useOriginalLayers, int homeLayer, LayerIds supplementalLayers) {
			// Insert model objects first
			if (Repository != null && ModelObjects != null && ModelObjects.Count > 0) {
				if (_modelsAndObjects == null) {
					_modelsAndObjects = new Dictionary<IModelObject, AttachedObjects>();
					foreach (IModelObject modelObject in ModelObjects)
						_modelsAndObjects.Add(modelObject, new AttachedObjects(modelObject, Repository));
				}
				if (CanUndeleteEntities(_modelsAndObjects.Keys))
					Repository.Undelete(_modelsAndObjects.Keys);
				else Repository.Insert(_modelsAndObjects.Keys);
			}
			// Insert shapes afterwards
			if (useOriginalLayers) 
				DoInsertShapes(true, Layer.NoLayerId, LayerIds.None);
			else 
				DoInsertShapes(false, homeLayer, supplementalLayers);
			// Attach model objects to shapes finally
			if (Repository != null && _modelsAndObjects != null) {
				foreach (KeyValuePair<IModelObject, AttachedObjects> item in _modelsAndObjects)
					InsertAndAttachObjects(item.Key, item.Value, Repository);
			}
		}


		private void DetachAndDeleteObjects(AttachedObjects attachedObjects, IRepository repository) {
			for (int sIdx = attachedObjects.Shapes.Count - 1; sIdx >= 0; --sIdx)
				attachedObjects.Shapes[sIdx].ModelObject = null;
			repository.Update(attachedObjects.Shapes);
			foreach (KeyValuePair<IModelObject, AttachedObjects> child in attachedObjects.Children)
				DetachAndDeleteObjects(child.Value, repository);
			repository.Delete(attachedObjects.Children.Keys);
		}


		private void InsertAndAttachObjects(IModelObject modelObject, AttachedObjects attachedObjects, IRepository repository) {
			if (CanUndeleteEntities(attachedObjects.Children.Keys))
				repository.Undelete(attachedObjects.Children.Keys);
			else repository.Insert(attachedObjects.Children.Keys);
			foreach (KeyValuePair<IModelObject, AttachedObjects> child in attachedObjects.Children) {
				InsertAndAttachObjects(child.Key, child.Value, repository);
				child.Key.Parent = modelObject;
			}
			repository.Update(attachedObjects.Children.Keys);
			for (int sIdx = attachedObjects.Shapes.Count - 1; sIdx >= 0; --sIdx)
				attachedObjects.Shapes[sIdx].ModelObject = modelObject;
			repository.Update(attachedObjects.Shapes);
			repository.Update(modelObject);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected enum DescriptionType {
			/// <ToBeCompleted></ToBeCompleted>
			Insert,
			/// <ToBeCompleted></ToBeCompleted>
			Delete,
			/// <ToBeCompleted></ToBeCompleted>
			Cut,
			/// <ToBeCompleted></ToBeCompleted>
			Paste
		};


		private static readonly string DescriptionFormatStr = Dataweb.NShape.Properties.Resources.MessageFmt_DescTxt0ShapeType1Shape23;
		private static readonly string WithModelsFormatStr = Dataweb.NShape.Properties.Resources.MessageFmt_With0Model1;

		private Diagram _diagram = null;
		private List<Shape> _shapes = new List<Shape>();
		private List<LayerInfo> _shapeLayers;
		private List<IModelObject> _modelObjects = null;
		private Dictionary<IModelObject, AttachedObjects> _modelsAndObjects = null;
	}


	/// <summary>
	/// Base class for Connecting and disconnecting two shapes
	/// </summary>
	public abstract class ConnectionCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		protected ConnectionCommand(Shape connectorShape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId)
			: this(null, connectorShape, gluePointId, targetShape, targetPointId) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected ConnectionCommand(IRepository repository, Shape connectorShape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId)
			: base(repository) {
			if (connectorShape == null) throw new ArgumentNullException("connectorShape");
			if (targetShape == null) throw new ArgumentNullException("targetShape");
			this.ConnectorShape = connectorShape;
			this.GluePointId = gluePointId;
			this.TargetShape = targetShape;
			this.TargetPointId = targetPointId;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected ConnectionCommand(Shape connectorShape, ControlPointId gluePointId)
			: this(null, connectorShape, gluePointId) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected ConnectionCommand(IRepository repository, Shape connectorShape, ControlPointId gluePointId)
			: base(repository) {
			if (connectorShape == null) throw new ArgumentNullException("connectorShape");
			this.ConnectorShape = connectorShape;
			this.GluePointId = gluePointId;
			this.TargetShape = null;
			this.TargetPointId = ControlPointId.None;

			ShapeConnectionInfo sci = connectorShape.GetConnectionInfo(gluePointId, null);
			if (sci.IsEmpty) throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageFmt_GluePoint0IsNotConnected, gluePointId);
			this.TargetShape = sci.OtherShape;
			this.TargetPointId = sci.OtherPointId;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void Connect() {
			ConnectorShape.Connect(GluePointId, TargetShape, TargetPointId);
			if (Repository != null)
				Repository.InsertConnection(ConnectorShape, GluePointId, TargetShape, TargetPointId);
			ConnectorShape.Invalidate();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void Disconnect() {
			ConnectorShape.Disconnect(GluePointId);
			if (Repository != null)
				Repository.DeleteConnection(ConnectorShape, GluePointId, TargetShape, TargetPointId);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Connect; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, ConnectorShape);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Shape ConnectorShape { get; set; }

		/// <ToBeCompleted></ToBeCompleted>
		protected Shape TargetShape { get; set; }

		/// <ToBeCompleted></ToBeCompleted>
		protected ControlPointId GluePointId { get; set; }

		/// <ToBeCompleted></ToBeCompleted>
		protected ControlPointId TargetPointId { get; set; }

	}


	/// <summary>
	/// Base class for (un)aggregating shapes in shape aggregations.
	/// </summary>
	public abstract class ShapeAggregationCommand : AutoDisconnectShapesCommand {

		/// <ToBeCompleted></ToBeCompleted>
		protected ShapeAggregationCommand(Diagram diagram, Shape aggregationShape, IEnumerable<Shape> shapes)
			: this(null, diagram, aggregationShape, shapes) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected ShapeAggregationCommand(IRepository repository, Diagram diagram, Shape aggregationShape, IEnumerable<Shape> shapes)
			: this(repository, diagram, aggregationShape, shapes, Layer.NoLayerId, LayerIds.None) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected ShapeAggregationCommand(IRepository repository, Diagram diagram, Shape aggregationShape, IEnumerable<Shape> shapes, LayerIds aggregationLayerIds)
			: base(repository) {
			Construct(diagram, aggregationShape, shapes, Layer.NoLayerId, aggregationLayerIds);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected ShapeAggregationCommand(IRepository repository, Diagram diagram, Shape aggregationShape, IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers)
			: base(repository) {
			Construct(diagram, aggregationShape, shapes, homeLayer, supplementalLayers);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void Construct(Diagram diagram, Shape aggregationShape, IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (aggregationShape == null) throw new ArgumentNullException("aggregationShape");
			if (shapes == null) throw new ArgumentNullException("shapes");
			this.Diagram = diagram;
			this.AggregationShape = aggregationShape;
			this.AggregationShapeOwnedByDiagram = diagram.Shapes.Contains(aggregationShape);
			this.Shapes = new List<Shape>(shapes);
			this.AggregationLayerInfo = LayerInfo.Create(homeLayer, supplementalLayers);
			//
			// Store original layer ids of shapes
			if (homeLayer != Layer.NoLayerId || supplementalLayers != LayerIds.None) {
				ShapeLayerInfos = new List<LayerInfo>(Shapes.Count);
				for (int i = 0; i < Shapes.Count; ++i) {
					Shape shape = Shapes[i];
					ShapeLayerInfos.Add(LayerInfo.Create(shape.HomeLayer, shape.SupplementalLayers));
				}
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void CreateShapeAggregation(bool maintainZOrders) {
			// Add aggregation shape to diagram
			if (!AggregationShapeOwnedByDiagram) {
				Diagram.Shapes.Add(AggregationShape);
				Diagram.AddShapeToLayers(AggregationShape, AggregationLayerInfo.HomeLayer, AggregationLayerInfo.SupplementalLayers);
			}
			// Insert aggregation shape to repository (if necessary)
			if (Repository != null) {
				if (!AggregationShapeOwnedByDiagram) {
					if (CanUndeleteEntity(AggregationShape))
						Repository.UndeleteAll(AggregationShape, Diagram);
					else Repository.InsertAll(AggregationShape, Diagram);
				}
			}

			// Remove shapes from diagram
			Diagram.Shapes.RemoveRange(Shapes);
			// Add Shapes to aggregation shape
			if (maintainZOrders) {
				int cnt = Shapes.Count;
				for (int i = 0; i < cnt; ++i)
					AggregationShape.Children.Add(Shapes[i], Shapes[i].ZOrder);
			} else AggregationShape.Children.AddRange(Shapes);
			Diagram.AddShapeToLayers(AggregationShape, AggregationLayerInfo.HomeLayer, AggregationLayerInfo.SupplementalLayers);

			// Finally, update the child shape's owner
			if (Repository != null) {
				foreach (Shape childShape in AggregationShape.Children)
					Repository.UpdateOwner(childShape, AggregationShape);
				if (AggregationShapeOwnedByDiagram)
					Repository.Update(AggregationShape);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void DeleteShapeAggregation() {
			// Update the child shape's owner
			if (Repository != null) {
				foreach (Shape childShape in AggregationShape.Children)
					Repository.UpdateOwner(childShape, Diagram);
			}

			// Move the shapes to their initial owner
			AggregationShape.Children.RemoveRange(Shapes);
			foreach (Shape childShape in AggregationShape.Children)
				AggregationShape.Children.Remove(childShape);
			// If the aggregation shape was not initialy part of the diagram, remove it.
			if (!AggregationShapeOwnedByDiagram)
				Diagram.Shapes.Remove(AggregationShape);
			int cnt = Shapes.Count;
			if (ZOrders != null) {
				for (int i = 0; i < cnt; ++i)
					Diagram.Shapes.Add(Shapes[i], ZOrders[i]);
			} else
				Diagram.Shapes.AddRange(Shapes);
			// Restore original layer assignment
			for (int i = 0; i < cnt; ++i) {
				Diagram.RemoveShapeFromLayers(Shapes[i]);
				if (ShapeLayerInfos != null)
					Diagram.AddShapeToLayers(Shapes[i], ShapeLayerInfos[i].HomeLayer, ShapeLayerInfos[i].SupplementalLayers);
			}

			// Delete shapes from repository
			if (Repository != null) {
				// If the aggregation shape was not initialy part of the diagram, remove it.
				if (!AggregationShapeOwnedByDiagram)
					// Shape aggregations are deleted with all their children
					Repository.DeleteAll(AggregationShape);
				else Repository.Update(AggregationShape);
			}
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = (securityManager.IsGranted(Permission.Insert | Permission.Delete, Shapes)
							&& securityManager.IsGranted(RequiredPermission, AggregationShape));
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Diagram Diagram { get; set; }
		/// <ToBeCompleted></ToBeCompleted>
		protected List<Shape> Shapes { get; set; }
		/// <ToBeCompleted></ToBeCompleted>
		protected List<LayerInfo> ShapeLayerInfos { get; set; }
		/// <ToBeCompleted></ToBeCompleted>
		protected List<int> ZOrders { get; set; }
		/// <ToBeCompleted></ToBeCompleted>
		protected Shape AggregationShape { get; set; }
		/// <ToBeCompleted></ToBeCompleted>
		protected LayerInfo AggregationLayerInfo { get; set; }
		// Specifies if the aggreagtion shape initialy was owned by the diagram
		/// <ToBeCompleted></ToBeCompleted>
		protected bool AggregationShapeOwnedByDiagram { get; set; }

	}


	/// <summary>Base class for commands that change properties of objects.</summary>
	public abstract class PropertySetCommand<T> : Command {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.PropertySetCommand`1" />.
		/// </summary>
		public PropertySetCommand(T modifiedObject, PropertyInfo propertyInfo, object oldValue, object newValue)
			: this(null, modifiedObject, propertyInfo, oldValue, newValue) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.PropertySetCommand`1" />.
		/// </summary>
		public PropertySetCommand(IRepository repository, T modifiedObject, PropertyInfo propertyInfo, object oldValue, object newValue)
			: this(null, SingleInstanceEnumerator<T>.Create(modifiedObject), propertyInfo, SingleInstanceEnumerator<object>.Create(oldValue), SingleInstanceEnumerator<object>.Create(newValue)) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.PropertySetCommand`1" />.
		/// </summary>
		public PropertySetCommand(IEnumerable<T> modifiedObjects, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: this(null, modifiedObjects, propertyInfo, oldValues, newValue) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.PropertySetCommand`1" />.
		/// </summary>
		public PropertySetCommand(IRepository repository, IEnumerable<T> modifiedObjects, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: this(repository, modifiedObjects, propertyInfo, oldValues, SingleInstanceEnumerator<object>.Create(newValue)) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.PropertySetCommand`1" />.
		/// </summary>
		public PropertySetCommand(IEnumerable<T> modifiedObjects, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: this(null, modifiedObjects, propertyInfo, oldValues, newValues) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.PropertySetCommand`1" />.
		/// </summary>
		public PropertySetCommand(IRepository repository, IEnumerable<T> modifiedObjects, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base() {
			if (modifiedObjects == null) throw new ArgumentNullException("modifiedObjects");
			Construct(propertyInfo);
			this.ModifiedObjects = new List<T>(modifiedObjects);
			this.OldValues = new List<object>(oldValues);
			this.NewValues = new List<object>(newValues);
		}


		/// <override></override>
		public override void Execute() {
			int valCnt = NewValues.Count;
			int objCnt = ModifiedObjects.Count;
			for (int i = 0; i < objCnt; ++i) {
				object newValue = NewValues[(valCnt == objCnt) ? i : 0];
				object currValue = PropertyInfo.GetValue(ModifiedObjects[i], null);
				// Check if the new value has been set already (e.g. by the PropertyGrid). If the new value is 
				// already set, skip setting the new value (again).
				// This check is necessary because if the value is a value that is exclusive-or'ed when set 
				// (e.g. a FontStyle), the change would be undone when setting the value again
				if (currValue == null && newValue != null
					|| currValue != null && newValue == null
					|| (currValue != null && !currValue.Equals(newValue))) {
					PropertyInfo.SetValue(ModifiedObjects[i], newValue, null);
				}
			}
		}


		/// <override></override>
		public override void Revert() {
			int valCnt = OldValues.Count;
			int objCnt = ModifiedObjects.Count;
			for (int i = 0; i < objCnt; ++i) {
				object oldValue = OldValues[(valCnt == objCnt) ? i : 0];
				object currValue = PropertyInfo.GetValue(ModifiedObjects[i], null);
				if (currValue == null && oldValue != null
					|| (currValue != null && !currValue.Equals(oldValue)))
					PropertyInfo.SetValue(ModifiedObjects[i], oldValue, null);
			}
		}


		/// <override></override>
		public override string Description {
			get {
				if (string.IsNullOrEmpty(base.Description)) {
					if (ModifiedObjects.Count == 1)
						base.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_ChangeProperty0Of1From2To3,
							PropertyInfo.Name, ModifiedObjects[0].GetType().Name, OldValues[0], NewValues[0]);
					else {
						if (OldValues.Count == 1 && NewValues.Count == 1) {
							base.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_ChangeProperty0Of123From4To5,
								PropertyInfo.Name,
								this.ModifiedObjects.Count,
								this.ModifiedObjects[0].GetType().Name,
								this.ModifiedObjects.Count > 1 ? Dataweb.NShape.Properties.Resources.Text_PluralEndingUnspecific : string.Empty,
								OldValues[0],
								NewValues[0]);
						} else if (OldValues.Count > 1 && NewValues.Count == 1) {
							base.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageTxt_ChangeProperty0Of123To4,
								PropertyInfo.Name,
								this.ModifiedObjects.Count,
								this.ModifiedObjects[0].GetType().Name,
								this.ModifiedObjects.Count > 1 ? Dataweb.NShape.Properties.Resources.Text_PluralEndingUnspecific : string.Empty,
								NewValues[0]);
						} else {
							base.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_ChangeProperty0Of123,
								PropertyInfo.Name, this.ModifiedObjects.Count, typeof(T).Name,
								ModifiedObjects.Count > 1 ? Dataweb.NShape.Properties.Resources.Text_PluralEndingUnspecific : string.Empty);
						}
					}
				}
				return base.Description;
			}
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return RequiredPermissions; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = true;
			for (int i = ModifiedObjects.Count - 1; i >= 0; --i) {
				if (ModifiedObjects[i] is ISecurityDomainObject) {
					if (!securityManager.IsGranted(RequiredPermission, ((ISecurityDomainObject)ModifiedObjects[i]).SecurityDomainName))
						isGranted = false;
				} else if (!securityManager.IsGranted(RequiredPermission))
					isGranted = false;
				if (!isGranted) break;
			}
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		private void Construct(PropertyInfo propertyInfo) {
			if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
			this.PropertyInfo = propertyInfo;
			// Retrieve required permissions
			RequiredPermissions = Permission.None;
			RequiredPermissionAttribute attr = Attribute.GetCustomAttribute(propertyInfo, typeof(RequiredPermissionAttribute)) as RequiredPermissionAttribute;
			RequiredPermissions = (attr != null) ? attr.Permission : Permission.None;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected PropertyInfo PropertyInfo { get; set; }
		/// <ToBeCompleted></ToBeCompleted>
		protected List<object> OldValues { get; set; }
		/// <ToBeCompleted></ToBeCompleted>
		protected List<object> NewValues { get; set; }
		/// <ToBeCompleted></ToBeCompleted>
		protected List<T> ModifiedObjects { get; set; }
		/// <ToBeCompleted></ToBeCompleted>
		protected Permission RequiredPermissions { get; set; }

	}

	#endregion


	/// <summary>
	/// Command for executing a list of commands.
	/// The Label of this command is created by concatenating the labels of each command.
	/// </summary>
	public class AggregatedCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		public AggregatedCommand()
			: this((IRepository)null) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AggregatedCommand(IRepository repository)
			: base(repository) {
			this.commands = new List<ICommand>();
			Description = string.Empty;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AggregatedCommand(IEnumerable<ICommand> commands)
			: this(null, commands) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AggregatedCommand(IRepository repository, IEnumerable<ICommand> commands)
			: base(repository) {
			if (commands == null) throw new ArgumentNullException("commands");
			this.commands = new List<ICommand>(commands);
			CreateLabelString();
		}


		/// <override></override>
		public override string Description {
			get {
				if (string.IsNullOrEmpty(base.Description))
					CreateLabelString();
				return base.Description;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int CommandCount { get { return commands.Count; } }


		/// <ToBeCompleted></ToBeCompleted>
		public void Add(ICommand command) {
			if (command == null) throw new ArgumentNullException("command");
			command.Repository = Repository;
			commands.Add(command);
			Description = string.Empty;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Insert(int index, ICommand command) {
			if (command == null) throw new ArgumentNullException("command");
			command.Repository = Repository;
			commands.Add(command);
			Description = string.Empty;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Remove(ICommand command) {
			if (command == null) throw new ArgumentNullException("command");
			RemoveAt(commands.IndexOf(command));
			Description = string.Empty;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void RemoveAt(int index) {
			commands.RemoveAt(index);
			Description = string.Empty;
		}


		/// <override></override>
		public override void Execute() {
			for (int i = 0; i < commands.Count; ++i) {
				if (commands[i].Repository != Repository)
					commands[i].Repository = Repository;
				commands[i].Execute();
			}
		}


		/// <override></override>
		public override void Revert() {
			for (int i = commands.Count - 1; i >= 0; --i) {
				if (commands[i].Repository != Repository)
					commands[i].Repository = Repository;
				commands[i].Revert();
			}
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get {
				Permission requiredPermission = Permission.None;
				for (int i = 0; i < commands.Count; ++i) {
					if (commands[i] is Command)
						requiredPermission = ((Command)commands[i]).RequiredPermission;
				}
				return requiredPermission;
			}
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			exception = null;
			bool isGranted = true;
			for (int i = 0; i < commands.Count; ++i) {
				if (!commands[i].IsAllowed(securityManager)) {
					if (createException) exception = commands[i].CheckAllowed(securityManager);
					isGranted = false;
					break;
				}
			}
			// If a necessary command is not granted and no exception was created yet, create it now
			if (!isGranted && createException && exception == null)
				exception = new NShapeSecurityException(this);
			return isGranted;
		}


		private void CreateLabelString() {
			Description = string.Empty;
			if (commands.Count > 0) {
				string newLine = commands.Count > 3 ? Environment.NewLine : string.Empty;
				Description = commands[0].Description;
				int lastIdx = commands.Count - 1;
				for (int i = 1; i <= lastIdx; ++i) {
					Description += (i < lastIdx) ? ", " : Dataweb.NShape.Properties.Resources.MessageTxt_And;
					Description += string.Format("{0}{1}{2}", newLine, commands[i].Description.Substring(0, 1).ToLowerInvariant(), commands[i].Description.Substring(1));
				}
			}
		}


		List<ICommand> commands;
	}


	#region Commands for Shapes

	/// <summary>
	/// Inserts the given shape(s) into diagram and repository.
	/// </summary>
	[Obsolete("Use CreateShapesCommand instead")]
	public class InsertShapeCommand : CreateShapesCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public InsertShapeCommand(Diagram diagram, LayerIds layers, Shape shape, bool withModelObjects, bool keepZOrder)
			: base(diagram, layers, shape, withModelObjects, keepZOrder) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InsertShapeCommand(IRepository repository, Diagram diagram, LayerIds layers, Shape shape, bool withModelObjects, bool keepZOrder)
			: base(repository, diagram, layers, shape, withModelObjects, keepZOrder) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InsertShapeCommand(Diagram diagram, LayerIds layers, Shape shape, bool withModelObjects, bool keepZOrder, int toX, int toY)
			: base(diagram, layers, shape, withModelObjects, keepZOrder, toX, toY) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InsertShapeCommand(IRepository repository, Diagram diagram, LayerIds layers, Shape shape, bool withModelObjects, bool keepZOrder, int toX, int toY)
			: base(repository, diagram, layers, shape, withModelObjects, keepZOrder, toX, toY) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InsertShapeCommand(Diagram diagram, LayerIds layers, IEnumerable<Shape> shapes, bool withModelObjects, bool keepZOrder, int deltaX, int deltaY)
			: base(diagram, layers, shapes, withModelObjects, keepZOrder, deltaX, deltaY) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InsertShapeCommand(IRepository repository, Diagram diagram, LayerIds layers, IEnumerable<Shape> shapes, bool withModelObjects, bool keepZOrder, int deltaX, int deltaY)
			: base(repository, diagram, layers, shapes, withModelObjects, keepZOrder, deltaX, deltaY) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InsertShapeCommand(Diagram diagram, LayerIds layers, IEnumerable<Shape> shapes, bool withModelObjects, bool keepZOrder)
			: base(diagram, layers, shapes, withModelObjects, keepZOrder) {
		}
	}


	/// <summary>
	/// Remove the given shapes and their model objects from diagram and repository.
	/// </summary>
	[Obsolete("Use DeleteShapesCommand instead.")]
	public class DeleteShapeCommand : DeleteShapesCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public DeleteShapeCommand(Diagram diagram, Shape shape, bool withModelObjects)
			: base(diagram, shape, withModelObjects) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteShapeCommand(IRepository repository, Diagram diagram, Shape shape, bool withModelObjects)
			: base(repository, diagram, shape, withModelObjects) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteShapeCommand(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects)
			: base(diagram, shapes, withModelObjects) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteShapeCommand(IRepository repository, Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects)
			: base(repository, diagram, shapes, withModelObjects) {
		}
	}
	

	/// <summary>
	/// Command for inserting shape(s) and their model objects into a diagram and into the repository.
	/// </summary>
	public class CreateShapesCommand : InsertOrRemoveShapeCommand {

		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public CreateShapesCommand(Diagram diagram, LayerIds layers, Shape shape, bool withModelObjects, bool keepZOrder)
			: this(null, diagram, layers, shape, withModelObjects, keepZOrder) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateShapesCommand(Diagram diagram, int homeLayer, LayerIds supplementalLayers, Shape shape, bool withModelObjects, bool keepZOrder)
			: this(null, diagram, homeLayer, supplementalLayers, shape, withModelObjects, keepZOrder) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public CreateShapesCommand(IRepository repository, Diagram diagram, LayerIds layers, Shape shape, bool withModelObjects, bool keepZOrder)
			: this(repository, diagram, Layer.NoLayerId, layers, shape, withModelObjects, keepZOrder) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public CreateShapesCommand(Diagram diagram, LayerIds layers, Shape shape, bool withModelObjects, bool keepZOrder, int toX, int toY)
			: this(null, diagram, layers, shape, withModelObjects, keepZOrder, toX, toY) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateShapesCommand(IRepository repository, Diagram diagram, int homeLayer, LayerIds supplementalLayers, Shape shape, bool withModelObjects, bool keepZOrder)
			: base(repository, diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shape == null) throw new ArgumentNullException("shape");
			Construct(homeLayer, supplementalLayers, shape, 0, 0, keepZOrder, withModelObjects);
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public CreateShapesCommand(IRepository repository, Diagram diagram, LayerIds layers, Shape shape, bool withModelObjects, bool keepZOrder, int toX, int toY)
			: this(repository, diagram, Layer.NoLayerId, layers, shape, withModelObjects, keepZOrder, toX, toY) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateShapesCommand(IRepository repository, Diagram diagram, int homeLayer, LayerIds supplementalLayers, Shape shape, bool withModelObjects, bool keepZOrder, int toX, int toY)
			: base(repository, diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shape == null) throw new ArgumentNullException("shape");
			Construct(homeLayer, supplementalLayers, shape, toX - shape.X, toY - shape.Y, keepZOrder, withModelObjects);
			this.Description += string.Format(Dataweb.NShape.Properties.Resources.MessageTxt_At0, new Point(toX, toY));
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public CreateShapesCommand(Diagram diagram, LayerIds layers, IEnumerable<Shape> shapes, bool withModelObjects, bool keepZOrder, int deltaX, int deltaY)
			: this(null, diagram, layers, shapes, withModelObjects, keepZOrder, deltaX, deltaY) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateShapesCommand(Diagram diagram, int homeLayer, LayerIds supplementalLayers, IEnumerable<Shape> shapes, bool withModelObjects, bool keepZOrder, int deltaX, int deltaY)
			: this(null, diagram, homeLayer, supplementalLayers, shapes, withModelObjects, keepZOrder, deltaX, deltaY) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public CreateShapesCommand(IRepository repository, Diagram diagram, LayerIds layers, IEnumerable<Shape> shapes, bool withModelObjects, bool keepZOrder, int deltaX, int deltaY)
			:this(repository, diagram, Layer.NoLayerId, layers, shapes, withModelObjects, keepZOrder, deltaX, deltaY) {
		}

		/// <ToBeCompleted></ToBeCompleted>
		public CreateShapesCommand(IRepository repository, Diagram diagram, int homeLayer, LayerIds supplementalLayers, IEnumerable<Shape> shapes, bool withModelObjects, bool keepZOrder, int deltaX, int deltaY)
			: base(repository, diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			SortOrder currentSortOrder = SortOrder.TopDown; //GetCurrentSortOrder(shapes); 
			Construct(homeLayer, supplementalLayers, shapes, deltaX, deltaY, currentSortOrder, keepZOrder, withModelObjects);
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public CreateShapesCommand(Diagram diagram, LayerIds layers, IEnumerable<Shape> shapes, bool withModelObjects, bool keepZOrder)
			: this(diagram, Layer.NoLayerId, layers, shapes, withModelObjects, keepZOrder) {
		}

		/// <ToBeCompleted></ToBeCompleted>
		public CreateShapesCommand(Diagram diagram, int homeLayer, LayerIds supplementalLayers, IEnumerable<Shape> shapes, bool withModelObjects, bool keepZOrder)
			: base(diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			SortOrder currentSortOrder = SortOrder.TopDown; //GetCurrentSortOrder(shapes)
			Construct(homeLayer, supplementalLayers, shapes, 0, 0, currentSortOrder, keepZOrder, withModelObjects);
		}


		/// <override></override>
		public override void Execute() {
			InsertShapesAndModels(_layerInfo.HomeLayer, _layerInfo.SupplementalLayers);
			Reconnect(Shapes);
			if (_connectionsToRestore != null) {
				foreach (KeyValuePair<Shape, List<ShapeConnectionInfo>> item in _connectionsToRestore) {
					for (int i = item.Value.Count - 1; i >= 0; --i)
						item.Key.Connect(item.Value[i].OwnPointId, item.Value[i].OtherShape, item.Value[i].OtherPointId);
				}
			}
		}


		/// <override></override>
		public override void Revert() {
			if (_connectionsToRestore != null) {
				foreach (KeyValuePair<Shape, List<ShapeConnectionInfo>> item in _connectionsToRestore) {
					for (int i = item.Value.Count - 1; i >= 0; --i)
						item.Key.Disconnect(item.Value[i].OwnPointId);
				}
			}
			// Disconnect shapes as long as the model objects still exist.
			Disconnect(Shapes);
			DeleteShapesAndModels();
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Insert; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, Shapes);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		private void Construct(int homeLayer, LayerIds supplementalLayers, Shape shape, int offsetX, int offsetY, bool keepZOrder, bool withModelObjects) {
			this._layerInfo = LayerInfo.Create(homeLayer, supplementalLayers);
			PrepareShape(shape, offsetX, offsetY, keepZOrder ? shape.ZOrder : 0);
			SetShape(shape, withModelObjects);
			Description = GetDescription(DescriptionType.Insert, shape, withModelObjects);
		}


		private void Construct(int homeLayer, LayerIds supplementalLayers, IEnumerable<Shape> shapes, int offsetX, int offsetY, SortOrder currentSortOrder, bool keepZOrder, bool withModelObjects) {
			this._layerInfo = LayerInfo.Create(homeLayer, supplementalLayers);
			PrepareShapes(shapes, offsetX, offsetY, currentSortOrder, keepZOrder);
			SetShapes(shapes, currentSortOrder, withModelObjects);
			Description = GetDescription(DescriptionType.Insert, shapes, withModelObjects);
		}


		private void PrepareShapes(IEnumerable<Shape> shapes, int offsetX, int offsetY, SortOrder currentSortOrder, bool keepZOrder) {
			if (currentSortOrder == SortOrder.Unspecified)
				currentSortOrder = GetCurrentSortOrder(shapes);
			Debug.Assert(GetCurrentSortOrder(shapes) == currentSortOrder);

			int currentZOrder = 0;
			foreach (Shape shape in shapes) {
				PrepareShape(shape, offsetX, offsetY, keepZOrder ? shape.ZOrder : currentZOrder);
				if (currentSortOrder == SortOrder.BottomUp)
					++currentZOrder;
				else --currentZOrder;
			}
		}


		/// <summary>
		/// Reset shape's ZOrder to 'Unassigned' and offset shape's position if neccessary
		/// </summary>
		private void PrepareShape(Shape shape, int offsetX, int offsetY, int zOrder) {
			if (shape.ZOrder != zOrder) shape.ZOrder = zOrder;
			if (offsetX != 0 || offsetY != 0) {
				// Check if the shape has glue points. 
				// If it has, store all connections of connected glue points and disconnect the shapes
				List<ShapeConnectionInfo> connInfos = null;
				foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Any, null)) {
					if (!shape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue))
						continue;
					if (sci.IsEmpty) continue;
					if (connInfos == null) connInfos = new List<ShapeConnectionInfo>();
					connInfos.Add(sci);
					shape.Disconnect(sci.OwnPointId);
				}
				if (connInfos != null) {
					if (_connectionsToRestore == null) _connectionsToRestore = new Dictionary<Shape, List<ShapeConnectionInfo>>();
					_connectionsToRestore.Add(shape, connInfos);
				}
				shape.MoveBy(offsetX, offsetY);
			}
		}


		#region Fields
		private LayerInfo _layerInfo;
		private Dictionary<Shape, List<ShapeConnectionInfo>> _connectionsToRestore;
		#endregion
	}

	
	/// <summary>
	/// Command for removing shape(s) and their model objects from a diagram and repository.
	/// </summary>
	public class DeleteShapesCommand : InsertOrRemoveShapeCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public DeleteShapesCommand(Diagram diagram, Shape shape, bool withModelObjects)
			: this(null, diagram, shape, withModelObjects) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteShapesCommand(IRepository repository, Diagram diagram, Shape shape, bool withModelObjects)
			: this(repository, diagram, SingleInstanceEnumerator<Shape>.Create(shape), withModelObjects) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteShapesCommand(Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects)
			: this(null, diagram, shapes, withModelObjects) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteShapesCommand(IRepository repository, Diagram diagram, IEnumerable<Shape> shapes, bool withModelObjects)
			: base(repository, diagram) {
			deleteWithModelObjects = withModelObjects;
			SetShapes(shapes, SortOrder.TopDown, withModelObjects);
			this.Description = GetDescription(DescriptionType.Delete, shapes, withModelObjects);
			// Store "shape to model object" assignments for undo
			foreach (Shape s in shapes) {
				if (s.ModelObject != null) {
					if (modelObjectAssignments == null) modelObjectAssignments = new Dictionary<Shape, IModelObject>();
					modelObjectAssignments.Add(s, s.ModelObject);
				}
			}
		}



		/// <override></override>
		public override void Execute() {
			Disconnect(Shapes);
			DetachModelObjects();
			DeleteShapesAndModels();
		}


		/// <override></override>
		public override void Revert() {
			InsertShapesAndModels();
			AttachModelObjects();
			Reconnect(Shapes);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Delete; }
		}


		private void AttachModelObjects() {
			// Attach the model objects to their shape
			if (modelObjectAssignments != null) {
				foreach (KeyValuePair<Shape, IModelObject> item in modelObjectAssignments)
					item.Key.ModelObject = item.Value;
			}
		}
		
		
		private void DetachModelObjects() {
			// Detach all model objects from their shape
			if (modelObjectAssignments != null) {
				foreach (KeyValuePair<Shape, IModelObject> item in modelObjectAssignments)
					item.Key.ModelObject = null;
			}
		}


		// A dictionary used to store the original relationsships between shapes and model objects
		private Dictionary<Shape, IModelObject> modelObjectAssignments;
		private bool deleteWithModelObjects = false;
	}


	/// <summary>
	/// Command for changing the Z-order of shapes.
	/// </summary>
	public class LiftShapeCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		public LiftShapeCommand(Diagram diagram, IEnumerable<Shape> shapes, ZOrderDestination liftMode)
			: this(null, diagram, shapes, liftMode) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LiftShapeCommand(IRepository repository, Diagram diagram, IEnumerable<Shape> shapes, ZOrderDestination liftMode)
			: base(repository) {
			Construct(diagram, shapes, liftMode);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LiftShapeCommand(Diagram diagram, Shape shape, ZOrderDestination liftMode)
			: this(diagram, SingleInstanceEnumerator<Shape>.Create(shape), liftMode) {
		}


		/// <override></override>
		public override void Execute() {
			// store current and new ZOrders
			if (_zOrderInfos == null || _zOrderInfos.Count == 0)
				ObtainZOrders();

			// process topDown/bottomUp to avoid ZOrder-sorting inside the diagram's ShapeCollection
			switch (_liftMode) {
				case ZOrderDestination.ToBottom:
					foreach (Shape shape in _shapes.TopDown) {
						ZOrderInfo info = _zOrderInfos[shape];
						PerformZOrderChange(shape, info.newZOrder);
					}
					break;
				case ZOrderDestination.ToTop:
					foreach (Shape shape in _shapes.BottomUp) {
						ZOrderInfo info = _zOrderInfos[shape];
						PerformZOrderChange(shape, info.newZOrder);
					}
					break;
				default:
					throw new NShapeUnsupportedValueException(_liftMode);
			}
			if (Repository != null) Repository.Update(_shapes);
		}


		/// <override></override>
		public override void Revert() {
			Debug.Assert(_zOrderInfos != null);
			foreach (Shape shape in _shapes.BottomUp) {
				ZOrderInfo info = _zOrderInfos[shape];
				PerformZOrderChange(shape, info.origZOrder);
			}
			if (Repository != null) Repository.Update(_shapes);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, _shapes);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		private void Construct(Diagram diagram, IEnumerable<Shape> shapes, ZOrderDestination liftMode) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			if (shapes == null) throw new ArgumentNullException("shapes");
			this._shapes = new ShapeCollection();
			this._shapes.AddRange(shapes);
			this._diagram = diagram;
			this._liftMode = liftMode;
			string formatStr = string.Empty;
			switch (liftMode) {
				case ZOrderDestination.ToTop: formatStr = Dataweb.NShape.Properties.Resources.CaptionFmt_Bring0Shape1OnTop; break;
				case ZOrderDestination.ToBottom: formatStr = Dataweb.NShape.Properties.Resources.CaptionFmt_Send0Shape1ToBottom; break;
				default: throw new NShapeUnsupportedValueException(liftMode);
			}
			if (this._shapes.Count == 1)
				this.Description = string.Format(formatStr, this._shapes.TopMost.Type.Name, string.Empty);
			else this.Description = string.Format(formatStr, this._shapes.Count, Dataweb.NShape.Properties.Resources.Text_PluralEndingShape);
		}


		private void ObtainZOrders() {
			_zOrderInfos = new Dictionary<Shape, ZOrderInfo>(_shapes.Count);
			switch (_liftMode) {
				case ZOrderDestination.ToBottom:
					if (Repository != null) {
						foreach (Shape shape in _shapes.TopDown)
							_zOrderInfos.Add(shape, new ZOrderInfo(shape.ZOrder, Repository.ObtainNewBottomZOrder(_diagram)));
					} else {
						foreach (Shape shape in _shapes.TopDown)
							_zOrderInfos.Add(shape, new ZOrderInfo(shape.ZOrder, _diagram.Shapes.MinZOrder));
					}
					break;
				case ZOrderDestination.ToTop:
					if (Repository != null) {
						foreach (Shape shape in _shapes.BottomUp)
							_zOrderInfos.Add(shape, new ZOrderInfo(shape.ZOrder, Repository.ObtainNewTopZOrder(_diagram)));
					} else {
						foreach (Shape shape in _shapes.BottomUp)
							_zOrderInfos.Add(shape, new ZOrderInfo(shape.ZOrder, _diagram.Shapes.MaxZOrder));
					}
					break;
				default:
					throw new NShapeUnsupportedValueException(_liftMode);
			}
		}


		private void PerformZOrderChange(Shape shape, int zOrder) {
			_diagram.Shapes.SetZOrder(shape, zOrder);

			//// remove shape from Diagram
			//diagram.Shapes.Remove(shape);
			//// restore the original ZOrder value
			//shape.ZOrder = zOrder;
			//// re-insert the shape on its previous position
			//diagram.Shapes.Add(shape);
			//diagram.AddShapeToLayers(shape, layerIds);
		}


		private struct ZOrderInfo {
			public ZOrderInfo(int origZOrder, int newZOrder) {
				this.origZOrder = origZOrder;
				this.newZOrder = newZOrder;
			}
			public int origZOrder;
			public int newZOrder;
		}


		#region Fields
		private Diagram _diagram;
		private ShapeCollection _shapes;
		private ZOrderDestination _liftMode;
		private Dictionary<Shape, ZOrderInfo> _zOrderInfos;
		#endregion
	}


	/// <summary>
	/// Command for moving a set of shapes by the same displacement.
	/// Used mainly for interactive moving of multiple selected shapes.
	/// </summary>
	public class MoveShapeByCommand : ShapesCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public MoveShapeByCommand(Shape shape, int dX, int dY)
			: this(null, shape, dX, dY) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public MoveShapeByCommand(IRepository repository, Shape shape, int dX, int dY)
			: base(repository, SingleInstanceEnumerator<Shape>.Create(shape)) {
			if (shape == null) throw new ArgumentNullException("shape");
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Move0, shape.Type.Name);
			this.dX = dX;
			this.dY = dY;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public MoveShapeByCommand(IEnumerable<Shape> shapes, int dX, int dY)
			: this(null, shapes, dX, dY) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public MoveShapeByCommand(IRepository repository, IEnumerable<Shape> shapes, int dX, int dY)
			: base(repository, EmptyEnumerator<Shape>.Empty) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			//// This approach is too simple - it will not work satisfactory because connected shapes are moved twice 
			//// (the shape is moved AND will follow its partner shape).
			//// Label shapes will re-calculate their relative position in this case and therefore they are not moved as expected
			//this.shapes = new List<Shape>(shapes);

			// Move only shapes without connected glue points
			foreach (Shape shape in shapes) {
				if (shape is LabelBase) {
					// Do not move labels connected to a selected shape (as they will move with their partner anyway)
					if (!CanMoveShape((LabelBase)shape, shapes)) continue;
				}
				this.Shapes.Add(shape);
			}
			// Collect connections to remove temporarily
			for (int i = 0; i < this.Shapes.Count; ++i) {
				if (IsConnectedToNonSelectedShapes(this.Shapes[i])) 
					continue;
				foreach (ControlPointId gluePointId in this.Shapes[i].GetControlPointIds(ControlPointCapabilities.Glue)) {
					ShapeConnectionInfo gluePointConnectionInfo = this.Shapes[i].GetConnectionInfo(gluePointId, null);
					if (!gluePointConnectionInfo.IsEmpty) {
						ConnectionInfoBuffer connInfoBuffer;
						connInfoBuffer.shape = this.Shapes[i];
						connInfoBuffer.connectionInfo = gluePointConnectionInfo;
						if (connectionsBuffer == null)
							connectionsBuffer = new List<ConnectionInfoBuffer>();
						connectionsBuffer.Add(connInfoBuffer);
					}
				}
			}
			this.dX = dX;
			this.dY = dY;
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Move0Shape, this.Shapes.Count);
			if (this.Shapes.Count > 1) this.Description += "s";
		}


		/// <override></override>
		public override void Execute() {
			// Remove connections temporarily so we can move connected lines as well.
			if (connectionsBuffer != null) {
				for (int i = 0; i < connectionsBuffer.Count; ++i)
					connectionsBuffer[i].shape.Disconnect(connectionsBuffer[i].connectionInfo.OwnPointId);
			}
			// move shapes
			int cnt = Shapes.Count;
			for (int i = 0; i < cnt; ++i)
				Shapes[i].MoveBy(dX, dY);
			// restore temporarily removed connections between selected shapes
			if (connectionsBuffer != null) {
				for (int i = 0; i < connectionsBuffer.Count; ++i)
					connectionsBuffer[i].shape.Connect(connectionsBuffer[i].connectionInfo.OwnPointId, connectionsBuffer[i].connectionInfo.OtherShape, connectionsBuffer[i].connectionInfo.OtherPointId);
			}
			// update cache
			if (Repository != null) Repository.Update(Shapes);
		}


		/// <override></override>
		public override void Revert() {
			// remove connections temporarily
			if (connectionsBuffer != null) {
				for (int i = 0; i < connectionsBuffer.Count; ++i)
					connectionsBuffer[i].shape.Disconnect(connectionsBuffer[i].connectionInfo.OwnPointId);
			}

			// move shapes
			for (int i = 0; i < Shapes.Count; ++i)
				Shapes[i].MoveBy(-dX, -dY);

			// restore temporarily removed connections between selected shapes
			if (connectionsBuffer != null) {
				for (int i = 0; i < connectionsBuffer.Count; ++i)
					connectionsBuffer[i].shape.Connect(connectionsBuffer[i].connectionInfo.OwnPointId, connectionsBuffer[i].connectionInfo.OtherShape, connectionsBuffer[i].connectionInfo.OtherPointId);
			}

			if (Repository != null) Repository.Update(Shapes);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}



		/// <summary>
		/// Checks whether the given shape is connected with all its glue points. 
		/// In this case, the shape cannot be moved.
		/// </summary>
		private bool CanMoveShape(Shape shape) {
			int gluePointCnt = 0, connectedCnt = 0;
			foreach (ControlPointId gluePointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
				++gluePointCnt;
				ShapeConnectionInfo sci = ((Shape)shape).GetConnectionInfo(gluePointId, null);
				if (!sci.IsEmpty) ++connectedCnt;
			}
			if (gluePointCnt == 0)
				return true;
			else return (gluePointCnt != connectedCnt);
		}


		/// <summary>
		/// Checks whether the shape is connected with all its glue points to selected shapes.
		/// In this case, the shape cannot be moved.
		/// </summary>
		private bool CanMoveShape(LabelBase shape, IEnumerable<Shape> selectedShapes) {
			foreach (ControlPointId gluePointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
				ShapeConnectionInfo sci = shape.GetConnectionInfo(gluePointId, null);
				if (!sci.IsEmpty) {
					foreach (Shape s in selectedShapes) {
						if (s == sci.OtherShape)
							return false;
					}
				}
			}
			return true;
		}


		private bool IsConnectedToNonSelectedShapes(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			foreach (ControlPointId gluePointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
				ShapeConnectionInfo sci = shape.GetConnectionInfo(gluePointId, null);
				if (!sci.IsEmpty && Shapes.IndexOf(sci.OtherShape) < 0)
					return true;
			}
			return false;
		}


		private int dX, dY;
		private struct ConnectionInfoBuffer : IEquatable<ConnectionInfoBuffer> {
			internal Shape shape;
			internal ShapeConnectionInfo connectionInfo;
			public bool Equals(ConnectionInfoBuffer other) {
				return other.shape == this.shape && other.connectionInfo == this.connectionInfo;
			}
		}
		private List<ConnectionInfoBuffer> connectionsBuffer;	// buffer used for storing connections that are temporarily disconnected for moving shapes
	}


	/// <summary>
	/// Command for moving multiple shapes by individual distances.
	/// </summary>
	public class MoveShapesCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		public MoveShapesCommand()
			: this(null) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public MoveShapesCommand(IRepository repository)
			: base(repository) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void AddMove(Shape shape, int dx, int dy) {
			if (shape == null) throw new ArgumentNullException("shape");
			ShapeMove sm;
			sm.shape = shape;
			sm.dx = dx;
			sm.dy = dy;
			shapeMoves.Add(sm);
		}


		#region Base Class Implementation

		/// <override></override>
		public override string Description {
			get {
                return string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Move0Shape1, shapeMoves.Count, 
                    (shapeMoves.Count > 1) ? Dataweb.NShape.Properties.Resources.Text_PluralEndingShape : string.Empty);
            }
		}


		/// <override></override>
		public override void Execute() {
			foreach (ShapeMove sm in shapeMoves)
				sm.shape.MoveControlPointBy(ControlPointId.Reference, sm.dx, sm.dy, ResizeModifiers.None);
		}


		/// <override></override>
		public override void Revert() {
			foreach (ShapeMove sm in shapeMoves)
				sm.shape.MoveControlPointBy(ControlPointId.Reference, -sm.dx, -sm.dy, ResizeModifiers.None);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = true;
			for (int i = shapeMoves.Count - 1; i >= 0; --i) {
				if (!securityManager.IsGranted(RequiredPermission, shapeMoves[i].shape)) {
					isGranted = false;
					break;
				}
			}
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected struct ShapeMove : IEquatable<ShapeMove> {
			/// <ToBeCompleted></ToBeCompleted>
			public Shape shape;
			/// <ToBeCompleted></ToBeCompleted>
			public int dx;
			/// <ToBeCompleted></ToBeCompleted>
			public int dy;
			/// <ToBeCompleted></ToBeCompleted>
			public bool Equals(ShapeMove other) {
				return (other.shape == this.shape
					&& other.dx == this.dx
					&& other.dy == this.dy);
			}
		}


		private List<ShapeMove> shapeMoves = new List<ShapeMove>();

		#endregion
	}


	/// <summary>
	/// Command for moving multiple shapes to individual destination points.
	/// </summary>
	public class MoveShapesToCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		public MoveShapesToCommand()
			: this(null) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public MoveShapesToCommand(IRepository repository)
			: base(repository) {
		}


		/// <summary>
		/// Adds a move for the shape the shape will be moved to the given coordinates. 
		/// The given shape has to be either at the original position or at the target position.
		/// </summary>
		public void AddMoveTo(Shape shape, int x0, int y0, int x1, int y1) {
			if (shape == null) throw new ArgumentNullException("shape");
			Debug.Assert((shape.X == x0 || shape.X == x1) && (shape.Y == y0 || shape.Y == y1));
			ShapeMove sm;
			sm.shape = shape;
			sm.origX = x0;
			sm.origY = y0;
			sm.destX = x1;
			sm.destY = y1;
			shapeMoves.Add(sm);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void AddMoveBy(Shape shape, int dx, int dy) {
			if (shape == null) throw new ArgumentNullException("shape");
			ShapeMove sm;
			sm.shape = shape;
			sm.origX = shape.X;
			sm.origY = shape.Y;
			sm.destX = shape.X + dx;
			sm.destY = shape.Y + dy;
			shapeMoves.Add(sm);
		}


		#region Base Class Implementation

		/// <override></override>
		public override string Description {
			get {
                return string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Move0Shape1, shapeMoves.Count, 
                    (shapeMoves.Count > 1) ? Dataweb.NShape.Properties.Resources.Text_PluralEndingShape : string.Empty);
            }
		}


		/// <override></override>
		public override void Execute() {
			foreach (ShapeMove sm in shapeMoves)
				sm.shape.MoveTo(sm.destX, sm.destY);
		}


		/// <override></override>
		public override void Revert() {
			foreach (ShapeMove sm in shapeMoves)
				sm.shape.MoveTo(sm.origX, sm.origY);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = true;
			for (int i = shapeMoves.Count - 1; i >= 0; --i) {
				if (!securityManager.IsGranted(RequiredPermission, shapeMoves[i].shape)) {
					isGranted = false;
					break;
				}
			}
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected struct ShapeMove : IEquatable<ShapeMove> {
			/// <ToBeCompleted></ToBeCompleted>
			public Shape shape;
			/// <ToBeCompleted></ToBeCompleted>
			public int origX;
			/// <ToBeCompleted></ToBeCompleted>
			public int origY;
			/// <ToBeCompleted></ToBeCompleted>
			public int destX;
			/// <ToBeCompleted></ToBeCompleted>
			public int destY;
			/// <ToBeCompleted></ToBeCompleted>
			public bool Equals(ShapeMove other) {
				return (other.destX == this.destX
					&& other.destY == this.destY
					&& other.origX == this.origX
					&& other.origY == this.origY
					&& other.shape == this.shape);
			}
		}


		private List<ShapeMove> shapeMoves = new List<ShapeMove>();

		#endregion

	}


	/// <summary>
	/// Command for moving a command control point of one or multiple shapes.
	/// </summary>
	public class MoveControlPointCommand : ShapesCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public MoveControlPointCommand(Shape shape, ControlPointId controlPointId, int dX, int dY, ResizeModifiers modifiers)
			: this(null, shape, controlPointId, dX, dY, modifiers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public MoveControlPointCommand(IRepository repository, Shape shape, ControlPointId controlPointId, int dX, int dY, ResizeModifiers modifiers)
			: this(repository, SingleInstanceEnumerator<Shape>.Create(shape), controlPointId, dX, dY, modifiers) {
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_MoveControlPointOf0, shape.Type.Name);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public MoveControlPointCommand(IEnumerable<Shape> shapes, ControlPointId controlPointId, int dX, int dY, ResizeModifiers modifiers)
			: this(null, shapes, controlPointId, dX, dY, modifiers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public MoveControlPointCommand(IRepository repository, IEnumerable<Shape> shapes, ControlPointId controlPointId, int dX, int dY, ResizeModifiers modifiers)
			: base(repository, shapes) {
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Move0ControlPoint1, this.Shapes.Count, 
                this.Shapes.Count > 1 ? Dataweb.NShape.Properties.Resources.Text_ControlPointPluralEnding : string.Empty);
			int cnt = this.Shapes.Count;
			moveInfos = new List<PointMoveInfo>(cnt);
			for (int i = 0; i < cnt; ++i)
				moveInfos.Add(new PointMoveInfo(this.Shapes[i], controlPointId, dX, dY, modifiers));
		}


		/// <override></override>
		public override void Execute() {
			for (int i = 0; i < Shapes.Count; ++i)
				Shapes[i].MoveControlPointTo(moveInfos[i].PointId, moveInfos[i].To.X, moveInfos[i].To.Y, moveInfos[i].Modifiers);

			if (Repository != null) Repository.Update(Shapes);
		}


		/// <override></override>
		public override void Revert() {
			for (int i = 0; i < Shapes.Count; ++i)
				//shapes[i].MoveControlPointBy(controlPointId, -dX, -dY, modifiers);
				Shapes[i].MoveControlPointTo(moveInfos[i].PointId, moveInfos[i].From.X, moveInfos[i].From.Y, moveInfos[i].Modifiers);

			if (Repository != null) Repository.Update(Shapes);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		private class PointMoveInfo {

			public PointMoveInfo(Shape shape, ControlPointId id, int dx, int dy, ResizeModifiers mod)
				: this(id, shape.GetControlPointPosition(id), Point.Empty, mod) {
				To.Offset(From.X + dx, From.Y + dy);
			}

			public PointMoveInfo(ControlPointId id, Point from, int dx, int dy, ResizeModifiers mod)
				: this(id, from, from, mod) {
				To.Offset(dx, dy);
			}

			public PointMoveInfo(ControlPointId id, Point from, Point to, ResizeModifiers mod) {
				PointId = id;
				From = from;
				To = to;
				Modifiers = mod;
			}

			public ControlPointId PointId;
			public Point From;
			public Point To;
			public ResizeModifiers Modifiers;
		}

		private List<PointMoveInfo> moveInfos;
	}


	/// <summary>
	/// Command for moving an active connection point (glue point) of a shape to the connection point of another shape and connect it.
	/// Former existing connections will be disconnected.
	/// </summary>
	public class MoveGluePointCommand : ShapeCommand {

		/// <summary>
		/// Constructs a glue point moving command.
		/// </summary>
		public MoveGluePointCommand(Shape shape, ControlPointId gluePointId, Shape targetShape, int dX, int dY, ResizeModifiers modifiers)
			: this(null, shape, gluePointId, targetShape, dX, dY, modifiers) {
		}


		/// <summary>
		/// Constructs a glue point moving command.
		/// </summary>
		public MoveGluePointCommand(IRepository repository, Shape shape, ControlPointId gluePointId, Shape targetShape, int dX, int dY, ResizeModifiers modifiers)
			: base(repository, shape) {
			// Find target point
			ControlPointId targetPointId = ControlPointId.None;
			if (targetShape != null) {
				targetPointId = targetShape.FindNearestControlPoint(_gluePointPosition.X + dX, _gluePointPosition.Y + dY, 0, ControlPointCapabilities.Connect);
				if (targetPointId == ControlPointId.None && targetShape.ContainsPoint(_gluePointPosition.X + dX, _gluePointPosition.Y + dY))
					targetPointId = ControlPointId.Reference;
			}
			BaseConstruct(gluePointId, targetShape, targetPointId, dX, dY, modifiers);
		}


		/// <summary>
		/// Constructs a glue point moving command.
		/// </summary>
		public MoveGluePointCommand(Shape shape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId, int dX, int dY, ResizeModifiers modifiers)
			: this(null, shape, gluePointId, targetShape, targetPointId, dX, dY, modifiers) {
		}


		/// <summary>
		/// Constructs a glue point moving command.
		/// </summary>
		public MoveGluePointCommand(IRepository repository, Shape shape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId, int dX, int dY, ResizeModifiers modifiers)
			: base(repository, shape) {
			BaseConstruct(gluePointId, targetShape, targetPointId, dX, dY, modifiers);
		}


		/// <override></override>
		public override void Execute() {
			// DetachGluePointFromConnectionPoint existing connection
			if (!_existingConnection.IsEmpty) {
				Shape.Disconnect(_gluePointId);
				if (Repository != null) Repository.DeleteConnection(Shape, _gluePointId, _existingConnection.OtherShape, _existingConnection.OtherPointId);
			}
			// Move point
			Shape.MoveControlPointBy(_gluePointId, _dX, _dY, _modifiers);
			// Establish new connection
			if (!_newConnection.IsEmpty) {
				Shape.Connect(_gluePointId, _newConnection.OtherShape, _newConnection.OtherPointId);
				if (Repository != null) Repository.InsertConnection(Shape, _gluePointId, _newConnection.OtherShape, _newConnection.OtherPointId);
			}
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override void Revert() {
			// DetachGluePointFromConnectionPoint new connection
			if (!_newConnection.IsEmpty) {
				Shape.Disconnect(_gluePointId);
				if (Repository != null) Repository.DeleteConnection(Shape, _gluePointId, _newConnection.OtherShape, _newConnection.OtherPointId);
			}
			// Move point
			Shape.MoveControlPointTo(_gluePointId, _gluePointPosition.X, _gluePointPosition.Y, _modifiers);
			// Restore previous connection
			if (!_existingConnection.IsEmpty) {
				Shape.Connect(_gluePointId, _existingConnection.OtherShape, _existingConnection.OtherPointId);
				if (Repository != null) Repository.InsertConnection(Shape, _gluePointId, _existingConnection.OtherShape, _existingConnection.OtherPointId);
			}
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout | Permission.Connect; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void BaseConstruct(ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId, int dX, int dY, ResizeModifiers modifiers) {
			this._gluePointId = gluePointId;
			this._targetShape = targetShape;
			this._targetPointId = targetPointId;
			this._dX = dX;
			this._dY = dY;
			this._modifiers = modifiers;
			// store original position of gluePoint (cannot be restored with dX/dY in case of PointToShape connections)
			_gluePointPosition = Shape.GetControlPointPosition(gluePointId);
			// store existing connection
			_existingConnection = Shape.GetConnectionInfo(this._gluePointId, null);

			// create new ConnectionInfo
			if (targetShape != null && targetPointId != ControlPointId.None)
				this._newConnection = new ShapeConnectionInfo(this._gluePointId, targetShape, targetPointId);
			// set description
			if (!_existingConnection.IsEmpty) {
				this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Disconnect0From1, Shape.Type.Name, _existingConnection.OtherShape.Type.Name);
				if (!_newConnection.IsEmpty)
					this.Description += string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_AndConnectTo0, _newConnection.OtherShape.Type.Name);
			} else {
				if (!_newConnection.IsEmpty)
					this.Description += string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Connect0To1, Shape.Type.Name, _newConnection.OtherShape.Type.Name);
				else
					this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_MoveGluePoint0Of1, gluePointId, Shape.Type.Name);
			}
		}


		#region Fields
		private Point _gluePointPosition;
		private ControlPointId _gluePointId;
		private Shape _targetShape;
		private ControlPointId _targetPointId;
		private int _dX;
		private int _dY;
		private ResizeModifiers _modifiers;
		private ShapeConnectionInfo _existingConnection;
		private ShapeConnectionInfo _newConnection = ShapeConnectionInfo.Empty;
		#endregion
	}


	/// <summary>
	/// Command for rotating all members of a set of shapes by the same angle.
	/// </summary>
	public class RotateShapesCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		public RotateShapesCommand(IEnumerable<Shape> shapes, int tenthsOfDegree)
			: this(null, shapes, tenthsOfDegree) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RotateShapesCommand(IRepository repository, IEnumerable<Shape> shapes, int tenthsOfDegree)
			: base(repository) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this._shapes = new List<Shape>(shapes);
			this._angle = tenthsOfDegree;
			if (this._shapes.Count == 1)
				this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Rotate0By1, this._shapes[0].Type.Name, tenthsOfDegree / 10f);
			else
				this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Rotate0ShapesBy1, this._shapes.Count, tenthsOfDegree / 10f);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RotateShapesCommand(IEnumerable<Shape> shapes, int tenthsOfDegree, int rotateCenterX, int rotateCenterY)
			: this(null, shapes, tenthsOfDegree, rotateCenterX, rotateCenterY) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RotateShapesCommand(IRepository repository, IEnumerable<Shape> shapes, int tenthsOfDegree, int rotateCenterX, int rotateCenterY)
			: base(repository) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			this._shapes = new List<Shape>(shapes);
			for (int i = 0; i < this._shapes.Count; ++i) {
				if (this._shapes[i] is ILinearShape) {
					List<Point> points = new List<Point>();
					foreach (ControlPointId id in this._shapes[i].GetControlPointIds(ControlPointCapabilities.Resize)) {
						Point p = this._shapes[i].GetControlPointPosition(id);
						points.Add(p);
					}
					_unrotatedLinePoints.Add((ILinearShape)this._shapes[i], points);
				}
			}
			this._angle = tenthsOfDegree;
			this._rotateCenterX = rotateCenterX;
			this._rotateCenterY = rotateCenterY;
			if (this._shapes.Count == 1)
				this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Rotate0By1At23, this._shapes[0].Type.Name, tenthsOfDegree / 10f, rotateCenterX, rotateCenterY);
			else
				this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Rotate0ShapesBy1At23, this._shapes.Count, tenthsOfDegree / 10f, rotateCenterX, rotateCenterY);
		}


		/// <override></override>
		public override void Execute() {
			foreach (Shape shape in _shapes) {
				if (!Geometry.IsValid(_rotateCenterX, _rotateCenterY))
					shape.Rotate(_angle, shape.X, shape.Y);
				else shape.Rotate(_angle, _rotateCenterX, _rotateCenterY);
			}
			if (Repository != null) Repository.Update(_shapes);
		}


		/// <override></override>
		public override void Revert() {
			foreach (Shape shape in _shapes) {
				if (!Geometry.IsValid(_rotateCenterX, _rotateCenterY))
					shape.Rotate(-_angle, shape.X, shape.Y);
				else shape.Rotate(-_angle, _rotateCenterX, _rotateCenterY);
			}
			if (Repository != null) Repository.Update(_shapes);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, _shapes);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		private int _angle;
		private List<Shape> _shapes;
		private int _rotateCenterX = Geometry.InvalidPoint.X;
		private int _rotateCenterY = Geometry.InvalidPoint.Y;
		private Dictionary<ILinearShape, List<Point>> _unrotatedLinePoints = new Dictionary<ILinearShape, List<Point>>();
	}


	/// <summary>
	/// Command for setting the caption text of shapes implementing the <see cref="T:Dataweb.NShape.Advanced.ICaptionedShape"/> interface.
	/// </summary>
	public class SetCaptionTextCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		public SetCaptionTextCommand(ICaptionedShape shape, int captionIndex, string oldValue, string newValue)
			: this(null, shape, captionIndex, oldValue, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public SetCaptionTextCommand(IRepository repository, ICaptionedShape shape, int captionIndex, string oldValue, string newValue)
			: base(repository) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (!(shape is Shape)) throw new ArgumentException(String.Format("{0} is not of type {1}.", shape.GetType().Name, typeof(Shape).Name));
			this._modifiedLabeledShapes = new List<ICaptionedShape>(1);
			this._modifiedLabeledShapes.Add(shape);
			this._labelIndex = captionIndex;
			this._oldValue = oldValue;
			this._newValue = newValue;
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_ChangeTextOf0From1To2, ((Shape)shape).Type.Name, this._oldValue, this._newValue);
		}


		/// <override></override>
		public override void Execute() {
			for (int i = _modifiedLabeledShapes.Count - 1; i >= 0; --i) {
				_modifiedLabeledShapes[i].SetCaptionText(_labelIndex, _newValue);
				if (Repository != null) Repository.Update((Shape)_modifiedLabeledShapes[i]);
			}
		}


		/// <override></override>
		public override void Revert() {
			for (int i = _modifiedLabeledShapes.Count - 1; i >= 0; --i) {
				_modifiedLabeledShapes[i].SetCaptionText(_labelIndex, _oldValue);
				if (Repository != null) Repository.Update((Shape)_modifiedLabeledShapes[i]);
			}
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Data; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = true;
			for (int i = _modifiedLabeledShapes.Count - 1; i >= 0; --i) {
				if (!securityManager.IsGranted(RequiredPermission, (Shape)_modifiedLabeledShapes[i])) {
					isGranted = false;
					break;
				}
			}
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		#region Fields
		private int _labelIndex;
		private string _oldValue;
		private string _newValue;
		private List<ICaptionedShape> _modifiedLabeledShapes;
		#endregion
	}


	/// <summary>
	/// Command for connecting a shape's active connection point (glue point) to another shape's passive connection point
	/// </summary>
	public class ConnectCommand : ConnectionCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public ConnectCommand(Shape connectorShape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId)
			: this(null, connectorShape, gluePointId, targetShape, targetPointId) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ConnectCommand(IRepository repository, Shape connectorShape, ControlPointId gluePointId, Shape targetShape, ControlPointId targetPointId)
			: base(repository, connectorShape, gluePointId, targetShape, targetPointId) {
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Connect0To1, connectorShape.Type.Name, targetShape.Type.Name);
		}


		/// <override></override>
		public override void Execute() {
			Connect();
		}


		/// <override></override>
		public override void Revert() {
			Disconnect();
		}

	}


	/// <summary>
	/// Command for disconnecting a shape's active connection point (glue point) from another shape's passive connection point
	/// </summary>
	public class DisconnectCommand : ConnectionCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public DisconnectCommand(Shape connectorShape, ControlPointId gluePointId)
			: this(null, connectorShape, gluePointId) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DisconnectCommand(IRepository repository, Shape connectorShape, ControlPointId gluePointId)
			: base(repository, connectorShape, gluePointId) {
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Disconnect0, connectorShape.Type.Name);
			this.ConnectorShape = connectorShape;
			this.GluePointId = gluePointId;

			this._connectionInfo = connectorShape.GetConnectionInfo(gluePointId, null);
			if (this._connectionInfo.IsEmpty)
				throw new NShapeInternalException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_ThereIsNoConnectionForPoint0OfShape1, gluePointId, connectorShape));
		}


		/// <override></override>
		public override void Execute() {
			ConnectorShape.Disconnect(GluePointId);
			Repository.DeleteConnection(ConnectorShape, GluePointId, _connectionInfo.OtherShape, _connectionInfo.OtherPointId);
		}


		/// <override></override>
		public override void Revert() {
			ConnectorShape.Connect(GluePointId, _connectionInfo.OtherShape, _connectionInfo.OtherPointId);
			Repository.InsertConnection(ConnectorShape, GluePointId, _connectionInfo.OtherShape, _connectionInfo.OtherPointId);
		}


		private ShapeConnectionInfo _connectionInfo;
	}

	#endregion


	#region Commands for aggregating/grouping shapes

	/// <summary>
	/// Command for moving shapes from the diagram into a new group shape.
	/// </summary>
	public class GroupShapesCommand : ShapeAggregationCommand {

		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public GroupShapesCommand(Diagram diagram, LayerIds layers, Shape shapeGroup, IEnumerable<Shape> childShapes)
			: this(null, diagram, Layer.NoLayerId, layers, shapeGroup, childShapes) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public GroupShapesCommand(Diagram diagram, int homeLayer, LayerIds supplementalLayers, Shape shapeGroup, IEnumerable<Shape> childShapes)
			: this(null, diagram, homeLayer, supplementalLayers, shapeGroup, childShapes) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public GroupShapesCommand(IRepository repository, Diagram diagram, LayerIds supplementalLayers, Shape shapeGroup, IEnumerable<Shape> childShapes)
			: this(repository, diagram, Layer.NoLayerId, supplementalLayers, shapeGroup, childShapes) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public GroupShapesCommand(IRepository repository, Diagram diagram, int homeLayer, LayerIds supplementalLayers, Shape shapeGroup, IEnumerable<Shape> childShapes)
			: base(repository, diagram, shapeGroup, childShapes, homeLayer, supplementalLayers) {
			if (!(AggregationShape is IShapeGroup)) throw new ArgumentException(string.Format("Shape does not implement required interface {0}.", typeof(IShapeGroup).Name));
            this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Aggregate0ShapesToA1, base.Shapes.Count, ((Shape)base.AggregationShape).Type.Name);

			// Calculate boundingRectangle of the children and
			// move aggregationShape to children's center
			if (AggregationShape.X == 0 && AggregationShape.Y == 0) {
				Rectangle r = Rectangle.Empty;
				foreach (Shape shape in childShapes) {
					if (r.IsEmpty) r = shape.GetBoundingRectangle(true);
					else r = Geometry.UniteRectangles(r, shape.GetBoundingRectangle(true));
				}
				AggregationShape.MoveTo(r.X + (r.Width / 2), r.Y + (r.Height / 2));
			}
			AggregationShapeOwnedByDiagram = false;
		}


		/// <override></override>
		public override void Execute() {
			// insert aggregationShape into diagram (and Cache)
			if (Repository != null) AggregationShape.ZOrder = Repository.ObtainNewTopZOrder(Diagram);
			CreateShapeAggregation(false);
		}


		/// <override></override>
		public override void Revert() {
			DeleteShapeAggregation();
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.None; }
		}
	}


	/// <summary>
	/// Command for moving shapes from the group shape into the diagram and deleting the group shape.
	/// </summary>
	public class UngroupShapesCommand : ShapeAggregationCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public UngroupShapesCommand(Diagram diagram, Shape shapeGroup)
			: this(null, diagram, shapeGroup) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public UngroupShapesCommand(IRepository repository, Diagram diagram, Shape shapeGroup)
			: base(repository, diagram, shapeGroup, (shapeGroup != null) ? shapeGroup.Children : null) {
			if (!(shapeGroup is IShapeGroup)) throw new ArgumentException(string.Format("Shape does not implement required interface {0}.", typeof(IShapeGroup).Name));
            this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Release0ShapesFrom1SAggregation, base.Shapes.Count, base.AggregationShape.Type.Name);
			AggregationShapeOwnedByDiagram = false;
		}


		/// <override></override>
		public override void Execute() {
			DeleteShapeAggregation();
		}


		/// <override></override>
		public override void Revert() {
			CreateShapeAggregation(false);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.None; }
		}

	}


	/// <summary>
	/// Command for moving shapes from the diagram into the children collection of a shape.
	/// </summary>
	public class AggregateCompositeShapeCommand : ShapeAggregationCommand {

		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public AggregateCompositeShapeCommand(Diagram diagram, LayerIds layers, Shape parentShape, IEnumerable<Shape> childShapes)
			: base(null, diagram, parentShape, childShapes, Layer.NoLayerId, layers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AggregateCompositeShapeCommand(Diagram diagram, int homeLayer, LayerIds supplementalLayers, Shape parentShape, IEnumerable<Shape> childShapes)
			: base(null, diagram, parentShape, childShapes, Layer.NoLayerId, supplementalLayers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public AggregateCompositeShapeCommand(IRepository repository, Diagram diagram, LayerIds layers, Shape parentShape, IEnumerable<Shape> childShapes)
			: base(null, diagram, parentShape, childShapes, Layer.NoLayerId, layers) {
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Aggregate0ShapesToACompositeShape, base.Shapes.Count);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AggregateCompositeShapeCommand(IRepository repository, Diagram diagram, int homeLayer, LayerIds supplementalLayers, Shape parentShape, IEnumerable<Shape> childShapes)
			: base(null, diagram, parentShape, childShapes, Layer.NoLayerId, supplementalLayers) {
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Aggregate0ShapesToACompositeShape, base.Shapes.Count);
		}


		/// <override></override>
		public override void Execute() {
			CreateShapeAggregation(true);
		}


		/// <override></override>
		public override void Revert() {
			DeleteShapeAggregation();
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get {
				// Needs Layout permissions.
				// Needs Insert- and delete permissions because the aggregated child shapes will move from the display into the shape
				return Permission.Layout;
			}
		}
	}


	/// <summary>
	/// Command for moving shapes from the children collection of a shape into the diagram.
	/// </summary>
	public class SplitCompositeShapeCommand : ShapeAggregationCommand {

		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public SplitCompositeShapeCommand(Diagram diagram, LayerIds layers, Shape parentShape)
			: this(null, diagram, Layer.NoLayerId, layers, parentShape) {
		}


		/// <summary>
		/// Splits a composite shape and into the parent shape and its child shapes. 
		/// Assigns the parent shape to the given layers.
		/// </summary>
		public SplitCompositeShapeCommand(Diagram diagram, int homeLayer, LayerIds supplementalLayers, Shape parentShape)
			: this(null, diagram, homeLayer, supplementalLayers, parentShape) {			
		}


		/// <summary>
		/// Splits a composite shape and into the parent shape and its child shapes. 
		/// </summary>
		public SplitCompositeShapeCommand(Diagram diagram, Shape parentShape)
			: this(null, diagram, Layer.NoLayerId, LayerIds.None, parentShape) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public SplitCompositeShapeCommand(IRepository repository, Diagram diagram, LayerIds layers, Shape parentShape)
			: this(repository, diagram, Layer.NoLayerId, layers, parentShape) {
		}


		/// <summary>
		/// Splits a composite shape and into the parent shape and its child shapes. 
		/// </summary>
		public SplitCompositeShapeCommand(IRepository repository, Diagram diagram, Shape parentShape)
			: base(repository, diagram, parentShape, (parentShape != null) ? parentShape.Children : null, parentShape.HomeLayer, parentShape.SupplementalLayers) {
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_SplitCompositeShapeInto0SingleShapes, base.Shapes.Count);
		}


		/// <summary>
		/// Splits a composite shape and into the parent shape and its child shapes. 
		/// Assigns the parent shape to the given layers.
		/// </summary>
		public SplitCompositeShapeCommand(IRepository repository, Diagram diagram, int homeLayer, LayerIds supplementalLayers, Shape parentShape)
			: base(repository, diagram, parentShape, (parentShape != null) ? parentShape.Children : null, homeLayer, supplementalLayers) {
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_SplitCompositeShapeInto0SingleShapes, base.Shapes.Count);
		}


		/// <override></override>
		public override void Execute() {
			DeleteShapeAggregation();
		}


		/// <override></override>
		public override void Revert() {
			CreateShapeAggregation(true);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get {
				// Needs Layout permissions
				return Permission.Layout;
			}
		}
	}

	#endregion


	#region Commands for editing vertices of LinearShapes

	/// <summary>
	/// Command for adding a new (non-vertex) connection point to an <see cref="T:Dataweb.NShape.Advanced.ILinearShape" />.
	/// </summary>
	public class AddConnectionPointCommand : ShapeCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public AddConnectionPointCommand(Shape shape, int x, int y)
			: this(null, shape, x, y) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AddConnectionPointCommand(IRepository repository, Shape shape, int x, int y)
			: base(repository, shape) {
			if (!(shape is ILinearShape)) throw new ArgumentException(string.Format("Shape does not implement required interface {0}.", typeof(ILinearShape).Name));
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_AddConnectionPointTo0At12, shape, x, y);
			this._x = x;
			this._y = y;
		}


		/// <override></override>
		public override void Execute() {
			_insertedPointId = ((ILinearShape)Shape).AddConnectionPoint(_x, _y);
			Shape.Invalidate();
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override void Revert() {
			((ILinearShape)Shape).RemoveConnectionPoint(_insertedPointId);
			Shape.Invalidate();
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Connect; }
		}


		#region Fields
		private int _x;
		private int _y;
		private ControlPointId _insertedPointId = ControlPointId.None;
		#endregion
	}


	/// <summary>
	/// Command for adding a new vertex to an <see cref="T:Dataweb.NShape.Advanced.ILinearShape" />.
	/// </summary>
	public class AddVertexCommand : ShapeCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public AddVertexCommand(Shape shape, int x, int y)
			: this(null, shape, x, y) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AddVertexCommand(IRepository repository, Shape shape, int x, int y)
			: base(repository, shape) {
			if (!(shape is ILinearShape)) throw new ArgumentException(string.Format("Shape does not implement required interface {0}.", typeof(ILinearShape).Name));
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_AddPointTo0At12, shape, x, y);
			this._x = x;
			this._y = y;
		}


		/// <override></override>
		public override void Execute() {
			_insertedPointId = ((ILinearShape)Shape).AddVertex(_x, _y);
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override void Revert() {
			((ILinearShape)Shape).RemoveVertex(_insertedPointId);
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		#region Fields
		private int _x;
		private int _y;
		private ControlPointId _insertedPointId = ControlPointId.None;
		#endregion
	}


	/// <summary>
	/// Command for inserting a new vertex into an <see cref="T:Dataweb.NShape.Advanced.ILinearShape" />, thus reshaping the ILinearShape.
	/// </summary>
	public class InsertVertexCommand : ShapeCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public InsertVertexCommand(Shape shape, ControlPointId beforePointId, int x, int y)
			: this(null, shape, beforePointId, x, y) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InsertVertexCommand(IRepository repository, Shape shape, ControlPointId beforePointId, int x, int y)
			: base(repository, shape) {
			if (!(shape is ILinearShape)) throw new ArgumentException(string.Format("Shape does not implement required interface {0}.", typeof(ILinearShape).Name));
			if (beforePointId == ControlPointId.None || beforePointId == ControlPointId.Any || beforePointId == ControlPointId.Reference || beforePointId == ControlPointId.FirstVertex)
				throw new ArgumentException("beforePointId");
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_AddPointTo0At12, shape, x, y);
			this._beforePointId = beforePointId;
			this._x = x;
			this._y = y;
		}


		/// <override></override>
		public override void Execute() {
			if (_insertedPointId != ControlPointId.None && Shape is LineShapeBase)
				_insertedPointId = ((LineShapeBase)Shape).InsertVertex(_beforePointId, _insertedPointId, _x, _y);
			else _insertedPointId = ((ILinearShape)Shape).InsertVertex(_beforePointId, _x, _y);
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override void Revert() {
			((ILinearShape)Shape).RemoveVertex(_insertedPointId);
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		#region Fields
		private ControlPointId _beforePointId = ControlPointId.None;
		private int _x;
		private int _y;
		private ControlPointId _insertedPointId = ControlPointId.None;
		#endregion
	}


	/// <summary>
	/// Command for removing a new (non-vertex) connection point from an <see cref="T:Dataweb.NShape.Advanced.ILinearShape" />.
	/// </summary>
	public class RemoveConnectionPointCommand : ShapeCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public RemoveConnectionPointCommand(Shape shape, ControlPointId removedPointId)
			: this(null, shape, removedPointId) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RemoveConnectionPointCommand(IRepository repository, Shape shape, ControlPointId removedPointId)
			: base(repository, shape) {
			if (!(shape is ILinearShape)) throw new ArgumentException(string.Format("Shape does not implement required interface {0}.", typeof(ILinearShape).Name));
			this._removedPointId = removedPointId;
			// MenuItemDefs always create their commands, so we have to handle this
			// case instead of throwing an exception.
			if (removedPointId != ControlPointId.None)
				this._p = shape.GetControlPointPosition(removedPointId);
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_RemoveConnectionPointFrom0At12, shape, _p.X, _p.Y);
		}


		/// <override></override>
		public override void Execute() {
			((ILinearShape)Shape).RemoveConnectionPoint(_removedPointId);
			Shape.Invalidate();
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override void Revert() {
			ControlPointId ptId = ((ILinearShape)Shape).AddConnectionPoint(_p.X, _p.Y);
			if (ptId != _removedPointId) {
				Debug.Fail("PointId should not change when reverting a command!");
				_removedPointId = ptId;
			}
			Shape.Invalidate();
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Connect; }
		}


		#region Fields
		private Point _p;
		private ControlPointId _removedPointId = ControlPointId.None;
		#endregion
	}


	/// <summary>
	/// Command for removing a vertex from an <see cref="T:Dataweb.NShape.Advanced.ILinearShape" />, thus reshaping the ILinearShape.
	/// </summary>
	public class RemoveVertexCommand : ShapeCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public RemoveVertexCommand(Shape shape, ControlPointId vertexId)
			: this(null, shape, vertexId) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RemoveVertexCommand(IRepository repository, Shape shape, ControlPointId vertexId)
			: base(repository, shape) {
			if (!(shape is ILinearShape)) throw new ArgumentException(string.Format("Shape does not implement required interface {0}.", typeof(ILinearShape).Name));
			// MenuItemDefs always create their commands, regardless if there is a valid vertexId that can be removed or not.
			// So we have to handle this case instead of throwing an exception.
			if (vertexId != ControlPointId.None)
				this._p = shape.GetControlPointPosition(vertexId);
			_ctrlPointPositions = new Dictionary<ControlPointId, Point>();
			foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.Resize | ControlPointCapabilities.Connect | ControlPointCapabilities.Movable)) {
				if (id == vertexId) continue;
				_ctrlPointPositions.Add(id, shape.GetControlPointPosition(id));
			}
			this._removedPointId = vertexId;
			this._nextPointId = ((ILinearShape)shape).GetNextVertexId(vertexId);

			// Do not find point position here because if controlPointId is not valid, an exception would be thrown
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_RemovePointAt01From2, _p.X, _p.Y, shape);
		}


		/// <override></override>
		public override void Execute() {
			if (_removedPointId == ControlPointId.None) throw new NShapeException("ControlPointId.None is not a valid vertex to remove.");
			// Store point position if not done yet
			if (!Geometry.IsValid(_p)) _p = Shape.GetControlPointPosition(_removedPointId);
			((ILinearShape)Shape).RemoveVertex(_removedPointId);
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override void Revert() {
			if (_removedPointId == ControlPointId.None) throw new NShapeException("ControlPointId.None is not a valid vertex to add.");
			ControlPointId id = ControlPointId.None;
			if (Shape is LineShapeBase) {
				id = ((LineShapeBase)Shape).InsertVertex(_nextPointId, _removedPointId, _p.X, _p.Y);
				Debug.Assert(id == _removedPointId);
			} else id = ((ILinearShape)Shape).InsertVertex(_nextPointId, _p.X, _p.Y);
			foreach (KeyValuePair<ControlPointId, Point> item in _ctrlPointPositions) {
				if (item.Key == _removedPointId || item.Key == id) continue;
				Shape.MoveControlPointTo(item.Key, item.Value.X, item.Value.Y, ResizeModifiers.None);
			}
			if (Repository != null) Repository.Update(Shape);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		#region Fields
		private Point _p = Geometry.InvalidPoint;
		private ControlPointId _removedPointId = ControlPointId.None;
		private ControlPointId _nextPointId = ControlPointId.None;
		private Dictionary<ControlPointId, Point> _ctrlPointPositions = null;
		#endregion
	}

	#endregion


	#region Commands for inserting/deleting/editing ModelObjects

	/// <ToBeCompleted></ToBeCompleted>
	[Obsolete("Use CreateModelObjectsCommand instead.")]
	public class InsertModelObjectsCommand : CreateModelObjectsCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public InsertModelObjectsCommand(IModelObject modelObject)
			: base(modelObject) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InsertModelObjectsCommand(IRepository repository, IModelObject modelObject)
			: base(repository, modelObject) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InsertModelObjectsCommand(IEnumerable<IModelObject> modelObjects)
			: base(null, modelObjects) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InsertModelObjectsCommand(IRepository repository, IEnumerable<IModelObject> modelObjects)
			: base(repository, modelObjects) {
		}

	}


	/// <summary>
	/// Command for inserting model objects into the model and into the repository.
	/// </summary>
	public class CreateModelObjectsCommand : InsertOrRemoveModelObjectsCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public CreateModelObjectsCommand(IModelObject modelObject)
			: this(SingleInstanceEnumerator<IModelObject>.Create(modelObject)) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateModelObjectsCommand(IRepository repository, IModelObject modelObject)
			: this(repository, SingleInstanceEnumerator<IModelObject>.Create(modelObject)) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateModelObjectsCommand(IEnumerable<IModelObject> modelObjects)
			: this(null, modelObjects) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateModelObjectsCommand(IRepository repository, IEnumerable<IModelObject> modelObjects)
			: base(repository) {
			_modelObjectBuffer = new List<IModelObject>(modelObjects);
		}


		/// <override></override>
		public override void Execute() {
			if (ModelObjects.Count == 0) SetModelObjects(_modelObjectBuffer);
			InsertModelObjects(true);
		}


		/// <override></override>
		public override void Revert() {
			RemoveModelObjects(true);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Insert; }
		}


		// ToDO: Remove this buffer as soon as the ModelObject gets a Children Property...
		private List<IModelObject> _modelObjectBuffer;
	}


	/// <summary>
	/// Command for removing model objects from the model and from the repository.
	/// </summary>
	public class DeleteModelObjectsCommand : InsertOrRemoveModelObjectsCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public DeleteModelObjectsCommand(IModelObject modelObject)
			: this(null, SingleInstanceEnumerator<IModelObject>.Create(modelObject)) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteModelObjectsCommand(IRepository repository, IModelObject modelObject)
			: this(repository, SingleInstanceEnumerator<IModelObject>.Create(modelObject)) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteModelObjectsCommand(IEnumerable<IModelObject> modelObjects)
			: this(null, modelObjects) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteModelObjectsCommand(IRepository repository, IEnumerable<IModelObject> modelObjects)
			: base(repository) {
			_modelObjectBuffer = new List<IModelObject>(modelObjects);
		}


		/// <override></override>
		public override void Execute() {
			if (ModelObjects.Count == 0) SetModelObjects(_modelObjectBuffer);
			RemoveModelObjects(true);
		}


		/// <override></override>
		public override void Revert() {
			InsertModelObjects(true);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Delete; }
		}


		// ToDO: Remove this buffer as soon as the ModelObject gets a Children Property...
		private List<IModelObject> _modelObjectBuffer;
	}


	/// <summary>
	/// Command for assigning the parent of a model object.
	/// </summary>
	public class SetModelObjectParentCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		public SetModelObjectParentCommand(IModelObject modelObject, IModelObject parent)
			: this(null, modelObject, parent) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public SetModelObjectParentCommand(IRepository repository, IModelObject modelObject, IModelObject parent)
			: base(repository) {
			if (modelObject == null) throw new ArgumentNullException("modelObject");
			if (modelObject.Parent != null && parent == null)
				this.Description = string.Format(
					Dataweb.NShape.Properties.Resources.MessageFmt_Remove01FromHierarchicalPositionUnder23,
					modelObject.Type.Name, modelObject.Name,
					modelObject.Parent.Type.Name, modelObject.Parent.Name);
			else if (modelObject.Parent == null && parent != null)
				this.Description = string.Format(
					Dataweb.NShape.Properties.Resources.MessageFmt_Move01ToHierarchicalPositionUnder23,
					modelObject.Type.Name, modelObject.Name,
					parent.Type.Name, parent.Name);
			else if (modelObject.Parent != null && parent != null)
				this.Description = string.Format(
					Dataweb.NShape.Properties.Resources.MessageFmt_ChangeHierarchicalPositionOf01From23To45,
					modelObject.Type.Name, modelObject.Name,
					modelObject.Parent.Type.Name, modelObject.Parent.Name,
					parent.Type.Name, parent.Name);
			else this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Move01, modelObject.Type.Name, modelObject.Name);

			this._modelObject = modelObject;
			this._oldParent = modelObject.Parent;
			this._newParent = parent;
		}


		/// <override></override>
		public override void Execute() {
			_modelObject.Parent = _newParent;
			if (Repository != null) Repository.UpdateOwner(_modelObject, _newParent);
		}


		/// <override></override>
		public override void Revert() {
			_modelObject.Parent = _oldParent;
			if (Repository != null) Repository.UpdateOwner(_modelObject, _oldParent);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Data; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, _newParent.Shapes);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		#region Fields
		private IModelObject _modelObject;
		private IModelObject _oldParent;
		private IModelObject _newParent;
		#endregion
	}


	/// <summary>
	/// Command for assigning a model object to a shape.
	/// </summary>
	public class AssignModelObjectCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		public AssignModelObjectCommand(Shape shape, IModelObject modelObject)
			: this(null, shape, modelObject) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AssignModelObjectCommand(IRepository repository, Shape shape, IModelObject modelObject)
			: base(repository) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (modelObject == null) throw new ArgumentNullException("modelObject");

			if (shape.ModelObject == null)
				this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Assign01To2, modelObject.Type.Name, modelObject.Name, shape.Type.Name);
			else
				this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Replace01Of2With34, shape.ModelObject.Type.Name, shape.ModelObject.Name, shape.Type.Name, modelObject.Type.Name, modelObject.Name);

			this._shape = shape;
			this._oldModelObject = shape.ModelObject;
			this._newModelObject = modelObject;
		}


		/// <override></override>
		public override void Execute() {
			_shape.ModelObject = _newModelObject;
			if (Repository != null) Repository.Update(_shape);
		}


		/// <override></override>
		public override void Revert() {
			_shape.ModelObject = _oldModelObject;
			if (Repository != null) Repository.Update(_shape);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Data; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, _shape);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		#region Fields
		private Shape _shape;
		private IModelObject _oldModelObject;
		private IModelObject _newModelObject;
		#endregion
	}

	#endregion


	#region Commands for editing Templates

	/// <ToBeCompleted></ToBeCompleted>
	[Obsolete("Use CopyTemplateFromTemplateCommand instead.")]
	public class ExchangeTemplateCommand : CopyTemplateFromTemplateCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public ExchangeTemplateCommand(Template originalTemplate, Template changedTemplate)
			: base(originalTemplate, changedTemplate) {
		}

		/// <ToBeCompleted></ToBeCompleted>
		public ExchangeTemplateCommand(IRepository repository, Template originalTemplate, Template changedTemplate)
			: base(repository, originalTemplate, changedTemplate) {
		}

	}


	/// <summary>
	/// Command for copying all properties and objects of a template into an other template.
	/// </summary>
	public class CopyTemplateFromTemplateCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		public CopyTemplateFromTemplateCommand(Template originalTemplate, Template changedTemplate)
			: this(null, originalTemplate, changedTemplate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CopyTemplateFromTemplateCommand(IRepository repository, Template originalTemplate, Template changedTemplate)
			: base(repository) {
			if (originalTemplate == null) throw new ArgumentNullException("originalTemplate");
			if (changedTemplate == null) throw new ArgumentNullException("changedTemplate");
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_ChangeTempate0, originalTemplate.Title);
			this._originalTemplate = originalTemplate;
			this._oldTemplate = new Template(originalTemplate.Name, originalTemplate.Shape.Clone());
			this._oldTemplate.CopyFrom(originalTemplate);
			this._newTemplate = new Template(changedTemplate.Name, changedTemplate.Shape.Clone());
			this._newTemplate.CopyFrom(changedTemplate);
		}


		/// <override></override>
		public override void Execute() {
			DoExchangeTemplates(_originalTemplate, _oldTemplate, _newTemplate);
		}


		/// <override></override>
		public override void Revert() {
			DoExchangeTemplates(_originalTemplate, _newTemplate, _oldTemplate);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Templates; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		private void DoExchangeTemplates(Template originalTemplate, Template oldTemplate, Template newTemplate) {
			if (Repository != null) {
				// Delete all existing model mappings
				Repository.Delete(originalTemplate.GetPropertyMappings());
				IModelObject originalModelObject = originalTemplate.Shape.ModelObject;
				// We delete the whole shape here in order to make sure that all deleted 
				// content of the shape is deleted from the repository.
				// Afterwards, the shape and its existing/new content will be undeleted/inserted.
				originalTemplate.Shape.ModelObject = null;		// Detach shape from model object				
				Repository.DeleteAll(originalTemplate.Shape);
				// Always delete the current model object if it has been changed
				if (originalModelObject != null && oldTemplate.Shape.ModelObject != newTemplate.Shape.ModelObject)
					Repository.Delete(originalModelObject);
			}
			// ToDo: Handle exchanging shapes of different ShapeTypes
			originalTemplate.CopyFrom(newTemplate);

			// Update template (does NOT include inserting/undeleting/updating the shape and model object)
			if (Repository != null) {
				// Insert/ update / undelete model object
				if (originalTemplate.Shape.ModelObject != null) {
					if (oldTemplate.Shape.ModelObject == newTemplate.Shape.ModelObject) {
						// Update model object 
						Repository.UpdateOwner(originalTemplate.Shape.ModelObject, originalTemplate);
						Repository.Update(originalTemplate.Shape.ModelObject);
					} else {
						// Insert / undelete model object if has been replaced
						if (CanUndeleteEntity(originalTemplate.Shape.ModelObject))
							Repository.Undelete(originalTemplate.Shape.ModelObject);
						else Repository.Insert(originalTemplate.Shape.ModelObject, originalTemplate);
					}
				}
				// Insert/undelete template shape
				if (CanUndeleteEntity(originalTemplate.Shape))
					Repository.Undelete(originalTemplate.Shape, originalTemplate);
				else Repository.Insert(originalTemplate.Shape, originalTemplate);
				// Insert template shape's children (CopyFrom() creates new children)
				if (originalTemplate.Shape.Children.Count > 0)
					Repository.InsertAll(originalTemplate.Shape.Children, originalTemplate.Shape);
				// Update template
				Repository.Update(originalTemplate);
				// Insert/undelete model mappings
				foreach (IModelMapping modelMapping in originalTemplate.GetPropertyMappings()) {
					if (CanUndeleteEntity(modelMapping))
						Repository.Undelete(modelMapping, originalTemplate);
					else Repository.Insert(modelMapping, originalTemplate);
				}
			}
			InvalidateShapesOfTemplate(originalTemplate);
		}


		private void DisposeShape(Shape shape) {
			if (shape.Children.Count > 0) {
				foreach (Shape childShape in shape.Children)
					DisposeShape(childShape);
			}
			shape.Dispose();
		}


		private void InvalidateShapesOfTemplate(Template template) {
			// Invalidate all changed selectedShapes
			if (Repository != null) {
				foreach (Diagram diagram in Repository.GetDiagrams())
					InvalidateShapesOfTemplate(template, diagram.Shapes);
			}
		}


		private void InvalidateShapesOfTemplate(Template template, IEnumerable<Shape> shapes) {
			foreach (Shape shape in shapes) {
				if (shape.Template == template)
					shape.NotifyStyleChanged(null);
				if (shape.Children.Count > 0)
					InvalidateShapesOfTemplate(template, shape.Children);
			}
		}


		#region Fields
		private Template _originalTemplate;
		private Template _oldTemplate;
		private Template _newTemplate;

		private List<ShapeConnectionInfo> _shapeConnectionInfos = new List<ShapeConnectionInfo>();
		#endregion
	}


	/// <summary>
	/// Command for inserting new templates into the repository.
	/// </summary>
	public class CreateTemplateCommand : TemplateCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public CreateTemplateCommand(Template template)
			: this(null, template) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateTemplateCommand(IRepository repository, Template template)
			: base(repository, template) {
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_CreateNewTempate0BasedOn1, template.Title, template.Shape.Type.Name);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateTemplateCommand(string templateName, Shape baseShape)
			: this(null, templateName, baseShape) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateTemplateCommand(IRepository repository, string templateName, Shape baseShape)
			: base(repository) {
			if (string.IsNullOrEmpty(templateName)) throw new ArgumentNullException("templateName");
			if (baseShape == null) throw new ArgumentNullException("baseShape");
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_CreateNewTempate0BasedOn1, templateName, baseShape.Type.Name);
			this._templateName = templateName;
			this._baseShape = baseShape;
		}


		/// <override></override>
		public override void Execute() {
			if (Template == null) {
				// Create a template shape and copy properties
				Shape templateShape = _baseShape.Type.CreateInstance();
				foreach (Shape childShape in _baseShape.Children.BottomUp)
					templateShape.Children.Add(childShape.Type.CreateInstance(), childShape.ZOrder);
				templateShape.CopyFrom(_baseShape);
				// Clone model object if necessary
				if (_baseShape.ModelObject != null) {
					IModelObject templateModelObject = _baseShape.ModelObject.Clone();
					templateShape.ModelObject = templateModelObject;
				}
				// Create the new template
				Template = new Template(_templateName, templateShape);
			}
			if (Repository != null) {
				if (CanUndeleteEntity(Template))
					Repository.UndeleteAll(Template);
				else Repository.InsertAll(Template);
			}
		}


		/// <override></override>
		public override void Revert() {
			if (Repository != null) {
				Repository.Delete(Template.GetPropertyMappings());
				Repository.DeleteAll(Template);
			}
		}


		#region Fields
		private string _templateName;
		private Shape _baseShape;
		#endregion
	}


	/// <summary>
	/// Command for deleting templates from the repository.
	/// </summary>
	public class DeleteTemplateCommand : TemplateCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public DeleteTemplateCommand(Template template)
			: this(null, template) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteTemplateCommand(IRepository repository, Template template)
			: base(repository, template) {
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_DeleteTempate0BasedOn1, template.Title, template.Shape.Type.Name);
		}


		/// <override></override>
		public override void Execute() {
			if (Repository != null) {
				Repository.Delete(Template.GetPropertyMappings());
				Repository.DeleteAll(Template);
			}
		}


		/// <override></override>
		public override void Revert() {
			if (Repository != null) {
				if (CanUndeleteEntity(Template))
					Repository.UndeleteAll(Template);
				else Repository.InsertAll(Template);
			}
		}

	}


	/// <summary>
	/// Command for replacing the shape of a template with a shape of another shape type (e.g. replace a rectangle shape with an ellipse shape).
	/// </summary>
	public class ExchangeTemplateShapeCommand : Command {

		/// <ToBeCompleted></ToBeCompleted>
		public ExchangeTemplateShapeCommand(Template originalTemplate, Template changedTemplate)
			: this(null, originalTemplate, changedTemplate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ExchangeTemplateShapeCommand(IRepository repository, Template originalTemplate, Template changedTemplate)
			: base(repository) {
			if (originalTemplate == null) throw new ArgumentNullException("originalTemplate");
			if (changedTemplate == null) throw new ArgumentNullException("changedTemplate");
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_ChangeShapeOfTempate0From1To2, originalTemplate.Title, originalTemplate.Shape.Type.Name, changedTemplate.Shape.Type.Name);
			this._originalTemplate = originalTemplate;
			this._oldTemplate = originalTemplate.Clone();	// Shape and ModelObject(!) are also cloned in this step
			this._newTemplate = changedTemplate;

			this._oldTemplateShape = originalTemplate.Shape;	// Should we store the orignial model object, too?
			this._newTemplateShape = changedTemplate.Shape;
			this._newTemplateShape.DisplayService = _oldTemplateShape.DisplayService;
		}


		/// <override></override>
		public override void Execute() {
			if (Repository != null && _shapesFromTemplate == null)
				_shapesFromTemplate = new List<ReplaceShapesBuffer>(GetShapesToReplace());

			// Copy all Properties of the new template to the reference of the original Template
			_originalTemplate.Shape = null;
			_originalTemplate.CopyFrom(_newTemplate);
			_originalTemplate.Shape = _newTemplateShape;
			// exchange oldShapes with newShapes
			int cnt = _shapesFromTemplate.Count;
			for (int i = 0; i < cnt; ++i) {
				ReplaceShapes(_shapesFromTemplate[i].diagram,
									_shapesFromTemplate[i].oldShape,
									_shapesFromTemplate[i].newShape,
									_shapesFromTemplate[i].oldConnections,
									_shapesFromTemplate[i].newConnections);
			}
			if (Repository != null)
				Repository.ReplaceTemplateShape(_originalTemplate, _oldTemplateShape, _newTemplateShape);
		}


		/// <override></override>
		public override void Revert() {
			_originalTemplate.Shape = null;
			_originalTemplate.CopyFrom(_oldTemplate);
			_originalTemplate.Shape = _oldTemplateShape;
			// exchange old shape with new Shape
			int cnt = _shapesFromTemplate.Count;
			for (int i = 0; i < cnt; ++i)
				ReplaceShapes(_shapesFromTemplate[i].diagram,
									_shapesFromTemplate[i].newShape,
									_shapesFromTemplate[i].oldShape,
									_shapesFromTemplate[i].newConnections,
									_shapesFromTemplate[i].oldConnections);

			if (Repository != null)
				Repository.ReplaceTemplateShape(_originalTemplate, _newTemplateShape, _oldTemplateShape);
		}


		private IEnumerable<ReplaceShapesBuffer> GetShapesToReplace() {
			// ToDo: In future versions, the cache should handle this by exchanging the loaded shapes at once and the unloaded shapes next time they are loaded
			// For now, find all Shapes in the Cache created from the changed Template and change their shape's type
			foreach (Diagram diagram in Repository.GetDiagrams()) {
				foreach (Shape shape in diagram.Shapes) {
					if (shape.Template == _originalTemplate) {
						// copy as much properties as possible from the old shape into the new shape
						ReplaceShapesBuffer buffer = new ReplaceShapesBuffer();
						buffer.diagram = diagram;
						buffer.oldShape = shape;
						// Create a new shape instance refering to the original template
						buffer.newShape = _newTemplate.Shape.Type.CreateInstance(_originalTemplate);
						buffer.newShape.CopyFrom(_newTemplate.Shape);	// Copy all properties of the new shape (Template will not be copied)
						buffer.oldConnections = new List<ShapeConnectionInfo>(shape.GetConnectionInfos(ControlPointId.Any, null));
						buffer.newConnections = new List<ShapeConnectionInfo>(buffer.oldConnections.Count);
						foreach (ShapeConnectionInfo sci in buffer.oldConnections) {
							// find a matching connection point...
							ControlPointId ptId = ControlPointId.None;
							if (sci.OwnPointId == ControlPointId.Reference)
								ptId = ControlPointId.Reference;	// Point-To-Shape connections are always possible
							else {
								// try to find a connection point with the same point id
								foreach (ControlPointId id in buffer.newShape.GetControlPointIds(ControlPointCapabilities.Connect)) {
									if (id == sci.OwnPointId) {
										ptId = id;
										break;
									}
								}
							}
							// if the desired point is not a valid ConnectionPoint, create a Point-To-Shape connection
							if (ptId == ControlPointId.None)
								ptId = ControlPointId.Reference;
							// store the new connectionInfo
							buffer.newConnections.Add(new ShapeConnectionInfo(ptId, sci.OtherShape, sci.OtherPointId));
						}

						// Make the new shape about the size of the original one
						Rectangle oldBounds = buffer.oldShape.GetBoundingRectangle(true);
						Rectangle newBounds = buffer.newShape.GetBoundingRectangle(true);
						if (newBounds != oldBounds)
							buffer.newShape.Fit(oldBounds.X, oldBounds.Y, oldBounds.Width, oldBounds.Height);

						yield return buffer;
					}
				}
			}
		}


		private void ReplaceShapes(Diagram diagram, Shape oldShape, Shape newShape, IEnumerable<ShapeConnectionInfo> oldConnections, IEnumerable<ShapeConnectionInfo> newConnections) {
			oldShape.Invalidate();
			// Detach model object from old shape before deleting the shape
			IModelObject origModelObject = oldShape.ModelObject;
			if (origModelObject != null)
				origModelObject.DetachShape(oldShape);
			// Disconnect all connections to the old shape
			foreach (ShapeConnectionInfo sci in oldConnections) {
				Debug.Assert(oldShape.IsConnected(ControlPointId.Any, null) != ControlPointId.None);
				if (sci.OtherShape.HasControlPointCapability(sci.OtherPointId, ControlPointCapabilities.Glue)) {
					sci.OtherShape.Disconnect(sci.OtherPointId);
					if (Repository != null) Repository.DeleteConnection(sci.OtherShape, sci.OtherPointId, oldShape, sci.OwnPointId);
				} else {
					oldShape.Disconnect(sci.OwnPointId);
					if (Repository != null) Repository.DeleteConnection(oldShape, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
				}
			}
			// Delete children of old template shape because the children will be discarded in CopyFrom()
			if (Repository != null) {
				if (oldShape.Children.Count > 0)
					Repository.DeleteAll(oldShape.Children);
			}
			// Exchange the shapes before deleting the old shape from the repository because it 
			// might be removed from the diagram in a "ShapeDeleted" event handler
			newShape.CopyFrom(oldShape);			// Template will not be copied
			newShape.ModelObject = origModelObject;	// Assign the original model object to the new shape
			diagram.Shapes.Replace(oldShape, newShape);
			if (Repository != null) {
				Repository.Delete(oldShape);
				if (CanUndeleteEntity(newShape))
					Repository.Undelete(newShape, diagram);
				else Repository.Insert(newShape, diagram);
				if (newShape.Children.Count > 0)
					Repository.InsertAll(newShape.Children, newShape);
			}
			//
			// Restore all connections to the new shape
			foreach (ShapeConnectionInfo sci in newConnections) {
				Debug.Assert(newShape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Connect) ||
							newShape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue));
				if (newShape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue)) {
					newShape.Connect(sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
					if (Repository != null) Repository.InsertConnection(newShape, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
				} else {
					sci.OtherShape.Connect(sci.OtherPointId, newShape, sci.OwnPointId);
					if (Repository != null) Repository.InsertConnection(sci.OtherShape, sci.OtherPointId, newShape, sci.OwnPointId);
				}
			}
			newShape.Invalidate();
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get {
				//return Permission.ModifyData | Permission.Present | Permission.Connect | Permission.Templates; 
				return Permission.Templates;
			}
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		#region Fields

		private struct ReplaceShapesBuffer : IEquatable<ReplaceShapesBuffer> {
			public Diagram diagram;
			public Shape oldShape;
			public Shape newShape;
			public List<ShapeConnectionInfo> oldConnections;
			public List<ShapeConnectionInfo> newConnections;
			public bool Equals(ReplaceShapesBuffer other) {
				return (other.diagram == this.diagram
					&& other.newConnections == this.newConnections
					&& other.newShape == this.newShape
					&& other.oldConnections == this.oldConnections
					&& other.oldShape == this.oldShape);
			}
		}

		private Template _originalTemplate;	// reference on the (original) Template which has to be changed
		private Template _oldTemplate;			// a clone of the original Template (needed for reverting the command)
		private Template _newTemplate;			// a clone of the new Template
		private Shape _oldTemplateShape;		// the original template shape
		private Shape _newTemplateShape;		// the new template shape
		private List<ReplaceShapesBuffer> _shapesFromTemplate = null;
		#endregion
	}

	#endregion


	#region Commands for Designs and Styles

	/// <summary>
	/// Command for inserting new designs into the repository.
	/// </summary>
	public class CreateDesignCommand : DesignCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public CreateDesignCommand(Design design)
			: this(null, design) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateDesignCommand(IRepository repository, Design design)
			: base(repository, design) {
			Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_CreateDesign0, design.Name);
			this.Design = design;
		}


		/// <override></override>
		public override void Execute() {
			if (Repository != null) {
				if (CanUndeleteEntity(Design))
					Repository.UndeleteAll(Design);
				else Repository.InsertAll(Design);
			}
		}


		/// <override></override>
		public override void Revert() {
			if (Repository != null) Repository.DeleteAll(Design);
		}

	}


	/// <summary>
	/// Command for removing designs from the repository.
	/// </summary>
	public class DeleteDesignCommand : DesignCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public DeleteDesignCommand(Design design)
			: this(null, design) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteDesignCommand(IRepository repository, Design design)
			: base(repository, design) {
			Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_DeleteDesign0, design.Name);
			this.Design = design;
		}


		/// <override></override>
		public override void Execute() {
			if (Repository != null) Repository.DeleteAll(Design);
		}


		/// <override></override>
		public override void Revert() {
			if (Repository != null) {
				if (CanUndeleteEntity(Design))
					Repository.UndeleteAll(Design);
				else Repository.InsertAll(Design);
			}
		}

	}


	/// <summary>
	/// Command for inserting new styles into the repository.
	/// </summary>
	public class CreateStyleCommand : DesignCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public CreateStyleCommand(Design design, Style style)
			: this(null, design, style) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateStyleCommand(IRepository repository, Design design, Style style)
			: base(repository, design) {
			if (style == null) throw new ArgumentNullException("style");
			Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_CreateStyle0, style.Title);
			this.Design = design;
			this._style = style;
		}


		/// <override></override>
		public override void Execute() {
			Design d = Design ?? Repository.GetDesign(null);
			d.AddStyle(_style);
			if (Repository != null) {
				if (CanUndeleteEntity(_style))
					Repository.Undelete(d, _style);
				if (CanUndeleteEntity(_style))
					Repository.Undelete(d, _style);
				else Repository.Insert(d, _style);
				Repository.Update(d);
			}
		}


		/// <override></override>
		public override void Revert() {
			Design d = Design ?? Repository.GetDesign(null);
			d.RemoveStyle(_style);
			if (Repository != null) {
				Repository.Delete(_style);
				Repository.Update(d);
			}
		}


		private Style _style;
	}


	/// <summary>
	/// Command for removing styles from the repository.
	/// </summary>
	public class DeleteStyleCommand : DesignCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public DeleteStyleCommand(Design design, Style style)
			: this(null, design, style) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DeleteStyleCommand(IRepository repository, Design design, Style style)
			: base(repository, design) {
			if (style == null) throw new ArgumentNullException("style");
			Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_DeleteStyle0, style.Title);
			this.Design = design;
			this._style = style;
		}


		/// <override></override>
		public override void Execute() {
			Design d = Design ?? Repository.GetDesign(null);
			d.RemoveStyle(_style);
			if (Repository != null) {
				Repository.Delete(_style);
				Repository.Update(d);
			}
		}


		/// <override></override>
		public override void Revert() {
			Design d = Design ?? Repository.GetDesign(null);
			d.AddStyle(_style);
			if (Repository != null) {
				if (CanUndeleteEntity(_style))
					Repository.Undelete(d, _style);
				else Repository.Insert(d, _style);
				Repository.Update(d);
			}
		}


		private Style _style;
	}

	#endregion


	#region Commands for Diagrams and Layers

	/// <ToBeCompleted></ToBeCompleted>
	[Obsolete("Use CreateDiagramCommand instead")]
	public class InsertDiagramCommand : CreateDiagramCommand {
		/// <ToBeCompleted></ToBeCompleted>
		public InsertDiagramCommand(Diagram diagram)
			: base(diagram) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public InsertDiagramCommand(IRepository repository, Diagram diagram)
			: base(repository, diagram) {
		}

	}


	/// <summary>
	/// Command for adding a layer to a diagram.
	/// </summary>
	public class AddLayerCommand : InsertOrRemoveLayerCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public AddLayerCommand(Diagram diagram, string layerName)
			: this(null, diagram, layerName) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AddLayerCommand(IRepository repository, Diagram diagram, string layerName)
			: base(repository, diagram, layerName) {
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_AddLayer0, layerName);
		}


		/// <override></override>
		public override void Execute() {
			AddLayers();
		}

		/// <override></override>
		public override void Revert() {
			RemoveLayers();
		}

	}


	/// <ToBeCompleted></ToBeCompleted>
	public abstract class ShapeLayersCommand : ShapesCommand {

		/// <ToBeCompleted></ToBeCompleted>
		protected ShapeLayersCommand(Diagram diagram, Shape shape) 
			: this(null, diagram, shape) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ShapeLayersCommand(Diagram diagram, IEnumerable<Shape> shapes)
			: this(null, diagram, shapes) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ShapeLayersCommand(IRepository repository, Diagram diagram, Shape shape) 
			: this(repository, diagram, EnumerationHelper.Enumerate(shape)) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ShapeLayersCommand(IRepository repository, Diagram diagram, IEnumerable<Shape> shapes)
			: base(repository, shapes) {
			Diagram = diagram;
			OriginalLayers = new List<LayerInfo>(Shapes.Count);
			foreach (Shape shape in Shapes) 
				OriginalLayers.Add(LayerInfo.Create(shape.HomeLayer, shape.SupplementalLayers));
		}


		/// <override></override>
		public override void Revert() {
			if (Shapes.Count > 0) {
				for (int i = Shapes.Count - 1; i >= 0; --i) {
					Shapes[i].SupplementalLayers = LayerIds.None;
					Shapes[i].HomeLayer = Layer.NoLayerId;
					Diagram.AddShapeToLayers(Shapes[i], OriginalLayers[i].HomeLayer, OriginalLayers[i].SupplementalLayers);
				}
				if (Repository != null) Repository.Update(Shapes);
			}
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Diagram Diagram { get; private set; }


		/// <ToBeCompleted></ToBeCompleted>
		protected List<LayerInfo> OriginalLayers { get; private set; }


		/// <ToBeCompleted></ToBeCompleted>
		protected int HomeLayer {
			get { return _homeLayer; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected LayerIds Layers {
			get { return _supplementalLayers; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void Construct(int homeLayer, LayerIds supplementalLayers) {
			_homeLayer = homeLayer;
			_supplementalLayers = supplementalLayers;
		}


		private int _homeLayer;
		private LayerIds _supplementalLayers;
	}


	/// <summary>
	/// Command for adding shapes to a layer of a diagram.
	/// </summary>
	public class AddShapesToLayersCommand : ShapeLayersCommand {

		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public AddShapesToLayersCommand(Diagram diagram, Shape shape, LayerIds layers)
			: this(null, diagram, SingleInstanceEnumerator<Shape>.Create(shape), layers) {
		}

		///// <ToBeCompleted></ToBeCompleted>
		//public AddShapesToLayersCommand(Diagram diagram, Shape shape, IEnumerable<int> layerIds)
		//    : this(null, diagram, shape, layerIds) {
		//}

		/// <ToBeCompleted></ToBeCompleted>
		public AddShapesToLayersCommand(Diagram diagram, Shape shape, int homeLayer, LayerIds supplementalLayers)
			: this(null, diagram, SingleInstanceEnumerator<Shape>.Create(shape), homeLayer, supplementalLayers) {
		}

		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public AddShapesToLayersCommand(IRepository repository, Diagram diagram, Shape shape, LayerIds layers)
			: this(repository, diagram, SingleInstanceEnumerator<Shape>.Create(shape), layers) {
		}


		///// <ToBeCompleted></ToBeCompleted>
		//public AddShapesToLayersCommand(IRepository repository, Diagram diagram, Shape shape, IEnumerable<int> layerIds)
		//    : this(repository, diagram, EnumerationHelper.Enumerate(shape), layerIds) {
		//}


		/// <ToBeCompleted></ToBeCompleted>
		public AddShapesToLayersCommand(IRepository repository, Diagram diagram, Shape shape, int homeLayer, LayerIds supplementalLayers)
			: this(repository, diagram, SingleInstanceEnumerator<Shape>.Create(shape), homeLayer, supplementalLayers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public AddShapesToLayersCommand(Diagram diagram, IEnumerable<Shape> shapes, LayerIds layers)
			: this(null, diagram, shapes, layers) {
		}


		///// <ToBeCompleted></ToBeCompleted>
		//public AddShapesToLayersCommand(Diagram diagram, IEnumerable<Shape> shapes, IEnumerable<int> layerIds)
		//    : this(null, diagram, shapes, layerIds) {
		//}


		/// <ToBeCompleted></ToBeCompleted>
		public AddShapesToLayersCommand(Diagram diagram, IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers)
			: this(null, diagram, shapes, homeLayer, supplementalLayers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public AddShapesToLayersCommand(IRepository repository, Diagram diagram, IEnumerable<Shape> shapes, LayerIds layers)
			: this(repository, diagram, shapes, Layer.NoLayerId, layers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AddShapesToLayersCommand(IRepository repository, Diagram diagram, IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers)
			: base(repository, diagram, shapes) {
			Construct(homeLayer, supplementalLayers);
		}


		/// <override></override>
		public override void Execute() {
			Diagram.AddShapesToLayers(Shapes, HomeLayer, Layers);
			if (Repository != null) Repository.Update(Shapes);
		}

	}


	/// <summary>
	/// Command for assigning shapes to a layer of a diagram, thus replacing the current layer assignments of the shape.
	/// </summary>
	public class AssignShapesToLayersCommand : ShapeLayersCommand {

		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public AssignShapesToLayersCommand(Diagram diagram, Shape shape, LayerIds layers)
			: this(null, diagram, shape, layers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AssignShapesToLayersCommand(Diagram diagram, Shape shape, int homeLayer, LayerIds supplementalLayers)
			: this(null, diagram, shape, Layer.NoLayerId, supplementalLayers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public AssignShapesToLayersCommand(IRepository repository, Diagram diagram, Shape shape, LayerIds layers)
			: this(repository, diagram, SingleInstanceEnumerator<Shape>.Create(shape), layers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AssignShapesToLayersCommand(IRepository repository, Diagram diagram, Shape shape, int homeLayer, LayerIds supplementalLayers)
			: this(repository, diagram, SingleInstanceEnumerator<Shape>.Create(shape), homeLayer, supplementalLayers) {
		}


		///// <ToBeCompleted></ToBeCompleted>
		//public AssignShapesToLayersCommand(IRepository repository, Diagram diagram, Shape shape, IEnumerable<int> layerIds)
		//    : this(repository, diagram, SingleInstanceEnumerator<Shape>.Create(shape), layerIds) {
		//}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public AssignShapesToLayersCommand(Diagram diagram, IEnumerable<Shape> shapes, LayerIds layers)
			: this(null, diagram, shapes, layers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AssignShapesToLayersCommand(Diagram diagram, IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers)
			: this(null, diagram, shapes, homeLayer, supplementalLayers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public AssignShapesToLayersCommand(IRepository repository, Diagram diagram, IEnumerable<Shape> shapes, LayerIds layers)
			: this(repository, diagram, shapes, Layer.NoLayerId, layers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public AssignShapesToLayersCommand(IRepository repository, Diagram diagram, IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers)
			: base(repository, diagram, shapes) {
			Construct(homeLayer, supplementalLayers);
		}


		/// <override></override>
		public override void Execute() {
			if (Shapes.Count > 0) {
				foreach (Shape shape in Shapes) {
					shape.SupplementalLayers = LayerIds.None;
					shape.HomeLayer = Layer.NoLayerId;
					Diagram.AddShapeToLayers(shape, HomeLayer, Layers);
				}
				if (Repository != null) Repository.Update(Shapes);
			}
		}

	}


	/// <summary>
	/// Command for inserting new diagrams (including its shapes) into the repository.
	/// </summary>
	public class CreateDiagramCommand : DiagramCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public CreateDiagramCommand(Diagram diagram)
			: this(null, diagram) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CreateDiagramCommand(IRepository repository, Diagram diagram)
			: base(repository, diagram) {
			Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_CreateDiagram0, diagram.Title);
		}


		/// <override></override>
		public override void Execute() {
			if (Repository != null) {
				if (CanUndeleteEntity(Diagram))
					Repository.UndeleteAll(Diagram);
				else Repository.InsertAll(Diagram);
			}
		}


		/// <override></override>
		public override void Revert() {
			if (Repository != null) Repository.DeleteAll(Diagram);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Insert; }
		}

	}


	/// <summary>
	/// Command for deleting a diagram (including its shapes) from the repository.
	/// </summary>
	public class DeleteDiagramCommand : DiagramCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public DeleteDiagramCommand(Diagram diagram)
			: this(null, diagram) {
		}

		/// <ToBeCompleted></ToBeCompleted>
		public DeleteDiagramCommand(IRepository repository, Diagram diagram)
			: base(repository, diagram) {
			this._shapes = new ShapeCollection(diagram.Shapes);
		}


		/// <override></override>
		public override void Execute() {
			if (Repository != null) Repository.DeleteAll(Diagram);
		}


		/// <override></override>
		public override void Revert() {
			if (Repository != null) {
				if (Diagram.Shapes.Count == 0)
					Diagram.Shapes.AddRange(_shapes);
				if (CanUndeleteEntity(Diagram))
					Repository.UndeleteAll(Diagram);
				else Repository.InsertAll(Diagram);
			}
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Delete; }
		}


		private ShapeCollection _shapes;
	}


	/// <summary>
	/// Command for editing properties of a diagram's layer.
	/// </summary>
	public class EditLayerCommand : DiagramCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public EditLayerCommand(Diagram diagram, Layer layer, ChangedProperty property, string oldValue, string newValue)
			: this(null, diagram, layer, property, oldValue, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public EditLayerCommand(IRepository repository, Diagram diagram, Layer layer, ChangedProperty property, string oldValue, string newValue)
			: base(repository, diagram) {
			if (newValue == null) throw new ArgumentNullException("newValue");
			if (property == ChangedProperty.LowerZoomThreshold || property == ChangedProperty.UpperZoomThreshold)
				throw new ArgumentException("property");
			this._oldStrValue = oldValue;
			this._newStrValue = newValue;
			Construct(layer, property);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public EditLayerCommand(Diagram diagram, Layer layer, ChangedProperty property, int oldValue, int newValue)
			: this(null, diagram, layer, property, oldValue, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public EditLayerCommand(IRepository repository, Diagram diagram, Layer layer, ChangedProperty property, int oldValue, int newValue)
			: base(repository, diagram) {
			if (property == ChangedProperty.Name || property == ChangedProperty.Title)
				throw new ArgumentException("property");
			this._oldIntValue = oldValue;
			this._newIntValue = newValue;
			Construct(layer, property);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public enum ChangedProperty {
			/// <ToBeCompleted></ToBeCompleted>
			Name,
			/// <ToBeCompleted></ToBeCompleted>
			Title,
			/// <ToBeCompleted></ToBeCompleted>
			LowerZoomThreshold,
			/// <ToBeCompleted></ToBeCompleted>
			UpperZoomThreshold
		}


		/// <ToBeCompleted></ToBeCompleted>
		public override void Execute() {
			switch (_changedProperty) {
				case ChangedProperty.Name:
					Diagram.Layers.RenameLayer(_layer.Name, _newStrValue);
					break;
				case ChangedProperty.Title:
					_layer.Title = _newStrValue;
					break;
				case ChangedProperty.LowerZoomThreshold:
					_layer.LowerZoomThreshold = _newIntValue;
					break;
				case ChangedProperty.UpperZoomThreshold:
					_layer.UpperZoomThreshold = _newIntValue;
					break;
				default:
					Debug.Fail("Unhandled switch case!");
					break;
			}
			if (Repository != null) Repository.Update(Diagram);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public override void Revert() {
			switch (_changedProperty) {
				case ChangedProperty.Name:
					Diagram.Layers.RenameLayer(_layer.Name, _oldStrValue);
					break;
				case ChangedProperty.Title:
					_layer.Title = _oldStrValue;
					break;
				case ChangedProperty.LowerZoomThreshold:
					_layer.LowerZoomThreshold = _oldIntValue;
					break;
				case ChangedProperty.UpperZoomThreshold:
					_layer.UpperZoomThreshold = _oldIntValue;
					break;
				default:
					Debug.Fail("Unhandled switch case!");
					break;
			}
			if (Repository != null) Repository.Update(Diagram);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		private void Construct(Layer layer, ChangedProperty property) {
			this._layer = layer;
			this._changedProperty = property;
			string quotes, oldVal, newVal;
			if (property == ChangedProperty.Name || property == ChangedProperty.Title) {
				quotes = "'";
				oldVal = _oldStrValue;
				newVal = _newStrValue;
			} else {
				quotes = string.Empty;
				oldVal = _oldIntValue.ToString();
				newVal = _newIntValue.ToString();
			}
			Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_ChangeLayerProperty0From121To131, property, quotes, oldVal, newVal);
		}


		private Layer _layer = null;
		private ChangedProperty _changedProperty;
		private string _oldStrValue, _newStrValue;
		private int _oldIntValue, _newIntValue;
	}


	/// <summary>
	/// Command for deleting a layer from a diagram.
	/// </summary>
	public class RemoveLayerCommand : InsertOrRemoveLayerCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public RemoveLayerCommand(Diagram diagram, Layer layer)
			: this(null, diagram, layer) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RemoveLayerCommand(IRepository repository, Diagram diagram, Layer layer)
			: base(repository, diagram, layer) {
			Construct();
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_RemoveLayer0FromDiagram1, layer.Title, diagram.Title);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RemoveLayerCommand(Diagram diagram, IEnumerable<Layer> layers)
			: this(null, diagram, layers) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RemoveLayerCommand(IRepository repository, Diagram diagram, IEnumerable<Layer> layers)
			: base(repository, diagram, layers) {
			Construct();
			this.Description = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Remove0LayersFromDiagram1, this.Layers.Count, diagram.Title);
		}


		/// <override></override>
		public override void Execute() {
			LayerIds layers = Layer.ConvertToLayerIds(_layerIds);
			foreach (Shape shape in _affectedShapes)
				Diagram.RemoveShapeFromLayers(shape, _layerIds.Contains(shape.HomeLayer) ? shape.HomeLayer : Layer.NoLayerId, layers);
			if (Repository != null) 
				Repository.Update(_affectedShapes);
			RemoveLayers();
		}


		/// <override></override>
		public override void Revert() {
			AddLayers();
			RestoreShapeLayers();
		}


		private void Construct() {
			_layerIds = new HashCollection<int>(LayerHelper.GetAllLayerIds(Layers));

			_affectedShapes = new List<Shape>();
			_originalLayers = new List<LayerInfo>();
			foreach (Shape shape in Diagram.Shapes) {
				if (_layerIds.Contains(shape.HomeLayer) || _layerIds.ContainsAny(LayerHelper.GetAllLayerIds(shape.SupplementalLayers))) {
					_affectedShapes.Add(shape);
					_originalLayers.Add(LayerInfo.Create(shape.HomeLayer, shape.SupplementalLayers));
				}
			}
		}


		private void RestoreShapeLayers() {
			int cnt = _affectedShapes.Count;
			for (int i = 0; i < cnt; ++i)
				Diagram.AddShapeToLayers(_affectedShapes[i], _originalLayers[i].HomeLayer, _originalLayers[i].SupplementalLayers);
			if (Repository != null) {
				Repository.Update(_affectedShapes);
				Repository.Update(Diagram);
			}
		}


		private HashCollection<int> _layerIds;
		private List<Shape> _affectedShapes;
		private List<LayerInfo> _originalLayers;
	}


	/// <summary>
	/// Command for removing shapes from a diagram's layer.
	/// </summary>
	public class RemoveShapesFromLayersCommand : ShapesCommand {

		/// <ToBeCompleted></ToBeCompleted>
		public RemoveShapesFromLayersCommand(Diagram diagram, Shape shape)
			: this(null, diagram, SingleInstanceEnumerator<Shape>.Create(shape)) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RemoveShapesFromLayersCommand(IRepository repository, Diagram diagram, Shape shape)
			: this(repository, diagram, SingleInstanceEnumerator<Shape>.Create(shape)) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RemoveShapesFromLayersCommand(Diagram diagram, IEnumerable<Shape> shapes)
			: this(null, diagram, shapes, LayerHelper.GetAllLayerIds(diagram.Layers)) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RemoveShapesFromLayersCommand(IRepository repository, Diagram diagram, IEnumerable<Shape> shapes)
			: this(repository, diagram, shapes, LayerHelper.GetAllLayerIds(diagram.Layers)) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public RemoveShapesFromLayersCommand(Diagram diagram, Shape shape, LayerIds removeFromLayerIds)
			: this(null, diagram, shape, removeFromLayerIds) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RemoveShapesFromLayersCommand(Diagram diagram, Shape shape, IEnumerable<int> removeFromLayerIds)
			: this(null, diagram, shape, removeFromLayerIds) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public RemoveShapesFromLayersCommand(IRepository repository, Diagram diagram, Shape shape, LayerIds removeFromLayerIds)
			: this(repository, diagram, SingleInstanceEnumerator<Shape>.Create(shape), removeFromLayerIds) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RemoveShapesFromLayersCommand(IRepository repository, Diagram diagram, Shape shape, IEnumerable<int> removeFromLayerIds)
			: this(repository, diagram, SingleInstanceEnumerator<Shape>.Create(shape), removeFromLayerIds) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public RemoveShapesFromLayersCommand(Diagram diagram, IEnumerable<Shape> shapes, LayerIds removeFromLayerIds)
			: this(null, diagram, shapes, removeFromLayerIds) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public RemoveShapesFromLayersCommand(Diagram diagram, IEnumerable<Shape> shapes, IEnumerable<int> removeFromLayerIds)
			: this(null, diagram, shapes, removeFromLayerIds) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use an overload taking home layer and supplemental layers instead.")]
		public RemoveShapesFromLayersCommand(IRepository repository, Diagram diagram, IEnumerable<Shape> shapes, LayerIds removeFromLayerIds)
			: this(repository, diagram, shapes, LayerHelper.GetAllLayerIds(removeFromLayerIds)) {
		}
		

		/// <ToBeCompleted></ToBeCompleted>
		public RemoveShapesFromLayersCommand(IRepository repository, Diagram diagram, IEnumerable<Shape> shapes, IEnumerable<int> removeFromLayerIds)
			: base(repository, shapes) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this._diagram = diagram;
			_removeFromHomeLayers = new HashCollection<int>(removeFromLayerIds);
			_removeFromLayers = Layer.ConvertToLayerIds(removeFromLayerIds);
			_originalLayers = new List<LayerInfo>(Shapes.Count);
			foreach (Shape shape in shapes)
				_originalLayers.Add(LayerInfo.Create(shape.HomeLayer, shape.SupplementalLayers));
		}


		/// <override></override>
		public override void Execute() {
			if (Shapes.Count > 0) {
				foreach (Shape shape in Shapes) {
					int homeLayer = _removeFromHomeLayers.Contains(shape.HomeLayer) ? shape.HomeLayer : Layer.NoLayerId;
					_diagram.RemoveShapeFromLayers(shape, homeLayer, _removeFromLayers);
				}
				if (Repository != null) Repository.Update(Shapes);
			}
		}


		/// <override></override>
		public override void Revert() {
			if (Shapes.Count > 0) {
				for (int i = 0; i < Shapes.Count; ++i)
					_diagram.AddShapeToLayers(Shapes[i], _originalLayers[i].HomeLayer, _originalLayers[i].SupplementalLayers);
				if (Repository != null) Repository.Update(Shapes);
			}
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Layout; }
		}


		private Diagram _diagram;
		private HashCollection<int> _removeFromHomeLayers;
		private LayerIds _removeFromLayers;
		private List<LayerInfo> _originalLayers;
	}

	#endregion


	#region Commands for PropertyController

	/// <summary>
	/// Command for setting property values of a design.
	/// </summary>
	public class DesignPropertySetCommand : PropertySetCommand<Design> {

		/// <ToBeCompleted></ToBeCompleted>
		public DesignPropertySetCommand(IEnumerable<Design> modifiedDesigns, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(modifiedDesigns, propertyInfo, oldValues, newValues) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DesignPropertySetCommand(IRepository repository, IEnumerable<Design> modifiedDesigns, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(repository, modifiedDesigns, propertyInfo, oldValues, newValues) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DesignPropertySetCommand(IEnumerable<Design> modifiedDesigns, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(modifiedDesigns, propertyInfo, oldValues, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DesignPropertySetCommand(IRepository repository, IEnumerable<Design> modifiedDesigns, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(repository, modifiedDesigns, propertyInfo, oldValues, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DesignPropertySetCommand(Design modifiedDesign, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(modifiedDesign, propertyInfo, oldValue, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DesignPropertySetCommand(IRepository repository, Design modifiedDesign, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(repository, modifiedDesign, propertyInfo, oldValue, newValue) {
		}


		/// <override></override>
		public override void Execute() {
			base.Execute();
			if (Repository != null) {
				for (int i = ModifiedObjects.Count - 1; i >= 0; --i)
					Repository.Update(ModifiedObjects[i]);
			}
		}


		/// <override></override>
		public override void Revert() {
			base.Revert();
			if (Repository != null) {
				for (int i = ModifiedObjects.Count - 1; i >= 0; --i)
					Repository.Update(ModifiedObjects[i]);
			}
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Designs; }
		}


		//public override bool IsAllowed(ISecurityManager securityManager) {
		//    if (securityManager == null) throw new ArgumentNullException("securityManager");
		//    return securityManager.IsGranted(RequiredPermission);
		//}

	}


	/// <summary>
	/// Command for setting property values of a diagram.
	/// </summary>
	public class DiagramPropertySetCommand : PropertySetCommand<Diagram> {

		/// <ToBeCompleted></ToBeCompleted>
		public DiagramPropertySetCommand(IEnumerable<Diagram> modifiedDiagrams, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(modifiedDiagrams, propertyInfo, oldValues, newValues) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DiagramPropertySetCommand(IRepository repository, IEnumerable<Diagram> modifiedDiagrams, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(repository, modifiedDiagrams, propertyInfo, oldValues, newValues) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DiagramPropertySetCommand(IEnumerable<Diagram> modifiedDiagrams, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(modifiedDiagrams, propertyInfo, oldValues, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DiagramPropertySetCommand(IRepository repository, IEnumerable<Diagram> modifiedDiagrams, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(repository, modifiedDiagrams, propertyInfo, oldValues, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DiagramPropertySetCommand(Diagram modifiedDiagram, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(modifiedDiagram, propertyInfo, oldValue, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DiagramPropertySetCommand(IRepository repository, Diagram modifiedDiagram, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(repository, modifiedDiagram, propertyInfo, oldValue, newValue) {
		}


		/// <override></override>
		public override void Execute() {
			base.Execute();
			if (Repository != null) {
				for (int i = ModifiedObjects.Count - 1; i >= 0; --i)
					Repository.Update(ModifiedObjects[i]);
			}
		}


		/// <override></override>
		public override void Revert() {
			base.Revert();
			if (Repository != null) {
				for (int i = ModifiedObjects.Count - 1; i >= 0; --i)
					Repository.Update(ModifiedObjects[i]);
			}
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return (RequiredPermissions != Permission.None) ? RequiredPermissions : (Permission.Data | Permission.Present); }
		}


		//public override bool IsAllowed(ISecurityManager securityManager) {
		//    if (securityManager == null) throw new ArgumentNullException("securityManager");
		//    for (int i = modifiedObjects.Count - 1; i >= 0; --i) {
		//        if (!securityManager.IsGranted(RequiredPermission, modifiedObjects[i].SecurityDomainName))
		//            return false;
		//    }
		//    return true;
		//}

	}


	/// <summary>
	/// Command for setting property values of a layer.
	/// </summary>
	public class LayerPropertySetCommand : PropertySetCommand<Layer> {

		/// <ToBeCompleted></ToBeCompleted>
		public LayerPropertySetCommand(Diagram diagram, IEnumerable<Layer> modifiedLayers, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(modifiedLayers, propertyInfo, oldValues, newValues) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this._diagram = diagram;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LayerPropertySetCommand(IRepository repository, Diagram diagram, IEnumerable<Layer> modifiedLayers, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(repository, modifiedLayers, propertyInfo, oldValues, newValues) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this._diagram = diagram;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LayerPropertySetCommand(Diagram diagram, IEnumerable<Layer> modifiedLayers, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(modifiedLayers, propertyInfo, oldValues, newValue) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this._diagram = diagram;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LayerPropertySetCommand(IRepository repository, Diagram diagram, IEnumerable<Layer> modifiedLayers, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(repository, modifiedLayers, propertyInfo, oldValues, newValue) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this._diagram = diagram;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LayerPropertySetCommand(Diagram diagram, Layer modifiedLayer, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(modifiedLayer, propertyInfo, oldValue, newValue) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this._diagram = diagram;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public LayerPropertySetCommand(IRepository repository, Diagram diagram, Layer modifiedLayer, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(repository, modifiedLayer, propertyInfo, oldValue, newValue) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			this._diagram = diagram;
		}


		/// <override></override>
		public override void Execute() {
			base.Execute();
			if (Repository != null) Repository.Update(_diagram);
		}


		/// <override></override>
		public override void Revert() {
			base.Revert();
			if (Repository != null) Repository.Update(_diagram);
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return (RequiredPermissions != Permission.None) ? RequiredPermissions : Permission.Layout; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, _diagram.SecurityDomainName);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		#region Fields
		private Diagram _diagram;
		#endregion
	}


	/// <summary>
	/// Command for setting property values of a model object.
	/// </summary>
	public class ModelObjectPropertySetCommand : PropertySetCommand<IModelObject> {

		/// <ToBeCompleted></ToBeCompleted>
		public ModelObjectPropertySetCommand(IEnumerable<IModelObject> modifiedModelObjects, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(modifiedModelObjects, propertyInfo, oldValues, newValues) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ModelObjectPropertySetCommand(IRepository repository, IEnumerable<IModelObject> modifiedModelObjects, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(repository, modifiedModelObjects, propertyInfo, oldValues, newValues) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ModelObjectPropertySetCommand(IEnumerable<IModelObject> modifiedModelObjects, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(modifiedModelObjects, propertyInfo, oldValues, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ModelObjectPropertySetCommand(IRepository repository, IEnumerable<IModelObject> modifiedModelObjects, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(repository, modifiedModelObjects, propertyInfo, oldValues, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ModelObjectPropertySetCommand(IModelObject modifiedModelObject, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(modifiedModelObject, propertyInfo, oldValue, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ModelObjectPropertySetCommand(IRepository repository, IModelObject modifiedModelObject, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(repository, modifiedModelObject, propertyInfo, oldValue, newValue) {
		}


		/// <override></override>
		public override void Execute() {
			base.Execute();
			if (Repository != null) {
				if (ModifiedObjects.Count == 1) Repository.Update(ModifiedObjects[0]);
				else Repository.Update(ModifiedObjects);
			}
		}


		/// <override></override>
		public override void Revert() {
			base.Revert();
			if (Repository != null) {
				if (ModifiedObjects.Count == 1) Repository.Update(ModifiedObjects[0]);
				else Repository.Update(ModifiedObjects);
			}
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return (RequiredPermissions != Permission.None) ? RequiredPermissions : Permission.Data; }
		}


		//public override bool IsAllowed(ISecurityManager securityManager) {
		//    if (securityManager == null) throw new ArgumentNullException("securityManager");
		//    for (int i = modifiedObjects.Count - 1; i >= 0; --i) {
		//        if (!securityManager.IsGranted(RequiredPermission, modifiedObjects[i].Shapes))
		//            return false;
		//    }
		//    return true;
		//}

	}


	/// <summary>
	/// Command for setting property values of a shape.
	/// </summary>
	public class ShapePropertySetCommand : PropertySetCommand<Shape> {

		/// <ToBeCompleted></ToBeCompleted>
		public ShapePropertySetCommand(IEnumerable<Shape> modifiedShapes, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(modifiedShapes, propertyInfo, oldValues, newValues) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ShapePropertySetCommand(IRepository repository, IEnumerable<Shape> modifiedShapes, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(repository, modifiedShapes, propertyInfo, oldValues, newValues) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ShapePropertySetCommand(IEnumerable<Shape> modifiedShapes, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(modifiedShapes, propertyInfo, oldValues, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ShapePropertySetCommand(IRepository repository, IEnumerable<Shape> modifiedShapes, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(repository, modifiedShapes, propertyInfo, oldValues, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ShapePropertySetCommand(Shape modifiedShape, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(modifiedShape, propertyInfo, oldValue, newValue) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ShapePropertySetCommand(IRepository repository, Shape modifiedShape, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(repository, modifiedShape, propertyInfo, oldValue, newValue) {
		}


		/// <override></override>
		public override void Execute() {
			base.Execute();
			if (Repository != null) {
				if (ModifiedObjects.Count == 1) Repository.Update(ModifiedObjects[0]);
				else Repository.Update(ModifiedObjects);
			}
		}


		/// <override></override>
		public override void Revert() {
			base.Revert();
			if (Repository != null) {
				if (ModifiedObjects.Count == 1) Repository.Update(ModifiedObjects[0]);
				else Repository.Update(ModifiedObjects);
			}
		}


		///// <override></override>
		//public override Permission RequiredPermission {
		//    get { return Permission.Present | Permission.ModifyData | Permission.Layout; }
		//}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, ModifiedObjects);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}

	}


	/// <summary>
	/// Command for setting property values of a style.
	/// </summary>
	public class StylePropertySetCommand : PropertySetCommand<Style> {

		/// <ToBeCompleted></ToBeCompleted>
		public StylePropertySetCommand(Design design, IEnumerable<Style> modifiedStyles, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(modifiedStyles, propertyInfo, oldValues, newValues) {
			if (design == null) throw new ArgumentNullException("design");
			this._design = design;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public StylePropertySetCommand(IRepository repository, Design design, IEnumerable<Style> modifiedStyles, PropertyInfo propertyInfo, IEnumerable<object> oldValues, IEnumerable<object> newValues)
			: base(repository, modifiedStyles, propertyInfo, oldValues, newValues) {
			if (design == null) throw new ArgumentNullException("design");
			this._design = design;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public StylePropertySetCommand(Design design, IEnumerable<Style> modifiedStyles, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(modifiedStyles, propertyInfo, oldValues, newValue) {
			if (design == null) throw new ArgumentNullException("design");
			this._design = design;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public StylePropertySetCommand(IRepository repository, Design design, IEnumerable<Style> modifiedStyles, PropertyInfo propertyInfo, IEnumerable<object> oldValues, object newValue)
			: base(repository, modifiedStyles, propertyInfo, oldValues, newValue) {
			if (design == null) throw new ArgumentNullException("design");
			this._design = design;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public StylePropertySetCommand(Design design, Style modifiedStyle, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(modifiedStyle, propertyInfo, oldValue, newValue) {
			if (design == null) throw new ArgumentNullException("design");
			this._design = design;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public StylePropertySetCommand(IRepository repository, Design design, Style modifiedStyle, PropertyInfo propertyInfo, object oldValue, object newValue)
			: base(repository, modifiedStyle, propertyInfo, oldValue, newValue) {
			if (design == null) throw new ArgumentNullException("design");
			this._design = design;
		}


		/// <override></override>
		public override void Execute() {
			base.Execute();
			UpdateRepository();
		}


		/// <override></override>
		public override void Revert() {
			base.Revert();
			UpdateRepository();
		}


		/// <override></override>
		public override Permission RequiredPermission {
			get { return Permission.Designs; }
		}


		//public override bool IsAllowed(ISecurityManager securityManager) {
		//    if (securityManager == null) throw new ArgumentNullException("securityManager");
		//    return securityManager.IsGranted(RequiredPermission);
		//}


		private void UpdateRepository() {
			if (Repository != null) {
				for (int i = ModifiedObjects.Count - 1; i >= 0; --i)
					Repository.Update(ModifiedObjects[i]);
				Repository.Update(_design);
			}
		}


		#region Fields
		private Design _design;
		#endregion
	}

	#endregion

}
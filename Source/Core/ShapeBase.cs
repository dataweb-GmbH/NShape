/******************************************************************************
  Copyright 2009-2017 dataweb GmbH
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
using System.Drawing;
using System.Drawing.Drawing2D;

using Dataweb.NShape.Commands;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Base class for all shape implementations
	/// </summary>
	public abstract class ShapeBase : Shape, IShapeCollection, IReadOnlyShapeCollection {

		#region Shape Members

		/// <override></override>
		public override void CopyFrom(Shape source) {
			if (source == null) throw new ArgumentNullException("source");

			// Delete all cached graphical objects and structures
			InvalidateDrawCache();

			// Do *not* copy the template!
			// If a template is needed, create the new shape instance with the source's template.
			// If the template would be copied, all templates created from shapes would be dependent 
			// on the template of the original shape.
			// Instead of copying the template here, we've changed the implementations of the Clone
			// method: It will now create a shape instance with the source shape's template.
			//
			// Old version:
			// Copy template only if the shape has no template yet.
			//if (template == null) template = source.Template;

			// Copy base properties
			if (this._modelObject != null) this._modelObject.DetachShape(this);
			this._modelObject = source.ModelObject;
			if (this._modelObject != null) this._modelObject.AttachShape(this);

			this._tag = source.Tag;
			this._data = source.Data;

			// Copy templated properties
			_privateLineStyle = (Template != null && source.LineStyle == Template.Shape.LineStyle) ? null : source.LineStyle;
			_securityDomainName = (Template != null && source.SecurityDomainName == Template.Shape.SecurityDomainName) ? '\0' : source.SecurityDomainName;

			// Do not copy these properties:
			// - DisplayService
			// - Parent
			// - ZOrder 
			// - Layers

			MoveTo(source.X, source.Y);
			// Clone children if there are any
			if (source.Children != null && source.Children.Count > 0) {
				// Create children collection if it does not exist
				if (IsChildrenCollectionEmpty)
					ChildrenCollection = CreateChildrenCollection(source.Children.Count);
				// Optimization: Copy the children collection if the shape is a standard shape
				if (source is ShapeBase && ((ShapeBase)source).ChildrenCollection != null)
					this.ChildrenCollection.CopyFrom(((ShapeBase)source).ChildrenCollection);
				else this.ChildrenCollection.CopyFrom(source.Children);
			}
		}


		/// <override></override>
		public override void MakePreview(IStyleSet styleSet) {
			if (styleSet == null) throw new ArgumentNullException("styleSet");
			if (!IsChildrenCollectionEmpty) ChildrenCollection.SetPreviewStyles(styleSet);
			_privateLineStyle = styleSet.GetPreviewStyle(LineStyle);
			if (ModelObject != null) ModelObject.Name += " (Preview)";
			// ToDo: Add Suffix to modelObject's children, too
		}


		/// <override></override>
		public override bool HasStyle(IStyle style) {
			if (IsStyleAffected(LineStyle, style)) return true;
			if (!IsChildrenCollectionEmpty) {
				foreach (Shape childShape in ChildrenCollection)
					if (childShape.HasStyle(style)) return true;
			}
			return false;
		}


		/// <override></override>
		protected internal override bool NotifyStyleChanged(IStyle style) {
			bool result = false;
			if (style == null || HasStyle(style)) {
				// If the interior changes, a simple invalidate is sufficent
				if (style is IFillStyle || style is IColorStyle)
					Invalidate();
				else {
					// Character- and Paragraph style require recalculating the draw cache, 
					// Line- and CapStyles additionaly affect the size of the shape
					Invalidate();
					if (style == null || style is ILineStyle || style is ICapStyle) {
						_boundingRectangleLoose = _boundingRectangleTight = Geometry.InvalidRectangle;
						// Line style's line width affects the cells (of the shape map) occupied by 
						// the shape, so we have to notify the owner
						if (Owner != null) {
							Owner.NotifyChildResizing(this);
							Owner.NotifyChildResized(this);
						}
					}
					InvalidateDrawCache();
					UpdateDrawCache();
					Invalidate();
				}
				result = true;
			}
			if (!IsChildrenCollectionEmpty)
				if (ChildrenCollection.NotifyStyleChanged(style)) result = true;
			return result;
		}


		/// <override></override>
		public override ShapeType Type {
			get { return _shapeType; }
		}


		/// <override></override>
		public override Template Template {
			get { return _template; }
		}


		/// <override></override>
		public override IModelObject ModelObject {
			get { return _modelObject; }
			set {
				if (_modelObject != value) {
					IModelObject currModelObj = _modelObject;
					_modelObject = value;
					if (currModelObj != null) currModelObj.DetachShape(this);
					if (_modelObject != null) _modelObject.AttachShape(this);
				}
			}
		}


		/// <override></override>
		public override IShapeCollection Children {
			get { return (IShapeCollection)this; }
		}


		/// <override></override>
		public override Diagram Diagram {
			get {
				if (_owner is DiagramShapeCollection)
					return ((DiagramShapeCollection)_owner).Owner;
				else return null;
			}
			internal set {
				//if (owner != null && owner != value.Shapes) {
				if (_owner != null && _owner.Contains(this)) {
					_owner.Remove(this);
					_owner = null;
				}
				if (value != null) {
					if (value.Shapes is ShapeCollection)
						_owner = (ShapeCollection)value.Shapes;
					else throw new ArgumentException(string.Format("{0}'s Shapes property must be a {1}", value.GetType().Name, typeof(ShapeCollection).Name));
				} else _owner = null;
			}
		}


		/// <override></override>
		public override Shape Parent {
			get {
				if (_owner is ShapeAggregation) return ((ShapeAggregation)_owner).Owner;
				else return null;
			}
			set {
				if (value != null) {
					if (_owner != null && _owner != value.Children) {
						_owner.Remove(this);
						_owner = null;
					}
					if (value is ShapeBase)
						_owner = ((ShapeBase)value).ChildrenCollection;
					else if (value.Children is ShapeAggregation)
						_owner = (ShapeAggregation)value.Children;
					else {
						_owner = (value.Children as ShapeAggregation);
						if (_owner == null)
							throw new ArgumentException(string.Format("{0}'s Children property must be a {1}", value.GetType().Name, typeof(ShapeAggregation).Name));
					}
				} else _owner = null;
			}
		}


		/// <override></override>
		public override object Tag {
			get { return _tag; }
			set { _tag = value; }
		}


		/// <override></override>
		public override string Data {
			get { return _data; }
			set { _data = value; }
		}

		
		/// <override></override>
		public override char SecurityDomainName {
			get {
				if (_securityDomainName == '\0' && Template != null)
					return Template.Shape.SecurityDomainName;
				else return _securityDomainName;
			}
			set {
				if (value < 'A' || value > 'Z')
					throw new ArgumentException(Properties.Resources.MessageTxt_TheDomainQualifierHasToBeAnUpperCaseANSILetterAZ);
				if (Template != null && Template.Shape.SecurityDomainName == value)
					_securityDomainName = '\0';
				else _securityDomainName = value;
			}
		}


		/// <override></override>
		public override IEnumerable<MenuItemDef> GetMenuItemDefs(int mouseX, int mouseY, int range) {
			bool isFeasible = ContainsPoint(mouseX, mouseY);
			string description = Properties.Resources.MessageTxt_CreateANewTemplate;
			yield return new CommandMenuItemDef(ToolSetController.MenuItemNameCreateTemplateFromShape, 
				Properties.Resources.CaptionTxt_CreateTemplate, null, Color.Empty, description, false, isFeasible,
				new CreateTemplateCommand(string.Format("{0} {1}", Type.Name, GetHashCode()), this));

			if (_template != null) {
				// ToDo 3: Implement a ResetShapeStylesAction for resetting all styles of the shape to the template's default styles
			} else yield break;
		}


		/// <override></override>
		public override void NotifyModelChanged(int modelPropertyId) {
			Debug.Assert(ModelObject != null);
			// ToDo: 
			// If the shape is the template's shape, it has no template although there is a 
			// valid property mapping. Find a better solution for this case.
			if (Template != null) {
				IModelMapping propertyMapping = Template.GetPropertyMapping(modelPropertyId);
				if (propertyMapping != null) {
					// set model object value
					if (propertyMapping.CanSetInteger) {
						Debug.Assert(!propertyMapping.CanSetFloat && !propertyMapping.CanSetString);
						propertyMapping.SetInteger(ModelObject.GetInteger(propertyMapping.ModelPropertyId));
					} else if (propertyMapping.CanSetFloat) {
						Debug.Assert(!propertyMapping.CanSetInteger && !propertyMapping.CanSetString);
						propertyMapping.SetFloat(ModelObject.GetFloat(propertyMapping.ModelPropertyId));
					} else if (propertyMapping.CanSetString) {
						Debug.Assert(!propertyMapping.CanSetInteger && !propertyMapping.CanSetFloat);
						propertyMapping.SetString(ModelObject.GetString(propertyMapping.ModelPropertyId));
					} else throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageTxt_PropertyMappingCannotSetAnyOfTheSupportedTypes);
					// Convert model value with the help of the propertyMapping and set the new value
					ProcessExecModelPropertyChange(propertyMapping);
				}
			}
		}


		/// <override></override>
		public override Point CalculateNormalVector(int x, int y) {
			// ToDo: Remove this default implementation and implement this method in all classes that have a defined shape
			return Geometry.CalcNormalVectorOfRectangle(GetBoundingRectangle(true), x, y, 100);
		}


		#region Connecting

		/// <override></override>
		public override bool CanConnect(ControlPointId ownPointId, Shape otherShape, ControlPointId otherPointId) {
			String message;
			return CanConnect(ownPointId, otherShape, otherPointId, out message);
		}
	

		/// <summary>
		/// Establishes a connction between this shape and the other shape.
		/// </summary>
		public override void Connect(ControlPointId ownPointId, Shape otherShape, ControlPointId otherPointId) {
			if (otherShape == null) throw new ArgumentNullException("otherShape");
			String message;
			if (!CanConnect(ownPointId, otherShape, otherPointId, out message))
				throw new NShapeException(message);

			// Perform the connection operation
			ShapeConnectionInfo connectionInfo = ShapeConnectionInfo.Create(ownPointId, otherShape, otherPointId);
			if (connectionInfos == null) connectionInfos = new List<ShapeConnectionInfo>();
			if (!connectionInfos.Contains(connectionInfo)) {
				// Model objects will be connected by AtachGluePointToConnectionPoint()
				otherShape.AttachGluePointToConnectionPoint(otherPointId, this, ownPointId);
				connectionInfos.Add(connectionInfo);
			}
			// Make the GluePoint move to the target connection point
			FollowConnectionPointWithGluePoint(ownPointId, otherShape, otherPointId);
		}


		/// <summary>
		/// Called upon the active shape of the connection. E.g. by the tool.
		/// If ownPointId is 0, the global connection is meant.
		/// </summary>
		public override void Disconnect(ControlPointId gluePointId) {
			if (connectionInfos == null) return;
			for (int i = connectionInfos.Count - 1; i >= 0; --i) {
				ShapeConnectionInfo connectionInfo = connectionInfos[i];
				if (connectionInfo.OwnPointId == gluePointId) {
					// Model objects will be disconnected by DetachGluePointFromConnectionPoint
					connectionInfo.OtherShape.DetachGluePointFromConnectionPoint(connectionInfo.OtherPointId, this, gluePointId);
					connectionInfos.RemoveAt(i);
					if (connectionInfos.Count == 0) connectionInfos = null;
				}
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal abstract int ControlPointCount { get; }


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapabilities) {
			// Default implementation: Every control point is a connection point
			if (controlPointId == ControlPointId.None || controlPointId == ControlPointId.Any) return false;
			else return ((controlPointCapabilities & ControlPointCapabilities.Connect) > 0);
		}


		/// <summary>
		/// Returns all connections between this shape and the other shape. 
		/// If otherShape is null, the connections to all other shapes are returned.
		/// </summary>
		public override IEnumerable<ShapeConnectionInfo> GetConnectionInfos(ControlPointId ownPointId, Shape otherShape) {
			if (ownPointId == ControlPointId.None) throw new ArgumentException("ownPointId");
			if (connectionInfos == null) yield break;
			// We cannot use foreach here because the collection may be canged from outside.
			for (int i = connectionInfos.Count - 1; i >= 0; --i) {
				ShapeConnectionInfo connectionInfo = connectionInfos[i];
				if (otherShape != null && connectionInfo.OtherShape != otherShape)
					continue;
				if (ownPointId != ControlPointId.Any) {
					switch (ownPointId) {
						case ControlPointId.FirstVertex:
						case ControlPointId.LastVertex:
						case ControlPointId.Reference:
							if (GetControlPointIndex(ownPointId) != GetControlPointIndex(connectionInfo.OwnPointId))
								continue;
							break;
						default:
							if (ownPointId != connectionInfo.OwnPointId)
								continue;
							break;
					}
				}
				yield return connectionInfo;
			}
		}


		/// <summary>
		/// Returns all connections between this shape and the other shape. 
		/// If otherShape is null, the connections to all other shapes are returned.
		/// </summary>
		public override ShapeConnectionInfo GetConnectionInfo(ControlPointId gluePointId, Shape otherShape) {
			if (!HasControlPointCapability(gluePointId, ControlPointCapabilities.Glue))
				throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_TheGivenControlPointId0IsNotAGluePoint, gluePointId));
			if (gluePointId == ControlPointId.None) throw new ArgumentException("gluePointId");
			if (connectionInfos != null) {
				foreach (ShapeConnectionInfo connectionInfo in connectionInfos) {
					if (otherShape != null && connectionInfo.OtherShape != otherShape)
						continue;
					if (gluePointId != ControlPointId.Any) {
						switch (gluePointId) {
							case ControlPointId.FirstVertex:
							case ControlPointId.LastVertex:
							case ControlPointId.Reference:
								if (GetControlPointIndex(gluePointId) != GetControlPointIndex(connectionInfo.OwnPointId))
									continue;
								break;
							default:
								if (gluePointId != connectionInfo.OwnPointId)
									continue;
								break;
						}
					}
					return connectionInfo;
					//if ((otherShape == null || connectionInfo.OtherShape == otherShape)
					//    && GetControlPointIndex(gluePointId) == GetControlPointIndex(connectionInfo.OwnPointId))
					//    return connectionInfo;
				}
			}
			return ShapeConnectionInfo.Empty;
		}


		/// <override></override>
		public override ControlPointId IsConnected(ControlPointId ownPointId, Shape otherShape) {
			if (ownPointId == ControlPointId.None) throw new ArgumentException("ownPointId");
			if (connectionInfos != null) {
				foreach (ShapeConnectionInfo connectionInfo in connectionInfos) {
					if (otherShape != null && connectionInfo.OtherShape != otherShape)
						continue;
					if (ownPointId != ControlPointId.Any) {
						switch (ownPointId) {
							case ControlPointId.FirstVertex:
							case ControlPointId.LastVertex:
							case ControlPointId.Reference:
								if (GetControlPointIndex(ownPointId) != GetControlPointIndex(connectionInfo.OwnPointId))
									continue;
								break;
							default:
								if (ownPointId != connectionInfo.OwnPointId)
									continue;
								break;
						}
					}
					return connectionInfo.OtherPointId;
				}
			}
			return ControlPointId.None;
		}


		/// <summary>
		/// Informs the shape that posesses the GluePoint that the connection point of an active connection has been moved. 
		/// If the connection is a point-to-point connection, the shape moves the glue point to the given position. 
		/// If the connection is a point-to-shape connection, the shape calculates the new endpoint (i.e. position of the sticky point) with the help of the connected shape. 
		/// If this <see cref="T:Dataweb.NShape.Advanced.Shape" /> is a <see cref="T:Dataweb.NShape.Advanced.ILinearShape" />, it calls CalcGluePoint to determine the new sticky point position.
		/// In all cases, if the glue point cannot be moved to the new required position because of constraints, the connection is dissolved.
		/// </summary>
		public override void FollowConnectionPointWithGluePoint(ControlPointId gluePointId, Shape connectedShape, ControlPointId movedPointId) {
			Debug.Assert(connectionInfos != null);
			Rectangle boundsBefore = GetBoundingRectangle(true);
			BeginResize();

			try {
				isGluePointFollowingConnectionPoint = true;
				DoMoveConnectedGluePoint(gluePointId, connectedShape, movedPointId);
			} finally {
				isGluePointFollowingConnectionPoint = false;
			}

			Rectangle boundsAfter = GetBoundingRectangle(true);
			EndResize(boundsAfter.Width - boundsBefore.Width, boundsAfter.Height - boundsBefore.Height);
		}

		#endregion


		#region Hit-Testing

		/// <summary>
		/// Determines whether the shape was hit by a mouse click at the given coordinates.
		/// </summary>
		public override sealed bool ContainsPoint(int x, int y) {
			return ContainsPointCore(x, y) || ChildrenContainPoint(x, y);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected abstract bool ContainsPointCore(int x, int y);


		/// <ToBeCompleted></ToBeCompleted>
		protected bool ChildrenContainPoint(int x, int y) {
			if (IsChildrenCollectionEmpty) return false;
			else return ChildrenCollection.ContainsPoint(x, y);
		}


		/// <override></override>
		public override ControlPointId HitTest(int x, int y, ControlPointCapabilities controlPointCapability, int range) {
			Rectangle r = Rectangle.Empty;
			r.Offset(x - range, y - range);
			r.Width = r.Height = range + range;
			foreach (ControlPointId pointId in GetControlPointIds(controlPointCapability))
				if (Geometry.RectangleContainsPoint(r, GetControlPointPosition(pointId)))
					return pointId;
			if ((range > 0 && IntersectsWith(r.X, r.Y, r.Width, r.Height))
					|| ContainsPoint(x, y)) {
				if (HasControlPointCapability(ControlPointId.Reference, controlPointCapability)) {
					if (controlPointCapability == ControlPointCapabilities.All) return ControlPointId.Reference;
					else if ((controlPointCapability & ControlPointCapabilities.Connect) == ControlPointCapabilities.Connect
						|| (controlPointCapability & ControlPointCapabilities.Glue) == ControlPointCapabilities.Glue) {
						if (IsConnectionPointEnabled(ControlPointId.Reference)) return ControlPointId.Reference;
					}
				}
			}
			return ControlPointId.None;
		}


		/// <override></override>
		public override IEnumerable<Point> IntersectOutlineWithLineSegment(int x1, int y1, int x2, int y2) {
			// Default implementation, override for better results
			Rectangle bounds = GetBoundingRectangle(true);
			if (Geometry.IsValid(bounds)) {
				// Calculate intersection points with an ellipse that fits into the boundingRectangle
				Point center = Point.Empty;
				center.X = bounds.X + (bounds.Width / 2);
				center.Y = bounds.Y + (bounds.Height / 2);
				foreach (Point p in Geometry.IntersectEllipseLine(center.X, center.Y, bounds.Width, bounds.Height, x1, y2, x2, y2, true))
					yield return p;
			} else yield break;
		}


		/// <summary>
		/// Determines whether the shape intersects with the given rectangle area.
		/// </summary>
		public override sealed bool IntersectsWith(int x, int y, int width, int height) {
			return IntersectsWithCore(x, y, width, height) || IntersectsWithChildren(x, y, width, height);
		}


		/// <summary>
		/// Determines whether the shape intersects with the given rectangle area.
		/// </summary>
		protected abstract bool IntersectsWithCore(int x, int y, int width, int height);


		/// <ToBeCompleted></ToBeCompleted>
		protected bool IntersectsWithChildren(int x, int y, int width, int height) {
			if (IsChildrenCollectionEmpty) return false;
			else return ChildrenCollection.IntersectsWith(x, y, width, height);
		}


		/// <override></override>
		public override sealed Rectangle GetBoundingRectangle(bool tight) {
			if (IsChildrenCollectionEmpty) {
				if (Geometry.IsValid(tight ? _boundingRectangleTight : _boundingRectangleLoose))
					return tight ? _boundingRectangleTight : _boundingRectangleLoose;
				else {
					// Re-calculate bounding rectagle 
					Rectangle boundingRect = CalculateBoundingRectangle(tight);
					Debug.Assert(Geometry.IsValid(boundingRect));
					if (!SuspendingInvalidation) {
						// Store calculated rectangle until shape size or outline changes
						if (tight) _boundingRectangleTight = boundingRect;
						else _boundingRectangleLoose = boundingRect;
					}
					return boundingRect;
				} 					
			} else {
				Rectangle result = CalculateBoundingRectangle(tight);
				if (Geometry.IsValid(result))
					result = Geometry.UniteRectangles(result, ChildrenCollection.GetBoundingRectangle(tight));
				return result;
			}
		}


		/// <overriden></overriden>
		/// <remarks>This is a very approximative implementation and should be overriden for 
		/// better performance. Do not call the base class when overriding.</remarks>
		protected internal override IEnumerable<Point> CalculateCells(int cellSize) {
			// The outer bounding rectangle (including the control points) is required here
			Rectangle r = GetBoundingRectangle(false);
			// This not 100% correct as cell 0 will be occupied by objects at 10/10 
			// as well as objects at -10/-10, 10/-10 and -10/10. 
			// On the other hand, integer division is >20 times faster than floored float divisions
			// and for this simple "bounding rectangle" approach, it works ok.
			int leftIdx = r.Left / cellSize;
			if (r.Left < 0) --leftIdx;
			int topIdx = r.Top / cellSize;
			if (r.Top < 0) --topIdx;
			int rightIdx = r.Right / cellSize;
			if (r.Right < 0) --rightIdx;
			int bottomIdx = r.Bottom / cellSize;
			if (r.Bottom < 0) --bottomIdx;
			Point p = Point.Empty;
			for (p.X = leftIdx; p.X <= rightIdx; p.X += 1)
				for (p.Y = topIdx; p.Y <= bottomIdx; p.Y += 1)
					yield return p;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected ShapeCollection Owner {
			get { return _owner; }
			set { _owner = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected abstract Rectangle CalculateBoundingRectangle(bool tight);

		#endregion


		#region Moving, sizing, rotating, expanding

		/// <summary>
		/// Moves the given ControlPoint by the given offsets. 
		/// Invalidates the shape and calls FollowConnectionPointWithGluePoint for all ControlPoints affected by the movement.
		/// </summary>
		/// <returns>True if the movement was performed as desired. False if the movement was performed as good as possible.</returns>
		public override sealed bool MoveControlPointBy(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers) {
			bool result;
			if (pointId == ControlPointId.None)
				throw new ArgumentException(string.Format(Properties.Resources.MessageFmt_0IsNotAValidControlPointForThisOperation, pointId));
			if (pointId == ControlPointId.Reference)
				result = MoveBy(deltaX, deltaY);
			else {
				if (HasControlPointCapability(pointId, ControlPointCapabilities.Glue)
					&& IsConnected(pointId, null) != ControlPointId.None)
					result = false;
				else {
					Rectangle boundsBefore = GetBoundingRectangle(true);
					BeginResize();

					result = MovePointByCore(pointId, deltaX, deltaY, modifiers);
					_boundingRectangleTight = _boundingRectangleLoose = Geometry.InvalidRectangle;

					Rectangle boundsAfter = GetBoundingRectangle(true);
					if (!EndResize(boundsAfter.Width - boundsBefore.Width, boundsAfter.Height - boundsBefore.Height))
						result = false;
				}
			}
			return result;
		}


		/// <summary>
		/// Moves the shape by the given offsets. 
		/// Invalidates the shape and calls FollowConnectionPointWithGluePoint for all ControlPoints affected by the movement.
		/// </summary>
		/// <returns>True if the movement was performed as desired. False if the movement was performed as good as possible.</returns>
		public override sealed bool MoveBy(int deltaX, int deltaY) {
			bool result;
			BeginMove();

			int oldPosX = X; int oldPosY = Y;
			result = MoveByCore(deltaX, deltaY);
			int dX = X - oldPosX; int dY = Y - oldPosY;

			if (!EndMove(dX, dY)) result = false;
			return result;
		}


		/// <summary>
		/// Rotates the shape around the given coordinates.
		/// </summary>
		/// <param name="deltaAngle">The rotation angle in tenths of degrees</param>
		/// <param name="x">X coordinate of the roation center</param>
		/// <param name="y">Y coordinate of the roation center</param>
		/// <returns></returns>
		public override sealed bool Rotate(int deltaAngle, int x, int y) {
			bool result;
			BeginRotate();

			result = RotateCore(deltaAngle, x, y);

			if (!EndRotate(deltaAngle)) result = false;
			return result;
		}


		/// <override></override>
		public override IEnumerable<ControlPointId> GetControlPointIds(ControlPointCapabilities controlPointCapability) {
			return Enumerator.Create(this, controlPointCapability);
		}


		/// <override></override>
		public override ControlPointId FindNearestControlPoint(int x, int y, int distance, ControlPointCapabilities controlPointCapability) {
			ControlPointId result = ControlPointId.None;
			Point p = Point.Empty;
			float currDistance = float.MaxValue;
			foreach (ControlPointId id in GetControlPointIds(controlPointCapability)) {
				p = GetControlPointPosition(id);
				float d = Geometry.DistancePointPoint(x, y, p.X, p.Y);
				if (d <= distance && d < currDistance) {
					currDistance = d;
					result = id;
				}
			}
			if (result == ControlPointId.None
				&& (controlPointCapability & ControlPointCapabilities.Connect) != 0
				&& ContainsPoint(x, y))
				result = ControlPointId.Reference;
			return result;
		}

		#endregion


		#region Drawing

		/// <override></override>
		public override IDisplayService DisplayService {
			get { return _displayService; }
			set {
				if (_displayService != value) {
					_displayService = value;
					if (ChildrenCollection != null && ChildrenCollection.Count > 0)
						ChildrenCollection.SetDisplayService(_displayService);
				}
			}
		}


		/// <override></override>
		public override ILineStyle LineStyle {
			get { return _privateLineStyle ?? Template.Shape.LineStyle; }
			set {
				BeginResize();
				// Set private LineStyle only if it differs from the template's line style (if a template exists)
				_privateLineStyle = (Template != null && value == Template.Shape.LineStyle) ? null : value;
				EndResize(0, 0);
			}
		}


		/// <summary>
		/// Paint the shape
		/// </summary>
		public override void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (!IsChildrenCollectionEmpty) ChildrenCollection.Draw(graphics);
		}


		/// <override></override>
		public override void DrawOutline(Graphics graphics, Pen pen) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (pen == null) throw new ArgumentNullException("pen");
			UpdateDrawCache();
			if (!IsChildrenCollectionEmpty) ChildrenCollection.DrawOutline(graphics, pen);
		}


		/// <summary>
		/// Creates an Icon (for e.g. the ToolBox) of the shape by painting it on the given image. 
		/// The shape will be modified so it is recommended to call this method on temporary clones of the shape only.
		/// TransparentColor specifies the background color of the calling control. The shape is rendered using antialiasing so if the transparentcolor does not match the background, the painted shape has ugly borders.
		/// </summary>
		/// <param name="image">The image the shape is drawn to.</param>
		/// <param name="margin">The margin around the shape.</param>
		/// <param name="transparentColor">The color which should become the TransparentColor of the Image.</param>
		public override void DrawThumbnail(Image image, int margin, Color transparentColor) {
			if (image == null) throw new ArgumentNullException("image");
			using (Graphics g = Graphics.FromImage(image)) {
				GdiHelpers.ApplyGraphicsSettings(g, RenderingQuality.MaximumQuality);
				g.Clear(transparentColor);

				Rectangle srcRectangle = GetBoundingRectangle(true);
				Rectangle destRectangle = Rectangle.Empty;
				destRectangle.X = destRectangle.Y = margin;
				destRectangle.Width = image.Width - (2 * margin);
				destRectangle.Height = image.Height - (2 * margin);

				float scale = Geometry.CalcScaleFactor(srcRectangle.Width, srcRectangle.Height, destRectangle.Width, destRectangle.Height);
				g.ScaleTransform(scale, scale, MatrixOrder.Append);

				int dx = (int)Math.Round(((image.Width / scale) - srcRectangle.Width) / 2f);
				int dy = (int)Math.Round(((image.Height / scale) - srcRectangle.Height) / 2f);

				MoveBy(-srcRectangle.X + dx, -srcRectangle.Y + dy);
				Draw(g);
			}
		}


		/// <summary>
		/// Invalidates the area of the shape in the display
		/// </summary>
		public override void Invalidate() {
			if (!SuspendingInvalidation) {
				if (!IsChildrenCollectionEmpty) ChildrenCollection.Invalidate();
			}
		}

		#endregion

		#endregion


		#region IDisposable Members

		// ToDo: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~ShapeBase() {
		//   // Put cleanup code in Dispose(bool disposing) below.
		//   Dispose(false);
		// }


		// Reference implementation of the Dispose() pattern
		/// <override></override>
		public override sealed void Dispose() {
			if (ObjectState == ObjectState.Normal) {
				_objectState = ObjectState.Disposing;

				// Do not change this code. Put cleanup code in Dispose(bool disposing) below.
				Dispose(true);	
				// ToDo: Suppress finalization if the finalizer is overridden above.
				// GC.SuppressFinalize(this);

				_objectState = ObjectState.Disposed;
			}
		}


		/// <summary>
		/// Disposes the shape.
		/// Detaches the shape from model objects and release shape connections.
		/// </summary>
		/// <remarks>
		/// Make sure this method is called before the arrays for storing connection points etc are disposed.
		/// </remarks>
		protected virtual void Dispose(bool disposing) {
			if (_objectState == ObjectState.Normal) {
				if (disposing) {
					// Dispose managed state (managed objects).
					if (ModelObject != null) {
						// If shape was detached by the repository.DeleteShape method, this will fail. 
						// As the shape will be finalized here, we can ignore the error.
						if (ModelObject != null && ModelObject.ShapeCount > 0) {
							foreach (Shape s in ModelObject.Shapes) {
								if (s == this) {
									ModelObject.DetachShape(this);
									break;
								}
							}
						}
					}
					if (connectionInfos != null) {
						for (int i = connectionInfos.Count - 1; i >= 0; --i) {
							ShapeConnectionInfo connectionInfo = connectionInfos[i];
							if (HasControlPointCapability(connectionInfo.OwnPointId, ControlPointCapabilities.Glue))
								Disconnect(connectionInfo.OwnPointId);
							else connectionInfo.OtherShape.Disconnect(connectionInfo.OtherPointId);
						}
					}
					GdiHelpers.DisposeObject(ref _matrix);
				}
				// ToDo: Free unmanaged resources (unmanaged objects) and override a finalizer below.
				// ToDo: Set large fields to null.
			}
		}


		/// <summary>
		/// Returns thrue of this instance was already disposed.
		/// </summary>
		protected ObjectState ObjectState {
			get { return _objectState; }
		}

		#endregion


		#region IEntity Members

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.ShapeBase" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Template", typeof(object));
			yield return new EntityFieldDefinition("ModelObject", typeof(object));
			yield return new EntityFieldDefinition("X", typeof(int));
			yield return new EntityFieldDefinition("Y", typeof(int));
			yield return new EntityFieldDefinition("ZOrder", typeof(int));
			if (version >= 6) yield return new EntityFieldDefinition("HomeLayer", typeof(int));
			yield return new EntityFieldDefinition("Layers", typeof(int));
			yield return new EntityFieldDefinition("SecurityDomainName", typeof(char));
			yield return new EntityFieldDefinition("LineStyle", typeof(object));
			if (version >= 5) yield return new EntityFieldDefinition("Data", typeof(string));
		}


		/// <override></override>
		[Browsable(false)]
		protected override sealed object IdCore {
			get { return _id; }
		}


		/// <override></override>
		protected override sealed void AssignIdCore(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this._id != null) throw new InvalidOperationException("Shape has already an id.");
			this._id = id;
		}


		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			InvalidateDrawCache();
			_template = reader.ReadTemplate();
			_modelObject = reader.ReadModelObject();
			if (_modelObject != null) _modelObject.AttachShape(this);

			int x = reader.ReadInt32();
			int y = reader.ReadInt32();
			MoveByCore(x, y);
			// These properties do not trigger invalidation
			ZOrder = reader.ReadInt32();
			if (version >= 6)
				HomeLayer = reader.ReadInt32();
			SupplementalLayers = (LayerIds)reader.ReadInt32();
			SecurityDomainName = reader.ReadChar();
			_privateLineStyle = reader.ReadLineStyle();
			if (version >= 5) _data = reader.ReadString();
		}


		/// <override></override>
		protected override void LoadInnerObjectsCore(string propertyName, IRepositoryReader reader, int version) {
			// nothing to do
		}


		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			writer.WriteTemplate(_template);
			writer.WriteModelObject(_modelObject);
			writer.WriteInt32(X);
			writer.WriteInt32(Y);
			writer.WriteInt32(ZOrder);
			if (version >= 6)
				writer.WriteInt32(HomeLayer);
			writer.WriteInt32((int)SupplementalLayers);
			writer.WriteChar(SecurityDomainName);
			writer.WriteStyle(_privateLineStyle);
			if (version >= 5) writer.WriteString(_data);
		}


		/// <override></override>
		protected override void SaveInnerObjectsCore(string propertyName, IRepositoryWriter writer, int version) {
			// nothing to do
		}


		/// <override></override>
		protected override void DeleteCore(IRepositoryWriter writer, int version) {
			// nothing to do...

			/*foreach (EntityPropertyDefinition pi in Type.GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}*/
		}

		#endregion


		/// <override></override>
		protected internal override void InitializeToDefault(IStyleSet styleSet) {
			_privateLineStyle = styleSet.LineStyles.Normal;
			_securityDomainName = 'A';
		}


		/// <summary>
		/// Intended for use by the framework. Do not call this method directly.
		/// </summary>
		/// <remarks>Called upon the passive shape of the connection. This method is 
		/// always called by the active shape in its Connect method. If ownPointId 
		/// is ControlPointId.Reference, the global connection is meant. In this case 
		/// the callee calls MoveControlPointTo upon the shape with the sticky point 
		/// to position the glue point.</remarks>
		protected internal override sealed void AttachGluePointToConnectionPoint(ControlPointId ownPointId, Shape otherShape, ControlPointId gluePointId) {
			if (ownPointId != ControlPointId.Reference
				&& !HasControlPointCapability(ownPointId, ControlPointCapabilities.Connect))
				throw new NShapeException(string.Format(Properties.Resources.MessageFm_AttachGluePointToConnectionPoint_0SPoint1HasToBeAConnectionPoint, Type.Name, ownPointId));
			if (!otherShape.HasControlPointCapability(gluePointId, ControlPointCapabilities.Glue))
				throw new NShapeException(string.Format(Properties.Resources.MessageFmt_AttachGluePointToConnectionPoint_0SPoint1HasToBeAGluePoint, otherShape.Type.Name, gluePointId));
			// store the ShapeConnectionInfo
			ShapeConnectionInfo connectionInfo = ShapeConnectionInfo.Create(ownPointId, otherShape, gluePointId);
			if (connectionInfos == null) connectionInfos = new List<ShapeConnectionInfo>();
			if (!connectionInfos.Contains(connectionInfo)) {
				if (this.ModelObject != null && otherShape.ModelObject != null && otherShape.Template != null)
					ModelObject.Connect(Template.GetMappedTerminalId(ownPointId), otherShape.ModelObject, otherShape.Template.GetMappedTerminalId(gluePointId));
				connectionInfos.Add(connectionInfo);
			}
		}


		/// <summary>
		/// Called upon the passive shape of the connection by the active shape. 
		/// If ownPointId is 0, the global connection is meant.
		/// </summary>
		protected internal override sealed void DetachGluePointFromConnectionPoint(ControlPointId ownPointId, Shape otherShape, ControlPointId gluePointId) {
			Debug.Assert(connectionInfos != null);
			// fill ShapeConnectionInfo
			ShapeConnectionInfo connectionInfo = ShapeConnectionInfo.Empty;
			connectionInfo.OwnPointId = ownPointId;
			connectionInfo.OtherShape = otherShape;
			connectionInfo.OtherPointId = gluePointId;

			// find ShapeConnectionInfo and remove it
			if (connectionInfos.Contains(connectionInfo)) {
				connectionInfos.Remove(connectionInfo);
				if (this.ModelObject != null && otherShape.ModelObject != null && otherShape.Template != null)
					ModelObject.Disconnect(Template.GetMappedTerminalId(ownPointId), otherShape.ModelObject, otherShape.Template.GetMappedTerminalId(gluePointId));
				// delete list if there are no more connections
				if (connectionInfos.Count == 0) connectionInfos = null;
			} else
				throw new NShapeException("The connection does not exist.");
		}


		/// <override></override>
		protected internal override object InternalTag {
			get { return _internalTag; }
			set { _internalTag = value; }
		}


		/// <summary>
		/// Returns true if calls to Invalidate() will be suppressed.
		/// Additionally, bounding rectangles are not buffered when calculated if invalidation is suspended.
		/// </summary>
		protected bool SuspendingInvalidation {
			get { return _suspendInvalidate; }
			private set { _suspendInvalidate = value; }
		}


		/// <summary>
		/// Protetced internal constructur. Should only be called by the <see cref="T:Dataweb.NShape.Advanced.ShapeType" />'s <see cref="T:Dataweb.NShape.Advanced.CreateShapeDelegate" />
		/// </summary>
		protected ShapeBase(ShapeType shapeType, Template template) {
			if (shapeType == null) throw new ArgumentNullException("shapeType");
			InvalidateDrawCache();
			this._shapeType = shapeType;
			this._template = template;
		}


		/// <summary>
		/// Protetced internal constructor. Should only be called by the <see cref="T:Dataweb.NShape.Advanced.ShapeType" />'s <see cref="T:Dataweb.NShape.Advanced.CreateShapeDelegate" />
		/// </summary>
		protected ShapeBase(ShapeType shapeType, IStyleSet styleSet)
			: this(shapeType, (Template)null) {
			if (styleSet == null) throw new ArgumentNullException("styleSet");
			InitializeToDefault(styleSet);
		}


		///// <summary>
		///// Moves the shape
		///// </summary>
		//protected abstract bool MoveToCore(int toX, int toY);


		/// <summary>
		/// Moves the shape
		/// </summary>
		protected abstract bool MoveByCore(int deltaX, int deltaY);


		///// <summary>
		///// Moves the shape
		///// </summary>
		//protected virtual bool MoveByCore(int deltaX, int deltaY) {
		//    if (Geometry.IsValid(boundingRectangleTight))
		//        boundingRectangleTight.Offset(deltaX, deltaY);
		//    if (Geometry.IsValid(boundingRectangleLoose))
		//        boundingRectangleLoose.Offset(deltaX, deltaY);
		//    return true;
		//}


		/// <summary>
		/// Transformes the desired movement according to the shape's angle and then performs the movement by calling MoveControlPointBy()
		/// </summary>
		protected abstract bool MovePointByCore(ControlPointId pointId, int deltaX, int deltaY, ResizeModifiers modifiers);


		/// <summary>
		/// Rotates the shape around the given coordinates.
		/// </summary>
		/// <param name="deltaAngle">The rotation angle in tenths of degrees</param>
		/// <param name="x">X coordinate of the roation center</param>
		/// <param name="y">Y coordinate of the roation center</param>
		/// <returns></returns>
		protected abstract bool RotateCore(int deltaAngle, int x, int y);


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void ProcessExecModelPropertyChange(IModelMapping propertyMapping) {
			if (propertyMapping.ShapePropertyId == PropertyIdLineStyle) {
				// assign private stylebecause if the style matches the template's style, it would not be assigned.
				_privateLineStyle = (propertyMapping.GetStyle() as ILineStyle);
				Invalidate();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected bool IsStyleAffected(IStyle shapeStyle, IStyle changedStyle) {
			// if styles are equal, style is affected
			if (changedStyle == shapeStyle)
				return true;
			// if shapeStyle contains changed style, style is affected
			if (changedStyle is IColorStyle) {
				if (shapeStyle is ICapStyle)
					if (((ICapStyle)shapeStyle).ColorStyle == changedStyle)
						return true;
				if (shapeStyle is IFillStyle)
					if (((IFillStyle)shapeStyle).AdditionalColorStyle == changedStyle || ((IFillStyle)shapeStyle).BaseColorStyle == changedStyle)
						return true;
				if (shapeStyle is ICharacterStyle)
					if (((ICharacterStyle)shapeStyle).ColorStyle == changedStyle)
						return true;
				if (shapeStyle is ILineStyle)
					if (((ILineStyle)shapeStyle).ColorStyle == changedStyle)
						return true;
			}
			return false;
		}


		/// <summary>
		/// Calculates the endpoint of a line that starts at startX, startY and is connected to this shape globally.
		/// </summary>
		protected virtual Point CalcGluePoint(ControlPointId gluePointId, Shape shape) {
			if (HasControlPointCapability(gluePointId, ControlPointCapabilities.Glue))
				throw new NShapeException("This Method has to be implemented. Base Method may not be called.");
			else
				throw new NShapeException(Properties.Resources.MessageFmt_CalcGluePoint_0HasNoGluePoints, this.Type.Name);
		}


		/// <summary>
		/// Calculates the new position of a glue point connected to a shape
		/// </summary>
		/// <param name="gluePointId">Id of the connected GluePoint</param>
		/// <param name="shape">The shape the GluePoint is connected to</param>
		/// <param name="fromX">The X position of the other end of the line.</param>
		/// <param name="fromY">The Y position of the other end of the line.</param>
		/// <returns></returns>
		protected Point CalcGluePointFromPosition(ControlPointId gluePointId, Shape shape, int fromX, int fromY) {
			// calculate the intersection of the line with the partner shape's outline
			Point result = shape.CalculateConnectionFoot(fromX, fromY);

			// if the partner shape has children, calculate the nearest intersection point
			if (shape.Children.Count > 0) {
				// ToDo 3: Create a better implementation by using IntersectOutlineWithLineSegment()
				Point p = Point.Empty;
				float distance;
				float lowestDistance = Geometry.DistancePointPoint(result.X, result.Y, fromX, fromY);
				// Use the nearest intersection point not contained by an other shape of the group
				foreach (Shape childShape in shape.Children) {
					p = childShape.CalculateConnectionFoot(fromX, fromY);
					if (p == Point.Empty) {
						ControlPointId nearestCtrlPtId = childShape.FindNearestControlPoint(fromX, fromY, int.MaxValue, ControlPointCapabilities.All);
						if (nearestCtrlPtId != ControlPointId.None)
							p = GetControlPointPosition(nearestCtrlPtId);
						else {
							p.X = int.MaxValue;
							p.Y = int.MaxValue;
						}
					}

					// calculate distance to point and set result if a new nearest point is found
					distance = Geometry.DistancePointPoint(p.X, p.Y, fromX, fromY);
					if (distance < lowestDistance) {
						// If the new nearest point is contained by an other shape:
						// Skip it, as we do not want lines that cross, intersect or end within shapes of the group
						Shape s = shape.Children.FindShape(p.X, p.Y, ControlPointCapabilities.None, 0, null);
						if (s == null || s == childShape) {
							lowestDistance = distance;
							result = p;
						}
					}
				}
			}
			Debug.Assert(Geometry.IsValid(result));
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual ControlPointId GetControlPointId(int index) {
			// This is the default
			return index + 1;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual int GetControlPointIndex(ControlPointId id) {
			return id - 1;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void ControlPointsHaveMoved() {
			if (connectionInfos != null) {
				foreach (ShapeConnectionInfo connectionInfo in connectionInfos) {
					if (connectionInfo.OtherShape.HasControlPointCapability(connectionInfo.OtherPointId, ControlPointCapabilities.Glue)) {
						// Check if the positions of the points are equal. Recalculate gluepoint position only if the 
						// partner point has really moved (for performance reasons and in order to prevent endless recursive 
						// calls when two lines are connected to each other).
						// Exceptions from this rule: 
						// If the own connection point is the a roation point, or a connection point at the position of FirstVertex/LastVertex, 
						// we always call FollowConnectionPointWithGluePoint. Otherwise, Labels connected to the center point of a shape will 
						// not rotate with their partner shape) as this point will not move when rotating
						Point ownPointPos = GetControlPointPosition(connectionInfo.OwnPointId);
						Boolean followConnectionPoint = (
							(connectionInfo.OtherShape.GetControlPointPosition(connectionInfo.OtherPointId) != ownPointPos)
							|| HasControlPointCapability(connectionInfo.OwnPointId, ControlPointCapabilities.Rotate)
						);
						if (followConnectionPoint == false && this is ILinearShape) {
							if (GetControlPointPosition(ControlPointId.FirstVertex) == ownPointPos || GetControlPointPosition(ControlPointId.LastVertex) == ownPointPos)
								followConnectionPoint = true;
						}
						if (followConnectionPoint)
							connectionInfo.OtherShape.FollowConnectionPointWithGluePoint(connectionInfo.OtherPointId, this, connectionInfo.OwnPointId);
					}
				}
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual bool IsConnectionPointEnabled(ControlPointId pointId) {
			if (_template == null) return true;
			else { return Template.GetMappedTerminalId(pointId) != TerminalId.Invalid; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void BeginMove() {
			Invalidate();
			SuspendingInvalidation = true;
			if (Owner != null && !SuspendingOwnerNotification)
				Owner.NotifyChildMoving(this);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual bool EndMove(int deltaX, int deltaY) {
			bool result = true;

			// Translate bounding rectangles (if calculated)
			if (Geometry.IsValid(_boundingRectangleTight))
			    _boundingRectangleTight.Offset(deltaX, deltaY);
			if (Geometry.IsValid(_boundingRectangleLoose))
				_boundingRectangleLoose.Offset(deltaX, deltaY);
			
			SuspendingInvalidation = false;

			// Notify children
			if (!IsChildrenCollectionEmpty)
				result = ChildrenCollection.NotifyParentMoved(deltaX, deltaY);

			// Notify owner
			if (Owner != null && !SuspendingOwnerNotification)
				Owner.NotifyChildMoved(this);

			// Notify connected shapes
			ControlPointsHaveMoved();

			Invalidate();
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void BeginResize() {
			Invalidate();
			SuspendingInvalidation = true;
			if (Owner != null && !SuspendingOwnerNotification)
				Owner.NotifyChildResizing(this);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual bool EndResize(int deltaX, int deltaY) {
			bool result = true;
			InvalidateDrawCache();
			SuspendingInvalidation = false;
			// Notify children
			if (!IsChildrenCollectionEmpty)
				result = ChildrenCollection.NotifyParentSized(deltaX, deltaY);
			// Notify owner
			if (Owner != null && !SuspendingOwnerNotification)
				Owner.NotifyChildResized(this);
			ControlPointsHaveMoved();
			Invalidate();
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void BeginRotate() {
			Invalidate();
			SuspendingInvalidation = true;
			if (Owner != null && !SuspendingOwnerNotification)
				Owner.NotifyChildRotating(this);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual bool EndRotate(int deltaAngle) {
			bool result = true;
			SuspendingInvalidation = false;
			InvalidateDrawCache();
			// Notify children
			if (!IsChildrenCollectionEmpty)
				result = ChildrenCollection.NotifyParentRotated(deltaAngle, X, Y);
			// Notify owner
			if (Owner != null && !SuspendingOwnerNotification)
				Owner.NotifyChildRotated(this);
			ControlPointsHaveMoved();
			Invalidate();
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected Matrix Matrix {
			get {
				if (_matrix == null) {
					if (ObjectState != ObjectState.Normal) throw new ObjectDisposedException(typeof(ShapeBase).Name);
					_matrix = new Matrix();
				}
				return _matrix;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void InvalidateDrawCache() {
			drawCacheIsInvalid = true;
			// Reset boundingRectangles
			_boundingRectangleTight = Geometry.InvalidRectangle;
			_boundingRectangleLoose = Geometry.InvalidRectangle;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected abstract void UpdateDrawCache();


		/// <summary>
		/// Recalculate all objects that define the shape's outline and appearance, such 
		/// as ControlPoints and GraphicsPath. The objects have to be calculated at the 
		/// current position and with the current size but unrotated.
		/// </summary>
		protected abstract void RecalcDrawCache();


		/// <summary>
		/// Transforms all objects that need to be transformed, such as Point-Arrays, 
		/// GraphicsPaths or Brushes.
		/// </summary>
		/// <param name="deltaX">Translation on X axis</param>
		/// <param name="deltaY">Translation on Y axis</param>
		/// <param name="deltaAngle">Rotation angle in tenths of degrees</param>
		/// <param name="rotationCenterX">X coordinate of the rotation center</param>
		/// <param name="rotationCenterY">Y coordinate of the rotation center</param>
		protected abstract void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY);


		private bool DoMoveConnectedGluePoint(ControlPointId gluePointId, Shape connectedShape, ControlPointId movedPointId) {
			Point currGluePtPos = GetControlPointPosition(gluePointId);
			Point newGluePtPos = Geometry.InvalidPoint;

			// If the connection is a point-to-shape connection, the shape calculates the new glue point position 
			// with the help of the connected shape. 
			if (movedPointId == ControlPointId.Reference)
				newGluePtPos = CalcGluePoint(gluePointId, connectedShape);
			else newGluePtPos = connectedShape.GetControlPointPosition(movedPointId);
			//Debug.Print("New GLuePoint position: {0}", newGluePtPos);

			// If calculating a new position failed, do not move the glue point.
			if (Geometry.IsValid(newGluePtPos)) {
				int deltaX = newGluePtPos.X - currGluePtPos.X;
				int deltaY = newGluePtPos.Y - currGluePtPos.Y;
				// We must call MovePointByCore because in case the shape rotates around the glue point position, 
				// the partner shape might have to recalc its positiona and angle
				//if (deltaX == 0 && deltaY == 0)
				//    return true;
				if (!MovePointByCore(gluePointId, deltaX, deltaY, ResizeModifiers.MaintainAspect))
					return false;
				return true;
			} else return false;
		}


		private bool SuspendingOwnerNotification {
			get { return _notifySuspendCounter > 0; }
		}


		private void SuspendOwnerNotification() {
			Debug.Assert(_notifySuspendCounter >= 0);
			++_notifySuspendCounter;
		}


		private void ResumeOwnerNotification() {
			Debug.Assert(_notifySuspendCounter > 0);
			--_notifySuspendCounter;
		}


		private Shape CreateInstanceForTemplate() {
			Shape shape = this.Type.CreateInstance();
			shape.CopyFrom(this);	// Template will be copied
			return shape;
		}


		private Boolean IsChildrenCollectionEmpty {
			get { return ChildrenCollection == null; }
		}


		private Boolean CanConnect(ControlPointId ownPointId, Shape otherShape, ControlPointId otherPointId, out String message) {
			if (otherShape == null) throw new ArgumentNullException("otherShape");
			message = null;
			if (otherShape.Diagram != null && this.Diagram != null && otherShape.Diagram != this.Diagram)
				message = Properties.Resources.MessageTxt_ConnectingToShapesOfOtherDiagramsIsNotSupported;
			else if (!HasControlPointCapability(ownPointId, ControlPointCapabilities.Glue)) {
				if (!otherShape.HasControlPointCapability(otherPointId, ControlPointCapabilities.Glue))
					message = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Neither0SPoint1Nor2SPoint3IsAGluePoint, Type.Name, ownPointId, otherShape.Type.Name, otherPointId);
			} else {
				// Check if connecting is possible:
				//
				// 1. The glue point must not be connected yet
				ShapeConnectionInfo ci = GetConnectionInfo(ownPointId, null);
				if (!ci.IsEmpty)
					message = String.Format(Properties.Resources.MessageFmt_0SGluePoint1IsAlreadyConnectedToA2, Type.Name, ci.OwnPointId, ci.OtherShape.Type.Name);
				// 2. The target shape's control point must not be a glue point
				else if (otherShape.HasControlPointCapability(otherPointId, ControlPointCapabilities.Glue))
					message = String.Format(Properties.Resources.MessageFmt_0SPoint1And2SPoint3AreBothGluePoints, Type.Name, ownPointId, otherShape.Type.Name, otherPointId);
				// 3. The target shape's control point has to be a connection point
				else if (otherPointId != ControlPointId.Reference
					&& !otherShape.HasControlPointCapability(otherPointId, ControlPointCapabilities.Connect))
					message = String.Format(Properties.Resources.MessageFm_AttachGluePointToConnectionPoint_0SPoint1HasToBeAConnectionPoint, otherShape.Type.Name, otherPointId);
				//else if (!IsConnectionPointEnabled(ownPointId))
				//    message = String.Format("{0}'s connection point {1} is disabled.", otherShape.Type.Name, ownPointId);
			}
			return (message == null);
		}


		#region Explicit IShapeCollection implementation

		/// <override></override>
		int ICollection.Count {
			get {
				if (IsChildrenCollectionEmpty) return 0;
				else return ChildrenCollection.Count;
			}
		}


		/// <override></override>
		void ICollection.CopyTo(Array array, int index) {
			if (!IsChildrenCollectionEmpty) ChildrenCollection.CopyTo(array, index);
		}


		/// <override></override>
		object ICollection.SyncRoot {
			get {
				if (IsChildrenCollectionEmpty) return null;
				else return ChildrenCollection.SyncRoot;
			}
		}


		/// <override></override>
		bool ICollection.IsSynchronized {
			get {
				if (IsChildrenCollectionEmpty) return false;
				else return ChildrenCollection.IsSynchronized;
			}
		}


		/// <override></override>
		int IReadOnlyShapeCollection.MaxZOrder {
			get {
				if (IsChildrenCollectionEmpty) return 0;
				else return ChildrenCollection.MaxZOrder;
			}
		}


		/// <override></override>
		int IReadOnlyShapeCollection.MinZOrder {
			get {
				if (IsChildrenCollectionEmpty) return 0;
				return ChildrenCollection.MinZOrder;
			}
		}


		/// <override></override>
		Shape IReadOnlyShapeCollection.TopMost {
			get {
				if (IsChildrenCollectionEmpty) return null;
				else return ChildrenCollection.TopMost;
			}
		}


		/// <override></override>
		Shape IReadOnlyShapeCollection.Bottom {
			get {
				if (IsChildrenCollectionEmpty) return null;
				else return ChildrenCollection.Bottom;
			}
		}


		/// <override></override>
		IEnumerable<Shape> IReadOnlyShapeCollection.TopDown {
			get {
				if (IsChildrenCollectionEmpty) return EmptyEnumerator<Shape>.Empty;
				else return ChildrenCollection.TopDown;
			}
		}


		/// <override></override>
		IEnumerable<Shape> IReadOnlyShapeCollection.BottomUp {
			get {
				if (IsChildrenCollectionEmpty) return EmptyEnumerator<Shape>.Empty;
				else return ChildrenCollection.BottomUp;
			}
		}


		/// <override></override>
		Shape IReadOnlyShapeCollection.FindShape(int x, int y, int width, int height, bool completelyInside, Shape lastFound) {
			if (IsChildrenCollectionEmpty) return null;
			else return ChildrenCollection.FindShape(x, y, width, height, completelyInside, lastFound);
		}


		/// <override></override>
		Shape IReadOnlyShapeCollection.FindShape(int x, int y, ControlPointCapabilities controlPointCapabilities, int distance, Shape lastFound) {
			if (IsChildrenCollectionEmpty) return null;
			else return ChildrenCollection.FindShape(x, y, controlPointCapabilities, distance, lastFound);
		}


		/// <override></override>
		IEnumerable<Shape> IReadOnlyShapeCollection.FindShapes(int x, int y, int width, int height, bool completelyInside) {
			if (IsChildrenCollectionEmpty) return EmptyEnumerator<Shape>.Empty;
			else return ChildrenCollection.FindShapes(x, y, width, height, completelyInside);
		}


		/// <override></override>
		IEnumerable<Shape> IReadOnlyShapeCollection.FindShapes(int x, int y, ControlPointCapabilities controlPointCapabilities, int distance) {
			if (IsChildrenCollectionEmpty) return EmptyEnumerator<Shape>.Empty;
			else return ChildrenCollection.FindShapes(x, y, controlPointCapabilities, distance);
		}


		/// <override></override>
		void IShapeCollection.Add(Shape item) {
			if (IsChildrenCollectionEmpty) ChildrenCollection = CreateChildrenCollection(DefaultCapacity);
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResizing(this);
			ChildrenCollection.Add(item);
			_boundingRectangleLoose = _boundingRectangleTight = Geometry.InvalidRectangle;
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResized(this);
		}


		/// <override></override>
		void IShapeCollection.Add(Shape item, int zOrder) {
			item.ZOrder = zOrder;
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResizing(this);
			try {
				SuspendOwnerNotification();
				((IShapeCollection)this).Add(item);
			} finally { ResumeOwnerNotification(); }
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResized(this);
		}


		/// <override></override>
		void IShapeCollection.Clear() {
			if (IsChildrenCollectionEmpty)
				return;
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResizing(this);
			try {
				SuspendOwnerNotification();
				ChildrenCollection.Clear();
				_boundingRectangleLoose = _boundingRectangleTight = Geometry.InvalidRectangle;
				if (ChildrenCollection.Count == 0) ChildrenCollection = null;
			} finally { ResumeOwnerNotification(); }
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResized(this);
		}


		/// <override></override>
		bool IShapeCollection.Contains(Shape item) {
			if (IsChildrenCollectionEmpty) return false;
			else return ChildrenCollection.Contains(item);
		}


		/// <override></override>
		bool IShapeCollection.ContainsAll(IEnumerable<Shape> items) {
			if (IsChildrenCollectionEmpty) return false;
			else return ChildrenCollection.ContainsAll(items);
		}


		/// <override></override>
		bool IShapeCollection.ContainsAny(IEnumerable<Shape> items) {
			if (IsChildrenCollectionEmpty) return false;
			else return ChildrenCollection.ContainsAny(items);
		}


		/// <override></override>
		void IShapeCollection.CopyTo(Shape[] array, int arrayIndex) {
			if (!IsChildrenCollectionEmpty) ChildrenCollection.CopyTo(array, arrayIndex);
		}


		/// <override></override>
		bool IShapeCollection.Remove(Shape item) {
			if (item == null) throw new ArgumentNullException("shape");
			Boolean result = false;
			if (!IsChildrenCollectionEmpty) {
				if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResizing(this);
				result = ChildrenCollection.Remove(item);
				_boundingRectangleLoose = _boundingRectangleTight = Geometry.InvalidRectangle;
				if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResized(this);
			};
			return result;
		}


		/// <override></override>
		void IShapeCollection.AddRange(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResizing(this);
			try {
				SuspendOwnerNotification();
				if (IsChildrenCollectionEmpty) ChildrenCollection = CreateChildrenCollection(DefaultCapacity);
				ChildrenCollection.AddRange(shapes);
				_boundingRectangleLoose = _boundingRectangleTight = Geometry.InvalidRectangle;
			} finally { ResumeOwnerNotification(); }
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResized(this);
		}


		/// <override></override>
		void IShapeCollection.Replace(Shape oldShape, Shape newShape) {
			if (IsChildrenCollectionEmpty) throw new InvalidOperationException(Properties.Resources.MessageTxt_TheGivenShapeDoesNotExistInTheCollection);

			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResizing(this);
			ChildrenCollection.Replace(oldShape, newShape);
			_boundingRectangleLoose = _boundingRectangleTight = Geometry.InvalidRectangle;
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResized(this);
		}


		/// <override></override>
		void IShapeCollection.ReplaceRange(IEnumerable<Shape> oldShapes, IEnumerable<Shape> newShapes) {
			if (IsChildrenCollectionEmpty) throw new InvalidOperationException(Properties.Resources.MessageTxt_TheGivenShapeDoesNotExistInTheCollection);

			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResizing(this);
			try {
				SuspendOwnerNotification();
				ChildrenCollection.ReplaceRange(oldShapes, newShapes);
				_boundingRectangleLoose = _boundingRectangleTight = Geometry.InvalidRectangle;
			} finally { ResumeOwnerNotification(); }
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResized(this);
		}


		/// <override></override>
		bool IShapeCollection.RemoveRange(IEnumerable<Shape> shapes) {
			if (IsChildrenCollectionEmpty) return false;

			bool result = false;
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResizing(this);
			try {
				SuspendOwnerNotification();
				result = ChildrenCollection.RemoveRange(shapes);
				_boundingRectangleLoose = _boundingRectangleTight = Geometry.InvalidRectangle;
				if (ChildrenCollection.Count == 0) ChildrenCollection = null;
			} finally { ResumeOwnerNotification(); }
			if (Owner != null && !SuspendingOwnerNotification) Owner.NotifyChildResized(this);
			return result;
		}


		/// <override></override>
		int IShapeCollection.GetZOrder(Shape shape) {
			if (IsChildrenCollectionEmpty) throw new InvalidOperationException(Properties.Resources.MessageTxt_TheGivenShapeDoesNotExistInTheCollection);
			return ChildrenCollection.GetZOrder(shape);
		}


		/// <override></override>
		void IShapeCollection.SetZOrder(Shape shape, int zOrder) {
			if (IsChildrenCollectionEmpty) throw new InvalidOperationException(Properties.Resources.MessageTxt_TheGivenShapeDoesNotExistInTheCollection);
			ChildrenCollection.SetZOrder(shape, zOrder);
		}

		#endregion


		#region Explicit IEnumerable<Shape> implementation

		IEnumerator<Shape> IEnumerable<Shape>.GetEnumerator() {
			if (IsChildrenCollectionEmpty) return EmptyEnumerator<Shape>.Empty;
			else return ChildrenCollection.GetEnumerator();
		}

		#endregion


		#region Explicit IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator() {
			if (IsChildrenCollectionEmpty) return EmptyEnumerator<Shape>.Empty;
			else return ChildrenCollection.GetEnumerator();
		}

		#endregion


		#region ControlPointId Enumerator

		private struct Enumerator : IEnumerable<ControlPointId>, IEnumerator<ControlPointId>, IEnumerator {

			public static Enumerator Create(ShapeBase shape, ControlPointCapabilities flags) {
				Enumerator result;
				result.shape = shape;
				result.flags = flags;
				result.currentIndex = -1;
				result.ctrlPointCnt = shape.ControlPointCount;
				return result;
			}


			#region IEnumerable<ControlPointId> Members

			public IEnumerator<ControlPointId> GetEnumerator() {
				return this;
			}

			#endregion


			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator() {
				return this;
			}

			#endregion


			#region IEnumerator<ControlPointId> Members

			public bool MoveNext() {
				bool result = false;
				while (currentIndex + 1 < ctrlPointCnt) {
					++currentIndex;
					if (shape.HasControlPointCapability(shape.GetControlPointId(currentIndex), flags)) {
						result = true;
						break;
					}
				}
				return result;
			}


			public void Reset() {
				ctrlPointCnt = shape.ControlPointCount;
				currentIndex = -1;
			}


			ControlPointId IEnumerator<ControlPointId>.Current {
				get {
					if (currentIndex < 0 || currentIndex >= ctrlPointCnt)
						throw new InvalidOperationException();
					return shape.GetControlPointId(currentIndex);
				}
			}

			#endregion


			#region IEnumerator Members

			public object Current {
				get { return (IEnumerator<int>)this.Current; }
			}

			#endregion


			#region IDisposable Members

			public void Dispose() {
				this.flags = 0;
				this.currentIndex = -1;
				this.ctrlPointCnt = 0;
				this.shape = null;
			}

			#endregion


			#region Fields

			private ShapeBase shape;
			private ControlPointCapabilities flags;
			private int currentIndex;
			private int ctrlPointCnt;

			#endregion
		}

		#endregion


		#region Fields

		/// <summary>True if any other shape is connected to this shape</summary>
		protected List<ShapeConnectionInfo> connectionInfos;

		/// <summary>True if the draw cache has to be recalculated</summary>
		protected bool drawCacheIsInvalid = true;

		/// <summary></summary>
		protected bool isGluePointFollowingConnectionPoint = false;

		/// <summary>True if Dispose() was called.</summary>
		private ObjectState _objectState = ObjectState.Normal;

		/// <summary>The ShapeType of this instance.</summary>
		private ShapeType _shapeType;

		/// <summary>The display this shape is contained in. May be null.</summary>
		private IDisplayService _displayService;

		/// <summary>Template of the shape</summary>
		private Template _template;

		/// <summary>Owning ShapeCollection, typically the shape collection of the diagram or the parent shape</summary>
		private ShapeCollection _owner;

		// The model object this shape displays
		private IModelObject _modelObject;

		// Tight fitting Bounding rectangle of the rotated shape
		private Rectangle _boundingRectangleTight = Geometry.InvalidRectangle;
		// Loose bounding rectangle of the rotated shape and its control points
		private Rectangle _boundingRectangleLoose = Geometry.InvalidRectangle;

		// Counter for suspend notifying the owner of changed children position/size/rotation
		private int _notifySuspendCounter = 0;

		// Used for suspending Invalidate() while moving/resizing/rotating
		private bool _suspendInvalidate = false;

		private object _id = null;
		private object _tag = null;
		private string _data = null;
		private object _internalTag = null;
		private char _securityDomainName;
		private ILineStyle _privateLineStyle = null;
		private Matrix _matrix = null;

		private const int DefaultCapacity = 4;

		#endregion
	}


	//internal class EmptyChildrenCollection : IShapeCollection, IReadOnlyShapeCollection {

	//    public delegate void CreateCollectionCallback();

	//    public delegate void DeleteCollectionCallback();

		
	//    public EmptyChildrenCollection(CreateCollectionCallback createCallback, DeleteCollectionCallback deleteCallback) {
	//        this.createCallback = createCallback;
	//        this.deleteCallback = deleteCallback;
	//    }


	//    #region Explicit IShapeCollection implementation

	//    /// <override></override>
	//    int ICollection.Count {
	//        get { return 0; }
	//    }


	//    /// <override></override>
	//    void ICollection.CopyTo(Array array, int index) {
	//        // Nothing to do
	//    }



	//    /// <override></override>
	//    object ICollection.SyncRoot {
	//        get { return null; }
	//    }


	//    /// <override></override>
	//    bool ICollection.IsSynchronized {
	//        get { return false; }
	//    }


	//    /// <override></override>
	//    int IReadOnlyShapeCollection.MaxZOrder {
	//        get { return 0; }
	//    }


	//    /// <override></override>
	//    int IReadOnlyShapeCollection.MinZOrder {
	//        get { return 0; }
	//    }


	//    /// <override></override>
	//    Shape IReadOnlyShapeCollection.TopMost {
	//        get { return null; }
	//    }


	//    /// <override></override>
	//    Shape IReadOnlyShapeCollection.Bottom {
	//        get { return null; }
	//    }


	//    /// <override></override>
	//    IEnumerable<Shape> IReadOnlyShapeCollection.TopDown {
	//        get { return EmptyEnumerator<Shape>.Empty; }
	//    }


	//    /// <override></override>
	//    IEnumerable<Shape> IReadOnlyShapeCollection.BottomUp {
	//        get { return EmptyEnumerator<Shape>.Empty; }
	//    }


	//    /// <override></override>
	//    Shape IReadOnlyShapeCollection.FindShape(int x, int y, int width, int height, bool completelyInside, Shape lastFound) {
	//        return null;
	//    }


	//    /// <override></override>
	//    Shape IReadOnlyShapeCollection.FindShape(int x, int y, ControlPointCapabilities controlPointCapabilities, int distance, Shape lastFound) {
	//        return null;
	//    }


	//    /// <override></override>
	//    IEnumerable<Shape> IReadOnlyShapeCollection.FindShapes(int x, int y, int width, int height, bool completelyInside) {
	//        return EmptyEnumerator<Shape>.Empty;
	//    }


	//    /// <override></override>
	//    IEnumerable<Shape> IReadOnlyShapeCollection.FindShapes(int x, int y, ControlPointCapabilities controlPointCapabilities, int distance) {
	//        return EmptyEnumerator<Shape>.Empty;
	//    }


	//    /// <override></override>
	//    void IShapeCollection.Add(Shape item) {
	//        throw new NotSupportedException();
	//    }


	//    /// <override></override>
	//    void IShapeCollection.Add(Shape item, int zOrder) {
	//        throw new NotSupportedException();
	//    }


	//    /// <override></override>
	//    void IShapeCollection.Clear() {
	//        throw new NotSupportedException();
	//    }


	//    /// <override></override>
	//    bool IShapeCollection.Contains(Shape item) {
	//        return false;
	//    }


	//    /// <override></override>
	//    bool IShapeCollection.ContainsAll(IEnumerable<Shape> items) {
	//        return false;
	//    }


	//    /// <override></override>
	//    bool IShapeCollection.ContainsAny(IEnumerable<Shape> items) {
	//        return false;
	//    }


	//    /// <override></override>
	//    void IShapeCollection.CopyTo(Shape[] array, int arrayIndex) {
	//        // Nothing to do
	//    }


	//    /// <override></override>
	//    bool IShapeCollection.Remove(Shape item) {
	//        throw new NotSupportedException();
	//    }


	//    /// <override></override>
	//    void IShapeCollection.AddRange(IEnumerable<Shape> shapes) {
	//        throw new NotSupportedException();
	//    }


	//    /// <override></override>
	//    void IShapeCollection.Replace(Shape oldShape, Shape newShape) {
	//        throw new NotSupportedException();
	//    }


	//    /// <override></override>
	//    void IShapeCollection.ReplaceRange(IEnumerable<Shape> oldShapes, IEnumerable<Shape> newShapes) {
	//        throw new NotSupportedException();
	//    }


	//    /// <override></override>
	//    bool IShapeCollection.RemoveRange(IEnumerable<Shape> shapes) {
	//        throw new NotSupportedException();
	//    }


	//    /// <override></override>
	//    void IShapeCollection.SetZOrder(Shape shape, int zOrder) {
	//        throw new InvalidOperationException("The given shape does not exist in the collection.");
	//    }

	//    #endregion


	//    #region Explicit IEnumerable<Shape> implementation

	//    IEnumerator<Shape> IEnumerable<Shape>.GetEnumerator() {
	//        return EmptyEnumerator<Shape>.Empty;
	//    }

	//    #endregion


	//    #region Explicit IEnumerable implementation

	//    IEnumerator IEnumerable.GetEnumerator() {
	//        return EmptyEnumerator<Shape>.Empty;
	//    }

	//    #endregion


	//    private CreateCollectionCallback createCallback = null;
	//    private DeleteCollectionCallback deleteCallback = null;
	//}

}
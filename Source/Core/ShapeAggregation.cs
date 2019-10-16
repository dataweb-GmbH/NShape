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


namespace Dataweb.NShape.Advanced {

	#region ShapeAggregation abstract base class

	/// <summary>
	/// Defines a set of shapes aggregated into another shape.
	/// </summary>
	public abstract class ShapeAggregation : ShapeCollection {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ShapeAggregation" />.
		/// </summary>
		protected ShapeAggregation(Shape owner)
			: base() {
			Construct(owner);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ShapeAggregation" />.
		/// </summary>
		protected ShapeAggregation(Shape owner, int capacity)
			: base(capacity) {
			Construct(owner, capacity);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ShapeAggregation" />.
		/// </summary>
		protected ShapeAggregation(Shape owner, IEnumerable<Shape> collection)
			: base(collection) {
			Construct(owner, Shapes.Capacity);
		}


		#region ShapeAggregation Members called by the owner shape

		/// <ToBeCompleted></ToBeCompleted>
		public Shape Owner {
			get { return _owner; }
			protected set { _owner = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public virtual void CopyFrom(IShapeCollection source) {
			if (source == null) throw new ArgumentNullException("source");
			Clear();
			foreach (Shape shape in source.BottomUp) {
				Shape shapeClone = null;
				// If the parent shape has no template, we assume that this is 
				// the shape of the template itself, so it's children should be
				// template-free, too
				if (Owner.Template == null) {
					shapeClone = shape.Type.CreateInstance();
					shapeClone.CopyFrom(shape);
				} else shapeClone = shape.Clone();
				shapeClone.ZOrder = shape.ZOrder;
				shapeClone.Parent = this.Owner;
				shapeClone.DisplayService = this.Owner.DisplayService;

				this.Shapes.Add(shapeClone);
				this.AddShapeToIndex(shapeClone);
			}

			if (source is ShapeAggregation) {
				ShapeAggregation src = (ShapeAggregation)source;
				this._aggregationAngle = src._aggregationAngle;
				this._rotationCenter = src._rotationCenter;
				//
				int shapeCnt = Shapes.Count;
				// Copy center points of the shapes
				this.ShapePositions.Clear();
				for (int i = 0; i < shapeCnt; ++i) {
					Point shapePos = src.ShapePositions[src.Shapes[i]];
					this.ShapePositions.Add(Shapes[i], shapePos);
				}
				// Copy relative positions of the children
				RelativePointPositions.Clear();
				for (int i = 0; i < shapeCnt; ++i) {
					// Copy all items
					PointPositions srcPtPositions = src.RelativePointPositions[src.Shapes[i]];
					PointPositions dstPtPositions = new PointPositions();
					foreach (KeyValuePair<ControlPointId, RelativePosition> item in srcPtPositions.Items)
						dstPtPositions.Items.Add(item.Key, item.Value);
					RelativePointPositions.Add(Shapes[i], dstPtPositions);
				}
			} else {
				// If the source ShapeCollection is not a ShapeAggregation, 
				// store unrotated ShapePositions nevertheless
				this.ShapePositions.Clear();
				foreach (Shape shape in Shapes) {
					Point shapePos = Point.Empty;
					shapePos.Offset(shape.X, shape.Y);
					this.ShapePositions.Add(shape, shapePos);
					this.RelativePointPositions.Add(shape, new PointPositions(shape, Owner));
				}
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void SetPreviewStyles(IStyleSet styleSet) {
			if (styleSet == null) throw new ArgumentNullException("styleSet");
			foreach (Shape shape in Shapes)
				shape.MakePreview(styleSet);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool ContainsPoint(int x, int y) {
			// TODO 2: Could be optimized using the shapeMap.
			foreach (Shape shape in TopDown) {
				if (shape.ContainsPoint(x, y))
					return true;
			}
			return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool IntersectsWith(int x, int y, int width, int height) {
			foreach (Shape shape in TopDown)
				if (shape.IntersectsWith(x, y, width, height))
					return true;
			return false;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public bool NotifyStyleChanged(IStyle style) {
			bool result = false;
			foreach (Shape shape in Shapes)
				if (shape.NotifyStyleChanged(style)) result = true;
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void Draw(Graphics graphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			foreach (Shape shape in BottomUp)
				shape.Draw(graphics);

#if DEBUG_DIAGNOSTICS
			//foreach (KeyValuePair<Shape, Point> item in shapePositions) {
			//    GdiHelpers.DrawPoint(graphics, Pens.Lime, item.Key.X, item.Key.Y, 3);
			//    GdiHelpers.DrawPoint(graphics, Pens.Red, item.Value.X, item.Value.Y, 3);
			//}
			//GdiHelpers.DrawAngle(graphics, new SolidBrush(Color.FromArgb(96, Color.Yellow)), rotationCenter, Geometry.TenthsOfDegreeToDegrees(aggregationAngle), 20);
			//GdiHelpers.DrawPoint(graphics, Pens.Blue, rotationCenter.X, rotationCenter.Y, 3);
#endif
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void DrawOutline(Graphics graphics, Pen pen) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (pen == null) throw new ArgumentNullException("pen");
			foreach (Shape shape in BottomUp) shape.DrawOutline(graphics, pen);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal void Invalidate() {
			foreach (Shape shape in Shapes)
				shape.Invalidate();
		}

		#endregion


		#region Methods called by the owner

 		/// <summary>
		/// Notifies the child shapes that their parent has been rotated. Rotates all children according to the parent's rotation.
		/// </summary>
		/// <returns>
		/// True if all child shapes have rotated in the desired way. 
		/// False if the child shapes cannot move as desired due to restrictions (such as connections).
		/// </returns>
		public virtual bool NotifyParentRotated(int angle, int rotationCenterX, int rotationCenterY) {
			if (Shapes.Count == 0) return false;
			else {
				bool result = true;
				try {
					SuspendUpdate();
					RevertRotation();

					// set new rotation center and angle
					_aggregationAngle = (_aggregationAngle + angle) % 3600;
					_rotationCenter.X = rotationCenterX;
					_rotationCenter.Y = rotationCenterY;

					// rotate child shapes around the parent's center
					foreach (Shape shape in Shapes) {
						// Get glue point states and handle connected glue points if there are any:
						//   * Skip all shapes with connected glue points as they will move with their partner shape anyway
						//   * Check all shapes *without* glue points whether shapes are connected and move control points 
						//     of these shapes (the ones with glue points) to the expected positions
						GluePointState gluePointState = GetGluePointState(shape);
						if (gluePointState == GluePointState.AllConnected || gluePointState == GluePointState.SomeConnected)
							continue;
						// Rotate shape
						if (!shape.Rotate(_aggregationAngle, _rotationCenter.X, _rotationCenter.Y))
							result = false;
						// Check whether the shape has connected shapes
						foreach (ShapeConnectionInfo connInfo in shape.GetConnectionInfos(ControlPointId.Any, null)) {
							Shape connectedShape = connInfo.OtherShape;
							// Skip shapes that are not part of the aggregation
							if (ShapePositions.ContainsKey(connectedShape)) {
								// Rotate connected shape
								if (!connectedShape.Rotate(_aggregationAngle, _rotationCenter.X, _rotationCenter.Y))
									result = false;
								// Move control points to the expected positions
								RestorePointPositions(connectedShape);
							}
						}
					}
					// Reset bounding rectangles
					boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
				} finally { ResumeUpdate(); }
				return result;
			}
		}


		/// <summary>
		/// Notifies the child shapes that their parent has been moved. Moves all children according to the parent's movement.
		/// </summary>
		/// <returns>
		/// True if all child shapes have moved in the desired way. 
		/// False if the child shapes cannot move as desired due to restrictions (such as connections).
		/// </returns>
		public virtual bool NotifyParentMoved(int deltaX, int deltaY) {
			if (Shapes.Count == 0) return true;
			else {
				bool result = true;
				try {
					SuspendUpdate();
					// move child shapes with parent
					foreach (Shape shape in Shapes) {
						// Update unrotated shape center - regardless wether the shape is moved or not (shapes with
						// connected glue points will not be moved)
						// Due to the fact that Point is a value Type, we have to overwrite the Point with a new Point.
						Point shapePos = ShapePositions[shape];
						shapePos.Offset(deltaX, deltaY);
						ShapePositions[shape] = shapePos;

						// Get glue point states and handle connected glue points if there are any:
						//   * Skip all shapes with connected glue points as they will move with their partner shape anyway
						//   * Check all shapes *without* glue points whether shapes are connected and move control points 
						//     of these shapes (the ones with glue points) to the expected positions
						GluePointState gluePointState = GetGluePointState(shape);
						if (gluePointState == GluePointState.AllConnected || gluePointState == GluePointState.SomeConnected)
							continue;

						// Move shape
						if (!shape.MoveBy(deltaX, deltaY))
							result = false;
						// Check whether the shape has connected shapes
						foreach (ShapeConnectionInfo connInfo in shape.GetConnectionInfos(ControlPointId.Any, null)) {
							// Skip shapes that are not part of the aggregation
							Shape connectedShape = connInfo.OtherShape;
							Point expectedShapePos;
							if (ShapePositions.TryGetValue(connectedShape, out expectedShapePos)) {
								expectedShapePos.Offset(deltaX, deltaY);
								// Move control points to the expected positions
								if (connectedShape is IPlanarShape) {
									if (expectedShapePos.X != connectedShape.X || expectedShapePos.Y != connectedShape.Y)
										connectedShape.MoveTo(expectedShapePos.X, expectedShapePos.X);
								} 
								RestorePointPositions(connectedShape);
							}
						}
					}
					if (Geometry.IsValid(boundingRectangleTight))
						boundingRectangleTight.Offset(deltaX, deltaY);
					if (Geometry.IsValid(boundingRectangleLoose))
						boundingRectangleLoose.Offset(deltaX, deltaY);
				} finally { ResumeUpdate(); }
				return result;
			}
		}


		/// <summary>
		/// Notifies the child shapes that their parent has been resized. The action performed depends on the implementing class.
		/// </summary>
		/// <returns>
		/// True if all child shapes have resized in the desired way. 
		/// False if the child shapes cannot move as desired due to restrictions (such as connections).
		/// </returns>
		public abstract bool NotifyParentSized(int deltaX, int deltaY);

		#endregion


		#region ShapeAggregation Methods called by the child shapes

		/// <override></override>
		public override void NotifyChildMoved(Shape shape) {
			base.NotifyChildMoved(shape);
			// reset rotation of the ShapeAggregation if any of the Children are changed directly
			if (!UpdateSuspended) {
				// reset rotation info and re-assign (unrotated) shape position
				//ResetRotation();	// ToDo: Only reset position of the particular shape
				
				UpdateShapePosition(shape);
				UpdateRelativePositions(shape);
				boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
			}
		}


		/// <override></override>
		public override void NotifyChildResized(Shape shape) {
			base.NotifyChildResized(shape);
			// reset rotation of the ShapeAggregation if any of the Children are changed directly
			if (!UpdateSuspended) {
				// reset rotation info and re-assign (unrotated) shape positions
				//ResetRotation();

				UpdateShapePosition(shape);
				UpdateRelativePositions(shape);
				boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
			}
		}


		/// <override></override>
		public override void NotifyChildRotated(Shape shape) {
			base.NotifyChildRotated(shape);
			if (!UpdateSuspended) {
				UpdateRelativePositions(shape);
				boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
			}
		}

		#endregion


		#region Overridden methods (protected)

		/// <override></override>
		protected override void ClearCore() {
			foreach (Shape shape in Shapes) {
				shape.Invalidate();
				shape.Parent = null;
				shape.DisplayService = null;
			}
			ShapePositions.Clear();
			RelativePointPositions.Clear();
			base.ClearCore();
		}


		/// <override></override>
		protected override int InsertCore(int index, Shape shape) {
			int result = base.InsertCore(index, shape);
			// Store position of the (unrotated) shape and reset bounding rectangle
			AddShapePosition(shape);
			AddRelativePosition(shape);
			// Set shape's parent
			shape.Parent = Owner;
			shape.DisplayService = Owner.DisplayService;
			return result;
		}


		/// <override></override>
		protected override bool RemoveCore(Shape shape) {
			bool result = base.RemoveCore(shape);
			if (result) {
				// Remove positions of the (unrotated) shape and reset bounding rectangle
				ShapePositions.Remove(shape);
				RelativePointPositions.Remove(shape);
				// Reset shape's parent and display service
				shape.Parent = null;
				shape.DisplayService = null;
			}
			return result;
		}


		/// <override></override>
		protected override void ReplaceCore(Shape oldShape, Shape newShape) {
			base.ReplaceCore(oldShape, newShape);
			// Reset old shape's Parent and DisplayService
			oldShape.Parent = null;
			oldShape.DisplayService = null;
			// Set new shape's Parent and DisplayService
			newShape.Parent = Owner;
			newShape.DisplayService = Owner.DisplayService;
		}

		#endregion


		/// <summary>List of unrotated positions (X/Y) of the shapes</summary>
		protected Dictionary<Shape, Point> ShapePositions {
			get { return _shapePositions; }
			set { _shapePositions = value; }
		}


		/// <summary>List of RelativePosition for each shape's anchor control points</summary>
		protected Dictionary<Shape, PointPositions> RelativePointPositions {
			get { return _relativePointPositions; }
			set { _relativePointPositions = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void SuspendUpdate() {
			++_suspendCounter;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected void ResumeUpdate() {
			Debug.Assert(_suspendCounter > 0);
			--_suspendCounter;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected bool UpdateSuspended { get { return _suspendCounter > 0; } }


		/// <ToBeCompleted></ToBeCompleted>
		private bool RestorePointPositions(Shape shape) {
			bool result = true;
			try {
				SuspendUpdate();
				Rectangle ownerBounds = Owner.GetBoundingRectangle(true);
				// Move given child shape to its new absolute position calculated from relativePosition
				PointPositions ptPositions = RelativePointPositions[shape];
				Debug.Assert(ptPositions != null);

				// Now restore positions of the anchor points
				foreach (ControlPointId id in GetAnchorPointIds(shape)) {
					ShapeConnectionInfo connInfo = ShapeConnectionInfo.Empty;
					if (shape.HasControlPointCapability(id, ControlPointCapabilities.Glue))
						connInfo = shape.GetConnectionInfo(id, null);

					// Move connected glue points to their partner point
					if (connInfo != ShapeConnectionInfo.Empty)
						shape.FollowConnectionPointWithGluePoint(id, connInfo.OtherShape, connInfo.OtherPointId);
					else {
						RelativePosition relPos;
						if (ptPositions.Items.TryGetValue(id, out relPos)) {
							Debug.Assert(relPos != RelativePosition.Empty);
							Point p = Owner.CalculateAbsolutePosition(relPos);
							Debug.Assert(Geometry.IsValid(p));

							//p = Geometry.RotatePoint(rotationCenter, Geometry.TenthsOfDegreeToDegrees(aggregationAngle), p);
							if (!shape.MoveControlPointTo(id, p.X, p.Y, ResizeModifiers.None))
								result = false;
						} else Debug.Fail("Unable to restore controlpoint's position: Control point position not found in list of original positions");
					}
				}
				boundingRectangleLoose = boundingRectangleTight = Geometry.InvalidRectangle;
			} finally {
				ResumeUpdate();
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		private IEnumerable<Point> GetPointPositions(Shape shape, IEnumerable<RelativePosition> relativePositions) {
			foreach (RelativePosition relPos in relativePositions)
				yield return shape.CalculateAbsolutePosition(relPos);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected IEnumerable<ControlPointId> GetAnchorPointIds(Shape shape) {
			// Get the 'Anchor' points of the given shape
			IEnumerable<ControlPointId> anchorPointIds;
			// As long as we do not have attributes for control points that specify move restrictions etc, we
			// use specific implementations that work better and/or faster for some shape types and a default 
			// implementation that works for all other shapes. 
			if (shape is ILinearShape) {
				anchorPointIds = GetLineAnchorPoints(shape);
			} else if (shape is DiameterShapeBase) {
				Rectangle dstBounds = Rectangle.Empty;
				PointPositions ptPositions = RelativePointPositions[shape];
				Geometry.CalcBoundingRectangle(GetPointPositions(Owner, ptPositions.Items.Values), out dstBounds);
				anchorPointIds = GetDiameterShapeAnchorPoints((DiameterShapeBase)shape, dstBounds);
			} else if (shape is ImageBasedShape) {
				anchorPointIds = GetImageAnchorPoints((ImageBasedShape)shape);
			} else if (shape is IsoscelesTriangleBase) {
				anchorPointIds = GetIsoscelesTriangleAnchorPoints((IsoscelesTriangleBase)shape);
			} else if (shape is RegularPolygoneBase) {
				Rectangle dstBounds = Rectangle.Empty;
				PointPositions ptPositions = RelativePointPositions[shape];
				Geometry.CalcBoundingRectangle(GetPointPositions(Owner, ptPositions.Items.Values), out dstBounds);
				anchorPointIds = GetRegularPolygonAnchorPoints((RegularPolygoneBase)shape, dstBounds);
			} else
				anchorPointIds = GetShapeAnchorPoints(shape);
			return anchorPointIds;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected GluePointState GetGluePointState(Shape shape) {
			int gluePtCnt = 0, connectedGluePtCnt = 0;
			foreach (ControlPointId gluePtId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
				++gluePtCnt;
				if (shape.IsConnected(gluePtId, null) != ControlPointId.None)
					++connectedGluePtCnt;
			}
			// Skip shapes connected with all glue points as they will move with their partner shape
			if (gluePtCnt == 0)
				return GluePointState.HasNone;
			else {
				if (connectedGluePtCnt == 0)
					return GluePointState.NoneConnected;
				if (connectedGluePtCnt == gluePtCnt)
					return GluePointState.AllConnected;
				else return GluePointState.SomeConnected;
			}
		}


		private void Construct(Shape owner) {
			Construct(owner, -1);
		}


		private void Construct(Shape owner, int capacity) {
			if (owner == null) throw new ArgumentNullException("owner");
			this._owner = owner;
			if (capacity <= 0) {
				_shapePositions = new Dictionary<Shape, Point>();
				_relativePointPositions = new Dictionary<Shape, PointPositions>();
			} else {
				_shapePositions = new Dictionary<Shape, Point>(capacity);
				_relativePointPositions = new Dictionary<Shape, PointPositions>(capacity);
			}
		}


		private void AddShapePosition(Shape shape) {
			// Transform the shape to the coordinate system of the rotated aggregation
			if (_aggregationAngle != 0) shape.Rotate(-_aggregationAngle, _rotationCenter.X, _rotationCenter.Y);

			// Get the shape's original center point (for restoring the unrotated position)
			Point shapeCenter = Point.Empty;
			shapeCenter.X = shape.X;
			shapeCenter.Y = shape.Y;
			ShapePositions.Add(shape, shapeCenter);

			// Transform the shape back to the diagram's coordinate system
			if (_aggregationAngle != 0) shape.Rotate(_aggregationAngle, _rotationCenter.X, _rotationCenter.Y);
		}


		private void AddRelativePosition(Shape shape) {
			// Transform the shape to the coordinate system of the rotated aggregation
			if (_aggregationAngle != 0) shape.Rotate(-_aggregationAngle, _rotationCenter.X, _rotationCenter.Y);

			PointPositions ptPositions = new PointPositions(shape, Owner);
			RelativePointPositions.Add(shape, ptPositions);

			// Transform the shape back to the diagram's coordinate system
			if (_aggregationAngle != 0) shape.Rotate(_aggregationAngle, _rotationCenter.X, _rotationCenter.Y);
		}
	

		private void RevertRotation() {
			foreach (Shape shape in Shapes) {
				Point unrotatedShapeCenter = ShapePositions[shape];
				shape.Rotate(-_aggregationAngle, shape.X, shape.Y);
				shape.MoveTo(unrotatedShapeCenter.X, unrotatedShapeCenter.Y);
			}
		}


		private void UpdateShapePosition(Shape shape) {
			// Calculate the new unrotated shape position and 
			// update it in the shapePositions dictionary
			Point currentPos = Point.Empty;
			currentPos.Offset(shape.X, shape.Y);
			if (_aggregationAngle != 0)
				currentPos = Geometry.RotatePoint(_rotationCenter,
					-Geometry.TenthsOfDegreeToDegrees(_aggregationAngle), currentPos);
			ShapePositions[shape] = currentPos;
		}


		private void UpdateRelativePositions(Shape shape) {
			PointPositions ptPositions = new PointPositions(shape, Owner);
			RelativePointPositions[shape] = ptPositions;
		}


		private IEnumerable<ControlPointId> GetLineAnchorPoints(Shape lineShape) {
			if (lineShape == null) throw new ArgumentNullException("lineShape");
			if (!(lineShape is ILinearShape)) throw new ArgumentException("Shape does not implement ILinearShape");
			// Return end points first
			yield return ControlPointId.FirstVertex;
			yield return ControlPointId.LastVertex;
			ILinearShape linearShape = (ILinearShape)lineShape;
			foreach (ControlPointId id in lineShape.GetControlPointIds(ControlPointCapabilities.Resize)) {
				// Skip reference and glue points
				if (lineShape.HasControlPointCapability(id, ControlPointCapabilities.Reference | ControlPointCapabilities.Glue))
					continue;
				yield return id;
			}
		}


		private IEnumerable<ControlPointId> GetIsoscelesTriangleAnchorPoints(IsoscelesTriangleBase triangleShape) {
			if (triangleShape == null) throw new ArgumentNullException("triangleShape");
			// ToDo: Ensure that the ControlPointId values match the expected values
			const int TopCenterControlPoint = 1;
			const int BottomLeftControlPoint = 2;
			const int BottomRightControlPoint = 4;
			// First, (re)store the shape position
			yield return ControlPointId.Reference;
			// Afterwards, (re)store position of resize points
			yield return TopCenterControlPoint;
			yield return BottomLeftControlPoint;
			yield return BottomRightControlPoint;
		}


		private IEnumerable<ControlPointId> GetDiameterShapeAnchorPoints(DiameterShapeBase diameterShape, Rectangle ownerBounds) {
			if (diameterShape == null) throw new ArgumentNullException("diameterShape");
			// ToDo: Ensure that the ControlPointId values match the expected values
			const int TopCenterControlPoint = 2;
			const int MiddleLeftControlPoint = 4;
			const int MiddleRightControlPoint = 5;
			const int BottomCenterControlPoint = 7;
			// (Re)store top left and bottom right control points and (at last) the position
			yield return ControlPointId.Reference;
			if (ownerBounds.Width <= ownerBounds.Height) {
				yield return MiddleLeftControlPoint;
				yield return MiddleRightControlPoint;
			} else {
				yield return TopCenterControlPoint;
				yield return BottomCenterControlPoint;
			}
		}


		private IEnumerable<ControlPointId> GetRegularPolygonAnchorPoints(RegularPolygoneBase polygonShape, Rectangle ownerBounds) {
			if (polygonShape == null) throw new ArgumentNullException("polygonShape");
			// (Re)store the shape position and one resize point (sufficient for defining the radius)
			yield return ControlPointId.Reference;
			// Calculate the point that is closest to the ownerBounds
			ControlPointId id = ControlPointId.None;
			int radius;
			Point dstA = Point.Empty, dstB = Point.Empty;
			if (ownerBounds.Width < ownerBounds.Height) {
				radius = ownerBounds.Width / 2;
				dstA.Offset(ownerBounds.Left, polygonShape.Y);
				dstB.Offset(ownerBounds.Right, polygonShape.Y);
			} else {
				radius = ownerBounds.Height / 2;
				dstA.Offset(polygonShape.X, ownerBounds.Top);
				dstB.Offset(polygonShape.X, ownerBounds.Bottom);
			}
			float lowestDistance = int.MaxValue;
			int vertexCnt = polygonShape.VertexCount;
			float angle = 360f / vertexCnt;
			for (int i = 0; i < vertexCnt; ++i) {
				Point p = Geometry.RotatePoint(polygonShape.X, polygonShape.Y, i * angle, polygonShape.X, polygonShape.Y + radius);
				float currentDistance = Math.Min(Geometry.DistancePointPoint(dstA, p), Geometry.DistancePointPoint(dstB, p));
				if (currentDistance < lowestDistance) {
					lowestDistance = currentDistance;
					id = i + 1;
				}
			}
			yield return id;
			// Correct the final position
			yield return ControlPointId.Reference;
		}


		private IEnumerable<ControlPointId> GetImageAnchorPoints(ImageBasedShape imageShape) {
			if (imageShape == null) throw new ArgumentNullException("imageShape");
			// (Re)store the corner and bottom resize point and the shape position
			yield return ControlPointId.Reference;
			yield return 8;
			// Correct the final position
			yield return ControlPointId.Reference;
		}


		private IEnumerable<ControlPointId> GetShapeAnchorPoints(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			// First, (re)store position of reference point
			yield return ControlPointId.Reference;
			// Then, (re)store positions of all other resize points
			foreach (ControlPointId id in shape.GetControlPointIds(ControlPointCapabilities.Resize | ControlPointCapabilities.Glue))
				yield return id;
		}


		#region Types and Fields

		/// <ToBeCompleted></ToBeCompleted>
		protected enum GluePointState {
			/// <ToBeCompleted></ToBeCompleted>
			HasNone,
			/// <ToBeCompleted></ToBeCompleted>
			AllConnected,
			/// <ToBeCompleted></ToBeCompleted>
			SomeConnected,
			/// <ToBeCompleted></ToBeCompleted>
			NoneConnected
		};

			/// <ToBeCompleted></ToBeCompleted>
		protected class PointPositions {

			/// <ToBeCompleted></ToBeCompleted>
			public PointPositions() {
				items = new SortedList<ControlPointId, RelativePosition>();
			}

			/// <ToBeCompleted></ToBeCompleted>
			public PointPositions(Shape shape, Shape owner)
				: this() {
				if (shape == null) throw new ArgumentNullException("shape");
				if (owner == null) throw new ArgumentNullException("owner");
				// First, store position of reference point
				RelativePosition relativePos = RelativePosition.Empty;
				relativePos = owner.CalculateRelativePosition(shape.X, shape.Y);
				Debug.Assert(relativePos != RelativePosition.Empty);
				items.Add(ControlPointId.Reference, relativePos);
				// Then, store all resize control point positions as relative position
				foreach (ControlPointId ptId in shape.GetControlPointIds(ControlPointCapabilities.Resize)) {
					Debug.Assert(ptId != ControlPointId.None);
					Point p = shape.GetControlPointPosition(ptId);
					relativePos = owner.CalculateRelativePosition(p.X, p.Y);
					Debug.Assert(relativePos != RelativePosition.Empty);
					items.Add(ptId, relativePos);
				}
			}

			/// <ToBeCompleted></ToBeCompleted>
			public SortedList<ControlPointId, RelativePosition> Items {
				get { return items; }
			}

			/// <ToBeCompleted></ToBeCompleted>
			private SortedList<ControlPointId, RelativePosition> items;
		}


		/// <summary>List of unrotated positions (X/Y) of the shapes</summary>
		private Dictionary<Shape, Point> _shapePositions;
		/// <summary>List of RelativePosition for each shape's anchor control points</summary>
		private Dictionary<Shape, PointPositions> _relativePointPositions;

		private Shape _owner = null;
		private int _suspendCounter = 0;
		// Fields for rotating shapes
		private int _aggregationAngle = 0;	// rotation angle in degrees
		private Point _rotationCenter = Point.Empty;
		
		#endregion
	}

	#endregion


	#region GroupAggregation class

	/// <ToBeCompleted></ToBeCompleted>
	public class GroupShapeAggregation : ShapeAggregation {

		/// <ToBeCompleted></ToBeCompleted>
		public GroupShapeAggregation(Shape owner)
			: base(owner) {
			if (owner == null) throw new ArgumentNullException("owner");
			if (!(owner is IShapeGroup)) throw new ArgumentException("owner");
		}


		/// <ToBeCompleted></ToBeCompleted>
		public GroupShapeAggregation(Shape owner, int capacity)
			: base(owner, capacity) {
			if (owner == null) throw new ArgumentNullException("owner");
			if (!(owner is IShapeGroup)) throw new ArgumentException("owner");
		}


		/// <override></override>
		public override void CopyFrom(IShapeCollection source) {
			base.CopyFrom(source);
			if (source is GroupShapeAggregation)
			   this._center = ((GroupShapeAggregation)source).Center;
			else CalcCenter();
		}


		/// <override></override>
		public override bool NotifyParentMoved(int deltaX, int deltaY) {
			bool result = true;
			try {
				SuspendUpdate();
				_center.Offset(deltaX, deltaY);
				base.NotifyParentMoved(deltaX, deltaY);
			} finally { ResumeUpdate(); }

			return result;
		}


		/// <override></override>
		public override bool NotifyParentSized(int deltaX, int deltaY) {
			try {
				SuspendUpdate();
				// Nothing to do
			} finally { ResumeUpdate(); }
			return false;
		}


		/// <override></override>
		public override void NotifyChildMoving(Shape shape) {
			base.NotifyChildMoving(shape);
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
		}


		/// <override></override>
		public override void NotifyChildMoved(Shape shape) {
			base.NotifyChildMoved(shape);
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChanged();
			}
		}


		/// <override></override>
		public override void NotifyChildResizing(Shape shape) {
			base.NotifyChildResizing(shape);
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
		}


		/// <override></override>
		public override void NotifyChildResized(Shape shape) {
			base.NotifyChildResized(shape);
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChanged();
			}
		}


		/// <override></override>
		public override void NotifyChildRotating(Shape shape) {
			base.NotifyChildRotating(shape);
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
		}


		/// <override></override>
		public override void NotifyChildRotated(Shape shape) {
			base.NotifyChildRotated(shape);
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChanged();
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Point Center {
			get { return _center; }
		}


		#region Overidden protected methods

		/// <override></override>
		protected override int InsertCore(int index, Shape shape) {
			int result = base.InsertCore(index, shape);
			if (!UpdateSuspended) {
				if (Owner != null) Owner.NotifyChildLayoutChanging();
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChanged();
			}
			return result;
		}


		/// <override></override>
		protected override void AddRangeCore(IEnumerable<Shape> collection) {
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
			try {
				SuspendUpdate();
				base.AddRangeCore(collection);
			} finally { ResumeUpdate(); }
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChanged();
			}
		}


		/// <override></override>
		protected override bool RemoveCore(Shape shape) {
			bool result = base.RemoveCore(shape);
			if (!UpdateSuspended) {
				if (Owner != null) Owner.NotifyChildLayoutChanging();
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChanged();
			}
			return result;
		}


		/// <override></override>
		protected override bool RemoveRangeCore(IEnumerable<Shape> shapes) {
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
			bool result = false;
			try {
				SuspendUpdate();
				result = base.RemoveRangeCore(shapes);
			} finally { ResumeUpdate(); }
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChanged();
			}
			return result;
		}


		/// <override></override>
		protected override void ReplaceCore(Shape oldShape, Shape newShape) {
			base.ReplaceCore(oldShape, newShape);
			if (!UpdateSuspended) {
				if (Owner != null) Owner.NotifyChildLayoutChanging();
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChanged();
			}
		}


		/// <override></override>
		protected override void ReplaceRangeCore(IEnumerable<Shape> oldShapes, IEnumerable<Shape> newShapes) {
			if (!UpdateSuspended && Owner != null) Owner.NotifyChildLayoutChanging();
			try {
				SuspendUpdate();
				base.ReplaceRangeCore(oldShapes, newShapes);
			} finally { ResumeUpdate(); }
			if (!UpdateSuspended) {
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChanged();
			}
		}


		/// <override></override>
		protected override void ClearCore() {
			base.ClearCore();
			if (!UpdateSuspended) {
				if (Owner != null) Owner.NotifyChildLayoutChanging();
				CalcCenter();
				if (Owner != null) Owner.NotifyChildLayoutChanged();
			}
		}

		#endregion


		/// <ToBeCompleted></ToBeCompleted>
		protected new IShapeGroup Owner {
			get { return (IShapeGroup)base.Owner; }
		}
		
		
		private void CalcCenter() {
			Rectangle r = GetBoundingRectangle(true);
			_center.X = r.X + (int)Math.Round(r.Width / 2f);
			_center.Y = r.Y + (int)Math.Round(r.Height / 2f);
		}


		#region Fields
		private Point _center = Point.Empty;
		#endregion
	}

	#endregion


	#region CompositeShapeAggregation class

	/// <ToBeCompleted></ToBeCompleted>
	public class CompositeShapeAggregation : ShapeAggregation {

		/// <ToBeCompleted></ToBeCompleted>
		public CompositeShapeAggregation(Shape owner)
			: base(owner) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CompositeShapeAggregation(Shape owner, int capacity)
			: base(owner, capacity) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CompositeShapeAggregation(Shape owner, IEnumerable<Shape> collection)
			: base(owner, (collection is ICollection<Shape>) ? ((ICollection<Shape>)collection).Count : -1) {
			if (collection == null) throw new ArgumentNullException("collection");
			// if collection provides an indexer, use it in order to reduce overhead
			if (collection is IList<Shape>) {
				IList<Shape> shapeList = (IList<Shape>)collection;
				int cnt = shapeList.Count;
				// Create dictionary of relative positions
				RelativePointPositions = new Dictionary<Shape, PointPositions>(cnt);
				// Add shapes
				foreach (Shape shape in shapeList)
					Add(shape);
			} else {
				// Create dictionary of relative positions
				if (collection is ICollection<Shape>)
					RelativePointPositions = new Dictionary<Shape, PointPositions>(((ICollection<Shape>)collection).Count);
				else RelativePointPositions = new Dictionary<Shape, PointPositions>();
				// Add shapes
				foreach (Shape shape in collection)
					Add(shape);
			}
		}


		/// <override></override>
		public override void CopyFrom(IShapeCollection source) {
			base.CopyFrom(source);
		}


		/// <override></override>
		public override bool NotifyParentSized(int deltaX, int deltaY) {
			return RestoreChildrenPointPositions();
		}


		/// <override></override>
		public override bool NotifyParentRotated(int angle, int rotationCenterX, int rotationCenterY) {
			if (base.NotifyParentRotated(angle, rotationCenterX, rotationCenterY))
				return RestoreChildrenPointPositions();
			else return false;
		}


		private bool RestoreChildrenPointPositions() {
			bool result = true;
			try {
				SuspendUpdate();
				Rectangle ownerBounds = Owner.GetBoundingRectangle(true);
				// Move children to their new absolute position calculated from relativePosition
				foreach (Shape shape in Shapes) {
					// Get the state of the shape's glue points
					GluePointState gluePointState = GetGluePointState(shape);
					if (gluePointState == GluePointState.AllConnected)
						continue;
					PointPositions ptPositions = RelativePointPositions[shape];
					Debug.Assert(ptPositions != null);

					// Now restore positions of the anchor points
					foreach (ControlPointId id in GetAnchorPointIds(shape)) {
						// Skip connected glue points
						if (gluePointState == GluePointState.SomeConnected) {
							if (shape.HasControlPointCapability(id, ControlPointCapabilities.Glue)
								&& shape.IsConnected(id, null) != ControlPointId.None)
								continue;
						}
						RelativePosition relPos;
						if (ptPositions.Items.TryGetValue(id, out relPos)) {
							Debug.Assert(relPos != RelativePosition.Empty);
							Point p = Owner.CalculateAbsolutePosition(relPos);
							Debug.Assert(Geometry.IsValid(p));
							if (!shape.MoveControlPointTo(id, p.X, p.Y, ResizeModifiers.None))
								result = false;
						} else Debug.Fail("Unable to restore controlpoint's position: Control point position not found in list of original positions");
					}
				}
				boundingRectangleLoose = boundingRectangleTight = Geometry.InvalidRectangle;
			} finally {
				ResumeUpdate();
			}
			return result;
		}

	}

	#endregion

}
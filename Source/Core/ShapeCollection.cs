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
using System.Collections;
using System.Collections.Generic;

using Dataweb.NShape.Advanced;
using System.Drawing;
using Dataweb.Utilities;
using System.Threading;
using System.Diagnostics;


namespace Dataweb.NShape {

	/// <summary>
	/// Defines the directions in which a shape is lifted.
	/// </summary>
	/// <status>Reviewed</status>
	public enum ZOrderDestination {
		/// <summary>Send to the back.</summary>
		ToBottom,
		/// <summary>Send one backwards.</summary>
		Downwards,
		/// <summary>Bring upwards by one.</summary>
		Upwards,
		/// <summary>Bring to the top.</summary>
		ToTop
	}


	/// <summary>
	/// A read-only collection of shapes sorted by z-order.
	/// </summary>
	/// <remarks>Also providing methods to find shapes and process the collection 
	/// items in every direction.</remarks>
	/// <status>Reviewed</status>
	public interface IReadOnlyShapeCollection : IReadOnlyCollection<Shape> {

		/// <summary>
		/// Retrieves the greatest z-order within the collection.
		/// </summary>
		/// <returns></returns>
		int MaxZOrder { get; }

		/// <summary>
		/// Retrieves the least z-order within the collection.
		/// </summary>
		/// <returns></returns>
		int MinZOrder { get; }

		/// <summary>
		/// Retrieves the top most shape.
		/// </summary>
		Shape TopMost { get; }

		/// <summary>
		/// Retrieves the bottom shape.
		/// </summary>
		Shape Bottom { get; }

		/// <summary>
		/// Enumerates shapes from the highest z-orders to the lowest.
		/// </summary>
		IEnumerable<Shape> TopDown { get; }

		/// <summary>
		/// Enumerates shapes from the lowest z-orders to the highest.
		/// </summary>
		IEnumerable<Shape> BottomUp { get; }

		/// <summary>
		/// Finds a shape within the given rectangle.
		/// </summary>
		Shape FindShape(int x, int y, int width, int height, bool completelyInside, Shape startShape);

		/// <summary>
		/// Finds all shapes within the given rectangle. Note: Some shapes may be returned multiple times.
		/// </summary>
		IEnumerable<Shape> FindShapes(int x, int y, int width, int height, bool completelyInside);

		/// <summary>
		/// Finds a shape, which has control points near a given position.
		/// </summary>
		Shape FindShape(int x, int y, ControlPointCapabilities controlPointCapabilities, int distance, Shape startShape);

		/// <summary>
		/// Finds all shapes, which have control points near the given position. Note: Some shapes may be returned multiple times.
		/// </summary>
		IEnumerable<Shape> FindShapes(int x, int y, ControlPointCapabilities controlPointCapabilities, int distance);

	}


	/// <summary>
	/// A modifyable collection of shapes.
	/// </summary>
	/// <status>Reviewed</status>
	public interface IShapeCollection : IReadOnlyShapeCollection {

		/// <ToBeCompleted></ToBeCompleted>
		void Add(Shape shape);

		/// <ToBeCompleted></ToBeCompleted>
		void Add(Shape shape, int zOrder);

		/// <ToBeCompleted></ToBeCompleted>
		bool Contains(Shape shape);

		/// <ToBeCompleted></ToBeCompleted>
		bool Remove(Shape shape);

		/// <ToBeCompleted></ToBeCompleted>
		void Clear();

		/// <ToBeCompleted></ToBeCompleted>
		void CopyTo(Shape[] array, int arrayIndex);

		/// <ToBeCompleted></ToBeCompleted>
		void AddRange(IEnumerable<Shape> shapes);

		/// <ToBeCompleted></ToBeCompleted>
		void Replace(Shape oldShape, Shape newShape);

		/// <ToBeCompleted></ToBeCompleted>
		void ReplaceRange(IEnumerable<Shape> oldShapes, IEnumerable<Shape> newShapes);

		/// <ToBeCompleted></ToBeCompleted>
		bool RemoveRange(IEnumerable<Shape> shapes);

		/// <ToBeCompleted></ToBeCompleted>
		Rectangle GetBoundingRectangle(bool tight);

		/// <ToBeCompleted></ToBeCompleted>
		bool ContainsAll(IEnumerable<Shape> shapes);

		/// <ToBeCompleted></ToBeCompleted>
		bool ContainsAny(IEnumerable<Shape> shapes);

		/// <summary>
		/// Retrieves the z-order of the given shape.
		/// </summary>
		/// <param name="shape"></param>
		int GetZOrder(Shape shape);
	
		/// <summary>
		/// Assigns a z-order to a shape.
		/// </summary>
		/// <param name="shape"></param>
		/// <param name="zOrder"></param>
		/// <remarks>This method modifies the shape. The caller must ensure that the repository is informed about the modification.</remarks>
		void SetZOrder(Shape shape, int zOrder);
	}


	/// <summary>
	/// Manages a list of shapes.
	/// </summary>
	/// <status>Interface reviewed</status>
	public class ShapeCollection : IShapeCollection, IReadOnlyShapeCollection, ICollection {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ShapeCollection" />.
		/// </summary>
		public ShapeCollection()
			: base() {
			Construct(10);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ShapeCollection" />.
		/// </summary>
		public ShapeCollection(int capacity) {
			Construct(capacity);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ShapeCollection" />.
		/// </summary>
		public ShapeCollection(IEnumerable<Shape> collection) {
			if (collection == null) throw new ArgumentNullException("collection");
			if (collection is ICollection)
				Construct(((ICollection)collection).Count);
			else Construct(0);
			AddRangeCore(collection);
		}


#if DEBUG_UI
		/// <ToBeCompleted></ToBeCompleted>
		~ShapeCollection() {
			if (_occupiedBrush != null) _occupiedBrush.Dispose();
			if (_emptyBrush != null) _emptyBrush.Dispose();
		}
#endif


		#region IShapeCollection Members

		/// <summary>
		/// Adds multiple shapes to the collection.
		/// </summary>
		/// <param name="shapes"></param>
		public void AddRange(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			AddRangeCore(shapes);
		}


		/// <summary>
		/// Replaces a shape by another.
		/// </summary>
		public void Replace(Shape oldShape, Shape newShape) {
			if (oldShape == null) throw new ArgumentNullException("oldShape");
			if (newShape == null) throw new ArgumentNullException("newShape");
			ReplaceCore(oldShape, newShape);
		}


		/// <summary>
		/// Replaces multiple shapes by others.
		/// </summary>
		public void ReplaceRange(IEnumerable<Shape> oldShapes, IEnumerable<Shape> newShapes) {
			if (oldShapes == null) throw new ArgumentNullException("oldShapes");
			if (newShapes == null) throw new ArgumentNullException("newShapes");
			ReplaceRangeCore(oldShapes, newShapes);
		}


		/// <summary>
		/// Removes multiple shapes.
		/// </summary>
		public bool RemoveRange(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shapes");
			return RemoveRangeCore(shapes);
		}


		/// <summary>
		/// Gets the shape with the highest z-order value.
		/// </summary>
		public Shape TopMost {
			get {
				if (Shapes.Count > 0) return Shapes[Shapes.Count - 1];
				else return null;
			}
		}


		/// <summary>
		/// Gets the shape with the lowest z-order value.
		/// </summary>
		public Shape Bottom {
			get {
				if (Shapes.Count > 0) return Shapes[0];
				else return null;
			}
		}


		/// <summary>
		/// Gets all shapes from the topmost to the bottom shape sorted by z-order values.
		/// </summary>
		public IEnumerable<Shape> TopDown {
			get { return ShapeEnumerator.CreateTopDown(Shapes); }
		}


		/// <summary>
		/// Gets all shapes from the bottom to the topmost shape sorted by z-order values.
		/// </summary>
		public IEnumerable<Shape> BottomUp {
			get { return ShapeEnumerator.CreateBottomUp(Shapes); }
		}


		#region ShapeCollection Methods called by the child shapes

		/// <summary>
		/// Notifies the owner that the child shape is going to move.
		/// </summary>
		/// <param name="shape">The shape trying to move</param>
		public virtual void NotifyChildMoving(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			MapRemove(shape);
			ResetBoundingRectangles();
		}


		/// <summary>
		/// Notifies the owner that the child shape is going to resize.
		/// </summary>
		/// <param name="shape">The shape trying to move</param>
		public virtual void NotifyChildResizing(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			MapRemove(shape);
			ResetBoundingRectangles();
		}


		/// <summary>
		/// Notifies the owner that the child shape is going to rotate.
		/// </summary>
		/// <param name="shape">The shape trying to move</param>
		public virtual void NotifyChildRotating(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			MapRemove(shape);
			ResetBoundingRectangles();
		}


		/// <summary>
		/// Notifies the parent that its child shape has moved.
		/// </summary>
		/// <param name="shape"></param>
		public virtual void NotifyChildMoved(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			MapInsert(shape);
		}


		/// <summary>
		/// Notifies the parent that its child shape has resized.
		/// </summary>
		public virtual void NotifyChildResized(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			MapInsert(shape);
		}


		/// <summary>
		/// Notifies the parent that its child shape has rotated.
		/// </summary>
		public virtual void NotifyChildRotated(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			MapInsert(shape);
		}

		#endregion


		/// <override></override>
		public Rectangle GetBoundingRectangle(bool tight) {
			if (tight) {
				if (!Geometry.IsValid(boundingRectangleTight))
					GetBoundingRectangleCore(tight, out boundingRectangleTight);
				return boundingRectangleTight;
			} else {
				if (!Geometry.IsValid(boundingRectangleLoose))
					GetBoundingRectangleCore(tight, out boundingRectangleLoose);
				return boundingRectangleLoose;
			}
		}


		/// <override></override>
		public bool ContainsAll(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shape");
			bool isEmpty = true;
			foreach (Shape shape in shapes) {
				if (isEmpty) isEmpty = false;
				if (!Contains(shape)) return false;
			}
			return !isEmpty;
		}


		/// <override></override>
		public bool ContainsAny(IEnumerable<Shape> shapes) {
			if (shapes == null) throw new ArgumentNullException("shape");
			bool result = false;
			foreach (Shape shape in shapes) {
				if (Contains(shape)) {
					result = true;
					break;
				}
			}
			return result;
		}


		/// <override></override>
		public Shape FindShape(int x, int y, ControlPointCapabilities controlPointCapabilities, int range, Shape startShape) {
			return GetFirstShapeInArea(x - range, y - range, 2 * range, 2 * range, controlPointCapabilities, startShape, SearchMode.Near);
		}


		/// <override></override>
		public Shape FindShape(int x, int y, int width, int height, bool completelyInside, Shape startShape) {
			return GetFirstShapeInArea(x, y, width, height, ControlPointCapabilities.None,
				startShape, completelyInside ? SearchMode.Contained : SearchMode.Near);
		}


		/// <summary>
		/// Finds shapes that contain (x, y) or own a control point within the range from (x, y).
		/// </summary>
		/// <returns>Shapes founds. Shapes may occur more than once.</returns>
		public IEnumerable<Shape> FindShapes(int x, int y, ControlPointCapabilities capabilities, int range) {
			foreach (Shape s in GetShapesInArea(x - range, y - range, 2 * range,
				2 * range, capabilities, SearchMode.Near))
				yield return s;
		}


		/// <summary>
		/// Finds shapes that intersect with or are contained within the given rectangle.
		/// </summary>
		/// <returns>Shapes found. Shapes may occur more than once.</returns>
		public IEnumerable<Shape> FindShapes(int x, int y, int width, int height, bool completelyInside) {
			foreach (Shape s in GetShapesInArea(x, y, width, height, ControlPointCapabilities.None,
				completelyInside ? SearchMode.Contained : SearchMode.Intersects))
				yield return s;
		}


		/// <override></override>
		public int MinZOrder {
			get {
				if (Shapes.Count <= 0) return 0;
				else return Bottom.ZOrder;
			}
		}


		/// <override></override>
		public int MaxZOrder {
			get {
				if (Shapes.Count <= 0) return 0;
				else return TopMost.ZOrder;
			}
		}


		/// <override></override>
		public int GetZOrder(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			return shape.ZOrder;
		}


		/// <override></override>
		public void SetZOrder(Shape shape, int zOrder) {
			if (shape == null) throw new ArgumentNullException("shape");
			Remove(shape);
			shape.ZOrder = zOrder;
			Add(shape);
		}

		#endregion


		#region ICollection<Shape> Members

		/// <override></override>
		public void Add(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			AddCore(shape);
			// Reset bounding rectangles
			boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
		}


		/// <override></override>
		public void Add(Shape shape, int zOrder) {
			if (shape == null) throw new ArgumentNullException("shape");
			shape.ZOrder = zOrder;
			Add(shape);
		}


		/// <override></override>
		public void Clear() {
			ClearCore();
		}


		/// <override></override>
		public bool Contains(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			return _shapeSet.Contains(shape);
		}


		/// <override></override>
		public void CopyTo(Shape[] array, int arrayIndex) {
			if (array == null) throw new ArgumentNullException("array");
			Shapes.CopyTo(array, arrayIndex);
		}


		/// <override></override>
		public int Count {
			get { return Shapes.Count; }
		}


		/// <override></override>
		public bool IsReadOnly {
			get { return false; }
		}


		/// <override></override>
		public bool Remove(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			return RemoveCore(shape);
		}

		#endregion


		#region IEnumerable<Shape> Members

		/// <override></override>
		public IEnumerator<Shape> GetEnumerator() {
			return ShapeEnumerator.CreateTopDown(Shapes);
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return ShapeEnumerator.CreateTopDown(Shapes);
		}

		#endregion


		#region ICollection Members

		/// <override></override>
		public void CopyTo(Array array, int index) {
			if (array == null) throw new ArgumentNullException("array");
			Array.Copy(Shapes.ToArray(), index, array, 0, array.Length);
		}


		int ICollection.Count {
			get { return Count; }
		}


		/// <override></override>
		public bool IsSynchronized {
			get { return false; }
		}


		/// <override></override>
		public object SyncRoot {
			get {
				if (_syncRoot == null)
					Interlocked.CompareExchange(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		#endregion


		/// <summary>
		/// Clones all shapes (including model objects) in the shapeCollection. Connections between shapes in the shape collection will be maintained.
		/// </summary>
		public ShapeCollection Clone() {
			return Clone(true);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ShapeCollection Clone(bool withModelObjects) {
#if DEBUG_DIAGNOSTICS
			Stopwatch w = new Stopwatch();
			w.Start();
#endif
			ShapeCollection result = new ShapeCollection(Count);

			Dictionary<Shape, Shape> shapeDict = new Dictionary<Shape, Shape>(Shapes.Count);
			Dictionary<Shape, List<ShapeConnectionInfo>> connections = new Dictionary<Shape, List<ShapeConnectionInfo>>();
			// clone from last to first shape in order to maintain the ZOrder
			foreach (Shape shape in BottomUp) {
				// Clone shape (and model)
				Shape clone = null;
				if (withModelObjects)
					clone = ShapeDuplicator.CloneShapeAndModelObject(shape);
				else clone = ShapeDuplicator.CloneShapeOnly(shape);
				//Shape clone = shape.Clone();
				//if (withModelObjects && clone.ModelObject != null)
				//   clone.ModelObject = shape.ModelObject.Clone();

				// Register original shape and clone in the dictionary for fast searching 
				// when restoring connections later
				shapeDict.Add(shape, clone);
				foreach (ShapeConnectionInfo ci in shape.GetConnectionInfos(ControlPointId.Any, null)) {
					if (shape.HasControlPointCapability(ci.OwnPointId, ControlPointCapabilities.Glue)) {
						if (!connections.ContainsKey(shape))
							connections.Add(shape, new List<ShapeConnectionInfo>(2));	// typically, a line has 2 connections
						connections[shape].Add(ci);
					}
				}
				result.Add(clone);
			}

			// Restore connections between copied shapes
			foreach (KeyValuePair<Shape, List<ShapeConnectionInfo>> item in connections) {
				foreach (ShapeConnectionInfo connInfo in item.Value) {
					// If the passive shape was among the copied shapes, restore the connection
					if (!shapeDict.ContainsKey(connInfo.OtherShape)) continue;
					Shape activeShape = shapeDict[item.Key];
					Shape passiveShape = shapeDict[connInfo.OtherShape];
					Debug.Assert(result.Contains(passiveShape));
					activeShape.Connect(connInfo.OwnPointId, passiveShape, connInfo.OtherPointId);
				}
			}
			shapeDict.Clear();
			connections.Clear();
#if DEBUG_DIAGNOSTICS
			w.Stop();
			Console.WriteLine("Cloning ShapeCollection with {0} elements: {1}", result.Count, w.Elapsed);
#endif
			return result;
		}

		/// <summary>List of shapes in collection.</summary>
		protected ReadOnlyList<Shape> Shapes {
			get { return _shapes; }
			set { _shapes = value; }
		}


		#region [Protected] Methods

		/// <summary>
		/// Adds the given shape to the collection.
		/// </summary>
		/// <param name="shape"></param>
		protected virtual int AddCore(Shape shape) {
			if (_shapeSet.Contains(shape)) throw new ArgumentException("The shape item already exists in the collection.");
			int idx = FindInsertPosition(shape.ZOrder);
			return InsertCore(idx, shape);
		}


		/// <summary>
		/// Adds the given shape to the collection. 
		/// The shape is inserted into the collection at the correct position, depending on the z-order.
		/// </summary>
		/// <returns>Index where the shape has been inserted. The shape indexes can vary.</returns>
		protected virtual int InsertCore(int index, Shape shape) {
			if (_shapeSet.Contains(shape)) throw new ArgumentException("The given shape is already part of the collection.");
			int result = -1;
			if (Shapes.Count == 0 || index == Shapes.Count || shape.ZOrder >= this.TopMost.ZOrder) {
				if (Shapes.Count > 0) { Debug.Assert(shape.ZOrder >= Shapes[Shapes.Count - 1].ZOrder); }

				// append shape
				Shapes.Add(shape);
				result = Shapes.Count - 1;
			} else if (shape.ZOrder < Shapes[0].ZOrder) {
				Debug.Assert(shape.ZOrder <= Shapes[0].ZOrder);

				Shapes.Insert(0, shape);
				result = index;
			} else {
				// prepend shape
				if (index > 0) Debug.Assert(Shapes[index - 1].ZOrder <= shape.ZOrder);
				Debug.Assert(shape.ZOrder <= Shapes[index].ZOrder);

				Shapes.Insert(index, shape);
				result = index;
			}
			AddShapeToIndex(shape);
			return result;
		}


		/// <summary>
		/// Inserts the elements of the collection into the ShapeCollection. 
		/// This method inserts each element into the correct position, sorted by z-order.
		/// </summary>
		protected virtual void AddRangeCore(IEnumerable<Shape> collection) {
			int lastInsertPos = -1;
			foreach (Shape shape in collection) {
				// Check if the next shape can be inserted at the last position
				// This is the case if the shapes are pre-sorted by ZOrder (upper shapes first)
				if (IndexIsValid(lastInsertPos, shape)) {
					if (Shapes[Shapes.Count - 1].ZOrder <= shape.ZOrder)
						lastInsertPos = InsertCore(Shapes.Count, shape);
					else {
						AssertIndexIsValid(lastInsertPos, shape);
						InsertCore(lastInsertPos, shape);
					}
				} else {
					int index = FindInsertPosition(shape.ZOrder);
					lastInsertPos = InsertCore(index, shape);
				}
			}
			// Reset BoundingRectangle
			boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
		}


		/// <summary>
		/// Replaces the old shape with the new shape.
		/// </summary>
		protected virtual void ReplaceCore(Shape oldShape, Shape newShape) {
			if (_shapeSet.Contains(newShape)) throw new InvalidOperationException("The value to be inserted does already exist in the collection.");
			if (!_shapeSet.Contains(oldShape)) throw new InvalidOperationException("The value to be replaced does not exist in the colection.");
			int idx = FindShapeIndex(oldShape);
			if (idx < 0) throw new InvalidOperationException("The given shape does not exist in the collection.");
			// Copy DiagramShape properties
			newShape.ZOrder = oldShape.ZOrder;
			newShape.SupplementalLayers = oldShape.SupplementalLayers;
			// Remove old shape from search dictionary and spacial index
			RemoveShapeFromIndex(oldShape);
			// Replace oldShape with newShape
			Shapes[idx] = newShape;
			// Add newShape in search dictionary and spacial index
			AddShapeToIndex(newShape);
			// Reset BoundingRectangle
			boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void ReplaceRangeCore(IEnumerable<Shape> oldShapes, IEnumerable<Shape> newShapes) {
			IList<Shape> oldShapesList, newShapesList;
			if (oldShapes is IList<Shape>) oldShapesList = (IList<Shape>)oldShapes;
			else oldShapesList = new List<Shape>(oldShapes);
			if (newShapes is IList<Shape>) newShapesList = (IList<Shape>)newShapes;
			else newShapesList = new List<Shape>(newShapes);

			if (oldShapesList.Count != newShapesList.Count) throw new NShapeInternalException("Numer of elements in the given collections differ from each other.");
			for (int i = 0; i < oldShapesList.Count; ++i)
				ReplaceCore(oldShapesList[i], newShapesList[i]);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual bool RemoveCore(Shape shape) {
			if (!_shapeSet.Contains(shape)) return false;
			int idx = FindShapeIndex(shape);
			Debug.Assert(idx >= 0);
			if (idx >= 0) {
				Shapes.RemoveAt(idx);
				RemoveShapeFromIndex(shape);
			}
			// Reset BoundingRectangle
			boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
			return true;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual bool RemoveRangeCore(IEnumerable<Shape> shapes) {
			bool result = true;

			// Convert IEnumerable<Shapes> to a list type providing an indexer
			IList<Shape> shapeList;
			if (shapes is IList<Shape>)
				shapeList = (IList<Shape>)shapes;
			else shapeList = new List<Shape>(shapes);

			// Remove shapes
			foreach (Shape shape in shapeList) {
				if (!RemoveCore(shape))
					result = false;
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected virtual void ClearCore() {
			Shapes.Clear();
			_shapeSet.Clear();
			if (_shapeMap != null) _shapeMap.Clear();
			// Reset BoundingRectangle
			boundingRectangleTight = boundingRectangleLoose = Geometry.InvalidRectangle;
		}


		/// <summary>
		/// Adds the given shape into the collection's indexes.
		/// </summary>
		protected void AddShapeToIndex(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			_shapeSet.Add(shape);
			MapInsert(shape);
		}


		/// <summary>
		/// removes the given shape from the collection's indexes.
		/// </summary>
		protected void RemoveShapeFromIndex(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			MapRemove(shape);
			_shapeSet.Remove(shape);
		}

		#endregion


		internal void SetDisplayService(IDisplayService displayService) {
			// set displayService for all shapes - the shapes set their children's DisplayService themselfs
			foreach (Shape shape in Shapes)
				shape.DisplayService = displayService;
		}


		#region [Private] Methods

		private void Construct(int capacity) {
			if (capacity > 0) {
				Shapes = new ReadOnlyList<Shape>(capacity);
				_shapeSet = new HashCollection<Shape>(capacity);
			} else {
				Shapes = new ReadOnlyList<Shape>();
				_shapeSet = new HashCollection<Shape>();
			}
			if (capacity >= 1000) _shapeMap = new MultiHashList<Shape>(capacity);
		}


		private uint CalcMapHashCode(Point cellIndex) {
			unchecked {
				uint x = (uint)(cellIndex.X);
				uint y = (uint)(cellIndex.Y);
				y = (y & 0x000000ffu) << 24 | (y & 0x0000ff00u) << 8 | (y & 0x00ff0000u) >> 8 | (y & 0xff000000u) >> 24;
				return x ^ y;
			}
		}


		/// <summary>
		/// Inserts the given shape into the shape map (spatial index).
		/// </summary>
		private void MapInsert(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (_shapeMap != null) {
				//Debug.Assert(CellsAreValid(shape));
				foreach (Point p in shape.CalculateCells(Diagram.CellSize)) {
					_shapeMap.Add(CalcMapHashCode(p), shape);
				}
			}
		}


		/// <summary>
		/// Removes the given shape into the shape map (spatial index).
		/// </summary>
		private void MapRemove(Shape shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			if (_shapeMap != null) {
				//Debug.Assert(CellsAreValid(shape));
				foreach (Point p in shape.CalculateCells(Diagram.CellSize))
					_shapeMap.Remove(CalcMapHashCode(p), shape);
			}
		}


#if DEBUG_UI
		/// <ToBeCompleted></ToBeCompleted>
		public void DrawOccupiedCells(Graphics graphics, int x, int y, int width, int height) {
			int maxListLength = 0;
			int itemCnt = 0;
			int listCnt = 0;

			Point p = Point.Empty;
			int minCellX = x / Diagram.CellSize;
			if (x < 0) --minCellX;
			int minCellY = y / Diagram.CellSize;
			if (y < 0) --minCellY;
			int maxCellX = width / Diagram.CellSize;
			int maxCellY = height / Diagram.CellSize;
			for (p.X = minCellX; p.X <= maxCellX; ++p.X) {
				for (p.Y = minCellY; p.Y <= maxCellY; ++p.Y) {
					int listLength = 0;
					foreach (Shape s in _shapeMap[CalcMapHashCode(p)]) {
						s.Invalidate();

						int left = p.X * Diagram.CellSize;
						int top = p.Y * Diagram.CellSize;
						if (s.IntersectsWith(left, top, Diagram.CellSize, Diagram.CellSize)
						   || Geometry.RectangleContainsRectangle(left, top, Diagram.CellSize, Diagram.CellSize, s.GetBoundingRectangle(false))) {
						   graphics.FillRectangle(_occupiedBrush, left, top, Diagram.CellSize, Diagram.CellSize);
						} else graphics.FillRectangle(_emptyBrush, left, top, Diagram.CellSize, Diagram.CellSize);
						++listLength;
					}
					// Collect list statistics: Maximum list length and average list 
					// length (populated lists only)
					if (listLength > 0) {
						if (listLength > maxListLength) maxListLength = listLength;
						itemCnt += listLength;
						++listCnt;
					}
				}
			}
			int avgListLength = (listCnt > 0) ? (int)Math.Round(itemCnt / (float)listCnt, MidpointRounding.AwayFromZero) : 0;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int GetShapeCount(int x, int y) {
			Point p = Point.Empty;
			p.Offset(x / Diagram.CellSize, y / Diagram.CellSize);
			return GetShapeCount(p);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int GetShapeCount(Point cellIndex) {
			int result = 0;
			foreach (Shape s in _shapeMap[CalcMapHashCode(cellIndex)])
				++result;
			return result;
		}
#endif


		private bool CellsAreValid(Shape shape) {
			int left = int.MaxValue, top = int.MaxValue;
			int right = int.MinValue, bottom = int.MinValue;
			foreach (Point p in shape.CalculateCells(Diagram.CellSize)) {
				if (p.X < left) left = p.X;
				if (p.X > right) right = p.X;
				if (p.Y < top) top = p.Y;
				if (p.Y > bottom) bottom = p.Y;
			}
			// If there are no cells at all, this is considered as "valid".
			if (left == int.MaxValue && top == int.MaxValue && right == int.MinValue && bottom == int.MinValue)
				return true;

			Rectangle cellBounds = Rectangle.FromLTRB(left * Diagram.CellSize, top * Diagram.CellSize,
				(right * Diagram.CellSize) + Diagram.CellSize, (bottom * Diagram.CellSize) + Diagram.CellSize);
			Rectangle shapeBounds = shape.GetBoundingRectangle(true);

			if (!Geometry.RectangleContainsRectangle(cellBounds, shapeBounds))
				return false;
			int doubleCellSize = Diagram.CellSize * 2;
			if (Math.Abs(cellBounds.Left - shapeBounds.Left) >= doubleCellSize
				|| Math.Abs(cellBounds.Top - shapeBounds.Top) >= doubleCellSize
				|| Math.Abs(shapeBounds.Right - cellBounds.Right) >= doubleCellSize
				|| Math.Abs(shapeBounds.Bottom - cellBounds.Bottom) >= doubleCellSize)
				return false;
			return true;
		}


		private void ReindexShape(Shape shape) {
			MapRemove(shape);
			MapInsert(shape);
		}


		private void AssertIndexIsValid(int index, Shape shape) {
			if (Count > 0) {
				if (index > Count)
					throw new NShapeInternalException("Index {0} is out of range: The collection contains only {1} element.", index, Count);
				if (index > 0 && index <= Count && Shapes[index - 1].ZOrder > shape.ZOrder)
					throw new NShapeInternalException("ZOrder value {0} cannot be inserted after ZOrder value {1}.", shape.ZOrder, Shapes[index - 1].ZOrder);
				if (index < Count && Shapes[index].ZOrder < shape.ZOrder)
					throw new NShapeInternalException("ZOrder value {0} cannot be inserted before ZOrder value {1}.", shape.ZOrder, Shapes[index].ZOrder);
			}
		}


		// Tests whether the shape can be inserted at the given index without violating
		// the z-order sorting.
		private bool IndexIsValid(int index, Shape shape) {
			if (index >= 0 && index <= Shapes.Count) {
				// Insert into an empty list
				if (Shapes.Count == 0)
					return (index == 0);
				// Insert elsewhere (placed here because it is the most likely case)
				else if (index > 0 && index < Shapes.Count)
					return (Shapes[index].ZOrder >= shape.ZOrder && Shapes[index - 1].ZOrder <= shape.ZOrder);
				// Prepend
				else if (index == 0)
					return (Shapes[0].ZOrder >= shape.ZOrder);
				// Append
				else if (index == Shapes.Count)
					return (Shapes[index - 1].ZOrder <= shape.ZOrder);
			}
			return false;
		}


		private bool IsShapeInRange(Shape shape, int x, int y, int distance, ControlPointCapabilities controlPointCapabilities) {
			// if no ControlPoints are needed, we use functions that ignore ControlPoint positions...
			if (controlPointCapabilities == ControlPointCapabilities.None) {
				if (distance == 0) return shape.ContainsPoint(x, y);
				else return shape.IntersectsWith(x - distance, y - distance, distance + distance, distance + distance);
			} else {
				// ...otherwise we use ContainsPoint which also checks the ControlPoints
				return shape.HitTest(x, y, controlPointCapabilities, distance) != ControlPointId.None;
			}
		}


		private IEnumerable<Shape> GetShapesInArea(int x, int y, int w, int h, ControlPointCapabilities capabilities, SearchMode mode) {
			bool tightBounds = (capabilities == ControlPointCapabilities.None);
			int range = w / 2;
			if (_shapeMap != null) {
				int fromX, fromY, toX, toY;
				ShapeUtils.CalcCell(x, y, Diagram.CellSize, out fromX, out fromY);
				ShapeUtils.CalcCell(x + w, y + h, Diagram.CellSize, out toX, out toY);
				Point p = Point.Empty;
				for (p.X = fromX; p.X <= toX; p.X += 1)
					for (p.Y = fromY; p.Y <= toY; p.Y += 1)
						foreach (Shape s in _shapeMap[CalcMapHashCode(p)])
							if (mode == SearchMode.Contained && Geometry.RectangleContainsRectangle(x, y, w, h, s.GetBoundingRectangle(tightBounds))
								|| mode == SearchMode.Intersects && s.IntersectsWith(x, y, w, h)
								|| mode == SearchMode.Near && IsShapeInRange(s, x + range, y + range, range, capabilities))
								yield return s;

			} else {
				for (int i = Shapes.Count - 1; i >= 0; --i) {
					if (mode == SearchMode.Contained && Geometry.RectangleContainsRectangle(x, y, w, h, Shapes[i].GetBoundingRectangle(tightBounds))
						|| mode == SearchMode.Intersects && Shapes[i].IntersectsWith(x, y, w, h)
						|| mode == SearchMode.Near && IsShapeInRange(Shapes[i], x + range, y + range, range, capabilities))
						yield return Shapes[i];
				}
			}
		}


		private Shape GetFirstShapeInArea(int x, int y, int w, int h, ControlPointCapabilities capabilities,
			Shape startShape, SearchMode mode) {
			Shape result = null;
			if (_shapeMap != null) {
				int startZOrder = startShape == null ? int.MaxValue : startShape.ZOrder;
				bool skipChildren = (startShape != null && startShape.Parent == null);
				int maxZOrder = int.MinValue;
				foreach (Shape s in GetShapesInArea(x, y, w, h, capabilities, mode)) {
					if (skipChildren && s.Parent != null) continue;
					if (s.ZOrder <= startZOrder && s.ZOrder > maxZOrder && s != startShape) {
						maxZOrder = s.ZOrder;
						result = s;
					}
				}
			} else {
				bool startShapeFound = (startShape == null);
				foreach (Shape s in GetShapesInArea(x, y, w, h, capabilities, mode)) {
					if (startShapeFound) {
						result = s;
						break;
					} else if (s == startShape) startShapeFound = true;
				}
			}
			if (result == null && startShape != null)
				result = GetFirstShapeInArea(x, y, w, h, capabilities, null, mode);
			return result;
		}


		private int FindShapeIndex(Shape shape) {
			if (_shapeSet.Contains(shape)) {
				int shapeCnt = Shapes.Count;
				int zOrder = shape.ZOrder;
				// Search with binary search style...
				int sPos = 0;
				int ePos = shapeCnt;
				while (ePos > sPos + 1) {
					int searchIdx = (ePos + sPos) / 2;
					// Find position where prevZOrder < zOrder < nextZOrder
					// If the zorder is equal, we approach the last shape with an equal zOrder.
					// If the correct shape is found while searching, return its position
					if (Shapes[searchIdx] == shape)
						return searchIdx;
					if (Shapes[searchIdx].ZOrder == zOrder) {
						// If there are shapes with identical zOrders, search them all
						int i = searchIdx;
						while (i < ePos && Shapes[i].ZOrder == zOrder) {
							if (Shapes[i] == shape)
								return i;
							else ++i;
						}
						i = searchIdx;
						while (i >= sPos && Shapes[i].ZOrder == zOrder) {
							if (Shapes[i] == shape)
								return i;
							else --i;
						}
					} else if (Shapes[searchIdx].ZOrder > zOrder)
						sPos = searchIdx;
					else ePos = searchIdx;
				}
				// If the shape was not found, there are duplicate ZOrders 
				// which have to be searched sequently
				int foundIndex = sPos;
				if (Shapes[foundIndex].ZOrder < zOrder) {
					for (int i = foundIndex; i < shapeCnt; ++i)
						if (Shapes[i] == shape) 
							return i;
				} else {
					for (int i = foundIndex; i >= 0; --i)
						if (Shapes[i] == shape) 
							return i;
				}
				Debug.Fail("The shape was not found although it exists in the list!");
			}
			return -1;
		}


		private int FindInsertPosition(int zOrder) {
			// zOrder is lower than the lowest zOrder in the list
			int shapeCnt = Shapes.Count;
			if (shapeCnt == 0 || zOrder < Shapes[0].ZOrder)
				return 0;

			// zOrder is higher than the highest ZOrder in the list
			else if (zOrder >= Shapes[shapeCnt - 1].ZOrder)
				return shapeCnt;

			// search correct position 
			else {
				int sPos = 0;
				int ePos = shapeCnt;
				while (ePos > sPos + 1) {
					int searchIdx = (ePos + sPos) / 2;
					// Find position where prevZOrder < zOrder < nextZOrder
					// If the zorder is equal, we approach the last shape with an equal zOrder.
					if (Shapes[searchIdx].ZOrder <= zOrder)
						sPos = searchIdx;
					else ePos = searchIdx;
				}
				int foundIndex = sPos;
				if (foundIndex >= shapeCnt)
					return shapeCnt;

				if (Shapes[foundIndex].ZOrder <= zOrder) {
					for (int i = foundIndex; i < shapeCnt; ++i) {
						if (Shapes[i].ZOrder > zOrder)
							return i;
					}
					return shapeCnt;
				} else {
					for (int i = foundIndex; i >= 0; --i) {
						if (Shapes[i].ZOrder < zOrder)
							return i + 1;
					}
					return 0;
				}
			}
		}


		private void ResetBoundingRectangles() {
			if (Geometry.IsValid(boundingRectangleTight))
				boundingRectangleTight = Geometry.InvalidRectangle;
			if (Geometry.IsValid(boundingRectangleLoose))
				boundingRectangleLoose = Geometry.InvalidRectangle;
		}


		private void GetBoundingRectangleCore(bool tight, out Rectangle boundingRectangle) {
			// return an empty rectangle if the collection has no children
			boundingRectangle = Geometry.InvalidRectangle;
			if (Shapes.Count > 0) {
				int left, right, top, bottom;
				left = top = int.MaxValue;
				right = bottom = int.MinValue;
				foreach (Shape shape in Shapes) {
					Rectangle r = shape.GetBoundingRectangle(tight);
					Debug.Assert(Geometry.IsValid(r));
					if (left > r.Left) left = r.Left;
					if (top > r.Top) top = r.Top;
					if (right < r.Right) right = r.Right;
					if (bottom < r.Bottom) bottom = r.Bottom;
				}
				if (left != int.MaxValue && top != int.MaxValue && right != int.MinValue && bottom != int.MinValue) {
					boundingRectangle.Offset(left, top);
					boundingRectangle.Width = right - left;
					boundingRectangle.Height = bottom - top;
				}
			}
		}

		#endregion


		#region [Private] Types

		/// <summary>
		/// Enumerates the elements of a shape collection.
		/// </summary>
		/// <status>reviewed</status>
		private struct ShapeEnumerator : IEnumerable<Shape>, IEnumerator<Shape>, IEnumerator {

			public static ShapeEnumerator CreateBottomUp(ReadOnlyList<Shape> shapeList) {
				ShapeEnumerator result;
				result._shapeList = shapeList;
				result._count = shapeList.Count;
				result._startIdx = result._currIdx = -1;
				result._step = 1;
				return result;
			}


			public static ShapeEnumerator CreateTopDown(ReadOnlyList<Shape> shapeList) {
				ShapeEnumerator result;
				result._shapeList = shapeList;
				result._count = shapeList.Count;
				result._startIdx = result._currIdx = shapeList.Count;
				result._step = -1;
				return result;
			}


			#region IEnumerable<Shape> Members

			public IEnumerator<Shape> GetEnumerator() { return this; }

			#endregion


			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator() { return this; }

			#endregion


			#region IEnumerator<Shape> Members

			public Shape Current {
				get {
					if (_currIdx >= 0 && _currIdx < _count)
						return _shapeList[_currIdx];
					else return null;
				}
			}

			#endregion


			#region IEnumerator Members

			object IEnumerator.Current {
				get { return Current; }
			}


			public bool MoveNext() {
				_currIdx += _step;
				return (_currIdx >= 0 && _currIdx < _count);
			}


			public void Reset() {
				_currIdx = _startIdx;
			}

			#endregion


			#region IDisposable Members

			public void Dispose() {
				_shapeList = null;
			}

			#endregion


			static ShapeEnumerator() {
				Empty._shapeList = null;
				Empty._count = 0;
				Empty._startIdx = -1;
				Empty._step = 1;
			}


			#region Fields

			public static readonly ShapeEnumerator Empty;

			private sbyte _step;
			private int _currIdx;
			private int _startIdx;
			private int _count;
			private ReadOnlyList<Shape> _shapeList;

			#endregion
		}


		private enum SearchMode { Contained, Intersects, Near };

		#endregion


		#region Fields


		/// <summary>
		/// The last calculated X-axis aligned bounding rectangle not including control points.
		/// </summary>
		/// <remarks>This is a protected field and not a property becaus properties cannot be passed as out or ref parameters.</remarks>
		protected Rectangle boundingRectangleTight;


		/// <summary>
		/// The last calculated X-axis aligned bounding rectangle including control points.
		/// </summary>
		/// <remarks>This is a protected field and not a property becaus properties cannot be passed as out or ref parameters.</remarks>
		protected Rectangle boundingRectangleLoose;


		/// <summary>List of shapes in collection.</summary>
		private ReadOnlyList<Shape> _shapes;

		// HashCollection of contained shapes used for fast searching
		private HashCollection<Shape> _shapeSet;
		// Hashtable to quickly find shapes given a coordinate.
		private MultiHashList<Shape> _shapeMap = null;

		private object _syncRoot = null;

		#endregion

#if DEBUG_UI
		private SolidBrush _occupiedBrush = new SolidBrush(Color.FromArgb(32, Color.Green));
		private SolidBrush _emptyBrush = new SolidBrush(Color.FromArgb(32, Color.Red));
#endif
	}

}

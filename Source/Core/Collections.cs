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
using System.Diagnostics;
using System.Threading;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// A generic readonly collection of items providing an enumerator and the total number of items in the collection
	/// </summary>
	public interface IReadOnlyCollection<T> : IEnumerable<T>, ICollection { }


	/// <summary>
	/// A list based class implementing the IReadOnlyCollection interface
	/// </summary>
	public class ReadOnlyList<T> : List<T>, IReadOnlyCollection<T> {
		
		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ReadOnlyList`1" />.
		/// </summary>
		public ReadOnlyList()
			: base() {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ReadOnlyList`1" />.
		/// </summary>
		public ReadOnlyList(int capacity)
			: base(capacity) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.ReadOnlyList`1" />.
		/// </summary>
		public ReadOnlyList(IEnumerable<T> collection)
			: base(collection) {
		}

	}


	/// <summary>
	/// A generic readonly collection for fast searching.
	/// </summary>
	public interface IReadOnlyHashCollection<T> : IReadOnlyCollection<T> {

		/// <summary>
		/// Tests whether all of the given items are in the collection.
		/// </summary>
		bool ContainsAll(IEnumerable<T> items);

		/// <summary>
		/// Tests whether at least one of the given items is in the collection.
		/// </summary>
		bool ContainsAny(IEnumerable<T> items);

	}
	
	
	/// <summary>
	/// Represents a set of values. 
	/// Provides a fast Contains() implementation.
	/// </summary>
	/// <typeparam name="T">The type of elements in the hash collection.</typeparam>
	public class HashCollection<T> : IReadOnlyHashCollection<T>, ICollection<T> {

		/// <summary>Constructor</summary>
		public HashCollection() {
			_innerDictionary = new Dictionary<T, byte>();
		}


		/// <summary>Creates a new <see cref="T:Dataweb.NShape.Advanced"/></summary>
		public HashCollection(int capacity) {
			_innerDictionary = new Dictionary<T, byte>(capacity);
		}


		/// <summary>Creates a new <see cref="T:Dataweb.NShape.Advanced"/></summary>
		public HashCollection(IEnumerable<T> items) {
			if (items is ICollection)
				_innerDictionary = new Dictionary<T, byte>(((ICollection)items).Count);
			else
				_innerDictionary = new Dictionary<T, byte>();
			foreach (T item in items)
				_innerDictionary.Add(item, 0);
		}


		/// <override></override>
		public void Add(T item) {
			_innerDictionary.Add(item, 0);
		}


		/// <summary>
		/// Adds all given items to the collection.
		/// If some of the given items are already part of the collection, they will be ignored.
		/// </summary>
		public void Add(IEnumerable<T> items) {
			foreach (T item in items)
				if (!Contains(item))
					Add(item);
		}


		/// <override></override>
		public void Clear() {
			_innerDictionary.Clear();
		}


		/// <override></override>
		public bool Contains(T item) {
			return _innerDictionary.ContainsKey(item);
		}


		/// <override></override>
		public bool ContainsAll(IEnumerable<T> items) {			
			foreach (T item in items)
				if (!Contains(item))
					return false;
			return true;
		}


		/// <override></override>
		public bool ContainsAny(IEnumerable<T> items) {
			foreach (T item in items)
				if (Contains(item))
					return true;
			return false;
		}


		/// <override></override>
		public void CopyTo(T[] array, int arrayIndex) {
			_innerDictionary.Keys.CopyTo(array, arrayIndex);
		}


		/// <override></override>
		public void CopyTo(Array array, int index) {
			CopyTo(array, index);
		}


		/// <override></override>
		public int Count {
			get { return _innerDictionary.Keys.Count; }
		}


		/// <override></override>
		public bool IsReadOnly {
			get { return false; }
		}


		/// <override></override>
		public bool Remove(T item) {
			return _innerDictionary.Remove(item);
		}


		/// <summary>
		/// Tries to remove all of the given items. 
		/// Returns true if at least one item was removed.
		/// </summary>
		public bool Remove(IEnumerable<T> items) {
			bool result = false;
			foreach (T item in items)
				if (_innerDictionary.Remove(item))
					result = true;
			return result;
		}


		/// <override></override>
		public IEnumerator<T> GetEnumerator() {
			return _innerDictionary.Keys.GetEnumerator();
		}


		/// <override></override>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return GetEnumerator();
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

		
		// Internally, we use a Dectionary<T, byte> because the value is irrelevant 
		// and byte is one of the smallest data types (bool requires 4 bytes!).
		private Dictionary<T, byte> _innerDictionary;
		private object _syncRoot = null;

	}


	/// <summary>
	/// Defines methods for an editable collection of layers.
	/// </summary>
	/// <status>reviewed</status>
	public interface ILayerCollection : ICollection<Layer> {

		/// <summary>
		/// Retrieve the <see cref="T:Dataweb.NShape.Layer" /> instance associated with the given <see cref="T:Dataweb.NShape.LayerIds" />.
		/// </summary>
		Layer this[LayerIds layerId] { get; }

		/// <summary>
		/// Retrieve the <see cref="T:Dataweb.NShape.Layer" /> instance associated with the given <see cref="T:Dataweb.NShape.LayerIds" />.
		/// </summary>
		Layer this[int layerId] { get; }

		/// <summary>
		/// Retrieve the <see cref="T:Dataweb.NShape.Layer" /> instance with the given name.
		/// </summary>
		Layer this[string name] { get; }
		
		/// <summary>
		/// Retrieve the <see cref="T:Dataweb.NShape.Layer" /> instance associated with the given <see cref="T:Dataweb.NShape.LayerIds" />.
		/// </summary>
		[Obsolete("Use method FindLayer instead.")]
		Layer GetLayer(LayerIds layerId);

		/// <summary>
		/// Retrieve the <see cref="T:Dataweb.NShape.Layer" /> instance associated with the given <see cref="T:Dataweb.NShape.LayerIds" />.
		/// Returns null if no such layer exists.
		/// </summary>
		Layer FindLayer(LayerIds layerId);

		/// <summary>
		/// Retrieve the <see cref="T:Dataweb.NShape.Layer" /> instance associated with the given <see cref="T:Dataweb.NShape.LayerIds" />.
		/// Returns null if no such layer exists.
		/// </summary>
		Layer FindLayer(int layerId);

		/// <summary>
		/// Retrieve the <see cref="T:Dataweb.NShape.Layer" /> instance with the given name.
		/// Returns null if no layer has such a name.
		/// </summary>
		Layer FindLayer(string name);

		/// <summary>
		/// Retrieve all <see cref="T:Dataweb.NShape.Layer" /> instances associated with the given combination of <see cref="T:Dataweb.NShape.LayerIds" />.
		/// Returns null if no such layer exists.
		/// </summary>
		IEnumerable<Layer> FindLayers(LayerIds layers);

		/// <summary>
		/// Retrieve all <see cref="T:Dataweb.NShape.Layer" /> instances associated with the given combination of <see cref="T:Dataweb.NShape.LayerIds" />.
		/// </summary>
		IEnumerable<Layer> FindLayers(IEnumerable<int> layerIds);

		/// <summary>
		/// Rename the specified <see cref="T:Dataweb.NShape.Layer" />.
		/// </summary>
		bool RenameLayer(string previousName, string newName);

		/// <summary>
		/// Tests whether a layer with the given id is part of the collection.
		/// </summary>
		bool Contains(int layerId);

		/// <summary>
		/// Tests whether a layer with the given name is part of the collection.
		/// </summary>
		bool Contains(string layerName);

		/// <summary>
		/// Returns the first unused layer id.
		/// If a layer id of the demanded type is not available, LayerId.NoLayerId will be returned.
		/// </summary>
		int GetNextAvailableLayerId();

	}


	/// <summary>
	/// Holds a list of layers.
	/// </summary>
	internal class LayerCollection : ILayerCollection {

		internal LayerCollection(Diagram diagram) {
			this._diagram = diagram;
		}


		/// <summary>
		/// Returns the first unused layer id.
		/// </summary>
		public int GetNextAvailableLayerId() {
			// As the layers are sorted by id, we can simply increment until we find a free slot...
			int result = 1;
			while (_layers.ContainsKey(result))
				++result;
			return result;
		}


		#region ILayerCollection Members

		public Layer this[LayerIds layerId] {
			get { return this[Layer.ConvertToLayerId(layerId)]; }
		}


		public Layer this[int layerId] {
			get { return _layers[layerId]; }
		}


		public Layer this[string layerName] {
			get {
				if (string.IsNullOrEmpty(layerName)) throw new ArgumentNullException("layerName");
				return _layers[_layerNames[layerName.ToLowerInvariant()]]; 
			}
		}

		
		public Layer FindLayer(string name) {
			if (name == null) throw new ArgumentNullException("name");			
			int layerId;
			if (_layerNames.TryGetValue(name.ToLowerInvariant(), out layerId))
				return _layers[layerId];
			return null;
		}


		[Obsolete("Use method FindLayer instead.")]
		public Layer GetLayer(LayerIds layerId) {
			return FindLayer(layerId);
		}


		public Layer FindLayer(LayerIds layerId) {
			int id = Layer.ConvertToLayerId(layerId);
			return FindLayer(id);
		}


		public Layer FindLayer(int layerId) {
			Layer result = null;
			_layers.TryGetValue(layerId, out result);
			return result;
		}


		public IEnumerable<Layer> GetLayers(LayerIds layerIds) {
			return FindLayers(layerIds);
		}


		public IEnumerable<Layer> FindLayers(LayerIds layerIds) {
			foreach (int id in LayerHelper.GetAllLayerIds(layerIds)) {
				Layer layer = FindLayer(id);
				if (layer != null) 
					yield return layer;
			}
		}


		public IEnumerable<Layer> FindLayers(IEnumerable<int> layerIds) {
			if (layerIds == null) throw new ArgumentNullException("layerIds");
			foreach (int id in layerIds) {
				Layer layer = FindLayer(id);
				if (layer != null)
					yield return layer;
			}
		}


		public bool RenameLayer(string previousName, string newName) {
			if (string.IsNullOrEmpty(previousName)) throw new ArgumentNullException("previousName");
			if (string.IsNullOrEmpty(newName)) throw new ArgumentNullException("newName");
			string previousNameLC = previousName.ToLowerInvariant();
			string newNameLC = newName.ToLowerInvariant();
			if (!_layerNames.ContainsKey(previousNameLC)) throw new KeyNotFoundException("previousName");
			if (_layerNames.ContainsKey(newNameLC)) throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_ALayerNamed0AlreadyExists, newName));
			
			Layer layer = _layers[_layerNames[previousNameLC]];
			if (layer != null) {
				_layerNames.Remove(previousNameLC);
				layer.Name = newName;
				_layerNames.Add(newNameLC, layer.LayerId);
				return true;
			} else return false;
		}

		#endregion


		#region ICollection<Layer> Members

		public void Add(Layer layer) {
			if (layer == null) throw new ArgumentNullException("item");
			if (string.IsNullOrEmpty(layer.Name)) throw new ArgumentNullException("layer.Name");
			// Assign the first unused layer id if the layer does not have an id.
			if (layer.LayerId == Layer.NoLayerId)
				layer.LayerId = GetNextAvailableLayerId();
			// Insert layer
			if (_layers.ContainsKey(layer.LayerId))
				throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_0Item1AlreadyExists, typeof(Layer).Name, layer.LayerId));
			string layerNameLC = layer.Name.ToLowerInvariant();
			if (_layerNames.ContainsKey(layerNameLC))
				throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_0Item1AlreadyExists, typeof(Layer).Name, layer.Name));
			_layers.Add(layer.LayerId, layer);
			_layerNames.Add(layerNameLC, layer.LayerId);
		}


		public void Clear() {
			_layers.Clear();
			_layerNames.Clear();
		}


		public bool Contains(Layer layer) {
			// The LayerCollection cannot contain layers with LayerId 'Layer.InvalidId' or 'Layer.UnassignedId'
			if (layer.LayerId == Layer.NoLayerId)
				return false;
			// Try to avoid calling ContainsValue as it will perform a linear search.
			if (_layers[layer.LayerId] == layer)
				return true;
			return _layers.ContainsValue(layer);
		}


		public bool Contains(int layerId) {
			return _layers.ContainsKey(layerId);
		}


		public bool Contains(string layerName) {
			if (string.IsNullOrEmpty(layerName)) throw new ArgumentNullException("layerName");
			return _layerNames.ContainsKey(layerName.ToLowerInvariant());
		}


		public void CopyTo(Layer[] array, int arrayIndex) {
			if (array == null) throw new ArgumentNullException("array");
			_layers.Values.CopyTo(array, arrayIndex);
		}


		public int Count {
			get { return _layers.Count; }
		}


		public bool IsReadOnly {
			get { return false; }
		}


		public bool Remove(Layer item) {
			if (item == null) throw new ArgumentNullException("item");
			Debug.Assert(this[item.Name] == item);
			_layerNames.Remove(item.Name.ToLowerInvariant());
			return _layers.Remove(item.LayerId);
		}

		#endregion


		#region IEnumerable<Layer> Members

		public IEnumerator<Layer> GetEnumerator() {
			return _layers.Values.GetEnumerator();
		}

		#endregion


		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator() {
			return _layers.Values.GetEnumerator();
		}

		#endregion


		#region [Private] Methods

		//private int GetLayerBit(int layerId) {
		//    //int result = -1;
		//    //foreach (int layerBit in GetLayerBits(layerId)) {
		//    //    if (result < 0) result = layerBit;
		//    //    else throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_0IsNotAValidLayerIdForOneSingleLayer));
		//    //}
		//    //return result;
		//}

		#endregion


		#region [Private] Types and Fields

		//private struct Enumerator : IEnumerator<Layer>, IEnumerator {

		//    public static Enumerator Create(SortedList<int, Layer> layerList) {
		//        if (layerList == null) throw new ArgumentNullException("layerList");
		//        Enumerator result = Enumerator.Empty;
		//        result.layerList = layerList;
		//        result.currentIdx = -1;
		//        result.currentLayer = null;
		//        return result;
		//    }


		//    public static readonly Enumerator Empty;


		//    public Enumerator(SortedList<int, Layer> layerList) {
		//        if (layerList == null) throw new ArgumentNullException("layerList");
		//        this.layerList = layerList;
		//        this.currentIdx = -1;
		//        this.currentLayer = null;
		//    }


		//    #region IEnumerator<Layer> Members

		//    public Layer Current { get { return currentLayer; } }

		//    #endregion


		//    #region IDisposable Members

		//    public void Dispose() {
		//        // nothing to do
		//    }

		//    #endregion


		//    #region IEnumerator Members

		//    object IEnumerator.Current { get { return currentLayer; } }

		//    public bool MoveNext() {
		//        bool result = false;
		//        currentLayer = null;
		//        while (currentIdx < layerList.Count - 1 && !result) {
		//            currentLayer = layerList[++currentIdx];
		//            if (currentLayer != null) result = true;
		//        }
		//        return result;
		//    }

		//    public void Reset() {
		//        currentIdx = -1;
		//        currentLayer = null;
		//    }

		//    #endregion


		//    static Enumerator() {
		//        Empty.layerList = null;
		//        Empty.currentIdx = -1;
		//        Empty.currentLayer = null;
		//    }

		//    #region Fields
		//    private SortedList<int, Layer> layerList;
		//    private int currentIdx;
		//    private Layer currentLayer;
		//    #endregion
		//}


		private SortedList<int, Layer> _layers = new SortedList<int, Layer>();
		private Dictionary<string, int> _layerNames = new Dictionary<string, int>();
		private Diagram _diagram = null;
		
		#endregion
	}

}

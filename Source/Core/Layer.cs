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


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Describes the layers a shape is part of as set of layer ids between 1 and 32.
	/// </summary>
	[Flags]
	public enum LayerIds : uint {
		/// <summary>No Layers.</summary>
		None = 0x0,
		/// <summary>Layer 1</summary>
		Layer01 = 0x00000001,
		/// <summary>Layer 2</summary>
		Layer02 = 0x00000002,
		/// <summary>Layer 3</summary>
		Layer03 = 0x00000004,
		/// <summary>Layer 4</summary>
		Layer04 = 0x00000008,
		/// <summary>Layer 5</summary>
		Layer05 = 0x00000010,
		/// <summary>Layer 6</summary>
		Layer06 = 0x00000020,
		/// <summary>Layer 7</summary>
		Layer07 = 0x00000040,
		/// <summary>Layer 8</summary>
		Layer08 = 0x00000080,
		/// <summary>Layer 9</summary>
		Layer09 = 0x00000100,
		/// <summary>Layer 10</summary>
		Layer10 = 0x00000200,
		/// <summary>Layer 11</summary>
		Layer11 = 0x00000400,
		/// <summary>Layer 12</summary>
		Layer12 = 0x00000800,
		/// <summary>Layer 13</summary>
		Layer13 = 0x00001000,
		/// <summary>Layer 14</summary>
		Layer14 = 0x00002000,
		/// <summary>Layer 15</summary>
		Layer15 = 0x00004000,
		/// <summary>Layer 16</summary>
		Layer16 = 0x00008000,
		/// <summary>Layer 17</summary>
		Layer17 = 0x00010000,
		/// <summary>Layer 18</summary>
		Layer18 = 0x00020000,
		/// <summary>Layer 19</summary>
		Layer19 = 0x00040000,
		/// <summary>Layer 20</summary>
		Layer20 = 0x00080000,
		/// <summary>Layer 21</summary>
		Layer21 = 0x00100000,
		/// <summary>Layer 22</summary>
		Layer22 = 0x00200000,
		/// <summary>Layer 23</summary>
		Layer23 = 0x00400000,
		/// <summary>Layer 24</summary>
		Layer24 = 0x00800000,
		/// <summary>Layer 25</summary>
		Layer25 = 0x01000000,
		/// <summary>Layer 26</summary>
		Layer26 = 0x02000000,
		/// <summary>Layer 27</summary>
		Layer27 = 0x04000000,
		/// <summary>Layer 28</summary>
		Layer28 = 0x08000000,
		/// <summary>Layer 29</summary>
		Layer29 = 0x10000000,
		/// <summary>Layer 30</summary>
		Layer30 = 0x20000000,
		/// <summary>Layer 31</summary>
		Layer31 = 0x40000000,
		/// <summary>Layer 32</summary>
		Layer32 = 0x80000000,
		/// <summary>All available layers.</summary>
		All = uint.MaxValue
	}


	/// <summary>
	/// A layer used for grouping shapes on a diagram.
	/// </summary>
	/// <status>reviewed</status>
	[TypeDescriptionProvider(typeof(TypeDescriptionProviderDg))]
	public class Layer {

		/// <summary>
		/// Defines a layer id that means 'no layer id has been assigned yet'.
		/// This is the default LayerId value. When inserted into a LayerCollection, the layer id will be set to the next available layer id.
		/// </summary>
		public const int NoLayerId = 0;


		/// <summary>
		/// Converts an integer layer id to a LayerIds value.
		/// Example: LayerId 3 will be converted to LayerIds.Layer03 which has the numeric value 0x00000004.
		/// </summary>
		public static LayerIds ConvertToLayerIds(int layerNo) {
			if (layerNo < 1 || layerNo > 32) throw new ArgumentOutOfRangeException("layerNo");
			return (layerNo != 0) ? (LayerIds)Math.Pow(2, layerNo - 1) : LayerIds.None;
		}


		/// <summary>
		/// Converts a collection of combinable integer layer ids to a LayerIds value.
		/// Layer ids that are not comnbinable will be omitted.
		/// </summary>
		public static LayerIds ConvertToLayerIds(IEnumerable<int> layerNos) {
			if (layerNos == null) throw new ArgumentNullException("layerIds");
			LayerIds result = LayerIds.None;
			foreach (int id in layerNos) {
				if (IsCombinable(id))
					result |= ConvertToLayerIds(id);
			}
			return result;
		}


		/// <summary>
		/// Converts a collection of combinable layers to a LayerIds value.
		/// Layers with ids that are not combinable will be omitted.
		/// </summary>
		public static LayerIds ConvertToLayerIds(IEnumerable<Layer> layers) {
			if (layers == null) throw new ArgumentNullException("layers");
			LayerIds result = LayerIds.None;
			foreach (Layer layer in layers) {
				if (IsCombinable(layer.LayerId))
					result |= ConvertToLayerIds(layer.LayerId);
			}
			return result;
		}


		/// <summary>
		/// Converts a combinable LayerIds value to an integer layer id.
		/// </summary>
		/// <param name="layerId">A LayerIds value.</param>
		/// <remarks>Combinations of LayerIds values as well as LayerIds.All will lead to an ArgumentException.</remarks>
		public static int ConvertToLayerId(LayerIds layerId) {
			if (layerId > LayerIds.Layer32) throw new ArgumentOutOfRangeException("layerId");
			if (layerId == LayerIds.All) throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_0IsNotAValidLayerIdForOneSingleLayer, layerId), "layerId");
			if (layerId == LayerIds.None) 
				return Layer.NoLayerId;
			double result = Math.Round(Math.Log((uint)layerId, 2), 12);
			if (result != (int)result)
				throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_0IsNotAValidLayerIdForOneSingleLayer, layerId), "layerId");
			return 1 + (int)result;
		}


		/// <summary>
		/// Tests whether the given layer id is combinable (can be converted to a LayerIds value).
		/// </summary>
		public static bool IsCombinable(int layerNo) {
			return (layerNo >= 1 && layerNo <= 32);
		}


		///// <summary>
		///// Gets the layer type of the given layer id.
		///// </summary>
		//public static LayerType GetLayerType(int layerId){
		//    return IsCombinable(layerId) ? LayerType.Supplemental : LayerType.Home;
		//}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Layer" />.
		/// </summary>
		public Layer(string name) {
			if (name == null) throw new ArgumentNullException("name");
			if (name == string.Empty) throw new ArgumentException("Parameter name must not be empty.");
			this._name = name;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Layer" />.
		/// </summary>
		internal Layer(int layerNo, string name)
			: this(name) {
			if (layerNo == (int)LayerIds.None || (uint)layerNo == (uint)LayerIds.All) throw new ArgumentException("Invalid layer id.");
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
			this._layerId = layerNo;
			this._name = name;
		}


		/// <summary>
		/// The <see cref="T:Dataweb.NShape.LayerIds" /> value used to identify the <see cref="T:Dataweb.NShape.Layer" />.
		/// </summary>
		[Obsolete("Use property LayerNo instead and convert it using Layer.ConvertToLayerIds().")]
		public LayerIds Id {
			get { return ConvertToLayerIds(_layerId); }
			internal set { LayerId = ConvertToLayerId(value); }
		}


		/// <summary>
		/// A <see cref="T:System.Int32" /> value used to identify the <see cref="T:Dataweb.NShape.Layer" />.
		/// </summary>
		public int LayerId {
			get { return _layerId; }
			internal set { 
				_layerId = value;
			}
		}


		///// <summary>
		///// Gets the <see cref="T:Dataweb.NShape.LayerType" /> value that specifies wheter this <see cref="T:Dataweb.NShape.Layer" /> is combinable or not.
		///// </summary>
		//public LayerType LayerType {
		//    get { return _layerType; }
		//}


		/// <summary>
		/// The language independent name of the <see cref="T:Dataweb.NShape.Layer" />.
		/// </summary>
		[RequiredPermission(Permission.Data)]
		public string Name {
			get { return _name; }
			internal set { _name = value; }
		}


		/// <summary>
		/// The localized title of the <see cref="T:Dataweb.NShape.Layer" />.
		/// </summary>
		[RequiredPermission(Permission.Present)]
		public string Title {
			get { return string.IsNullOrEmpty(_title) ? _name : _title; }
			set {
				if (value == _name || string.IsNullOrEmpty(value))
					_title = null;
				else _title = value;
			}
		}


		/// <summary>
		/// Specifies the minimum zoom level where the <see cref="T:Dataweb.NShape.Layer" /> is still visible. On lower zoom levels, the layer will be hidden automatically.
		/// </summary>
		[RequiredPermission(Permission.Present)]
		public int LowerZoomThreshold {
			get { return _lowerZoomThreshold; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("LowerZoomThreshold");
				_lowerZoomThreshold = value;
			}
		}


		/// <summary>
		/// Specifies the maximum zoom level where the <see cref="T:Dataweb.NShape.Layer" /> is still visible. On higher zoom levels, the layer will be hidden automatically.
		/// </summary>
		[RequiredPermission(Permission.Present)]
		public int UpperZoomThreshold {
			get { return _upperZoomThreshold; }
			set {
				if (value < 0) throw new ArgumentOutOfRangeException("UpperZoomThreshold");
				_upperZoomThreshold = value;
			}
		}


		#region Fields

		//private LayerIds id = LayerIds.None;
		private int _layerId = NoLayerId;
		//private LayerType _layerType = LayerType.Supplemental;
		private string _name = string.Empty;
		private string _title = string.Empty;
		private int _lowerZoomThreshold = 0;
		private int _upperZoomThreshold = 5000;

		#endregion
	}


	/// <ToBeCompleted></ToBeCompleted>
	public struct LayerInfo {

		/// <ToBeCompleted></ToBeCompleted>
		public static bool operator ==(LayerInfo a, LayerInfo b) {
			return (a.HomeLayer == b.HomeLayer && a.SupplementalLayers == b.SupplementalLayers);
		}

		/// <ToBeCompleted></ToBeCompleted>
		public static bool operator !=(LayerInfo a, LayerInfo b) {
			return !(a == b);
		}

		/// <ToBeCompleted></ToBeCompleted>
		public static readonly LayerInfo Empty;

		/// <ToBeCompleted></ToBeCompleted>
		public static LayerInfo Create(int homeLayer, LayerIds supplementalLayers) {
			LayerInfo result = LayerInfo.Empty;
			result.HomeLayer = homeLayer;
			result.SupplementalLayers = supplementalLayers;
			return result;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public int HomeLayer { get; set; }

		/// <ToBeCompleted></ToBeCompleted>
		public LayerIds SupplementalLayers { get; set; }

		/// <ToBeCompleted></ToBeCompleted>
		public override bool Equals(object obj) {
			return base.Equals(obj);
		}

		/// <ToBeCompleted></ToBeCompleted>
		public override int GetHashCode() {
			// Overflow is fine, just wrap
			unchecked {
				// We use prime numbers 17 and 23, could also be other prime numbers
				int result = 17;
				result = result * 23 + HomeLayer.GetHashCode();
				result = result * 23 + SupplementalLayers.GetHashCode();
				return result;
			}
		}

		/// <ToBeCompleted></ToBeCompleted>
		static LayerInfo() {
			Empty.HomeLayer = Layer.NoLayerId;
			Empty.SupplementalLayers = LayerIds.None;
		}

	}


	#region LayerEventArgs

	/// <ToBeCompleted></ToBeCompleted>
	public class LayerEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public LayerEventArgs(Layer layer) {
			this._layer = layer;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public Layer Layer {
			get { return _layer; }
			internal set { _layer = value; }
		}

		internal LayerEventArgs() { }

		private Layer _layer = null;
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class LayersEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public LayersEventArgs(IEnumerable<Layer> layers) {
			if (layers == null) throw new ArgumentNullException("layers");
			this.layers = new ReadOnlyList<Layer>(layers);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public IReadOnlyCollection<Layer> Layers { get { return layers; } }


		/// <ToBeCompleted></ToBeCompleted>
		protected internal LayersEventArgs() {
			layers = new ReadOnlyList<Layer>();
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal void SetLayers(ReadOnlyList<Layer> layers) {
			this.layers.Clear();
			this.layers.AddRange(layers);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal void SetLayers(IEnumerable<Layer> layers) {
			this.layers.Clear();
			this.layers.AddRange(layers);
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal void SetLayers(Layer layer) {
			this.layers.Clear();
			this.layers.Add(layer);
		}


		private ReadOnlyList<Layer> layers = null;
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class LayerRenamedEventArgs : LayerEventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public LayerRenamedEventArgs(Layer layer, string oldName, string newName)
			: base(layer) {

			this.oldName = oldName;
			this.newName = newName;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public string OldName {
			get { return oldName; }
			internal set { oldName = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public string NewName {
			get { return newName; }
			internal set { newName = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal LayerRenamedEventArgs() {
		}


		private string oldName;
		private string newName;
	}


	/// <ToBeCompleted></ToBeCompleted>
	public class LayerZoomThresholdChangedEventArgs : LayerEventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public LayerZoomThresholdChangedEventArgs(Layer layer, int oldZoomThreshold, int newZoomThreshold)
			: base(layer) {
			this.oldZoomThreshold = oldZoomThreshold;
			this.newZoomThreshold = newZoomThreshold;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int OldZoomThreshold {
			get { return oldZoomThreshold; }
			internal set { oldZoomThreshold = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public int NewZoomThreshold {
			get { return newZoomThreshold; }
			internal set { newZoomThreshold = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected internal LayerZoomThresholdChangedEventArgs() {
		}


		private int oldZoomThreshold;
		private int newZoomThreshold;
	}

	#endregion


	/// <ToBeCompleted></ToBeCompleted>
	internal class LayerHelper {

		/// <summary>
		/// Returns all layer ids of the given combined layers.
		/// </summary>
		public static IEnumerable<int> GetAllLayerIds(LayerIds layers) {
			if (layers == LayerIds.None) yield break;
			int bitNr = 1;
			foreach (LayerIds id in Enum.GetValues(typeof(LayerIds))) {
				if (id == LayerIds.None || id == LayerIds.All) continue;
				if ((layers & id) != 0)
					yield return bitNr;
				++bitNr;
			}
		}


		/// <summary>
		/// Returns all layer ids of the given layers.
		/// </summary>
		public static IEnumerable<int> GetAllLayerIds(IEnumerable<Layer> layers) {
			if (layers == null) throw new ArgumentNullException("layers");
			foreach (Layer layer in layers)
				yield return layer.LayerId;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static LayerEventArgs GetLayerEventArgs(string layerName, Diagram diagram) {
			if (diagram == null) throw new ArgumentNullException("diagram");
			Layer layer = diagram.Layers.FindLayer(layerName);
			Debug.Assert(layer != null);
			_layerEventArgs.Layer = layer;
			return _layerEventArgs;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static LayerEventArgs GetLayerEventArgs(Layer layer) {
			if (layer == null) throw new ArgumentNullException("layer");
			_layerEventArgs.Layer = layer;
			return _layerEventArgs;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static LayersEventArgs GetLayersEventArgs(Layer layer) {
			if (layer == null) throw new ArgumentNullException("layer");
			_layersEventArgs.SetLayers(layer);
			return _layersEventArgs;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static LayersEventArgs GetLayersEventArgs(ReadOnlyList<Layer> layers) {
			if (layers == null) throw new ArgumentNullException("layers");
			_layersEventArgs.SetLayers(layers);
			return _layersEventArgs;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static LayersEventArgs GetLayersEventArgs(IEnumerable<Layer> layers) {
			if (layers == null) throw new ArgumentNullException("layers");
			_layersEventArgs.SetLayers(layers);
			return _layersEventArgs;
		}


		private static LayerEventArgs _layerEventArgs = new LayerEventArgs();
		private static LayersEventArgs _layersEventArgs = new LayersEventArgs();
	}

}

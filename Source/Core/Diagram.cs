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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;


namespace Dataweb.NShape {

	/// <summary>
	/// Describes export image file format.
	/// </summary>
	public enum ImageFileFormat {
		/// <summary>Specifies bitmap (BMP) image format.</summary>
		Bmp,
		/// <summary>Specifies the enhanced Windows metafile image format (EMF).</summary>
		Emf,
		/// <summary>Specifies the Graphics Interchange Format (GIF) image format.</summary>
		Gif,
		/// <summary>Specifies the Joint Photographic Experts Group (JPEG) image format.</summary>
		Jpeg,
		/// <summary>Specifies the W3C Portable Network Graphics (PNG) image format.</summary>
		Png,
		/// <summary>Specifies the Tag Image File Format (TIFF) image format.</summary>
		Tiff,
		/// <summary>Specifies the enhanced Windows metafile plus image format (EMF).</summary>
		EmfPlus,
		/// <summary>Specifies the Scalable Vector Graphics file format (SVG).</summary>
		Svg
	}


	/// <summary>
	/// Read/write shape collection owned by a diagram.
	/// </summary>
	internal class DiagramShapeCollection : ShapeCollection {

		/// <override></override>
		public override void NotifyChildMoving(Shape shape) {
			base.NotifyChildMoving(shape);
			CheckOwnerboundsUpdateNeeded(shape);
			++_suspendUpdateCounter;
		}


		/// <override></override>
		public override void NotifyChildMoved(Shape shape) {
			base.NotifyChildMoved(shape);
			RaiseShapeMovedEvent(shape);

			CheckOwnerboundsUpdateNeeded(shape);
			--_suspendUpdateCounter;
			if (_suspendUpdateCounter == 0) DoUpdateOwnerBounds();
		}


		/// <override></override>
		public override void NotifyChildResizing(Shape shape) {
			base.NotifyChildResizing(shape);
			CheckOwnerboundsUpdateNeeded(shape);
			++_suspendUpdateCounter;
		}


		/// <override></override>
		public override void NotifyChildResized(Shape shape) {
			base.NotifyChildResized(shape);
			RaiseShapeResizedEvent(shape);

			CheckOwnerboundsUpdateNeeded(shape);
			--_suspendUpdateCounter;
			if (_suspendUpdateCounter == 0) DoUpdateOwnerBounds();
		}


		/// <override></override>
		public override void NotifyChildRotating(Shape shape) {
			base.NotifyChildRotating(shape);
			CheckOwnerboundsUpdateNeeded(shape);
			++_suspendUpdateCounter;
		}


		/// <override></override>
		public override void NotifyChildRotated(Shape shape) {
			base.NotifyChildRotated(shape);
			RaiseShapeRotatedEvent(shape);

			CheckOwnerboundsUpdateNeeded(shape);
			--_suspendUpdateCounter;
			if (_suspendUpdateCounter == 0) DoUpdateOwnerBounds();
		}


		internal DiagramShapeCollection(Diagram owner)
			: this(owner, 1000) {
		}


		internal DiagramShapeCollection(Diagram owner, int capacity)
			: base(capacity) {
			if (owner == null) throw new ArgumentNullException(nameof(owner));
			this._owner = owner;
		}


		internal DiagramShapeCollection(Diagram owner, IEnumerable<Shape> collection)
			: this(owner, (collection is ICollection) ? ((ICollection)collection).Count : 0) {
			AddRangeCore(collection);
		}


		internal Diagram Owner {
			get { return _owner; }
		}


		/// <override></override>
		protected override void AddRangeCore(IEnumerable<Shape> collection) {
			Debug.Assert(_suspendUpdateCounter == 0);
			if (collection is ICollection) _suspendUpdateCounter = ((ICollection)collection).Count;
			else foreach (Shape s in collection) ++_suspendUpdateCounter;
			base.AddRangeCore(collection);
		}


		/// <override></override>
		protected override bool RemoveRangeCore(IEnumerable<Shape> collection) {
			Debug.Assert(_suspendUpdateCounter == 0);
			if (collection is ICollection) _suspendUpdateCounter = ((ICollection)collection).Count;
			else foreach (Shape s in collection) ++_suspendUpdateCounter;
			return base.RemoveRangeCore(collection);
		}


		/// <override></override>
		protected override void ReplaceRangeCore(IEnumerable<Shape> oldShapes, IEnumerable<Shape> newShapes) {
			Debug.Assert(_suspendUpdateCounter == 0);
			if (oldShapes is ICollection) _suspendUpdateCounter = ((ICollection)oldShapes).Count;
			else foreach (Shape s in oldShapes) ++_suspendUpdateCounter;
			base.ReplaceRangeCore(oldShapes, newShapes);
		}


		/// <override></override>
		protected override int InsertCore(int index, Shape shape) {
			int result = base.InsertCore(index, shape);
			shape.Diagram = _owner;
			shape.DisplayService = _owner.DisplayService;
			shape.Invalidate();

			CheckOwnerboundsUpdateNeeded(shape);
			if (_suspendUpdateCounter > 0) --_suspendUpdateCounter;
			if (_suspendUpdateCounter == 0) DoUpdateOwnerBounds();

			return result;
		}


		/// <override></override>
		protected override void ReplaceCore(Shape oldShape, Shape newShape) {
			base.ReplaceCore(oldShape, newShape);
			oldShape.Diagram = null;
			oldShape.Invalidate();
			oldShape.DisplayService = null;
			newShape.Diagram = _owner;
			newShape.DisplayService = _owner.DisplayService;
			newShape.Invalidate();

			CheckOwnerboundsUpdateNeeded(oldShape);
			CheckOwnerboundsUpdateNeeded(newShape);
			if (_suspendUpdateCounter > 0) --_suspendUpdateCounter;
			if (_suspendUpdateCounter == 0) DoUpdateOwnerBounds();
		}


		/// <override></override>
		protected override bool RemoveCore(Shape shape) {
			bool result = base.RemoveCore(shape);
			shape.Invalidate();
			shape.DisplayService = null;
			shape.Diagram = null;

			CheckOwnerboundsUpdateNeeded(shape);
			if (_suspendUpdateCounter > 0) --_suspendUpdateCounter;
			if (_suspendUpdateCounter == 0) DoUpdateOwnerBounds();

			return result;
		}


		/// <override></override>
		protected override void ClearCore() {
			foreach (Shape shape in Shapes) {
				CheckOwnerboundsUpdateNeeded(shape);
				shape.Invalidate();
				shape.DisplayService = null;
			}
			base.ClearCore();
			DoUpdateOwnerBounds();
		}


		private void CheckOwnerboundsUpdateNeeded(Shape shape) {
			if (!_ownerBoundsUpdateNeeded) {
				Rectangle shapeBounds = shape.GetBoundingRectangle(true);
				if (shapeBounds.Left < 0 || _owner.Width < shapeBounds.Right
					|| shapeBounds.Top < 0 || _owner.Height < shapeBounds.Bottom)
					_ownerBoundsUpdateNeeded = true;
			}
		}


		private void DoUpdateOwnerBounds() {
			Debug.Assert(_suspendUpdateCounter == 0);
			if (_ownerBoundsUpdateNeeded) {
				if (_owner != null) _owner.OnResized(EventArgs.Empty);
				_ownerBoundsUpdateNeeded = false;
			}
		}


		private void RaiseShapeMovedEvent(Shape shape) {
			_shapeEventArgs.SetShape(shape);
			_owner.OnShapeMoved(_shapeEventArgs);
		}


		private void RaiseShapeResizedEvent(Shape shape) {
			_shapeEventArgs.SetShape(shape);
			_owner.OnShapeResized(_shapeEventArgs);
		}


		private void RaiseShapeRotatedEvent(Shape shape) {
			_shapeEventArgs.SetShape(shape);
			_owner.OnShapeRotated(_shapeEventArgs);
		}


		#region Fields
		private Diagram _owner = null;
		private int _suspendUpdateCounter = 0;
		private bool _ownerBoundsUpdateNeeded;
		private ShapeEventArgs _shapeEventArgs = new ShapeEventArgs();
		#endregion
	}


	/// <summary>
	/// Displays shapes in layers.
	/// </summary>
	[TypeDescriptionProvider(typeof(TypeDescriptionProviderDg))]
	public sealed class Diagram : IEntity, ISecurityDomainObject, IDisposable {

		/// <summary>
		/// Maximum number of pixels for bitmap pictures.
		/// </summary>
		public const int MaxBitmapPixelCount = 188457984;


		/// <summary>
		/// Returns true if the shape is assigned at least to one of the visible shared layers.
		/// </summary>
		public static bool IsShapeVisible(Shape shape, LayerIds visibleLayers, ICollection<int> visibleHomeLayers) {
			if (shape.SupplementalLayers == LayerIds.None && shape.HomeLayer == Layer.NoLayerId)
				return true;
			else {
				if (IsSupplementalLayerVisible(shape, visibleLayers))
					return true;
				else if (IsHomeLayerVisible(shape, visibleHomeLayers))
					return true;
				else
					return false;
			}
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Diagram" />.
		/// </summary>
		public Diagram(string name) {
			if (name == null) throw new ArgumentNullException(nameof(name));
			this._name = name;
			_diagramShapes = new DiagramShapeCollection(this, _expectedShapes);
			_layers = new LayerCollection(this);
			// A new diagram has no layers.

			// Set size to DIN A4
			using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero)) {
				Width = (int)Math.Round((gfx.DpiX * (1/25.4f)) * 210);
				Height = (int)Math.Round((gfx.DpiY * (1/25.4f)) * 297);
			}
		}


		/// <override></override>
		public void Dispose() {
			InvalidateDrawCache();
			GdiHelpers.DisposeObject(ref _backImage);
			GdiHelpers.DisposeObject(ref _backImageLowRes);
			GdiHelpers.DisposeObject(ref _colorBrush);
			GdiHelpers.DisposeObject(ref _imageAttribs);
		}


		#region [Public] Events

		/// <summary>Raised when the diagram size changes.</summary>
		public event EventHandler Resized;

		/// <summary>Raised when a shape of the diagram has been moved.</summary>
		public event EventHandler<ShapeEventArgs> ShapeMoved;

		/// <summary>Raised when a shape of the diagram has been resized.</summary>
		public event EventHandler<ShapeEventArgs> ShapeResized;

		/// <summary>Raised when a shape of the diagram has been rotated.</summary>
		public event EventHandler<ShapeEventArgs> ShapeRotated;

		#endregion


		#region [Public] Properties

		/// <summary>
		/// Culture invariant name.
		/// </summary>
		[CategoryGeneral()]
		[LocalizedDisplayName("PropName_Diagram_Name")]
		[Description("PropDesc_Diagram_Name")]
		[RequiredPermission(Permission.Data)]
		public string Name {
			get { return _name; }
			set { _name = value ?? string.Empty; }
		}


		/// <summary>
		/// Culture depending title.
		/// </summary>
		[CategoryGeneral()]
		[LocalizedDisplayName("PropName_Diagram_Title")]
		[LocalizedDescription("PropDesc_Diagram_Title")]
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
		/// Width of diagram in pixels.
		/// </summary>
		[CategoryLayout()]
		[LocalizedDisplayName("PropName_Diagram_Width")]
		[LocalizedDescription("PropDesc_Diagram_Width")]
		[RequiredPermission(Permission.Layout)]
		public int Width {
			get { return _size.Width; }
			set {
				if (_size.Width != value) {
					if (_displayService != null)
						_displayService.Invalidate(0, 0, Width, Height);

					_size.Width = (value <= 0) ? 1 : value;
					OnResized(EventArgs.Empty);
					
					if (_displayService != null) 
						_displayService.Invalidate(0, 0, Width, Height);
				}
			}
		}


		/// <summary>
		/// Height of diagram in pixels.
		/// </summary>
		[CategoryLayout()]
		[LocalizedDisplayName("PropName_Diagram_Height")]
		[LocalizedDescription("PropDesc_Diagram_Height")]
		[RequiredPermission(Permission.Layout)]
		public int Height {
			get { return _size.Height; }
			set {
				if (_size.Height != value) {
					if (_displayService != null)
						_displayService.Invalidate(0, 0, Width, Height);

					_size.Height = (value <= 0) ? 1 : value;
					OnResized(EventArgs.Empty);

					if (_displayService != null)
						_displayService.Invalidate(0, 0, Width, Height);
				}
			}
		}


		/// <summary>
		/// Size of diagram in pixels.
		/// </summary>
		[Browsable(false)]
		public Size Size {
			get { return _size; }
			set {
				if (_displayService != null)
					_displayService.Invalidate(0, 0, Width, Height);

				_size.Width = (value.Width <= 0) ? 1 : value.Width;
				_size.Height = (value.Height <= 0) ? 1 : value.Height;
				OnResized(EventArgs.Empty);

				if (_displayService != null)
					_displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Background color of the diagram.
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_Diagram_BackgroundColor")]
		[LocalizedDescription("PropDesc_Diagram_BackgroundColor")]
		[RequiredPermission(Permission.Present)]
		public Color BackgroundColor {
			get { return _backColor; }
			set { 
				_backColor = value;
				if (_colorBrush != null) {
					_colorBrush.Dispose();
					_colorBrush = null;
				}
				if (_displayService != null)
					_displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Second color of background gradient.
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_Diagram_BackgroundGradientColor")]
		[LocalizedDescription("PropDesc_Diagram_BackgroundGradientColor")]
		[RequiredPermission(Permission.Present)]
		public Color BackgroundGradientColor {
			get { return _targetColor; }
			set { 
				_targetColor = value;
				if (_colorBrush != null) {
					_colorBrush.Dispose();
					_colorBrush = null;
				}
				if (_displayService != null)
					_displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Background image of diagram.
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_Diagram_BackgroundImage")]
		[LocalizedDescription("PropDesc_Diagram_BackgroundImage")]
		[Editor("Dataweb.NShape.WinFormsUI.NamedImageUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
		[RequiredPermission(Permission.Present)]
		public NamedImage BackgroundImage {
			get { return _backImage; }
			set {
				if (_backImage != null) {
					GdiHelpers.DisposeObject(ref _backImage);
					GdiHelpers.DisposeObject(ref _backImageLowRes); 
				}

				_backImage = value;

				InvalidateDrawCache();
				// Create the fastRedraw brush now as this action can take some time with big images...
				UpdateFastRedrawBackImage();
				//
				if (_displayService != null) _displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Image layout of background image.
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_Diagram_BackgroundImageLayout")]
		[LocalizedDescription("PropDesc_Diagram_BackgroundImageLayout")]
		[RequiredPermission(Permission.Present)]
		public ImageLayoutMode BackgroundImageLayout {
			get { return _imageLayout; }
			set { 
				_imageLayout = value;
				InvalidateDrawCache();
				if (_displayService != null) _displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Gamma correction factor for the background image.
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_Diagram_BackgroundImageGamma")]
		[LocalizedDescription("PropDesc_Diagram_BackgroundImageGamma")]
		[RequiredPermission(Permission.Present)]
		public float BackgroundImageGamma {
			get { return _imageGamma; }
			set {
				if (value <= 0) throw new ArgumentOutOfRangeException(nameof(BackgroundImageGamma), "Value has to be greater 0.");
				_imageGamma = value;
				InvalidateDrawCache();
				if (_displayService != null) _displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Specifies if the background image should be displayed as gray scale image.
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_Diagram_BackgroundImageGrayscale")]
		[LocalizedDescription("PropDesc_Diagram_BackgroundImageGrayscale")]
		[RequiredPermission(Permission.Present)]
		public bool BackgroundImageGrayscale {
			get { return _imageGrayScale; }
			set {
				_imageGrayScale = value;
				InvalidateDrawCache();
				if (_displayService != null) _displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Transparency of the background image in percentage.
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_Diagram_BackgroundImageTransparency")]
		[LocalizedDescription("PropDesc_Diagram_BackgroundImageTransparency")]
		[RequiredPermission(Permission.Present)]
		public byte BackgroundImageTransparency {
			get { return _imageTransparency; }
			set {
				if (value < 0 || value > 100) throw new ArgumentOutOfRangeException(nameof(BackgroundImageTransparency), "The value has to be between 0 and 100.");
				_imageTransparency = value;
				InvalidateDrawCache();
				if (_displayService != null) _displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// The specified color of the background image will be transparent.
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_Diagram_BackgroundImageTransparentColor")]
		[LocalizedDescription("PropDesc_Diagram_BackgroundImageTransparentColor")]
		[RequiredPermission(Permission.Present)]
		public Color BackgroundImageTransparentColor {
			get { return _imageTransparentColor; }
			set {
				_imageTransparentColor = value;
				InvalidateDrawCache();
				if (_displayService != null) _displayService.Invalidate(0, 0, Width, Height);
			}
		}


		/// <summary>
		/// Toggles the visibility of the diagram's background image
		/// </summary>
		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_Diagram_IsBackgroundImageVisible")]
		[LocalizedDescription("PropDesc_Diagram_IsBackgroundImageVisible")]
		[RequiredPermission(Permission.Present)]
		public bool IsBackgroundImageVisible {
			get { return _isBackgroundImageVisible; }
			set {
				if (_isBackgroundImageVisible != value) {
					_isBackgroundImageVisible = value;
					if (_displayService != null) _displayService.Invalidate(0, 0, Width, Height);
				}
			}
		}


		/// <summary>
		/// Indicates the diagram model object displayed by this diagram. May be null.
		/// </summary>
		[Browsable(false)]
		[RequiredPermission(Permission.Data)]
		public IDiagramModelObject ModelObject {
			get { return _modelObject; }
			set {
				if (_modelObject != value) {
					if (_modelObject != value) {
						// Store model object reference for calling Detach() *after* the property value was applied
						// because the DiagramModelObject's Detach() will also set the diagram's ModelObject property to null.
						IDiagramModelObject oldModelObj = _modelObject;
						_modelObject = value;
						if (oldModelObj != null) oldModelObj.DetachDiagram();
						if (_modelObject != null) _modelObject.AttachDiagram(this);
					}
				}
			}
		}


		/// <summary>
		/// Specifies the display service to use for this diagram.
		/// </summary>
		[Browsable(false)]
		public IDisplayService DisplayService {
			get { return _displayService; }
			set {
				if (_displayService != value) {
					_displayService = value;
					_diagramShapes.SetDisplayService(_displayService);
				}
				if (_displayService != null) _displayService.Invalidate(0, 0, Width, Height);
				else InvalidateDrawCache();
			}
		}


		/// <summary>
		/// Provides access to the diagram layers.
		/// </summary>
		[Browsable(false)]
		public ILayerCollection Layers {
		   get { return _layers; }
		}


		/// <summary>
		/// Provides access to the diagram shapes.
		/// </summary>
		[Browsable(false)]
		public IShapeCollection Shapes {
			get { return _diagramShapes; }
		}


		/// <summary>
		/// Indicates the name of the security domain this shape belongs to.
		/// </summary>
		[CategoryGeneral()]
		[LocalizedDisplayName("PropName_Diagram_SecurityDomainName")]
		[LocalizedDescription("PropDesc_Diagram_SecurityDomainName")]
		[RequiredPermission(Permission.Security)]
		public char SecurityDomainName {
			get { return _securityDomainName; }
			set {
				if (value < 'A' || value > 'Z')
					throw new ArgumentOutOfRangeException(nameof(SecurityDomainName), "The domain qualifier has to be an upper case  ANSI letter (A-Z).");
				_securityDomainName = value; 
			}
		}


		/// <summary>
		/// Specifies whether the diagram is rendered with high visual quality. 
		/// This property is typically set by the diagram presenter.
		/// </summary>
		[Browsable(false)]
		public bool HighQualityRendering {
			get { return _highQualityRendering; }
			set {
				_highQualityRendering = value;
				if (_colorBrush != null) {
					_colorBrush.Dispose();
					_colorBrush = null;
				}
			}
		}

		#endregion


		#region [Public] Methods: Layer management

		/// <summary>
		/// Gets all <see cref="T:Dataweb.NShape.LayerIds" /> the given <see cref="T:Dataweb.NShape.Advanced.NShape" /> is part of.
		/// </summary>
		[Obsolete("Use GetShapeLayerIds instead.")]
		public LayerIds GetShapeLayers(Shape shape) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			return shape.SupplementalLayers;
		}


		/// <summary>
		/// Returns the id of each home- and supplemental layer the given <see cref="T:Dataweb.NShape.Advanced.NShape" /> is part of.
		/// </summary>
		/// <param name="shape"></param>
		/// <returns></returns>
		public IEnumerable<int> GetShapeLayerIds(Shape shape) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			if (shape.HomeLayer != Layer.NoLayerId)
				yield return shape.HomeLayer;
			foreach (int layerId in LayerHelper.GetAllLayerIds(shape.SupplementalLayers))
				yield return layerId;
		}


		/// <summary>
		/// Associates the given <see cref="T:Dataweb.NShape.Advanced.Shape" /> to all specified layers.
		/// </summary>
		[Obsolete("Use an overloaded version taking home layer and/or supplemental layers instead.")]
		public void AddShapeToLayers(Shape shape, LayerIds layers) {
			AddShapeToLayers(shape, Layer.NoLayerId, layers);
		}


		/// <summary>
		/// Associates the given <see cref="T:Dataweb.NShape.Advanced.Shape" /> to all specified layers.
		/// </summary>
		public void AddShapeToLayers(Shape shape, int homeLayer) {
			AddShapeToLayers(shape, homeLayer, LayerIds.None);
		}


		/// <summary>
		/// Associates the given <see cref="T:Dataweb.NShape.Advanced.Shape" /> to all specified layers.
		/// </summary>
		public void AddShapeToLayers(Shape shape, int homeLayer, LayerIds supplementalLayers) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			if (homeLayer != Layer.NoLayerId)
				shape.HomeLayer = homeLayer;
			if (supplementalLayers != LayerIds.None)
				shape.SupplementalLayers |= supplementalLayers;
		}


		/// <summary>
		/// Associates the given collection of <see cref="T:Dataweb.NShape.Advanced.Shape" /> to all specified layers.
		/// </summary>
		[Obsolete("Use an overloaded version taking home layer and supplemental layers instead.")]
		public void AddShapesToLayers(IEnumerable<Shape> shapes, LayerIds layers) {
			AddShapesToLayers(shapes, Layer.NoLayerId, layers);
		}


		/// <summary>
		/// Associates the given collection of <see cref="T:Dataweb.NShape.Advanced.Shape" /> to all specified layers.
		/// </summary>
		public void AddShapesToLayers(IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers) {
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			// Assign layers
			foreach (Shape shape in shapes) {
				if (homeLayer != Layer.NoLayerId)
					shape.HomeLayer = homeLayer;
				if (supplementalLayers != LayerIds.None)
					shape.SupplementalLayers |= supplementalLayers;
			}
		}


		/// <summary>
		/// Disociates the given <see cref="T:Dataweb.NShape.Advanced.Shape" /> to all specified layers.
		/// </summary>
		[Obsolete("Use an overloaded version taking home layer and/or supplemental layers instead.")]
		public void RemoveShapeFromLayers(Shape shape, LayerIds layers) {
			RemoveShapeFromLayers(shape, Layer.NoLayerId, layers);
		}


		/// <summary>
		/// Disociates the given <see cref="T:Dataweb.NShape.Advanced.Shape" /> to all specified layers.
		/// </summary>
		public void RemoveShapeFromLayers(Shape shape, int homeLayer) {
			RemoveShapeFromLayers(shape, homeLayer, LayerIds.None);
		}


		/// <summary>
		/// Removes the given <see cref="T:Dataweb.NShape.Advanced.Shape" /> from all specified layers.
		/// </summary>
		public void RemoveShapeFromLayers(Shape shape, int homeLayer, LayerIds supplementalLayers) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			// Remove home layer (if applicable)
			if (homeLayer == shape.HomeLayer)
				shape.HomeLayer = Layer.NoLayerId;
			// Remove shared layers
			if (supplementalLayers != LayerIds.None) {
				if (supplementalLayers == LayerIds.All)
					shape.SupplementalLayers = LayerIds.None;
				else
					shape.SupplementalLayers ^= (shape.SupplementalLayers & supplementalLayers);
			}
		}


		/// <summary>
		/// Removes the given <see cref="T:Dataweb.NShape.Advanced.Shape" /> from all layers.
		/// </summary>
		public void RemoveShapeFromLayers(Shape shape) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			shape.SupplementalLayers = LayerIds.None;
			shape.HomeLayer = Layer.NoLayerId;
		}


		/// <summary>
		/// Disociates the given collection of <see cref="T:Dataweb.NShape.Advanced.Shape" /> to all specified layers.
		/// </summary>
		[Obsolete("Use an overloaded version taking home layer and/or supplemental layers instead.")]
		public void RemoveShapesFromLayers(IEnumerable<Shape> shapes, LayerIds layers) {
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			RemoveShapesFromLayers(shapes, Layer.NoLayerId, layers);
		}


		/// <summary>
		/// Disociates the given collection of <see cref="T:Dataweb.NShape.Advanced.Shape" /> to all specified layers.
		/// </summary>
		public void RemoveShapesFromLayers(IEnumerable<Shape> shapes, int homeLayer) {
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			RemoveShapesFromLayers(shapes, homeLayer, LayerIds.None);
		}


		/// <summary>
		/// Disociates the given collection of <see cref="T:Dataweb.NShape.Advanced.Shape" /> to all specified layers.
		/// </summary>
		public void RemoveShapesFromLayers(IEnumerable<Shape> shapes, int homeLayer, LayerIds supplementalLayers) {
			if (shapes == null) throw new ArgumentNullException(nameof(shapes));
			// Assign layers
			foreach (Shape shape in shapes) {
				if (homeLayer == shape.HomeLayer)
					shape.HomeLayer = Layer.NoLayerId;
				if (supplementalLayers != LayerIds.None)
					shape.SupplementalLayers ^= (shape.SupplementalLayers & supplementalLayers);
			}
		}


		/// <summary>
		/// Delete all content of this <see cref="T:Dataweb.NShape.Diagram" />.
		/// </summary>
		public void Clear() {
			_diagramShapes.Clear();
			_layers.Clear();
		}

		#endregion


		#region [Public] Methods: Drawing and painting

		/// <summary>
		/// Exports the contents of the diagram to an image of the given format.
		/// </summary>
		/// <param name="imageFormat">Specifies the format of the graphics file.</param>
		public Image CreateImage(ImageFileFormat imageFormat) {
			return CreateImage(imageFormat, null, 0, false, Color.White, -1);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		/// <param name="imageFormat">Specifies the format of the graphics file.</param>
		/// <param name="shapes">The shapes that should be drawn. If null/Nothing, the whole diagram area will be exported.</param>
		public Image CreateImage(ImageFileFormat imageFormat, IEnumerable<Shape> shapes) {
			return CreateImage(imageFormat, shapes, 0, false, Color.White, -1);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		/// <param name="imageFormat">Specifies the format of the graphics file.</param>
		/// <param name="shapes">The shapes that should be drawn. If null/Nothing, the whole diagram area will be exported.</param>
		/// <param name="withBackground">Specifies whether the diagram's background should be exported to the graphics file.</param>
		public Image CreateImage(ImageFileFormat imageFormat, IEnumerable<Shape> shapes, bool withBackground) {
			return CreateImage(imageFormat, shapes, null, 0, withBackground, Color.White, -1);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		/// <param name="imageFormat">Specifies the format of the graphics file.</param>
		/// <param name="shapes">The shapes that should be drawn. If null/Nothing, the whole diagram area will be exported.</param>
		/// <param name="visibleLayerIds">Specifies the layers that should be visible.</param>
		/// <param name="withBackground">Specifies whether the diagram's background should be exported to the graphics file.</param>
		public Image CreateImage(ImageFileFormat imageFormat, IEnumerable<Shape> shapes, IEnumerable<int> visibleLayerIds, bool withBackground) {
			return CreateImage(imageFormat, shapes, visibleLayerIds, 0, withBackground, Color.White, -1);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		/// <param name="imageFormat">Specifies the format of the graphics file.</param>
		/// <param name="shapes">The shapes that should be drawn. If null/Nothing, the whole diagram area will be exported.</param>
		/// <param name="margin">Specifies the thickness of the margin around the exported diagram area.</param>
		public Image CreateImage(ImageFileFormat imageFormat, IEnumerable<Shape> shapes, int margin) {
			return CreateImage(imageFormat, shapes, null, margin, false, Color.White, -1);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		/// <param name="imageFormat">Specifies the format of the graphics file.</param>
		/// <param name="shapes">The shapes that should be drawn. If null/Nothing, the whole diagram area will be exported.</param>
		/// <param name="visibleLayerIds">Specifies the layers that should be visible.</param>
		/// <param name="margin">Specifies the thickness of the margin around the exported diagram area.</param>
		public Image CreateImage(ImageFileFormat imageFormat, IEnumerable<Shape> shapes, IEnumerable<int> visibleLayerIds, int margin) {
			return CreateImage(imageFormat, shapes, visibleLayerIds, margin, false, Color.White, -1);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes (plus margin on each side) to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		/// <param name="imageFormat">Specifies the format of the graphics file.</param>
		/// <param name="shapes">The shapes that should be drawn. If null/Nothing, the whole diagram area will be exported.</param>
		/// <param name="margin">Specifies the thickness of the margin around the exported diagram area.</param>
		/// <param name="withBackground">Specifies whether the diagram's background should be exported to the graphics file.</param>
		/// <param name="backgroundColor">Specifies a color for the exported image's background. 
		/// If the diagram is exported with background, the diagram's background will be drawn over the specified background color.</param>
		public Image CreateImage(ImageFileFormat imageFormat, IEnumerable<Shape> shapes, int margin, bool withBackground, Color backgroundColor) {
			return CreateImage(imageFormat, shapes, null, margin, withBackground, backgroundColor, -1);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes (plus margin on each side) to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		/// <param name="imageFormat">Specifies the format of the graphics file.</param>
		/// <param name="shapes">The shapes that should be drawn. If null/Nothing, the whole diagram area will be exported.</param>
		/// <param name="visibleLayerIds">Specifies the layers that should be visible.</param>
		/// <param name="margin">Specifies the thickness of the margin around the exported diagram area.</param>
		/// <param name="withBackground">Specifies whether the diagram's background should be exported to the graphics file.</param>
		/// <param name="backgroundColor">Specifies a color for the exported image's background. 
		/// If the diagram is exported with background, the diagram's background will be drawn over the specified background color.</param>
		public Image CreateImage(ImageFileFormat imageFormat, IEnumerable<Shape> shapes, IEnumerable<int> visibleLayerIds, int margin, bool withBackground, Color backgroundColor) {
			return CreateImage(imageFormat, shapes, visibleLayerIds, margin, withBackground, backgroundColor, -1);
		}


		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes (plus margin on each side) to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		/// <param name="imageFormat">Specifies the format of the graphics file.</param>
		/// <param name="shapes">The shapes that should be drawn. If null/Nothing, the whole diagram area will be exported.</param>
		/// <param name="margin">Specifies the thickness of the margin around the exported diagram area.</param>
		/// <param name="withBackground">Specifies whether the diagram's background should be exported to the graphics file.</param>
		/// <param name="backgroundColor">Specifies a color for the exported image's background. 
		/// If the diagram is exported with background, the diagram's background will be drawn over the specified background color.</param>
		/// <param name="dpi">Specifies the resolution for the export file. Only applies to pixel based image file formats.</param>
		public Image CreateImage(ImageFileFormat imageFormat, IEnumerable<Shape> shapes, int margin, bool withBackground, Color backgroundColor, int dpi) {
			return CreateImage(imageFormat, shapes, null, margin, withBackground, backgroundColor, dpi);
		}

		
		/// <summary>
		/// Exports the part of the diagram that encloses all given shapes (plus margin on each side) to an image of the given format.
		/// Pass null/Nothing for Parameter shapes in order to expor the whole diagram area.
		/// </summary>
		/// <param name="imageFormat">Specifies the format of the graphics file.</param>
		/// <param name="shapes">The shapes that should be drawn. If null/Nothing, the whole diagram area will be exported.</param>
		/// <param name="visibleLayerIds">Specifies the visible layers.</param>
		/// <param name="margin">Specifies the thickness of the margin around the exported diagram area.</param>
		/// <param name="withBackground">Specifies whether the diagram's background should be exported to the graphics file.</param>
		/// <param name="backgroundColor">Specifies a color for the exported image's background. 
		/// If the diagram is exported with background, the diagram's background will be drawn over the specified background color.</param>
		/// <param name="dpi">Specifies the resolution for the export file. Only applies to pixel based image file formats.</param>
		public Image CreateImage(ImageFileFormat imageFormat, IEnumerable<Shape> shapes, IEnumerable<int> visibleLayerIds, int margin, bool withBackground, Color backgroundColor, int dpi) {
			Image result = null;
			
			// Split up the visible layers (shared layers and exclusive layers)
			LayerIds visibleLayers;
			HashCollection<int> visibleHomeLayers;
			if (visibleLayerIds != null) {
				visibleLayers = Layer.ConvertToLayerIds(visibleLayerIds);
				visibleHomeLayers = new HashCollection<int>(visibleLayerIds);
			} else {
				visibleLayers = LayerIds.All;
				visibleHomeLayers = new HashCollection<int>(LayerHelper.GetAllLayerIds(Layers));
			}

			// Get/Create info graphics
			bool disposeInfoGfx;
			Graphics infoGraphics;
			if (DisplayService != null) {
				infoGraphics = DisplayService.InfoGraphics;
				disposeInfoGfx = false;
			} else {
				infoGraphics = Graphics.FromHwnd(IntPtr.Zero);
				disposeInfoGfx = true;
			}

			try {
				// If dpi value is not valid, get current dpi from display service
				if (dpi <= 0) 
					dpi = (int)Math.Round((infoGraphics.DpiX + infoGraphics.DpiY) / 2f);

				// Get bounding rectangle around the given shapes
				Rectangle imageBounds = Rectangle.Empty;
				if (shapes == null) {
					imageBounds.X = imageBounds.Y = 0;
					imageBounds.Width = Width;
					imageBounds.Height = Height;
				} else {
					int left, top, right, bottom;
					left = top = int.MaxValue;
					right = bottom = int.MinValue;
					// Calculate the bounding rectangle of the given shapes
					Rectangle boundingRect = Rectangle.Empty;
					foreach (Shape shape in shapes) {
						if (IsShapeVisible(shape, visibleLayers, visibleHomeLayers)) {
							boundingRect = shape.GetBoundingRectangle(true);
							if (boundingRect.Left < left) left = boundingRect.Left;
							if (boundingRect.Top < top) top = boundingRect.Top;
							if (boundingRect.Right > right) right = boundingRect.Right;
							if (boundingRect.Bottom > bottom) bottom = boundingRect.Bottom;
						}
					}
					if (Geometry.IsValid(left, top, right, bottom))
						imageBounds = Rectangle.FromLTRB(left, top, right, bottom);
				}
				imageBounds.Inflate(margin, margin);
				imageBounds.Width += 1;
				imageBounds.Height += 1;

				bool originalQualitySetting = this.HighQualityRendering;
				HighQualityRendering = true;
				UpdateBackgroundBrush();

				float scaleX = 1, scaleY = 1;
				switch (imageFormat) {
					case ImageFileFormat.Svg:
						throw new NotImplementedException();

					case ImageFileFormat.Emf:
					case ImageFileFormat.EmfPlus:
						// Create MetaFile and graphics context
						IntPtr hdc = infoGraphics.GetHdc();
						try {
							Rectangle bounds = Rectangle.Empty;
							bounds.Size = imageBounds.Size;
							result = new Metafile(hdc, bounds, MetafileFrameUnit.Pixel,
												(imageFormat == ImageFileFormat.Emf) ? EmfType.EmfOnly : EmfType.EmfPlusDual,
												Name);
						} finally {
							infoGraphics.ReleaseHdc(hdc);
						}
						break;

					case ImageFileFormat.Bmp:
					case ImageFileFormat.Gif:
					case ImageFileFormat.Jpeg:
					case ImageFileFormat.Png:
					case ImageFileFormat.Tiff:
						int imgWidth = imageBounds.Width;
						int imgHeight = imageBounds.Height;
						if (dpi > 0 && dpi != infoGraphics.DpiX || dpi != infoGraphics.DpiY) {
							scaleX = dpi / infoGraphics.DpiX;
							scaleY = dpi / infoGraphics.DpiY;
							imgWidth = (int)Math.Round(scaleX * imageBounds.Width);
							imgHeight = (int)Math.Round(scaleY * imageBounds.Height);
						}
						try {
							result = new Bitmap(Math.Max(1, imgWidth), Math.Max(1, imgHeight), PixelFormat.Format32bppArgb);
							((Bitmap)result).SetResolution(dpi, dpi);
						} catch (OutOfMemoryException) {
							// Maximum number of pixels GDI can handle for Bitmaps.
							if (imgWidth * imgHeight > Diagram.MaxBitmapPixelCount)
								throw new NotSupportedException(string.Format(
									"The diagram size of {0} x {1} at {2} dpi exceeds maximum size for bitmap pictures of {3} pixels.",
									imgWidth, imgHeight, dpi, Diagram.MaxBitmapPixelCount
								));
						}
						break;

					default:
						throw new NShapeUnsupportedValueException(typeof(ImageFileFormat), imageFormat);
				}

				// Draw diagram
				using (Graphics gfx = Graphics.FromImage(result)) {
					GdiHelpers.ApplyGraphicsSettings(gfx, RenderingQuality.MaximumQuality);

					// Fill background with background color
					if (backgroundColor.A < 255) {
						if (imageFormat == ImageFileFormat.Bmp || imageFormat == ImageFileFormat.Jpeg) {
							// For image formats that do not support transparency, fill background with the RGB part of 
							// the given backgropund color
							gfx.Clear(Color.FromArgb(255, backgroundColor));
						} else if (backgroundColor.A > 0) {
							// Skip filling background for meta files if transparency is 100%: 
							// Filling Background with Color.Transparent causes graphical glitches with many applications
							gfx.Clear(backgroundColor);
						}
					} else {
						// Graphics.Clear() does not work as expected for classic EMF (fills only the top left pixel
						// instead of the whole graphics context). 
						if (imageFormat == ImageFileFormat.Emf) {
							using (SolidBrush brush = new SolidBrush(backgroundColor))
								gfx.FillRectangle(brush, gfx.ClipBounds);
						} else gfx.Clear(backgroundColor);
					}

					// Transform graphics (if necessary)
					gfx.TranslateTransform(-imageBounds.X, -imageBounds.Y, MatrixOrder.Prepend);
					if (scaleX != 1 || scaleY != 1) gfx.ScaleTransform(scaleX, scaleY, MatrixOrder.Append);

					// Draw diagram background
					if (withBackground) DrawBackground(gfx, imageBounds);
					// Draw diagram shapes
					if (shapes == null) {
						foreach (Shape shape in _diagramShapes.BottomUp)
							if (IsShapeVisible(shape, visibleLayers, visibleHomeLayers))
								shape.Draw(gfx);
					} else {
						// Add shapes to ShapeCollection (in order to maintain zOrder while drawing)
						int cnt = (shapes is ICollection) ? ((ICollection)shapes).Count : -1;
						ShapeCollection shapeCollection = new ShapeCollection(cnt);
						foreach (Shape s in shapes) {
							// Sort out shapes that are invisible due to invisible layers.
							if (!IsShapeVisible(s, visibleLayers, visibleHomeLayers)) 
								continue;
							// Sort out duplicate references to shapes (as they can occur in the result of Diagram.FindShapes())
							if (shapeCollection.Contains(s)) 
								continue;
							shapeCollection.Add(s, s.ZOrder);
						}
						// Draw shapes
						foreach (Shape shape in shapeCollection.BottomUp) 
							shape.Draw(gfx);
						shapeCollection.Clear();
					}
					// Reset transformation
					gfx.ResetTransform();
				}
				// Restore original graphics settings
				HighQualityRendering = originalQualitySetting;
				UpdateBackgroundBrush();

				return result;
			} finally {
				if (disposeInfoGfx) 
					GdiHelpers.DisposeObject(ref infoGraphics);
			}
		}


		/// <summary>
		/// Draws the diagram background.
		/// </summary>
		/// <param name="graphics"></param>
		/// <param name="clipRectangle"></param>
		public void DrawBackground(Graphics graphics, Rectangle clipRectangle) {
			if (graphics == null) throw new ArgumentNullException(nameof(graphics));
			Rectangle bounds = Rectangle.Empty;
			bounds.X = Math.Max(0, clipRectangle.X);
			bounds.Y = Math.Max(0, clipRectangle.Y);
			bounds.Width = Math.Min(clipRectangle.Right, Width) - bounds.X;
			bounds.Height = Math.Min(clipRectangle.Bottom, Height) - bounds.Y;

			// draw diagram background color
			UpdateBackgroundBrush();
			//graphics.FillRectangle(colorBrush, clipRectangle);
			graphics.FillRectangle(_colorBrush, bounds);

			// draw diagram background image
			if (_isBackgroundImageVisible && !NamedImage.IsNullOrEmpty(_backImage)) {
				Rectangle diagramBounds = Rectangle.Empty;
				diagramBounds.Width = Width;
				diagramBounds.Height = Height;
				if (this._isFastRedrawEnabled && _backImageLowRes != null) {
					// Draw only the part of the image inside the cliprectangle.
					Rectangle imgClipBounds = Rectangle.Intersect(diagramBounds, clipRectangle);

					// Most layout modes will not stretch the image to the full resolution image's bounds, so we have to do it manually.
					float scaleFactor = GetLowResImageScaleFactor(_backImage.Image);

					// Get source and destination bounds according to the layout.
					//RectangleF imgSourceBounds = GdiHelpers.GetImageSourceBounds(_backImage.Image, _imageLayout, diagramBounds);
					//Rectangle imgDestinationBounds = GdiHelpers.GetImageDestinationBounds(_backImage.Image, _imageLayout, diagramBounds);
					RectangleF imgSourceBounds = GdiHelpers.GetImageSourceClipBounds(_backImage.Image, _imageLayout, diagramBounds, imgClipBounds);
					Rectangle imgDestinationBounds = GdiHelpers.GetImageDestinationClipBounds(_backImage.Image, _imageLayout, diagramBounds, imgClipBounds);
					imgSourceBounds.X *= scaleFactor;
					imgSourceBounds.Y *= scaleFactor;
					imgSourceBounds.Width *= scaleFactor;
					imgSourceBounds.Height *= scaleFactor;
					
					// Don't draw image if source width/height are <= 0!
					if (!(imgSourceBounds.Width <= 0 || imgSourceBounds.Height <= 0))
						graphics.DrawImage(_backImageLowRes, imgDestinationBounds, imgSourceBounds.X, imgSourceBounds.Y, imgSourceBounds.Width, imgSourceBounds.Height, GraphicsUnit.Pixel, ImageAttribs);
				} else {
					RectangleF imgSourceBounds = GdiHelpers.GetImageSourceBounds(_backImage.Image, _imageLayout, diagramBounds);
					Rectangle imgDestinationBounds = GdiHelpers.GetImageDestinationBounds(_backImage.Image, _imageLayout, diagramBounds);

					// Draw only the part of the image inside the cliprectangle.
					Rectangle imgClipBounds = Rectangle.Intersect(Rectangle.Round(imgSourceBounds), clipRectangle);
					GdiHelpers.DrawImage(graphics, _backImage.Image, ImageAttribs, _imageLayout, diagramBounds, imgClipBounds, 0, Geometry.InvalidPointF);
				}
			}
		}


		/// <summary>
		/// Draws the diagram shapes.
		/// </summary>
		public void DrawShapes(Graphics graphics, IEnumerable<int> visibleLayerIds, Rectangle clipRectangle) {
			// Split up visible layers in shaped layers and home layers for improved performance			
			HashCollection<int> visibleHomeLayers = new HashCollection<int>(visibleLayerIds);
			LayerIds visibleLayers = Layer.ConvertToLayerIds(visibleLayerIds);
			
			DrawShapes(graphics, visibleLayers, visibleHomeLayers, clipRectangle);
		}

	
		/// <summary>
		/// Draws the diagram shapes.
		/// </summary>
		internal void DrawShapes(Graphics graphics, LayerIds visibleLayers, HashCollection<int> visibleHomeLayers, Rectangle clipRectangle) {
			if (graphics == null) throw new ArgumentNullException(nameof(graphics));
			int x = clipRectangle.X;
			int y = clipRectangle.Y;
			int width = clipRectangle.Width;
			int height = clipRectangle.Height;

			foreach (Shape shape in _diagramShapes.BottomUp) {
				// Paint shape if it intersects with the clipping area and is visible
				if (IsShapeVisible(shape, visibleLayers, visibleHomeLayers)) {
					if (Geometry.RectangleIntersectsWithRectangle(shape.GetBoundingRectangle(false), x, y, width, height))
						shape.Draw(graphics);
				}
			}
		}
		
		#endregion


		/// <override></override>
		public override string ToString() {
			return Title;
		}


		/// <ToBeCompleted></ToBeCompleted>
		internal void OnShapeMoved(ShapeEventArgs e) {
			if (ShapeMoved != null) ShapeMoved(this, e);
		}


		/// <ToBeCompleted></ToBeCompleted>
		internal void OnShapeResized(ShapeEventArgs e) {
			if (ShapeResized!= null) ShapeResized(this, e);
		}


		/// <ToBeCompleted></ToBeCompleted>
		internal void OnShapeRotated(ShapeEventArgs e) {
			if (ShapeRotated != null) ShapeRotated(this, e);
		}


		internal void OnResized(EventArgs e) {
		    //if (displayService != null) displayService.NotifyBoundsChanged();
			if (Resized != null) Resized(this, e);
		}


		/// <summary>
		/// Specifies whether the background image (if set) will be excluded from rendering.
		/// </summary>
		internal bool IsFastRedrawEnabled {
			get { return _isFastRedrawEnabled; }
			set {
				if (_isFastRedrawEnabled != value) {
					_isFastRedrawEnabled = value;
					if (_displayService != null)
						_displayService.Invalidate(0, 0, Width, Height);
				}
			}
		}

		
		#region IEntity Members (Explicit implementation)

		/// <summary>
		/// The entity type name of <see cref="T:Dataweb.NShape.Diagram" />.
		/// </summary>
		public static string EntityTypeName {
			get { return _entityTypeName; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Diagram" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("Name", typeof(string));
			if (version >= 3) yield return new EntityFieldDefinition("Title", typeof(string));
			if (version >= 4) yield return new EntityFieldDefinition("SecurityDomain", typeof(char));
			yield return new EntityFieldDefinition("Width", typeof(int));
			yield return new EntityFieldDefinition("Height", typeof(int));
			yield return new EntityFieldDefinition("BackgroundColor", typeof(Color));
			yield return new EntityFieldDefinition("BackgroundGradientEndColor", typeof(Color));
			yield return new EntityFieldDefinition("BackgroundImageFileName", typeof(string));
			yield return new EntityFieldDefinition("BackgroundImage", typeof(Image));
			yield return new EntityFieldDefinition("ImageLayout", typeof(byte));
			yield return new EntityFieldDefinition("ImageGamma", typeof(float));
			yield return new EntityFieldDefinition("ImageTransparency", typeof(byte));
			yield return new EntityFieldDefinition("ImageGrayScale", typeof(bool));
			yield return new EntityFieldDefinition("ImageTransparentColor", typeof(int));
			if (version >= 7) 
				yield return new EntityFieldDefinition("ModelObject", typeof(object));

			yield return new EntityInnerObjectsDefinition("Layers", "Core.Layer",
				new string[] { "Id", "Name", "Title", "LowerVisibilityThreshold", "UpperVisibilityThreshold" },
				new Type[] { typeof(int), typeof(string), typeof(string), typeof(int), typeof(int) });
		}


		[CategoryGeneral()]
		object IEntity.Id {
			get { return _id; }
		}


		void IEntity.AssignId(object id) {
			if (id == null)
				throw new ArgumentNullException(nameof(id));
			if (this._id != null)
				throw new InvalidOperationException(string.Format("{0} has already a id.", GetType().Name));
			this._id = id;
		}


		void IEntity.LoadFields(IRepositoryReader reader, int version) {
			_name = reader.ReadString();
			if (version >= 3) _title = reader.ReadString();
			if (version >= 4) _securityDomainName = reader.ReadChar();
			_size.Width = reader.ReadInt32();
			_size.Height = reader.ReadInt32();
			_backColor = Color.FromArgb(reader.ReadInt32());
			_targetColor = Color.FromArgb(reader.ReadInt32());
			string imgName = reader.ReadString();
			Image img = reader.ReadImage();
			// Use setter in order to ensure the lowRes image exists when drawing
			if (img != null) BackgroundImage = new NamedImage(img, imgName);
			_imageLayout = (ImageLayoutMode)reader.ReadByte();
			_imageGamma = reader.ReadFloat();
			_imageTransparency = reader.ReadByte();
			_imageGrayScale = reader.ReadBool();
			_imageTransparentColor = Color.FromArgb(reader.ReadInt32());
			if (version >= 7) {
				_modelObject = reader.ReadDiagramModelObject();
				if (_modelObject != null) _modelObject.AttachDiagram(this);
			}
		}


		void IEntity.LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			Debug.Assert(propertyName == "Layers");
			Debug.Assert(_layers.Count == 0);
			reader.BeginReadInnerObjects();
			while (reader.BeginReadInnerObject()) {
				int layerId;
				if (version < 6)
					layerId = Layer.ConvertToLayerId((LayerIds)reader.ReadInt32());
				else
					layerId = reader.ReadInt32();
				string name = reader.ReadString();
				Layer l = new Layer(layerId, name);
				l.Title = reader.ReadString();
				l.LowerZoomThreshold = reader.ReadInt32();
				l.UpperZoomThreshold = reader.ReadInt32();
				reader.EndReadInnerObject();
				_layers.Add(l);
			}
			reader.EndReadInnerObjects();
		}


		void IEntity.SaveFields(IRepositoryWriter writer, int version) {
			writer.WriteString(_name);
			if (version >= 3) writer.WriteString(_title);
			if (version >= 4) writer.WriteChar(_securityDomainName);
			writer.WriteInt32(_size.Width);
			writer.WriteInt32(_size.Height);
			writer.WriteInt32(BackgroundColor.ToArgb());
			writer.WriteInt32(BackgroundGradientColor.ToArgb());
			if (NamedImage.IsNullOrEmpty(_backImage)) {
				writer.WriteString(string.Empty);
				writer.WriteImage(null);
			} else {
				writer.WriteString(_backImage.Name);
				object imgTag = _backImage.Image.Tag;
				_backImage.Image.Tag = _backImage.Name;
				writer.WriteImage(_backImage.Image);
				_backImage.Image.Tag = imgTag;
			}
			writer.WriteByte((byte)_imageLayout);
			writer.WriteFloat(_imageGamma);
			writer.WriteByte(_imageTransparency);
			writer.WriteBool(_imageGrayScale);
			writer.WriteInt32(_imageTransparentColor.ToArgb());
			if (version >= 7)
				writer.WriteDiagramModelObject(ModelObject);
		}


		void IEntity.SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			Debug.Assert(propertyName == "Layers");
			writer.BeginWriteInnerObjects();
			foreach (Layer l in _layers) {
				writer.BeginWriteInnerObject();
				if (version < 6)
					writer.WriteInt32((int)Layer.ConvertToLayerIds(l.LayerId));
				else 
					writer.WriteInt32((int)l.LayerId);
				writer.WriteString(l.Name);
				writer.WriteString(l.Title);
				writer.WriteInt32(l.LowerZoomThreshold);
				writer.WriteInt32(l.UpperZoomThreshold);
				writer.EndWriteInnerObject();
			}
			writer.EndWriteInnerObjects();
		}


		void IEntity.Delete(IRepositoryWriter writer, int version) {
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition)
					writer.DeleteInnerObjects();
			}
		}

		#endregion


		#region [Private] Properties

		/// <summary>
		/// Gets image attributes for special image effects (e.g. image opacity, grayscale mode, color keying).
		/// Returns null if no special effects have to be applied.
		/// </summary>
		ImageAttributes ImageAttribs {
			get {
				if (_imageAttribs == null) {
					// If the image attribute's parameters are all set to their default, we don't need them (which gives a nice performance plus).
					if (_imageGamma != 1 || _imageTransparency != 0 || _imageGrayScale || _imageTransparentColor.A != 0)
						_imageAttribs = GdiHelpers.GetImageAttributes(_imageLayout, _imageGamma, _imageTransparency, _imageGrayScale, false, _imageTransparentColor);
				}
				return _imageAttribs; 
			}
		}

		#endregion


		#region [Private Static] Methods

		/// <summary>
		/// Returns true if the shape is assigned at least to one of the visible shared layers.
		/// </summary>
		private static bool IsSupplementalLayerVisible(Shape shape, LayerIds visibleSupplementalLayers) {
			Debug.Assert(shape != null);
			if (visibleSupplementalLayers == LayerIds.None || shape.SupplementalLayers == LayerIds.None)
				return false;
			return ((shape.SupplementalLayers & visibleSupplementalLayers) == 0) ? false : true;
		}


		/// <summary>
		/// Returns true if the shape is assigned at least to one of the visible layers.
		/// </summary>
		/// <remarks>
		/// It's strongly recommended to use a collection type with a fast implementation of the Contains() method, 
		/// <see cref="T:Dataweb.NShape.Advanced.HashCollection´1" /> for example.
		/// </remarks>
		private static bool IsHomeLayerVisible(Shape shape, ICollection<int> visibleHomeLayerIds) {
			if (visibleHomeLayerIds.Count == 0)
				return false;
			// No home layer assigned -> visible regarding the home layers
			if (shape.HomeLayer == Layer.NoLayerId)
				return false;
			return visibleHomeLayerIds.Contains(shape.HomeLayer);
		}

		#endregion


		#region [Private] Methods

		private void InvalidateDrawCache() {
			GdiHelpers.DisposeObject(ref _imageAttribs);
			GdiHelpers.DisposeObject(ref _backImageLowRes);
			if (!NamedImage.IsNullOrEmpty(_backImage))
				if (_backImage.ImageFileExists)
					_backImage.Unload();
		}


		private void UpdateBackgroundBrush() {
			if (_colorBrush == null) {
				if (BackgroundGradientColor != BackgroundColor && _highQualityRendering) {
					_colorBrushBounds.Location = Point.Empty;
					_colorBrushBounds.Width = 100;
					_colorBrushBounds.Height = 100;
					_colorBrush = new LinearGradientBrush(_colorBrushBounds, BackgroundGradientColor, BackgroundColor, 45);
				} else _colorBrush = new SolidBrush(BackgroundColor);
			}
			if (_colorBrush is LinearGradientBrush && Size != _colorBrushBounds.Size) {
				LinearGradientBrush gradientBrush = (LinearGradientBrush)_colorBrush;
				_colorBrushBounds.Location = Point.Empty; 
				_colorBrushBounds.Width = Width;
				_colorBrushBounds.Height = Height;
				PointF center = PointF.Empty;
				center.X = _colorBrushBounds.X + (_colorBrushBounds.Width / 2f);
				center.Y = _colorBrushBounds.Y + (_colorBrushBounds.Height / 2f);
				GdiHelpers.TransformLinearGradientBrush(gradientBrush, 45, _colorBrushBounds, center, 0);
			}
		}


		// Create a low-quality texture brush that is shown while editing the diagram. 
		private void UpdateFastRedrawBackImage() {
			if (_backImageLowRes == null && !NamedImage.IsNullOrEmpty(BackgroundImage)) {
				Size brushImageSize = GetLowResImageSize(BackgroundImage.Image);
				if (brushImageSize != Geometry.InvalidSize)
					_backImageLowRes = _backImage.Image.GetThumbnailImage(brushImageSize.Width, brushImageSize.Height, null, IntPtr.Zero);
			}
		}


		/// <summary>
		/// Returns the size of the low res image.
		/// If a low res image is not needed, Geometry.InvalidSize will be returned.
		/// </summary>
		private Size GetLowResImageSize(Image image) {
			Size result = Geometry.InvalidSize;

			float scaleFactor = GetLowResImageScaleFactor(image);
			if (scaleFactor < 1) {
				result.Width = Math.Max(1, (int)Math.Round(image.Width * scaleFactor));
				result.Height = Math.Max(1, (int)Math.Round(image.Height * scaleFactor));
			}
			//double highestRatio = Math.Max((double)fastRedrawImageSize / (double)image.Width, (double)fastRedrawImageSize / (double)image.Height);
			//float scaleX = (float)Math.Round(highestRatio, 6);
			//float scaleY = (float)Math.Round(highestRatio, 6);
			//if (scaleX <= 0.9 || scaleY <= 0.9) {
			//    result.Width = Math.Max(1, (int)Math.Round(image.Width * scaleX));
			//    result.Height = Math.Max(1, (int)Math.Round(image.Height * scaleY));
			//}
			
			return result;
		}


		private float GetLowResImageScaleFactor(Image image) {
			double highestRatio = Math.Max((double)_fastRedrawImageSize / (double)image.Width, (double)_fastRedrawImageSize / (double)image.Height);
			return (highestRatio < 0.9) ? (float)Math.Round(highestRatio, 6) : 1f;
		}

		#endregion


		#region Fields

		/// <summary>Defines the cell size of the diagram's spatial index.</summary>
		public const int DefaultIndexCellSize = 100;

		/// <summary>
		/// Defines the cell size of the diagram's spatial index.
		/// This property is meant as a 'per application' (or at least a 'per project') setting:
		/// Do not modify this property after loading or creating diagrams, otherwise the spatial 
		/// indexes of all existing diagrams will be corrupted!
		/// </summary>
		/// <remarks>
		/// The spatial index' cell size affects the number of shapes per cell but also the 
		/// number of cells a shape occupies.
		/// When working with large diagrams (~ 50000 x 50000 or larger) and large shapes, 
		/// increasing the cell size from 100 to 1000 improves performance drastically.
		/// </remarks>
		public static int IndexCellSize { get; set; } = DefaultIndexCellSize;

		private const string _entityTypeName = "Core.Diagram";
		private const int _expectedShapes = 10000;
		private const int _fastRedrawImageSize = 1024;

		private object _id;
		private string _title;
		private string _name;
		private IDiagramModelObject _modelObject;
		private IDisplayService _displayService;
		private LayerCollection _layers = null;
		private DiagramShapeCollection _diagramShapes = null;
		private Size _size = new Size(1, 1);
		private char _securityDomainName = 'A';

		// Rendering stuff
		private Color _backColor = Color.WhiteSmoke;
		private Color _targetColor = Color.White;
		private bool _highQualityRendering = true;
		private bool _isBackgroundImageVisible = true;
		// Background image stuff
		private NamedImage _backImage;
		private Image _backImageLowRes;
		private ImageLayoutMode _imageLayout;
		private float _imageGamma = 1.0f;
		private byte _imageTransparency = 0;
		private bool _imageGrayScale = false;
		private Color _imageTransparentColor = Color.Empty;
		private bool _isFastRedrawEnabled = false;
		// Drawing and Painting stuff
		private Brush _colorBrush = null;					// Brush for painting the diagram's background color
		private ImageAttributes _imageAttribs = null; // ImageAttributes for drawing the background image
		private Rectangle _colorBrushBounds = Rectangle.Empty;
		
		// Buffers
		private List<Shape> _shapeBuffer = new List<Shape>();

		#endregion

	}

}

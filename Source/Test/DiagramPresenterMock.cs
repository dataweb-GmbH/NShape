using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;

namespace NShapeTest {


	public class DiagramPresenterMock : IDiagramPresenter, IDisplayService {

		public DiagramPresenterMock(string testName) {
			TestName = testName;
		}


		~DiagramPresenterMock() {
			if (_InfoGraphics != null) {
				_InfoGraphics.Dispose();
				_InfoGraphics = null;
			}
		}


		public string TestName { get; private set; }


		#region IDisplayService Interface

		public Graphics InfoGraphics => _InfoGraphics;

		public IFillStyle HintBackgroundStyle => Project?.Design.FillStyles.Blue;

		public ILineStyle HintForegroundStyle => Project?.Design.LineStyles.Normal;

		public void Invalidate(int x, int y, int width, int height) {
			// ...
		}

		public void Invalidate(Rectangle rectangle) {
			// ...
		}

		public void NotifyBoundsChanged() {
			// ...
		}

		#endregion


		#region IDiagramPresenter Interface

		public DiagramSetController DiagramSetController { get; set; }

		public Project Project { get; set; }

		public Diagram Diagram {
			get { return _Diagram; }
			set {
				if (_Diagram != value) {
					if (_Diagram != null) {
						_Diagram.DisplayService = null;
						try { 
							_Image.Save(ImageDirectory, System.Drawing.Imaging.ImageFormat.Png); 
						} catch (Exception) { /* Ignore errors while saving */ }
						GdiHelpers.DisposeObject(ref _Image);
					}
					_Diagram = value;
					if (_Diagram != null) {
						_Image = new NamedImage(
							new Bitmap(_Diagram.Width, _Diagram.Height), 
							Path.Combine(ImageDirectory, ImageFileName), 
							Path.GetFileNameWithoutExtension(ImageFileName)
						);
						_Diagram.DisplayService = this;
					}
				}
			}
		}

		public Tool ActiveTool { get; set; }

		public IDisplayService DisplayService => this;

		public IShapeCollection SelectedShapes { get { return _SelectedShapes; } }

		public LayerIds ActiveLayers => throw new NotImplementedException();

		public ICollection<int> ActiveLayerIds => throw new NotImplementedException();

		public LayerIds HiddenLayers => throw new NotImplementedException();

		public ICollection<int> HiddenLayerIds => throw new NotImplementedException();

		public Font Font => throw new NotImplementedException();

		public bool HighQualityRendering => true;

		public int SnapDistance { get; set; } = 5;
		public bool SnapToGrid { get; set; } = true;
		public int GridSize { get; set; } = 20;
		public bool IsGridVisible { get; set; } = true;
		public ControlPointShape ResizeGripShape { get; set; } = ControlPointShape.Square;
		public ControlPointShape ConnectionPointShape { get; set; } = ControlPointShape.Circle;
		public int GripSize { get; set; } = 3;

		public int ZoomedGripSize => GripSize;

		public int ZoomLevel { get => 100; set { } }
		public int ZoomLevelHD { get => 100 * 100; set { } }

		public float ZoomFactor => 1;

		public bool Capture { get; set; }

		public int MinRotateRange => 30;

		public bool InvokeRequired => throw new NotImplementedException();

		public event EventHandler ShapesSelected;
		public event EventHandler<DiagramPresenterShapeClickEventArgs> ShapeClick;
		public event EventHandler<DiagramPresenterShapeClickEventArgs> ShapeDoubleClick;
		public event EventHandler<DiagramPresenterShapesEventArgs> ShapesInserted;
		public event EventHandler<DiagramPresenterShapesEventArgs> ShapesRemoved;
		public event EventHandler<DiagramPresenterShapeEventArgs> ShapeMoved;
		public event EventHandler<DiagramPresenterShapeEventArgs> ShapeResized;
		public event EventHandler<DiagramPresenterShapeEventArgs> ShapeRotated;
		public event EventHandler DiagramChanging;
		public event EventHandler DiagramChanged;
		public event EventHandler<LayersEventArgs> LayerVisibilityChanged;
		public event EventHandler<LayersEventArgs> ActiveLayersChanged;
		public event EventHandler ZoomChanging;
		public event EventHandler ZoomChanged;
		public event EventHandler<UserMessageEventArgs> UserMessage;

		public IAsyncResult BeginInvoke(Delegate method, object[] args) {
			throw new NotImplementedException();
		}

		public void CloseCaptionEditor(bool applyChanges) {
			throw new NotImplementedException();
		}

		public void ControlToDiagram(int cX, int cY, out int dX, out int dY) {
			dX = cX;
			dY = cY;
		}

		public void ControlToDiagram(Point cPt, out Point dPt) {
			dPt = cPt;
		}

		public void ControlToDiagram(Rectangle cRect, out Rectangle dRect) {
			dRect = cRect;
		}

		public void ControlToDiagram(Size cSize, out Size dSize) {
			dSize = cSize;
		}

		public void ControlToDiagram(int cDistance, out int dDistance) {
			dDistance = cDistance;
		}

		public void Copy() {
			throw new NotImplementedException();
		}

		public void Cut() {
			throw new NotImplementedException();
		}

		public void DeleteShape(Shape shape, bool withModelObjects) {
			throw new NotImplementedException();
		}

		public void DeleteShapes(IEnumerable<Shape> shapes, bool withModelObjects) {
			throw new NotImplementedException();
		}

		public void DiagramToControl(int dX, int dY, out int cX, out int cY) {
			cX = dX;
			cY = dY;
		}

		public void DiagramToControl(Point dPt, out Point cPt) {
			cPt = dPt;
		}

		public void DiagramToControl(Rectangle dRect, out Rectangle cRect) {
			cRect = dRect;
		}

		public void DiagramToControl(int dDistance, out int cDistance) {
			cDistance = dDistance;
		}

		public void DiagramToControl(Size dSize, out Size cSize) {
			cSize = dSize;
		}

		public void DrawAnglePreview(Point center, Point mousePos, int cursorId, int startAngle, int sweepAngle) {
			// ...
		}

		public void DrawCaptionBounds(IndicatorDrawMode drawMode, ICaptionedShape shape, int captionIndex) {
			// ...
		}

		public void DrawConnectionPoint(IndicatorDrawMode drawMode, Shape shape, ControlPointId pointId) {
			// ...
		}

		public void DrawLine(Point a, Point b) {
			// ...
		}

		public void DrawResizeGrip(IndicatorDrawMode drawMode, Shape shape, ControlPointId pointId) {
			// ...
		}

		public void DrawRotateGrip(IndicatorDrawMode drawMode, Shape shape, ControlPointId pointId) {
			// ...
		}

		public void DrawSelectionFrame(Rectangle frameRect) {
			// ...
		}


		public void Draw(string imageFileName = null) {
			Rectangle imageBounds = new Rectangle(0, 0, _Image.Width, _Image.Height);
			using (Graphics gfx = Graphics.FromImage(_Image.Image)) {
				Diagram.DrawBackground(gfx, imageBounds);
				Diagram.DrawShapes(gfx, EnumerationHelper.Empty<int>(), imageBounds);
				if (ActiveTool != null)
					ActiveTool.Draw(this);
			}
			string imageDir = Path.GetDirectoryName(_Image.FilePath);
			_Image.Save(imageDir, System.Drawing.Imaging.ImageFormat.Png);
			
			if (!string.IsNullOrEmpty(imageFileName)) {
				string newImagePath = Path.Combine(imageDir, imageFileName);
				if (File.Exists(newImagePath))
					File.Delete(newImagePath);
				File.Move(_Image.FilePath, newImagePath);
			}
		}


		public void DrawShape(Shape shape) {
			if (_Image != null) {
				using (Graphics gfx = Graphics.FromImage(_Image.Image))
					shape.Draw(gfx);
			}
		}

		public void DrawShapeOutline(IndicatorDrawMode drawMode, Shape shape) {
			if (_Image != null) {
				using (Graphics gfx = Graphics.FromImage(_Image.Image)) {
					switch (drawMode) {
						case IndicatorDrawMode.Deactivated:
							shape.DrawOutline(gfx, Pens.Gray);
							break;
						case IndicatorDrawMode.Highlighted:
							shape.DrawOutline(gfx, Pens.DarkRed);
							break;
						case IndicatorDrawMode.Normal:
							shape.DrawOutline(gfx, Pens.DarkGreen);
							break;
						default: throw new NotImplementedException();
					}
				}
			}
		}

		public void DrawShapes(IEnumerable<Shape> shapes) {
			if (_Image != null) {
				using (Graphics gfx = Graphics.FromImage(_Image.Image)) {
					foreach (Shape shape in shapes)
						shape.Draw(gfx);
				}
			}
		}

		public void DrawSnapIndicators(Shape shape) {
			// ...
		}

		public object EndInvoke(IAsyncResult result) {
			throw new NotImplementedException();
		}

		public void EnsureVisible(int x, int y) {
			throw new NotImplementedException();
		}

		public void EnsureVisible(Shape shape) {
			throw new NotImplementedException();
		}

		public void EnsureVisible(Rectangle viewArea) {
			throw new NotImplementedException();
		}

		public IEnumerable<int> GetVisibleLayerIds() {
			throw new NotImplementedException();
		}

		public void InsertShape(Shape shape) {
			DiagramSetController.InsertShape(Diagram, shape, LayerIds.None, false);
		}

		public void InsertShapes(IEnumerable<Shape> shapes) {
			DiagramSetController.InsertShapes(Diagram, shapes, Layer.NoLayerId, LayerIds.None, false);
		}

		public void InvalidateDiagram(Rectangle rect) {
			// ...
		}

		public void InvalidateDiagram(int left, int top, int width, int height) {
			// ...
		}

		public void InvalidateGrips(Shape shape, ControlPointCapabilities controlPointCapability) {
			// ...
		}

		public void InvalidateGrips(IEnumerable<Shape> shapes, ControlPointCapabilities controlPointCapability) {
			// ...
		}

		public void InvalidateSnapIndicators(Shape preview) {
			// ...
		}

		public object Invoke(Delegate method, object[] args) {
			throw new NotImplementedException();
		}

		public bool IsLayerActive(LayerIds layerId) {
			return true;
		}

		public bool IsLayerActive(int layerId) {
			return true;
		}

		public bool IsLayerActive(IEnumerable<int> layerIds) {
			return true;
		}

		public bool IsLayerVisible(LayerIds layerId) {
			return true;
		}

		public bool IsLayerVisible(int layerId) {
			return true;
		}

		public bool IsLayerVisible(IEnumerable<int> layerIds) {
			return true;
		}

		public bool IsLayerVisible(int homeLayer, LayerIds supplementalLayers) {
			return true;
		}

		public void OpenCaptionEditor(ICaptionedShape shape, int x, int y) {
			throw new NotImplementedException();
		}

		public void OpenCaptionEditor(ICaptionedShape shape, int labelIndex) {
			throw new NotImplementedException();
		}

		public void OpenCaptionEditor(ICaptionedShape shape, int labelIndex, string newText) {
			throw new NotImplementedException();
		}

		public void Paste() {
			throw new NotImplementedException();
		}

		public void ResetTransformation() {
			// ...
		}

		public void RestoreTransformation() {
			// ..
		}

		public void ResumeUpdate() {
			// ...
		}

		public void ScreenToDiagram(Point sPt, out Point dPt) {
			dPt = sPt;
		}

		public void ScreenToDiagram(Rectangle sRect, out Rectangle dRect) {
			dRect = sRect;
		}

		public void SelectAll() {
			if (Diagram != null) {
				UnselectAll();
				SelectedShapes.AddRange(Diagram.Shapes);
			}
		}

		public void SelectShape(Shape shape) {
			SelectShape(shape, false);
		}

		public void SelectShape(Shape shape, bool addToSelection) {
			if (Diagram != null) {
				if (!addToSelection)
					UnselectAll();
				if (!_SelectedShapes.Contains(shape))
					_SelectedShapes.Add(shape);
			}
		}

		public void SelectShapes(IEnumerable<Shape> shapes, bool addToSelection) {
			if (Diagram != null) {
				if (!addToSelection)
					UnselectAll();
				foreach (Shape s in shapes)
					if (!_SelectedShapes.Contains(s))
						_SelectedShapes.Add(s);
			}
		}

		public void SelectShapes(Rectangle area, bool addToSelection) {
			SelectShapes(Diagram.Shapes.FindShapes(area.X, area.Y, area.Width, area.Height, true), addToSelection);
		}

		public void SetCursor(int cursorId) {
			// ...
		}

		public void SetLayerActive(int layerId, bool active) {
			throw new NotImplementedException();
		}

		public void SetLayerActive(IEnumerable<int> layerIds, bool active) {
			throw new NotImplementedException();
		}

		public void SetLayerActive(IEnumerable<Layer> layers, bool active) {
			throw new NotImplementedException();
		}

		public void SetLayerActive(LayerIds layers, bool active) {
			throw new NotImplementedException();
		}

		public void SetLayerVisibility(int layerId, bool visible) {
			throw new NotImplementedException();
		}

		public void SetLayerVisibility(IEnumerable<int> layerIds, bool visible) {
			throw new NotImplementedException();
		}

		public void SetLayerVisibility(IEnumerable<Layer> layers, bool visible) {
			throw new NotImplementedException();
		}

		public void SetLayerVisibility(LayerIds layers, bool visible) {
			throw new NotImplementedException();
		}

		public void SuspendUpdate() {
			// ...
		}

		public void UnselectAll() {
			_SelectedShapes.Clear();
		}

		public void UnselectShape(Shape shape) {
			_SelectedShapes.Remove(shape);
		}

		public void Update() {
			// ...
		}

		#endregion

		
		private string ImageDirectory {
			get { return Path.GetTempPath(); }
		}


		private string ImageFileName {
			get { return $"{TestName}.png"; }
		}


		private ShapeCollection _SelectedShapes = new ShapeCollection();
		private Graphics _InfoGraphics = Graphics.FromHwnd(IntPtr.Zero);

		private Diagram _Diagram;
		private NamedImage _Image;

	}

}

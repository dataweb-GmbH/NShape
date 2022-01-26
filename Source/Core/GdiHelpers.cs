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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Security.Cryptography;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Defines the rendering quality of a <see cref="T:Dataweb.NShape.Controllers.IDiagramPresenters" />.
	/// </summary>
	public enum RenderingQuality {
		/// <summary>Best rendering Quality, lowest rendering speed.</summary>
		MaximumQuality,
		/// <summary>High quality rendering with acceptable performance loss.</summary>
		HighQuality,
		/// <summary>Rendering with system default settings.</summary>
		DefaultQuality,
		/// <summary>Low Quality, high rendering speed.</summary>
		LowQuality
	}


	/// <summary>
	/// Used for storing the main settings for rendering quality.
	/// </summary>
	internal class GraphicsSettings {

		public GraphicsSettings() {
			CompositingMode = CompositingMode.SourceOver;
			CompositingQuality = CompositingQuality.AssumeLinear;
			InterpolationMode = InterpolationMode.NearestNeighbor;
			PixelOffsetMode = PixelOffsetMode.None;
			SmoothingMode = SmoothingMode.None;
			TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
		}

		public GraphicsSettings(Graphics gfx) {
			GetGraphicsSettings(gfx);
		}

		public CompositingMode CompositingMode { get; set; }

		public CompositingQuality CompositingQuality { get; set; }

		public InterpolationMode InterpolationMode { get; set; }

		public PixelOffsetMode PixelOffsetMode { get; set; }

		public SmoothingMode SmoothingMode { get; set; }

		public TextRenderingHint TextRenderingHint { get; set; }

		public void GetGraphicsSettings(Graphics gfx) {
			CompositingMode = gfx.CompositingMode;
			CompositingQuality = gfx.CompositingQuality;
			InterpolationMode = gfx.InterpolationMode;
			PixelOffsetMode = gfx.PixelOffsetMode;
			SmoothingMode = gfx.SmoothingMode;
			TextRenderingHint = gfx.TextRenderingHint;
		}

		public void SetGraphicsSettings(Graphics gfx) {
			gfx.CompositingMode = CompositingMode;
			gfx.CompositingQuality = CompositingQuality;
			gfx.InterpolationMode = InterpolationMode;
			gfx.PixelOffsetMode = PixelOffsetMode;
			gfx.SmoothingMode = SmoothingMode;
			gfx.TextRenderingHint = TextRenderingHint;
		}

		public static void SetFastestGraphicsSettings(Graphics gfx) {
			gfx.CompositingMode = CompositingMode.SourceOver;
			gfx.CompositingQuality = CompositingQuality.AssumeLinear;
			gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
			gfx.PixelOffsetMode = PixelOffsetMode.None;
			gfx.SmoothingMode = SmoothingMode.None;
			gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
		}

	}


	/// <summary>
	/// A helper class providing methods mainly for setting drawing quality of graphics context and drawing images.
	/// </summary>
	internal static class GdiHelpers {

		/// <summary>
		/// If the given object exists, it will be disposed and set to null/Nothing.
		/// </summary>
		public static void DisposeObject<T>(ref T disposableObject) where T : class, IDisposable {
			if (disposableObject != null) {
				disposableObject.Dispose();
				disposableObject = null;
			}
		}


		#region Methods for drawing geometric primitives

		/// <summary>
		/// Visualizing points - used for debugging purposes
		/// </summary>
		public static void DrawPoint(Graphics gfx, Pen pen, PointF point, int radius) {
			DrawPoint(gfx, pen, point.X, point.Y, radius);
		}


		/// <summary>
		/// Visualizing points - used for debugging purposes
		/// </summary>
		public static void DrawPoint(Graphics gfx, Pen pen, float x, float y, int radius) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (pen == null) throw new ArgumentNullException("pen");
			gfx.DrawLine(pen, x - radius, y, x + radius, y);
			gfx.DrawLine(pen, x, y - radius, x, y + radius);
		}


		/// <summary>
		/// Visualizing angles - used for debugging purposes
		/// </summary>
		public static void DrawAngle(Graphics gfx, Brush brush, PointF center, float angle, int radius) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (brush == null) throw new ArgumentNullException("brush");
			Rectangle rect = Rectangle.Empty;
			rect.X = (int)Math.Round(center.X - radius);
			rect.Y = (int)Math.Round(center.Y - radius);
			rect.Width = rect.Height = radius + radius;
			gfx.FillPie(brush, rect, 0, angle);
			gfx.DrawPie(Pens.White, rect, 0, angle);
		}


		/// <summary>
		/// Visualizing lines - used for debugging purposes
		/// </summary>
		public static void DrawLine(Graphics graphics, Pen pen, PointF p1, PointF p2) {
			float a, b, c;
			Geometry.CalcLine(p1.X, p1.Y, p2.X, p2.Y, out a, out b, out c);
			DrawLine(graphics, pen, a, b, c);
		}


		/// <summary>
		/// Visualizing lines - used for debugging purposes
		/// </summary>
		public static void DrawLine(Graphics graphics, Pen pen, float a, float b, float c) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (pen == null) throw new ArgumentNullException("pen");
			// Gleichung nach y auflösen, 2 Werte für X einsetzen und dann die zugehörigen y ausrechnen
			float x1 = 0, x2 = 0, y1 = 0, y2 = 0;
			if (b != 0) {
				x1 = -10000;
				x2 = 10000;
				y1 = (-(a * x1) - c) / b;
				y2 = (-(a * x2) - c) / b;
			}
			else if (a != 0) {
				y1 = -10000;
				y2 = 10000;
				x1 = (-(b * y1) - c) / a;
				x2 = (-(b * y2) - c) / a;
			}
			if ((a != 0 || b != 0) 
				&& !(float.IsNaN(x1) || float.IsNaN(x2) || float.IsNaN(y1) || float.IsNaN(y2))
				&& !(float.IsInfinity(x1) || float.IsInfinity(x2) || float.IsInfinity(y1) || float.IsInfinity(y2)))
				graphics.DrawLine(pen, x1, y1, x2, y2);
		}


		/// <summary>
		/// Visualizing angles - used for debugging purposes
		/// </summary>
		public static void DrawAngle(Graphics gfx, Brush brush, PointF center, float startAngle, float sweepAngle, int radius) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (brush == null) throw new ArgumentNullException("brush");
			Rectangle rect = Rectangle.Empty;
			rect.X = (int)Math.Round(center.X - radius);
			rect.Y = (int)Math.Round(center.Y - radius);
			rect.Width = rect.Height = radius + radius;
			gfx.FillPie(brush, rect, startAngle, sweepAngle);
			gfx.DrawPie(Pens.White, rect, startAngle, sweepAngle);
		}


		/// <summary>
		/// Visualizing rotated rectangles - used for debugging purposes
		/// </summary>
		public static void DrawRotatedRectangle(Graphics gfx, Pen pen, RectangleF rect, float angleDeg) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (pen == null) throw new ArgumentNullException("pen");
			PointF[] pts = new PointF[5] {
				new PointF(rect.Left, rect.Top),
				new PointF(rect.Right, rect.Top),
				new PointF(rect.Right, rect.Bottom),
				new PointF(rect.Left, rect.Bottom),
				new PointF(rect.Left, rect.Top) };
			PointF center = PointF.Empty;
			center.X = rect.Left + (rect.Width / 2f);
			center.Y = rect.Top + (rect.Height / 2f);
			_matrix.Reset();
			_matrix.RotateAt(angleDeg, center);
			_matrix.TransformPoints(pts);
			gfx.DrawLines(pen, pts);
		}


		/// <summary>
		/// Visualizing rotated ellipses - used for debugging purposes
		/// </summary>
		public static void DrawRotatedEllipse(Graphics gfx, Pen pen, float centerX, float centerY, float width, float height, float angleDeg) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (pen == null) throw new ArgumentNullException("pen");
			using (GraphicsPath path = new GraphicsPath()) {
				path.StartFigure();
				path.AddEllipse(centerX - width / 2f, centerY - height / 2f, width, height);

				PointF center = new PointF(centerX, centerY);
				_matrix.Reset();
				_matrix.RotateAt(angleDeg, center);
				path.Transform(_matrix);
				gfx.DrawPath(pen, path);
			}
		}

		#endregion


		#region Graphics settings

		/// <summary>
		/// Sets all parameters that affect rendering quality / rendering speed
		/// </summary>
		public static void ApplyGraphicsSettings(Graphics graphics, RenderingQuality renderingQuality) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			switch (renderingQuality) {
				case RenderingQuality.MaximumQuality:
					graphics.CompositingQuality = CompositingQuality.HighQuality;
					graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
					//graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;			// produces quite blurry results
					graphics.SmoothingMode = SmoothingMode.HighQuality;
					//graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;	// smoothed but blurry fonts
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;	// sharp but slightly chunky fonts
					break;

				case RenderingQuality.HighQuality:
					graphics.CompositingQuality = CompositingQuality.AssumeLinear;	// From MSDN: Slightly better but slightly slower than Default					
					graphics.InterpolationMode = InterpolationMode.High;
					graphics.SmoothingMode = SmoothingMode.HighQuality;				// antialiasing and nice font rendering
					//graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;	// smoothed but blurry fonts
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;	// sharp but slightly chunky fonts
					break;

				case RenderingQuality.LowQuality:
					// Fastest settings for drawing with transparency
					graphics.CompositingQuality = CompositingQuality.HighSpeed;
					graphics.InterpolationMode = InterpolationMode.Low;
					graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
					graphics.SmoothingMode = SmoothingMode.HighSpeed;
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
					break;

				case RenderingQuality.DefaultQuality:
				default:
					graphics.CompositingQuality = CompositingQuality.Default;
					graphics.InterpolationMode = InterpolationMode.Default;
					graphics.PixelOffsetMode = PixelOffsetMode.Default;
					graphics.SmoothingMode = SmoothingMode.Default;
					graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
					break;
			}
		}


		/// <summary>
		///  Copy all parameters that affect rendering quality / rendering speed from the given infoGraphics.
		/// </summary>
		public static void ApplyGraphicsSettings(Graphics graphics, Graphics infoGraphics) {
			if (graphics == null) throw new ArgumentNullException("graphics");
			if (infoGraphics == null) throw new ArgumentNullException("infoGraphics");
			graphics.SmoothingMode = infoGraphics.SmoothingMode;
			graphics.TextRenderingHint = infoGraphics.TextRenderingHint;
			graphics.InterpolationMode = infoGraphics.InterpolationMode;
			graphics.CompositingMode = infoGraphics.CompositingMode;
			graphics.CompositingQuality = infoGraphics.CompositingQuality;
			graphics.PixelOffsetMode = infoGraphics.PixelOffsetMode;
		}

		#endregion


		#region Drawing images

		/// <summary>
		/// Copies the given image. The given oldColor will be replaced by the given newColor (used for icon rendering).
		/// </summary>
		public static Bitmap GetIconBitmap(Bitmap sourceImg, Color oldBackgroundColor, Color newBackgroundColor) {
			if (sourceImg == null) throw new ArgumentNullException("sourceImg");
			Bitmap result = new Bitmap(sourceImg.Width, sourceImg.Height, PixelFormat.Format32bppPArgb);
			result.SetResolution(sourceImg.HorizontalResolution, sourceImg.VerticalResolution);
			using (Graphics gfx = Graphics.FromImage(result)) {
				using (ImageAttributes imgAttr = new ImageAttributes()) {
					ColorMap colorMap = new ColorMap();
					colorMap.OldColor = oldBackgroundColor;
					colorMap.NewColor = newBackgroundColor;
					imgAttr.SetRemapTable(new ColorMap[] { colorMap });

					gfx.Clear(Color.Transparent);
					gfx.DrawImage(sourceImg, new Rectangle(0, 0, result.Width, result.Height), 0, 0, sourceImg.Width, sourceImg.Height, GraphicsUnit.Pixel, imgAttr);
				}
			}
			return result;
		}


		/// <summary>
		/// Get a suitable GDI+ <see cref="T:System.Drawing.Drawing2D.WrapMode" /> out of the given NShapeImageLayout.
		/// </summary>
		public static WrapMode GetWrapMode(ImageLayoutMode imageLayout) {
			switch (imageLayout) {
				case ImageLayoutMode.Center:
				case ImageLayoutMode.Fit:
				case ImageLayoutMode.Original:
				case ImageLayoutMode.Stretch:
					return WrapMode.Clamp;
				case ImageLayoutMode.CenterTile:
				case ImageLayoutMode.Tile:
					return WrapMode.Tile;
				case ImageLayoutMode.FlipTile:
					return WrapMode.TileFlipXY;
				default: throw new NShapeUnsupportedValueException(imageLayout);
			}
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(IFillStyle fillStyle) {
			return GetImageAttributes(fillStyle.ImageLayout, fillStyle.ImageGammaCorrection, fillStyle.ImageTransparency, fillStyle.ConvertToGrayScale, false, Color.Empty);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(ImageLayoutMode imageLayout) {
			return GetImageAttributes(imageLayout, -1f, 0, false, false, Color.Empty);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(ImageLayoutMode imageLayout, bool forPreview) {
			return GetImageAttributes(imageLayout, -1f, 0, false, forPreview, Color.Empty);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(ImageLayoutMode imageLayout, Color transparentColor) {
			return GetImageAttributes(imageLayout, -1f, 0, false, false, transparentColor);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(ImageLayoutMode imageLayout, Color transparentColor, bool forPreview) {
			return GetImageAttributes(imageLayout, -1f, 0, false, forPreview, transparentColor);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(ImageLayoutMode imageLayout, float gamma, byte transparency, bool grayScale) {
			return GetImageAttributes(imageLayout, gamma, transparency, grayScale, false, Color.Empty);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(ImageLayoutMode imageLayout, float gamma, byte transparency, bool grayScale, bool forPreview) {
			return GetImageAttributes(imageLayout, gamma, transparency, grayScale, forPreview, Color.Empty);
		}


		/// <summary>
		/// Get ImageAttributes for drawing an images or creating a TextureBrush.
		/// </summary>
		public static ImageAttributes GetImageAttributes(ImageLayoutMode imageLayout, float gamma, byte transparency, bool grayScale, bool forPreview, Color transparentColor) {
			if (transparency < 0 || transparency > 100) throw new ArgumentOutOfRangeException("transparency");
			ImageAttributes imageAttribs = new ImageAttributes();
			//
			// Set WrapMode
			imageAttribs.SetWrapMode(GetWrapMode(imageLayout));
			//
			// Set Gamma correction
			if (gamma > 0) imageAttribs.SetGamma(gamma);
			//
			// Reset color matrix before applying effects
			ResetColorMatrix(_colorMatrix);
			// Add conversion to grayScale
			if (grayScale || (forPreview && Design.PreviewsAsGrayScale))
				ApplyGrayScale(_colorMatrix);
			// Add transparency
			float transparencyFactor = forPreview ? Design.GetPreviewTransparency(transparency) / 100f : transparency / 100f;
			if (transparencyFactor != 0) ApplyTransparency(_colorMatrix, transparencyFactor);
			// Apply color matrix
			imageAttribs.SetColorMatrix(_colorMatrix);
			//
			// Set color remap table
			if (transparentColor != Color.Empty) {
				_colorMaps[0].OldColor = transparentColor;
				_colorMaps[0].NewColor = Color.Transparent;
				imageAttribs.SetRemapTable(_colorMaps);
			}
			return imageAttribs;
		}


		/// <summary>
		/// Calculates the source bounds for drawing the given image into the given bounds using the specified layout.
		/// </summary>
		public static Rectangle GetImageSourceBounds(Image image, ImageLayoutMode imageLayout, Rectangle bounds) {
			float scaleX, scaleY;
			float aspectRatio = CalcImageScaleAndAspect(out scaleX, out scaleY, bounds.Width, bounds.Height, image, imageLayout);

			// Get the image bounds because meta files may be transformed which results in bounds that do not match the rectangle (0, 0, image.Width, image.Height).
			GraphicsUnit gfxUnit = GraphicsUnit.Pixel;
			Rectangle imageBounds = Rectangle.Ceiling(image.GetBounds(ref gfxUnit));

			Rectangle result = Rectangle.Empty;
			switch (imageLayout) {
				case ImageLayoutMode.Tile:
				case ImageLayoutMode.FlipTile:
					if (imageBounds.Right > bounds.Width || imageBounds.Bottom > bounds.Height) {
						result.X = Math.Max(0, imageBounds.X);
						result.Y = Math.Max(0, imageBounds.Y);
						result.Width = imageBounds.Width - Math.Max(0, imageBounds.Width - bounds.Width);
						result.Height = imageBounds.Height - Math.Max(0, imageBounds.Height - bounds.Height);
					} else
						result = imageBounds;
					break;
				case ImageLayoutMode.Original:
					if (imageBounds.Right > bounds.Width || imageBounds.Bottom > bounds.Height) {
						result.X = Math.Max(0, imageBounds.X);
						result.Y = Math.Max(0, imageBounds.Y);
						result.Width = imageBounds.Width - Math.Max(0, imageBounds.Right - bounds.Width);
						result.Height = imageBounds.Height - Math.Max(0, imageBounds.Bottom - bounds.Height);
					} else
						result = imageBounds;
					break;
				case ImageLayoutMode.Stretch:
				case ImageLayoutMode.Fit:
					result = imageBounds;
					break;
				case ImageLayoutMode.Center:
				case ImageLayoutMode.CenterTile:
					if (imageBounds.Width > bounds.Width || imageBounds.Height > bounds.Height) {
						result.Width = Math.Min(imageBounds.Width, bounds.Width);
						result.Height = Math.Min(imageBounds.Height, bounds.Height);
						result.X = imageBounds.X + (int)Math.Round((imageBounds.Width - result.Width) / 2f);
						result.Y = imageBounds.Y + (int)Math.Round((imageBounds.Height - result.Height) / 2f);
					} else
						result = imageBounds;
					break;
				default:
					throw new NShapeUnsupportedValueException(imageLayout);
			}
			return result;
		}


		/// <summary>
		/// Calculates the destination bounds for drawing the given image into the given bounds using the specified layout.
		/// </summary>
		public static Rectangle GetImageDestinationBounds(Image image, ImageLayoutMode imageLayout, Rectangle bounds) {
			float scaleX, scaleY;
			float aspectRatio = CalcImageScaleAndAspect(out scaleX, out scaleY, bounds.Width, bounds.Height, image, imageLayout);

			// Get the image bounds because meta files may be transformed which results in bounds that do not match the rectangle (0, 0, image.Width, image.Height).
			GraphicsUnit gfxUnit = GraphicsUnit.Pixel;
			Rectangle imageBounds = Rectangle.Ceiling(image.GetBounds(ref gfxUnit));

			// Transform image bounds
			Rectangle result = Rectangle.Empty;
			switch (imageLayout) {
				case ImageLayoutMode.Tile:
				case ImageLayoutMode.FlipTile:
					result = bounds;
					break;
				case ImageLayoutMode.Original:
					result.X = bounds.X + imageBounds.X;
					result.Y = bounds.Y + imageBounds.Y;
					result.Width = Math.Min(imageBounds.Width, bounds.Width - imageBounds.X);
					result.Height = Math.Min(imageBounds.Height, bounds.Height - imageBounds.Y);
					break;
				case ImageLayoutMode.CenterTile:
				case ImageLayoutMode.Center:
					result.X = Math.Max(bounds.X, bounds.X + (int)Math.Round((bounds.Width - imageBounds.Width) / 2f));
					result.Y = Math.Max(bounds.Y, bounds.Y + (int)Math.Round((bounds.Height - imageBounds.Height) / 2f));
					result.Width = Math.Min(bounds.Width, image.Width);
					result.Height = Math.Min(bounds.Height, image.Height);
					break;
				case ImageLayoutMode.Stretch:
				case ImageLayoutMode.Fit:
					result.X = bounds.X;
					result.Y = bounds.Y;
					if (imageLayout == ImageLayoutMode.Fit) {
						result.X += (int)Math.Round((bounds.Width - (imageBounds.Width * scaleX)) / 2f);
						result.Y += (int)Math.Round((bounds.Height - (imageBounds.Height * scaleY)) / 2f);
					}
					result.Width = (int)Math.Round(imageBounds.Width * scaleX);
					result.Height = (int)Math.Round(imageBounds.Height* scaleY);
					break;
				default:
					throw new NShapeUnsupportedValueException(imageLayout);
			}
			return result;
		}


		/// <summary>
		/// Calculates the source bounds for drawing the given image into the given bounds using the specified layout.
		/// </summary>
		public static Rectangle GetImageSourceClipBounds(Image image, ImageLayoutMode imageLayout, Rectangle bounds, Rectangle clipBounds) {
			float scaleX, scaleY;
			float aspectRatio = CalcImageScaleAndAspect(out scaleX, out scaleY, bounds.Width, bounds.Height, image, imageLayout);

			// Get the image bounds because meta files may be transformed which results in bounds that do not match the rectangle (0, 0, image.Width, image.Height).
			GraphicsUnit gfxUnit = GraphicsUnit.Pixel;
			Rectangle imageBounds = Rectangle.Ceiling(image.GetBounds(ref gfxUnit));

			Rectangle result = Rectangle.Empty;
			switch (imageLayout) {
				case ImageLayoutMode.Tile:
				case ImageLayoutMode.FlipTile:
					if (imageBounds.Right > bounds.Width || imageBounds.Bottom > bounds.Height) {
						result.X = Math.Max(0, imageBounds.X);
						result.Y = Math.Max(0, imageBounds.Y);
						result.Width = imageBounds.Width - Math.Max(0, imageBounds.Width - bounds.Width);
						result.Height = imageBounds.Height - Math.Max(0, imageBounds.Height - bounds.Height);
					} else
						result = imageBounds;
					break;
				case ImageLayoutMode.Original:
					if (imageBounds.Right > clipBounds.Width || imageBounds.Bottom > clipBounds.Height) {
						result.X = Math.Max(clipBounds.X, imageBounds.X);
						result.Y = Math.Max(clipBounds.Y, imageBounds.Y);
						result.Width = imageBounds.Width - Math.Max(0, imageBounds.Right - clipBounds.Width);
						result.Height = imageBounds.Height - Math.Max(0, imageBounds.Bottom - clipBounds.Height);
					} else
						result = clipBounds;
					break;
				case ImageLayoutMode.Stretch:
				case ImageLayoutMode.Fit:
					result = imageBounds;
					break;
				case ImageLayoutMode.Center:
				case ImageLayoutMode.CenterTile:
					if (imageBounds.Width > bounds.Width || imageBounds.Height > bounds.Height) {
						result.Width = Math.Min(imageBounds.Width, bounds.Width);
						result.Height = Math.Min(imageBounds.Height, bounds.Height);
						result.X = imageBounds.X + (int)Math.Round((imageBounds.Width - result.Width) / 2f);
						result.Y = imageBounds.Y + (int)Math.Round((imageBounds.Height - result.Height) / 2f);
					} else
						result = imageBounds;
					break;
				default:
					throw new NShapeUnsupportedValueException(imageLayout);
			}
			return result;
		}


		/// <summary>
		/// Calculates the destination bounds for drawing the given image into the given bounds using the specified layout.
		/// </summary>
		public static Rectangle GetImageDestinationClipBounds(Image image, ImageLayoutMode imageLayout, Rectangle bounds, Rectangle clipBounds) {
			float scaleX, scaleY;
			float aspectRatio = CalcImageScaleAndAspect(out scaleX, out scaleY, bounds.Width, bounds.Height, image, imageLayout);

			// Get the image bounds because meta files may be transformed which results in bounds that do not match the rectangle (0, 0, image.Width, image.Height).
			GraphicsUnit gfxUnit = GraphicsUnit.Pixel;
			Rectangle imageBounds = Rectangle.Ceiling(image.GetBounds(ref gfxUnit));

			// Transform image bounds
			Rectangle result = Rectangle.Empty;
			switch (imageLayout) {
				case ImageLayoutMode.Tile:
				case ImageLayoutMode.FlipTile:
					result = bounds;
					break;
				case ImageLayoutMode.Original:
					result.X = clipBounds.X + imageBounds.X;
					result.Y = clipBounds.Y + imageBounds.Y;
					result.Width = Math.Min(imageBounds.Width, clipBounds.Width - imageBounds.X);
					result.Height = Math.Min(imageBounds.Height, clipBounds.Height - imageBounds.Y);
					break;
				case ImageLayoutMode.CenterTile:
				case ImageLayoutMode.Center:
					result.X = Math.Max(bounds.X, bounds.X + (int)Math.Round((bounds.Width - imageBounds.Width) / 2f));
					result.Y = Math.Max(bounds.Y, bounds.Y + (int)Math.Round((bounds.Height - imageBounds.Height) / 2f));
					result.Width = Math.Min(bounds.Width, image.Width);
					result.Height = Math.Min(bounds.Height, image.Height);
					break;
				case ImageLayoutMode.Stretch:
				case ImageLayoutMode.Fit:
					result.X = bounds.X;
					result.Y = bounds.Y;
					if (imageLayout == ImageLayoutMode.Fit) {
						result.X += (int)Math.Round((bounds.Width - (imageBounds.Width * scaleX)) / 2f);
						result.Y += (int)Math.Round((bounds.Height - (imageBounds.Height * scaleY)) / 2f);
					}
					result.Width = (int)Math.Round(imageBounds.Width * scaleX);
					result.Height = (int)Math.Round(imageBounds.Height* scaleY);
					break;
				default:
					throw new NShapeUnsupportedValueException(imageLayout);
			}
			return result;
		}


		/// <summary>
		/// Draw an image into the specified bounds
		/// </summary>
		public static void DrawImage(Graphics gfx, Image image, ImageAttributes imageAttribs, ImageLayoutMode imageLayout, Rectangle destinationBounds) {
			DrawImage(gfx, image, imageAttribs, imageLayout, destinationBounds, 0);
		}


		/// <summary>
		/// Draw an image into the specified bounds
		/// </summary>
		public static void DrawImage(Graphics gfx, Image image, ImageAttributes imageAttribs, ImageLayoutMode imageLayout, Rectangle destinationBounds, float angle) {
			PointF center = PointF.Empty;
			center.X = destinationBounds.X + (destinationBounds.Width / 2f);
			center.Y = destinationBounds.Y + (destinationBounds.Height / 2f);
			DrawImage(gfx, image, imageAttribs, imageLayout, destinationBounds, destinationBounds, angle, center);
		}


		/// <summary>
		/// Draw an image into the specified bounds
		/// </summary>
		public static void DrawImage(Graphics gfx, Image image, ImageAttributes imageAttribs, ImageLayoutMode imageLayout, Rectangle destinationBounds, float angle, PointF rotationCenter) {
			DrawImage(gfx, image, imageAttribs, imageLayout, destinationBounds, destinationBounds, angle, rotationCenter);
		}


		/// <summary>
		/// Draw an image into the specified bounds
		/// </summary>
		public static void DrawImage(Graphics gfx, Image image, ImageAttributes imageAttribs, ImageLayoutMode imageLayout, Rectangle destinationBounds, Rectangle clipBounds, float angle, PointF rotationCenter) {
			if (gfx == null) throw new ArgumentNullException("gfx");
			if (image == null) throw new ArgumentNullException("image");

			if (angle != 0) {
				gfx.TranslateTransform(rotationCenter.X, rotationCenter.Y);
				gfx.RotateTransform(angle);
				gfx.TranslateTransform(-rotationCenter.X, -rotationCenter.Y);
			}

			// Draw the image
			GraphicsUnit gfxUnit = GraphicsUnit.Pixel; // GraphicsUnit.Display; 
			switch (imageLayout) {
				case ImageLayoutMode.CenterTile:
				case ImageLayoutMode.FlipTile:
				case ImageLayoutMode.Tile: {
						// Calculate the bounds for the whole image (source bounds and destination bounds) for calculating 
						// the number of tiles and the tile offset
						Rectangle imgSourceBounds = GetImageSourceBounds(image, imageLayout, destinationBounds);
						Rectangle imgDestinationBounds = GetImageDestinationBounds(image, imageLayout, destinationBounds);

						// Calculate the number of tiles
						int tileCountX = (int)Math.Ceiling(destinationBounds.Width / (float)image.Width);
						int tileCountY = (int)Math.Ceiling(destinationBounds.Height / (float)image.Height);
						// Calculate the 
						int startX, startY, endX, endY;
						if (imageLayout == ImageLayoutMode.CenterTile) {
							// Set start coordinates for the tiles
							if (tileCountX == 1) startX = imgDestinationBounds.X;
							else startX = (int)Math.Round(imgDestinationBounds.X - ((image.Width * tileCountX) / 2f));
							if (tileCountY == 1) startY = imgDestinationBounds.Y;
							else startY = (int)Math.Round(imgDestinationBounds.Y - ((image.Height * tileCountY) / 2f));
						} else {
							startX = destinationBounds.X;
							startY = destinationBounds.Y;
						}
						endX = destinationBounds.Right;
						endY = destinationBounds.Bottom;

						for (int x = startX; x < endX; x += image.Width) {
							for (int y = startY; y < endY; y += image.Height) {
								// Set source bounds for tile
								Rectangle srcTileBounds = imgSourceBounds;
								if (x < destinationBounds.X)
									srcTileBounds.X = imgSourceBounds.X + (destinationBounds.X - x);
								if (y < destinationBounds.Y)
									srcTileBounds.Y = imgSourceBounds.Y + (destinationBounds.Y - y);
								// Set destination bounds for tile
								Rectangle dstTileBounds = imgSourceBounds;
								dstTileBounds.X = Math.Max(x, destinationBounds.X);
								dstTileBounds.Y = Math.Max(y, destinationBounds.Y);
								// Set source and destination bounds' size
								if (x + image.Width > endX)
									srcTileBounds.Width =
									dstTileBounds.Width = endX - dstTileBounds.X;
								if (y + image.Height > endY)
									srcTileBounds.Height =
									dstTileBounds.Height = endY - dstTileBounds.Y;
								// Handle the FlipTile case
								if (imageLayout == ImageLayoutMode.FlipTile) {
									int modX = (x / image.Width) % 2;
									int modY = (y / image.Height) % 2;
									if (modX != 0) {
										srcTileBounds.X = imgSourceBounds.Right;
										srcTileBounds.Width = -srcTileBounds.Width;
									}
									if (modY != 0) {
										srcTileBounds.Y = imgSourceBounds.Bottom;
										srcTileBounds.Height = -srcTileBounds.Height;
									}
								}
								// Don't draw image if source width/height are <= 0!
								if (imageLayout == ImageLayoutMode.FlipTile) {
									if (srcTileBounds.Width == 0 || srcTileBounds.Height == 0)
										continue;
								} else {
									if (srcTileBounds.Width <= 0 || srcTileBounds.Height <= 0)
										continue;
								}
								// Draw only tiles that intersect with the clipping area
								//if (!Geometry.RectangleIntersectsWithRectangle(dstTileBounds, clipBounds))
								//    continue;
								gfx.DrawImage(image, dstTileBounds, srcTileBounds.X, srcTileBounds.Y, srcTileBounds.Width, srcTileBounds.Height, gfxUnit, imageAttribs);
							}
						}
					}
					break;
				case ImageLayoutMode.Original:
				// Don't use DrawImageUnscaledAndClipped! It will not - as the name would imply - draw the image 1:1 into the 
				// given rectangle, it will fill the given rectangle instead and therefore scale it to the extents of the rectangle. 
				// The "Unscaled" refers to the image's resolution (DPI) settings which will be ignored in this case (it will not scale from 100px to 100mm).
				case ImageLayoutMode.Center:
				case ImageLayoutMode.Fit:
				case ImageLayoutMode.Stretch: {
						// Get source and destination bounds according to the layout.
						Rectangle imgSourceClipBounds = GetImageSourceClipBounds(image, imageLayout, destinationBounds, clipBounds);
						Rectangle imgDestinationClipBounds = GetImageDestinationClipBounds(image, imageLayout, destinationBounds, clipBounds);

						// Don't draw image if source width/height are <= 0!
						if (!(imgSourceClipBounds.Width <= 0 || imgSourceClipBounds.Height <= 0))
							gfx.DrawImage(image, imgDestinationClipBounds, imgSourceClipBounds.X, imgSourceClipBounds.Y, imgSourceClipBounds.Width, imgSourceClipBounds.Height, gfxUnit, imageAttribs);
					}
					break;
				default: throw new NShapeUnsupportedValueException(imageLayout);
			}
			if (angle != 0) {
				gfx.TranslateTransform(rotationCenter.X, rotationCenter.Y);
				gfx.RotateTransform(-angle);
				gfx.TranslateTransform(-rotationCenter.X, -rotationCenter.Y);
			}
		}

		#endregion


		#region Creating and transforming brushes

		/// <summary>
		/// Copies the given image into a new image if the desired size is at half (or less) of the image's size, the image will be shrinked to the desired size.
		/// If the desired size is more than half of the image's size, the original image will be returned.
		/// </summary>
		public static Image GetBrushImage(Image image, int desiredWidth, int desiredHeight) {
			if (image == null) throw new ArgumentNullException("image");
			if (desiredWidth <= 0) desiredWidth = 1;
			if (desiredHeight <= 0) desiredHeight = 1;
			float scaleFactor = Geometry.CalcScaleFactor(
				image.Width,
				image.Height,
				image.Width / Math.Max(1, (image.Width / desiredWidth)),
				image.Height / Math.Max(1, (image.Height / desiredHeight)));
			if (scaleFactor > 0.75)
				return image;
			else {
				int scaledWidth = (int)Math.Round(image.Width * scaleFactor);
				int scaledHeight = (int)Math.Round(image.Height * scaleFactor);
				//Stopwatch sw = new Stopwatch();
				//sw.Start();
				Bitmap newImage = (Bitmap)image.GetThumbnailImage(scaledWidth, scaledHeight, null, IntPtr.Zero);
				//sw.Stop();
				//Debug.Print("Creating Thumbnail scaled to {0:F2}%: {1}", scaleFactor * 100, sw.Elapsed);
				return newImage;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static TextureBrush CreateTextureBrush(Image image, int width, int height, ImageLayoutMode imageLayout, float gamma, byte transparency, bool grayScale) {
			if (image == null) throw new ArgumentNullException("image");
			return CreateTextureBrush(image, width, height, GetImageAttributes(imageLayout, gamma, transparency, grayScale));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static TextureBrush CreateTextureBrush(Image image, ImageLayoutMode imageLayout, float gamma, byte transparency, bool grayScale) {
			if (image == null) throw new ArgumentNullException("image");
			return CreateTextureBrush(image, GetImageAttributes(imageLayout, gamma, transparency, grayScale));
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static TextureBrush CreateTextureBrush(Image image, ImageAttributes imageAttribs) {
			if (image == null) throw new ArgumentNullException("image");
			Rectangle brushBounds = Rectangle.Empty;
			brushBounds.X = 0;
			brushBounds.Y = 0;
			brushBounds.Width = image.Width;
			brushBounds.Height = image.Height;
			return new TextureBrush(image, brushBounds, imageAttribs);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static TextureBrush CreateTextureBrush(Image image, int desiredWidth, int desiredHeight, ImageAttributes imageAttribs) {
			if (image == null) throw new ArgumentNullException("image");
			if (!(image is Bitmap)) throw new NotSupportedException(string.Format("{0} images are not supported for this operation.", image.GetType().Name));
			return CreateTextureBrush(GetBrushImage(image, desiredWidth, desiredHeight), imageAttribs);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void TransformLinearGradientBrush(LinearGradientBrush brush, float gradientAngleDeg, Rectangle unrotatedBounds, Point center, float angleDeg) {
			if (brush == null) throw new ArgumentNullException("brush");
			if (brush == null) throw new ArgumentNullException("brush");
			// move brush higher than needed  and make the brush larger than needed
			// (ensure that there are no false color pixels from antialiasing inside the gradient)
			
			PointF boundsCenter = PointF.Empty;
			boundsCenter.X = unrotatedBounds.X + (unrotatedBounds.Width / 2f);
			boundsCenter.Y = unrotatedBounds.Y + (unrotatedBounds.Height / 2f);
			float gradientSize = (int)Math.Ceiling(CalculateGradientSize(angleDeg, gradientAngleDeg, boundsCenter.X - unrotatedBounds.Left, Math.Max(boundsCenter.Y - unrotatedBounds.Top, unrotatedBounds.Bottom - boundsCenter.Y))) + 2;
			float scaleFactor = ((float)Math.Sqrt((gradientSize * gradientSize) * 2) + 1f) / ((LinearGradientBrush)brush).Rectangle.Width;

			float dx = boundsCenter.X - center.X;
			float dy = boundsCenter.Y - center.Y;
			Geometry.RotatePoint(0, 0, angleDeg, ref dx, ref dy);

			brush.ResetTransform();
			brush.TranslateTransform(center.X + dx, center.Y + dy - gradientSize);
			brush.RotateTransform(gradientAngleDeg);
			brush.ScaleTransform(scaleFactor, scaleFactor);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void TransformLinearGradientBrush(LinearGradientBrush brush, float gradientAngleDeg, RectangleF unrotatedBounds, PointF center, float angleDeg) {
			if (brush == null) throw new ArgumentNullException("brush");
			// move brush higher than needed  and make the brush larger than needed
			// (ensure that there are no false color pixels from antialiasing inside the gradient)
			float gradientSize = (CalculateGradientSize(angleDeg, gradientAngleDeg, center.X - unrotatedBounds.Left, Math.Max(center.Y - unrotatedBounds.Top, unrotatedBounds.Bottom - center.Y))) + 2;
			float scaleFactor = ((float)Math.Sqrt((gradientSize * gradientSize) * 2) + 1f) / ((LinearGradientBrush)brush).Rectangle.Width;

			brush.ResetTransform();
			brush.TranslateTransform(center.X, center.Y - gradientSize);
			brush.RotateTransform(gradientAngleDeg);
			brush.ScaleTransform(scaleFactor, scaleFactor);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void TransformPathGradientBrush(PathGradientBrush brush, Rectangle unrotatedBounds, Point center, float angleDeg) {
			throw new NotImplementedException();
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void TransformPathGradientBrush(PathGradientBrush brush, Rectangle unrotatedBounds, Rectangle rotatedBounds, float angleDeg) {
			throw new NotImplementedException();
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void TransformTextureBrush(TextureBrush brush, ImageLayoutMode imageLayout, Rectangle unrotatedBounds, Point center, float angleDeg) {
			if (brush == null) throw new ArgumentNullException("brush");
			// scale image
			float scaleX, scaleY;
			GdiHelpers.CalcImageScaleAndAspect(out scaleX, out scaleY, unrotatedBounds.Width, unrotatedBounds.Height, brush.Image, imageLayout);
			// rotate at center point
			((TextureBrush)brush).ResetTransform();
			((TextureBrush)brush).TranslateTransform(center.X, center.Y);
			((TextureBrush)brush).RotateTransform(angleDeg);
			((TextureBrush)brush).TranslateTransform(-unrotatedBounds.Width / 2f, -unrotatedBounds.Height / 2f);
			// scale image
			switch (imageLayout) {
				case ImageLayoutMode.Tile:
				case ImageLayoutMode.FlipTile:
				case ImageLayoutMode.Original:
					// nothing to do
					break;
				case ImageLayoutMode.Center:
				case ImageLayoutMode.CenterTile:
					((TextureBrush)brush).TranslateTransform((unrotatedBounds.Width - brush.Image.Width) / 2f, (unrotatedBounds.Height - brush.Image.Height) / 2f);
					break;
				case ImageLayoutMode.Stretch:
				case ImageLayoutMode.Fit:
					if (imageLayout == ImageLayoutMode.Fit)
						((TextureBrush)brush).TranslateTransform((unrotatedBounds.Width - (brush.Image.Width * scaleX)) / 2f, (unrotatedBounds.Height - (brush.Image.Height * scaleY)) / 2f);
					((TextureBrush)brush).ScaleTransform(scaleX, scaleY);
					break;
				default: throw new NShapeUnsupportedValueException(imageLayout);
			}
		}


		/// <summary>
		/// Calculates scale factors and Aspect ratio depending on the given ImageLayout.
		/// </summary>
		/// <param name="scaleFactorX">Scaling factor on X axis.</param>
		/// <param name="scaleFactorY">Scaling factor on Y axis.</param>
		/// <param name="dstWidth">The desired width of the image.</param>
		/// <param name="dstHeight">The desired height of the image.</param>
		/// <param name="image">the untransformed Image object.</param>
		/// <param name="imageLayout">ImageLayout enumeration. Deines the scaling behavior.</param>
		/// <returns>Aspect ratio of the image.</returns>
		public static float CalcImageScaleAndAspect(out float scaleFactorX, out float scaleFactorY, int dstWidth, int dstHeight, Image image, ImageLayoutMode imageLayout) {
			float aspectRatio = 1;
			scaleFactorX = 1f;
			scaleFactorY = 1f;
			if (image != null && image.Height > 0 && image.Width > 0) {
				if (image.Width == image.Height) aspectRatio = 1;
				else aspectRatio = (float)image.Width / image.Height;

				switch (imageLayout) {
					case ImageLayoutMode.Fit:
						double lowestRatio = Math.Min((double)dstWidth / (double)image.Width, (double)dstHeight / (double)image.Height);
						scaleFactorX = (float)Math.Round(lowestRatio, 6);
						scaleFactorY = (float)Math.Round(lowestRatio, 6);
						break;
					case ImageLayoutMode.Stretch:
						scaleFactorX = (float)dstWidth / image.Width;
						scaleFactorY = (float)dstHeight / image.Height;
						break;
					case ImageLayoutMode.Original:
					case ImageLayoutMode.Center:
						// nothing to do
						break;
					case ImageLayoutMode.CenterTile:
					case ImageLayoutMode.Tile:
					case ImageLayoutMode.FlipTile:
						// nothing to do
						break;
					default:
						throw new NShapeUnsupportedValueException(imageLayout);
				}
			}
			return aspectRatio;
		}


		// calculate gradient size dependent of the shape's rotation
		/// <ToBeCompleted></ToBeCompleted>
		private static float CalculateGradientSize(float angle, float gradientAngle, int leftToCenter, int topToCenter) {
			float result = 0f;

			double a = angle;
			int r = leftToCenter;
			int o = topToCenter;

			if (a < 0) a = 360 + a;
			a = a % 180;
			if (a / 90 >= 1 && a / 90 < 2) {
				r = topToCenter;
				o = leftToCenter;
				a = a % 90;
			}
			if (a > 45)
				a = (a - ((a % 45) * 2)) * (Math.PI / 180);
			else
				a = a * (Math.PI / 180);
			double cos = Math.Cos(a);
			double sin = Math.Sin(a);

			// calculate offset 
			double dR = (r * sin) + (r * cos);
			double dO = Math.Abs((o * cos) - (o * sin));
			result = (float)Math.Round(dR + dO, 6);

			return result;
		}


		// Calculate gradient size dependent of the shape's rotation
		/// <ToBeCompleted></ToBeCompleted>
		private static float CalculateGradientSize(float angle, float gradientAngle, float leftToCenter, float topToCenter) {
			float result = 0f;

			double a = angle;
			float r = leftToCenter;
			float o = topToCenter;

			if (a < 0) a = 360 + a;
			a = a % 180;
			if (a / 90 >= 1 && a / 90 < 2) {
				r = topToCenter;
				o = leftToCenter;
				a = a % 90;
			}
			if (a > 45)
				a = (a - ((a % 45) * 2)) * (Math.PI / 180);
			else
				a = a * (Math.PI / 180);
			double cos = Math.Cos(a);
			double sin = Math.Sin(a);

			// calculate offset 
			double dR = (r * sin) + (r * cos);
			double dO = Math.Abs((o * cos) - (o * sin));
			result = (float)Math.Round(dR + dO, 6);

			return result;
		}

		#endregion


		#region Comparing Images

		//public static bool AreEqual(Bitmap bitmapA, Bitmap bitmapB) {
		//    bool areEqual = true;
		//    // First, check image dimensions and pixel format
		//    if (bitmapA.Width == bitmapB.Width && bitmapA.Height == bitmapB.Height && bitmapA.PixelFormat == bitmapB.PixelFormat) {
		//        Rectangle rect = Rectangle.Empty;
		//        rect.Width = bitmapA.Width;
		//        rect.Height = bitmapA.Height;
		//        BitmapData bmpDataA = null;
		//        BitmapData bmpDataB = null;
		//        try {
		//            bmpDataA = bitmapA.LockBits(rect, ImageLockMode.ReadOnly, bitmapA.PixelFormat);
		//            bmpDataB = bitmapB.LockBits(rect, ImageLockMode.ReadOnly, bitmapB.PixelFormat);

		//            int offsetA = 0;
		//            int offsetB = 0;
		//            int rowSize = rect.Width * GetBytesPerPixel(bitmapA.PixelFormat);
		//            for (int y = 0; areEqual && y < rect.Height; ++y) {
		//                for (int x = 0; x < rowSize; ++x) {
		//                    if (Marshal.ReadByte(bmpDataA.Scan0, offsetA) != Marshal.ReadByte(bmpDataB.Scan0, offsetB)) {
		//                        areEqual = false;
		//                        break;
		//                    }
		//                    ++offsetA;
		//                    ++offsetB;
		//                }
		//                offsetA += bmpDataA.Stride - rowSize;
		//                offsetB += bmpDataB.Stride - rowSize;
		//            }
		//        } finally {
		//            bitmapA.UnlockBits(bmpDataA);
		//            bitmapB.UnlockBits(bmpDataB);
		//        }
		//    }
		//    return areEqual;
		//}


		/// <summary>
		/// Returns the MD5 hash of the given image as HEX string.
		/// </summary>
		/// <remarks>
		/// For bitmap images, the unencoded, uncompressed raw pixel data is used for calculating the hash, 
		/// otherwise the data of the underlying memory stream.
		/// </remarks>
		public static string CalculateHashCode(Image image)
		{
			byte[] imageData = (image is Bitmap) ? GetRawImageData((Bitmap)image) : GetEncodedImageData(image);
			return CalculateHashCode(imageData);
		}


		/// <summary>
		/// Returns the MD5 hash of the given image as HEX string.
		/// </summary>
		public static string CalculateHashCode(Bitmap image) {
			return CalculateHashCode(GetRawImageData(image));
		}
		

		/// <summary>
		/// Returns the MD5 hash of the given byte array as HEX string.
		/// </summary>
		public static string CalculateHashCode(byte[] data) {
			return ByteArrayToHexString(_MD5Provider.ComputeHash(data));
		}
		

		/// <summary>
		/// Gets the uncompressed raw pixel data of the image as byte array.
		/// </summary>
		public static byte[] GetRawImageData(Bitmap image) {
			byte[] result = null;
			BitmapData bmpData = null;
			try {
				bmpData = ((Bitmap)image).LockBits(Rectangle.FromLTRB(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
				int byteCount = bmpData.Stride * image.Height;
				result = new byte[byteCount];
				System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, result, 0, byteCount);
			} finally {
				if (bmpData != null)
					((Bitmap)image).UnlockBits(bmpData);
			}
			return result;
		}


		/// <summary>
		/// Gets the encoded image data as byte array.
		/// </summary>
		public static byte[] GetEncodedImageData(Image image)
		{
			byte[] result = null;
			using (MemoryStream memStream = new MemoryStream()) {
				GdiHelpers.SaveImage(image, memStream);
				result = memStream.ToArray();
			}
			return result;
		}


		/// <summary>
		/// Returns true if the given Images are considered as equal.
		/// For performance reasons, the images are compred by comparing the hash codes of the underlying image data.
		/// </summary>
		public static bool AreEqual(Image imgA, Image imgB) {
			byte[] imgDataA = (imgA is Bitmap) ? GetRawImageData((Bitmap)imgA) : GetEncodedImageData(imgA);
			byte[] imgDataB = (imgB is Bitmap) ? GetRawImageData((Bitmap)imgB) : GetEncodedImageData(imgB);
			return CalculateHashCode(imgDataA) == CalculateHashCode(imgDataB);
		}


		/// <summary>
		/// Returns true if the given byte arrays are considered as equal.
		/// For performance reasons, the byte arrays are compred by comparing their hash codes.
		/// </summary>
		public static bool AreEqual(byte[] dataA, byte[] dataB) {
			return CalculateHashCode(dataA) == CalculateHashCode(dataB);
		}


		/// <summary>
		/// Converts the given byte array to a HEX string.
		/// </summary>
		public static string ByteArrayToHexString(byte[] bytes) {
			int requiredLength = bytes.Length * 2;
			System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(requiredLength, requiredLength);
			foreach (byte b in bytes)
				stringBuilder.Append(HexStrings[b]);
			return stringBuilder.ToString();
		}

		#endregion


		#region Exporting Images

		/// <ToBeCompleted></ToBeCompleted>
		public static void SaveImage(Image image, string filePath) {
			if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
			using (Stream stream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
				SaveImage(image, stream);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void SaveImage(Image image, string filePath, ImageFileFormat imageFormat) {
			if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
			using (Stream stream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
				SaveImage(image, stream, imageFormat);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void SaveImage(Image image, string filePath, ImageFileFormat imageFormat, int compressionQuality) {
			if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
			using (Stream stream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
				SaveImage(image, stream, imageFormat, compressionQuality);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void SaveImage(Image image, Stream stream) {
			SaveImage(image, stream, GetImageFileFormat(image.RawFormat), 100);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void SaveImage(Image image, Stream stream, ImageFileFormat imageFormat) {
			SaveImage(image, stream, imageFormat, 100);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void SaveImage(Image image, Stream stream, ImageFileFormat imageFormat, int compressionQuality) {
			if (image == null) throw new ArgumentNullException("image");
			if (stream == null) throw new ArgumentNullException("stream");
			if (image is Metafile) {
				EmfHelper.SaveEnhMetaFile(stream, (Metafile)image.Clone());

				#region Old version: Works but replaces all metafile records with a single DrawImage record
				//// Create a new metafile
				//Graphics gfx = Graphics.FromHwnd(IntPtr.Zero);
				//IntPtr hdc = gfx.GetHdc();
				//Metafile metaFile = new Metafile(filePath, 
				//   hdc, 
				//   Rectangle.FromLTRB(0, 0, image.Width, image.Height), 
				//   MetafileFrameUnit.Pixel, 
				//   (imageFormat == NShapeImageFormat.EmfPlus) ? EmfType.EmfPlusDual : EmfType.EmfOnly);
				//gfx.ReleaseHdc(hdc);
				//gfx.Dispose();

				//// Create graphics context for drawing
				//gfx = Graphics.FromImage(metaFile);
				//GdiHelpers.ApplyGraphicsSettings(gfx, NShapeRenderingQuality.MaximumQuality);
				//// Draw image
				//gfx.DrawImage(image, Point.Empty);

				//gfx.Dispose();
				//metaFile.Dispose();
				#endregion

			} else if (image is Bitmap) {
				ImageCodecInfo codecInfo = GetEncoderInfo(imageFormat);
				EncoderParameters encoderParams = new EncoderParameters(3);
				encoderParams.Param[0] = new EncoderParameter(Encoder.RenderMethod, (long)EncoderValue.RenderProgressive);
				// JPG specific encoder parameter
				encoderParams.Param[1] = new EncoderParameter(Encoder.Quality, (long)compressionQuality);
				// TIFF specific encoder parameter
				encoderParams.Param[2] = new EncoderParameter(Encoder.Compression, (long)EncoderValue.CompressionLZW);

				image.Save(stream, codecInfo, encoderParams);
			} else {
				image.Save(stream, GetGdiImageFormat(imageFormat));
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static void CreateMetafile(string filePath, ImageFileFormat imageFormat, int width, int height, DrawCallback callback) {
			if (callback == null) throw new ArgumentNullException("callback");
			if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
			Metafile metaFile = null;
			// Create a new metafile
			using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero)) {
				IntPtr hdc = gfx.GetHdc();
				try {
					metaFile= new Metafile(filePath,
						hdc,
						Rectangle.FromLTRB(0, 0, width, height),
						MetafileFrameUnit.Pixel,
						(imageFormat == ImageFileFormat.EmfPlus) ? EmfType.EmfPlusDual : EmfType.EmfOnly);
				} finally { gfx.ReleaseHdc(hdc); }
			}
			// Create graphics context for drawing
			if (metaFile != null) {
				using (Graphics gfx = Graphics.FromImage(metaFile)) {
					GdiHelpers.ApplyGraphicsSettings(gfx, RenderingQuality.MaximumQuality);
					// Draw image
					Rectangle bounds = Rectangle.Empty;
					bounds.Width = width;
					bounds.Height = height;
					callback(gfx, bounds);
				}
				metaFile.Dispose();
				metaFile = null;
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static ImageFormat GetGdiImageFormat(ImageFileFormat imageFormat) {
			ImageFormat result;
			switch (imageFormat) {
				case ImageFileFormat.Bmp: result = ImageFormat.Bmp; break;
				case ImageFileFormat.Emf:
				case ImageFileFormat.EmfPlus: result = ImageFormat.Emf; break;
				case ImageFileFormat.Gif: result = ImageFormat.Gif; break;
				case ImageFileFormat.Jpeg: result = ImageFormat.Jpeg; break;
				case ImageFileFormat.Png: result = ImageFormat.Png; break;
				case ImageFileFormat.Tiff: result = ImageFormat.Tiff; break;
				default: return result = ImageFormat.Bmp;
			}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static ImageFileFormat GetImageFileFormat(ImageFormat imageFormat) {
			ImageFileFormat result;
			if (imageFormat.Guid == ImageFormat.Bmp.Guid)
				result = ImageFileFormat.Bmp;
			else if (imageFormat.Guid == ImageFormat.Emf.Guid)
				result = ImageFileFormat.EmfPlus;
			else if (imageFormat.Guid == ImageFormat.Gif.Guid)
				result = ImageFileFormat.Gif;
			else if (imageFormat.Guid == ImageFormat.Jpeg.Guid)
				result = ImageFileFormat.Jpeg;
			else if (imageFormat.Guid == ImageFormat.Png.Guid)
				result = ImageFileFormat.Png;
			else if (imageFormat.Guid == ImageFormat.Tiff.Guid)
				result = ImageFileFormat.Tiff;
			else if (imageFormat.Guid == ImageFormat.Wmf.Guid)
				result = ImageFileFormat.Emf;
			else
				result = ImageFileFormat.Bmp;
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public static ImageCodecInfo GetEncoderInfo(ImageFileFormat imageFormat) {
			ImageFormat format = GetGdiImageFormat(imageFormat);
			ImageCodecInfo result = null;
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
			foreach (ImageCodecInfo ici in encoders)
				if (ici.FormatID.Equals(format.Guid)) {
					result = ici;
					break;
				}
			return result;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public delegate void DrawCallback(Graphics graphics, Rectangle bounds);


		#endregion


		#region [Private] Methods

		private static void ResetColorMatrix(ColorMatrix colorMatrix) {
			// Reset color matrix to these values:
			//		1	0	0	0	0
			//		0	1	0	0	0
			//		0	0	1	0	0
			//		0	0	0	1	0
			//		0	0	0	0	1
			colorMatrix.Matrix00 =
			colorMatrix.Matrix11 =
			colorMatrix.Matrix22 =
			colorMatrix.Matrix33 =
			colorMatrix.Matrix44 = 1;
			colorMatrix.Matrix01 = colorMatrix.Matrix02 = colorMatrix.Matrix03 = colorMatrix.Matrix04 =
			colorMatrix.Matrix10 = colorMatrix.Matrix12 = colorMatrix.Matrix13 = colorMatrix.Matrix14 =
			colorMatrix.Matrix20 = colorMatrix.Matrix21 = colorMatrix.Matrix23 = colorMatrix.Matrix24 =
			colorMatrix.Matrix30 = colorMatrix.Matrix31 = colorMatrix.Matrix32 = colorMatrix.Matrix34 =
			colorMatrix.Matrix40 = colorMatrix.Matrix41 = colorMatrix.Matrix42 = colorMatrix.Matrix43 = 0;
		}


		private static void ApplyGrayScale(ColorMatrix colorMatrix) {
			// Grayscale == no color saturation
			ApplySaturation(colorMatrix, 0);
		}


		private static void ApplyContrast(ColorMatrix colorMatrix, float contrastFactor) {
			ApplyContrast(colorMatrix, contrastFactor, contrastFactor, contrastFactor);
		}


		private static void ApplyContrast(ColorMatrix colorMatrix, float contrastFactorR, float contrastFactorG, float contrastFactorB) {
			// Contrast correction matrix:
			//		c	0	0	0	0
			//		0	c	0	0	0
			//		0	0	c	0	0
			//		0	0	0	1	0
			//		0	0	0	0	1
			// Note: 
			// Due to a overflow bug in GDI+, the RGB color channel values will flip to 0 as soon as 
			// the original value multiplied with the contrast factor exceeds 255.
			// This bug was corrected with Windows 7.
			
			// Workaround for the GDI+ bug (see above)
			colorMatrix.Matrix40 = 
			colorMatrix.Matrix41 = 
			colorMatrix.Matrix42 = 0.000001f;

			// Set contrast color shear matrix
			colorMatrix.Matrix00 *= contrastFactorR;
			colorMatrix.Matrix11 *= contrastFactorG;
			colorMatrix.Matrix22 *= contrastFactorB;
		}


		private static void ApplyInvertation(ColorMatrix colorMatrix) {
			ApplyInvertation(colorMatrix, 1);
		}


		private static void ApplyInvertation(ColorMatrix colorMatrix, float invertationFactor) {
			ApplyInvertation(colorMatrix, invertationFactor, invertationFactor, invertationFactor);
		}


		private static void ApplyInvertation(ColorMatrix colorMatrix, float invertFactorR, float invertFactorG, float invertFactorB) {
			// Matrix layout:
			//		-r		0		0		0		0
			//		0		-g		0		0		0
			//		0		0		-b		0		0
			//		0		0		0		1		0
			//		1		1		1		0		1
			// Set invertation color shear matrix
			colorMatrix.Matrix00 *= -invertFactorR;
			colorMatrix.Matrix11 *= -invertFactorG;
			colorMatrix.Matrix22 *= -invertFactorB;
			colorMatrix.Matrix40 = colorMatrix.Matrix41 = colorMatrix.Matrix42 = 1f;
		}


		private static void ApplySaturation(ColorMatrix colorMatrix, float saturationFactor) {
			ApplySaturation(colorMatrix, saturationFactor, saturationFactor, saturationFactor);
		}


		private static void ApplySaturation(ColorMatrix colorMatrix, float saturationFactorR, float saturationFactorG, float saturationFactorB) {
			float complementR = luminanceFactorR * (colorMatrix.Matrix00 - saturationFactorR);
			float complementG = luminanceFactorG * (colorMatrix.Matrix11 - saturationFactorG);
			float complementB = luminanceFactorB * (colorMatrix.Matrix22 - saturationFactorB);
			//float complementR = 0.3086f * (colorMatrix.Matrix00 - saturationFactorR);
			//float complementG = 0.6094f * (colorMatrix.Matrix11 - saturationFactorG);
			//float complementB = 0.0820f * (colorMatrix.Matrix22 - saturationFactorB);

			colorMatrix.Matrix00 = complementR + saturationFactorR;
			colorMatrix.Matrix01 = complementR;
			colorMatrix.Matrix02 = complementR;

			colorMatrix.Matrix10 = complementG;
			colorMatrix.Matrix11 = complementG + saturationFactorG;
			colorMatrix.Matrix12 = complementG;

			colorMatrix.Matrix20 = complementB;
			colorMatrix.Matrix21 = complementB;
			colorMatrix.Matrix22 = complementB + saturationFactorB;
		}


		private static void ApplyTransparency(ColorMatrix colorMatrix, float transparencyFactor) {
			colorMatrix.Matrix33 *= 1f - transparencyFactor;
		}


		private static byte GetBytesPerPixel(PixelFormat pixelFormat) {
			switch (pixelFormat) {
				case PixelFormat.Gdi:
				case PixelFormat.Indexed:
				case PixelFormat.Max:
				case PixelFormat.PAlpha:
				case PixelFormat.Undefined:
				case PixelFormat.Format4bppIndexed:
					throw new NotSupportedException();
				case PixelFormat.Format8bppIndexed:
					return 1;
				case PixelFormat.Format16bppArgb1555:
				case PixelFormat.Format16bppGrayScale:
				case PixelFormat.Format16bppRgb555:
				case PixelFormat.Format16bppRgb565:
				case PixelFormat.Format1bppIndexed:
					return 2;
				case PixelFormat.Format24bppRgb:
					return 3;
				case PixelFormat.Format32bppArgb:
				case PixelFormat.Format32bppPArgb:
				case PixelFormat.Format32bppRgb:
					return 4;
				case PixelFormat.Format48bppRgb:
					return 6;
				case PixelFormat.Format64bppArgb:
				case PixelFormat.Format64bppPArgb:
					return 8;
				default:
					throw new ArgumentException();
			}
		}

		#endregion


		#region Fields

		// Mapping array for speeding up HEX string conversion: Contains a hex string for each byte value
		private static readonly string[] HexStrings = { 
			"00", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0A", "0B", "0C", "0D", "0E", "0F", 
			"10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "1A", "1B", "1C", "1D", "1E", "1F", 
			"20", "21", "22", "23", "24", "25", "26", "27", "28", "29", "2A", "2B", "2C", "2D", "2E", "2F", 
			"30", "31", "32", "33", "34", "35", "36", "37", "38", "39", "3A", "3B", "3C", "3D", "3E", "3F", 
			"40", "41", "42", "43", "44", "45", "46", "47", "48", "49", "4A", "4B", "4C", "4D", "4E", "4F", 
			"50", "51", "52", "53", "54", "55", "56", "57", "58", "59", "5A", "5B", "5C", "5D", "5E", "5F", 
			"60", "61", "62", "63", "64", "65", "66", "67", "68", "69", "6A", "6B", "6C", "6D", "6E", "6F", 
			"70", "71", "72", "73", "74", "75", "76", "77", "78", "79", "7A", "7B", "7C", "7D", "7E", "7F", 
			"80", "81", "82", "83", "84", "85", "86", "87", "88", "89", "8A", "8B", "8C", "8D", "8E", "8F", 
			"90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "9A", "9B", "9C", "9D", "9E", "9F", 
			"A0", "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "AA", "AB", "AC", "AD", "AE", "AF", 
			"B0", "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "BA", "BB", "BC", "BD", "BE", "BF", 
			"C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF", 
			"D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DA", "DB", "DC", "DD", "DE", "DF", 
			"E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9", "EA", "EB", "EC", "ED", "EE", "EF", 
			"F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "FA", "FB", "FC", "FD", "FE", "FF" 
		};

		private static ImageAttributes _imageAttribs = new ImageAttributes();
		private static ColorMap[] _colorMaps = new ColorMap[] { new ColorMap() };
		private static ColorMatrix _colorMatrix = new ColorMatrix();
		private static Matrix _matrix = new Matrix();

		private static MD5CryptoServiceProvider _MD5Provider = new MD5CryptoServiceProvider();
		private static System.Text.ASCIIEncoding _asciiEncoder = new System.Text.ASCIIEncoding();

		// Constants for the color-to-greyscale conversion
		// Luminance correction factor (the human eye has preferences regarding colors)
		private const float luminanceFactorR = 0.3f;
		private const float luminanceFactorG = 0.59f;
		private const float luminanceFactorB = 0.11f;
		// Alternative values
		//private const float luminanceFactorRed = 0.3f;
		//private const float luminanceFactorGreen = 0.5f;
		//private const float luminanceFactorBlue = 0.3f;

		#endregion
	}


	/// <summary>
	/// This class combines GDI+ images (bitmaps or metafiles) with a name (typically the filename without path and extension).
	/// </summary>
	[TypeConverter("Dataweb.NShape.WinFormsUI.NamedImageTypeConverter, Dataweb.NShape.WinFormsUI")]
	public class NamedImage : IDisposable {

		/// <summary>
		/// Loads an image from a file.
		/// </summary>
		public static NamedImage FromFile(string fileName) {
			return new NamedImage(fileName);
		}


		/// Indicates whether the specified <see cref="T:Dataweb.NShape.Advanced.NamedImage" /> is null or has neither an <see cref="T:System.Drawing.Imaging.Image" /> nor an existing file to load from.
		public static bool IsNullOrEmpty(NamedImage namedImage) {
			if (namedImage != null) {
				if (namedImage._image != null)
					return false;
				else if (File.Exists(namedImage.FilePath))
					return false;
			}
			return true;
		}


		/// <override></override>
		public override string ToString() {
			return this._name;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NamedImage" />.
		/// </summary>
		public NamedImage() {
			_image = null;
			_name = string.Empty;
			_filePath = string.Empty;
			_hashCode = null;
			//_canLoadFromFile = false;
			//_imageType = typeof(Image);
			_imageSize = Size.Empty;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NamedImage" />.
		/// </summary>
		public NamedImage(string fileName)
			: this() {
			if (fileName == null) throw new ArgumentNullException("fileName");
			if (fileName == string.Empty) throw new ArgumentException("fileName");
			Load(fileName);
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NamedImage" />.
		/// </summary>
		/// <remarks>The given image will be cloned.</remarks>
		public NamedImage(Image image, string name)
			: this() {
			if (image == null) throw new ArgumentNullException("image");
			if (name == null) throw new ArgumentNullException("name");
			Image = image;
			Name = name;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NamedImage" />.
		/// </summary>
		/// <remarks>The given image will be cloned.</remarks>
		public NamedImage(Image image, string fileName, string name)
			: this() {
			if (image == null) throw new ArgumentNullException("image");
			if (fileName == null) throw new ArgumentNullException("fileName");
			if (name == null) throw new ArgumentNullException("name");
			Name = name;
			FilePath = fileName;
			Image = (Image)image.Clone();
		}


		///// <summary>
		///// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NamedImage" />.
		///// </summary>
		///// <remarks>The given image will be cloned.</remarks>
		//public NamedImage(Image image, string fileName, string name, string hash)
		//	: this()
		//{
		//	if (image == null) throw new ArgumentNullException("image");
		//	if (fileName == null) throw new ArgumentNullException("fileName");
		//	if (name == null) throw new ArgumentNullException("name");
		//	Name = name;
		//	FilePath = fileName;
		//	Image = (Image)image.Clone();
		//	Debug.Assert(hash == GdiHelpers.CalculateHashCode(Image));
		//	_hashCode = hash;
		//}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.NamedImage" />.
		/// </summary>
		public NamedImage(NamedImage source) {
			if (source == null) throw new ArgumentNullException("source");
			Name = source.Name;
			FilePath = source.FilePath;
			//this.canLoadFromFile = source.canLoadFromFile;
			//if (source.canLoadFromFile) {
			//   image = null;	// Load on next usage
			//   imageSize = source.imageSize;
			//   imageType = source.imageType;
			//} else {
				if (source.Image == null)
					Image = null;
				else Image = (Image)source.Image.Clone();
			//}
		}


		/// <summary>
		/// Finalizer of <see cref="T:Dataweb.NShape.Advanced.NamedImage" />.
		/// </summary>
		~NamedImage() {
			Dispose();
		}


		#region IDisposable Members

		/// <summary>
		/// Releases all resources.
		/// </summary>
		public void Dispose() {
			if (_image != null) _image.Dispose();
			_image = null;
		}

		#endregion


		/// <summary>
		/// Creates a copy of this <see cref="T:Dataweb.NShape.Advanced.NamedImage" />.
		/// </summary>
		public NamedImage Clone() {
			return new NamedImage(this);
		}


		/// <summary>
		/// The image.
		/// </summary>
		public Image Image {
			get {
				if (_image == null && File.Exists(_filePath))
					Load(_filePath);
				return _image;
			}
			set {
				if (_image != value) {
					if (_image != null) {
						GdiHelpers.DisposeObject(ref _image);
						GdiHelpers.DisposeObject(ref _memStream);
					}
					//canLoadFromFile = false;
					//imageType = typeof(Image);
					_hashCode = null;
					_imageSize = Size.Empty;
					_image = value;
					if (_image != null) {
						_imageSize = _image.Size;
						//imageType = image.GetType();
					}
				}
			}
		}


		/// <summary>
		/// Name of the image (typically the filename without path and extension)
		/// </summary>
		public string Name {
			get {
				if (!string.IsNullOrEmpty(_name))
					return _name;
				else if (!string.IsNullOrEmpty(_filePath))
					return Path.GetFileNameWithoutExtension(_filePath);
				else return string.Empty;
			}
			set {
				if (string.IsNullOrEmpty(value))
					_name = string.Empty;
				else _name = value;
			}
		}


		/// <summary>
		/// Path to the image file
		/// </summary>
		public string FilePath {
			get { return _filePath; }
			set {
				if (string.IsNullOrEmpty(value))
					_filePath = string.Empty;
				else _filePath = value;
			}
		}


		/// <summary>
		/// Gets the hash code of the image data.
		/// If the image was not loaded yet, calculating the hash code will load the image first.
		/// </summary>
		public string ImageHash {
			get {
				if (string.IsNullOrEmpty(_hashCode) && Image != null) {
					byte[] imgData = (Image is Bitmap) ? GdiHelpers.GetRawImageData((Bitmap)Image) : GetEncodedImageData();
					_hashCode = GdiHelpers.CalculateHashCode(imgData);
				}
				return _hashCode;
			}
		}


		///// <summary>
		///// Gets the image's uncompressed raw pixel data as byte array.
		///// </summary>
		//internal byte[] GetRawImageData()
		//{
		//	byte[] result = null;
		//	if (_memStream == null && Image != null) {
		//		_memStream = new MemoryStream();
		//		GdiHelpers.SaveImage(Image, _memStream);
		//	}
		//	if (Image is Bitmap) {
		//		result = GdiHelpers.GetImageData(Image);
		//	} else {
		//		result = _memStream.ToArray();
		//	}
		//	return result;
		//}


		/// <summary>
		/// Gets the image's encoded data as byte array.
		/// </summary>
		internal byte[] GetEncodedImageData()
		{
			byte[] result = null;
			if (_memStream == null && Image != null) {
				// Save image if it has not been saved yet!
				_memStream = new MemoryStream();
				GdiHelpers.SaveImage(Image, _memStream);
			}
			if (_memStream != null)
				result = _memStream.ToArray();
			return result;
		}


		/// <summary>
		/// Width of the image. 0 if no image is set.
		/// </summary>
		public int Width {
			get { return _imageSize.Width; }
		}


		/// <summary>
		/// Height of the image. 0 if no image is set.
		/// </summary>
		public int Height {
			get { return _imageSize.Height; }
		}


		/// <summary>
		/// Size of the image. 0 if no image is set.
		/// </summary>
		public Size Size {
			get { return _imageSize; }
		}


		/// <summary>
		/// Returns true if FilePath refers to an existing file.
		/// </summary>
		/// <remarks>This property will not check whether the file is really a supported image file.</remarks>
		public bool ImageFileExists {
			get { return String.IsNullOrEmpty(FilePath) ? false : File.Exists(FilePath); }
		}


		/// <summary>
		/// Saves this NamedImage to an image file.
		/// </summary>
		/// <param name="directoryName"></param>
		/// <param name="format"></param>
		public void Save(string directoryName, ImageFormat format) {
			if (!Directory.Exists(directoryName))
				throw new FileNotFoundException(Properties.Resources.MessageTxt_DirectoryDoesNotExist, directoryName);
			string fileName = Name;
			if (string.IsNullOrEmpty(fileName)) fileName = Guid.NewGuid().ToString();
			fileName += GetImageFormatExtension(format);
			//_image.Save(Path.Combine(directoryName, fileName), format);
			GdiHelpers.SaveImage(_image, Path.Combine(directoryName, fileName), GdiHelpers.GetImageFileFormat(format));

			if (string.IsNullOrEmpty(_name) && string.IsNullOrEmpty(_filePath)) {
				_filePath = fileName;
				//canLoadFromFile = true;
			}
		}


		/// <summary>
		/// Loads the image file into this NamedImage.
		/// </summary>
		public void Load(string fileName) {
			if (!File.Exists(fileName)) throw new FileNotFoundException(Dataweb.NShape.Properties.Resources.MessageTxt_FileNotFoundOrAccessDenied, fileName);
			if (_image != null)
				GdiHelpers.DisposeObject(ref _image);
			_hashCode = null;
			// GDI+ only reads the file header and keeps the image file locked for loading the 
			// image data later on demand. 
			// So we have to read the entire image to a buffer and create the image from a MemoryStream
			byte[] buffer = File.ReadAllBytes(fileName);
			_memStream = null;
			try {
				// Create the image from the read byte buffer (MemoryStream constructor does *not* copy 
				// the buffer and must not be disposed in the lifetime of the image)
				// Do *not* calculate the hash code from the (compressed) bytes read from the file but from the 
				// (uncompressed) data of the memory stream!
				_memStream = new MemoryStream(buffer);
				_image = Image.FromStream(_memStream, true, true);
				_imageSize = _image.Size;
				_imageType = _image.GetType();
				if (!_canLoadFromFile) _canLoadFromFile = true;
				if (_filePath != fileName) _filePath = fileName;
			} catch (Exception) {
				if (_image == null && _memStream != null)
					GdiHelpers.DisposeObject(ref _memStream);
				_hashCode = null;
			}
		}


		/// <summary>
		/// Unloads the image.
		/// Must not be called if there is no file the image cannot be loaded from a file again.
		/// </summary>
		/// <remarks>Throws a FileNotFoundException if the FilePath of this NamedImage does not point to an image file.</remarks>
		public void Unload() {
			if (!File.Exists(FilePath)) throw new FileNotFoundException(Dataweb.NShape.Properties.Resources.MessageTxt_FileNotFoundOrAccessDenied, FilePath);
			GdiHelpers.DisposeObject(ref _image);
		}


		/// <summary>
		/// Gets the appropriate file extension for the given ImageFormat.
		/// </summary>
		private string GetImageFormatExtension(ImageFormat imageFormat) {
			string result = string.Empty;
			if (_image.RawFormat.Guid == ImageFormat.Bmp.Guid) result = ".bmp";
			else if (_image.RawFormat.Guid == ImageFormat.Emf.Guid) result = ".emf";
			else if (_image.RawFormat.Guid == ImageFormat.Exif.Guid) result = ".exif";
			else if (_image.RawFormat.Guid == ImageFormat.Gif.Guid) result = ".gif";
			else if (_image.RawFormat.Guid == ImageFormat.Icon.Guid) result = ".ico";
			else if (_image.RawFormat.Guid == ImageFormat.Jpeg.Guid) result = ".jpeg";
			else if (_image.RawFormat.Guid == ImageFormat.MemoryBmp.Guid) result = ".bmp";
			else if (_image.RawFormat.Guid == ImageFormat.Png.Guid) result = ".png";
			else if (_image.RawFormat.Guid == ImageFormat.Tiff.Guid) result = ".tiff";
			else if (_image.RawFormat.Guid == ImageFormat.Wmf.Guid) result = ".wmf";
			else Debug.Fail("Unsupported image format.");
			return result;
		}


		#region Fields
		
		private Image _image = null;
		private string _name = string.Empty;
		private string _filePath = string.Empty;
		private string _hashCode = null;
		private Size _imageSize = Size.Empty;
		private bool _canLoadFromFile = false;
		private Type _imageType = typeof(Image);
		private MemoryStream _memStream = null;

		#endregion
	}


	/// <ToBeCompleted></ToBeCompleted>
	public enum ExifPropertyTags {
		/// <ToBeCompleted></ToBeCompleted>
		ImageWidth = 0x100,
		/// <ToBeCompleted></ToBeCompleted>
		ImageLength = 0x101,
		/// <ToBeCompleted></ToBeCompleted>
		BitsPerSample = 0x102,
		/// <ToBeCompleted></ToBeCompleted>
		Compression = 0x103,
		/// <ToBeCompleted></ToBeCompleted>
		PhotometricInterpretation = 0x106,
		/// <ToBeCompleted></ToBeCompleted>
		FillOrder = 0x10A,
		/// <ToBeCompleted></ToBeCompleted>
		DocumentName = 0x10D,
		/// <ToBeCompleted></ToBeCompleted>
		ImageDescription = 0x10E,
		/// <ToBeCompleted></ToBeCompleted>
		Make = 0x10F,
		/// <ToBeCompleted></ToBeCompleted>
		Model = 0x110,
		/// <ToBeCompleted></ToBeCompleted>
		StripOffsets = 0x111,
		/// <ToBeCompleted></ToBeCompleted>
		Orientation = 0x112,
		/// <ToBeCompleted></ToBeCompleted>
		SamplesPerPixel = 0x115,
		/// <ToBeCompleted></ToBeCompleted>
		RowsPerStrip = 0x116,
		/// <ToBeCompleted></ToBeCompleted>
		StripByteCounts = 0x117,
		/// <ToBeCompleted></ToBeCompleted>
		XResolution = 0x11A,
		/// <ToBeCompleted></ToBeCompleted>
		YResolution = 0x11B,
		/// <ToBeCompleted></ToBeCompleted>
		PlanarConfiguration = 0x11C,
		/// <ToBeCompleted></ToBeCompleted>
		ResolutionUnit = 0x128,
		/// <ToBeCompleted></ToBeCompleted>
		TransferFunction = 0x12D,
		/// <ToBeCompleted></ToBeCompleted>
		Software = 0x131,
		/// <ToBeCompleted></ToBeCompleted>
		DateTime = 0x132,
		/// <ToBeCompleted></ToBeCompleted>
		Artist = 0x13B,
		/// <ToBeCompleted></ToBeCompleted>
		WhitePoint = 0x13E,
		/// <ToBeCompleted></ToBeCompleted>
		PrimaryChromaticities = 0x13F,
		/// <ToBeCompleted></ToBeCompleted>
		TransferRange = 0x156,
		/// <ToBeCompleted></ToBeCompleted>
		JPEGProc = 0x200,
		/// <ToBeCompleted></ToBeCompleted>
		JPEGInterchangeFormat = 0x201,
		/// <ToBeCompleted></ToBeCompleted>
		JPEGInterchangeFormatLength = 0x202,
		/// <ToBeCompleted></ToBeCompleted>
		YCbCrCoefficients = 0x211,
		/// <ToBeCompleted></ToBeCompleted>
		YCbCrSubSampling = 0x212,
		/// <ToBeCompleted></ToBeCompleted>
		YCbCrPositioning = 0x213,
		/// <ToBeCompleted></ToBeCompleted>
		ReferenceBlackWhite = 0x214,
		/// <ToBeCompleted></ToBeCompleted>
		BatteryLevel = 0x828F,
		/// <ToBeCompleted></ToBeCompleted>
		Copyright = 0x8298,
		/// <ToBeCompleted></ToBeCompleted>
		ExposureTime = 0x829A,
		/// <ToBeCompleted></ToBeCompleted>
		FNumber = 0x829D,
		/// <ToBeCompleted></ToBeCompleted>
		IPTC_NAA = 0x83BB,
		/// <ToBeCompleted></ToBeCompleted>
		ExifIFDPointer = 0x8769,
		/// <ToBeCompleted></ToBeCompleted>
		InterColorProfile = 0x8773,
		/// <ToBeCompleted></ToBeCompleted>
		ExposureProgram = 0x8822,
		/// <ToBeCompleted></ToBeCompleted>
		SpectralSensitivity = 0x8824,
		/// <ToBeCompleted></ToBeCompleted>
		GPSInfoIFDPointer = 0x8825,
		/// <ToBeCompleted></ToBeCompleted>
		ISOSpeedRatings = 0x8827,
		/// <ToBeCompleted></ToBeCompleted>
		OECF = 0x8828,
		/// <ToBeCompleted></ToBeCompleted>
		ExifVersion = 0x9000,
		/// <ToBeCompleted></ToBeCompleted>
		DateTimeOriginal = 0x9003,
		/// <ToBeCompleted></ToBeCompleted>
		DateTimeDigitized = 0x9004,
		/// <ToBeCompleted></ToBeCompleted>
		ComponentsConfiguration = 0x9101,
		/// <ToBeCompleted></ToBeCompleted>
		CompressedBitsPerPixel = 0x9102,
		/// <ToBeCompleted></ToBeCompleted>
		ShutterSpeedValue = 0x9201,
		/// <ToBeCompleted></ToBeCompleted>
		ApertureValue = 0x9202,
		/// <ToBeCompleted></ToBeCompleted>
		BrightnessValue = 0x9203,
		/// <ToBeCompleted></ToBeCompleted>
		ExposureBiasValue = 0x9204,
		/// <ToBeCompleted></ToBeCompleted>
		MaxApertureValue = 0x9205,
		/// <ToBeCompleted></ToBeCompleted>
		SubjectDistance = 0x9206,
		/// <ToBeCompleted></ToBeCompleted>
		MeteringMode = 0x9207,
		/// <ToBeCompleted></ToBeCompleted>
		LightSource = 0x9208,
		/// <ToBeCompleted></ToBeCompleted>
		Flash = 0x9209,
		/// <ToBeCompleted></ToBeCompleted>
		FocalLength = 0x920A,
		/// <ToBeCompleted></ToBeCompleted>
		SubjectArea = 0x9214,
		/// <ToBeCompleted></ToBeCompleted>
		MakerNote = 0x927C,
		/// <ToBeCompleted></ToBeCompleted>
		UserComment = 0x9286,
		/// <ToBeCompleted></ToBeCompleted>
		SubSecTime = 0x9290,
		/// <ToBeCompleted></ToBeCompleted>
		SubSecTimeOriginal = 0x9291,
		/// <ToBeCompleted></ToBeCompleted>
		SubSecTimeDigitized = 0x9292,
		/// <ToBeCompleted></ToBeCompleted>
		FlashPixVersion = 0xA000,
		/// <ToBeCompleted></ToBeCompleted>
		ColorSpace = 0xA001,
		/// <ToBeCompleted></ToBeCompleted>
		PixelXDimension = 0xA002,
		/// <ToBeCompleted></ToBeCompleted>
		PixelYDimension = 0xA003,
		/// <ToBeCompleted></ToBeCompleted>
		RelatedSoundFile = 0xA004,
		/// <ToBeCompleted></ToBeCompleted>
		InteroperabilityIFDPointer = 0xA005,
		/// <ToBeCompleted></ToBeCompleted>
		FlashEnergy = 0xA20B,
		/// <ToBeCompleted></ToBeCompleted>
		FlashEnergy_TIFF = 0x920B,
		/// <ToBeCompleted></ToBeCompleted>
		SpatialFrequencyResponse = 0xA20C,
		/// <ToBeCompleted></ToBeCompleted>
		SpatialFrequencyResponse_TIFF = 0x920C,
		/// <ToBeCompleted></ToBeCompleted>
		FocalPlaneXResolution = 0xA20E,
		/// <ToBeCompleted></ToBeCompleted>
		FocalPlaneXResolution_TIFF = 0x920E,
		/// <ToBeCompleted></ToBeCompleted>
		FocalPlaneYResolution = 0xA20F,
		/// <ToBeCompleted></ToBeCompleted>
		FocalPlaneYResolution_TIFF = 0x920F,
		/// <ToBeCompleted></ToBeCompleted>
		FocalPlaneResolutionUnit = 0xA210,
		/// <ToBeCompleted></ToBeCompleted>
		FocalPlaneResolutionUnit_TIFF = 0x9210,
		/// <ToBeCompleted></ToBeCompleted>
		SubjectLocation = 0xA214,
		/// <ToBeCompleted></ToBeCompleted>
		SubjectLocation_TIFF = 0x9214,
		/// <ToBeCompleted></ToBeCompleted>
		ExposureIndex = 0xA215,
		/// <ToBeCompleted></ToBeCompleted>
		ExposureIndex_TIFF = 0x9215,
		/// <ToBeCompleted></ToBeCompleted>
		SensingMethod = 0xA217,
		/// <ToBeCompleted></ToBeCompleted>
		SensingMethod_TIFF = 0x9217,
		/// <ToBeCompleted></ToBeCompleted>
		FileSource = 0xA300,
		/// <ToBeCompleted></ToBeCompleted>
		SceneType = 0xA301,
		/// <ToBeCompleted></ToBeCompleted>
		CFAPattern = 0xA302,
		/// <ToBeCompleted></ToBeCompleted>
		CFAPattern_TIFF = 0x828E,
		/// <ToBeCompleted></ToBeCompleted>
		CustomRendered = 0xA401,
		/// <ToBeCompleted></ToBeCompleted>
		ExposureMode = 0xA402,
		/// <ToBeCompleted></ToBeCompleted>
		WhiteBalance = 0xA403,
		/// <ToBeCompleted></ToBeCompleted>
		DigitalZoomRatio = 0xA404,
		/// <ToBeCompleted></ToBeCompleted>
		FocalLengthIn35mmFilm = 0xA405,
		/// <ToBeCompleted></ToBeCompleted>
		SceneCaptureType = 0xA406,
		/// <ToBeCompleted></ToBeCompleted>
		GainControl = 0xA407,
		/// <ToBeCompleted></ToBeCompleted>
		Contrast = 0xA408,
		/// <ToBeCompleted></ToBeCompleted>
		Saturation = 0xA409,
		/// <ToBeCompleted></ToBeCompleted>
		Sharpness = 0xA40A,
		/// <ToBeCompleted></ToBeCompleted>
		DeviceSettingDescription = 0xA40B,
		/// <ToBeCompleted></ToBeCompleted>
		SubjectDistanceRange = 0xA40C,
		/// <ToBeCompleted></ToBeCompleted>
		ImageUniqueID = 0xA420
	}


	/// <ToBeCompleted></ToBeCompleted>
	public enum ExifPropertyTypes {
		/// <ToBeCompleted></ToBeCompleted>
		Byte = 0x1,
		/// <ToBeCompleted></ToBeCompleted>
		ASCII = 0x2,
		/// <ToBeCompleted></ToBeCompleted>
		Short = 0x3,
		/// <ToBeCompleted></ToBeCompleted>
		Long = 0x4,
		/// <ToBeCompleted></ToBeCompleted>
		Rational = 0x5,
		/// <ToBeCompleted></ToBeCompleted>
		Undefined = 0x7,
		/// <ToBeCompleted></ToBeCompleted>
		SLONG = 0x9,
		/// <ToBeCompleted></ToBeCompleted>
		SRational = 0xA
	}


	/// <ToBeCompleted></ToBeCompleted>
	public enum ExifIntrTags {
		/// <ToBeCompleted></ToBeCompleted>
		InteroperabilityIndex = 0x0001,
		/// <ToBeCompleted></ToBeCompleted>
		InteroperabilityVersion = 0x0002,
		/// <ToBeCompleted></ToBeCompleted>
		RelatedImageFileFormat = 0x1000,
		/// <ToBeCompleted></ToBeCompleted>
		RelatedImageWidth = 0x1001,
		/// <ToBeCompleted></ToBeCompleted>
		RelatedImageLength = 0x1002,
	}


	/// <ToBeCompleted></ToBeCompleted>
	public enum ExifGpsTags {
		/// <ToBeCompleted></ToBeCompleted>
		GPSVersionID = 0x0,
		/// <ToBeCompleted></ToBeCompleted>
		GPSLatitudeRef = 0x1,
		/// <ToBeCompleted></ToBeCompleted>
		GPSLatitude = 0x2,
		/// <ToBeCompleted></ToBeCompleted>
		GPSLongitudeRef = 0x3,
		/// <ToBeCompleted></ToBeCompleted>
		GPSLongitude = 0x4,
		/// <ToBeCompleted></ToBeCompleted>
		GPSAltitudeRef = 0x5,
		/// <ToBeCompleted></ToBeCompleted>
		GPSAltitude = 0x6,
		/// <ToBeCompleted></ToBeCompleted>
		GPSTimeStamp = 0x7,
		/// <ToBeCompleted></ToBeCompleted>
		GPSSatellites = 0x8,
		/// <ToBeCompleted></ToBeCompleted>
		GPSStatus = 0x9,
		/// <ToBeCompleted></ToBeCompleted>
		GPSMeasureMode = 0xA,
		/// <ToBeCompleted></ToBeCompleted>
		GPSDOP = 0xB,
		/// <ToBeCompleted></ToBeCompleted>
		GPSSpeedRef = 0xC,
		/// <ToBeCompleted></ToBeCompleted>
		GPSSpeed = 0xD,
		/// <ToBeCompleted></ToBeCompleted>
		GPSTrackRef = 0xE,
		/// <ToBeCompleted></ToBeCompleted>
		GPSTrack = 0xF,
		/// <ToBeCompleted></ToBeCompleted>
		GPSImgDirectionRef = 0x10,
		/// <ToBeCompleted></ToBeCompleted>
		GPSImgDirection = 0x11,
		/// <ToBeCompleted></ToBeCompleted>
		GPSMapDatum = 0x12,
		/// <ToBeCompleted></ToBeCompleted>
		GPSDestLatitudeRef = 0x13,
		/// <ToBeCompleted></ToBeCompleted>
		GPSDestLatitude = 0x14,
		/// <ToBeCompleted></ToBeCompleted>
		GPSDestLongitudeRef = 0x15,
		/// <ToBeCompleted></ToBeCompleted>
		GPSDestLongitude = 0x16,
		/// <ToBeCompleted></ToBeCompleted>
		GPSDestBearingRef = 0x17,
		/// <ToBeCompleted></ToBeCompleted>
		GPSDestBearing = 0x18,
		/// <ToBeCompleted></ToBeCompleted>
		GPSDestDistanceRef = 0x19,
		/// <ToBeCompleted></ToBeCompleted>
		GPSDestDistance = 0x1A,
		/// <ToBeCompleted></ToBeCompleted>
		GPSProcessingMethod = 0x1B,
		/// <ToBeCompleted></ToBeCompleted>
		GPSAreaInformation = 0x1C,
		/// <ToBeCompleted></ToBeCompleted>
		GPSDateStamp = 0x1D,
		/// <ToBeCompleted></ToBeCompleted>
		GPSDifferential = 0x1E
	}

}

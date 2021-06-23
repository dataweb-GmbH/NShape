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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace Dataweb.NShape.Advanced {

	// Based on code from http://www.dotnet247.com/247reference/msgs/23/118514.aspx
	/// <summary>
	/// Helper class for copying and deleting EMF files.
	/// </summary>
	internal static class EmfHelper {

		[DllImport("gdi32.dll")]
		static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, string fileName);

		[DllImport("gdi32.dll")]
		public static extern uint GetEnhMetaFileBits(IntPtr hemfSrc, uint bufferSize, byte[] buffer);
		
		[DllImport("gdi32.dll")]
		public static extern IntPtr SetEnhMetaFileBits(uint bufferSize, byte[] buffer);

		[DllImport("gdi32.dll")]
		static extern bool DeleteEnhMetaFile(IntPtr hemf);


		/// <summary>
		/// Copies the given <see cref="T:System.Drawing.Imaging.MetaFile" /> to the clipboard.
		/// The given <see cref="T:System.Drawing.Imaging.MetaFile" /> is set to an invalid state inside this function.
		/// </summary>
		[Obsolete("Use ClipboardHelper.PutEnhMetafileOnClipboard instead.")]
		static public bool PutEnhMetafileOnClipboard(IntPtr hWnd, Metafile metafile) {
			return ClipboardHelper.PutEnhMetafileOnClipboard(hWnd, metafile);
		}


		/// <summary>
		/// Copies the given <see cref="T:System.Drawing.Imaging.MetaFile" /> to the clipboard.
		/// The given <see cref="T:System.Drawing.Imaging.MetaFile" /> is set to an invalid state inside this function.
		/// </summary>
		[Obsolete("Use ClipboardHelper.PutEnhMetafileOnClipboard instead.")]
		static public bool PutEnhMetafileOnClipboard(IntPtr hWnd, Metafile metafile, bool clearClipboard) {
			return ClipboardHelper.PutEnhMetafileOnClipboard(hWnd, metafile, clearClipboard);
		}


		/// <summary>
		/// Copies the given <see cref="T:System.Drawing.Imaging.MetaFile" /> to the specified file. If the file does not exist, it will be created.
		/// The given <see cref="T:System.Drawing.Imaging.MetaFile" /> is set to an invalid state inside this function.
		/// </summary>
		static public bool SaveEnhMetaFile(string fileName, Metafile metafile){
			if (metafile == null) throw new ArgumentNullException("metafile");
			bool result = false;
			IntPtr hEmf = metafile.GetHenhmetafile();
			if (hEmf != IntPtr.Zero) {
				IntPtr resHEnh = CopyEnhMetaFile(hEmf, fileName);
				if (resHEnh != IntPtr.Zero) {
					DeleteEnhMetaFile(resHEnh);
					result = true;
				}
				DeleteEnhMetaFile(hEmf);
				metafile.Dispose();
			}
			return result;
		}


		/// <summary>
		/// Copies the given <see cref="T:System.Drawing.Imaging.MetaFile" /> to the specified stream.
		/// The given <see cref="T:System.Drawing.Imaging.MetaFile" /> is set to an invalid state inside this function.
		/// </summary>
		static public bool SaveEnhMetaFile(Stream stream, Metafile metafile) {
			if (metafile == null) throw new ArgumentNullException("metafile");
			bool result = false;
			IntPtr hEmf = metafile.GetHenhmetafile();
			if (hEmf != IntPtr.Zero) {
				// Get required buffer size
				uint bufferSize = GetEnhMetaFileBits(hEmf, 0, null);
				// Create and fill buffer 
				byte[] buffer = new byte[bufferSize];
				uint res = GetEnhMetaFileBits(hEmf, bufferSize, buffer);
				if (res > 0) 
					stream.Write(buffer, 0, (int)bufferSize);
			}
			return result;
		}
	
	}


		/// <summary>
	/// Helper class for copying and deleting EMF files.
	/// </summary>
	internal static class ClipboardHelper {

		[DllImport("user32.dll")]
		public static extern bool OpenClipboard(IntPtr hWndNewOwner);

		[DllImport("user32.dll")]
		public static extern bool EmptyClipboard();

		[DllImport("user32.dll")]
		public static extern bool CloseClipboard();


		/// <summary>
		/// Copies the given <see cref="T:System.Drawing.Imaging.MetaFile" /> to the clipboard.
		/// The given <see cref="T:System.Drawing.Imaging.MetaFile" /> is set to an invalid state and deleted inside this function.
		/// </summary>
		public static bool AddEnhMetafileToClipboard(Metafile metafile) {
			if (metafile == null) throw new ArgumentNullException("metafile");
			bool result = false;
			IntPtr hEMF, hEMF2;
			hEMF = metafile.GetHenhmetafile(); // invalidates metafile
			if (!hEMF.Equals(IntPtr.Zero)) {
				try {
					hEMF2 = CopyEnhMetaFile(hEMF, null);
					if (!hEMF2.Equals(IntPtr.Zero)) {
						IntPtr hRes = SetClipboardData(CF_ENHMETAFILE, hEMF2);
						result = hRes.Equals(hEMF2);
					}
				} finally {
					DeleteEnhMetaFile(hEMF);
				}
			}
			return result;
		}


		/// <summary>
		/// Opens the clipboard, clears it and copies the given <see cref="T:System.Drawing.Imaging.MetaFile" /> to the clipboard.
		/// The given <see cref="T:System.Drawing.Imaging.MetaFile" /> is set to an invalid state and deleted inside this function.
		/// </summary>
		public static bool PutEnhMetafileOnClipboard(IntPtr hWnd, Metafile metafile) {
			return PutEnhMetafileOnClipboard(hWnd, metafile, true);
		}


		/// <summary>
		/// Opens the clipboard, clears it by request and copies the given <see cref="T:System.Drawing.Imaging.MetaFile" /> to the clipboard.
		/// The given <see cref="T:System.Drawing.Imaging.MetaFile" /> is set to an invalid state and deleted inside this function.
		/// </summary>
		public static bool PutEnhMetafileOnClipboard(IntPtr hWnd, Metafile metafile, bool clearClipboard) {
			if (metafile == null) throw new ArgumentNullException("metafile");
			bool result = false;
			if (OpenClipboard(hWnd)) {
				try {
					if (clearClipboard) {
						if (!EmptyClipboard())
							return false;
					}
					AddEnhMetafileToClipboard(metafile);
				} finally {
					CloseClipboard();
				}
			}
			return result;
		}


		/// <summary>
		/// Copies the given <see cref="T:System.Drawing.Bitmap" /> to the clipboard.
		/// The given <see cref="T:System.Drawing.Imaging.MetaFile" /> is set to an invalid state inside this function.
		/// </summary>
		public static bool AddBitmapToClipboard(Bitmap bitmap) {
			if (bitmap == null) throw new ArgumentNullException("bitmap");
			bool result = false;
			IntPtr hBMP, hBMP2;
			hBMP = bitmap.GetHbitmap(); // invalidates bitmap
			if (!hBMP.Equals(IntPtr.Zero)) {
				try {
					bool createDIBitmap = false;
					uint flags = LR_DEFAULTCOLOR | LR_DEFAULTSIZE;
					if (createDIBitmap) flags |= LR_CREATEDIBSECTION;

					hBMP2 = CopyImage(hBMP, IMAGE_BITMAP, bitmap.Width, bitmap.Height, flags);
					if (!hBMP2.Equals(IntPtr.Zero)) {
						IntPtr hRes = SetClipboardData(createDIBitmap ? CF_DIB : CF_BITMAP, hBMP2);
						result = hRes.Equals(hBMP2);
					}
				} finally {
					DeleteObject(hBMP);
				}
			}
			return result;
		}


		/// <summary>
		/// Opens the clipboard, clears it by request and copies the given <see cref="T:System.Drawing.Bitmap" /> to the clipboard.
		/// The given <see cref="T:System.Drawing.Imaging.MetaFile" /> is set to an invalid state inside this function.
		/// </summary>
		public static bool PutBitmapOnClipboard(IntPtr hWnd, Bitmap bitmap) {
			return PutBitmapOnClipboard(hWnd, bitmap, true);
		}


		/// <summary>
		/// Opens the clipboard, clears it by request and copies the given <see cref="T:System.Drawing.Bitmap" /> to the clipboard.
		/// The given <see cref="T:System.Drawing.Imaging.MetaFile" /> is set to an invalid state inside this function.
		/// </summary>
		public static bool PutBitmapOnClipboard(IntPtr hWnd, Bitmap bitmap, bool clearClipboard) {
			if (bitmap == null) throw new ArgumentNullException("bitmap");
			bool result = false;
			if (OpenClipboard(hWnd)) {
				try {
					if (clearClipboard) {
						if (!EmptyClipboard())
							return false;
					}
					result = AddBitmapToClipboard(bitmap);
				} finally {
					CloseClipboard();
				}
			}
			return result;
		}


		[DllImport("user32.dll")]
		static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

		[DllImport("gdi32.dll")]
		static extern IntPtr CopyEnhMetaFile(IntPtr hemfSrc, string fileName);

		[DllImport("user32.dll")]
		static extern IntPtr CopyImage(IntPtr hdl, uint type, int width, int height, uint flags);

		[DllImport("gdi32.dll")]
		static extern bool DeleteEnhMetaFile(IntPtr hemf);

		[DllImport("gdi32.dll")]
		static extern bool DeleteObject(IntPtr hobj);


		const uint IMAGE_BITMAP = 0;
		const uint IMAGE_ICON = 1;
		const uint IMAGE_CURSOR = 2;
		const uint IMAGE_ENHMETAFILE = 3;

		const uint LR_DEFAULTCOLOR = 0x00000000;
		const uint LR_DEFAULTSIZE = 0x00000040;
		const uint LR_CREATEDIBSECTION = 0x00002000;

		const uint CF_BITMAP = 2;
		const uint CF_DIB = 8;
		const uint CF_ENHMETAFILE = 14;

	}

}

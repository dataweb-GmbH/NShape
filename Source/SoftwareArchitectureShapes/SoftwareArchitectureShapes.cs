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
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Reflection;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;


namespace Dataweb.NShape.SoftwareArchitectureShapes {

	#region Commands for editing EntitySymbol columns

	public abstract class EntityShapeColumnCommand : Command {

		protected EntityShapeColumnCommand(EntitySymbol shape, string columnText)
			: base() {
			this._shape = shape;
			this.columnText = columnText;
		}


		protected EntitySymbol Shape {
			get { return _shape; }
		}
		
		protected string ColumnText {
			get { return columnText; }
		}


		public override Permission RequiredPermission {
			get { return Permission.Data; }
		}


		/// <override></override>
		protected override bool CheckAllowedCore(ISecurityManager securityManager, bool createException, out Exception exception) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			bool isGranted = securityManager.IsGranted(RequiredPermission, Shape.SecurityDomainName);
			exception = (!isGranted && createException) ? new NShapeSecurityException(this) : null;
			return isGranted;
		}


		private EntitySymbol _shape;
		private string columnText;
	}


	public class AddColumnCommand : EntityShapeColumnCommand {

		public AddColumnCommand(EntitySymbol shape, string columnText)
			: base(shape, columnText) {
			base.Description = string.Format("Add column to {0}", shape.Type.Name);
		}
		
		
		#region ICommand Members

		/// <override></override>
		public override void Execute() {
			Shape.AddColumn(ColumnText);
			if (Repository != null) Repository.Update((Shape)Shape);
		}


		/// <override></override>
		public override void Revert() {
			Shape.RemoveColumn(ColumnText);
			if (Repository != null) Repository.Update((Shape)Shape);
		}

		#endregion

	}


	public class InsertColumnCommand : EntityShapeColumnCommand {
		
		public InsertColumnCommand(EntitySymbol shape, int beforeColumnIndex, string columnText)
			: base(shape, columnText) {
			base.Description = string.Format("Insert new column in {0}", shape.Type.Name);
			this.beforeIndex = beforeColumnIndex;
		}


		#region ICommand Members

		/// <override></override>
		public override void Execute() {
			Shape.AddColumn(Shape.GetCaptionText(Shape.CaptionCount - 1));
			for (int i = Shape.CaptionCount - 2; i > beforeIndex; --i)
				Shape.SetCaptionText(i, Shape.GetCaptionText(i-1));
			Shape.SetCaptionText(beforeIndex, ColumnText);
			if (Repository != null) Repository.Update((Shape)Shape);
		}


		/// <override></override>
		public override void Revert() {
			for (int i = Shape.CaptionCount - 1; i > beforeIndex; --i)
				Shape.SetCaptionText(i - 1, Shape.GetCaptionText(i));
			// The shape's Text does count as caption but not as column, that's why CaptionCount-2.
			Shape.RemoveColumnAt(Shape.CaptionCount - 2);
			if (Repository != null) Repository.Update((Shape)Shape);
		}

		#endregion


		private int beforeIndex;
	}


	public class EditColumnCommand : EntityShapeColumnCommand {

		public EditColumnCommand(EntitySymbol shape, int columnIndex, string columnText)
			: base(shape, columnText) {
			base.Description = string.Format("Edit column '{0}' in {1}", columnText, shape.Type.Name);
			this.oldColumnText = shape.ColumnNames[columnIndex];
			this.columnIndex = columnIndex;
		}


		#region ICommand Members

		/// <override></override>
		public override void Execute() {
			string[] columns = new string[Shape.ColumnNames.Length];
			Array.Copy(Shape.ColumnNames, columns, Shape.ColumnNames.Length);
			columns[columnIndex] = ColumnText;

			Shape.ColumnNames = columns;
			if (Repository != null) Repository.Update((Shape)Shape);
		}


		/// <override></override>
		public override void Revert() {
			string[] columns = new string[Shape.ColumnNames.Length];
			Array.Copy(Shape.ColumnNames, columns, Shape.ColumnNames.Length);
			columns[columnIndex] = oldColumnText;

			Shape.ColumnNames = columns;
			if (Repository != null) Repository.Update((Shape)Shape);
		}

		#endregion


		private string oldColumnText;
		private int columnIndex;
	}


	public class RemoveColumnCommand : EntityShapeColumnCommand {

		public RemoveColumnCommand(EntitySymbol shape, int removeColumnIndex, string columnText)
			: base(shape, columnText) {
			base.Description = string.Format("Remove column '{0}' from {1}", columnText, shape.Type.Name);
			this.removeIndex = removeColumnIndex;
		}


		#region ICommand Members

		/// <override></override>
		public override void Execute() {
			int maxCaptionIdx = Shape.CaptionCount - 1;
			for (int i = removeIndex; i < maxCaptionIdx; ++i)
				Shape.SetCaptionText(i, Shape.GetCaptionText(i + 1));
			// The shape's Text does count as caption but not as column, that's why maxCaptionIdx - 1.
			Shape.RemoveColumnAt(maxCaptionIdx - 1);
			if (Repository != null) Repository.Update((Shape)Shape);
		}


		/// <override></override>
		public override void Revert() {
			Shape.AddColumn(Shape.GetCaptionText(Shape.CaptionCount - 1));
			for (int i = Shape.CaptionCount - 2; i > removeIndex; --i)
				Shape.SetCaptionText(i, Shape.GetCaptionText(i - 1));
			Shape.SetCaptionText(removeIndex, ColumnText);
			if (Repository != null) Repository.Update((Shape)Shape);
		}

		#endregion


		private int removeIndex;
	}

	#endregion


	public class EntitySymbol : RectangleBase {

		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameAppendColumn = "AppendColumnAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameInsertColumn = "InsertColumnAction";
		/// <ToBeCompleted></ToBeCompleted>
		public const string MenuItemNameRemoveColumn = "RemoveColumnAction";


		/// <override></override>
		public override void CopyFrom(Shape source) {
			base.CopyFrom(source);
			// Copy default styles for captions before copying the caption's properties
			if (source is EntitySymbol) {
				EntitySymbol src = (EntitySymbol)source;
				this.ColumnBackgroundColorStyle = src.ColumnBackgroundColorStyle;
				this.ColumnCharacterStyle = src.ColumnCharacterStyle;
				this.ColumnParagraphStyle = src.ColumnParagraphStyle;
			}
			// Copy captions via the ICaptionedShape interface
			if (source is ICaptionedShape) {
				ClearColumns();
				ICaptionedShape src = (ICaptionedShape)source;
				for (int i = 1; i < src.CaptionCount; ++i) {
					if (CaptionCount < src.CaptionCount)
						AddColumn(src.GetCaptionText(i));
					else SetCaptionText(i, src.GetCaptionText(i));
					SetCaptionCharacterStyle(i, src.GetCaptionCharacterStyle(i));
					SetCaptionParagraphStyle(i, src.GetCaptionParagraphStyle(i));
				}
			}
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new EntitySymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override void MakePreview(IStyleSet styleSet) {
			base.MakePreview(styleSet);
			_privateColumnCharacterStyle = styleSet.GetPreviewStyle(ColumnCharacterStyle);
			_privateColumnParagraphStyle = styleSet.GetPreviewStyle(ColumnParagraphStyle);
			_privateColumnBackgroundColorStyle = styleSet.GetPreviewStyle(ColumnBackgroundColorStyle);
		}


		/// <override></override>
		public override bool HasStyle(IStyle style) {
			if (IsStyleAffected(ColumnBackgroundColorStyle, style) 
				|| IsStyleAffected(ColumnCharacterStyle, style) 
				|| IsStyleAffected(ColumnParagraphStyle, style))
				return true;
			else return base.HasStyle(style);
		}


		#region IEntity Implementation

		/// <override></override>
		protected override void SaveFieldsCore(IRepositoryWriter writer, int version) {
			base.SaveFieldsCore(writer, version);
			writer.WriteStyle(ColumnBackgroundColorStyle);
			writer.WriteStyle(ColumnCharacterStyle);
			writer.WriteStyle(ColumnParagraphStyle);
			writer.WriteInt32(ColumnNames.Length);
		}


		/// <override></override>
		protected override void LoadFieldsCore(IRepositoryReader reader, int version) {
			base.LoadFieldsCore(reader, version);
			ColumnBackgroundColorStyle = reader.ReadColorStyle();
			ColumnCharacterStyle = reader.ReadCharacterStyle();
			ColumnParagraphStyle = reader.ReadParagraphStyle();
			int colCnt = reader.ReadInt32();
			if (_columnNames == null) _columnNames = new string[colCnt];
			else Array.Resize(ref _columnNames, colCnt);
		}


		/// <override></override>
		protected override void SaveInnerObjectsCore(string propertyName, IRepositoryWriter writer, int version) {
			if (propertyName == attrNameColumns) {
				writer.BeginWriteInnerObjects();
				int cnt = CaptionCount;
				for (int i = 1; i < cnt; ++i) {	// Skip first caption (title)
					writer.BeginWriteInnerObject();
					writer.WriteInt32(i - 1);
					writer.WriteString(GetCaptionText(i));
					writer.EndWriteInnerObject();
				}
				writer.EndWriteInnerObjects();
			} else base.SaveInnerObjectsCore(propertyName, writer, version);
		}


		/// <override></override>
		protected override void LoadInnerObjectsCore(string propertyName, IRepositoryReader reader, int version) {
			if (propertyName == attrNameColumns) {
				reader.BeginReadInnerObjects();
				while (reader.BeginReadInnerObject()) {
					int colIdx = reader.ReadInt32();
					string colName = reader.ReadString();
					reader.EndReadInnerObject();
					InsertColumn(colIdx, colName);
				}
				reader.EndReadInnerObjects();
			} else base.LoadInnerObjectsCore(propertyName, reader, version);
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.SoftwareArchitectureShapes.EntitySymbol" />.
		/// </summary>
		public static new IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			foreach (EntityPropertyDefinition pi in RectangleBase.GetPropertyDefinitions(version))
				yield return pi;
			yield return new EntityFieldDefinition("ColumnBackgroundColorStyle", typeof(object));
			yield return new EntityFieldDefinition("ColumnCharacterStyle", typeof(object));
			yield return new EntityFieldDefinition("ColumnParagraphStyle", typeof(object));
			yield return new EntityFieldDefinition("ColumnCount", typeof(int));
			yield return new EntityInnerObjectsDefinition(attrNameColumns, attrNameColumn, columnAttrNames, columnAttrTypes);
		}

		#endregion


		#region ICaptionedShape Implementation

		/// <override></override>
		public override int CaptionCount { get { return base.CaptionCount + _columnCaptions.Count; } }


		/// <override></override>
		public override bool GetCaptionBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			if (index < base.CaptionCount)
				return base.GetCaptionBounds(index, out topLeft, out topRight, out bottomRight, out bottomLeft);
			else {
				int idx = index - 1;
				Rectangle captionBounds;
				CalcCaptionBounds(index, out captionBounds);
				Geometry.TransformRectangle(Center, Angle, captionBounds, out topLeft, out topRight, out bottomRight, out bottomLeft);
				return (Geometry.ConvexPolygonContainsPoint(_columnFrame, bottomLeft.X, bottomLeft.Y)
					&& Geometry.ConvexPolygonContainsPoint(_columnFrame, bottomRight.X, bottomRight.Y));
			}
		}


		/// <override></override>
		public override bool GetCaptionTextBounds(int index, out Point topLeft, out Point topRight, out Point bottomRight, out Point bottomLeft) {
			if (index < base.CaptionCount) {
				return base.GetCaptionTextBounds(index, out topLeft, out topRight, out bottomRight, out bottomLeft);
			} else {
				int idx = index - 1;
				Rectangle bounds;
				CalcCaptionBounds(index, out bounds);
				bounds = _columnCaptions[idx].CalculateTextBounds(bounds, ColumnCharacterStyle, ColumnParagraphStyle, DisplayService);
				Geometry.TransformRectangle(Center, Angle, bounds, out topLeft, out topRight, out bottomRight, out bottomLeft);
				return (Geometry.QuadrangleContainsPoint(_columnFrame[0], _columnFrame[1], _columnFrame[2], _columnFrame[3], topLeft.X, topLeft.Y)
					&& Geometry.QuadrangleContainsPoint(_columnFrame[0], _columnFrame[1], _columnFrame[2], _columnFrame[3], bottomRight.X, bottomRight.Y));
			}
		}


		/// <override></override>
		public override string GetCaptionText(int index) {
			if (index < base.CaptionCount)
				return base.GetCaptionText(index);
			else
				return _columnCaptions[index - 1].Text;
		}


		/// <override></override>
		public override ICharacterStyle GetCaptionCharacterStyle(int index) {
			if (index < base.CaptionCount)
				return base.GetCaptionCharacterStyle(index);
			else return ColumnCharacterStyle;
		}


		/// <override></override>
		public override IParagraphStyle GetCaptionParagraphStyle(int index) {
			if (index < base.CaptionCount)
				return base.GetCaptionParagraphStyle(index);
			else return ColumnParagraphStyle;
		}


		/// <override></override>
		public override void SetCaptionText(int index, string text) {
			if (index < base.CaptionCount)
				base.SetCaptionText(index, text);
			else {
				Invalidate();
				_columnCaptions[index - 1].Text = text;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		/// <override></override>
		public override void SetCaptionCharacterStyle(int index, ICharacterStyle characterStyle) {
			if (index < base.CaptionCount)
				base.SetCaptionCharacterStyle(index, characterStyle);
			else {
				int idx = index - 1;
				// Create if needed
				if (_columnCharacterStyles == null)
					_columnCharacterStyles = new SortedList<int, ICharacterStyle>(1);
				// Set private style for a single caption
				if (characterStyle != ColumnCharacterStyle) {
					if (!_columnCharacterStyles.ContainsKey(idx))
						_columnCharacterStyles.Add(idx, characterStyle);
					else _columnCharacterStyles[idx] = characterStyle;
				} else {
					if (_columnCharacterStyles != null) {
						if (_columnCharacterStyles.ContainsKey(idx))
							_columnCharacterStyles.Remove(idx);
						// Delete if not needed any more
						if (_columnCharacterStyles.Count == 0)
							_columnCharacterStyles = null;
					}
				}
			}
		}


		/// <override></override>
		public override void SetCaptionParagraphStyle(int index, IParagraphStyle paragraphStyle) {
			if (index < base.CaptionCount)
				base.SetCaptionParagraphStyle(index, paragraphStyle);
			else {
				int idx = index - 1;
				// Create if needed
				if (_columnParagraphStyles == null) 
					_columnParagraphStyles = new SortedList<int, IParagraphStyle>(1);
				// Set private style for a single caption
				if (paragraphStyle != ColumnParagraphStyle) {
					if (!_columnParagraphStyles.ContainsKey(idx))
						_columnParagraphStyles.Add(idx, paragraphStyle);
					else _columnParagraphStyles[idx] = paragraphStyle;
				} else {
					if (_columnParagraphStyles != null) {
						if (_columnParagraphStyles.ContainsKey(idx))
							_columnParagraphStyles.Remove(idx);
						// Delete if not needed any longer
						if (_columnParagraphStyles.Count == 0)
							_columnParagraphStyles = null;
					}
				}
			}
		}


		/// <override></override>
		public override void ShowCaptionText(int index) {
			if (index < base.CaptionCount)
				base.ShowCaptionText(index);
			else
				_columnCaptions[index - 1].IsVisible = true;
		}


		/// <override></override>
		public override void HideCaptionText(int index) {
			if (index < base.CaptionCount)
				base.HideCaptionText(index);
			else {
				_columnCaptions[index - 1].IsVisible = false;
				Invalidate();
			}
		}

		#endregion


		#region [Public] Properties

		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_EntitySymbol_ColumnBackgroundColorStyle", typeof(Properties.Resources))]
		[LocalizedDescription("PropDesc_EntitySymbol_ColumnBackgroundColorStyle", typeof(Properties.Resources))]
		[PropertyMappingId(PropertyIdColumnBackgroundColorStyle)]
		[RequiredPermission(Permission.Present)]
		public virtual IColorStyle ColumnBackgroundColorStyle {
			get { return _privateColumnBackgroundColorStyle ?? ((EntitySymbol)Template.Shape).ColumnBackgroundColorStyle; }
			set {
				_privateColumnBackgroundColorStyle = (Template != null && Template.Shape is EntitySymbol && value == ((EntitySymbol)Template.Shape).ColumnBackgroundColorStyle) ? null : value;
				Invalidate();
			}
		}


		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_EntitySymbol_ColumnCharacterStyle", typeof(Properties.Resources))]
		[LocalizedDescription("PropDesc_EntitySymbol_ColumnCharacterStyle", typeof(Properties.Resources))]
		[PropertyMappingId(PropertyIdColumnCharacterStyle)]
		[RequiredPermission(Permission.Present)]
		public ICharacterStyle ColumnCharacterStyle {
			get { return _privateColumnCharacterStyle ?? ((EntitySymbol)Template.Shape).ColumnCharacterStyle; }
			set {
				Invalidate();
				_privateColumnCharacterStyle = (Template != null && Template.Shape is EntitySymbol && value == ((EntitySymbol)Template.Shape).ColumnCharacterStyle) ? null : value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		[CategoryAppearance()]
		[LocalizedDisplayName("PropName_EntitySymbol_ColumnParagraphStyle", typeof(Properties.Resources))]
		[LocalizedDescription("PropDesc_EntitySymbol_ColumnParagraphStyle", typeof(Properties.Resources))]
		[PropertyMappingId(PropertyIdColumnParagraphStyle)]
		[RequiredPermission(Permission.Present)]
		public IParagraphStyle ColumnParagraphStyle {
			get { return _privateColumnParagraphStyle ?? ((EntitySymbol)Template.Shape).ColumnParagraphStyle;
			}
			set {
				Invalidate();
				_privateColumnParagraphStyle = (Template != null && Template.Shape is EntitySymbol && value == ((EntitySymbol)Template.Shape).ColumnParagraphStyle) ? null : value;
				InvalidateDrawCache();
				Invalidate();
			}
		}


		[CategoryLayout()]
		[LocalizedDisplayName("Propname_EntitySymbol_ColumnNames", typeof(Properties.Resources))]
		[LocalizedDescription("PropDesc_EntitySymbol_ColumnNames", typeof(Properties.Resources))]
		[RequiredPermission(Permission.Present)]
		[TypeConverter("Dataweb.NShape.WinFormsUI.TextTypeConverter")]
		[Editor("Dataweb.NShape.WinFormsUI.TextUITypeEditor", typeof(UITypeEditor))]
		public string[] ColumnNames {
			get {
				if (_columnNames == null || _columnNames.Length != _columnCaptions.Count)
					_columnNames = new string[_columnCaptions.Count];
				for (int i = _columnCaptions.Count - 1; i >= 0; --i) {
					if (_columnNames[i] != _columnCaptions[i].Text)
						_columnNames[i] = _columnCaptions[i].Text;
				}
				return _columnNames; 
			}
			set {
				if (value == null) throw new ArgumentNullException();
				Invalidate();

				// Remove columns that are no longer needed
				int valueCnt = value.Length;
				if (_columnNames.Length > valueCnt) {
					for (int i = _columnNames.Length - 1; i >= valueCnt; --i)
						RemoveColumnAt(i);
				}
				// Replace existing and add new columns
				for (int i = 0; i < valueCnt; ++i) {
					if (i < _columnNames.Length) {
						_columnCaptions[i].Text = 
							_columnNames[i] = value[i];
					} else AddColumn(value[i]);
				}

				InvalidateDrawCache();
				Invalidate();
			}
		}
		#endregion


		#region Caption objects stuff

		public void AddColumn(string columnName) {
			_columnCaptions.Add(new Caption(columnName));
			Array.Resize(ref _columnControlPoints, _columnControlPoints.Length + 2);
			Array.Resize(ref _columnNames, _columnNames.Length + 1);
			_columnNames[_columnNames.Length - 1] = columnName;
			InvalidateDrawCache();
			Invalidate();
		}


		public void InsertColumn(int index, string columnName) {
			_columnCaptions.Insert(index, new Caption(columnName));
			Array.Resize(ref _columnControlPoints, _columnControlPoints.Length + 2);
			Array.Resize(ref _columnNames, _columnCaptions.Count);
			for (int i = _columnCaptions.Count - 1; i >= 0; --i)
				_columnNames[i] = _columnCaptions[i].Text;
			InvalidateDrawCache();
			Invalidate();
		}


		public void RemoveColumn(string columnName) {
			for (int i = _columnCaptions.Count - 1; i >= 0; --i) {
				if (columnName.Equals(_columnCaptions[i].Text, StringComparison.InvariantCulture)) {
					RemoveColumnAt(i);
					break;
				}
			}
			InvalidateDrawCache();
			Invalidate();
		}


		public void RemoveColumnAt(int index) {
			if (index < 0 || index > _columnCaptions.Count)
				throw new ArgumentOutOfRangeException("index");
			// Check whether connection points are not connected
			const String stillConnectedMsg = "Cannot remove connection point {0}: Other shapes are still connected to this point.";
			ControlPointId leftCtrlPtId = GetControlPointId(base.ControlPointCount + (2 * index));
			if (IsConnected(leftCtrlPtId, null) != ControlPointId.None)
				throw new NShapeException(stillConnectedMsg, leftCtrlPtId);
			ControlPointId rightCtrlPtId = GetControlPointId(base.ControlPointCount + (2 * index) + 1);
			if (IsConnected(rightCtrlPtId, null) != ControlPointId.None)
				throw new NShapeException(stillConnectedMsg, rightCtrlPtId);

			// Remove caption
			_columnCaptions.RemoveAt(index);
			if (index < _columnControlPoints.Length - 2)
				Array.Copy(_columnControlPoints, index + 2, _columnControlPoints, index, _columnControlPoints.Length - index - 2);
			Array.Resize(ref _columnControlPoints, _columnControlPoints.Length - 2);
			if (index < _columnNames.Length - 1)
				Array.Copy(_columnNames, index + 1, _columnNames, index, _columnNames.Length - index - 1);
			Array.Resize(ref _columnNames, _columnCaptions.Count);
			
			InvalidateDrawCache();
			Invalidate();
		}


		public void ClearColumns() {
			_columnCaptions.Clear();
			Array.Resize<Point>(ref _columnControlPoints, 0);
			Array.Resize(ref _columnNames, 0);
			InvalidateDrawCache();
			Invalidate();
		}

		#endregion


		/// <override></override>
		public override IEnumerable<MenuItemDef> GetMenuItemDefs(int mouseX, int mouseY, int range) {
			// return actions of base class
			IEnumerator<MenuItemDef> enumerator = GetBaseActions(mouseX, mouseY, range);
			while (enumerator.MoveNext()) yield return enumerator.Current;
			// return own actions

			string newColumnTxt = string.Format("Column {0}", CaptionCount);
			int captionIdx = -1;
			if (ContainsPoint(mouseX, mouseY)) {
				Point tl, tr, bl, br;
				for (int i = _columnCaptions.Count - 1; i >= 0; --i) {
					// +1 because Text Property is Caption '0'
					GetCaptionBounds(i + 1, out tl, out tr, out br, out bl);
					if (Geometry.QuadrangleContainsPoint(tl, tr, br, bl, mouseX, mouseY)) {
						// +1 because Text Property is Caption '0'
						captionIdx = i + 1;
						break;
					}
				}
			}

			yield return new CommandMenuItemDef(MenuItemNameAppendColumn, Properties.Resources.CaptionTxt__AppendColumn, 
				null, string.Empty, true, new AddColumnCommand(this, newColumnTxt));

			bool isFeasible = captionIdx >= 0;
			string description = Properties.Resources.MessageTxt_NoCaptionClicked;
			if (isFeasible)
				description = string.Format(Properties.Resources.MessageFmt_InsertNewColumnBeforeColumn0, _columnNames[captionIdx]);
			yield return new CommandMenuItemDef(MenuItemNameInsertColumn, Properties.Resources.CaptionTxt_InsertColumn,
				null, description, isFeasible, new InsertColumnCommand(this, captionIdx, newColumnTxt));
			
			if (isFeasible)
				description = string.Format(Properties.Resources.MessageFmt_RemoveColumn0, _columnNames[captionIdx]);
			yield return new CommandMenuItemDef(MenuItemNameRemoveColumn, Properties.Resources.CaptionTxt_RemoveColumn,
				null, description, isFeasible, new RemoveColumnCommand(this, captionIdx, _columnCaptions[captionIdx - 1].Text));
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int startX, int startY) {
			// First, calculate intersection point with rectangle
			Point result = base.CalculateConnectionFoot(startX, startY);
			// Then, check result for intersection with one of the rounded corners
			if (Geometry.IsValid(result)) {
				// Check the top and bottom side (between the rounded corners:
				// If the line intersects with any of these sides, we need not calculate the rounded corner intersection
				int cornerRadius = CalcCornerRadius();
				float angleDeg = Geometry.TenthsOfDegreeToDegrees(Angle);
				
				// If there is no intersection with any of the straight sides, check the rounded corners:
				if (!Geometry.RectangleIntersectsWithLine(X, Y, Width - (2 * cornerRadius), Height, angleDeg, startX, startY, result.X, result.Y, true)
					&& !Geometry.RectangleIntersectsWithLine(X, Y, Width, Height - (2 * cornerRadius), angleDeg, startX, startY, result.X, result.Y, true)) {
					// Calculate all center points of all corner roundings
					PointF topLeft = PointF.Empty, topRight = PointF.Empty, bottomRight = PointF.Empty, bottomLeft = PointF.Empty;
					RectangleF rect = RectangleF.Empty;
					rect.X = X - (Width / 2f);
					rect.Y = Y - (Height / 2f);
					rect.Width = Width;
					rect.Height = Height;
					rect.Inflate(-cornerRadius, -cornerRadius);
					Geometry.RotateRectangle(rect, X, Y, angleDeg, out topLeft, out topRight, out bottomRight, out bottomLeft);
					// Check corner roundings for intersection with the calculated line
					PointF p = Geometry.InvalidPointF;
					if (Geometry.CircleIntersectsWithLine(topLeft.X, topLeft.Y, cornerRadius, startX, startY, X, Y, false)) {
						p = Geometry.IntersectCircleWithLine(topLeft.X, topLeft.Y, cornerRadius, startX, startY, X, Y, false);
						if (Geometry.IsValid(p)) result = Point.Round(p);
					} else if (Geometry.CircleIntersectsWithLine(topRight.X, topRight.Y, cornerRadius, startX, startY, X, Y, false)) {
						p = Geometry.IntersectCircleWithLine(topRight.X, topRight.Y, cornerRadius, startX, startY, X, Y, false);
						if (Geometry.IsValid(p)) result = Point.Round(p);
					} else if (Geometry.CircleIntersectsWithLine(bottomRight.X, bottomRight.Y, cornerRadius, startX, startY, X, Y, false)) {
						p = Geometry.IntersectCircleWithLine(bottomRight.X, bottomRight.Y, cornerRadius, startX, startY, X, Y, false);
						if (Geometry.IsValid(p)) result = Point.Round(p);
					} else if (Geometry.CircleIntersectsWithLine(bottomLeft.X, bottomLeft.Y, cornerRadius, startX, startY, X, Y, false)) {
						p = Geometry.IntersectCircleWithLine(bottomLeft.X, bottomLeft.Y, cornerRadius, startX, startY, X, Y, false);
						if (Geometry.IsValid(p)) result = Point.Round(p);
					}
				}
			} else result = Center;
			return result;
		}


		/// <override></override>
		protected override int ControlPointCount { 
			get { return base.ControlPointCount + _columnControlPoints.Length; } 
		}


		/// <override></override>
		public override Point GetControlPointPosition(ControlPointId controlPointId) {
			if (controlPointId <= base.ControlPointCount)
				return base.GetControlPointPosition(controlPointId);
			else {
				UpdateDrawCache();
				int idx = controlPointId - base.ControlPointCount - 1;
				return _columnControlPoints[idx];
			}
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			if (controlPointId <= base.ControlPointCount) {
				switch (controlPointId) {
					case ControlPointIds.TopLeftControlPoint:
					case ControlPointIds.TopRightControlPoint:
					case ControlPointIds.MiddleLeftControlPoint:
					case ControlPointIds.MiddleRightControlPoint:
					case ControlPointIds.BottomLeftControlPoint:
					case ControlPointIds.BottomRightControlPoint:
						return (controlPointCapability & ControlPointCapabilities.Resize) != 0;
					case ControlPointIds.TopCenterControlPoint:
					case ControlPointIds.BottomCenterControlPoint:
						return ((controlPointCapability & ControlPointCapabilities.Resize) != 0
							|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId)));
					case ControlPointIds.MiddleCenterControlPoint:
						return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0
							|| (controlPointCapability & ControlPointCapabilities.Reference) != 0);
					default:
						return base.HasControlPointCapability(controlPointId, controlPointCapability);
				}
			} else
				return (controlPointCapability & ControlPointCapabilities.Connect) != 0;
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			base.Draw(graphics);

			Pen pen = ToolCache.GetPen(LineStyle, null, null);
			Brush columnBrush = ToolCache.GetBrush(ColumnBackgroundColorStyle);
			int cornerRadius = CalcCornerRadius();
			int headerHeight = CalcHeaderHeight();
			int columnHeight = CalcColumnHeight();

			// fill column background
			if (Height > headerHeight) {
				graphics.FillPolygon(columnBrush, _columnFrame);
				graphics.DrawPolygon(pen, _columnFrame);

				// draw column names
				int top = (int)Math.Round(Y - (Height / 2f));
				int bottom = (int)Math.Round(Y + (Height / 2f));
				int captionCnt = _columnCaptions.Count;
				for (int i = 0; i < captionCnt; ++i) {
					// draw all captions that fit into the text area. 
					if (top + headerHeight + (i * columnHeight) + columnHeight <= bottom - cornerRadius) {
						if (_columnCaptions[i].IsVisible) {
							// If there are private styles for a single caption, use these
							if (_columnCharacterStyles != null || _columnParagraphStyles != null) {
								ICharacterStyle characterStyle = null;
								if (_columnCharacterStyles != null)
									_columnCharacterStyles.TryGetValue(i, out characterStyle);
								IParagraphStyle paragraphStyle = null;
								if (_columnParagraphStyles != null)
									_columnParagraphStyles.TryGetValue(i, out paragraphStyle);
								_columnCaptions[i].Draw(graphics, characterStyle ?? ColumnCharacterStyle, paragraphStyle ?? ColumnParagraphStyle);
							} else
								_columnCaptions[i].Draw(graphics, ColumnCharacterStyle, ColumnParagraphStyle);
						}
					} else {
						// draw ellipsis indicators
						graphics.DrawLines(pen, _upperScrollArrow);
						graphics.DrawLines(pen, _lowerScrollArrow);
						break;
					}
				}
			}
			graphics.DrawPath(pen, Path);
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			_privateColumnBackgroundColorStyle = styleSet.ColorStyles.White;
			_privateColumnCharacterStyle = styleSet.CharacterStyles.Caption;
			_privateColumnParagraphStyle = styleSet.ParagraphStyles.Label;
			Width = 80;
			Height = 120;
			Text = "Table";
		}


		protected internal EntitySymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected override bool IsConnectionPointEnabled(ControlPointId pointId) {
			if (pointId <= base.ControlPointCount)
				return base.IsConnectionPointEnabled(pointId);
			else
				return true;
		}


		/// <override></override>
		protected override bool MovePointByCore(ControlPointId pointId, float transformedDeltaX, float transformedDeltaY, float sin, float cos, ResizeModifiers modifiers) {
			bool result = base.MovePointByCore(pointId, transformedDeltaX, transformedDeltaY, sin, cos, modifiers);
			//for (int id = base.ControlPointCount + 1; id <= ControlPointCount; ++id) {
			//   ControlPointHasMoved(id);
			//}
			return result;
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			base.CalcControlPoints();

			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

			int headerHeight = CalcHeaderHeight();
			int ctrlPtCnt = base.ControlPointCount;
			if (ControlPointCount > ctrlPtCnt) {
				int halfColumnHeight = (int)Math.Round(CalcColumnHeight() / 2f);
				int y;
				for (int i = 0; i < _columnControlPoints.Length; ++i) {
					if (i % 2 == 0) {
						y = top + headerHeight + (i * halfColumnHeight) + halfColumnHeight;
						if (y > bottom) y = bottom;
						_columnControlPoints[i].X = left;
						_columnControlPoints[i].Y = y;
					}
					else {
						y = top + headerHeight + ((i - 1) * halfColumnHeight) + halfColumnHeight;
						if (y > bottom) y = bottom;
						_columnControlPoints[i].X = right;
						_columnControlPoints[i].Y = y;
					}
				}
			}
		}


		/// <override></override>
		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);
			// transform ControlPoints
			if (_columnControlPoints.Length > 0)
				Matrix.TransformPoints(_columnControlPoints);
			// transform column frame
			Matrix.TransformPoints(_columnFrame);
			// transform ellipsis indicator
			Matrix.TransformPoints(_upperScrollArrow);
			Matrix.TransformPoints(_lowerScrollArrow);
			// transform Column paths
			foreach (Caption caption in _columnCaptions)
				caption.TransformPath(Matrix);
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int cornerRadius = CalcCornerRadius() + 1;
			int headerHeight = CalcHeaderHeight();
			captionBounds = Rectangle.Empty;
			captionBounds.X = left + cornerRadius;
			captionBounds.Width = Width - cornerRadius - cornerRadius;
			if (index == 0) {
				captionBounds.Y = top;
				captionBounds.Height = headerHeight;
			} else {
				int columnHeight = CalcColumnHeight();
				captionBounds.Y = top + headerHeight + ((index - 1) * columnHeight);
				captionBounds.Height = columnHeight;
			}
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				// calculate main shape
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				int cornerRadius = CalcCornerRadius();
				int headerHeight = CalcHeaderHeight();
				int columnHeight = CalcColumnHeight();

				_rectBuffer.X = left;
				_rectBuffer.Y = top;
				_rectBuffer.Width = Width;
				_rectBuffer.Height = Height;

				//Path.StartFigure();
				//Path.AddLine(left + cornerRadius, top, right - cornerRadius, top);
				//Path.AddArc(right - cornerRadius - cornerRadius, top, cornerRadius + cornerRadius, cornerRadius + cornerRadius, -90, 90);
				//Path.AddLine(right, top + cornerRadius, right, bottom - cornerRadius);
				//Path.AddArc(right - cornerRadius - cornerRadius, bottom - cornerRadius - cornerRadius, cornerRadius + cornerRadius, cornerRadius + cornerRadius, 0, 90);
				//Path.AddLine(right - cornerRadius, bottom, left + cornerRadius, bottom);
				//Path.AddArc(left, bottom - cornerRadius - cornerRadius, cornerRadius + cornerRadius, cornerRadius + cornerRadius, 90, 90);
				//Path.AddLine(left, bottom - cornerRadius, left, top + cornerRadius);
				//Path.AddArc(left, top, cornerRadius + cornerRadius, cornerRadius + cornerRadius, 180, 90);
				//Path.CloseFigure();
				AddRoundedRectangleFigure(Path, left, top, right, bottom, cornerRadius);

				if (Height > headerHeight) {
					int headerTop = top + headerHeight;
					int dblLineWidth = LineStyle.LineWidth + LineStyle.LineWidth;

					// column section frame lines
					_columnFrame[0].X = left + cornerRadius;
					_columnFrame[0].Y = headerTop;
					_columnFrame[1].X = right - cornerRadius;
					_columnFrame[1].Y = headerTop;
					_columnFrame[2].X = right - cornerRadius;
					_columnFrame[2].Y = bottom - cornerRadius;
					_columnFrame[3].X = left + cornerRadius;
					_columnFrame[3].Y = bottom - cornerRadius;

					Rectangle bounds = Rectangle.Empty;
					int captionCnt = _columnCaptions.Count;
					if (captionCnt > 0) {
						int colX, colY, colWidth, colHeight;
						colX = left + cornerRadius;
						colWidth = Width - cornerRadius - cornerRadius;
						colHeight = columnHeight;
						
						for (int i = 0; i < captionCnt; ++i) {
							// calc ColumnName text path
							colY = top + headerHeight + (i * columnHeight);
							// check if the column text is inside the column area
							if (colY + colHeight <= bottom - cornerRadius)
								_columnCaptions[i].CalculatePath(colX, colY, colWidth, colHeight, ColumnCharacterStyle, ColumnParagraphStyle);
							else {
								// if not, draw an ellipsis symbol (double downward arrow)
								int offsetX = dblLineWidth + dblLineWidth + dblLineWidth;
								int offsetY = dblLineWidth + dblLineWidth;

								// calculate arrows indicating that not all columns can be drawn
								_upperScrollArrow[0].X = right - cornerRadius - offsetX - offsetX;
								_upperScrollArrow[0].Y = bottom - cornerRadius - offsetY - offsetY - offsetY;
								_upperScrollArrow[1].X = right - cornerRadius - offsetX - (offsetX / 2);
								_upperScrollArrow[1].Y = bottom - cornerRadius - offsetY - offsetY;
								_upperScrollArrow[2].X = right - cornerRadius - offsetX;
								_upperScrollArrow[2].Y = bottom - cornerRadius - offsetY - offsetY - offsetY;

								_lowerScrollArrow[0].X = right - cornerRadius - offsetX - offsetX;
								_lowerScrollArrow[0].Y = bottom - cornerRadius - offsetY - offsetY;
								_lowerScrollArrow[1].X = right - cornerRadius - offsetX - (offsetX / 2);
								_lowerScrollArrow[1].Y = bottom - cornerRadius - offsetY;
								_lowerScrollArrow[2].X = right - cornerRadius - offsetX;
								_lowerScrollArrow[2].Y = bottom - cornerRadius - offsetY - offsetY;
								break;
							}	// end of "is column in column area check"
						} // end of for loop (processing all columnName-captions)
					} // end of block (if (columnCaptions.Count > 0) )
				} // end of check "Height > ColumnHeight"
				return true;
			}
			return false;
		}


		/// <override></override>
		protected override void ProcessExecModelPropertyChange(IModelMapping propertyMapping) {
			switch (propertyMapping.ShapePropertyId) {
				case PropertyIdColumnBackgroundColorStyle:
					_privateColumnBackgroundColorStyle = (propertyMapping.GetStyle() as IColorStyle);
					Invalidate();
					break;
				case PropertyIdColumnCharacterStyle:
					_privateColumnCharacterStyle = (propertyMapping.GetStyle() as ICharacterStyle);
					InvalidateDrawCache();
					Invalidate();
					break;
				case PropertyIdColumnParagraphStyle:
					_privateColumnParagraphStyle = (propertyMapping.GetStyle() as IParagraphStyle);
					InvalidateDrawCache();
					Invalidate();
					break;
				default:
					base.ProcessExecModelPropertyChange(propertyMapping);
					break;
			}
		}


		private IEnumerator<MenuItemDef> GetBaseActions(int mouseX, int mouseY, int range) {
			return base.GetMenuItemDefs(mouseX, mouseY, range).GetEnumerator();
		}


		private int CalcHeaderHeight() {
			int cornerRadius = CalcCornerRadius() + 1;
			Size size = Size.Empty;
			size.Width = Width - (2 * cornerRadius);
			size.Height = Height - (2 * cornerRadius);
			Size result = TextMeasurer.MeasureText(string.IsNullOrEmpty(Text) ? "Ig" : Text, ToolCache.GetFont(CharacterStyle), size, ParagraphStyle);
			result.Height += (cornerRadius + LineStyle.LineWidth + LineStyle.LineWidth);
			return result.Height;
		}


		private int CalcColumnHeight() {
			SizeF result = TextMeasurer.MeasureText("Ig", ToolCache.GetFont(ColumnCharacterStyle), Size.Empty, ColumnParagraphStyle);
			result.Height *= 1.5f;
			return (int)Math.Ceiling(result.Height);
		}


		private int CalcCornerRadius() {
			int cornerRadius = 10;
			if (Width <= 80)
				cornerRadius = (int)Math.Round((Width - 2) / 8f);
			else if (Height <= 80)
				cornerRadius = (int)Math.Round((Height - 2) / 8f);

			if (cornerRadius <= 0)
				cornerRadius = 1;
			return cornerRadius;
		}


		private void AddRoundedRectangleFigure(GraphicsPath path, int left, int top, int right, int bottom, int cornerRadius) {
			path.StartFigure();
			path.AddLine(left + cornerRadius, top, right - cornerRadius, top);
			path.AddArc(right - cornerRadius - cornerRadius, top, cornerRadius + cornerRadius, cornerRadius + cornerRadius, -90, 90);
			path.AddLine(right, top + cornerRadius, right, bottom - cornerRadius);
			path.AddArc(right - cornerRadius - cornerRadius, bottom - cornerRadius - cornerRadius, cornerRadius + cornerRadius, cornerRadius + cornerRadius, 0, 90);
			path.AddLine(right - cornerRadius, bottom, left + cornerRadius, bottom);
			path.AddArc(left, bottom - cornerRadius - cornerRadius, cornerRadius + cornerRadius, cornerRadius + cornerRadius, 90, 90);
			path.AddLine(left, bottom - cornerRadius, left, top + cornerRadius);
			path.AddArc(left, top, cornerRadius + cornerRadius, cornerRadius + cornerRadius, 180, 90);
			path.CloseFigure();
		}


		#region Fields

		protected const int PropertyIdColumnBackgroundColorStyle = 9;
		protected const int PropertyIdColumnCharacterStyle = 10;
		protected const int PropertyIdColumnParagraphStyle = 11;

		private const string attrNameColumns = "TableColumns";
		private const string attrNameColumn = "Column";
		private static readonly string[] columnAttrNames = new string[] { "ColumnIndex", "ColumnName" };
		private static readonly Type[] columnAttrTypes = new Type[] { typeof(int), typeof(string) };

		private string[] _columnNames = new string[0];
		private List<Caption> _columnCaptions = new List<Caption>(0);
		private List<Rectangle> _columnBounds = new List<Rectangle>(0);
		private SortedList<int, ICharacterStyle> _columnCharacterStyles = null;
		private SortedList<int, IParagraphStyle> _columnParagraphStyles = null;
		private Point[] _columnControlPoints = new Point[0];
		private IColorStyle _privateColumnBackgroundColorStyle = null;
		private ICharacterStyle _privateColumnCharacterStyle = null;
		private IParagraphStyle _privateColumnParagraphStyle = null;

		private Rectangle _rectBuffer = Rectangle.Empty;
		private Point[] _columnFrame = new Point[4];
		private Point[] _upperScrollArrow = new Point[3];
		private Point[] _lowerScrollArrow = new Point[3];

		#endregion
	}


	public class ClassSymbol : EntitySymbol {
		
		protected internal ClassSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new ClassSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override IEnumerable<MenuItemDef> GetMenuItemDefs(int mouseX, int mouseY, int range) {
			return base.GetMenuItemDefs(mouseX, mouseY, range);
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Text = "Class";
		}
	}


	public class CloudSymbol : RectangleBase {

		/// <summary>
		/// Provides constants for the control point id's of the shape.
		/// </summary>
		new public class ControlPointIds {
			/// <summary>ControlPointId of the top left control point.</summary>
			public const int TopLeftControlPoint = 1;
			/// <summary>ControlPointId of the top center control point.</summary>
			public const int TopCenterControlPoint = 2;
			/// <summary>ControlPointId of the top right control point.</summary>
			public const int TopRightControlPoint = 3;
			/// <summary>ControlPointId of the middle left control point.</summary>
			public const int MiddleLeftControlPoint = 4;
			/// <summary>ControlPointId of the middle right control point.</summary>
			public const int MiddleRightControlPoint = 5;
			/// <summary>ControlPointId of the bottom left control point.</summary>
			public const int BottomLeftControlPoint = 6;
			/// <summary>ControlPointId of the bottom center control point.</summary>
			public const int BottomCenterControlPoint = 7;
			/// <summary>ControlPointId of the bottom right control point.</summary>
			public const int BottomRightControlPoint = 8;
			/// <summary>ControlPointId of the center control point.</summary>
			public const int MiddleCenterControlPoint = 9;
			/// <summary>ControlPointId of the top left connection point.</summary>
			public const int TopLeftConnectionPoint = 10;
			/// <summary>ControlPointId of the top right connection point.</summary>
			public const int TopRightConnectionPoint = 11;
			/// <summary>ControlPointId of the middle left connection point.</summary>
			public const int MiddleLeftConnectionPoint = 12;
			/// <summary>ControlPointId of the middle right connection point.</summary>
			public const int MiddleRightConnectionPoint = 13;
			/// <summary>ControlPointId of the bottom left connection point.</summary>
			public const int BottomLeftConnectionPoint = 14;
			/// <summary>ControlPointId of the bottom right connection point.</summary>
			public const int BottomRightConnectionPoint = 15;
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Width = 120;
			Height = 80;
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new CloudSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopCenterControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.MiddleLeftControlPoint:
				case ControlPointIds.MiddleRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Reference) != 0
						|| (controlPointCapability & ControlPointCapabilities.Rotate) != 0);
				case ControlPointIds.TopLeftConnectionPoint:
				case ControlPointIds.TopRightConnectionPoint:
				case ControlPointIds.MiddleLeftConnectionPoint:
				case ControlPointIds.MiddleRightConnectionPoint:
				case ControlPointIds.BottomLeftConnectionPoint:
				case ControlPointIds.BottomRightConnectionPoint:
					return ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int startX, int startY) {
			UpdateDrawCache();
			Point result = Geometry.GetNearestPoint(startX, startY, Geometry.IntersectPolygonLine(Path.PathPoints, startX, startY, X, Y, true));
			if (!Geometry.IsValid(result)) result = Center;
			return result;
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			base.Draw(graphics);

			//// draw debug info
			//graphics.DrawRectangles(Pens.DarkGreen, arcBounds);
			//for (int i = 0; i < shapePoints.Length; ++i) {
			//   graphics.DrawLine(Pens.Red, shapePoints[i].X - 2, shapePoints[i].Y, shapePoints[i].X + 2, shapePoints[i].Y);
			//   graphics.DrawLine(Pens.Red, shapePoints[i].X, shapePoints[i].Y - 2, shapePoints[i].X, shapePoints[i].Y + 2);
			//}
			//Path.Reset();	// call CalcPath every Frame
		}


		protected internal CloudSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected override int ControlPointCount {
			get { return 15; }
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			base.CalcControlPoints();
			float w = Width / 128f;
			float h = Height / 128f;
			ControlPoints[9].X = 0 - (int)Math.Round(56 * w);
			ControlPoints[9].Y = 0;
			ControlPoints[10].X = 0 - (int)Math.Round(32 * w);
			ControlPoints[10].Y = 0 - (int)Math.Round(48 * h);
			ControlPoints[11].X = 0 + (int)Math.Round(8 * w);
			ControlPoints[11].Y = 0 - (int)Math.Round(48 * h);
			ControlPoints[12].X = 0 + (int)Math.Round(56 * w);
			ControlPoints[12].Y = 0 - (int)Math.Round(16 * h);
			ControlPoints[13].X = 0 + (int)Math.Round(30 * w);
			ControlPoints[13].Y = 0 + (int)Math.Round(48 * h);
			ControlPoints[14].X = 0 - (int)Math.Round(16 * w);
			ControlPoints[14].Y = 0 + (int)Math.Round(48 * h);
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			int left, right, top, bottom;
			left = top = int.MaxValue;
			right = bottom = int.MinValue;
			foreach (Point p in ControlPoints) {
				if (p.X < left) left = p.X;
				if (p.X > right) right = p.X;
				if (p.Y < top) top = p.Y;
				if (p.Y > bottom) bottom = p.Y;
			}
			captionBounds = Rectangle.Empty;
			captionBounds.X = left;
			captionBounds.Y = top;
			captionBounds.Width = right - left;
			captionBounds.Height = bottom - top;
			if (captionBounds.X > (-Width / 2f))
				captionBounds.X -= X;
			if (captionBounds.Y > (-Height / 2f))
				captionBounds.Y -= Y;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				float left, top, width, height;
				float w = Width / 128f;
				float h = Height / 128f;

				Path.StartFigure();
				/*****************************************/
				left = -(64 * w);
				top = -(58 * h);
				width = 37 * w;
				height = 64 * h;
				AddArcToGraphicsPath(left, top, width, height, ControlPoints[9].X, ControlPoints[9].Y, ControlPoints[10].X, ControlPoints[10].Y);
				_arcBounds[0] = RectangleF.FromLTRB(left, top, left + width, top + height);
				/*****************************************/
				left = -(35 * w);
				top = -(64 * h);
				width = 46 * w;
				height = 62 * h;
				AddArcToGraphicsPath(left, top, width, height, ControlPoints[10].X, ControlPoints[10].Y, ControlPoints[11].X, ControlPoints[11].Y);
				_arcBounds[1] = RectangleF.FromLTRB(left, top, left + width, top + height);
				/*****************************************/
				left = (3 * w);
				top = -(64 * h);
				width = 54 * w;
				height = 78 * h;
				AddArcToGraphicsPath(left, top, width, height, ControlPoints[11].X, ControlPoints[11].Y, ControlPoints[12].X, ControlPoints[12].Y);
				_arcBounds[2] = RectangleF.FromLTRB(left, top, left + width, top + height);
				/*****************************************/
				left = (21 * w);
				top = -(25 * h);
				width = 43 * w;
				height = 81 * h;
				AddArcToGraphicsPath(left, top, width, height, ControlPoints[12].X, ControlPoints[12].Y, ControlPoints[13].X, ControlPoints[13].Y);
				_arcBounds[3] = RectangleF.FromLTRB(left, top, left + width, top + height);
				/*****************************************/
				left = -(20 * w);
				top = -(0 * h);
				width = 53 * w;
				height = 64 * h;
				AddArcToGraphicsPath(left, top, width, height, ControlPoints[13].X, ControlPoints[13].Y, ControlPoints[14].X, ControlPoints[14].Y);
				_arcBounds[4] = RectangleF.FromLTRB(left, top, left + width, top + height);
				/*****************************************/
				left = -(64 * w);
				top = -(10 * h);
				width = 52 * w;
				height = (73 * h);
				AddArcToGraphicsPath(left, top, width, height, ControlPoints[14].X, ControlPoints[14].Y, ControlPoints[9].X, ControlPoints[9].Y);
				_arcBounds[5] = RectangleF.FromLTRB(left, top, left + width, top + height);
				/*****************************************/
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		private void AddArcToGraphicsPath(float left, float top, float width, float height, int fromX, int fromY, int toX, int toY) {
			int centerX = (int)Math.Round(left + (width / 2));
			int centerY = (int)Math.Round(top + (height / 2));

			float startAngle, sweepAngle;
			startAngle = Geometry.Angle(centerX, centerY, (int)Math.Round(left + width), centerY, fromX, fromY) / (float)(Math.PI / 180);
			sweepAngle = (360 + Geometry.Angle(centerX, centerY, fromX, fromY, toX, toY) / (float)(Math.PI / 180)) % 360;

			if (width == 0) width = 1;
			if (height == 0) height = 1;
			Path.AddArc(left, top, width, height, startAngle, sweepAngle);
		}


		/// <override></override>
		public override void DrawOutline(Graphics graphics, Pen pen) {
			base.DrawOutline(graphics, pen);

			//if (connections.Count == 1) {
			//   Point startPt = connections[0].PassiveShape.GetControlPointPosition(connections[0].ConnectionPointId == 1 ? 2 : 1);
			//   int startX = startPt.X;
			//   int startY = startPt.Y;
			//   Point result = Center;

			//   graphics.DrawPolygon(Pens.Red, Path.PathPoints);

			//   float distance, lowestDistance;
			//   lowestDistance = float.MaxValue;
			//   foreach (Point p in Geometry.IntersectPolygonLine(Path.PathPoints, startX, startY, X, Y)) {
			//      distance = Geometry.DistancePointPoint(p.X, p.Y, startX, startY);
			//      Geometry.DrawPoint(graphics, Pens.Yellow, p.X, p.Y, 3);
			//      if (distance < lowestDistance) {
			//         lowestDistance = distance;
			//         result = p;
			//      }
			//   }

			//   Geometry.DrawPoint(graphics, Pens.Red, result.X, result.Y, 3);
			//}
		}


		#region Fields
		private RectangleF[] _arcBounds = new RectangleF[6];
		#endregion
	}


	public class AnnotationSymbol : RectangleBase {

		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Width = 120;
			Height = 80;
			FillStyle = styleSet.FillStyles.Yellow;
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new AnnotationSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopCenterControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.MiddleLeftControlPoint:
				case ControlPointIds.MiddleRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return (controlPointCapability & ControlPointCapabilities.Resize) != 0;
				case ControlPointIds.MiddleCenterControlPoint:
				case ControlPointId.Reference:
					return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| (controlPointCapability & ControlPointCapabilities.Reference) != 0
						|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0
							&& IsConnectionPointEnabled(controlPointId)));
				default:
					return false;
			}
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int startX, int startY) {
			Point result = Point.Empty;
			result.Offset(X, Y);

			// Calculate shape
			CalculateShapePoints();
			Matrix.Reset();
			Matrix.Translate(X, Y);
			Matrix.RotateAt(Geometry.TenthsOfDegreeToDegrees(Angle), Center);
			Matrix.TransformPoints(_shapePoints);

			float currDist, dist = float.MaxValue;
			foreach (Point p in Geometry.IntersectPolygonLine(_shapePoints, startX, startY, X, Y, true)) {
				currDist = Geometry.DistancePointPoint(p.X, p.Y, startX, startY);
				if (currDist < dist) {
					dist = currDist;
					result = p;
				}
			}
			return result;
		}


		/// <override></override>
		public override void Draw(Graphics graphics) {
			base.Draw(graphics);
			Pen pen = ToolCache.GetPen(LineStyle, null, null);
			graphics.DrawLines(pen, _foldingPoints);
		}


		protected internal AnnotationSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected override void TransformDrawCache(int deltaX, int deltaY, int deltaAngle, int rotationCenterX, int rotationCenterY) {
			base.TransformDrawCache(deltaX, deltaY, deltaAngle, rotationCenterX, rotationCenterY);
			Matrix.TransformPoints(_foldingPoints);
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				CalculateShapePoints();

				Path.Reset();
				Path.StartFigure();
				Path.AddPolygon(_shapePoints);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		private void CalculateShapePoints() {
			int foldingSize = Math.Min(Width / 8, Height / 8);
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

			_shapePoints[0].X = left;
			_shapePoints[0].Y = top;
			_shapePoints[1].X = right - foldingSize;
			_shapePoints[1].Y = top;
			_shapePoints[2].X = right;
			_shapePoints[2].Y = top + foldingSize;
			_shapePoints[3].X = right;
			_shapePoints[3].Y = bottom;
			_shapePoints[4].X = left;
			_shapePoints[4].Y = bottom;

			_foldingPoints[0].X = right - foldingSize;
			_foldingPoints[0].Y = top;
			_foldingPoints[1].X = right - foldingSize;
			_foldingPoints[1].Y = top + foldingSize;
			_foldingPoints[2].X = right;
			_foldingPoints[2].Y = top + foldingSize;
		}


		#region Fields
		
		private Point[] _shapePoints = new Point[5];
		private Point[] _foldingPoints = new Point[3];
		
		#endregion
	}


	public class DatabaseSymbol : RectangleBase {

		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Width = 80;
			Height = 80;
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new DatabaseSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override Point CalculateConnectionFoot(int startX, int startY) {
			// First, calculate intersection point with the shape aligned bounding box (the bounding box that is rotated with the shape)
			Point result = base.CalculateConnectionFoot(startX, startY);
			bool calcUpperEllipseIntersection = false;
			bool calcLowerEllipseIntersection = false;
			//
			// Then, check if the intersection point would intersect with any of the ellipsis parts of the shape
			// by checking if the calculated intersection point is on the (shape aligned) bounding box of the
			// upper/lower half-ellipse
			float angleDeg = Angle == 0 ? 0 : Geometry.TenthsOfDegreeToDegrees(Angle);
			if (Angle == 0) {
				// An unrotated shape simplifies the calculation
				Rectangle halfEllipseBounds = Rectangle.Empty;
				// check bounds of the upper half-ellipse
				halfEllipseBounds.X = X - (int)Math.Round(Width / 2f);
				halfEllipseBounds.Y = Y - (int)Math.Round(Height / 2f);
				halfEllipseBounds.Width = Width;
				halfEllipseBounds.Height = (int)Math.Round(EllipseHeight / 2);
				calcUpperEllipseIntersection = Geometry.RectangleContainsPoint(halfEllipseBounds.X, halfEllipseBounds.Y, halfEllipseBounds.Width, halfEllipseBounds.Height, result.X, result.Y, true);
				if (!calcUpperEllipseIntersection) {
					// check bounds of the lower half-ellipse
					halfEllipseBounds.Width = Width;
					halfEllipseBounds.Height = (int)Math.Round(EllipseHeight / 2);
					halfEllipseBounds.X = X - (int)Math.Round(Width / 2f);
					halfEllipseBounds.Y = Y + (int)Math.Round(Height / 2f) - halfEllipseBounds.Height;
					calcLowerEllipseIntersection = Geometry.RectangleContainsPoint(halfEllipseBounds.X, halfEllipseBounds.Y, halfEllipseBounds.Width, halfEllipseBounds.Height, result.X, result.Y, true);
				}
			} else {
				// check bounds of upper half ellipse
				int boxWidth = Width;
				int boxHeight = (int)Math.Round(EllipseHeight / 2);
				Point boxCenter = Point.Empty;
				boxCenter.X = X;
				boxCenter.Y = (int)Math.Round(Y - (Height / 2f) + (EllipseHeight / 4f));
				boxCenter = Geometry.RotatePoint(Center, angleDeg, boxCenter);
				calcUpperEllipseIntersection = Geometry.RectangleIntersectsWithLine(boxCenter.X, boxCenter.Y, boxWidth, boxHeight, angleDeg, startX, startY, X, Y, true);
				// check bounds of the lower half-ellipse
				if (!calcUpperEllipseIntersection) {
					boxCenter.X = X;
					boxCenter.Y = (int)Math.Round(Y + (Height / 2f) - (EllipseHeight / 4f));
					boxCenter = Geometry.RotatePoint(Center, angleDeg, boxCenter);
					calcLowerEllipseIntersection = Geometry.RectangleIntersectsWithLine(boxCenter.X, boxCenter.Y, boxWidth, boxHeight, angleDeg, startX, startY, X, Y, true);
				}
			}

			// Calculate intersection point with ellipse if neccessary
			if (calcUpperEllipseIntersection || calcLowerEllipseIntersection) {
				Point ellipseCenter = Point.Empty;
				if (calcUpperEllipseIntersection)
					ellipseCenter.Y = (int)Math.Round(Y - (Height / 2f) + (EllipseHeight / 2f));
				else
					ellipseCenter.Y = (int)Math.Round(Y + (Height / 2f) - (EllipseHeight / 2f));
				ellipseCenter.X = X;
				if (Angle != 0) ellipseCenter = Geometry.RotatePoint(Center, angleDeg, ellipseCenter);
				PointF p = Geometry.GetNearestPoint(startX, startY, Geometry.IntersectEllipseLine(ellipseCenter.X, ellipseCenter.Y, Width, EllipseHeight, angleDeg, startX, startY, X, Y, false));
				if (Geometry.IsValid(p)) result = Point.Round(p);
			}
			return result;
		}
		
		
		protected internal DatabaseSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			captionBounds = Rectangle.Empty;
			captionBounds.X = left;
			captionBounds.Y = top + (int)Math.Ceiling(EllipseHeight);
			captionBounds.Width = Width;
			captionBounds.Height = Height - (int)Math.Ceiling(EllipseHeight);
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int bottom = top + Height;
				float arcWidth = Math.Max(Width, 0.00001f);
				float arcHeight = Math.Max(EllipseHeight, 0.00001f);

				Path.StartFigure();
				Path.AddEllipse(left, top, Width, EllipseHeight);
				Path.CloseFigure();
				Path.StartFigure();
				Path.AddArc(left, top, arcWidth, arcHeight, 0, 180);
				Path.AddLine(left, top + (EllipseHeight / 2), left, bottom - (EllipseHeight / 2));
				Path.AddArc(left, bottom - EllipseHeight, arcWidth, arcHeight, 180, -180);
				Path.AddLine(left + Width, bottom - (EllipseHeight / 2), left + Width, top + (EllipseHeight / 2));
				Path.CloseAllFigures();
				return true;
			}
			return false;
		}


		private float EllipseHeight {
			get { return Math.Max(Width / zFactor, 1); }
		}


		#region Fields
		private const float zFactor = 6;
		#endregion
	}


	public class ComponentSymbol : RectangleBase {

		/// <summary>
		/// Provides constants for the control point id's of the shape.
		/// </summary>
		new public class ControlPointIds {
			/// <summary>ControlPointId of the top left control point.</summary>
			public const int TopLeftControlPoint = 1;
			/// <summary>ControlPointId of the top center control point.</summary>
			public const int TopCenterControlPoint = 2;
			/// <summary>ControlPointId of the top right control point.</summary>
			public const int TopRightControlPoint = 3;
			/// <summary>ControlPointId of the middle left control point.</summary>
			public const int MiddleLeftControlPoint = 4;
			/// <summary>ControlPointId of the middle right control point.</summary>
			public const int MiddleRightControlPoint = 5;
			/// <summary>ControlPointId of the bottom left control point.</summary>
			public const int BottomLeftControlPoint = 6;
			/// <summary>ControlPointId of the bottom center control point.</summary>
			public const int BottomCenterControlPoint = 7;
			/// <summary>ControlPointId of the bottom right control point.</summary>
			public const int BottomRightControlPoint = 8;
			/// <summary>ControlPointId of the center control point.</summary>
			public const int MiddleCenterControlPoint = 9;

			/// <summary>ControlPointId of a connection point.</summary>
			public const int TopLeftConnectionPoint = 33;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int MiddleLeftConnectionPoint = 18;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int BottomLeftConnectionPoint = 34;

			/// <summary>ControlPointId of a connection point.</summary>
			public const int UpperDockTopConnectionPoint = 25;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int UpperDockMiddleConnectionPoint = 14;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int UpperDockBottomConnectionPoint = 26;

			/// <summary>ControlPointId of a connection point.</summary>
			public const int LowerDockTopConnectionPoint = 27;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int LowerDockMiddleConnectionPoint = 15;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int LowerDockBottomConnectionPoint = 28;

			/// <summary>ControlPointId of a connection point.</summary>
			public const int TopConnectionPoint1 = 10;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int TopConnectionPoint2 = 19;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int TopConnectionPoint3 = 20;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int TopConnectionPoint4 = 11;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int TopConnectionPoint5 = 21;

			/// <summary>ControlPointId of a connection point.</summary>
			public const int RightConnectionPoint1 = 29;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int RightConnectionPoint2 = 16;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int RightConnectionPoint3 = 30;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int RightConnectionPoint4 = 31;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int RightConnectionPoint5 = 17;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int RightConnectionPoint6 = 32;
			
			/// <summary>ControlPointId of a connection point.</summary>
			public const int BottomConnectionPoint1 = 12;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int BottomConnectionPoint2 = 22;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int BottomConnectionPoint3 = 23;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int BottomConnectionPoint4 = 13;
			/// <summary>ControlPointId of a connection point.</summary>
			public const int BottomConnectionPoint5 = 24;
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new ComponentSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override int ControlPointCount {
			get { return 34; }
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.MiddleLeftControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				case ControlPointIds.TopCenterControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.MiddleRightControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0)
						|| ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
				case ControlPointIds.MiddleCenterControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Rotate) != 0
						|| (controlPointCapability & ControlPointCapabilities.Reference) != 0);
				// Connection points
				case ControlPointIds.TopLeftConnectionPoint :
				case ControlPointIds.MiddleLeftConnectionPoint :
				case ControlPointIds.BottomLeftConnectionPoint:
				case ControlPointIds.UpperDockTopConnectionPoint:
				case ControlPointIds.UpperDockMiddleConnectionPoint:
				case ControlPointIds.UpperDockBottomConnectionPoint:
				case ControlPointIds.LowerDockTopConnectionPoint:
				case ControlPointIds.LowerDockMiddleConnectionPoint:
				case ControlPointIds.LowerDockBottomConnectionPoint:
				case ControlPointIds.TopConnectionPoint1:
				case ControlPointIds.TopConnectionPoint2:
				case ControlPointIds.TopConnectionPoint3:
				case ControlPointIds.TopConnectionPoint4:
				case ControlPointIds.TopConnectionPoint5:
				case ControlPointIds.RightConnectionPoint1:
				case ControlPointIds.RightConnectionPoint2:
				case ControlPointIds.RightConnectionPoint3:
				case ControlPointIds.RightConnectionPoint4:
				case ControlPointIds.RightConnectionPoint5:
				case ControlPointIds.RightConnectionPoint6:
				case ControlPointIds.BottomConnectionPoint1:
				case ControlPointIds.BottomConnectionPoint2:
				case ControlPointIds.BottomConnectionPoint3:
				case ControlPointIds.BottomConnectionPoint4:
				case ControlPointIds.BottomConnectionPoint5:
					return ((controlPointCapability & ControlPointCapabilities.Connect) != 0 && IsConnectionPointEnabled(controlPointId));
				default:
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal ComponentSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected override void CalcControlPoints() {
			// calculate positions of standard drag- and rotate handles
			base.CalcControlPoints();

			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int right = left + Width;
			int bottom = top + Height;

			// add specific connectionpoints for "Lollies"
			int h, w;
			w = (int)Math.Round((float)Width / 8);
			h = (int)Math.Round((float)Height / 8);
			ControlPoints[9].X = -(w + w);
			ControlPoints[9].Y = top;
			ControlPoints[10].X = (w + w);
			ControlPoints[10].Y = top;

			ControlPoints[11].X = -(w + w);
			ControlPoints[11].Y = bottom;
			ControlPoints[12].X = (w + w);
			ControlPoints[12].Y = bottom;

			ControlPoints[13].X = left;
			ControlPoints[13].Y = -(h + h);
			ControlPoints[14].X = left;
			ControlPoints[14].Y = (h + h);

			ControlPoints[15].X = right;
			ControlPoints[15].Y = -(h + h);
			ControlPoints[16].X = right;
			ControlPoints[16].Y = (h + h);

			ControlPoints[17].X = left + w;
			ControlPoints[17].Y = 0;

			ControlPoints[18].X = 0 - w;
			ControlPoints[18].Y = top;
			ControlPoints[19].X = w;
			ControlPoints[19].Y = top;
			ControlPoints[20].X = (w + w + w);
			ControlPoints[20].Y = top;

			ControlPoints[21].X = -w;
			ControlPoints[21].Y = bottom;
			ControlPoints[22].X = +w;
			ControlPoints[22].Y = bottom;
			ControlPoints[23].X = (w + w + w);
			ControlPoints[23].Y = bottom;

			ControlPoints[24].X = left;
			ControlPoints[24].Y = -(h + h + h);
			ControlPoints[25].X = left;
			ControlPoints[25].Y = -h;
			ControlPoints[26].X = left;
			ControlPoints[26].Y = +h;
			ControlPoints[27].X = left;
			ControlPoints[27].Y = (h + h + h);

			ControlPoints[28].X = right;
			ControlPoints[28].Y = -(h + h + h);
			ControlPoints[29].X = right;
			ControlPoints[29].Y = -h;
			ControlPoints[30].X = right;
			ControlPoints[30].Y = +h;
			ControlPoints[31].X = right;
			ControlPoints[31].Y = (h + h + h);

			ControlPoints[32].X = left + w;
			ControlPoints[32].Y = top;
			ControlPoints[33].X = left + w;
			ControlPoints[33].Y = bottom;
		}


		/// <override></override>
		protected override void CalcCaptionBounds(int index, out Rectangle captionBounds) {
			if (index != 0) throw new ArgumentOutOfRangeException("index");
			int left = (int)Math.Round(-Width / 2f);
			int top = (int)Math.Round(-Height / 2f);
			int w = (int)Math.Round((float)Width / 8);
			captionBounds = Rectangle.Empty;
			captionBounds.X = left + (w + w);
			captionBounds.Y = top;
			captionBounds.Width = Width - (w + w);
			captionBounds.Height = Height;
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				Path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;

				int h, w;
				int left = (int)Math.Round(-Width / 2f);
				int top = (int)Math.Round(-Height / 2f);
				int right = left + Width;
				int bottom = top + Height;
				w = (int)Math.Round((float)Width / 8);
				h = (int)Math.Round((float)Height / 8);

				_shapePoints[0].X = left + w;
				_shapePoints[0].Y = top + h;
				_shapePoints[1].X = left + w;
				_shapePoints[1].Y = top;
				_shapePoints[2].X = right;
				_shapePoints[2].Y = top;
				_shapePoints[3].X = right;
				_shapePoints[3].Y = bottom;
				_shapePoints[4].X = left + w;
				_shapePoints[4].Y = bottom;
				_shapePoints[5].X = left + w;
				_shapePoints[5].Y = bottom - h;


				Rectangle rect1Buffer = Rectangle.Empty;
				rect1Buffer.X = left;
				rect1Buffer.Y = h;
				rect1Buffer.Width = w + w;
				rect1Buffer.Height = h + h;

				Rectangle rect2Buffer = Rectangle.Empty;
				rect2Buffer.X = left;
				rect2Buffer.Y = top + h;
				rect2Buffer.Width = w + w;
				rect2Buffer.Height = h + h;

				Path.StartFigure();
				Path.AddLines(_shapePoints);
				Path.AddRectangle(rect1Buffer);
				Path.AddLine(left + w, h, left + w, -h);
				Path.AddRectangle(rect2Buffer);
				Path.CloseFigure();
				return true;
			}
			return false;
		}


		#region Fields
		private Point[] _shapePoints = new Point[6];
		#endregion
	}


	/// <summary>
	/// Displays a symbol for a document.
	/// </summary>
	public class DocumentSymbol : RectangleBase {

		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			Height = 60;
			Width = (int)(Height / Math.Sqrt(2));
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new DocumentSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		public override bool HasControlPointCapability(ControlPointId controlPointId, ControlPointCapabilities controlPointCapability) {
			switch (controlPointId) {
				case ControlPointIds.TopLeftControlPoint:
				case ControlPointIds.TopRightControlPoint:
				case ControlPointIds.BottomLeftControlPoint:
				case ControlPointIds.BottomCenterControlPoint:
				case ControlPointIds.BottomRightControlPoint:
					return ((controlPointCapability & ControlPointCapabilities.Resize) != 0);
				default: 
					return base.HasControlPointCapability(controlPointId, controlPointCapability);
			}
		}


		protected internal DocumentSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}


		/// <override></override>
		protected override bool CalculatePath() {
			if (base.CalculatePath()) {
				Path.Reset();
				Path.FillMode = System.Drawing.Drawing2D.FillMode.Winding;
				int currWidth = Width;
				int currHeight = Height;
				int size = currWidth > 40 && currHeight > 40 ? 20 : Math.Min(currWidth / 2, currHeight / 2);
				int halfHeight = currHeight / 2;
				int halfWidth = currWidth / 2;
				// Rechteck zeichnen
				Point[] points = new Point[5];
				points[0].X = -halfWidth;
				points[0].Y = -halfHeight;
				points[1].X = -halfWidth + currWidth - size;
				points[1].Y = -halfHeight;
				points[2].X = -halfWidth + currWidth;
				points[2].Y = -halfHeight + size;
				points[3].X = -halfWidth + currWidth;
				points[3].Y = -halfHeight + currHeight;
				points[4].X = -halfWidth;
				points[4].Y = -halfHeight + currHeight;
				Path.StartFigure();
				Path.AddLines(points);
				Path.CloseFigure();
				// Ecke hinzufügen
				points = new Point[3];
				points[0].X = -halfWidth + currWidth - size;
				points[0].Y = -halfHeight;
				points[1].X = -halfWidth + currWidth - size;
				points[1].Y = -halfHeight + size;
				points[2].X = -halfWidth + currWidth;
				points[2].Y = -halfHeight + size;
				Path.StartFigure();
				Path.AddLines(points);

				int maxH = currHeight / 8;
				for (int i = 1; i < maxH; ++i) {
					int right = -halfWidth + currWidth - 10;
					if (8 * i < size) right -= size;
					Path.StartFigure();
					Path.AddLine(-halfWidth + 10, -halfHeight + i * 8, right, -halfHeight + i * 8);
				}
				return true;
			}
			return false;
		}


		private Point[] shapePoints = new Point[6];
	}


	public class VectorImage : CustomizableMetaFile {
		
		internal static VectorImage CreateInstance(ShapeType shapeType, Template template, 
			string resourceBasename, Assembly resourceAssembly) {
			return new VectorImage(shapeType, template, resourceBasename, resourceAssembly);
		}


		/// <override></override>
		public override Shape Clone() {
			Shape result = new VectorImage(Type, Template, this._resourceName, this._resourceAssembly);
			result.CopyFrom(this);
			return result;
		}


		protected internal VectorImage(ShapeType shapeType, IStyleSet styleSet, string resourceBaseName, Assembly resourceAssembly)
			: base(shapeType, styleSet, resourceBaseName, resourceAssembly) {
		}


		protected internal VectorImage(ShapeType shapeType, Template template, string resourceBaseName, Assembly resourceAssembly)
			: base(shapeType, template, resourceBaseName, resourceAssembly) {
		}
	}

}
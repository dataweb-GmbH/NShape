/******************************************************************************
  Copyright 2009-2016 dataweb GmbH
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

using System.ComponentModel;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.SoftwareArchitectureShapes {

	public class DataFlowArrow : PolylineBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new DataFlowArrow(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		[CategoryAppearance()]
		[Description("Defines the line cap appearance of the line's beginning.\nUse the template editor to modify all shapes of a template.\nUse the design editor to modify and create styles.")]
		[PropertyMappingId(PropertyIdStartCapStyle)]
		[RequiredPermission(Permission.Present)]
		public ICapStyle StartCapStyle {
			get {
				if (StartCapStyleInternal == null && Template == null) throw new NShapeException("Property StartCapStyle is not set.");
				return StartCapStyleInternal == null ? ((DataFlowArrow)Template.Shape).StartCapStyle : StartCapStyleInternal;
			}
			set {
				StartCapStyleInternal = (Template != null && value == ((DataFlowArrow)Template.Shape).StartCapStyle) ? null : value;
			}
		}


		[CategoryAppearance()]
		[Description("Defines the line cap appearance of the line's ending.\nUse the template editor to modify all shapes of a template.\nUse the design editor to modify and create styles.")]
		[PropertyMappingId(PropertyIdEndCapStyle)]
		[RequiredPermission(Permission.Present)]
		public ICapStyle EndCapStyle {
			get {
				if (EndCapStyleInternal == null && Template == null) throw new NShapeException("Property EndCapStyle is not set.");
				return EndCapStyleInternal == null ? ((DataFlowArrow)Template.Shape).EndCapStyle : EndCapStyleInternal;
			}
			set {
				EndCapStyleInternal = (Template != null && value == ((DataFlowArrow)Template.Shape).EndCapStyle) ? null : value;
			}
		}


		protected internal DataFlowArrow(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}
	}


	public class DependencyArrow : PolylineBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new DependencyArrow(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		[CategoryAppearance()]
		[Description("Defines the line cap appearance of the line's beginning.\nUse the template editor to modify all shapes of a template.\nUse the design editor to modify and create styles.")]
		[PropertyMappingId(PropertyIdStartCapStyle)]
		[RequiredPermission(Permission.Present)]
		public ICapStyle StartCapStyle {
			get {
				if (StartCapStyleInternal == null && Template == null) throw new NShapeException("Property StartCapStyle is not set.");
				return StartCapStyleInternal == null ? ((DependencyArrow)Template.Shape).StartCapStyle : StartCapStyleInternal;
			}
			set {
				StartCapStyleInternal = (Template != null && value == ((DependencyArrow)Template.Shape).StartCapStyle) ? null : value;
			}
		}


		[CategoryAppearance()]
		[Description("Defines the line cap appearance of the line's ending.\nUse the template editor to modify all shapes of a template.\nUse the design editor to modify and create styles.")]
		[PropertyMappingId(PropertyIdEndCapStyle)]
		[RequiredPermission(Permission.Present)]
		public ICapStyle EndCapStyle {
			get {
				if (EndCapStyleInternal == null && Template == null) throw new NShapeException("Property EndCapStyle is not set.");
				return EndCapStyleInternal == null ? ((DependencyArrow)Template.Shape).EndCapStyle : EndCapStyleInternal;
			}
			set {
				EndCapStyleInternal = (Template != null && value == ((DependencyArrow)Template.Shape).EndCapStyle) ? null : value;
			}
		}


		protected internal DependencyArrow(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}

	}


	public class InterfaceUsageSymbol : PolylineBase {

		/// <override></override>
		public override Shape Clone() {
			InterfaceUsageSymbol result = new InterfaceUsageSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			if (Template == null) {
				//EndCapStyle = styleSet.GetCapStyle("Fork Cap");
				// @@Kurt
				// Den Style "Fork Cap" gibt es nicht in den Standard-Designs. Da müssen wir uns erst noch was überlegen wie 
				// man das Design um benötigte Styles erweitern kann (am Besten beim Registrieren der GeneralShapes)
				EndCapStyleInternal = styleSet.CapStyles.Special2;
			}
		}


		protected internal InterfaceUsageSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}
	}


	public class InterfaceSymbol : PolylineBase {

		/// <override></override>
		public override Shape Clone() {
			Shape result = new InterfaceSymbol(Type, this.Template);
			result.CopyFrom(this);
			return result;
		}


		/// <override></override>
		protected override void InitializeToDefault(IStyleSet styleSet) {
			base.InitializeToDefault(styleSet);
			if (Template == null) {
				//StartEndCapStyle = styleSet.GetCapStyle("Circle Cap");
				// @@Kurt
				// Den Style "Circle Cap" gibt es nicht in den Standard-Designs. Da müssen wir uns erst noch was überlegen wie 
				// man das Design um benötigte Styles erweitern kann (am Besten beim Registrieren der GeneralShapes)
				EndCapStyleInternal = styleSet.CapStyles.Special1;
			}
		}


		protected internal InterfaceSymbol(ShapeType shapeType, Template template)
			: base(shapeType, template) {
		}

	}

}

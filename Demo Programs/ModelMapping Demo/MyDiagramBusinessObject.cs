using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.WinFormsUI;


namespace ModelMapping_Demo
{


	/// <summary>
	/// This test class is for demonstration purposes only.
	/// Since DiagramModelObjects do not support property mapping, this demo implementation only makes some 
	/// visual properties of the NShape display component available in the NShape property editor by passing 
	/// them through the diagram model.
	/// </summary>
	public class MyDiagramBusinessObject : IDiagramModelObject
	{

		public MyDiagramBusinessObject(DiagramModelObjectType type)
		{
			this._diagramModelObjectType = type;
		}


		#region [Public] Properties

		[Category("Presentation")]
		[DisplayName("Is Sheet Visible")]
		[Description("Specifies whether the diagram sheet is visible.")]
		public bool IsSheetVisible {
			get { return (Display != null) ? Display.IsSheetVisible : false; }
			set {
				if (Display != null)
					Display.IsSheetVisible = value;
			}
		}


		[Category("Presentation")]
		[DisplayName("Zoom")]
		[Description("Specifies the zoom factor in percentage from 1 to 1000.")]
		public int Zoom {
			get { return (Display != null) ? Display.ZoomLevel : 100; }
			set {
				if (value < 1) value = 1;
				else if (value > 1000) value = 1000;

				if (Display != null)
					Display.ZoomLevel = value;
			}
		}


		[Category("Presentation")]
		[DisplayName("Grid Color")]
		[Description("Specifies whether the color of the background grid.")]
		public Color GridColor {
			get { return (Display != null) ? Display.GridColor : Color.LightGray; }
			set {
				if (Display != null)
					Display.GridColor = value;
			}
		}


		[Category("Editing")]
		[DisplayName("Is Grid Visible")]
		[Description("Specifies whether the background grid lines are visible.")]
		public bool IsGridVisible {
			get { return (Display != null) ? Display.IsGridVisible : false; }
			set {
				if (Display != null)
					Display.IsGridVisible = value;
			}
		}


		[Category("Editing")]
		[DisplayName("Grid Size")]
		[Description("Specifies the size of the background grid (in pixels).")]
		public int GridSize {
			get { return (Display != null) ? Display.GridSize : 0; }
			set {
				if (Display != null)
					Display.GridSize = value;
			}
		}


		#endregion


		#region IDiagramModelObject Implementation

		[Browsable(false)]
		public string Name {
			get { return _name; }
			set { if (_name != value) _name = value; }
		}


		public void AttachDiagram(Diagram diagram)
		{
			_diagram = diagram;
		}


		public IDiagramModelObject Clone()
		{
			MyDiagramBusinessObject clone = new MyDiagramBusinessObject(this._diagramModelObjectType);
			clone.Name = this.Name;
			clone.IsSheetVisible = this.IsSheetVisible;
			clone.IsGridVisible = this.IsGridVisible;
			clone.GridSize = this.GridSize;
			clone.GridColor = this.GridColor;
			clone.Zoom = this.Zoom;
			return clone;
		}


		public void DetachDiagram()
		{
			_diagram = null;
		}


		public IEnumerable<MenuItemDef> GetMenuItemDefs()
		{
			yield break;
		}


		[Browsable(false)]
		public Diagram Diagram {
			get { return _diagram; }
		}


		[Browsable(false)]
		public DiagramModelObjectType Type {
			get { return _diagramModelObjectType; }
		}


		[Browsable(false)]
		public object Id {
			get { return _id; }
		}


		[Browsable(false)]
		public char SecurityDomainName {
			get { return 'A'; }
		}

		#endregion


		#region IEntity implementation

		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.GenericModelObject" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version)
		{
			yield return new EntityFieldDefinition("Name", typeof(string));
			yield return new EntityFieldDefinition("IsSheetVisible", typeof(bool));
			yield return new EntityFieldDefinition("IsGridVisible", typeof(bool));
			yield return new EntityFieldDefinition("Zoom", typeof(int));
			yield return new EntityFieldDefinition("GridSize", typeof(int));
			yield return new EntityFieldDefinition("GridColor", typeof(int));
		}


		public void AssignId(object id)
		{
			if (this._id != null) throw new InvalidOperationException("An id is already assigned.");
			this._id = id;
		}

		public void Delete(IRepositoryWriter writer, int version)
		{
			throw new NotImplementedException();
		}

		public void LoadFields(IRepositoryReader reader, int version)
		{
			Name = reader.ReadString();
			IsSheetVisible = reader.ReadBool();
			IsGridVisible = reader.ReadBool();
			Zoom = reader.ReadInt32();
			GridSize = reader.ReadInt32();
			GridColor = Color.FromArgb(reader.ReadInt32());
		}

		public void LoadInnerObjects(string propertyName, IRepositoryReader reader, int version)
		{
			throw new NotImplementedException();
		}

		public void SaveFields(IRepositoryWriter writer, int version)
		{
			writer.WriteString(Name);
			writer.WriteBool(IsSheetVisible);
			writer.WriteBool(IsGridVisible);
			writer.WriteInt32(Zoom);
			writer.WriteInt32(GridSize);
			writer.WriteInt32(GridColor.ToArgb());
		}

		public void SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version)
		{
			throw new NotImplementedException();
		}

		#endregion


		private Display Display {
			get {
				if (Diagram != null && Diagram.DisplayService is Display)
					return (Display)Diagram.DisplayService;
				else return null;
			}
		}


		// Fields
		private Diagram _diagram;
		private object _id = null;
		private string _name;
		private DiagramModelObjectType _diagramModelObjectType;
	}

}

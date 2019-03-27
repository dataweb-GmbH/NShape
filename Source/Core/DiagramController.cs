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
using System.Collections.Generic;

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.Controllers {

	/// <ToBeCompleted></ToBeCompleted>
	public class DiagramController {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Controllers.DiagramController" />.
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="diagram"></param>
		public DiagramController(DiagramSetController owner, Diagram diagram) {
			if (owner == null) throw new ArgumentNullException("owner");
			if (owner.Project == null) throw new InvalidOperationException("DiagramSetController's Project property is not set.");
			this._owner = owner;
			this._diagram = diagram;
			if (((IEntity)diagram).Id != null)
				owner.Project.Repository.GetDiagramShapes(this._diagram);
		}


		#region [Public] Events

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler DiagramChanging;

		/// <ToBeCompleted></ToBeCompleted>
		public event EventHandler DiagramChanged;

		#endregion


		#region [Public] Properties

		/// <ToBeCompleted></ToBeCompleted>
		public DiagramSetController Owner {
			get { return _owner; }
		}


		/// <summary>
		/// Provides access to a <see cref="T:Dataweb.NShape.Project" />.
		/// </summary>
		public Project Project {
			get { return (_owner == null) ? null : _owner.Project; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Diagram Diagram { 
			get { return _diagram; }
			set {
				if (DiagramChanging != null) DiagramChanging(this, EventArgs.Empty);
				_diagram = value;
				if (_diagram != null) 
					_owner.Project.Repository.GetDiagramShapes(_diagram);
				if (DiagramChanged != null) DiagramChanged(this, EventArgs.Empty);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Tool Tool {
			get { return _owner.ActiveTool; }
			set { _owner.ActiveTool = value; }
		}

		#endregion


		#region [Public] Methods

		/// <ToBeCompleted></ToBeCompleted>
		public void CreateDiagram(string name) {
			_diagram = new Diagram(name);
			_owner.Project.Repository.Insert(_diagram);
			Diagram = _diagram;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public void OpenDiagram(string name) {
			Diagram = _owner.Project.Repository.GetDiagram(name);
		}


		/// <summary>
		/// Returns a collection of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" /> for constructing context menus etc.
		/// </summary>
		public IEnumerable<MenuItemDef> GetMenuItemDefs(IShapeCollection selectedShapes) {
			// ToDo: No actions at the moment
			yield break;
		}

		#endregion


		#region Fields

		private Diagram _diagram = null;
		private DiagramSetController _owner = null;

		#endregion
	}

}
/******************************************************************************
  Copyright 2009-2019 dataweb GmbH
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
using System.Drawing;
using System.Diagnostics;

using Dataweb.NShape.Commands;


namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Represents a possible action. Used to define a button, toolbar button or menu item that is linked to diagramming actions.
	/// </summary>
	public abstract class MenuItemDef {

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
		/// </summary>
		protected MenuItemDef() {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
		/// </summary>
		[Obsolete("Use MenuItemDef(string name, string title) instead.")]
		protected MenuItemDef(string title)
			: this(null, title, null, Color.Empty, null, false, true) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
		/// </summary>
		protected MenuItemDef(string name, string title)
			: this(name, title, null, Color.Empty, null, false, false) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
		/// </summary>
		[Obsolete("Use MenuItemDef(string name, string title, ...) instead.")]
		protected MenuItemDef(string title, Bitmap image, Color imageTransparentColor)  
			: this(null, title, image, imageTransparentColor, null, false, true) {
		}

		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
		/// </summary>
		protected MenuItemDef(string name, string title, Bitmap image, Color imageTransparentColor)
			: this(name, title, image, imageTransparentColor, null, false, true) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
		/// </summary>
		[Obsolete("Use MenuItemDef(string name, string title, ... ) instead.")]
		protected MenuItemDef(string title, Bitmap image, string description, bool isFeasible) 
			: this(null, title, image, Color.Empty, description, false, isFeasible) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
		/// </summary>
		protected MenuItemDef(string name, string title, Bitmap image, string description, bool isFeasible)
			: this(name, title, image, Color.Empty, description, false, isFeasible) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
		/// </summary>
		[Obsolete("Use MenuItemDef(string name, string title, ... ) instead.")]
		protected MenuItemDef(string title, Bitmap image, Color transparentColor, string name, string description, bool isChecked, bool isFeasible) 
			:this(name, title, image, transparentColor, description, isChecked, isFeasible) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
		/// </summary>
		protected MenuItemDef(string name, string title, Bitmap image, Color transparentColor, string description, bool isFeasible)
			: this(name, title, image, transparentColor, description, false, isFeasible) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
		/// </summary>
		protected MenuItemDef(string name, string title, Bitmap image, Color transparentColor, string description, bool isChecked, bool isFeasible)
			: this() {
			this._name = name;
			this._title = title;
			this._image = image;
			this._transparentColor = transparentColor;
			if (this._image != null) {
				if (this._transparentColor.IsEmpty)
					this._image.MakeTransparent();
				else this._image.MakeTransparent(this._transparentColor);
			}
			this._description = description;
			this._isChecked = isChecked;
			this._isFeasible = isFeasible;
		}


		/// <summary>
		/// Gets or sets an object that provides additional data.
		/// </summary>
		public object Tag {
			get { return _tag; }
			set { _tag = value; }
		}


		/// <summary>
		/// Culture invariant name that can be used as key for the presenting widget.
		/// </summary>
		public virtual string Title {
			get { return _title; }
			set { _title = value; }
		}


		/// <summary>
		/// Culture-depending title to display as caption of the presenting widget.
		/// </summary>
		public virtual string Name {
			get {
				if (string.IsNullOrEmpty(_name))
					return this.GetType().Name;
				else return _name;
			}
			set { _name = value; }
		}


		/// <summary>
		/// This text is displayed as tool tip by the presenting widget.
		/// Describes the performed action if active, the reason why it is disabled if the requirement for the action 
		/// is not met (e.g. Unselecting shapes requires selected shapes) or the reason why the action is not allowed.
		/// </summary>
		public virtual string Description {
			get { return _description; }
			set { _description = value; }
		}


		/// <summary>
		/// Subitems of the action.
		/// </summary>
		public virtual MenuItemDef[] SubItems {
			get { return _subItems; }
			protected set { _subItems = value; }
		}


		/// <summary>
		/// True if all requirements for performing the action are met. If false, the presenting widget should appear disabled.
		/// </summary>
		public virtual bool IsFeasible {
			get { return _isFeasible; }
			set { _isFeasible = value; }
		}


		/// <summary>
		/// Specifies if the action may or may not be executed due to security restrictions.
		/// </summary>
		public abstract bool IsGranted(ISecurityManager securityManager);


		/// <summary>
		/// True if the presenting item should appear as checked item.
		/// </summary>
		public virtual bool Checked {
			get { return _isChecked; }
			set { _isChecked = value; }
		}


		/// <summary>
		/// An image for the presenting widget's icon.
		/// </summary>
		public virtual Bitmap Image {
			get { return _image; }
			set {
				_image = value;
				if (_image != null && _transparentColor != Color.Empty)
					_image.MakeTransparent(_transparentColor);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public virtual Color ImageTransparentColor {
			get { return _transparentColor; }
			set {
				_transparentColor = value;
				if (_image != null && _transparentColor != Color.Empty)
					_image.MakeTransparent(_transparentColor);
			}
		}


		/// <ToBeCompleted></ToBeCompleted>
		public abstract void Execute(MenuItemDef action, Project project);


		#region Fields

		/// <ToBeCompleted></ToBeCompleted>
		private MenuItemDef[] _subItems = null;

		private object _tag = null;
		private string _title = string.Empty;
		private string _name = null;
		private string _description = null;
		private bool _isFeasible = true;
		private bool _isChecked = false;
		private Bitmap _image = null;
		private Color _transparentColor = Color.Empty;

		#endregion
	}


	/// <summary>
	/// Dummy action for creating MenuSeperators
	/// </summary>
	public class SeparatorMenuItemDef : MenuItemDef {

		/// <ToBeCompleted></ToBeCompleted>
		public SeparatorMenuItemDef() : base() { }


		/// <override></override>
		public override void Execute(MenuItemDef action, Project project) {
			if (action == null) throw new ArgumentNullException("action");
			if (project == null) throw new ArgumentNullException("project");
			// nothing to do
		}


		/// <override></override>
		public override string Name {
			get { return _name; }
			set { /* nothing to do */ }
		}


		/// <override></override>
		public override string Title {
			get { return _title; }
			set { /* nothing to do */ }
		}


		/// <override></override>
		public override string Description {
			get { return string.Empty; }
			set { /* nothing to do */ }
		}


		/// <override></override>
		public override bool IsGranted(ISecurityManager securityManager) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			return true;
		}


		/// <override></override>
		public override bool IsFeasible {
			get { return true; }
			set { /* nothing to do */ }
		}


		/// <override></override>
		public override bool Checked {
			get { return false; }
			set { /* nothing to do */ }
		}


		/// <override></override>
		public override Bitmap Image {
			get { return null; }
			set { /* nothing to do */ }
		}


		/// <override></override>
		public override Color ImageTransparentColor {
			get { return Color.Empty; }
			set { /* nothing to do */ }
		}


		private const string _name = "SeparatorAction";
		private const string _title = "----------";
	}


	/// <summary>
	/// Throws a NotImplementedException. 
	/// This class is meant as a placeholder and should never be used in a productive environment.
	/// </summary>
	public class NotImplementedMenuItemDef : MenuItemDef {

		const string NotImplementedMenuItemName = "NotImplementedAction";

		/// <ToBeCompleted></ToBeCompleted>
		public NotImplementedMenuItemDef(string title)
			: base(NotImplementedMenuItemName, title) {
		}

		/// <override></override>
		public override void Execute(MenuItemDef action, Project project) {
			if (action == null) throw new ArgumentNullException("action");
			if (project == null) throw new ArgumentNullException("project");
			throw new NotImplementedException();
		}


		/// <override></override>
		public override bool IsGranted(ISecurityManager securityManager) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			return true;
		}


		/// <override></override>
		public override bool IsFeasible {
			get { return false; }
			set { /* nothing to do */ }
		}


		/// <override></override>
		public override bool Checked {
			get { return false; }
			set { /* nothing to do */ }
		}


		/// <override></override>
		public override Bitmap Image {
			get { return null; }
			set { /* nothing to do */ }
		}


		/// <override></override>
		public override Color ImageTransparentColor {
			get { return Color.Empty; }
			set { /* nothing to do */ }
		}


		private const string notImplementedText = "This action is not yet implemented.";
	}


	/// <summary>
	/// Defines a group of <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
	/// </summary>
	public class GroupMenuItemDef : MenuItemDef {

		/// <ToBeCompleted></ToBeCompleted>
		public GroupMenuItemDef()
			: base() {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use MenuItemDef(string name, string title) instead.")]
		public GroupMenuItemDef(string title)
			: base(title) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public GroupMenuItemDef(string name, string title)
			: base(name, title) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use MenuItemDef(string name, string title, ...) instead.")]
		public GroupMenuItemDef(string title, Bitmap image, Color imageTransparentColor)
			: base(title, image, imageTransparentColor) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public GroupMenuItemDef(string name, string title, Bitmap image, Color imageTransparentColor)
			: base(name, title, image, imageTransparentColor) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use MenuItemDef(string name, string title, ...) instead.")]
		public GroupMenuItemDef(string title, Bitmap image, string description, bool isFeasible)
			: base(title, image, description, isFeasible) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public GroupMenuItemDef(string name, string title, Bitmap image, string description, bool isFeasible)
			: base(name, title, image, description, isFeasible) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use MenuItemDef(string name, string title, ...) instead.")]
		public GroupMenuItemDef(string title, Bitmap image, Color transparentColor, string name, string description, bool isChecked, bool isFeasible)
			: base(name, title, image, transparentColor, description, isChecked, isFeasible) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public GroupMenuItemDef(string name, string title, Bitmap image, Color transparentColor, string description, bool isChecked, bool isFeasible)
			: base(name, title, image, transparentColor, description, isChecked, isFeasible) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use MenuItemDef(string name, string title, ...) instead.")]
		public GroupMenuItemDef(string title, Bitmap image, string description, bool isFeasible, MenuItemDef[] actions, int defaultActionIndex)
			: this(null, title, image, Color.Empty, description, false, isFeasible, actions, defaultActionIndex) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public GroupMenuItemDef(string name, string title, Bitmap image, string description, bool isFeasible, MenuItemDef[] actions, int defaultActionIndex)
			: this(name, title, image, Color.Empty, description, false, isFeasible, actions, defaultActionIndex) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use MenuItemDef(string name, string title, ...) instead.")]
		public GroupMenuItemDef(string title, Bitmap image, Color transparentColor, string name, string description, bool isChecked, bool isFeasible, MenuItemDef[] actions, int defaultActionIndex)
			: this(name, title, image, transparentColor, description, isChecked, isFeasible, actions, defaultActionIndex) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public GroupMenuItemDef(string name, string title, Bitmap image, Color transparentColor, string description, bool isChecked, bool isFeasible, MenuItemDef[] actions, int defaultActionIndex)
			: base(name, title, image, transparentColor, description, isChecked, isFeasible) {
			this.SubItems = actions;
			this._defaultActionIdx = defaultActionIndex;
		}


		/// <override></override>
		public override bool IsGranted(ISecurityManager securityManager) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			return true;
		}


		/// <override></override>
		public override void Execute(MenuItemDef action, Project project) {
			//if (action == null) throw new ArgumentNullException("action");
			//if (project == null) throw new ArgumentNullException("project");
			//if (DefaultAction != null) DefaultAction.Execute(DefaultAction, project);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public MenuItemDef DefaultAction {
			get { return (SubItems == null || _defaultActionIdx < 0 || _defaultActionIdx >= SubItems.Length) ? null : SubItems[_defaultActionIdx]; }
		}


		private int _defaultActionIdx = -1;
	}


	/// <summary>
	/// Executes a given delegate.
	/// </summary>
	public class DelegateMenuItemDef : MenuItemDef {

		/// <ToBeCompleted></ToBeCompleted>
		public const char NoSecurityDomain = '\0';


		/// <ToBeCompleted></ToBeCompleted>
		public delegate void ActionExecuteDelegate(MenuItemDef action, Project project);


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string title) instead.")]
		public DelegateMenuItemDef(string title)
			: base(title, null, Color.Empty) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DelegateMenuItemDef(string name, string title)
			: base(name, title) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string text, ...) instead.")]
		public DelegateMenuItemDef(string title, Bitmap image, Color imageTransparentColor)
			: base(title, image, imageTransparentColor) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DelegateMenuItemDef(string name, string title, Bitmap image, Color imageTransparentColor)
			: base(name, title, image, imageTransparentColor) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string text, ...) instead.")]
		public DelegateMenuItemDef(string title, Bitmap image, string description, bool isFeasible, Permission requiredPermission, ActionExecuteDelegate executeDelegate)
			: this(string.Format("{0} Action", title), title, image, Color.Empty, description, false, isFeasible, requiredPermission, null, NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DelegateMenuItemDef(string name, string title, Bitmap image, string description, bool isFeasible, Permission requiredPermission, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, Color.Empty, description, false, isFeasible, requiredPermission, null, NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string text, ...) instead.")]		
		public DelegateMenuItemDef(string title, Bitmap image, string description, bool isFeasible, Permission requiredPermission, IEnumerable<ISecurityDomainObject> objects, ActionExecuteDelegate executeDelegate)
			: this(string.Format("{0} Action", title), title, image, Color.Empty, description, false, isFeasible, requiredPermission, objects, NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DelegateMenuItemDef(string name, string title, Bitmap image, string description, bool isFeasible, Permission requiredPermission, IEnumerable<ISecurityDomainObject> objects, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, Color.Empty, description, false, isFeasible, requiredPermission, objects, NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string text, ...) instead.")]
		public DelegateMenuItemDef(string title, Bitmap image, string description, bool isFeasible, Permission requiredPermission, IEnumerable<Shape> shapes, ActionExecuteDelegate executeDelegate)
			: this(string.Format("{0} Action", title), title, image, Color.Empty, description, false, isFeasible, requiredPermission, ConvertEnumerator<ISecurityDomainObject>.Create(shapes), NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DelegateMenuItemDef(string name, string title, Bitmap image, string description, bool isFeasible, Permission requiredPermission, IEnumerable<Shape> shapes, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, Color.Empty, description, false, isFeasible, requiredPermission, ConvertEnumerator<ISecurityDomainObject>.Create(shapes), NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string text, ...) instead.")]
		public DelegateMenuItemDef(string title, Bitmap image, string description, bool isFeasible, Permission requiredPermission, char securityDomainName,  ActionExecuteDelegate executeDelegate)
			: this(string.Format("{0} Action", title), title, image, Color.Empty, description, false, isFeasible, requiredPermission, null, securityDomainName, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DelegateMenuItemDef(string name, string title, Bitmap image, string description, bool isFeasible, Permission requiredPermission, char securityDomainName, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, Color.Empty, description, false, isFeasible, requiredPermission, null, securityDomainName, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string text, ...) instead.")]
		public DelegateMenuItemDef(string title, Bitmap image, string name, string description, bool isChecked, bool isFeasible, Permission requiredPermission, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, Color.Empty, description, isChecked, isFeasible, requiredPermission, null, NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DelegateMenuItemDef(string name, string title, Bitmap image, string description, bool isChecked, bool isFeasible, Permission requiredPermission, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, Color.Empty, description, isChecked, isFeasible, requiredPermission, null, NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string text, ...) instead.")]
		public DelegateMenuItemDef(string title, Bitmap image, Color transparentColor, string name, string description, bool isChecked, bool isFeasible, Permission requiredPermission, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, transparentColor, description, isChecked, isFeasible, requiredPermission, null, NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DelegateMenuItemDef(string name, string title, Bitmap image, Color transparentColor, string description, bool isChecked, bool isFeasible, Permission requiredPermission, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, transparentColor, description, isChecked, isFeasible, requiredPermission, null, NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string text, ...) instead.")]
		public DelegateMenuItemDef(string title, Bitmap image, Color transparentColor, string name, string description, bool isChecked, bool isFeasible, Permission requiredPermission, IEnumerable<ISecurityDomainObject> objects, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, transparentColor, description, isChecked, isFeasible, requiredPermission, objects, NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DelegateMenuItemDef(string name, string title, Bitmap image, Color transparentColor, string description, bool isChecked, bool isFeasible, Permission requiredPermission, IEnumerable<ISecurityDomainObject> objects, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, transparentColor, description, isChecked, isFeasible, requiredPermission, objects, NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string text, ...) instead.")]
		public DelegateMenuItemDef(string title, Bitmap image, Color transparentColor, string name, string description, bool isChecked, bool isFeasible, Permission requiredPermission, IEnumerable<Shape> shapes, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, transparentColor, description, isChecked, isFeasible, requiredPermission, ConvertEnumerator<ISecurityDomainObject>.Create(shapes), NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DelegateMenuItemDef(string name, string title, Bitmap image, Color transparentColor, string description, bool isChecked, bool isFeasible, Permission requiredPermission, IEnumerable<Shape> shapes, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, transparentColor, description, isChecked, isFeasible, requiredPermission, ConvertEnumerator<ISecurityDomainObject>.Create(shapes), NoSecurityDomain, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string text, ...) instead.")]
		public DelegateMenuItemDef(string title, Bitmap image, Color transparentColor, string name, string description, bool isChecked, bool isFeasible, Permission requiredPermission, char securityDomainName, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, transparentColor, description, isChecked, isFeasible, requiredPermission, null, securityDomainName, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public DelegateMenuItemDef(string name, string title, Bitmap image, Color transparentColor, string description, bool isChecked, bool isFeasible, Permission requiredPermission, char securityDomainName, ActionExecuteDelegate executeDelegate)
			: this(name, title, image, transparentColor, description, isChecked, isFeasible, requiredPermission, null, securityDomainName, executeDelegate) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		protected DelegateMenuItemDef(string name, string title, Bitmap image, Color transparentColor, string description, bool isChecked, bool isFeasible, Permission requiredPermission, IEnumerable<ISecurityDomainObject> objects, char securityDomainName, ActionExecuteDelegate executeDelegate)
			: base(name, title, image, transparentColor, description, isChecked, isFeasible) {
			this._executeDelegate = executeDelegate;
			this._requiredPermission = requiredPermission;
			this._securityDomainObjects = objects;
			this._securityDomainName = securityDomainName;
			Debug.Assert(PermissionsAreValid());
		}


		/// <override></override>
		public override void Execute(MenuItemDef action, Project project) {
			if (action == null) throw new ArgumentNullException("action");
			if (project == null) throw new ArgumentNullException("project");
			_executeDelegate(action, project);
		}


		/// <override></override>
		public override bool IsGranted(ISecurityManager securityManager) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			if (_securityDomainObjects != null)
				return securityManager.IsGranted(_requiredPermission, _securityDomainObjects);
			else if (_securityDomainName != NoSecurityDomain)
				return securityManager.IsGranted(_requiredPermission, _securityDomainName);
			else return securityManager.IsGranted(_requiredPermission);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public Permission RequiredPermission {
			get { return _requiredPermission; }
			set { _requiredPermission = value; }
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ActionExecuteDelegate Delegate {
			get { return _executeDelegate; }
			set { _executeDelegate = value; }
		}


		private bool PermissionsAreValid() {
			switch (_requiredPermission) {
				case Permission.All: return false;
				case Permission.None: return true;
				case Permission.Connect:
				case Permission.Data:
				case Permission.Delete:
				case Permission.Insert:
				case Permission.Layout:
				case Permission.Present:
					return (_securityDomainObjects != null || _securityDomainName != NoSecurityDomain);
				case Permission.Designs:
				case Permission.Security:
				case Permission.Templates:
					return (_securityDomainObjects == null && _securityDomainName == NoSecurityDomain);
				default:
					// Permissions are flags, so we have to check for domain independent permissions and assume a domain dependent flag combination otherwise.
					if ((_requiredPermission & Permission.Designs) != 0 || (_requiredPermission & Permission.Designs) != 0 || (_requiredPermission & Permission.Designs) != 0)
						return (_securityDomainObjects == null && _securityDomainName == NoSecurityDomain);
					else 
						return (_securityDomainObjects != null || _securityDomainName != NoSecurityDomain);
			}
		}


		// Fields
		private Permission _requiredPermission = Permission.None;
		private char _securityDomainName = NoSecurityDomain;
		private IEnumerable<ISecurityDomainObject> _securityDomainObjects = null;
		private ActionExecuteDelegate _executeDelegate;
	}


	/// <summary>
	/// Adds a Command to the History and executes it.
	/// </summary>
	public class CommandMenuItemDef : MenuItemDef {

		/// <ToBeCompleted></ToBeCompleted>
		public CommandMenuItemDef()
			: base() { }


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string title) instead.")]
		public CommandMenuItemDef(string title)
			: base(title) { }


		/// <ToBeCompleted></ToBeCompleted>
		public CommandMenuItemDef(string name, string title)
			: base(name, title) { }


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string title) instead.")]
		public CommandMenuItemDef(string title, Bitmap image, Color transparentColor)
			: base(title, image, transparentColor) { }


		/// <ToBeCompleted></ToBeCompleted>
		public CommandMenuItemDef(string name, string title, Bitmap image, Color transparentColor)
			: base(name, title, image, transparentColor) { }


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string title) instead.")]
		public CommandMenuItemDef(string title, Bitmap image, string description, bool isFeasible, ICommand command)
			: this((command != null) ? command.GetType().Name : null, title, image, Color.Empty, description, false, isFeasible, command) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string title, ...) instead.")]
		public CommandMenuItemDef(string title, Bitmap image, string name, string description, bool isFeasible, ICommand command)
			: this(name, title, image, Color.Empty, description, false, isFeasible, command) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CommandMenuItemDef(string name, string title, Bitmap image, string description, bool isFeasible, ICommand command)
			: this(name, title, image, Color.Empty, description, false, isFeasible, command) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete("Use DelegateMenuItemDef(string name, string title, ...) instead.")]
		public CommandMenuItemDef(string title, Bitmap image, Color transparentColor, string name, string description, bool isChecked, bool isFeasible, ICommand command)
			: this(name, title, image, transparentColor, description, isChecked, isFeasible, command) {
		}


		/// <ToBeCompleted></ToBeCompleted>
		public CommandMenuItemDef(string name, string title, Bitmap image, Color transparentColor, string description, bool isChecked, bool isFeasible, ICommand command)
			: base(name, title, image, transparentColor, description, isChecked, isFeasible) {
			this._command = command;
		}


		/// <override></override>
		public override string Description {
			get {
				if (IsFeasible) return _command.Description;
				else return base.Description;
			}
			set { base.Description = value; }
		}


		/// <override></override>
		public override bool IsGranted(ISecurityManager securityManager) {
			if (securityManager == null) throw new ArgumentNullException("securityManager");
			if (_command != null) {
				Exception exc = _command.CheckAllowed(securityManager);
				if (exc != null) {
					Description = exc.Message;
					return false;
				} else return true;
			} else return false;
		}


		/// <override></override>
		public override void Execute(MenuItemDef action, Project project) {
			if (action == null) throw new ArgumentNullException("action");
			if (project == null) throw new ArgumentNullException("project");
			if (_command != null) project.ExecuteCommand(_command);
		}


		/// <summary>
		/// Specifies the command executed by the <see cref="T:Dataweb.NShape.Advanced.MenuItemDef" />.
		/// </summary>
		public ICommand Command {
			get { return _command; }
		}


		#region Fields
		private ICommand _command = null;
		#endregion
	}

}

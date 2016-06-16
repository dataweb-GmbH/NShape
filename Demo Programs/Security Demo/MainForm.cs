/******************************************************************************
  Copyright 2009-2012 dataweb GmbH
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
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.Controllers;
using Dataweb.NShape.WinFormsUI;


namespace Security_Demo {

	public partial class MainForm : Form {

		public MainForm() {
			InitializeComponent();
			//SecurityManager.CurrentRole = StandardRole.Guest;
		}


		public RoleBasedSecurityManager SecurityManager {
			get { return (RoleBasedSecurityManager)project.SecurityManager; }
		}


		/// <summary>
		/// Update all controls that display security information
		/// </summary>
		public void UpdateSecurityInfoCtrls() {
			if (cboUser.SelectedItem == null || (StandardRole)cboUser.SelectedItem != SecurityManager.CurrentRole)
				cboUser.SelectedItem = SecurityManager.CurrentRole;
			char domainName = (char)(cboDomain.SelectedItem ?? SecurityDemoHelper.NoDomain);

			SecurityDemoHelper.UpdatePermissionTableCtrls(SecurityManager, domainPermissionInfoPanel, domainName, SecurityAccess.View);
			SecurityDemoHelper.UpdatePermissionTableCtrls(SecurityManager, domainPermissionInfoPanel, domainName, SecurityAccess.Modify);
			UpdateDomainPermissionCtrls();
			
			// Update displayed properties
			propertyGrid1.Refresh();
		}


		#region [Private] Methods

		private void UpdateDomainPermissionCtrls() {
			UpdateDomainPermissionCtrls(currentSecurityObjects, currentSecurityObjectType);
		}


		private void UpdateDomainPermissionCtrls(ISecurityDomainObject securityObject, Type objType) {
			IEnumerable<ISecurityDomainObject> securityObjs = null;
			if (securityObject != null)
				securityObjs = SingleInstanceEnumerator<ISecurityDomainObject>.Create(securityObject);
			UpdateDomainPermissionCtrls(securityObjs, objType);
		}


		private void UpdateDomainPermissionCtrls(IEnumerable<ISecurityDomainObject> securityObjects, Type objType) {
			lblCurrentSecObj.Text = (objType != null) ? objType.Name : string.Empty;
			string currObjDom = string.Empty;
			if (securityObjects != null) {
				foreach (ISecurityDomainObject obj in securityObjects) {
					if (obj == null) continue;
					if (currObjDom.IndexOf(obj.SecurityDomainName) < 0)
						currObjDom += string.Format("{0}{1}", string.IsNullOrEmpty(currObjDom) ? "" : ", ", obj.SecurityDomainName);
				}
				lblCurrentDomain.Text = currObjDom;
			}

			string currGenPermissions = string.Empty;
			string currDomPermissions = string.Empty;
			foreach (Permission permission in Enum.GetValues(typeof(Permission))) {
				switch (permission) {
					case Permission.All:
					case Permission.None:
						// Nothing to do
						break;
					case Permission.Designs:
					case Permission.Security:
					case Permission.Templates:
						// Update effective permissions
						if (!string.IsNullOrEmpty(currGenPermissions))
							currGenPermissions += ", ";
						currGenPermissions += SecurityDemoHelper.GetPermissionString(permission);
						break;
					case Permission.Connect:
					case Permission.Data:
					case Permission.Delete:
					case Permission.Insert:
					case Permission.Layout:
					case Permission.Present:
						if (securityObjects != null) {
							if (SecurityManager.IsGranted(permission, securityObjects)) {
								if (!string.IsNullOrEmpty(currDomPermissions))
									currDomPermissions += ", ";
								currDomPermissions += SecurityDemoHelper.GetPermissionString(permission);
							}
						}
						break;
					default:
						Debug.Fail("Unhandled Permission!");
						break;
				}
			}
			if (string.IsNullOrEmpty(currGenPermissions))
				currGenPermissions = Permission.None.ToString();
			lblCurrentGenPerm.Text = currGenPermissions;
			if (string.IsNullOrEmpty(currDomPermissions))
				currDomPermissions = Permission.None.ToString();
			lblCurrentDomPerm.Text = currDomPermissions;
		}


		private void InitializeSecurityManager() {
			// Clear all default security settings (clear permissions and delete domains)
			foreach (StandardRole role in Enum.GetValues(typeof(StandardRole))) {
				if (role == StandardRole.Custom) continue;
				foreach (SecurityAccess access in Enum.GetValues(typeof(SecurityAccess)))
					SecurityManager.SetPermissions(role, Permission.None, access);
			}
			SecurityManager.RemoveDomain('A');
			SecurityManager.RemoveDomain('B');

			// Define security domains
			SecurityManager.AddDomain('A', "User Shapes - All editing options allowed.");
			SecurityManager.AddDomain('B', "Predefined Wires - Wire shapes can be moved and (dis)connected.");
			SecurityManager.AddDomain('C', "Predefined Shapes - Loaded shapes can be moved and resized.");
			SecurityManager.AddDomain('D', "Diagram - Diagram's Background can be changed.");
			SecurityManager.AddDomain('E', "Model Shapes - Only Data properties can be changed.");

			// Define permission sets for all roles
			Permission generalPermissions;
			Permission domainAPermissions;
			Permission domainBPermissions;
			Permission domainCPermissions;
			Permission domainDPermissions;
			Permission domainEPermissions;
			Permission domainStdPermissions;
			SecurityAccess generalPermissionAccess;
			SecurityAccess domainPermissionAccess;
			// Assign the permissions to each role's permission set
			foreach (StandardRole role in cboUser.Items) {
				switch (role) {
					case StandardRole.Administrator:
						// Administrators have full access -> Permissions of all domains are granted with modify access
						generalPermissionAccess =
						domainPermissionAccess = SecurityAccess.Modify;
						generalPermissions =
						domainAPermissions =
						domainBPermissions =
						domainCPermissions =
						domainDPermissions =
						domainEPermissions =
						domainStdPermissions = Permission.All;
						break;
					case StandardRole.SuperUser:
						// Super user has full access to the general permissions
						generalPermissionAccess = SecurityAccess.Modify;
						generalPermissions = Permission.Designs | Permission.Templates;
						// Super user has modify access
						domainPermissionAccess = SecurityAccess.Modify;
						// Super user the following access permissions for the domains:
						// Full access for user-created shapes
						domainAPermissions = Permission.Insert | Permission.Delete | Permission.Layout | Permission.Data | Permission.Connect | Permission.Present;
						// Pre-defined wire shapes (loaded from the sample diagram) may be moved, connected, colored and the data properties may be modified.
						domainBPermissions = Permission.Layout | Permission.Connect | Permission.Data | Permission.Present;
						// Pre-defined shapes (loaded from the sample diagram) may be moved, colored and the data properties may be modified.
						domainCPermissions = Permission.Layout | Permission.Data | Permission.Present;
						// The (loaded) diagram may be redesigned (background color/-image) and the data properties may be modified.
						domainDPermissions = Permission.Present | Permission.Data;
						// The model shapes grant only access to their data properties.
						domainEPermissions = Permission.Data | Permission.Present;
						// All other domains do not grant any access permission.
						domainStdPermissions = Permission.None;
						break;
					default:
						// All other roles only grant read-only access...
						generalPermissionAccess = SecurityAccess.View;
						// ... except for the designer who may adjust the design and re-style the template shapes.
						generalPermissions = (role == StandardRole.Designer) ? Permission.Designs | Permission.Templates : Permission.None;

						// All other roles only grant read-only access (except for the designer)
						domainPermissionAccess = (role == StandardRole.Designer) ? SecurityAccess.Modify : SecurityAccess.View;
						// User created shape grant access to all of their properties.
						// Whether this access is read only or not depends on the domain permission access.
						domainAPermissions = Permission.Insert | Permission.Delete | Permission.Layout | Permission.Data | Permission.Connect;
						// Pre-defined wire shapes (loaded from the sample diagram) grant access to their layout properties and may be connected.
						// Whether this access is read only or not depends on the domain permission access.
						domainBPermissions = Permission.Layout | Permission.Connect;
						// Pre-defined shapes (loaded from the sample diagram) grant access to their layout properties.
						// Whether this access is read only or not depends on the domain permission access.
						domainCPermissions = Permission.Layout;
						// The (loaded) diagram grants access to its presentation properties.
						// Whether this access is read only or not depends on the domain permission access.
						domainDPermissions = Permission.Present;
						// The model shapes grant access to their data properties.
						// Whether this access is read only or not depends on the domain permission access.
						domainEPermissions = Permission.Data;
						// All other domains do not grant any access permission.
						domainStdPermissions = Permission.None;
						break;
				}

				// Apply general permissions
				SecurityManager.SetPermissions(role, generalPermissions, generalPermissionAccess);
				// Apply shape permissions for shapes inserted by the user
				SecurityManager.SetPermissions('A', role, domainAPermissions, domainPermissionAccess);
				// Apply shape permissions for the pre-defined wire shapes (loaded from the sample diagram)
				SecurityManager.SetPermissions('B', role, domainBPermissions, domainPermissionAccess);
				// Apply shape permissions for the pre-defined shapes (loaded from the sample diagram)
				SecurityManager.SetPermissions('C', role, domainCPermissions, domainPermissionAccess);
				// Apply diagram permissions
				SecurityManager.SetPermissions('D', role, domainDPermissions, domainPermissionAccess);
				// Apply model shape permissions
				SecurityManager.SetPermissions('E', role, domainEPermissions, domainPermissionAccess);
				// Set permissions for all other security domains
				for (char c = 'F'; c != 'Z'; ++c)
					SecurityManager.SetPermissions(c, role, domainStdPermissions, domainPermissionAccess);
			}
			// Set the current user role.
			SecurityManager.CurrentRole = StandardRole.Designer;
		}

		#endregion


		#region [Private] Methods: Event Handler Implementations

		private void Form1_Load(object sender, EventArgs e) {
			try {
				// Set up the XML storee component (storage directory and file extension)
				string dir = Path.GetDirectoryName(Path.GetDirectoryName(Application.StartupPath)) + @"\Demo Programs\Security Demo\Sample Project";
				xmlStore.DirectoryName = dir;
				xmlStore.FileExtension = ".nspj";

				// Add a search path for shape assemblies (application's startup directory in this case)
				project.LibrarySearchPaths.Add(Application.StartupPath);
				// Set the name of the project to open and open it
				project.Name = "Security Demo Sample Project";
				project.Open();

				// Activate the pointer tool and load the sample diagram
				display1.ActiveTool = toolSetController.DefaultTool;
				display1.LoadDiagram("Simple Circuit");

				chkHideMenuItems.Checked = true;
				chkHideProperties.Checked = true;
				SecurityDemoHelper.FillUserAndDomainComboBoxes(cboUser, cboDomain);
				InitializeSecurityManager();

				UpdateSecurityInfoCtrls();
			} catch (Exception exc) {
				// Always catch exceptions in the Form.Load event (and show their messages) as the framework 
				// swallows all exceptions thrown in this exception handler without any notice in non-debug mode!
				MessageBox.Show(this, exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		
		private void userOrDomainCheckBox_SelectedIndexChanged(object sender, EventArgs e) {
			if (cboUser.SelectedItem != null) {
				SecurityManager.CurrentRole = (StandardRole)cboUser.SelectedItem;
				UpdateSecurityInfoCtrls();
			}
		}

		
		private void propertyController1_ObjectsSet(object sender, Dataweb.NShape.Controllers.PropertyControllerEventArgs e) {
			// Get common security domain name of all selected objects
			if (e.Objects.Count > 0) {
				char dom = SecurityDemoHelper.NoDomain;
				foreach (object o in e.Objects) {
					if (o is ISecurityDomainObject) {
						if (dom == SecurityDemoHelper.NoDomain)
							dom = ((ISecurityDomainObject)o).SecurityDomainName;
						else if (dom != ((ISecurityDomainObject)o).SecurityDomainName) {
							dom = '!';
							break;
						}
					}
				}
				cboDomain.SelectedIndex = Math.Max(dom - 'A', -1);
			} else 
				cboDomain.SelectedIndex = -1;
			
			// Store security objects and their type for later use
			if (e.Objects.Count > 0) {
				currentSecurityObjects = SecurityDemoHelper.GetSecurityDomainObjects(e.Objects);
				currentSecurityObjectType = e.ObjectsType;
			} else {
				currentSecurityObjects = null;
				currentSecurityObjectType = null;
			}
			UpdateDomainPermissionCtrls(currentSecurityObjects, currentSecurityObjectType);
		}

		
		private void chkHideMenuItems_CheckedChanged(object sender, EventArgs e) {
			display1.HideDeniedMenuItems =
			toolSetListViewPresenter.HideDeniedMenuItems = chkHideMenuItems.Checked;
		}

		
		private void chkHideProperties_CheckedChanged(object sender, EventArgs e) {
			propertyController.PropertyDisplayMode = chkHideProperties.Checked ? NonEditableDisplayMode.Hidden : NonEditableDisplayMode.ReadOnly;
			// Refresh displayed properties
			propertyGrid1.Refresh();
		}

		
		private void editPermissionsButton_Click(object sender, EventArgs e) {
			if (securityEditor == null) {
				// Show the demo's security settings editor dialog
				securityEditor = new SecuritySettingsEditor(this);
				securityEditor.FormClosed += dlg_FormClosed;
				securityEditor.Show(this);
			} else securityEditor.Activate();
		}


		private void toolSetController1_LibraryManagerSelected(object sender, EventArgs e) {
			// Show the LibraryManagerDialog when requested by the tool set controller 
			using (LibraryManagementDialog dlg = new LibraryManagementDialog(project))
				dlg.ShowDialog(this);
		}


		private void toolSetController1_TemplateEditorSelected(object sender, TemplateEditorEventArgs e) {
			// Show the TemplateEditorDialog when requested by the tool set controller 
			using (TemplateEditorDialog dlg = new TemplateEditorDialog(project, e.Template))
				dlg.ShowDialog(this);
		}


		private void toolSetController1_DesignEditorSelected(object sender, EventArgs e) {
			// Show the DesignEditorDialog when requested by the tool set controller
			DesignEditorDialog dlg = new DesignEditorDialog(project);
			dlg.FormClosed += new FormClosedEventHandler(dlg_FormClosed);
			dlg.Show(this);
		}


		private void dlg_FormClosed(object sender, FormClosedEventArgs e) {
			Form dlg = sender as Form;
			if (dlg != null) {
				dlg.FormClosed -= dlg_FormClosed;
				dlg.Dispose();
			}
			if (dlg == securityEditor) securityEditor = null;
		}

		#endregion


		private IEnumerable<ISecurityDomainObject> currentSecurityObjects = null;
		private Type currentSecurityObjectType = null;
		private SecuritySettingsEditor securityEditor = null;
	}


	internal static class SecurityDemoHelper {

		public const char NoDomain = '\0';


		public static void FillUserAndDomainComboBoxes(ComboBox cboUser, ComboBox cboDomain) {
			cboUser.Items.Clear();
			foreach (StandardRole role in Enum.GetValues(typeof(StandardRole))) {
				switch (role) {
					case StandardRole.Administrator:
					case StandardRole.SuperUser:
					case StandardRole.Designer:
					case StandardRole.Guest:
						cboUser.Items.Add(role); break;
					default:
						continue;
				}
			}
			cboDomain.Items.Clear();
			for (char c = 'A'; c <= 'Z'; ++c)
				cboDomain.Items.Add(c);
			cboDomain.SelectedIndex = -1;
		}


		public static void UpdatePermissionTableCtrls(RoleBasedSecurityManager securityManager, TableLayoutPanel panel, char securityDomain, SecurityAccess access) {
			bool isGranted = false;
			foreach (Permission permission in Enum.GetValues(typeof(Permission))) {
				switch (permission) {
					case Permission.All:
					case Permission.None:
						break;
					case Permission.Designs:
					case Permission.Security:
					case Permission.Templates: {
							isGranted = securityManager.IsGranted(permission, access);
							SetCheckState(panel, permission, access, isGranted);
						}
						break;
					case Permission.Connect:
					case Permission.Data:
					case Permission.Delete:
					case Permission.Insert:
					case Permission.Layout:
					case Permission.Present: {
							isGranted = (securityDomain != NoDomain) ? securityManager.IsGranted(permission, access, securityDomain) : false;
							SetCheckState(panel, permission, access, isGranted);
						}
						break;
					default:
						Debug.Fail("Unhandled Permission!");
						break;
				}
			}
		}


		public static bool GetCheckState(Panel panel, Permission permission, SecurityAccess access) {
			string name = string.Format("chk{0}{1}", access, permission);
			return ((CheckBox)panel.Controls[name]).Checked;
		}


		public static void SetCheckState(Panel panel, Permission permission, SecurityAccess access, bool value) {
			string name = string.Format("chk{0}{1}", access, GetPermissionString(permission));
			CheckBox chk = (CheckBox)panel.Controls[name];
			if (chk.Checked != value) chk.Checked = value;
		}


		public static string GetPermissionString(Permission permission) {
			string result = null;
			if (permission == Permission.Security)
				result = "Security";
			else if (permission == Permission.Data)
				result = "Data";
			else result = permission.ToString();
			return result;
		}


		public static IEnumerable<ISecurityDomainObject> GetSecurityDomainObjects(IEnumerable<object> objects) {
			foreach (object o in objects) {
				if (o is ISecurityDomainObject)
					yield return o as ISecurityDomainObject;
			}
		}

	}

}

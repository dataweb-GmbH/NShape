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
using System.Windows.Forms;
using Dataweb.NShape;


namespace Security_Demo {

	public partial class SecuritySettingsEditor : Form {
		
		public SecuritySettingsEditor(MainForm owner) {
			InitializeComponent();
			if (owner == null) throw new ArgumentNullException("owner");
			this.owner = owner;

			SecurityDemoHelper.FillUserAndDomainComboBoxes(cboUser, cboDomain);
			cboUser.SelectedItem = owner.SecurityManager.CurrentRole;
			cboDomain.SelectedIndex = 0;
		}


		private void UpdateSecurityControls() {
			if (cboUser.SelectedItem != null && cboDomain.SelectedItem != null) {
				// Update Security infos and the owner's controls
				owner.SecurityManager.CurrentRole = (StandardRole)cboUser.SelectedItem;
				owner.UpdateSecurityInfoCtrls();

				char domainName = (char)cboDomain.SelectedItem;
				SecurityDemoHelper.UpdatePermissionTableCtrls(owner.SecurityManager, permissionsPanel, domainName, Dataweb.NShape.SecurityAccess.View);
				SecurityDemoHelper.UpdatePermissionTableCtrls(owner.SecurityManager, permissionsPanel, domainName, Dataweb.NShape.SecurityAccess.Modify);
			}
		}

		
		private void permissionCheckBox_CheckedChanged(object sender, EventArgs e) {
			CheckBox chkBox = sender as CheckBox;
			if (chkBox != null) {
				// Find out which permission has changed 
				Permission permission = Permission.None;
				foreach (Permission p in Enum.GetValues(typeof(Permission))) {
					if (chkBox.Name.Contains(SecurityDemoHelper.GetPermissionString(p))) {
						permission = p;
						break;
					}
				}
				// Check if the permission is set
				char domainName = (char)cboDomain.SelectedItem;
				SecurityAccess access = chkBox.Name.Contains("View") ? SecurityAccess.View : SecurityAccess.Modify;
				bool permissionGranted = owner.SecurityManager.IsGranted(permission, access, domainName);

				// Update only if the granted state changed
				if (permissionGranted != chkBox.Checked) {
					StandardRole role = (StandardRole)cboUser.SelectedItem;
					// Set General or Domain permissions
					if (permission == Permission.Designs || permission == Permission.Security || permission == Permission.Templates) {
						if (chkBox.Checked)
							owner.SecurityManager.AddPermissions(role, permission, access);
						else owner.SecurityManager.RemovePermissions(role, permission, access);
					} else {
						if (chkBox.Checked)
							owner.SecurityManager.AddPermissions(domainName, role, permission, access);
						else owner.SecurityManager.RemovePermissions(domainName, role, permission, access);
					}

					UpdateSecurityControls();
				}
			}
		}

		
		private void cboUserOrDom_SelectedIndexChanged(object sender, EventArgs e) {
			UpdateSecurityControls();
		}


		private MainForm owner;
	}

}

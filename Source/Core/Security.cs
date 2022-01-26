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


namespace Dataweb.NShape {

	/// <summary>
	/// Specifies the permission.
	/// </summary>
	[Flags]
	public enum Permission {
		/// <summary>No permissions are granted.</summary>
		None = 0x0000,
		/// <summary>Assign a security domain to an <see cref="T:Dataweb.NShape.ISecurityDomainObject" />. This permission is security domain independent.</summary>
		Security = 0x0001,
		/// <summary>Assign a security domain to any shape. This permission is security domain independent.</summary>
		[Obsolete("Use Permission.Security instead of Permission.ModifyPermissionSet.")]
		ModifyPermissionSet = 0x0001,
		/// <summary>Modify position, size, rotation or z-order of shapes. This permission depends on the shape to modify.</summary>
		Layout = 0x0002,
		/// <summary>Modify the appearance of the shape (color, line thickness etc.) and assign other styles. This permission depends on the shape to modify.</summary>
		Present = 0x0004,
		/// <summary>Modify data properties. This permission depends on the <see cref="T:Dataweb.NShape.ISecurityDomainObject" /> to modify.</summary>
		Data = 0x0008,
		/// <summary>Modify data properties. This permission depends on the <see cref="T:Dataweb.NShape.ISecurityDomainObject" /> to modify.</summary>
		[Obsolete("Use Permission.Data instead of Permission.ModifyData.")]
		ModifyData = 0x0008,
		/// <summary>Insert shape into diagram. This permission depends on the <see cref="T:Dataweb.NShape.ISecurityDomainObject" /> to insert.</summary>
		Insert = 0x0010,
		/// <summary>Remove shape from diagram. This permission depends on the <see cref="T:Dataweb.NShape.ISecurityDomainObject" /> to remove.</summary>
		Delete = 0x0020,
		/// <summary>Connect or disconnect shapes. This permission depends on the active <see cref="T:Dataweb.NShape.Advanced.Shape" /> of the connection.</summary>
		Connect = 0x0040,
		/// <summary>Edit, insert and delete templates. This permission is security domain independent.</summary>
		Templates = 0x0080,
		/// <summary>Edit, insert and delete designs. This permission is security domain independent.</summary>
		Designs = 0x0100,
		/// <summary>All available permissions are granted.</summary>
		All = int.MaxValue
	}


	/// <summary>
	/// Specifies whether a permission is granted for modification or viewing.
	/// </summary>
	public enum SecurityAccess {
		/// <summary>Permission for modification</summary>
		Modify,
		/// <summary>Permission for viewing</summary>
		View
	}


	/// <summary>
	/// Specifies the set of <see cref="T:Dataweb.NShape.Permission" /> required for changing a property.
	/// </summary>
	[AttributeUsage((AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field), AllowMultiple = true, Inherited = true)]
	public sealed class RequiredPermissionAttribute : Attribute {
		
		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.RequiredPermissionAttribute" />.
		/// </summary>
		public RequiredPermissionAttribute(Permission requiredPermission) {
			_permission = requiredPermission;
		}

		/// <summary>
		/// Specifies the set of <see cref="T:Dataweb.NShape.Permission" /> required required.
		/// </summary>
		public Permission Permission {
			get { return _permission; }
		}

		private Permission _permission;
	}


	/// <summary>
	/// Interface for security aware objects.
	/// </summary>
	public interface ISecurityDomainObject {
		/// <summary>Specifies the permission set that required permissions are checked against.</summary>
		char SecurityDomainName { get; }
	}


	/// <summary>
	/// Controls the access to diagram operations.
	/// </summary>
	public interface ISecurityManager {

		/// <summary>
		/// Checks whether the given domain-independent permission is granted by for the current role.
		/// </summary>
		bool IsGranted(Permission permission);

		/// <summary>
		/// Checks whether the given domain-independent permission is granted by for the current role.
		/// </summary>
		bool IsGranted(Permission permission, SecurityAccess access);

		/// <summary>
		/// Checks  whether then given permission is granted for the domain and the current role.
		/// </summary>
		bool IsGranted(Permission permission, char domainName);

		/// <summary>
		/// Checks whether the given access on the then given permission is granted for the domain and the current role.
		/// </summary>
		bool IsGranted(Permission permission, SecurityAccess access, char domainName);

		/// <summary>
		/// Checks whether a given permission is granted for a given <see cref="T:Dataweb.NShape.ISecurityDomainObject" />
		/// by the current user permissions.
		/// </summary>
		bool IsGranted(Permission permission, ISecurityDomainObject securityDomainObject);

		/// <summary>
		/// Checks whether the specified access for a given permission is granted for the given 
		/// <see cref="T:Dataweb.NShape.ISecurityDomainObject" />'s security domain.
		/// </summary>
		bool IsGranted(Permission permission, SecurityAccess access, ISecurityDomainObject securityDomainObject);

		/// <summary>
		/// Checks whether a given permission is granted for all <see cref="T:Dataweb.NShape.ISecurityDomainObject" />s 
		/// of a list by the current user permissions.
		/// </summary>
		bool IsGranted<TSecurityDomainObject>(Permission permission, IEnumerable<TSecurityDomainObject> securityDomainObjects)
		    where TSecurityDomainObject : ISecurityDomainObject;

		/// <summary>
		/// Checks whether the specified access for a given permission is granted for the security domain of all 
		/// <see cref="T:Dataweb.NShape.ISecurityDomainObject" />s in the list.
		/// </summary>
		bool IsGranted<TSecurityDomainObject>(Permission permission, SecurityAccess access, IEnumerable<TSecurityDomainObject> securityDomainObjects) 
			where TSecurityDomainObject : ISecurityDomainObject;

	}


	/// <summary>
	/// Defines a standard user role
	/// </summary>
	public enum StandardRole {
		/// <summary>All permissions are granted.</summary>
		Administrator,
		/// <summary>Most permissions are granted.</summary>
		SuperUser,
		/// <summary>Permissions required for designing diagrams are granted.</summary>
		Designer,
		/// <summary>Permissions needed for changing the state of objects are granted.</summary>
		Operator,
		/// <summary>Nearly no permissions are granted.</summary>
		Guest,
		/// <summary>Custom permissons are granted.</summary>
		Custom
	}
	
	
	/// <summary>
	/// SecurityManager implementation based on a fixed set of user roles.
	/// </summary>
	public class RoleBasedSecurityManager : ISecurityManager {

		/// <ToBeCompleted></ToBeCompleted>
		public const char NoSecurityDomain = '\0';


		/// <summary>
		/// Creates a default security object with standard roles and domains.
		/// </summary>
		public RoleBasedSecurityManager() {
			char domain;

			AddRole(roleNameAdministrator, Dataweb.NShape.Properties.Resources.CaptionTxt_RoleDescriptionAdmin);
			AddRole(roleNameSuperUser, Dataweb.NShape.Properties.Resources.CaptionTxt_RoleDescriptionSuperUser);
			AddRole(roleNameDesigner, Dataweb.NShape.Properties.Resources.CaptionTxt_RoleDescriptionDesigner);
			AddRole(roleNameOperator, Dataweb.NShape.Properties.Resources.CaptionTxt_RoleDescriptionOperator);
			AddRole(roleNameGuest, Dataweb.NShape.Properties.Resources.CaptionTxt_RoleDescriptionGuest);
			_currentRole = StandardRole.Administrator;
			_currentUserRole = GetRole(GetRoleName(_currentRole), true);

			// Set General permissions
			AddPermissions(roleNameAdministrator, Permission.All);
			AddPermissions(roleNameSuperUser, Permission.Designs | Permission.Templates);
			AddPermissions(roleNameDesigner, Permission.Designs);
			AddPermissions(roleNameOperator, Permission.None);
			AddPermissions(roleNameGuest, Permission.None);

			// Set permissions for domain A
			domain = 'A';
			AddDomain(domain, Dataweb.NShape.Properties.Resources.CaptionTxt_DomainDescriptionA);
			AddPermissions(domain, roleNameAdministrator, Permission.All);
			AddPermissions(domain, roleNameSuperUser, 
				Permission.Connect
				| Permission.Delete
				| Permission.Insert
				| Permission.Layout
				| Permission.Data
				| Permission.Present);
			AddPermissions(domain, roleNameDesigner, 
				Permission.Layout
				| Permission.Data
				| Permission.Present);
			AddPermissions(domain, roleNameOperator, Permission.Layout | Permission.Data);
			AddPermissions(domain, roleNameGuest, Permission.None);

			// Set permissions for domain B
			domain = 'B';
			AddDomain(domain, Dataweb.NShape.Properties.Resources.CaptionTxt_DomainDescriptionB);
			AddPermissions(domain, roleNameAdministrator, Permission.All);
			AddPermissions(domain, roleNameSuperUser, 
				Permission.Connect 
				| Permission.Delete 
				| Permission.Insert 
				| Permission.Layout 
				| Permission.Data 
				| Permission.Present);
			AddPermissions(domain, roleNameDesigner,
				Permission.Connect
				| Permission.Delete
				| Permission.Insert
				| Permission.Layout
				| Permission.Data
				| Permission.Present);
			AddPermissions(domain, roleNameOperator, Permission.Layout | Permission.Data);
			AddPermissions(domain, roleNameGuest, Permission.None);
			//
		}


		/// <summary>
		/// Defines the role of the current user.
		/// </summary>
		public string CurrentRoleName {
			get { return _currentUserRole.name; }
			set {
				switch (value) {
					case roleNameAdministrator:
						_currentRole = StandardRole.Administrator;
						break;
					case roleNameSuperUser:
						_currentRole = StandardRole.SuperUser;
						break;
					case roleNameDesigner:
						_currentRole = StandardRole.Designer;
						break;
					case roleNameOperator:
						_currentRole = StandardRole.Operator;
						break;
					case roleNameGuest:
						_currentRole = StandardRole.Guest;
						break;
					default:
						_currentRole = StandardRole.Custom;
						break;
				}
				_currentUserRole = GetRole(value, true);
			}
		}


		/// <summary>
		/// Defines the role of the current user.
		/// </summary>
		public StandardRole CurrentRole {
			get { return _currentRole; }
			set {
				switch (value) {
					case StandardRole.Administrator:
						_currentUserRole = GetRole(roleNameAdministrator, true);
						break;
					case StandardRole.SuperUser:
						_currentUserRole = GetRole(roleNameSuperUser, true);
						break;
					case StandardRole.Designer:
						_currentUserRole = GetRole(roleNameDesigner, true);
						break;
					case StandardRole.Operator:
						_currentUserRole = GetRole(roleNameOperator, true);
						break;
					case StandardRole.Guest:
						_currentUserRole = GetRole(roleNameGuest, true);
						break;
					case StandardRole.Custom:
						throw new ArgumentException(Dataweb.NShape.Properties.Resources.MessageTxt_CustomRolesAreSetBySettingTheCurrentRoleNameProperty);
				}
				_currentRole = value;
			}
		}


		#region ISecurityManager Members

		/// <override></override>
		public bool IsGranted(Permission permission) {
		    return _currentUserRole.IsGranted(permission, SecurityAccess.Modify);
		}


		/// <override></override>
		public bool IsGranted(Permission permission, SecurityAccess access) {
			return _currentUserRole.IsGranted(permission, access);
		}


		/// <override></override>
		public bool IsGranted(Permission permission, char domainName) {
		    return _currentUserRole.IsGranted(permission, domainName);
		}


		/// <override></override>
		public bool IsGranted(Permission permission, SecurityAccess access, char domainName) {
			return _currentUserRole.IsGranted(permission, access, domainName);
		}


		/// <override></override>
		public bool IsGranted(Permission permission, ISecurityDomainObject shape) {
			if (shape == null) throw new ArgumentNullException("shape");
			return IsGranted(permission, shape.SecurityDomainName);
		}


		/// <override></override>
		public bool IsGranted(Permission permission, SecurityAccess access, ISecurityDomainObject securityDomainObject) {
			if (securityDomainObject == null) throw new ArgumentNullException("shape");
			return IsGranted(permission, access, securityDomainObject.SecurityDomainName);
		}


		/// <override></override>
		public bool IsGranted<TSecurityDomainObject>(Permission permission, IEnumerable<TSecurityDomainObject> securityDomainObjects)
			where TSecurityDomainObject : ISecurityDomainObject {
			return IsGranted<TSecurityDomainObject>(permission, SecurityAccess.Modify, securityDomainObjects);
		}


		/// <override></override>
		public bool IsGranted<TSecurityDomainObject>(Permission permission, SecurityAccess access, IEnumerable<TSecurityDomainObject> securityDomainObjects)
			where TSecurityDomainObject : ISecurityDomainObject {
			bool result = true;
			bool collectionIsEmpty = true;
			foreach (TSecurityDomainObject s in securityDomainObjects) {
				if (collectionIsEmpty) collectionIsEmpty = false;
				if (!IsGranted(permission, s)) {
					result = false;
					break;
				}
			}
			if (collectionIsEmpty) 
				result = CouldBeGranted(permission, access);
			return result;
		}

		#endregion


		/// <summary>
		/// Adds a domain to the security.
		/// </summary>
		public void AddDomain(char name, string description) {
			if (!IsValidDomainName(name)) throw new ArgumentException(Dataweb.NShape.Properties.Resources.MessageTxt_ThisIsNotAnAllowedDomainName);
			if (description == null) throw new ArgumentNullException("description");
			if (domains[name - 'A'] != null) throw new ArgumentException(Dataweb.NShape.Properties.Resources.MessageTxt_ADomainWithThisNameExistsAlready);
			domains[name - 'A'] = description;
		}


		/// <summary>
		/// Removes a domain from the security.
		/// </summary>
		public void RemoveDomain(char name) {
			if (!IsValidDomainName(name)) throw new ArgumentException(Dataweb.NShape.Properties.Resources.MessageTxt_ThisIsNotAnAllowedDomainName);
			if (domains[name - 'A'] == null) throw new ArgumentException(Dataweb.NShape.Properties.Resources.MessageTxt_ADomainWithThisNameDoesNotExist);
			domains[name - 'A'] = null;
		}


		/// <summary>
		/// Adds a role to the security.
		/// </summary>
		public void AddRole(string name, string description) {
			if (name == null) throw new ArgumentNullException("name");
			roles.Add(name.ToLowerInvariant(), new UserRole(name, description));
		}


		/// <summary>
		/// Adds a new security role by copying an existing one.
		/// </summary>
		public void AddRole(string name, string description, string sourceRoleName) {
			if (name == null) throw new ArgumentNullException("name");
			roles.Add(name.ToLowerInvariant(), GetRole(sourceRoleName, true).Clone());
		}


		/// <summary>
		/// Adds permissions for the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void AddPermissions(StandardRole role, Permission permissions) {
			AddPermissions(role, permissions, SecurityAccess.Modify);
		}


		/// <summary>
		/// Adds permissions for the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void AddPermissions(StandardRole role, Permission permissions, SecurityAccess access) {
			string roleName = GetRoleName(role);
			AddPermissions(roleName, permissions, access);
		}


		/// <summary>
		/// Adds permissions for the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void AddPermissions(string roleName, Permission permissions) {
			AddPermissions(roleName, permissions, SecurityAccess.Modify);
		}


		/// <summary>
		/// Adds permissions for the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void AddPermissions(string roleName, Permission permissions, SecurityAccess access) {
			if (roleName == null) throw new ArgumentNullException("role");
			GetRole(roleName, true).AddPermissions(permissions, access);
		}


		/// <summary>
		/// Adds permissions for the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void AddPermissions(char domain, StandardRole role, Permission permissions) {
			AddPermissions(domain, role, permissions, SecurityAccess.Modify);
		}


		/// <summary>
		/// Adds permissions for the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void AddPermissions(char domain, StandardRole role, Permission permissions, SecurityAccess access) {
			string roleName = GetRoleName(role);
			AddPermissions(domain, roleName, permissions, access);
		}


		/// <summary>
		/// Adds permissions for the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void AddPermissions(char domain, string roleName, Permission permissions) {
			AddPermissions(domain, roleName, permissions, SecurityAccess.Modify);
		}


		/// <summary>
		/// Adds permissions for the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void AddPermissions(char domain, string roleName, Permission permissions, SecurityAccess access) {
			if (roleName == null) throw new ArgumentNullException("role");
			GetRole(roleName, true).AddPermissions(domain, permissions, access);
		}


		/// <summary>
		/// Redefines the permissions of the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void SetPermissions(StandardRole role, Permission permissions) {
			SetPermissions(role, permissions, SecurityAccess.Modify);
		}


		/// <summary>
		/// Redefines the permissions of the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void SetPermissions(StandardRole role, Permission permissions, SecurityAccess access) {
			string roleName = GetRoleName(role);
			SetPermissions(roleName, permissions, access);
		}


		/// <summary>
		/// Redefines the permissions of the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void SetPermissions(string roleName, Permission permissions) {
			SetPermissions(roleName, permissions, SecurityAccess.Modify);
		}


		/// <summary>
		/// Redefines the permissions of the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void SetPermissions(string roleName, Permission permissions, SecurityAccess access) {
			if (roleName == null) throw new ArgumentNullException("role");
			GetRole(roleName, true).SetPermissions(permissions, access);
		}


		/// <summary>
		/// Redefines the permissions of the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void SetPermissions(char domain, StandardRole role, Permission permissions) {
			SetPermissions(domain, role, permissions, SecurityAccess.Modify);
		}


		/// <summary>
		/// Redefines the permissions of the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void SetPermissions(char domain, StandardRole role, Permission permissions, SecurityAccess access) {
			string roleName = GetRoleName(role);
			SetPermissions(domain, roleName, permissions, access);
		}


		/// <summary>
		/// Redefines the permissions of the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void SetPermissions(char domain, string roleName, Permission permissions) {
			SetPermissions(domain, roleName, permissions, SecurityAccess.Modify);
		}


		/// <summary>
		/// Redefines the permissions of the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void SetPermissions(char domain, string roleName, Permission permissions, SecurityAccess access) {
			if (roleName == null) throw new ArgumentNullException("role");
			GetRole(roleName, true).SetPermissions(domain, permissions, access);
		}


		/// <summary>
		/// Removes permissions from the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void RemovePermissions(StandardRole role, Permission permissions) {
			RemovePermissions(role, permissions, SecurityAccess.View);
		}


		/// <summary>
		/// Removes permissions from the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void RemovePermissions(StandardRole role, Permission permissions, SecurityAccess access) {
			string roleName = GetRoleName(role);
			RemovePermissions(roleName, permissions, access);
		}


		/// <summary>
		/// Removes permissions from the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void RemovePermissions(string roleName, Permission permissions) {
			RemovePermissions(roleName, permissions, SecurityAccess.View);
		}


		/// <summary>
		/// Removes permissions from the given role.
		/// </summary>
		/// <remarks>Security domain independent permissions are checked against the role permissions.</remarks>
		public void RemovePermissions(string roleName, Permission permissions, SecurityAccess access) {
			if (roleName == null) throw new ArgumentNullException("role");
			GetRole(roleName, true).RemovePermissions(permissions, access);
		}


		/// <summary>
		/// Removes permissions from the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void RemovePermissions(char domain, StandardRole role, Permission permissions) {
			RemovePermissions(domain, role, permissions, SecurityAccess.View);
		}


		/// <summary>
		/// Removes permissions from the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void RemovePermissions(char domain, StandardRole role, Permission permissions, SecurityAccess access) {
			string roleName = GetRoleName(role);
			RemovePermissions(domain, roleName, permissions, access);
		}


		/// <summary>
		/// Removes permissions from the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void RemovePermissions(char domain, string roleName, Permission permissions) {
			RemovePermissions(domain, roleName, permissions, SecurityAccess.View);
		}


		/// <summary>
		/// Removes permissions from the given domain and role.
		/// </summary>
		/// <remarks>Security domain dependent permissions are checked against the role's domain permissions.</remarks>
		public void RemovePermissions(char domain, string roleName, Permission permissions, SecurityAccess access) {
			if (roleName == null) throw new ArgumentNullException("role");
			GetRole(roleName, true).RemovePermissions(domain, permissions, access);
		}


		/// <summary>
		/// Returns the name of the given role.
		/// </summary>
		public string GetRoleName(StandardRole role) {
			switch (role) {
				case StandardRole.Administrator: return roleNameAdministrator;
				case StandardRole.Designer: return roleNameDesigner;
				case StandardRole.Guest: return roleNameGuest;
				case StandardRole.Operator: return roleNameOperator;
				case StandardRole.SuperUser: return roleNameSuperUser;
				default: 
					throw new InvalidOperationException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_0IsNotAValidRoleForThisOperation, role));
			}
		}


		private class UserRole {

			public static readonly Permission GeneralPermissions;
			public static readonly Permission ShapePermissions;


			public UserRole(string name, string description) {
				this.name = name;
				this.description = description;
			}


			public UserRole Clone() {
				UserRole result = new UserRole(name, description);
				result.generalPermissionSetModify = generalPermissionSetModify;
				result.generalPermissionSetView = generalPermissionSetView;
				domainPermissionSetsModify.CopyTo(result.domainPermissionSetsModify, 0);
				domainPermissionSetsView.CopyTo(result.domainPermissionSetsView, 0);
				return result;
			}


			public void AddPermissions(Permission permissions) {
				AddPermissions(permissions, SecurityAccess.Modify);
			}


			public void AddPermissions(Permission permissions, SecurityAccess access) {
				AssertGeneralPermissionSet(permissions);
				if (access == SecurityAccess.Modify)
					generalPermissionSetModify |= permissions;
				generalPermissionSetView |= permissions;
			}


			public void AddPermissions(char domain, Permission permissions) {
				AddPermissions(domain, permissions, SecurityAccess.Modify);
			}


			public void AddPermissions(char domain, Permission permissions, SecurityAccess access) {
				AssertValidDomainQualifier(domain);
				AssertDomainPermissionSet(permissions);
				if (access == SecurityAccess.Modify)
					domainPermissionSetsModify[domain - 'A'] |= permissions;
				domainPermissionSetsView[domain - 'A'] |= permissions;
			}


			public void SetPermissions(Permission permissions) {
				SetPermissions(permissions, SecurityAccess.Modify);
			}


			public void SetPermissions(Permission permissions, SecurityAccess access) {
				AssertGeneralPermissionSet(permissions);
				if (access == SecurityAccess.Modify) 
					generalPermissionSetModify = permissions;
				generalPermissionSetView = permissions;
			}


			public void SetPermissions(char domain, Permission permissions) {
				SetPermissions(domain, permissions, SecurityAccess.Modify);
			}


			public void SetPermissions(char domain, Permission permissions, SecurityAccess access) {
				AssertValidDomainQualifier(domain);
				AssertDomainPermissionSet(permissions);
				domainPermissionSetsModify[domain - 'A'] = (access == SecurityAccess.Modify) ? permissions : Permission.None;
				domainPermissionSetsView[domain - 'A'] = permissions;
			}


			public void RemovePermissions(Permission permissions) {
				RemovePermissions(permissions, SecurityAccess.View);
			}


			public void RemovePermissions(Permission permissions, SecurityAccess access) {
				AssertGeneralPermissionSet(permissions);
				if (access == SecurityAccess.View)
					generalPermissionSetView &= ~permissions;
				generalPermissionSetModify &= ~permissions;
			}


			public void RemovePermissions(char domain, Permission permissions) {
				RemovePermissions(domain, permissions, SecurityAccess.View);
			}


			public void RemovePermissions(char domain, Permission permissions, SecurityAccess access) {
				AssertValidDomainQualifier(domain);
				AssertDomainPermissionSet(permissions);
				if (access == SecurityAccess.View)
					domainPermissionSetsView[domain - 'A'] &= ~permissions;
				domainPermissionSetsModify[domain - 'A'] &= ~permissions;
			}


			public bool IsGranted(Permission permissions) {
				return IsGranted(permissions, SecurityAccess.Modify);
			}


			public bool IsGranted(Permission permissions, SecurityAccess access) {
				AssertGeneralPermissionSet(permissions);
				Permission userPermissions = permissions & GeneralPermissions;
				if (access == SecurityAccess.Modify)
					return ((generalPermissionSetModify & userPermissions) == userPermissions);
				else return ((generalPermissionSetView & userPermissions) == userPermissions);
			}


			public bool IsGranted(Permission permissions, char domain) {
				return IsGranted(permissions, SecurityAccess.Modify, domain);
			}


			public bool IsGranted(Permission permissions, SecurityAccess access, char domain) {
				AssertValidDomainQualifier(domain);
				Permission userPermissions = permissions & GeneralPermissions;
				Permission shapePermissions = permissions & ShapePermissions;
				if (access == SecurityAccess.Modify) {
					return ((generalPermissionSetModify & userPermissions) == userPermissions
						&& (domainPermissionSetsModify[domain - 'A'] & shapePermissions) == shapePermissions);
				} else {
					return ((generalPermissionSetView & userPermissions) == userPermissions
						&& (domainPermissionSetsView[domain - 'A'] & shapePermissions) == shapePermissions);
				}
			}


#if DEBUG
			public override String ToString()
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.AppendFormat("User role '{0}'", name);
				sb.AppendFormat("Description: '{0}'", description);
				sb.AppendFormat("General View Permissions: {0}, {1}", generalPermissionSetView, Environment.NewLine);
				sb.AppendFormat("General Modify Permissions: {0}, {1}", generalPermissionSetModify, Environment.NewLine);
				sb.AppendFormat("Shape View Permissions: ", Environment.NewLine);
				for (Int32 i = 0; i < 26; ++i) {
					if (domainPermissionSetsView[i] == Permission.None) continue;
					sb.AppendFormat("{0}: {1}, ", Convert.ToChar(i + 'A'), domainPermissionSetsView[i], Environment.NewLine);
				}
				sb.Append(Environment.NewLine);
				sb.AppendFormat("Shape Modify Permissions: ");
				for (Int32 i = 0; i < 26; ++i) {
					if (domainPermissionSetsModify[i] == Permission.None) continue;
					sb.AppendFormat("{0}: {1}, ", Convert.ToChar(i + 'A'), domainPermissionSetsModify[i], Environment.NewLine);
				}
				return sb.ToString();
			}
#endif


			private void AssertValidDomainQualifier(Char domain) {
				if (domain < 'A' || domain > 'Z')
					throw new ArgumentOutOfRangeException("domain", Dataweb.NShape.Properties.Resources.MessageTxt_TheDomainQualifierHasToBeAnUpperCaseANSILetterAZ);
			}


			private void AssertGeneralPermissionSet(Permission permissions) {
				if (permissions != Permission.All && (permissions & ShapePermissions) != 0) 
					throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_0IsNotADomainIndependentPermissionSet, (permissions & ShapePermissions)));
			}


			private void AssertDomainPermissionSet(Permission permissions) {
				if (permissions != Permission.All && (permissions & GeneralPermissions) != 0)
					throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_0IsNotADomainDependentPermissionSet, (permissions & GeneralPermissions)));
			}


			static UserRole() {
				GeneralPermissions = Permission.Designs | Permission.Security | Permission.Templates;
				ShapePermissions = Permission.Connect | Permission.Delete | Permission.Insert | Permission.Layout | Permission.Data | Permission.Present;
			}


			// Domain that holds the shape-independent permissions of the user
			public string name;
			public string description;
			public Permission generalPermissionSetModify = Permission.None;
			public Permission generalPermissionSetView = Permission.None;
			public Permission[] domainPermissionSetsModify = new Permission[26];
			public Permission[] domainPermissionSetsView = new Permission[26];
		}


		private UserRole GetRole(string name, bool throwIfNotFound) {
			UserRole result;
			if (roles.TryGetValue(name.ToLowerInvariant(), out result))
				return result;
			else {
				if (throwIfNotFound)
					throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Role0DoesNotExist, name));
				return null;
			}
		}


		private bool IsValidDomainName(char name) {
			return name >= 'A' && name <= 'Z';
		}


		private bool CouldBeGranted(Permission permission) {
			for (char dom = 'A'; dom <= 'Z'; ++dom)
				if (IsGranted(permission, dom))
					return true;
			return false;
		}


		private bool CouldBeGranted(Permission permission, SecurityAccess access) {
			for (char dom = 'A'; dom <= 'Z'; ++dom)
				if (IsGranted(permission, access, dom))
					return true;
			return false;
		}


		// Contains the descriptions for the domains. If description is null, the domain
		// is not allowed.
		private string[] domains = new string[26];

		// List of known roles.
		private SortedList<String, UserRole> roles = new SortedList<String, UserRole>();

		// Reference of current Role.
		private UserRole _currentUserRole;
		private StandardRole _currentRole;

		private const string roleNameAdministrator = "Administrator";
		private const string roleNameSuperUser = "Super User";
		private const string roleNameDesigner = "Designer";
		private const string roleNameOperator = "Operator";
		private const string roleNameGuest = "Guest";
	}

}

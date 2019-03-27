/******************************************************************************
  Copyright 2009-2018 dataweb GmbH
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Resources;

namespace Dataweb.NShape.Advanced {

	/// <summary>
	/// Simulates a string coming from a resource.
	/// </summary>
	/// <remarks>
	/// Later versions will hold a reference to a ResourceManager and read the string from there.
	/// </remarks>
	public class ResourceString {

		/// <ToBeCompleted></ToBeCompleted>
		static public implicit operator ResourceString(string s) {
			return new ResourceString(s);
		}


		/// <ToBeCompleted></ToBeCompleted>
		public ResourceString(string s) {
			_value = s;
		}


		/// <ToBeCompleted></ToBeCompleted>
		public string Value {
			get { return _value; }
		}


		private string _value;

	}


	internal class ResourceManagerCache {

		public const string ResourcesExtension = ".Resources";


		public ResourceManagerCache() {
		}


		internal static ResourceManager GetResourceManager(Type resourceType) {
			return GetResourceManager(resourceType, string.Empty);
		}


		internal static ResourceManager GetResourceManager(Type resourceType, string resourceName) {
			if (ResourceManagerCache._instance == null)
				ResourceManagerCache._instance = new ResourceManagerCache();
			return ResourceManagerCache._instance.InternalGetResourceManager(resourceType, resourceName);
		}


		private ResourceManager InternalGetResourceManager(Type resourceType, string resourceName) {
			ResourceManager resourceManager = null;
			System.Reflection.Assembly assembly = resourceType.Assembly;
			if (string.IsNullOrEmpty(resourceName)) {
				string[] manifestResourceNames = assembly.GetManifestResourceNames();
				for (int i = 0; i < manifestResourceNames.Length; ++i) {
					string resName = manifestResourceNames[i];
					if (resName.EndsWith(ResourcesExtension, StringComparison.OrdinalIgnoreCase))
						resName = resName.Substring(0, resName.Length - ResourcesExtension.Length);
					if (resName == resourceType.Name || resName == resourceType.FullName) {
						resourceName = resName;
						break;
					}
				}
			}
			if (!string.IsNullOrEmpty(resourceName)) {
				string resourceKey = string.Concat(assembly.FullName, " | ", resourceName);
				if (!_resourceManagers.TryGetValue(resourceKey, out resourceManager) || resourceManager == null) {
					resourceManager = new ResourceManager(resourceName, assembly);
					_resourceManagers[resourceKey] = resourceManager;
				}
			}
			return resourceManager;
		}


		private static ResourceManagerCache _instance;
		private Dictionary<string, ResourceManager> _resourceManagers = new Dictionary<string, ResourceManager>();
	}


	/// <summary>
	/// Class for loading resource strings
	/// </summary>
	public class StringResourceLoader {
		internal StringResourceLoader(string resourceKey)
			: this(resourceKey, typeof(Properties.Resources)) {
		}


		/// <summary>
		/// Creates a new StringResourceLoader instance for a specific resource key which can be obtained.
		/// </summary>
		public StringResourceLoader(string resourceKey, Type resourceManagerProvider) {
			this._resourceKey = resourceKey;
			this._resourceManagerProvider = resourceManagerProvider;
		}


		/// <summary>
		/// Loads and returns the string for the resource key this instance was constructed for.
		/// </summary>
		public string GetString() {
			string resourceName = _resourceManagerProvider.FullName.EndsWith(ResourceManagerCache.ResourcesExtension) ? _resourceManagerProvider.FullName : string.Empty;
			ResourceManager resourceManager = ResourceManagerCache.GetResourceManager(_resourceManagerProvider, resourceName);
			// Return an empty string if the resource was not found as this will trigger a fallback to the default resource
			return resourceManager.GetString(_resourceKey) ?? string.Empty;
		}


		private readonly string _resourceKey;
		private readonly Type _resourceManagerProvider;
	}


	#region Localized Attributes

	/// <summary>
	/// A localizable DisplayNameAttribute that works with resource strings.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
	public class LocalizedDisplayNameAttribute : DisplayNameAttribute {

		/// <summary>
		/// Contructs a LocalizedDisplayNameAttribute instance.
		/// </summary>
		internal LocalizedDisplayNameAttribute(string resourceName)
			: this(resourceName, typeof(Properties.Resources)) {
		}


		/// <summary>
		/// Contructs a LocalizedDisplayNameAttribute instance.
		/// </summary>
		public LocalizedDisplayNameAttribute(string resourceName, Type resourceManagerProvider)
			: base() {
			_resourceLoader = new StringResourceLoader(resourceName, resourceManagerProvider);
		}

		/// <override></override>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public override string DisplayName {
			get {
				if (string.IsNullOrEmpty(DisplayNameValue))
					DisplayNameValue = _resourceLoader.GetString();
				return base.DisplayName;
			}
		}

		private readonly StringResourceLoader _resourceLoader = null;
	}


	/// <summary>
	/// A localizable DescriptionAttribute that works with resource strings.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false)]
	public class LocalizedDescriptionAttribute : DescriptionAttribute {

		/// <summary>
		/// Contructs a LocalizedDisplayNameAttribute instance.
		/// </summary>
		internal LocalizedDescriptionAttribute(string resourceName)
			: this(resourceName, typeof(Properties.Resources)) {
		}


		/// <summary>
		/// Contructs a LocalizedDisplayNameAttribute instance.
		/// </summary>
		public LocalizedDescriptionAttribute(string resourceName, Type resourceManagerProvider)
			: base() {
			_resourceLoader = new StringResourceLoader(resourceName, resourceManagerProvider);
		}

		/// <override></override>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public override string Description {
			get {
				if (string.IsNullOrEmpty(DescriptionValue))
					DescriptionValue = _resourceLoader.GetString();
				return base.Description;
			}
		}

		private readonly StringResourceLoader _resourceLoader = null;
	}

	#endregion

}
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


namespace Dataweb.NShape.Advanced
{

	#region Category Attributes

	/// <summary>
	/// Specifies the name of the category in which to group the property or event when displayed 
	/// in a System.Windows.Forms.PropertyGrid control set to Categorized mode.
	/// </summary>
	public class CategoryNShape : CategoryAttribute
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.ComponentModel.CategoryAttribute" /> class using the category name 'NShape'.
		/// </summary>
		public CategoryNShape()
			: base(categoryName) {
		}


		private static readonly string categoryName = Dataweb.NShape.Properties.Resources.Text_CategoryNShape;

	}


	/// <summary>
	/// Specifies the name of the category in which to group the property or event when displayed 
	/// in a System.Windows.Forms.PropertyGrid control set to Categorized mode.
	/// </summary>
	public class CategoryGeneral : CategoryAttribute
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.ComponentModel.CategoryAttribute" /> class using the category name 'General'.
		/// </summary>
		public CategoryGeneral()
			: base(categoryName) {
		}


		/// <override></override>
		protected override string GetLocalizedString(string value) {
			return Properties.Resources.Text_CategoryGeneral;
		}


		private static readonly string categoryName = Dataweb.NShape.Properties.Resources.Text_CategoryGeneral;

	}


	/// <summary>
	/// Specifies the name of the category in which to group the property or event when displayed 
	/// in a System.Windows.Forms.PropertyGrid control set to Categorized mode.
	/// </summary>
	public class CategoryAppearance : CategoryAttribute {

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.ComponentModel.CategoryAttribute" /> class using the category name 'Appearance'.
		/// </summary>
		public CategoryAppearance()
			: base(CategoryAttribute.Appearance.Category) {
		}

	}


	/// <summary>
	/// Specifies the name of the category in which to group the property or event when displayed 
	/// in a System.Windows.Forms.PropertyGrid control set to Categorized mode.
	/// </summary>
	public class CategoryBehavior : CategoryAttribute
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.ComponentModel.CategoryAttribute" /> class using the category name 'Behavior'.
		/// </summary>
		public CategoryBehavior()
			: base(CategoryAttribute.Behavior.Category) {
		}

	}


	/// <summary>
	/// Specifies the name of the category in which to group the property or event when displayed 
	/// in a System.Windows.Forms.PropertyGrid control set to Categorized mode.
	/// </summary>
	public class CategoryLayout : CategoryAttribute {

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.ComponentModel.CategoryAttribute" /> class using the category name 'Layout'.
		/// </summary>
		public CategoryLayout()
			: base(CategoryAttribute.Layout.Category) {
		}

	}


	/// <summary>
	/// Specifies the name of the category in which to group the property or event when displayed 
	/// in a System.Windows.Forms.PropertyGrid control set to Categorized mode.
	/// </summary>
	public class CategoryData : CategoryAttribute
	{

		/// <summary>
		/// Initializes a new instance of the <see cref="T:System.ComponentModel.CategoryAttribute" /> class using the category name 'Data'.
		/// </summary>
		public CategoryData()
			: base(CategoryAttribute.Data.Category) {
		}

	}

	#endregion


	/// <summary>
	/// Provides services to shapes
	/// </summary>
	public interface IDisplayService {

		/// <summary>
		/// Invalidate the given rectangle.
		/// </summary>
		/// <param name="x">Left side of the rectangle</param>
		/// <param name="y">Top side of the rectangle</param>
		/// <param name="width">Width of the rectangle</param>
		/// <param name="height">Height of the rectangle</param>
		void Invalidate(int x, int y, int width, int height);

		/// <summary>
		/// Invalidate the given rectangle.
		/// </summary>
		/// <param name="rectangle">Rectangle to invalidate.</param>
		void Invalidate(Rectangle rectangle);

		/// <summary>
		/// Update layout according to the changed bounds.
		/// </summary>
		[Obsolete("Use the Diagram class' Resized event for getting notified about changed bounds.")]
		void NotifyBoundsChanged();

		/// <summary>
		/// Info graphics for mearusing text, etc. Do not dispose!
		/// </summary>
		Graphics InfoGraphics { get; }

		/// <summary>
		/// Fill style for drawing hints.
		/// </summary>
		IFillStyle HintBackgroundStyle { get; }

		/// <summary>
		/// Line style for drawing hints.
		/// </summary>
		ILineStyle HintForegroundStyle { get; }

	}


	/// <summary>
	/// Represents a place, where shapes and model object typea are registered.
	/// </summary>
	/// <status>reviewed</status>
	public interface IRegistrar {

		/// <summary>
		/// Registers a library for shape or model objects.
		/// </summary>
		/// <param name="name">The name of the library.</param>
		/// <param name="preferredRepositoryVersion">
		/// Defines the preferred repository version for shapes. 
		/// Will be used when shapes from this library are stored for the first time.
		/// </param>
		void RegisterLibrary(string name, int preferredRepositoryVersion);

		/// <summary>
		/// Registers a shape type implemented in the library.
		/// </summary>
		void RegisterShapeType(ShapeType shapeType);

		/// <summary>
		/// Registers a model object type implemented in the library.
		/// </summary>
		void RegisterModelObjectType(ModelObjectType modelObjectType);


		/// <summary>
		/// Registers a model object type implemented in the library.
		/// </summary>
		void RegisterDiagramModelObjectType(DiagramModelObjectType diagramModelObjectType);

	}


	/// <summary>
	/// Represents the state of a disposable NShape object.
	/// </summary>
	public enum ObjectState : byte {
		/// <summary>The object is initialized and usable.</summary>
		Normal = 0,
		/// <summary>The object has started (but not yet finished) disposing and is not usable any more.</summary>
		Disposing = 1,
		/// <summary>The object has already been disposed and is not usable any more.</summary>
		Disposed = 2
	}


	/// <summary>
	/// Encapsulates the configuration on project level.
	/// </summary>
	public class ProjectSettings : IEntity {

		/// <summary>
		/// Constructs a projects projectData instance.
		/// </summary>
		public ProjectSettings() {
		}


		/// <summary>
		/// Empties the projectData.
		/// </summary>
		public void Clear() {
			this._id = null;
			this._lastSaved = DateTime.MinValue;
			this._libraries.Clear();
		}


		/// <summary>
		/// Copies all properties from the given projectData.
		/// </summary>
		/// <param name="source"></param>
		public void CopyFrom(ProjectSettings source) {
			if (source == null) throw new ArgumentNullException("source");
			_id = ((IEntity)source).Id;
			_lastSaved = source.LastSaved;
			foreach (LibraryData library in source._libraries) {
				if (!_libraries.Contains(library))
					_libraries.Add(library);
			}
		}


		/// <summary>
		/// Defines the date of the last saving of the project.
		/// </summary>
		public DateTime LastSaved
		{
			get { return _lastSaved; }
			set { _lastSaved = value; }
		}


		/// <summary>
		/// Stores a desciptive text.
		/// </summary>
		public string Description
		{
			get { return _description; }
			set { _description = value; }
		}


		/// <summary>
		/// Adds a dynamic library to the project.
		/// </summary>
		public void AddLibrary(string name, string assemblyName, int libraryVersion) {
			if (name == null) throw new ArgumentNullException("name");
			if (assemblyName == null) throw new ArgumentNullException("assemblyName");
			_libraries.Add(new LibraryData(name, assemblyName, libraryVersion));
		}


		/// <summary>
		/// Retrieves the repository version of the given library.
		/// </summary>
		public int GetRepositoryVersion(string libraryName) {
			if (libraryName == null) throw new ArgumentNullException("libraryName");
			LibraryData ld = FindLibraryData(libraryName, true);
			return ld.RepositoryVersion;
		}


		/// <summary>
		/// Sets the repository version used for loading/saving the library's shapes/model objects.
		/// </summary>
		public void SetRepositoryVersion(string libraryName, int version) {
			// The core library will not be enlisted in the project settings
			if (string.Compare(libraryName, Project.CoreLibraryName) == 0)
				return;
			LibraryData ld = FindLibraryData(libraryName, true);
			ld.RepositoryVersion = version;
		}


		/// <summary>
		/// Indicates the library assemblies required for the project.
		/// </summary>
		public IEnumerable<string> AssemblyNames
		{
			get
			{
				foreach (LibraryData ld in _libraries)
					yield return ld.AssemblyName;
			}
		}


		#region IEntity Members

		/// <summary>
		/// Receives the entity type name.
		/// </summary>
		public static string EntityTypeName
		{
			get { return entityTypeName; }
		}


		/// <summary>
		/// Retrieves the persistable properties of <see cref="T:Dataweb.NShape.Advanced.ProjectSettings" />.
		/// </summary>
		public static IEnumerable<EntityPropertyDefinition> GetPropertyDefinitions(int version) {
			yield return new EntityFieldDefinition("LastSavedUtc", typeof(DateTime));
			if (version >= 5) yield return new EntityFieldDefinition("Description", typeof(string));
			yield return new EntityInnerObjectsDefinition("Libraries", "Core.Library", librariesAttrNames, librariesAttrTypes);
		}


		object IEntity.Id
		{
			get { return _id; }
		}


		void IEntity.AssignId(object id) {
			if (id == null) throw new ArgumentNullException("id");
			if (this._id != null)
				throw new InvalidOperationException("Project settings have already an id.");
			this._id = id;
		}


		void IEntity.SaveFields(IRepositoryWriter writer, int version) {
			writer.WriteDate(DateTime.Now);
			if (version >= 5) writer.WriteString(_description);
		}


		void IEntity.LoadFields(IRepositoryReader reader, int version) {
			_lastSaved = reader.ReadDate();
			if (version >= 5) _description = reader.ReadString();
		}


		void IEntity.SaveInnerObjects(string propertyName, IRepositoryWriter writer, int version) {
			Project.AssertSupportedVersion(true, version);
			writer.BeginWriteInnerObjects();
			foreach (LibraryData ld in _libraries) {
				writer.BeginWriteInnerObject();
				writer.WriteString(ld.Name);
				writer.WriteString(ld.AssemblyName);
				writer.WriteInt32(ld.RepositoryVersion);
				writer.EndWriteInnerObject();
			}
			writer.EndWriteInnerObjects();
		}


		void IEntity.LoadInnerObjects(string propertyName, IRepositoryReader reader, int version) {
			Project.AssertSupportedVersion(false, version);
			reader.BeginReadInnerObjects();
			while (reader.BeginReadInnerObject()) {
				LibraryData ld = new LibraryData(null, null, 0);
				ld.Name = reader.ReadString();
				ld.AssemblyName = reader.ReadString();
				ld.RepositoryVersion = reader.ReadInt32();
				_libraries.Add(ld);
				reader.EndReadInnerObject();
			}
			reader.EndReadInnerObjects();
		}


		void IEntity.Delete(IRepositoryWriter writer, int version) {
			foreach (EntityPropertyDefinition pi in GetPropertyDefinitions(version)) {
				if (pi is EntityInnerObjectsDefinition) {
					writer.DeleteInnerObjects();
				}
			}
		}

		#endregion


		private class LibraryData {

			public LibraryData(string name, string assemblyName, int repositoryVersion) {
				Name = name;
				AssemblyName = assemblyName;
				RepositoryVersion = repositoryVersion;
			}

			// Specifies the name of the library
			public string Name;
			// Specifies the full assembly name including version and public key token.
			public string AssemblyName;
			// Specifies this library's repository version as used in the project.
			public int RepositoryVersion;
		}


		private LibraryData FindLibraryData(string libraryName, bool throwIfNotFound) {
			LibraryData result = null;
			foreach (LibraryData ld in _libraries)
				if (ld.Name.Equals(libraryName, StringComparison.InvariantCultureIgnoreCase)) {
					result = ld;
					break;
				}
			if (result == null && throwIfNotFound) throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Library0NotFound, libraryName));
			return result;
		}


		#region Fields

		private static readonly string entityTypeName = "Core.Project";
		private static readonly string[] librariesAttrNames = new string[] { "Name", "AssemblyName", "RepositoryVersion" };
		private static readonly Type[] librariesAttrTypes = new Type[] { typeof(string), typeof(string), typeof(int) };

		private object _id;
		private string _description;
		private DateTime _lastSaved;
		private List<LibraryData> _libraries = new List<LibraryData>();

		#endregion
	}

}

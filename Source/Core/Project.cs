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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Reflection;

using Dataweb.NShape.Advanced;
using Dataweb.NShape.Commands;
using System.Text;


namespace Dataweb.NShape {

	/// <summary>
	/// Collection of elements making up a NShape project.
	/// </summary>
	/// <status>reviewed</status>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(Project), "Project.bmp")]
	public sealed class Project : Component, IRegistrar, IStyleSetProvider {

		/// <summary>
		/// Checks whether a name is a valid identifier for NShape.
		/// </summary>
		public static bool IsValidName(string name) {
			if (name == null) return false;
			foreach (char c in name) {
				if (c >= 'a' && c <= 'z') continue;
				if (c >= 'A' && c <= 'Z') continue;
				if (c >= '0' && c <= '9') continue;
				if (c == '_') continue;
				return false;
			}
			return true;
		}


		/// <summary>
		/// Constructs a new project instance.
		/// </summary>
		public Project() {
			Construct();
		}


		/// <summary>
		/// Constructs a new project instance.
		/// </summary>
		/// <param name="container"></param>
		public Project(IContainer container) {
			container.Add(this);
			Construct();
		}


		#region Events

		/// <summary>
		/// Occurs when the the project was opened.
		/// </summary>
		public event EventHandler Opened;

		/// <summary>
		/// Occurs when the the project is going to be closed.
		/// </summary>
		public event EventHandler Closing;

		/// <summary>
		/// Occurs when the project was closed.
		/// </summary>
		public event EventHandler Closed;

		/// <summary>
		/// Occurs when a NShape library was loaded.
		/// </summary>
		public event EventHandler<LibraryLoadedEventArgs> LibraryLoaded;

		/// <summary>
		/// Occurs when styles were changed.
		/// </summary>
		public event EventHandler StylesChanged;

		#endregion


		#region [Public] Properties

		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		/// <summary>
		/// Specifies the name of the project.
		/// </summary>
		/// <remarks>The name is used as the repository name as well.</remarks>
		[CategoryNShape()]
		public string Name {
			get { return _name; }
			set {
				_name = value;
				if (_repository != null) _repository.ProjectName = value;
			}
		}


		/// <summary>
		/// Gets or sets a descritive text for the project.
		/// </summary>
		[CategoryNShape()]
		public string Description {
			get { return _settings.Description; }
			set { _settings.Description  = value; }
		}

		
		/// <summary>
		/// Specifies the directories, where NShape libraries are looked for.
		/// </summary>
		[CategoryNShape()]
		[Description("A collection of paths where shape and model library assemblies are expected to be found.")]
		[TypeConverter("Dataweb.NShape.WinFormsUI.TextTypeConverter, Dataweb.NShape.WinFormsUI")]
		[Editor("Dataweb.NShape.WinFormsUI.TextUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
		public IList<string> LibrarySearchPaths {
			get { return _librarySearchPaths; }
			set {
				if (value == null) _librarySearchPaths = new List<string>();
				else _librarySearchPaths = value;
			}
		}


		/// <summary>
		/// Specifies the repository used to store the project.
		/// </summary>
		[CategoryNShape()]
		[Description("Specifies the IRepository class used for loading/saving project and diagram data.")]
		public IRepository Repository {
			get { return _repository; }
			set {
				if (IsOpen) throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_ProjectIsStillOpen);
				if (_repository != null) {
					_repository.StyleUpdated -= repository_StyleUpdated;
					_repository.ProjectName = string.Empty;
				}
				_repository = value;
				if (_repository != null) {
					_repository.ProjectName = _name;
					_repository.StyleUpdated += repository_StyleUpdated;
					_repository.StyleDeleted += repository_StyleDeleted;
				}
			}
		}


		/// <summary>
		/// Specifies whether the project creates templates for each item in registered 
		/// shape and model libraries.
		/// </summary>
		[CategoryBehavior()]
		[DefaultValue(true)]
		public bool AutoGenerateTemplates {
			get { return _autoCreateTemplates; }
			set { _autoCreateTemplates = value; }
		}


		/// <summary>
		/// Specifies whether the project automatically loads missing libraries when opening a repository.
		/// </summary>
		[CategoryBehavior()]
		[DefaultValue(false)]
		public bool AutoLoadLibraries {
			get { return _autoLoadLibraries; }
			set { _autoLoadLibraries = value; }
		}


		/// <summary>
		/// Provides access to the registered diagra model object types.
		/// </summary>
		[Browsable(false)]
		public IReadOnlyDiagramModelObjectTypeCollection DiagramModelObjectTypes {
			get { return _diagramModelObjectTypes; }
		}


		/// <summary>
		/// Provides access to the registered model object types.
		/// </summary>
		[Browsable(false)]
		public IReadOnlyModelObjectTypeCollection ModelObjectTypes {
			get { return _modelObjectTypes; }
		}


		/// <summary>
		/// Provides access to the registered shape types.
		/// </summary>
		[Browsable(false)]
		public IReadOnlyShapeTypeCollection ShapeTypes {
			get { return _shapeTypes; }
		}


		/// <summary>
		/// Provides undo/redo capability for the project.
		/// </summary>
		[Browsable(false)]
		public History History {
			get { return _history; }
		}


		/// <summary>
		/// Specifies the security manager used with this project.
		/// </summary>
		[Browsable(false)]
		public ISecurityManager SecurityManager {
			get { return _security; }
			set {
				AssertClosed();
				if (value == null) throw new ArgumentNullException(nameof(SecurityManager));
				_security = value;
			}
		}


		/// <summary>
		/// Accesses the project settings.
		/// </summary>
		[Browsable(false)]
		public ProjectSettings Settings {
			get { return _settings; }
		}


		/// <summary>
		/// Accesses the styles used for the project.
		/// </summary>
		[Browsable(false)]
		public Design Design {
			get {
				AssertOpen();
				return _repository.GetDesign(null);
			}
		}


		/// <summary>
		/// Retrieves the registered libraries.
		/// </summary>
		[Browsable(false)]
		public IEnumerable<Assembly> Libraries {
			get { foreach (Library l in _libraries) yield return l.Assembly; }
		}

		#endregion


		#region [Public] Methods

		/// <summary>
		/// Uses the given design for the project.
		/// </summary>
		/// <param name="newDesign"></param>
		public void ApplyDesign(Design newDesign) {
			if (newDesign == null) throw new ArgumentNullException(nameof(newDesign));
			Design design = _repository.GetDesign(null);
			bool styleFound = false;
			foreach (IStyle style in newDesign.Styles) {
				styleFound = design.AssignStyle(style);
				IStyle s = design.FindStyleByName(style.Name, style.GetType());
				if (styleFound) _repository.Update(s);
				else _repository.Insert(design, s);
			}
			_repository.Update(design);
			_repository.Update();

			if (StylesChanged != null) StylesChanged(this, EventArgs.Empty);
		}


		/// <summary>
		/// Uses the given design for the project.
		/// </summary>
		public void ApplyDesign(string designName) {
			if (designName == null) throw new ArgumentNullException(nameof(designName));
			Design design = null;
			foreach (Design d in _repository.GetDesigns()) {
				if (designName.Equals(d.Name, StringComparison.InvariantCultureIgnoreCase)) {
					design = d;
					break;
				}
			}
			if (design == null)
				throw new NShapeException(Properties.Resources.MessageFmt_Design0DoesNotExist, designName);
			ApplyDesign(design);
		}


		/// <summary>
		/// Checks whether the specified <see cref="T:System.Reflection.Assembly" /> is an NShape library.
		/// </summary>
		/// <remarks>
		/// NShape libraries must implement a static class named <see cref="T:NShapeLibraryInitializer" />. 
		/// See documentation for details.
		/// </remarks>
		public bool IsValidLibrary(string assemblyPath) {
			string msg;
			return IsValidLibrary(assemblyPath, out msg);
		}


		/// <summary>
		/// Checks whether the specified <see cref="T:System.Reflection.Assembly" /> is an NShape library.
		/// </summary>
		/// <remarks>
		/// NShape libraries must implement a static class named <see cref="T:NShapeLibraryInitializer" />. 
		/// See documentation for details.
		/// </remarks>
		public bool IsValidLibrary(string assemblyPath, out string reason) {
			bool result = false;
			reason = null;
			try {
				string fullAssemblyPath = GetFullAssemblyPath(assemblyPath);
				AssemblyName assemblyName = AssemblyName.GetAssemblyName(assemblyPath);
				if (assemblyName != null) {
					Assembly assembly = Assembly.Load(assemblyName.FullName);
					result = IsValidLibrary(assembly, out reason);
				} else
					reason = Dataweb.NShape.Properties.Resources.MessageTxt_InvalidAssemblyPath;
			} catch (Exception exc) {
				reason = exc.Message;
			}
			return result;
		}


		/// <summary>
		/// Checks whether the given <see cref="T:System.Reflection.Assembly" /> is an NShape library.
		/// </summary>
		/// <remarks>
		/// NShape libraries must implement a static class named <see cref="T:NShapeLibraryInitializer" />. 
		/// See documentation for details.
		/// </remarks>
		public bool IsValidLibrary(Assembly assembly) {
			string msg;
			return IsValidLibrary(assembly, out msg);
		}


		/// <summary>
		/// Checks whether the given <see cref="T:System.Reflection.Assembly" /> is an NShape library.
		/// </summary>
		/// <remarks>
		/// NShape libraries must implement a static class named <see cref="T:NShapeLibraryInitializer" />. 
		/// See documentation for details.
		/// </remarks>
		public bool IsValidLibrary(Assembly assembly, out string reason) {
			bool result = false;
			reason = null;
			try {
				if (GetInitializerType(assembly) != null)
					result = true;
				else
					reason = string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_AssemblyDoesNotContainThe0Class, NShapeLibraryInitializerClassName);
			} catch (Exception exc) {
				reason = exc.Message;
			}
			return result;
		}


		/// <summary>
		/// Adds the given library assembly to the project.
		/// </summary>
		/// <param name="assembly">Shape or model object library assembly to add.</param>
		/// <param name="unloadOnClose">
		/// Specifies whether the library should be unloaded when closing the project.
		/// When setting AutoLoadLibraries to false, libraries should not be unloaded when closing the project.
		/// </param>
		public void AddLibrary(Assembly assembly, bool unloadOnClose) {
			if (assembly == null) throw new ArgumentNullException(nameof(assembly));
			DoLoadLibrary(assembly, unloadOnClose);
			if (IsOpen) {
				DoRegisterLibrary(FindLibraryByAssemblyName(assembly.FullName), true);
				_repository.Update();
			}
			if (LibraryLoaded != null) LibraryLoaded(this, new LibraryLoadedEventArgs(assembly.FullName));
		}


		/// <summary>
		/// Adds the library assembly with the given assembly name to the project.
		/// </summary>
		/// <param name="assemblyName">Shape library's assembly name.</param>
		/// <param name="unloadOnClose">
		/// Specifies whether the library should be unloaded when closing the project.
		/// When setting AutoLoadLibraries to false, libraries should not be unloaded when closing the project.
		/// </param>
		public void AddLibraryByName(string assemblyName, bool unloadOnClose) {
			if (string.IsNullOrEmpty(assemblyName)) throw new ArgumentNullException(nameof(assemblyName));
			Assembly a = Assembly.Load(assemblyName);
			AddLibrary(a, unloadOnClose);
		}


		/// <summary>
		/// Adds the library assembly at the given location to the project.
		/// </summary>
		/// <param name="assemblyPath">Complete file path to the library assembly.</param>
		/// <param name="unloadOnClose">
		/// Specifies whether the library should be unloaded when closing the project.
		/// When setting AutoLoadLibraries to false, libraries should not be unloaded when closing the project.
		/// </param>
		public void AddLibraryByFilePath(string assemblyPath, bool unloadOnClose) {
			Assembly a = Assembly.LoadFile(GetFullAssemblyPath(assemblyPath));
			AddLibrary(a, unloadOnClose);
		}


		/// <summary>
		/// Unloads and removes the given library.
		/// </summary>
		public void RemoveLibrary(Assembly assembly) {
			for (int i = _libraries.Count - 1; i >= 0; --i) {
				if (_libraries[i].Assembly == assembly)
					RemoveLibrary(_libraries[i]);
			}
		}


		/// <summary>
		/// Unloads and removes all libraries.
		/// </summary>
		public void RemoveAllLibraries() {
			AssertClosed();
			_shapeTypes.Clear();
			_modelObjectTypes.Clear();
			_diagramModelObjectTypes.Clear();
			_libraries.Clear();
		}


		/// <summary>
		/// Modifies the repository version of this project to the latest version supported by the repository and the loaded libraries.
		/// </summary>
		public void UpgradeVersion() {
			if (!Repository.CanModifyVersion)
				throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageFmt_CurrentRepositoryDoesNotSupportModifyingTheRepositoryStorageVersion);
			ChangeVersion(LastSupportedSaveVersion);
		}


		/// <summary>
		/// Executes a command and adds it to the project's history.
		/// </summary>
		/// <param name="command"></param>
		public void ExecuteCommand(ICommand command) {
			if (command == null) throw new ArgumentNullException(nameof(command));
			Exception exc = command.CheckAllowed(_security);
			if (exc != null) throw exc;
			command.Repository = _repository;
			_history.ExecuteAndAddCommand(command);
		}


		/// <summary>
		/// Opens a new project.
		/// </summary>
		public void Create() {
			DoOpen(true);
		}


		/// <summary>
		/// Opens an existing project.
		/// </summary>
		public void Open() {
			DoOpen(false);
		}


		/// <summary>
		/// Closes the project.
		/// </summary>
		public void Close() {
			if (Closing != null) Closing(this, EventArgs.Empty);
			if (IsOpen) {
				if (_repository.IsOpen) {
					// Get styleSet before closing the repository
					IStyleSet styleSet = ((IStyleSetProvider)this).StyleSet;
					// Delete GDI+ objects created from styles
					ToolCache.RemoveStyleSetTools(styleSet);

					// Close Repository
					_repository.Close();
				}
				// Unload libraries that were loaded when opening the project
				for (int i = _libraries.Count - 1; i >= 0; --i)
					if (_libraries[i].UnloadOnClose)
						_libraries.RemoveAt(i);
				// Clean up project data
				_settings.Clear();
				_model = null;
				_history.Clear();
				// Create new settings
				_settings = new ProjectSettings();
				// TODO 2: Unload dynamic libraries and remove the corresponding shape and model types.
				//RemoveAllLibraries();
			}
			if (Closed != null) Closed(this, EventArgs.Empty);
		}


		/// <summary>
		/// Indicates whether the project is open.
		/// </summary>
		[Browsable(false)]
		public bool IsOpen {
			get { return _repository != null && _repository.IsOpen; }
		}


		/// <summary>
		/// Registers all entities with the repository.
		/// </summary>
		public void RegisterEntityTypes() {
			RegisterEntityTypesCore(true);
		}


		/// <summary>
		/// Returns the XML representation of the data stored in the repository.
		/// </summary>
		/// <remarks>Only supported by <see cref="T:Dataweb.NShape.Advanced.CachedRepository" />.</remarks>
		public string GetXml() {
			string result = null;
			using (MemoryStream xmlStream = new MemoryStream()) {
				WriteXml(xmlStream);
				result = Encoding.UTF8.GetString(xmlStream.GetBuffer(), 0, (int)xmlStream.Length);
			}
			return result;
		}


		/// <summary>
		/// Reads XML data into the project using the specified <see cref="T:System.IO.Stream" />.
		/// Throws an exception if a Repository instance is assigned. The repository will be created.
		/// </summary>
		/// <remarks>Only supported by <see cref="T:Dataweb.NShape.Advanced.CachedRepository" />.</remarks>
		public void ReadXml(Stream stream) {
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			AssertClosed();
			if (Repository != null) 
				throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageTxt_ARepositoryIsAssignedToTheProjectRepositoryPropertyMustBeNull);

			CachedRepository cachedRepository = null;
			XmlStore tmpStore = null;
			try {
				// Create and initialize a temporary XML store
				tmpStore = new XmlStore(stream);
				tmpStore.LazyLoading = false;
				tmpStore.ImageLocation = XmlStore.ImageFileLocation.Embedded;

				// Create a new CachedRepository and assign the store
				cachedRepository = new CachedRepository();
				cachedRepository.Store = tmpStore;

				// Now open the project
				_repository = cachedRepository;
				_repository.ProjectName = Name;
				Open();
				
				// Iterate diagram objects in order to make sure everything is loaded before 
				// disposing the temporary XML store
				foreach (Diagram d in _repository.GetDiagrams()) { }

				// Todo: Find a solution for the following problems:
				// * When loading the entities from stream, the referenced Id's won't match the Id's of 
				//   the existing entity instances.
				// * When closing the repository before loading the stream, the shape libraries
				//   will not be / cannot be registered from here (both managed by the project component).
			} catch (Exception) {
				throw;
			} finally {
				if (cachedRepository != null)
					cachedRepository.Store = null;
				if (tmpStore != null) {
					tmpStore.Dispose();
					tmpStore = null;
				}
			}
		}


		/// <summary>
		/// Writes the current cache content as XML data.
		/// </summary>
		/// <remarks>Only supported by <see cref="T:Dataweb.NShape.Advanced.CachedRepository" />.</remarks>
		public void WriteXml(Stream stream) {
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			//if (!stream.CanSeek) throw new ArgumentException("The provided stream does not support seeking.");
			if (Repository.IsModified) throw new NShapeException(Properties.Resources.MessageTxt_UnsavedRepositoryModificationsPending);
			
			CachedRepository cachedRepository = Repository as CachedRepository;
			if (cachedRepository == null) throw new NotSupportedException();

			XmlStore tmpStore = null;
			Store currentStore = cachedRepository.Store;
			try {
				// Create and initialize temporary XML store
				tmpStore = new XmlStore(stream);
				tmpStore.ImageLocation = XmlStore.ImageFileLocation.Embedded;
				tmpStore.ProjectName = cachedRepository.ProjectName;
				tmpStore.Version = cachedRepository.Version;

				// Make sure all diagrams are loaded completely
				foreach (Diagram d in Repository.GetDiagrams())
					Repository.GetDiagramShapes(d);

				// Assign store to repository and save data
				cachedRepository.Store = tmpStore;
				tmpStore.Create(cachedRepository);
				tmpStore.SaveChanges(cachedRepository);
				tmpStore.Close(cachedRepository);
			} finally {
				cachedRepository.Store = currentStore;
				if (tmpStore != null) {
					tmpStore.Dispose();
					tmpStore = null;
				}
			}
		}

		#endregion


		#region IRegistrar Members

		/// <override></override>
		void IRegistrar.RegisterLibrary(string name, int preferredRepositoryVersion) {
			if (!Project.IsValidName(name)) throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_0IsNotAValidLibraryName, name));
			_initializingLibrary.Name = name;
			_initializingLibrary.PreferredRepositoryVersion = preferredRepositoryVersion;
			if (_addingLibrary)
				_settings.AddLibrary(name, _initializingLibrary.Assembly.FullName, preferredRepositoryVersion);
		}


		/// <override></override>
		void IRegistrar.RegisterShapeType(ShapeType shapeType) {
			if (_initializingLibrary == null)
				throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_RegisterShapeTypeCanOnlyBeCalledWhileALibraryIsInitializing);
			if (string.IsNullOrEmpty(_initializingLibrary.Name))
				throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_RegisterLibraryHasNotBeenCalledOrTheLibraryHasAnEmptyLibraryName);
			if (shapeType == null) throw new ArgumentNullException(nameof(shapeType));
			if (shapeType.LibraryName != _initializingLibrary.Name)
				throw new InvalidOperationException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_TheLibraryNameOfShapeType0Is1InsteadOf2, shapeType.GetType().Name, shapeType.LibraryName, _initializingLibrary.Name));
			//
			shapeType.StyleSetProvider = this;
			_shapeTypes.Add(shapeType);
			// If the cache is not open, the following actions will be performed
			// when opening it.
			if (_repository != null && _repository.IsOpen) {
				RegisterShapeEntityType(shapeType, _addingLibrary);
				if (_autoCreateTemplates && _addingLibrary) CreateDefaultTemplate(shapeType);
			}
		}


		/// <override></override>
		void IRegistrar.RegisterModelObjectType(ModelObjectType modelObjectType) {
			if (_initializingLibrary == null)
				throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_RegisterModelObjectTypeCanOnlyBeCalledWhileALibraryIsInitializing);
			if (string.IsNullOrEmpty(_initializingLibrary.Name))
				throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_RegisterLibraryHasNotBeenCalledOrTheLibraryHasAnEmptyLibraryName);
			if (modelObjectType == null) throw new ArgumentNullException(nameof(modelObjectType));
			if (!Project.IsValidName(modelObjectType.Name))
				throw new ArgumentException(Dataweb.NShape.Properties.Resources.MessageFmt_0IsNotAValidModelObjectTypeName, modelObjectType.Name);
			if (modelObjectType.LibraryName != _initializingLibrary.Name)
				throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_AllModelObjectsOfARegisteringLibraryMustHaveTheLibrarySLibraryName);
			if (modelObjectType.LibraryName != _initializingLibrary.Name)
				throw new InvalidOperationException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_TheLibraryNameOfModelObjectType0Is1InsteadOf2, modelObjectType.GetType().Name, modelObjectType.LibraryName, _initializingLibrary.Name));
			//
			_modelObjectTypes.Add(modelObjectType);
			// Create a delegate that adds required parameters to the CreateModelObjectDelegate 
			// of the shape when called
			if (_repository != null && _repository.IsOpen) {
				RegisterModelObjectEntityType(modelObjectType, _addingLibrary);
			}
		}


		/// <override></override>
		void IRegistrar.RegisterDiagramModelObjectType(DiagramModelObjectType diagramModelObjectType)
		{
			if (_initializingLibrary == null)
				throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_RegisterDiagramModelObjectTypeCanOnlyBeCalledWhileALibraryIsInitializing);
			if (string.IsNullOrEmpty(_initializingLibrary.Name))
				throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_RegisterLibraryHasNotBeenCalledOrTheLibraryHasAnEmptyLibraryName);
			if (diagramModelObjectType == null) throw new ArgumentNullException(nameof(diagramModelObjectType));
			if (!Project.IsValidName(diagramModelObjectType.Name))
				throw new ArgumentException(Dataweb.NShape.Properties.Resources.MessageFmt_0IsNotAValidModelObjectTypeName, diagramModelObjectType.Name);
			if (diagramModelObjectType.LibraryName != _initializingLibrary.Name)
				throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_AllModelObjectsOfARegisteringLibraryMustHaveTheLibrarySLibraryName);
			if (diagramModelObjectType.LibraryName != _initializingLibrary.Name)
				throw new InvalidOperationException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_TheLibraryNameOfModelObjectType0Is1InsteadOf2, diagramModelObjectType.GetType().Name, diagramModelObjectType.LibraryName, _initializingLibrary.Name));
			//
			_diagramModelObjectTypes.Add(diagramModelObjectType);
			// Create a delegate that adds required parameters to the CreateModelObjectDelegate 
			// of the shape when called
			if (_repository != null && _repository.IsOpen) {
				RegisterDiagramModelObjectEntityType(diagramModelObjectType, _addingLibrary);
			}
		}

		#endregion


		#region IStyleSetProvider Members

		/// <override></override>
		[Browsable(false)]
		IStyleSet IStyleSetProvider.StyleSet {
			get {
				AssertOpen();
				return (IStyleSet)_repository.GetDesign(null);
			}
		}

		#endregion


		#region [Internal] Methods

		/// <ToBeCompleted></ToBeCompleted>
		internal static void AssertSupportedVersion(bool save, int version) {
			int minVersion = save ? FirstSupportedSaveVersion : FirstSupportedLoadVersion;
			int maxVersion = save ? LastSupportedSaveVersion : LastSupportedLoadVersion;
			if (version < minVersion || version > maxVersion) {
				string messageFmt = save ? Dataweb.NShape.Properties.Resources.MessageFmt_SavingRepositoryFailedDueToVersion012
					: Properties.Resources.MessageFmt_LoadingRepositoryFailedTheRepositoryDueToVersion012;
				string msg = string.Format(messageFmt, version, minVersion, maxVersion);
				throw new NShapeException(msg);
			}
		}


		// Changes the version of the repository to the specified version number (even downgrades are possible)
		internal void ChangeVersion(int version) {
			bool isUpgradeToMaxVersion = !(version < LastSupportedSaveVersion);

			// Make sure the repository is loaded completely before upgrading the repository version
			foreach (Diagram d in Repository.GetDiagrams())
				_repository.GetDiagramShapes(d, Geometry.InvalidRectangle);

			// Update repository version
			_repository.Version = version;

			// Set all version numbers to the library version (which reflects the lastest supported version)
			foreach (Library lib in _libraries)
				_settings.SetRepositoryVersion(lib.Name, isUpgradeToMaxVersion ? lib.PreferredRepositoryVersion : version);

			// Recreate all core library entity types using the latest version
			_repository.RemoveAllEntityTypes();
			foreach (IEntityType entityType in CreateCoreLibraryBaseEntityTypes(isUpgradeToMaxVersion ? LastSupportedSaveVersion : version))
				_repository.AddEntityType(entityType);
			foreach (IEntityType modelObjectEntityType in CreateCoreLibraryModelObjectEntityTypes(isUpgradeToMaxVersion ? LastSupportedSaveVersion : version))
				_repository.AddEntityType(modelObjectEntityType);
			foreach (IEntityType shapeEntityType in CreateCoreLibraryShapeEntityTypes(isUpgradeToMaxVersion ? LastSupportedSaveVersion : version))
				_repository.AddEntityType(shapeEntityType);

			// Recreate entity types for all registered shape- and model object types using the latest library version
			foreach (ModelObjectType modelObjectType in ModelObjectTypes) {
				if (string.Compare(modelObjectType.LibraryName, CoreLibraryName) == 0) continue;
				RegisterModelObjectEntityType(modelObjectType, isUpgradeToMaxVersion);
			}
			foreach (ShapeType shapeType in ShapeTypes) {
				if (string.Compare(shapeType.LibraryName, CoreLibraryName) == 0) continue;
				RegisterShapeEntityType(shapeType, isUpgradeToMaxVersion);
			}

			// Save repository after upgrading
			_repository.SaveChanges();
		}

		#endregion


		#region [Private] Implementation

		private void Construct() {
			_settings = new ProjectSettings();
			_history = new History();
			_registerArgs = new object[1] { ((IRegistrar)this) };
		}


		private void AssertOpen() {
			if (!IsOpen) throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_ProjectMustBeOpenToExecuteThisOperation);
		}


		private void AssertClosed() {
			if (IsOpen) throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_ProjectMustNotBeOpenToExecuteThisOperation);
		}


		private void RegisterDiagramModelObjectEntityType(DiagramModelObjectType diagramModelObjectType, bool create)
		{
			int version = FindLibraryVersion(diagramModelObjectType.LibraryName, create);
			IEntityType entityType = new EntityType(diagramModelObjectType.FullName, EntityCategory.ModelObject,
				version, () => diagramModelObjectType.CreateInstance(), diagramModelObjectType.GetPropertyDefinitions(version));
			_repository.AddEntityType(entityType);
		}


		private void RegisterModelObjectEntityType(ModelObjectType modelObjectType, bool create)
		{
			int version = FindLibraryVersion(modelObjectType.LibraryName, create);
			IEntityType entityType = new EntityType(modelObjectType.FullName, EntityCategory.ModelObject,
				version, () => modelObjectType.CreateInstance(), modelObjectType.GetPropertyDefinitions(version));
			_repository.AddEntityType(entityType);
		}


		private void RegisterShapeEntityType(ShapeType shapeType, bool create) {
			int version = FindLibraryVersion(shapeType.LibraryName, create);
			IEntityType entityType = new EntityType(shapeType.FullName, EntityCategory.Shape,
				version, () => shapeType.CreateInstanceForLoading(), shapeType.GetPropertyDefinitions(version));
			_repository.AddEntityType(entityType);
		}


		private Library FindLibraryByAssemblyName(string assemblyName) {
			// Check whether already loaded
			AssemblyName soughtAssemblyName = new AssemblyName(assemblyName);
			foreach (Library l in _libraries) {
				AssemblyName libAssemblyName = l.Assembly.GetName();
				if (AssemblyName.ReferenceMatchesDefinition(soughtAssemblyName, libAssemblyName))
					return l;
			}
			return null;
		}


		private Assembly FindAssemblyInSearchPath(string assemblyName) {
			if (!_autoLoadLibraries)
				return null;
			AssemblyName soughtAssemblyName = new AssemblyName(assemblyName);
			int cnt = LibrarySearchPaths.Count;
			for (int pathIdx = 0; pathIdx < cnt; ++pathIdx) {
				string[] files = Directory.GetFiles(LibrarySearchPaths[pathIdx], "*.dll");
				for (int fileIdx = files.Length - 1; fileIdx >= 0; --fileIdx) {
					try {
						AssemblyName foundAssemblyName = AssemblyName.GetAssemblyName(files[fileIdx]);
						if (AssemblyName.ReferenceMatchesDefinition(soughtAssemblyName, foundAssemblyName)) {
							// Use Assembly.Load() here, because otherwise the assembly will be loaded into a 
							// different context which results in type conversion problems.
							// See http://blogs.msdn.com/b/aszego/archive/2009/10/16/avoid-using-assembly-loadfrom.aspx
							// for a detailed description on the problem.
							return Assembly.Load(foundAssemblyName);
						}
					} catch (BadImageFormatException ex) {
						Debug.Print(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_AnExceptionOccuredWhileSearchingAssembly0InPath12,
							assemblyName, files[fileIdx], ex.Message));
					} catch (NotImplementedException ex) {
						Debug.Print(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_ANotImplementedExceptionOccuredWhileSearchingAssembly0InPath12,
							assemblyName, files[fileIdx], ex.Message));
					}
				}
			}
			return null;
		}


		private void DoOpen(bool create) {
			if (IsOpen)
				throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_ProjectIsAlreadyOpen);
			if (string.IsNullOrEmpty(Name))
				throw new InvalidOperationException(Dataweb.NShape.Properties.Resources.MessageTxt_NoNameDefinedForTheProject);
			// Create/Reset repository
			if (_repository == null) {
				_repository = new CachedRepository();
				_repository.ProjectName = Name;
			} else {
				Debug.Assert(!_repository.IsOpen);
				_repository.Close();
				_repository.RemoveAllEntityTypes();
			}
			_diagramModelObjectTypes.Clear();
			_modelObjectTypes.Clear();
			_shapeTypes.Clear();
			if (create) {
				// Store libraries that should not be unloaded so we can reload them
				// right after re-creating the project settings
				List<Library> reloadLibs = new List<Library>(_libraries);
				_libraries.Clear();
				// Register core library
				_repository.Version = LastSupportedSaveVersion;
				RegisterEntityTypesCore(true);

				// Create repository and model
				_repository.Create();
				_model = new Model();
				_repository.Insert(_model);
				// Re-initialize libraries
				try {
					_settings = _repository.GetProject();
					if (reloadLibs.Count > 0) {
						foreach (Library lib in reloadLibs) {
							_libraries.Add(lib);
							DoRegisterLibrary(lib, true);
						}
						_repository.Update();
					}
				} catch (Exception exc) {
					Debug.Print(exc.Message);
					_repository.Close();
					throw;
				}
			} else {
				// We unload all shape and model object types here. Only the ones defined by the
				// project will be usable.
				_repository.ReadVersion();
				if (_repository.Version <= 0)
					throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageFmt_InvalidRepositoryBaseVersion0, _repository.Version);
				RegisterCoreLibraryTypes(false);
				_repository.Open();
				try {
					_settings = _repository.GetProject();
					// Load the project libraries
					foreach (string ln in _settings.AssemblyNames) {
						Library lib = FindLibraryByAssemblyName(ln);
						if (lib == null) {
							if (_autoLoadLibraries) {
								Assembly a = null;
								try {
									a = Assembly.Load(ln);
								} catch (FileLoadException exc) {
									a = FindAssemblyInSearchPath(ln);
									if (a == null) throw new LoadLibraryException(ln, exc);
								} catch (FileNotFoundException exc) {
									a = FindAssemblyInSearchPath(ln);
									if (a == null) throw new LoadLibraryException(ln, exc);
								}
								Debug.Assert(a != null);
								lib = DoLoadLibrary(a, true);
							} else throw new LoadLibraryException(ln);
						}
						Debug.Assert(lib != null);
						DoRegisterLibrary(lib, false);
					}
				} catch (Exception exc) {
					Debug.Print(exc.Message);
					_repository.Close();
					throw;
				}
			}
			if (Opened != null) Opened(this, EventArgs.Empty);
		}


		private void RegisterEntityTypesCore(bool create) {
			_repository.RemoveAllEntityTypes();
			RegisterCoreLibraryTypes(create);
			foreach (Library l in _libraries)
				DoRegisterLibrary(l, true);

			// Register static diagram model entity types
			foreach (DiagramModelObjectType dmot in _diagramModelObjectTypes)
				if (!dmot.LibraryName.Equals(CoreLibraryName, StringComparison.InvariantCultureIgnoreCase))
					RegisterDiagramModelObjectEntityType(dmot, create);
			// Register static model entity types
			foreach (ModelObjectType mot in _modelObjectTypes)
				if (!mot.LibraryName.Equals(CoreLibraryName, StringComparison.InvariantCultureIgnoreCase))
					RegisterModelObjectEntityType(mot, create);
			// Register static shape entity types
			foreach (ShapeType st in _shapeTypes)
				if (!st.LibraryName.Equals(CoreLibraryName, StringComparison.InvariantCultureIgnoreCase))
					RegisterShapeEntityType(st, create);
		}


		// Registers entity types for styles, designs, projectData, templates and diagramControllers with the repository.
		private void RegisterCoreLibraryTypes(bool create) {
			int version;
			// When creating a repository without a valid version, we use the last supported save version.
			if (create && _repository.Version <= 0)
				_repository.Version = LastSupportedSaveVersion;
			version = _repository.Version;
			//
			foreach (IEntityType entityType in CreateCoreLibraryBaseEntityTypes(version))
				_repository.AddEntityType(entityType);
			//
			IRegistrar registrar = (IRegistrar)this;
			// Register core library
			_initializingLibrary = new Library(GetType().Assembly);
			registrar.RegisterLibrary(CoreLibraryName, version);
			// Register the framework's mandatory core library shape types
			foreach (ShapeType shapeType in CreateCoreLibraryShapeTypes())
				registrar.RegisterShapeType(shapeType);
			// Register the framework's mandatory core library model object types
			foreach (ModelObjectType modelObjectType in CreateCoreLibraryModelObjectTypes())
				registrar.RegisterModelObjectType(modelObjectType);
			foreach (DiagramModelObjectType diagramModelObjectType in CreateCoreLibraryDiagramModelObjectTypes())
				registrar.RegisterDiagramModelObjectType(diagramModelObjectType);
			_initializingLibrary = null;
			//
			// Register the framework's mandatory core shape and model object entity types
			foreach (IEntityType modelObjectEntityType in CreateCoreLibraryModelObjectEntityTypes(version))
				_repository.AddEntityType(modelObjectEntityType);
			foreach (IEntityType shapeEntityType in CreateCoreLibraryShapeEntityTypes(version))
				_repository.AddEntityType(shapeEntityType);
			//int libVersion = FindLibraryVersion(shapeType.LibraryName, create);
			//    IEntityType entityType = new EntityType(shapeType.FullName, EntityCategory.Shape,
			//        version, () => shapeType.CreateInstanceForLoading(), shapeType.GetPropertyDefinitions(version));
			//    repository.AddEntityType(entityType);


			//// Register static model entity types
			//foreach (ModelObjectType mot in modelObjectTypes)
			//    RegisterModelObjectEntityType(mot, create);
			//// Register static shape entity types
			//foreach (ShapeType st in shapeTypes)
			//    RegisterShapeEntityType(st, create);
		}


		/// <summary>
		/// Creates entity types for all framework base entities such as project settings, styles, templates, diagrams, etc.
		/// </summary>
		private IEnumerable<IEntityType> CreateCoreLibraryBaseEntityTypes(int version) {
			AssertSupportedVersion(true, version);

			// Create Design and Style entity types
			yield return new EntityType(CapStyle.EntityTypeName, EntityCategory.Style,
				version, () => new CapStyle(), CapStyle.GetPropertyDefinitions(version));
			yield return new EntityType(CharacterStyle.EntityTypeName, EntityCategory.Style,
				version, () => new CharacterStyle(), CharacterStyle.GetPropertyDefinitions(version));
			yield return new EntityType(ColorStyle.EntityTypeName, EntityCategory.Style,
				version, () => new ColorStyle(), ColorStyle.GetPropertyDefinitions(version));
			yield return new EntityType(FillStyle.EntityTypeName, EntityCategory.Style,
				version, () => new FillStyle(), FillStyle.GetPropertyDefinitions(version));
			yield return new EntityType(LineStyle.EntityTypeName, EntityCategory.Style,
				version, () => new LineStyle(), LineStyle.GetPropertyDefinitions(version));
			yield return new EntityType(ParagraphStyle.EntityTypeName, EntityCategory.Style,
				version, () => new ParagraphStyle(), ParagraphStyle.GetPropertyDefinitions(version));
			yield return new EntityType(Design.EntityTypeName, EntityCategory.Design,
				version, () => new Design(), Design.GetPropertyDefinitions(version));

			// Create Project, Template and Diagram entity types
			yield return new EntityType(ProjectSettings.EntityTypeName, EntityCategory.ProjectSettings,
				version, () => new ProjectSettings(), ProjectSettings.GetPropertyDefinitions(version));
			yield return new EntityType(Template.EntityTypeName, EntityCategory.Template,
				version, () => new Template(), Template.GetPropertyDefinitions(version));
			yield return new EntityType(Diagram.EntityTypeName, EntityCategory.Diagram,
				version, () => new Diagram(string.Empty), Diagram.GetPropertyDefinitions(version));

			// Create mandatory Model and modelMapping entity types
			yield return new EntityType(Model.EntityTypeName, EntityCategory.Model,
				version, () => new Model(), Model.GetPropertyDefinitions(version));
			yield return new EntityType(NumericModelMapping.EntityTypeName, EntityCategory.ModelMapping,
				version, () => new NumericModelMapping(), NumericModelMapping.GetPropertyDefinitions(version));
			yield return new EntityType(FormatModelMapping.EntityTypeName, EntityCategory.ModelMapping,
				version, () => new FormatModelMapping(), FormatModelMapping.GetPropertyDefinitions(version));
			yield return new EntityType(StyleModelMapping.EntityTypeName, EntityCategory.ModelMapping,
				version, () => new StyleModelMapping(), StyleModelMapping.GetPropertyDefinitions(version));
		}


		/// <summary>
		/// Creates entity types for all mandatory framework shape entities such as ShapeGroup.
		/// </summary>
		private IEnumerable<ShapeType> CreateCoreLibraryShapeTypes() {
			yield return new ShapeType(ShapeGroupName, CoreLibraryName, CoreLibraryName,
				ShapeGroup.CreateInstance, ShapeGroup.GetPropertyDefinitions, false);
		}


		/// <summary>
		/// Creates shape types for all mandatory framework shapes such as ShapeGroup.
		/// </summary>
		private IEnumerable<IEntityType> CreateCoreLibraryShapeEntityTypes(int version) {
			// Do not use the ShapeType directly as this instance is not registered!
			// Use its name for fetching the registered shape type from the ShapeTypes collection.
			foreach (ShapeType st in CreateCoreLibraryShapeTypes()) {
				ShapeType shapeType = ShapeTypes[st.FullName];
				yield return new EntityType(shapeType.FullName, EntityCategory.Shape,
					version, () => shapeType.CreateInstanceForLoading(), shapeType.GetPropertyDefinitions(version));
			}
		}


		/// <summary>
		/// Creates model object types for all mandatory framework model objects such as GenericModelObject.
		/// </summary>
		private IEnumerable<ModelObjectType> CreateCoreLibraryModelObjectTypes() {
			TerminalId maxTerminalId = 4;
			yield return new GenericModelObjectType(GenericModelObjectName, CoreLibraryName, CoreLibraryName,
				GenericModelObject.CreateInstance, GenericModelObject.GetPropertyDefinitions, maxTerminalId);
		}


		/// <summary>
		/// Creates model object types for all mandatory framework model objects such as GenericModelObject.
		/// </summary>
		private IEnumerable<DiagramModelObjectType> CreateCoreLibraryDiagramModelObjectTypes()
		{
			yield return new GenericDiagramModelObjectType(GenericModelObjectName, CoreLibraryName, CoreLibraryName,
				GenericDiagramModelObject.CreateInstance, GenericModelObject.GetPropertyDefinitions);
		}


		/// <summary>
		/// Creates entity types for all mandatory framework model object entities such as GenericModelObject.
		/// </summary>
		private IEnumerable<IEntityType> CreateCoreLibraryModelObjectEntityTypes(int version) {
			foreach (ModelObjectType modelObjectType in CreateCoreLibraryModelObjectTypes()) {
				yield return new EntityType(modelObjectType.FullName, EntityCategory.ModelObject,
					version, () => modelObjectType.CreateInstance(), modelObjectType.GetPropertyDefinitions(version));
			}
		}


		private Library DoLoadLibrary(Assembly assembly, bool unloadOnClose) {
			if (assembly == null) throw new ArgumentNullException(nameof(assembly));
			Library result = FindLibraryByAssemblyName(assembly.FullName);
			if (result != null) throw new InvalidOperationException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Library0IsAlreadyLoaded, assembly.FullName));
			result = new Library(assembly, unloadOnClose);
			_libraries.Add(result);
			return result;
		}


		private void DoRegisterLibrary(Library library, bool adding) {
			_addingLibrary = adding;
			_initializingLibrary = library;
			try {
				InitializeLibrary(library);
			} catch (System.Reflection.ReflectionTypeLoadException exc) {
				DoRemoveLibrary(library);
				if (exc.LoaderExceptions != null && exc.LoaderExceptions.Length > 0)
					throw exc.LoaderExceptions[exc.LoaderExceptions.Length - 1];
				else throw;
			} catch (Exception exc) {
				DoRemoveLibrary(library);
				throw exc;
			} finally {
				_addingLibrary = false;
				_initializingLibrary = null;
			}
		}


		// ToDo: Implement RemoveLibrary, RemoveLibraryByPath and RemoveLibraryByName
		private void DoRemoveLibrary(Library library) {
			if (library == null) throw new ArgumentNullException(nameof(library));
			_libraries.Remove(library);
		}


		private void RemoveLibrary(Library library) {
			if (library == null) throw new ArgumentNullException(nameof(library));
			List<ShapeType> registeredShapeTypes = new List<ShapeType>(GetRegisteredShapeTypes(library));
			List<ModelObjectType> registeredModelObjectTypes = new List<ModelObjectType>(GetRegisteredModelObjectTypes(library));
			List<DiagramModelObjectType> registeredDiagramModelObjectType = new List<DiagramModelObjectType>(GetRegisteredDiagramModelObjectTypes(library));
			List<Template> registeredShapeTypeTemplates = new List<Template>();
			bool canRemove = true;
			// Get and check all corresponding templates if they are referenced by shapes.
			foreach (Template template in Repository.GetTemplates()) {
				foreach (ShapeType registeredShapeType in registeredShapeTypes) {
					if (IsShapeOfType(template.Shape, registeredShapeType)) {
						if (Repository.IsTemplateInUse(template))
							canRemove = false;
						else registeredShapeTypeTemplates.Add(template);
						if (canRemove == false) break;
					}
					if (canRemove == false) break;
				}
			}
			// Check if there are shapes without templates 
			if (Repository.IsShapeTypeInUse(registeredShapeTypes)) {
				canRemove = false;
				throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageFmt_UnableToUnloadLibrary0ShapeTypeInUse, library.Assembly.GetName().Name);
			}
			if (Repository.IsModelObjectTypeInUse(registeredModelObjectTypes)) {
				canRemove = false;
				throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageFmt_UnableToUnloadLibrary0ModelObjectTypeInUse, library.Assembly.GetName().Name);
			}

			if (canRemove) {
				// Remove (unused) Templates
				foreach (Template registeredShapeTypeTemplate in registeredShapeTypeTemplates)
					Repository.DeleteAll(registeredShapeTypeTemplate);
				// Remove Shape EntityTypes
				foreach (ShapeType registeredShapeType in registeredShapeTypes) {
					_shapeTypes.Remove(registeredShapeType);
					_repository.RemoveEntityType(registeredShapeType.FullName);
				}
				// Remove Model EntityTypes
				foreach (ModelObjectType registeredModelObjectType in registeredModelObjectTypes) {
					_modelObjectTypes.Remove(registeredModelObjectType);
					_repository.RemoveEntityType(registeredModelObjectType.FullName);
				}
				_libraries.Remove(library);

				// Clear the history in order to prevent the user to undo 
				// changes that require the removed shape types.
				if (_history != null) _history.Clear();
			}
		}


		private bool IsShapeOfType(Shape shape, ShapeType shapeType) {
			if (shape == null) throw new ArgumentNullException(nameof(shape));
			if (shapeType == null) throw new ArgumentNullException(nameof(shapeType));
			bool result = false;
			if (shape.Type == shapeType)
				result = true;
			else {
				foreach (Shape childShape in shape.Children)
					if (IsShapeOfType(childShape, shapeType)) {
						result = true;
						break;
					}
			}
			return result;
		}


		private IEnumerable<ShapeType> GetRegisteredShapeTypes(Library library) {
			return GetRegisteredShapeTypes(SingleInstanceEnumerator<Library>.Create(library));
		}


		private IEnumerable<ShapeType> GetRegisteredShapeTypes(IEnumerable<Library> libraries) {
			foreach (ShapeType shapeType in _shapeTypes) {
				foreach (Library library in libraries) {
					if (shapeType.LibraryName == library.Name)
						yield return shapeType;
				}
			}
		}


		private IEnumerable<ModelObjectType> GetRegisteredModelObjectTypes(Library library) {
			return GetRegisteredModelObjectTypes(SingleInstanceEnumerator<Library>.Create(library));
		}


		private IEnumerable<ModelObjectType> GetRegisteredModelObjectTypes(IEnumerable<Library> libraries) {
			foreach (ModelObjectType modelObjectType in _modelObjectTypes) {
				foreach (Library library in libraries) {
					if (modelObjectType.LibraryName == library.Name)
						yield return modelObjectType;
				}
			}
		}


		private IEnumerable<DiagramModelObjectType> GetRegisteredDiagramModelObjectTypes(Library library)
		{
			return GetRegisteredDiagramModelObjectTypes(SingleInstanceEnumerator<Library>.Create(library));
		}


		private IEnumerable<DiagramModelObjectType> GetRegisteredDiagramModelObjectTypes(IEnumerable<Library> libraries)
		{
			foreach (DiagramModelObjectType diagramModelObjectType in _diagramModelObjectTypes) {
				foreach (Library library in libraries) {
					if (diagramModelObjectType.LibraryName == library.Name)
						yield return diagramModelObjectType;
				}
			}
		}


		private string GetFullAssemblyPath(string assemblyPath) {
			if (assemblyPath == null) throw new ArgumentNullException(nameof(assemblyPath));
			if (!Path.HasExtension(assemblyPath)) assemblyPath += ".dll";
			if (!Path.IsPathRooted(assemblyPath)) {
				string libDir = this.GetType().Assembly.Location;
				assemblyPath = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(libDir)), Path.GetFileName(assemblyPath));
			}
			if (!File.Exists(assemblyPath) && _autoLoadLibraries) {
				string assemblyFileName = Path.GetFileName(assemblyPath);
				string libPath = string.Empty;
				foreach (string dir in LibrarySearchPaths) {
					libPath = dir;
					if (!libPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
						libPath += Path.DirectorySeparatorChar;
					if (File.Exists(libPath + assemblyFileName)) {
						assemblyPath = libPath + assemblyFileName;
						break;
					}
				}
				if (!File.Exists(assemblyPath))
					throw new NShapeException(Dataweb.NShape.Properties.Resources.MessageFmt_Assembly0CannotBeFoundAtTheSpecifiedPath, assemblyPath);
			}
			return assemblyPath;
		}


		private Type GetInitializerType(Assembly assembly) {
			if (assembly == null) throw new ArgumentNullException(nameof(assembly));
			Type initializerType = null;
			foreach (Type t in assembly.GetTypes()) {
				if (t is Type) {
					if (t.Name.Equals(NShapeLibraryInitializerClassName, StringComparison.InvariantCultureIgnoreCase)) {
						initializerType = t;
						break;
					}
				}
			}
			if (initializerType == null)
				throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Assembly0IsNotANShapeLibraryItDoesNotImplementStaticClass1, assembly.Location, NShapeLibraryInitializerClassName));
			MethodInfo methodInfo = initializerType.GetMethod(InitializeMethodName);
			if (methodInfo == null)
				throw new ArgumentException(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_Assembly0IsNotANShapeLibraryItDoesNotImplement12, assembly.FullName, NShapeLibraryInitializerClassName, InitializeMethodName));
			return initializerType;
		}


		private void InitializeLibrary(Library library) {
			Type initializerType = GetInitializerType(library.Assembly);
			this._initializingLibrary = library;
			try {
				initializerType.InvokeMember(InitializeMethodName, BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static, null, null, _registerArgs);
			} catch (TargetInvocationException ex) {
				throw ex.InnerException;
			} finally {
				_initializingLibrary = null;
			}
		}


		/// <summary>
		/// For each loaded Type, a new <see cref="T:Dataweb.NShape.Advanced.Template" /> is created.
		/// These automatically created templates are not stored by the cache.
		/// </summary>
		/// <param name="shapeType"></param>
		private void CreateDefaultTemplate(ShapeType shapeType) {
			if (shapeType.SupportsAutoTemplates) {
				Template template = new Template(shapeType.Name, shapeType.CreateInstance());
				Repository.InsertAll(template);
			}
		}


		// Notify ToolCache that a style has changed
		// Invalidate all (loaded) shapes using the changed style
		private void repository_StyleUpdated(object sender, RepositoryStyleEventArgs e) {
			IStyle changedStyle = e.Style;
			Design design = null;
			foreach (Design d in _repository.GetDesigns()) {
				if (d.ContainsStyle(changedStyle)) {
					design = d;
					break;
				}
			}
			if (design != null) {
				// Update Toolcache and PreviewStyle
				if (changedStyle is CapStyle) {
					DoNotifyStyleChanged(design.CapStyles, (CapStyle)changedStyle);
					design.CapStyles.SetPreviewStyle((CapStyle)changedStyle, design.CreatePreviewStyle((ICapStyle)changedStyle));
				} else if (changedStyle is CharacterStyle) {
					DoNotifyStyleChanged(design.CharacterStyles, (CharacterStyle)changedStyle);
					design.CharacterStyles.SetPreviewStyle((CharacterStyle)changedStyle, design.CreatePreviewStyle((ICharacterStyle)changedStyle));
				} else if (changedStyle is ColorStyle) {
					DoNotifyStyleChanged(design.ColorStyles, (ColorStyle)changedStyle);
					design.ColorStyles.SetPreviewStyle((ColorStyle)changedStyle, design.CreatePreviewStyle((IColorStyle)changedStyle));
				} else if (changedStyle is FillStyle) {
					DoNotifyStyleChanged(design.FillStyles, (FillStyle)changedStyle);
					design.FillStyles.SetPreviewStyle((FillStyle)changedStyle, design.CreatePreviewStyle((IFillStyle)changedStyle));
				} else if (changedStyle is LineStyle) {
					DoNotifyStyleChanged(design.LineStyles, (LineStyle)changedStyle);
					design.LineStyles.SetPreviewStyle((LineStyle)changedStyle, design.CreatePreviewStyle((ILineStyle)changedStyle));
				} else if (changedStyle is ParagraphStyle) {
					DoNotifyStyleChanged(design.ParagraphStyles, (ParagraphStyle)changedStyle);
					design.ParagraphStyles.SetPreviewStyle((ParagraphStyle)changedStyle, design.CreatePreviewStyle((IParagraphStyle)changedStyle));
				}
			}

			// If the style is contained in the current design, notify all shapes that the style has changed
			if (design == Design) {
				foreach (Template template in _repository.GetTemplates())
					template.Shape.NotifyStyleChanged(changedStyle);
				foreach (Diagram diagram in _repository.GetDiagrams()) {
					foreach (Shape shape in diagram.Shapes)
						shape.NotifyStyleChanged(changedStyle);
				}
			}
		}


		private void DoNotifyStyleChanged<TStyle, TStyleInterface>(StyleCollection<TStyle, TStyleInterface> styleCollection, TStyle style)
			where TStyle : class, TStyleInterface
			where TStyleInterface : class, IStyle {
			if (styleCollection == null) throw new ArgumentNullException(nameof(styleCollection));
			if (style == null) throw new ArgumentNullException(nameof(style));
			// Maintain StyleCollection's name index in case the style was renamed
			Debug.Assert(styleCollection.Contains(style) == styleCollection.Contains(style.Name));
			// Create and set new PreviewStyle if the style is in the currently active design
			if (styleCollection.ContainsPreviewStyle(style)) {
				TStyle previewStyle = styleCollection.GetPreviewStyle(style);
				Debug.Assert(previewStyle != null);
				ToolCache.NotifyStyleChanged(previewStyle);
			}
			ToolCache.NotifyStyleChanged(style);
		}


		private void repository_StyleDeleted(object sender, RepositoryStyleEventArgs e) {
			ToolCache.NotifyStyleChanged(e.Style);
		}


		// Determines the repository version to use for the given library.
		private int FindLibraryVersion(string libraryName, bool create) {
			int result = -1;
			if (libraryName.Equals(CoreLibraryName, StringComparison.InvariantCultureIgnoreCase))
				result = create ? LastSupportedSaveVersion : _repository.Version;
			else {
				if (create) {
					// Find library and return the preferred version
					foreach (Library lib in _libraries) {
						if (string.Compare(lib.Name, libraryName) == 0) {
							result = lib.PreferredRepositoryVersion;
							break;
						}
					}
				} else
					result = _settings.GetRepositoryVersion(libraryName);
			}
			if (result <= 0) throw new Exception(string.Format(Dataweb.NShape.Properties.Resources.MessageFmt_InvalidRepositoryVersion0, result));
			return result;
		}

		#endregion


		#region [Private] Library Class

		/// <summary>
		/// Describes a shape or model object library.
		/// </summary>
		private class Library {

			public Library(Assembly assembly)
				: this(assembly, true) {
			}


			public Library(Assembly assembly, bool unloadOnClose) {
				if (assembly == null) throw new ArgumentNullException(nameof(assembly));
				this._assembly = assembly;
				this._name = null;
				this._unloadOnClose = unloadOnClose;
			}


			/// <summary>
			/// User-defined name to identify the library.
			/// </summary>
			public string Name {
				get { return _name; }
				set { _name = value; }
			}


			/// <summary>
			/// Specifies the assembly used for the library.
			/// </summary>
			public Assembly Assembly {
				get { return _assembly; }
				set { _assembly = value; }
			}


			/// <summary>
			/// Specifies the preferred repository version for the library.
			/// </summary>
			public int PreferredRepositoryVersion {
				get { return _preferredRepositoryVersion; }
				set { _preferredRepositoryVersion = value; }
			}


			/// <summary>
			/// Specifies whether the library will be unloaded when the project is closed.
			/// </summary>
			public bool UnloadOnClose {
				get { return _unloadOnClose; }
			}


			#region Fields

			private string _name;
			private Assembly _assembly;
			private int _preferredRepositoryVersion;
			private bool _unloadOnClose = true;

			#endregion
		}

		#endregion


		/// <ToBeCompleted></ToBeCompleted>
		public const string NShapeLibraryInitializerClassName = "NShapeLibraryInitializer";
		/// <ToBeCompleted></ToBeCompleted>
		public const string InitializeMethodName = "Initialize";

		// Supported repository versions of the Core library:
		internal const int LastSupportedSaveVersion = 7;
		internal const int LastSupportedLoadVersion = 7;
		// NShape 1.0.0 was released using repository version 2.
		internal const int FirstSupportedSaveVersion = 2;
		internal const int FirstSupportedLoadVersion = 2;

		/// <summary>Library name of the core library</summary>
		internal const string CoreLibraryName = "Core";
		private const string ShapeGroupName = "ShapeGroup";
		private const string GenericModelObjectName = "GenericModelObject";

		#region Fields

		private const string _entityTypeName = "Project";
		private string[] _attributeNames = new string[] { "Name", "Date", "DesignId" };

		// -- Constituting Sub-Objects --
		private ShapeTypeCollection _shapeTypes = new ShapeTypeCollection();
		private ModelObjectTypeCollection _modelObjectTypes = new ModelObjectTypeCollection();
		private DiagramModelObjectTypeCollection _diagramModelObjectTypes = new DiagramModelObjectTypeCollection();
		private IRepository _repository = null;
		private History _history = null;
		private ISecurityManager _security = new RoleBasedSecurityManager();

		// -- Properties --
		private string _name = null;
		private bool _autoCreateTemplates = true;
		private bool _autoLoadLibraries = false;
		private ProjectSettings _settings = null;
		private Model _model = null;
		private IList<string> _librarySearchPaths = new List<string>();

		// -- State --
		// List of actually loaded libraries. Different from ProjectSettings.libraries 
		// because there it is the required libraries (including required repository version).
		private List<Library> _libraries = new List<Library>();

		// Set to current library during InitializingLibrary method.
		private Library _initializingLibrary;

		// Indicates that a new library is currently being registered (in contrast to
		// one that has to be loaded during opening a project).
		private bool _addingLibrary;

		// -- Helpers --
		private object[] _registerArgs;

		#endregion

	}


	/// <summary>
	/// Provides information for the LibraryLoaded event.
	/// </summary>
	/// <status>reviewed</status>
	public class LibraryLoadedEventArgs : EventArgs {

		/// <ToBeCompleted></ToBeCompleted>
		public LibraryLoadedEventArgs(string libraryName) {
			if (libraryName == null) throw new ArgumentNullException(nameof(libraryName));
			LibraryName = libraryName;
		}

		/// <ToBeCompleted></ToBeCompleted>
		public string LibraryName {
			get;
			private set;
		}

	}

}
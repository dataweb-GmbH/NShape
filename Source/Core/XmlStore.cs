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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;

using Dataweb.NShape.Advanced;
using System.Drawing.Design;


namespace Dataweb.NShape {

	/// <summary>
	/// Uses an XML file as the data store.
	/// </summary>
	/// <remarks>XML capability should go to CachedRepository or even higher. So we 
	/// can create the XML document from any cache. Only responsibilities left 
	/// here, is how ids are generated.</remarks>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(XmlStore), "XmlStore.bmp")]
	public class XmlStore : Store {

		/// <summary>Defines the default extension of the backup file created when setting BackupFileGenerationMode to "BakFile".</summary>
		public const string DefaultBackupFileExtension = ".bak";


		/// <summary>Defines the behavior of automatic backup file generation.</summary>
		public enum BackupFileGenerationMode {
			/// <summary>No backup file(s) will be created when saving contents to an existing XML repository file.</summary>
			None,
			/// <summary>Backup file(s) will be created when saving contents to an existing XML repository file.</summary>
			BakFile 
		}


		/// <summary>Defines how images are stored.</summary>
		public enum ImageFileLocation {
			/// <summary>Images will be saved to files in a seperate directory.</summary>
			Directory,
			/// <summary>Images will be embedded into the XML code as Base64 string.</summary>
			Embedded
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.XmlStore" />.
		/// </summary>
		public XmlStore() {
			this.DirectoryName = string.Empty;
			this.FileExtension = ".xml";
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.XmlStore" />.
		/// </summary>
		public XmlStore(string directoryName, string fileExtension)
			: this(directoryName, fileExtension, false) {
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.XmlStore" />.
		/// </summary>
		public XmlStore(string directoryName, string fileExtension, bool lazyLoading) {
			if (directoryName == null) throw new ArgumentNullException("directoryName");
			if (fileExtension == null) throw new ArgumentNullException("fileExtension");
			this.DirectoryName = directoryName;
			this.FileExtension = fileExtension;
			this.LazyLoading = lazyLoading;
		}


		/// <summary>
		/// Initializes a new instance of <see cref="T:Dataweb.NShape.XmlStore" />.
		/// </summary>
		public XmlStore(Stream stream) {
			if (stream == null) throw new ArgumentNullException("stream");
			this.Stream = stream;
		}


		#region [Public] Properties

		/// <summary>
		/// Specifies the version of the assembly containing the component.
		/// </summary>
		[CategoryNShape()]
		public string ProductVersion {
			get { return this.GetType().Assembly.GetName().Version.ToString(); }
		}


		/// <override></override>
		[Browsable(false)]
		public override bool CanModifyVersion {
			get { return true; }
		}


		/// <summary>
		/// Gets or sets whether diagrams should be loaded without content (lazy loading).
		/// </summary>
		/// <remarks>
		/// When using lazy loading, the shapes must be loaded explicitly by calling Repository.GetDiagramShapes.
		/// Stream based XmlStores do not support lazy loading.
		/// </remarks>
		[CategoryBehavior]
		[DefaultValue(false)]
		public bool LazyLoading {
			get { return usePartialLoading; }
			set { usePartialLoading = value; }
		}


		/// <summary>
		/// Defines the directory, where the NShape project is stored.
		/// </summary>
		[CategoryNShape()]
		[Editor("Dataweb.NShape.WinFormsUI.DirectoryUITypeEditor, Dataweb.NShape.WinFormsUI", typeof(UITypeEditor))]
		public string DirectoryName {
			get { return directoryName; }
			set {
				if (value == null) throw new ArgumentNullException("DirectoryName");
				directoryName = value;
			}
		}


		/// <summary>
		/// Specifies the desired extension for the project file.
		/// </summary>
		[CategoryNShape()]
		public string FileExtension {
			get { return fileExtension; }
			set {
				if (value == null) throw new ArgumentNullException("FileExtension");
				if (value.StartsWith("*")) value = value.Substring(1);
				fileExtension = value;
				if (!string.IsNullOrEmpty(fileExtension) && fileExtension[0] != '.')
					fileExtension = '.' + fileExtension;
			}
		}


		/// <summary>
		/// Defines the file name without extension, where the NShape designs are stored.
		/// </summary>
		[CategoryNShape()]
		public string DesignFileName {
			get { return designFileName; }
			set { designFileName = value; }
		}


		/// <summary>
		/// Specifies whether a backup for the current contents should be created when saving changes to file.
		/// </summary>
		[CategoryNShape()]
		[DefaultValue(BackupFileGenerationMode.BakFile)]
		public BackupFileGenerationMode BackupGenerationMode {
			get { return backupGenerationMode; }
			set { backupGenerationMode = value; }
		}


		/// <summary>
		/// Specifies the file extension of automatically created backup copies. 
		/// The BackupFileExtension will be appended to the existing file extension, e.g. "Project1.nspj.bak".
		/// </summary>
		[CategoryNShape()]
		[DefaultValue(DefaultBackupFileExtension)]
		public string BackupFileExtension {
			get { return backupFileExtension; }
			set {
				if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
				backupFileExtension = value; 
			}
		}


		/// <summary>
		/// Retrieves the file path of the project xml file.
		/// </summary>
		[Browsable(false)]
		public string ProjectFilePath {
			get {
				string result = Path.Combine(directoryName, projectName);
				if (!string.IsNullOrEmpty(fileExtension)) result += fileExtension;
				return result;
			}
			set {
				if (string.IsNullOrEmpty(value)) {
					directoryName = null;
					projectName = null;
				} else {
					directoryName = Path.GetDirectoryName(value);
					projectName = Path.GetFileNameWithoutExtension(value);
					fileExtension = Path.GetExtension(value);
				}
			}
		}


		/// <summary>
		/// Retrieves the file path of the design xml file.
		/// </summary>
		[Browsable(false)]
		public string DesignFilePath {
			get {
				string result;
				if (string.IsNullOrEmpty(directoryName)) {
					result = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					result += Path.DirectorySeparatorChar + "Dataweb" + Path.DirectorySeparatorChar + "NShape";
				} else result = directoryName;

				if (string.IsNullOrEmpty(DesignFileName)) throw new InvalidOperationException("Project name for XML repository not set.");
				result += Path.DirectorySeparatorChar;
				result += DesignFileName;
				return result;
			}
		}


		/// <summary>
		/// Specifies whether images are stored in files or 'inline' as base 64 string.
		/// </summary>
		[CategoryNShape()]
		[DefaultValue(ImageFileLocation.Embedded)]
		public ImageFileLocation ImageLocation {
			get { return storeImageLocation ?? defaultImageLocation; }
			set {
				// When setting the imageLocation explicitly, override the setting read from the store.
				storeImageLocation = null;
				defaultImageLocation = value; 
			}
		}

		#endregion


		#region [Public] Store Implementation

		/// <override></override>
		public override string ProjectName {
			get { return projectName; }
			set {
				if (value == null) projectName = string.Empty;
				else projectName = value;
				// Clear image directory
				imageDirectory = null;
			}
		}


		/// <override></override>
		public override bool Exists() {
			return File.Exists(ProjectFilePath) || Stream != null;
		}


		/// <override></override>
		public override void ReadVersion(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			version = DoReadVersion(cache, (Stream != null));
			cache.SetRepositoryBaseVersion(version);
		}


		/// <override></override>
		public override void Create(IStoreCache cache) {
			DoOpen(cache, true);
		}


		/// <override></override>
		public override void Open(IStoreCache cache) {
			DoOpen(cache, false);
		}


		/// <override></override>
		public override void Close(IStoreCache storeCache) {
			base.Close(storeCache);
			isOpen = false;
			isOpenComplete = false;
			CloseReader();
			CloseWriter();
			storeImageLocation = null;
		}


		/// <override></override>
		public override void Erase() {
			if (BackupGenerationMode != BackupFileGenerationMode.None)
				CreateBackupFiles(ProjectFilePath);

			// Delete project file and (optional) image directory
			// (Check for existance because creating backup file will not copy but rename the original files)
			if (File.Exists(ProjectFilePath)) {
				File.Delete(ProjectFilePath);
				if (Directory.Exists(ImageDirectory))
					Directory.Delete(ImageDirectory, true);
			}
		}


		/// <override></override>
		public override void LoadProjects(IStoreCache cache, IEntityType entityType, params object[] parameters) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (entityType == null) throw new ArgumentNullException("entityType");
			// Do nothing. OpenComplete must be called after the libraries have been loaded.
		}


		/// <override></override>
		public override void LoadDesigns(IStoreCache cache, object projectId) {
			if (cache == null) throw new ArgumentNullException("cache");
			// Do nothing. OpenComplete must be called after the libraries have been loaded.
		}


		/// <override></override>
		public override void LoadModel(IStoreCache cache, object projectId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadTemplates(IStoreCache cache, object projectId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadDiagrams(IStoreCache cache, object projectId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadDiagramShapes(IStoreCache cache, Diagram diagram) {
			if (isOpen) {
				if (LazyLoading) {
					// If the diagram shapes were loaded before, do nothing
					if (diagramsToLoad != null && !diagramsToLoad.Contains(diagram))
						return;
					// Open the store stream
					OpenReader(cache, GetReadStream());

					// Find first diagram
					IEntityType diagramEntityType = cache.FindEntityTypeByName(Diagram.EntityTypeName);
					string diagramTag = GetElementTag(diagramEntityType);
					XmlSkipToElement(diagramTag);

					// Find given diagram (according to its Id)
					string diagramIdStr = ((IEntity)diagram).Id.ToString();
					while (xmlReader.GetAttribute(0) != diagramIdStr)
						xmlReader.ReadToNextSibling(diagramTag);
					xmlReader.ReadToDescendant(shapesTag);

					ReadDiagramShapes(cache, xmlReader, diagram);
					ReadDiagramShapeConnections(cache, xmlReader);

					// Remove the diagram from the list of not-yet-loaded diagrams
					if (diagramsToLoad != null) {
						diagramsToLoad.Remove(diagram);
						if (diagramsToLoad.Count == 0) diagramsToLoad = null;
					}
				} else
					OpenComplete(cache);
			}
		}


		/// <override></override>
		public override void LoadTemplateShapes(IStoreCache cache, object templateId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadChildShapes(IStoreCache cache, object parentShapeId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadTemplateModelObjects(IStoreCache cache, object templateId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadModelModelObjects(IStoreCache cache, object modelId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override void LoadChildModelObjects(IStoreCache cache, object parentModelObjectId) {
			if (isOpen) OpenComplete(cache);
		}


		/// <override></override>
		public override bool CheckStyleInUse(IStoreCache cache, object styleId) {
			// XML store does not support partial loading, so we have nothing to do here
			return false;
		}


		/// <override></override>
		public override bool CheckTemplateInUse(IStoreCache cache, object templateId) {
			// XML store does not support partial loading, so we have nothing to do here
			return false;
		}


		/// <override></override>
		public override bool CheckModelObjectInUse(IStoreCache cache, object modelObjectId) {
			// XML store does not support partial loading, so we have nothing to do here
			return false;
		}


		/// <override></override>
		public override bool CheckShapeTypeInUse(IStoreCache cache, string typeName) {
			// XML store does not support partial loading, so we have nothing to do here
			return false;
		}


		/// <override></override>
		public override void SaveChanges(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (!isOpen) throw new NShapeException("Store is not open.");
			if (string.IsNullOrEmpty(ProjectName)) throw new NShapeException("Project name was not specified.");
			if (diagramsToLoad != null && diagramsToLoad.Count > 0)
				throw new NShapeException("Not all diagram shapes have been loaded. Load all diagram shapes before saving.");

			if (Stream != null)
				SaveChangesToStream(cache);
			else
				SaveChangesToFile(cache);
		}


		private void SaveChangesToFile(IStoreCache cache) {
			// New strategy:
			// * Calculate temporary file names
			// -------------------------
			//		* Open file as stream
			//		* Set stream as current store stream
			//      -------------------------
			//			* Save repository to stream
			//      -------------------------
			//		* Unset stream as current store stream
			//		* Close (dispose) file's stream
			// -------------------------
			// * Restore original file names
			// 
			// * Create backup files (if wanted)
			// * Replace original files with temporary files
			// 
			// * Read store
			
			
			// Calculate temporary file names for project and image directory
			string tempProjectFilePath = GetTemporaryProjectFilePath();
			string tempImageDirectory = CalcImageDirectoryName(tempProjectFilePath);
			try {
				try {
					// Save changes to temporary file
					DoSaveChanges(cache, tempProjectFilePath);
				} finally {
					// Restore image directory (will be changed when calling DoSaveChanges with a path that differs from ProjectFilePath)
					imageDirectory = CalcImageDirectoryName(ProjectFilePath);
				}
				//
				// Backup current project files
				switch (backupGenerationMode) {
					case BackupFileGenerationMode.None:
						Erase();
						break;
					case BackupFileGenerationMode.BakFile:
						CreateBackupFiles(ProjectFilePath);
						break;
					default: throw new InvalidEnumArgumentException();
				}
				// Ensure that the old files do not exist any more before renaming the temporary files
				int retryCnt = 0;
				while (File.Exists(ProjectFilePath) || File.Exists(ImageDirectory)) {
					// Wait a little time for the file system to refresh the changed file entries
					System.Threading.Thread.Sleep(5);
					if (++retryCnt > 100) break;
				}
				//
				// Replace original files with temporary files (by renaming them)
				Debug.Assert(File.Exists(tempProjectFilePath));
				File.Move(tempProjectFilePath, ProjectFilePath);
				if (Directory.Exists(tempImageDirectory))
					Directory.Move(tempImageDirectory, ImageDirectory);
			} catch (Exception exc) {
				Debug.Print(exc.Message);
				throw;
			} finally {
				// Clean up temporary files
				if (File.Exists(tempProjectFilePath))
					File.Delete(tempProjectFilePath);
				if (Directory.Exists(tempImageDirectory))
					Directory.Delete(tempImageDirectory, true);
			}
		}


		/// <summary>Saves the contents of the given store cache to the given stream as UTF8 encoded XML string</summary>
		private void SaveChangesToStream(IStoreCache cache) {
		    if (cache == null) throw new ArgumentNullException("cache");
			if (Stream == null) throw new NShapeException("Cannot save to stream: Stream does not exist.");
		    //if (!stream.CanSeek) throw new ArgumentException("The provided stream does not support seeking.");
		    if (!isOpen) throw new NShapeException("Store is not open.");
		    if (string.IsNullOrEmpty(ProjectName)) throw new NShapeException("Project name was not specified.");
		    try {
		        // Save changes to stream
		        DoSaveChanges(cache, Stream);
		    } catch (Exception exc) {
		        Debug.Print(exc.Message);
		        throw;
		    } finally {
		    }
		}


		/// <override></override>
		protected internal override int Version {
			get { return version; }
			set { version = value; }
		}

		#endregion


		#region [Protected] Methods: Implementation

		/// <summary>
		/// Loads connections between <see cref="T:Dataweb.NShape.Advanced.Shapes" /> instances.
		/// </summary>
		protected void LoadShapeConnections(IStoreCache cache, Diagram diagram) {
			Debug.Assert(isOpen);
			OpenComplete(cache);
		}


		/// <summary>
		/// Reads the save version from the XML file.
		/// </summary>
		protected int DoReadVersion(IStoreCache cache, bool keepOpen) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (isOpen) throw new InvalidOperationException(string.Format("{0} is already open.", GetType().Name));
			// Open the stream (if existing)
			OpenReader(cache, GetReadStream());
			try {
				xmlReader.MoveToContent();
				if (xmlReader.Name != rootTag || !xmlReader.HasAttributes)
					throw new NShapeException("XML file '{0}' is not a valid NShape project file.", ProjectFilePath);
				return int.Parse(xmlReader.GetAttribute(0));
			} finally {
				if (!keepOpen) CloseReader();
			}
		}
		
		
		/// <summary>
		/// Opens the XML file and read the project settings. The rest of the file is loaded on the first request of data.
		/// </summary>
		protected void DoOpen(IStoreCache cache, bool create) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (isOpen) throw new InvalidOperationException(string.Format("{0} is already open.", GetType().Name));
			// Reset the image location read from store
			storeImageLocation = null;
			if (create) {
				// Update version number (in case the store was attached after creating the project)
				Version = cache.RepositoryBaseVersion;
				//
				// Nothing else to do. Data is kept in memory until SaveChanges is called.
				//
				isOpenComplete = true;
			} else {
				try {
					if (!isOpen) {
						int fileVersion = DoReadVersion(cache, true);
						Debug.Assert(fileVersion == version);
					}
					XmlSkipStartElement(rootTag);
					// We only read the designs and the project here. This gives the application
					// a chance to load required libraries. Templates and diagramControllers are then loaded
					// in OpenComplete.
					ReadProjectSettings(cache, xmlReader);
				} catch (Exception) {
					CloseReader();
					throw;
				}
			}
			isOpen = true;
		}


		/// <summary>
		/// Calculates the directory for the images given the complete file path.
		/// </summary>
		protected string CalcImageDirectoryName() {
			return CalcImageDirectoryName(StoreFilePath);
		}


		/// <summary>
		/// Create a path with slashes ('/') instead of back slashes ('\') as path delimiter in order to ensure that xml repositories can be loaded from both, Windows and Linux operating systems.
		/// </summary>
		protected string UnifyPath(string path) {
			Uri resultUri;
			if (Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out resultUri))
				return Uri.UnescapeDataString(resultUri.AbsolutePath);
			return path;
		}


		/// <summary>
		/// Indicates whether the given id is interpreted as empty.
		/// </summary>
		protected internal bool IsNullOrEmpty<T>(object id) {
			if (object.ReferenceEquals(id, null)) return true;
			else return object.Equals(id, default(T));
		}


		/// <summary>
		/// Indicates whether the given ids are considered as equal.
		/// </summary>
		protected internal bool AreEqual(object id1, object id2) {
			if (object.ReferenceEquals(id1, id2)) return true;
			else return object.Equals(id1, id2);
		}

		#endregion


		#region [Protected] Type: XmlStoreReader 

		/// <summary>
		/// Implements a cache repositoryReader for XML.
		/// </summary>
		protected class XmlStoreReader : RepositoryReader {

			/// <summary>
			/// Initializes a new instance of <see cref="T:Dataweb.NShape.XmlStore.XmlStoreReader" />.
			/// </summary>
			public XmlStoreReader(XmlReader xmlReader, XmlStore store, IStoreCache cache)
				: base(cache) {
				if (xmlReader == null) throw new ArgumentNullException("xmlReader");
				if (store == null) throw new ArgumentNullException("store");
				this.store = store;
				this.xmlReader = xmlReader;
			}


			#region [Public] Methods: RepositoryReader Implementation

			/// <override></override>
			public override void BeginReadInnerObjects() {
				if (propertyInfos == null) throw new NShapeException("Property EntityType is not set.");
				if (innerObjectsReader != null) throw new InvalidOperationException("EndReadInnerObjects was not called.");
				++PropertyIndex;
				string elementName = Cache.CalculateElementName(propertyInfos[PropertyIndex].Name);
				if (!xmlReader.IsStartElement(elementName)) throw new InvalidOperationException(string.Format("Element '{0}' expected.", elementName));
				if (!xmlReader.IsEmptyElement) xmlReader.Read();
				innerObjectsReader = new XmlStoreReader(xmlReader, store, Cache);
				// Set a marker to detect wrong call sequence
				InnerObjectsReader.PropertyIndex = int.MinValue;
				InnerObjectsReader.ResetFieldReading(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).PropertyDefinitions);
			}


			/// <override></override>
			public override void EndReadInnerObjects() {
				if (innerObjectsReader == null) throw new InvalidOperationException("BeginReadInnerObjects was not called.");
				Debug.Assert(xmlReader.IsEmptyElement || xmlReader.NodeType == XmlNodeType.EndElement);
				xmlReader.Read(); // read end tag of collection
				innerObjectsReader = null;
			}


			/// <override></override>
			public override void EndReadInnerObject() {
				xmlReader.Read(); // Read out of the attributes
				// Previous version: XmlSkipEndElement(store.CalculateElementName(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).Name));
				InnerObjectsReader.PropertyIndex = int.MinValue;
			}


			/// <override></override>
			protected override bool DoReadBool() {
				bool result = bool.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			/// <override></override>
			protected override byte DoReadByte() {
				byte result = byte.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			/// <override></override>
			protected override short DoReadInt16() {
				short result = short.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			/// <override></override>
			protected override int DoReadInt32() {
				int result = int.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			/// <override></override>
			protected override long DoReadInt64() {
				long result = long.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			/// <override></override>
			protected override float DoReadFloat() {
				float result = float.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			/// <override></override>
			protected override double DoReadDouble() {
				double result = double.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			/// <override></override>
			protected override char DoReadChar() {
				char result = char.Parse(xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset));
				xmlReader.MoveToNextAttribute();
				return result;
			}


			/// <override></override>
			protected override string DoReadString() {
				string result = null;
				// Backwards compatibility workaround, see comment in "GetPropertyDefinitions" of class "Template".
				if (store.version == 2 && store.IsVersion2TemplateDefinition(propertyInfos)) {
					// If repository version is 2, we check the attribute name against the expected property name.
					// In case of "Title" and "Description", we are fault-tolerant and return null if the attribute 
					// name does not match the property name in order to maintain + restore backwards compatibility.
					string attrName = xmlReader.Name.ToLower();
					if (attrName == "title" || attrName == "description") {
						if (propertyInfos[PropertyIndex + compatibilityOffset] is EntityFieldDefinition
							&& string.Compare(propertyInfos[PropertyIndex + compatibilityOffset].ElementName, xmlReader.Name, true) != 0) {
							++ compatibilityOffset;
							return null;
						}
					}
				}
				result = xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset - compatibilityOffset);
				xmlReader.MoveToNextAttribute();
				return result;
			}


			/// <override></override>
			protected override DateTime DoReadDate() {
				System.Globalization.DateTimeFormatInfo info = new System.Globalization.DateTimeFormatInfo();
				string attrValue = xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset);
				DateTime dateTime;
				if (!DateTime.TryParseExact(attrValue, datetimeFormat, null, System.Globalization.DateTimeStyles.AssumeUniversal, out dateTime))
					dateTime = Convert.ToDateTime(attrValue);	// ToDo: This is for compatibility with older file versions - Remove later
				xmlReader.MoveToNextAttribute();
				return dateTime.ToLocalTime();
			}


			/// <override></override>
			protected override Image DoReadImage() {
				Image result = null;
				string fileName = xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset);
				if (!string.IsNullOrEmpty(fileName))
					result = store.LoadImage(fileName);
				xmlReader.MoveToNextAttribute();
				return result;
			}

			#endregion


			#region [Protected] Methods: Implementation

			/// <override></override>
			protected internal override object ReadId() {
				++PropertyIndex;
				ValidatePropertyIndex();
				object result = null;
				string idStr = xmlReader.GetAttribute(PropertyIndex + xmlAttributeOffset);
				if (!string.IsNullOrEmpty(idStr)) {
					result = ParseId(idStr);
					if (result.Equals(Guid.Empty))
						result = null;
				}
				xmlReader.MoveToNextAttribute();
				return result;
			}


			// Assumes that the current tag is of the expected type or the end element of 
			// the enclosing element. Moves to the first attribute for subsequent reading.
			/// <override></override>
			protected internal override bool DoBeginObject() {
				// If the enclosing element is empty, it is still the current tag and indicates 
				// an empty inner objects list.
				if (xmlReader.NodeType == XmlNodeType.EndElement || (xmlReader.IsEmptyElement && !xmlReader.HasAttributes))
					return false;
				else {
					xmlReader.MoveToFirstAttribute();
					PropertyIndex = -2;
					compatibilityOffset = 0;
					return true;
				}
			}


			/// <override></override>
			protected internal override void DoEndObject() {
				// Read over the end tag of the object.
			}


			/// <override></override>
			protected override void ValidatePropertyIndex() {
				base.ValidatePropertyIndex();
				if (PropertyIndex + xmlAttributeOffset >= xmlReader.AttributeCount) {
					// Check if these are the properties of the Template class (repository verison 2)
					if (store.Version == 2 && store.IsVersion2TemplateDefinition(propertyInfos)) {
						// If it is, we regardtitle and description as optional.
						if (xmlReader.AttributeCount >= 2 && xmlReader.AttributeCount <= 4)
							return;
					}
					throw new NShapeException("An entity tries to read {0} properties although there are only {1} properties stored in the repository. Check whether the repository is valid and/or up-to-date.", PropertyIndex + xmlAttributeOffset + 1, xmlReader.AttributeCount);
				}
			}


			internal override void ResetFieldReading(IEnumerable<EntityPropertyDefinition> propertyInfos) {
				base.ResetFieldReading(propertyInfos);
				compatibilityOffset = 0;
			}

			#endregion


			private XmlStoreReader InnerObjectsReader {
				get { return (XmlStoreReader)innerObjectsReader; }
			}


			#region Fields

			// There is always one more XML attribute (the id) than there are properties 
			// in the entity type.
			private const int xmlAttributeOffset = 1;
			
			private XmlStore store;
			private XmlReader xmlReader;
			// For compatibility mode. See comment in Template.GetPropertyDefinitions
			private int compatibilityOffset = 0;

			#endregion
		}

		#endregion


		#region [Protected] Types: XmlStoreWriters

		/// <summary>
		/// Writes fields and inner objects to XML.
		/// </summary>
		protected class XmlStoreWriter : RepositoryWriter {

			/// <summary>
			/// Initializes a new instance of <see cref="T:Dataweb.NShape.XmlStore.XmlStoreWriter" />.
			/// </summary>
			public XmlStoreWriter(XmlWriter xmlWriter, XmlStore xmlStore, IStoreCache cache)
				: base(cache) {
				if (xmlWriter == null) throw new ArgumentNullException("xmlWriter");
				if (xmlStore == null) throw new ArgumentNullException("xmlStore");
				this.xmlStore = xmlStore;
				this.xmlWriter = xmlWriter;
			}


			#region RepositoryWriter Members

			/// <override></override>
			protected override void DoWriteId(object id) {
				++PropertyIndex;
				string fieldName = GetXmlAttributeName(PropertyIndex);
				if (id == null)
					XmlAddAttributeString(fieldName, Guid.Empty.ToString());
				else
					XmlAddAttributeString(fieldName, id.ToString());
			}


			/// <override></override>
			protected override void DoWriteBool(bool value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			/// <override></override>
			protected override void DoWriteByte(byte value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			/// <override></override>
			protected override void DoWriteInt16(short value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			/// <override></override>
			protected override void DoWriteInt32(int value) {
				++PropertyIndex;
				string fieldName = GetXmlAttributeName(PropertyIndex);
				XmlAddAttributeString(fieldName, value.ToString());
			}


			/// <override></override>
			protected override void DoWriteInt64(long value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			/// <override></override>
			protected override void DoWriteFloat(float value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			/// <override></override>
			protected override void DoWriteDouble(double value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			/// <override></override>
			protected override void DoWriteChar(char value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToString());
			}


			/// <override></override>
			protected override void DoWriteString(string value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), (value == null) ? string.Empty : value);
			}


			/// <override></override>
			protected override void DoWriteDate(DateTime value) {
				++PropertyIndex;
				XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), value.ToUniversalTime().ToString(datetimeFormat));
			}


			/// <override></override>
			protected override void DoWriteImage(Image image) {
				++PropertyIndex;
				if (image == null) {
					XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), "");
				} else {
					string fileName = xmlStore.SaveImageFile(Entity, image);
					XmlAddAttributeString(GetXmlAttributeName(PropertyIndex), fileName);
				}
			}


			/// <override></override>
			protected override void DoBeginWriteInnerObjects() {
				// Sanity checks
				if (propertyInfos == null)
					throw new InvalidOperationException("EntityType is not set.");
				if (Entity == null)
					throw new InvalidOperationException("InnerObject's parent object is not set to an instance of an object.");
				if (!(propertyInfos[PropertyIndex + 1] is EntityInnerObjectsDefinition))
					throw new InvalidOperationException(string.Format("The current property info for '{0}' does not refer to inner objects. Check whether the writing methods fit the PropertyInfos property.", propertyInfos[PropertyIndex + 1]));
				// Advance to next inner objects property
				++PropertyIndex;
				innerObjectsWriter = new XmlStoreWriter(xmlWriter, xmlStore, Cache);
				innerObjectsWriter.Reset(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).PropertyDefinitions);
				xmlWriter.WriteStartElement(Cache.CalculateElementName(propertyInfos[PropertyIndex].Name));
			}


			/// <override></override>
			protected override void DoEndWriteInnerObjects() {
				xmlWriter.WriteEndElement();
			}


			/// <override></override>
			protected override void DoBeginWriteInnerObject() {
				Debug.Assert(Entity != null && innerObjectsWriter != null);
				xmlWriter.WriteStartElement(Cache.CalculateElementName(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).EntityTypeName));
				innerObjectsWriter.Prepare(null);
				// Skip the property index for the id since inner objects do not have one.
				++InnerObjectsWriter.PropertyIndex;
			}


			/// <override></override>
			protected override void DoEndWriteInnerObject() {
				Debug.Assert(Entity != null && innerObjectsWriter != null);
				xmlWriter.WriteEndElement();
			}


			/// <override></override>
			protected override void DoDeleteInnerObjects() {
				throw new NotImplementedException();
			}


			/// <override></override>
			protected internal override void Prepare(IEntity entity) {
				base.Prepare(entity);
			}

			#endregion


			#region [Private] Methods

			/// <summary>
			/// Gets an XML compatible attribute name for the property definition at the specified index.
			/// </summary>
			private string GetXmlAttributeName(int propertyIndex) {
				/* Not required for inner objects
				 * if (Entity == null) 
					throw new NShapeException("Persistable object to store is not set. Please assign an IEntity object to the property Object before calling a save method.");*/
				if (propertyInfos == null)
					throw new NShapeException("EntityType is not set. Please assign an EntityType to the property EntityType before calling a save method.");
				return propertyIndex == -1 ? "id" : propertyInfos[propertyIndex].ElementName;
			}


			/// <summary>
			/// Writes an attribute with the given name/value to the current XML node.
			/// </summary>
			private void XmlAddAttributeString(string name, string value) {
				xmlWriter.WriteAttributeString(name, value);
			}


			private XmlStoreWriter InnerObjectsWriter {
				get { return (XmlStoreWriter)innerObjectsWriter; }
			}

			#endregion


			// Fields
			private XmlWriter xmlWriter;
			private XmlStore xmlStore;
		}


		/// <summary>
		/// Writes images as Base64 string to the given XML writer.
		/// This repository writer is used for collecting all images fom all entities without saving anything else but images.
		/// </summary>
		protected class XmlEmbeddedImageWriter : RepositoryWriter {

			/// <summary>
			/// Initializes a new instance of <see cref="T:Dataweb.NShape.XmlStore.XmlStoreWriter" />.
			/// </summary>
			public XmlEmbeddedImageWriter(XmlWriter xmlWriter, XmlStore xmlStore, IStoreCache cache)
				: base(cache) {
				if (xmlWriter == null) throw new ArgumentNullException("xmlWriter");
				if (xmlStore == null) throw new ArgumentNullException("xmlStore");
				this.xmlStore = xmlStore;
				this.xmlWriter = xmlWriter;
			}


			#region RepositoryWriter Members

			/// <override></override>
			protected override void DoWriteId(object id) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteBool(bool value) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteByte(byte value) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteInt16(short value) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteInt32(int value) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteInt64(long value) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteFloat(float value) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteDouble(double value) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteChar(char value) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteString(string value) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteDate(DateTime value) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteImage(Image image) {
				++PropertyIndex;
				if (image == null) {
					// Nothing to do
				} else {
					xmlStore.SaveEmbeddedImage(Entity, image);
				}
			}


			/// <override></override>
			protected override void DoWriteModelObject(IModelObject modelObject) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteStyle(IStyle style) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoWriteTemplate(Template template) {
				SkipProperty();
			}


			/// <override></override>
			protected override void DoBeginWriteInnerObjects() {
				// Sanity checks
				if (propertyInfos == null)
					throw new InvalidOperationException("EntityType is not set.");
				if (Entity == null)
					throw new InvalidOperationException("InnerObject's parent object is not set to an instance of an object.");
				if (!(propertyInfos[PropertyIndex + 1] is EntityInnerObjectsDefinition))
					throw new InvalidOperationException(string.Format("The current property info for '{0}' does not refer to inner objects. Check whether the writing methods fit the PropertyInfos property.", propertyInfos[PropertyIndex + 1]));
				// Advance to next inner objects property
				++PropertyIndex;
				innerObjectsWriter = new XmlEmbeddedImageWriter(xmlWriter, xmlStore, Cache);
				innerObjectsWriter.Reset(((EntityInnerObjectsDefinition)propertyInfos[PropertyIndex]).PropertyDefinitions);
			}


			/// <override></override>
			protected override void DoEndWriteInnerObjects() {
				// Nothing to do here
			}


			/// <override></override>
			protected override void DoBeginWriteInnerObject() {
				Debug.Assert(Entity != null && innerObjectsWriter != null);
				innerObjectsWriter.Prepare(null);
				// Skip the property index for the id since inner objects do not have one.
				++InnerObjectsWriter.PropertyIndex;
			}


			/// <override></override>
			protected override void DoEndWriteInnerObject() {
				Debug.Assert(Entity != null && innerObjectsWriter != null);
				// Nothing else to do here
			}


			/// <override></override>
			protected override void DoDeleteInnerObjects() {
				throw new NotImplementedException();
			}


			/// <override></override>
			protected internal override void Prepare(IEntity entity) {
				base.Prepare(entity);
			}

			#endregion


			#region [Private] Methods and Fields

			private void SkipProperty() {
				// Nothing to do: Advance to the next property
				++PropertyIndex;
			}


			private XmlEmbeddedImageWriter InnerObjectsWriter {
				get { return (XmlEmbeddedImageWriter)innerObjectsWriter; }
			}


			private const string imageTag = "image";
			private const string entityIdTag = "entity_id";
			private const string propertyNameTag = "property_name";

			private XmlStore xmlStore;
			private XmlWriter xmlWriter;

			#endregion

		}

		#endregion


		internal string ImageDirectory {
			get {
				if (string.IsNullOrEmpty(imageDirectory))
					imageDirectory = CalcImageDirectoryName();
				return imageDirectory;
			}
		}


		/// <summary>
		/// Loads an image from the given image file from the store's image storage.
		/// </summary>
		internal Image LoadImage(string fileName) {
			if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
			
			Image result = null;
			if (embeddedImages != null) {
				// Search image in the list of embedded images...
				if (embeddedImages.TryGetValue(fileName, out result))
					// ... and remove it as soon as it was fetched by its entity.
					embeddedImages.Remove(fileName);
				else { Debug.Fail("Image {0} was not found in the list of embedded images.", fileName); }
				if (embeddedImages.Count == 0) embeddedImages = null;
			} else if (Directory.Exists(ImageDirectory)) {
				// Read image from file
				string filePath = UnifyPath(Path.Combine(ImageDirectory, Path.GetFileName(fileName)));

				// GDI+ only reads the file header and keeps the image file locked for loading the 
				// image data later on demand. 
				// So we have to read the entire image to a buffer and create the image from a 
				// MemoryStream in order to avoid locked (and thus unaccessible) image files.
				byte[] buffer = null;
				using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
					buffer = new byte[fileStream.Length];
					fileStream.Read(buffer, 0, buffer.Length);
					fileStream.Close();
				}
				if (buffer != null) {
					MemoryStream memStream = new MemoryStream(buffer);
					result = Image.FromStream(memStream);
				}
			}
			return result;
		}
		
		
		/// <summary>
		/// Saves the given image to an image file in the store's ImageDirectory.
		/// A file name will be calculated and returned.
		/// </summary>
		private string SaveImageFile(IEntity entity, Image image) {
			// Calculate image name
			string fileName = GetImageFileName(entity, image);

			// Save image to file
			switch (ImageLocation) {
				case ImageFileLocation.Directory:
					SaveImageFile(fileName, image);
					break;
				case ImageFileLocation.Embedded:
					// Return the registered key ("file name") of the previously saved image
					if (embeddedImages != null) {
						int idx = embeddedImages.IndexOfValue(image);
						fileName = embeddedImages.Keys[idx];
						embeddedImages.RemoveAt(idx);
					}
					break;
				default:
					throw new NotSupportedException(string.Format("Unsupported {0} value '{1}'", typeof(ImageFileLocation).GetType().FullName, ImageLocation));
			}
			if (image.Tag is NamedImage)
				((NamedImage)image.Tag).FilePath = fileName;
			return fileName;
		}


		private void SaveImageFile(string fileName, Image image) {
			// Create directory if it does not exist
			if (!Directory.Exists(ImageDirectory))
				Directory.CreateDirectory(ImageDirectory);

			// Build image file path and set file extension
			string filePath = UnifyPath(Path.Combine(ImageDirectory, fileName));
			// Save image file to image directory
			using (Stream stream = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.Write))
				GdiHelpers.SaveImage(image, stream);
		}


		/// <summary>
		/// Saves the given image as base64 encoded string to the store's stream/file.
		/// A file name will be calculated and returned.
		/// </summary>
		private string SaveEmbeddedImage(IEntity entity, Image image) {
			// Calculate image name
			string fileName = GetImageFileName(entity, image);

			// Save image to file
			switch (ImageLocation) {
				case ImageFileLocation.Directory:
					// Do nothing here. Just return the file name
					break;
				case ImageFileLocation.Embedded:
					// Register image file name for embedded images. This is necessary because 
					// unsaved images (which do not have an Id yet) are given a random unique file name.
					if (embeddedImages == null) embeddedImages = new SortedList<string, Image>();
					// When images share a file name, we assume they are equal. 
					// For performance reasons, we do not check this assumption.
					if (!embeddedImages.ContainsKey(fileName))
						embeddedImages.Add(fileName, image);

					SaveEmbeddedImage(fileName, image);
					break;
				default:
					throw new NotSupportedException(string.Format("Unsupported {0} value '{1}'", typeof(ImageFileLocation).GetType().FullName, ImageLocation));
			}
			return fileName;
		}
		
		
		private void SaveEmbeddedImage(string fileName, Image image) {
			// Save the image to a memory stream before embedding the underlying byte array (of 
			// the memory stream) as base64 string.
			using (MemoryStream memStream = new MemoryStream()) {
				GdiHelpers.SaveImage(image, memStream);

				// Write (complete) XML node
				xmlWriter.WriteStartElement(imageTag);
				xmlWriter.WriteAttributeString(fileNameTag, fileName);
				xmlWriter.WriteBase64(memStream.GetBuffer(), 0, (int)memStream.Length);
				xmlWriter.WriteEndElement();
			}
		}


		// This method works, but it has a little side efect: Metafiles grow each time they are saved using this method.
		// So we are using the GdiHelpers' method "SaveImage" which uses the native GDI+ functions.
		private static void SaveImageToStream(Image image, Stream stream) {
			if (image is Metafile) {
				// The MetaFile.Save() method will use the PNG encoder (see MSDN documentation), so the metafile 
				// would be converted into a bitmap. In order to avoid this, we create a new meta file and 
				// copy the original meta file by drawing it to the new one.
				using (Graphics gfx = Graphics.FromHwnd(IntPtr.Zero)) {
					IntPtr hdc = gfx.GetHdc();
					using (Metafile metaFile = new Metafile(stream, hdc)) {
						gfx.ReleaseHdc(hdc);
						using (Graphics metaFileGfx = Graphics.FromImage(metaFile)) {
							Rectangle bounds = Rectangle.Empty;
							bounds.Width = image.Width;
							bounds.Height = image.Height;
							ImageAttributes imgAttribs = GdiHelpers.GetImageAttributes(ImageLayoutMode.Original);
							GdiHelpers.DrawImage(metaFileGfx, image, imgAttribs, ImageLayoutMode.Original, bounds, bounds);
						}
					}
				}
			} else
				image.Save(stream, image.RawFormat);
		}


		// Contains the file path to the current store file (ProjectFilePath or temporary file path)
		// Resets the imageDirectory thus forcing a re-caulation of the image directory path based 
		// on the new store file path.
		private string StoreFilePath {
			get { return storeFilePath ?? ProjectFilePath; }
			set {
				storeFilePath = value;
				imageDirectory = null;
			}
		}


		/// <summary>
		/// Retrieves the file path of the project xml file.
		/// </summary>
		[Browsable(false)]
		private Stream Stream {
			get { return externalStream; }
			set { externalStream = value; }
		}


		internal static object ParseId(string idStr) {
			object result = null;
			if (IsGuidString(idStr))
				result = new Guid(idStr);
			else {
				long idValue;
				if (long.TryParse(idStr, out idValue))
					result = idValue;
			}
			return result;
		}


		internal string GetImageFileName(IEntity entity, Image image) {
			string fileName = null;
			if (image.Tag is NamedImage)
				fileName = Path.GetFileName(((NamedImage)image.Tag).FilePath);
			if (String.IsNullOrEmpty(fileName)) {
				string imageName = image.Tag.ToString();
				fileName = GetImageFileName(entity, image, imageName);
			}
			return fileName;
		}


		internal string GetImageFileName(IEntity entity, Image image, string imageName) {
			if (entity == null) throw new ArgumentNullException("entity");
			try {
				// If the imageName is a path, we extract the file name
				if (!string.IsNullOrEmpty(imageName) && imageName.Contains("\\"))
					imageName = Path.GetFileNameWithoutExtension(imageName); 
			} catch(ArgumentException) { }
			string result = string.Format("{0} ({1})", string.IsNullOrEmpty(imageName) ? "Image" : imageName, (entity.Id == null) ? Guid.NewGuid() : entity.Id);

			if (image.RawFormat.Guid == ImageFormat.Bmp.Guid) result += ".bmp";
			else if (image.RawFormat.Guid == ImageFormat.Emf.Guid) result += ".emf";
			else if (image.RawFormat.Guid == ImageFormat.Exif.Guid) result += ".exif";
			else if (image.RawFormat.Guid == ImageFormat.Gif.Guid) result += ".gif";
			else if (image.RawFormat.Guid == ImageFormat.Icon.Guid) result += ".ico";
			else if (image.RawFormat.Guid == ImageFormat.Jpeg.Guid) result += ".jpeg";
			else if (image.RawFormat.Guid == ImageFormat.MemoryBmp.Guid) result += ".bmp";
			else if (image.RawFormat.Guid == ImageFormat.Png.Guid) result += ".png";
			else if (image.RawFormat.Guid == ImageFormat.Tiff.Guid) result += ".tiff";
			else if (image.RawFormat.Guid == ImageFormat.Wmf.Guid) result += ".wmf";
			else Debug.Fail("Unsupported image format.");

			return result;
		}


		private static bool IsGuidString(string str) {
			const char separator = '-';
			return (str.Length == 36 && (str[8] == separator && str[13] == separator && str[18] == separator && str[23] == separator));
		}


		#region [Private] Methods: Save project files

		private string CalcImageDirectoryName(string filePath) {
			string result = Path.GetDirectoryName(filePath);
			if (string.IsNullOrEmpty(result)) throw new ArgumentException("XML repository file name must be a complete path.");
			result = UnifyPath(Path.Combine(result, Path.GetFileNameWithoutExtension(filePath) + " Images"));
			return result;
		}


		private string GetTemporaryProjectFilePath() {
			return Path.Combine(Path.GetDirectoryName(ProjectFilePath), "~" + Path.GetFileName(ProjectFilePath));
		}


		private void CreateBackupFiles(string filePath) {
			if (File.Exists(filePath)) {
				string projectImageDir = CalcImageDirectoryName(filePath);
				// Calculate backup file names:
				// Add the backup file extension in order to preserve the original file extension
				string backupFilepath = filePath + backupFileExtension;
				string backupImageDir = CalcImageDirectoryName(filePath) + backupFileExtension;
				try {
					// Delete old backup (if existing)
					if (File.Exists(backupFilepath))
						File.Delete(backupFilepath);
					if (Directory.Exists(backupImageDir))
						Directory.Delete(backupImageDir, true);
					// Rename current files
					File.Move(filePath, backupFilepath);
					if (Directory.Exists(projectImageDir))
						Directory.Move(projectImageDir, backupImageDir);
				} catch (Exception) {
					throw;
				}
			}
		}


		private void DoSaveChanges(IStoreCache cache, string filePath) {
			try {
				// Set the current store file path (also adjusts the image directory)
				if (filePath != ProjectFilePath)
					StoreFilePath = filePath;

				// If it is a new project, we must create the file. Otherwise it is already open.
				if (cache.ProjectId == null) {
					CreateFile(filePath, false);
				} else if (cache.LoadedProjects[cache.ProjectId].State == ItemState.Deleted) {
					// First try to delete the file.
					// This ensures that the image directory still exists in case the file cannot be deleted.
					if (File.Exists(filePath)) 
						File.Delete(filePath);
					if (Directory.Exists(ImageDirectory)) 
						Directory.Delete(ImageDirectory, true);
				} else {
					OpenComplete(cache);
					CloseReader();
					// TODO 2: We should keep the file open and clear it here instead of re-creating it.
					CreateFile(filePath, true);
				}
				OpenWriter(cache, GetWriteStream());
				WriteProject(cache);
			} finally {
				storeFilePath = null;
				// Close and reopen to update Windows directory and keep file ownership
				CloseWriter();
			}
		}


		private void DoSaveChanges(IStoreCache cache, Stream stream) {
			if (version != cache.RepositoryBaseVersion) 
				throw new NShapeException("The store's Load/save version '{0}' differs from the repository's load/save version '{1}'.", version, cache.RepositoryBaseVersion);
			try {
				CloseReader();
				OpenWriter(cache, stream);
				WriteProject(cache);
			} finally {
				// Close and reopen to update Windows directory and keep file ownership
				CloseWriter();
			}
		}


		private Stream GetReadStream() {
			Stream storeStream = Stream ?? projectFileStream;
			if (storeStream == null)
				storeStream = projectFileStream = OpenFileStream(StoreFilePath, true);
			return storeStream;
		}


		private Stream GetWriteStream() {
			Stream storeStream = Stream ?? projectFileStream;
			if (storeStream == null)
				storeStream = projectFileStream = OpenFileStream(StoreFilePath, false);
			return storeStream;
		}


		private Stream OpenFileStream(string filePath, bool forReading) {
			if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
			if (forReading && !File.Exists(filePath)) throw new FileNotFoundException("File not found or access denied", filePath);
			// Assign file stream to fileStream field
			return File.Open(filePath, FileMode.OpenOrCreate, 
				forReading ? FileAccess.Read : FileAccess.ReadWrite, 
				forReading ? FileShare.Read : FileShare.None);
		}


		private void CloseFileStream(Stream stream) {
			if (stream == null) throw new ArgumentNullException("stream");
			stream.Close();
			stream.Dispose();
			stream = null;
		}
	
		#endregion


		#region [Private] Methods: Read from XML file

		private void ReadProjectSettings(IStoreCache cache, XmlReader xmlReader) {
			if (!XmlSkipToElement(projectTag)) throw new NShapeException("Invalid XML file. Project tag not found.");
			// Load project data
			IEntityType projectSettingsEntityType = cache.FindEntityTypeByName(ProjectSettings.EntityTypeName);
			ProjectSettings project = (ProjectSettings)projectSettingsEntityType.CreateInstanceForLoading();
			repositoryReader.ResetFieldReading(projectSettingsEntityType.PropertyDefinitions);
			XmlSkipStartElement(projectTag);
			repositoryReader.DoBeginObject();
			object id = repositoryReader.ReadId();
			((IEntity)project).AssignId(id ?? Guid.Empty);

			((IEntity)project).LoadFields(repositoryReader, cache.RepositoryBaseVersion);
			xmlReader.Read(); // read out of attributes
			foreach (EntityPropertyDefinition pi in projectSettingsEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)project).LoadInnerObjects(pi.Name, repositoryReader, cache.RepositoryBaseVersion);
			cache.LoadedProjects.Add(new EntityBucket<ProjectSettings>(project, null, ItemState.Original));
			
			// Load embedded images
			ReadEmbeddedImages(cache, xmlReader);
			
			// Load the project styles
			Design projectDesign = new Design();
			// project design GUID only needed for runtime.
			((IEntity)projectDesign).AssignId(Guid.NewGuid());
			ReadAllStyles(cache, projectDesign);
			cache.LoadedDesigns.Add(new EntityBucket<Design>(projectDesign, project, ItemState.Original));
		}


		private void ReadDesigns(IStoreCache cache, XmlReader xmlReader) {
			IEntityType designEntityType = cache.FindEntityTypeByName(Design.EntityTypeName);
			string designCollectionTag = GetElementCollectionTag(designEntityType);
			XmlSkipToElement(designCollectionTag);
			if (XmlSkipStartElement(designCollectionTag)) {
				do {
					if (XmlSkipStartElement(designEntityType.ElementName)) {
						Design design = (Design)designEntityType.CreateInstanceForLoading();
						repositoryReader.ResetFieldReading(designEntityType.PropertyDefinitions);
						repositoryReader.DoBeginObject();
						((IEntity)design).LoadFields(repositoryReader, designEntityType.RepositoryVersion);
						xmlReader.Read(); // read out of attributes
						foreach (EntityPropertyDefinition pi in designEntityType.PropertyDefinitions)
							if (pi is EntityInnerObjectsDefinition)
								((IEntity)design).LoadInnerObjects(pi.Name, repositoryReader, designEntityType.RepositoryVersion);
						// Global designs are stored with parent id DBNull
						cache.LoadedDesigns.Add(new EntityBucket<Design>(design, null, ItemState.Original));
						design.Clear();

						ReadAllStyles(cache, design);
					}
				} while (xmlReader.ReadToNextSibling(designEntityType.ElementName));
				XmlSkipEndElement(designCollectionTag);
			}
		}


		private void OpenComplete(IStoreCache cache) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (!isOpenComplete) {
				// The position is on the model
				ReadModel(cache, xmlReader);
				ReadTemplates(cache, xmlReader);
				ReadDiagrams(cache, xmlReader);
				XmlSkipEndElement(projectTag);
				XmlSkipEndElement(rootTag);
				isOpenComplete = true;
			}
		}


		private void ReadAllStyles(IStoreCache cache, Design design) {
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(ColorStyle.EntityTypeName));
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(CapStyle.EntityTypeName));
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(CharacterStyle.EntityTypeName));
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(FillStyle.EntityTypeName));
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(LineStyle.EntityTypeName));
			ReadStyles(cache, xmlReader, design, cache.FindEntityTypeByName(ParagraphStyle.EntityTypeName));
			if (string.Compare(xmlReader.Name, shapeStyleTag, StringComparison.InvariantCultureIgnoreCase) == 0) {
				xmlReader.Read();
				XmlReadEndElement(shapeStyleTag);
			}
		}


		private void ReadStyles(IStoreCache cache, XmlReader xmlReader, Design design, IEntityType styleEntityType) {
			if (!xmlReader.IsStartElement(GetElementCollectionTag(styleEntityType)))
				throw new NShapeException("Element '{0}' expected but not found.", GetElementCollectionTag(styleEntityType));
			xmlReader.Read(); // Read over the collection tag
			repositoryReader.ResetFieldReading(styleEntityType.PropertyDefinitions);
			while (string.Compare(xmlReader.Name, GetElementTag(styleEntityType), StringComparison.InvariantCultureIgnoreCase) == 0) {
				Style style = (Style)styleEntityType.CreateInstanceForLoading();
				repositoryReader.DoBeginObject();
				style.AssignId(repositoryReader.ReadId());
				style.LoadFields(repositoryReader, cache.RepositoryBaseVersion);
				xmlReader.Read(); // read out of attributes
				foreach (EntityPropertyDefinition pi in styleEntityType.PropertyDefinitions)
					if (pi is EntityInnerObjectsDefinition)
						style.LoadInnerObjects(pi.Name, repositoryReader, styleEntityType.RepositoryVersion);
				cache.LoadedStyles.Add(new EntityBucket<IStyle>(style, design, ItemState.Original));
				design.AddStyle(style);
				// Reads the end tag of the specific style if present
				XmlReadEndElement(GetElementTag(styleEntityType));
			}
			XmlReadEndElement(GetElementCollectionTag(styleEntityType));
		}


		private void ReadEmbeddedImages(IStoreCache cache, XmlReader xmlReader) {
			if (XmlSkipStartElement(imagesTag)) {
				// The images tag will only be written in case of ImageLocation == Embedded
				storeImageLocation = ImageFileLocation.Embedded;
				if (xmlReader.NodeType != XmlNodeType.EndElement) {
					// Create the storage for the embedded images
					embeddedImages = new SortedList<string, Image>();
					// Read all embedded images
					do {
						if (XmlSkipStartElement(imageTag)) {
							xmlReader.MoveToFirstAttribute();
							// Read file name
							string fileName = xmlReader.GetAttribute(fileNameTag);
							// Read embedded image (do not dispose the memory stream as this would destroy the image)
							xmlReader.Read();
							string base64String = xmlReader.ReadString();
							byte[] buffer = Convert.FromBase64String(base64String);
							MemoryStream memStream = new MemoryStream(buffer);

							embeddedImages.Add(fileName, Image.FromStream(memStream));
						}
					} while (xmlReader.ReadToNextSibling(imageTag));
					
				}
				XmlSkipEndElement(imagesTag);
			}
		}


		private void ReadModel(IStoreCache cache, XmlReader xmlReader) {
			IEntityType modelEntityType = cache.FindEntityTypeByName(Model.EntityTypeName);
			if (XmlSkipStartElement(modelEntityType.ElementName)) {
				if (xmlReader.NodeType != XmlNodeType.EndElement) {
					Model model = (Model)modelEntityType.CreateInstanceForLoading();
					repositoryReader.ResetFieldReading(modelEntityType.PropertyDefinitions);
					repositoryReader.DoBeginObject();
					((IEntity)model).AssignId(repositoryReader.ReadId());
					((IEntity)model).LoadFields(repositoryReader, modelEntityType.RepositoryVersion);
					xmlReader.Read(); // read out of attributes
					foreach (EntityPropertyDefinition pi in modelEntityType.PropertyDefinitions)
						if (pi is EntityInnerObjectsDefinition)
							((IEntity)model).LoadInnerObjects(pi.Name, repositoryReader, modelEntityType.RepositoryVersion);
					// Global models are stored with parent id DBNull
					cache.LoadedModels.Add(new EntityBucket<Model>(model, null, ItemState.Original));

					ReadModelObjects(cache, xmlReader, model);
				}
			}
			XmlSkipEndElement(modelEntityType.ElementName);
		}


		private void ReadModelObjects(IStoreCache cache, XmlReader xmlReader, IEntity owner) {
			if (XmlSkipStartElement(modelObjectsTag)) {
				while (xmlReader.NodeType != XmlNodeType.EndElement)
					ReadModelObject(cache, repositoryReader, owner);
				XmlSkipEndElement(modelObjectsTag);
			}
		}


		private void ReadTemplates(IStoreCache cache, XmlReader xmlReader) {
			IEntityType templateEntityType = cache.FindEntityTypeByName(Template.EntityTypeName);
			string templateCollectionTag = GetElementCollectionTag(templateEntityType);
			string templateTag = GetElementTag(templateEntityType);
			if (!xmlReader.IsStartElement(templateCollectionTag))
				throw new NShapeException("Element '{0}' expected but not found.", templateCollectionTag);
			xmlReader.Read(); // Read over the collection tag
			repositoryReader.ResetFieldReading(templateEntityType.PropertyDefinitions);
			XmlStoreReader innerReader = new XmlStoreReader(xmlReader, this, cache);
			while (xmlReader.IsStartElement(templateTag)) {
				// Read the template
				Template template = (Template)templateEntityType.CreateInstanceForLoading();
				repositoryReader.DoBeginObject();
				template.AssignId(repositoryReader.ReadId());
				template.LoadFields(repositoryReader, templateEntityType.RepositoryVersion);
				xmlReader.Read(); // read out of attributes
				// Read the model object
				IModelObject modelObject = null;
				if (XmlSkipStartElement("no_model")) XmlReadEndElement("no_model");
				else modelObject = ReadModelObject(cache, innerReader, template);
				// Read the shape
				template.Shape = ReadShape(cache, innerReader, null);
				cache.LoadedTemplates.Add(new EntityBucket<Template>(template, cache.Project, ItemState.Original));
				// Read the template's inner objects
				foreach (EntityPropertyDefinition pi in templateEntityType.PropertyDefinitions)
					if (pi is EntityInnerObjectsDefinition)
						template.LoadInnerObjects(pi.Name, repositoryReader, templateEntityType.RepositoryVersion);

				XmlSkipAttributes();
				XmlStoreReader reader = new XmlStoreReader(xmlReader, this, cache);
				ReadModelMappings(cache, reader, template);

				// Read the template end tag
				XmlReadEndElement(GetElementTag(templateEntityType));
			}
			XmlReadEndElement(GetElementCollectionTag(templateEntityType));
		}


		private void ReadModelMappings(IStoreCache cache, XmlStoreReader reader, Template template) {
			if (XmlSkipStartElement(modelmappingsTag)) {
				while (xmlReader.NodeType != XmlNodeType.EndElement) {
					IModelMapping modelMapping = ReadModelMapping(cache, reader, template);
					template.MapProperties(modelMapping);
					//XmlSkipAttributes();
				}
				XmlSkipEndElement(modelmappingsTag);
			}
		}


		private IModelMapping ReadModelMapping(IStoreCache cache, XmlStoreReader reader, IEntity owner) {
			Debug.Assert(xmlReader.NodeType == XmlNodeType.Element);
			string modelMappingTag = xmlReader.Name;
			IEntityType entityType = cache.FindEntityTypeByElementName(modelMappingTag);
			if (entityType == null)
				throw new NShapeException("No shape type found for tag '{0}'.", modelMappingTag);
			XmlSkipStartElement(modelMappingTag);
			IModelMapping modelMapping = (IModelMapping)entityType.CreateInstanceForLoading();
			reader.ResetFieldReading(entityType.PropertyDefinitions);
			reader.DoBeginObject();
			((IEntity)modelMapping).AssignId(reader.ReadId());
			((IEntity)modelMapping).LoadFields(reader, entityType.RepositoryVersion);
			xmlReader.Read(); // Reads out of attributes
			foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)modelMapping).LoadInnerObjects(pi.Name, reader, entityType.RepositoryVersion);
			// Reads the end element
			XmlReadEndElement(modelMappingTag);
			// Insert shape into cache
			cache.LoadedModelMappings.Add(new EntityBucket<IModelMapping>(modelMapping, owner, ItemState.Original));
			return modelMapping;
		}


		private void ReadDiagrams(IStoreCache cache, XmlReader xmlReader) {
			IEntityType diagramEntityType = cache.FindEntityTypeByName(Diagram.EntityTypeName);
			string diagramCollectionTag = GetElementCollectionTag(diagramEntityType);
			string diagramTag = GetElementTag(diagramEntityType);
			XmlSkipToElement(diagramCollectionTag);
			if (XmlSkipStartElement(diagramCollectionTag)) {
				repositoryReader.ResetFieldReading(diagramEntityType.PropertyDefinitions);
				do {
					if (XmlSkipStartElement(diagramTag)) {
						Diagram diagram = (Diagram)diagramEntityType.CreateInstanceForLoading();
						repositoryReader.DoBeginObject();
						((IEntity)diagram).AssignId(repositoryReader.ReadId());
						((IEntity)diagram).LoadFields(repositoryReader, diagramEntityType.RepositoryVersion);
						xmlReader.Read(); // read out of attributes
						foreach (EntityPropertyDefinition pi in diagramEntityType.PropertyDefinitions)
							if (pi is EntityInnerObjectsDefinition)
								((IEntity)diagram).LoadInnerObjects(pi.Name, repositoryReader, diagramEntityType.RepositoryVersion);
						cache.LoadedDiagrams.Add(new EntityBucket<Diagram>(diagram, cache.Project, ItemState.Original));
                        // Skip the diagram shapes in case of partial loading is enabled
						if (!LazyLoading) {
							XmlSkipAttributes();
							XmlStoreReader reader = new XmlStoreReader(xmlReader, this, cache);
							ReadDiagramShapes(cache, xmlReader, diagram);
							ReadDiagramShapeConnections(cache, xmlReader);
							xmlReader.ReadToNextSibling(diagramTag);
						} else {
							if (diagramsToLoad == null) diagramsToLoad = new List<Diagram>();
							diagramsToLoad.Add(diagram);
						}
					}
				} while (XmlSkipToElement(diagramTag));
				XmlSkipEndElement(diagramCollectionTag);
			}
		}


		private void ReadDiagramShapes(IStoreCache cache, XmlReader xmlReader, Diagram diagram) {
			if (XmlSkipStartElement(shapesTag)) {
				XmlStoreReader storeReader = new XmlStoreReader(xmlReader, this, cache);
				while (xmlReader.NodeType != XmlNodeType.EndElement) {
					Shape shape = ReadShape(cache, storeReader, diagram);
					diagram.Shapes.Add(shape);
					diagram.AddShapeToLayers(shape, shape.Layers);	// not really necessary
				}
				XmlSkipEndElement(shapesTag);
			}
		}


		private Shape ReadShape(IStoreCache cache, XmlStoreReader reader, IEntity owner) {
			Debug.Assert(xmlReader.NodeType == XmlNodeType.Element);
			string shapeTag = xmlReader.Name;
			IEntityType shapeEntityType = cache.FindEntityTypeByElementName(shapeTag);
			if (shapeEntityType == null)
				throw new NShapeException("No shape type found for tag '{0}'.", shapeTag);
			XmlSkipStartElement(shapeTag);
			Shape shape = (Shape)shapeEntityType.CreateInstanceForLoading();
			reader.ResetFieldReading(shapeEntityType.PropertyDefinitions);
			reader.DoBeginObject();
			((IEntity)shape).AssignId(reader.ReadId());
			((IEntity)shape).LoadFields(reader, shapeEntityType.RepositoryVersion);
			xmlReader.Read(); // Reads out of attributes
			foreach (EntityPropertyDefinition pi in shapeEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)shape).LoadInnerObjects(pi.Name, reader, shapeEntityType.RepositoryVersion);
			// Read the child shapes
			if (XmlReadStartElement(childrenTag)) {
				do {
					Shape s = ReadShape(cache, reader, shape);
					shape.Children.Add(s);
				} while (xmlReader.Name != childrenTag && xmlReader.NodeType != XmlNodeType.EndElement);
				if (xmlReader.Name != childrenTag) throw new NShapeException("Shape children are invalid in XML document.");
				XmlReadEndElement(childrenTag);
			}
			// Reads the shape's end element
			XmlReadEndElement(shapeTag);
			// Insert shape into cache
			cache.LoadedShapes.Add(new EntityBucket<Shape>(shape, owner, ItemState.Original));
			return shape;
		}


		private void ReadDiagramShapeConnections(IStoreCache cache, XmlReader xmlReader) {
			if (XmlSkipStartElement(connectionsTag)) {
				while (string.Compare(xmlReader.Name, connectionTag, StringComparison.InvariantCultureIgnoreCase) == 0) {
					xmlReader.MoveToFirstAttribute();
					object connectorId = ParseId(xmlReader.GetAttribute(0));
					xmlReader.MoveToNextAttribute();
					int gluePointId = int.Parse(xmlReader.GetAttribute(1));
					xmlReader.MoveToNextAttribute();
					object targetShapeId = ParseId(xmlReader.GetAttribute(2));
					xmlReader.MoveToNextAttribute();
					int targetPointId = int.Parse(xmlReader.GetAttribute(3));
					xmlReader.MoveToNextAttribute();
					Shape connector = cache.GetShape(connectorId);
					Shape targetShape = cache.GetShape(targetShapeId);
					connector.Connect(gluePointId, targetShape, targetPointId);
					XmlSkipEndElement(connectionTag);
				}
				XmlSkipEndElement(connectionsTag);
			}
		}


		// TODO 2: This is more or less identical to ReadShape. Unify?
		// That would require a IEntityWithChildren interface.
		private IModelObject ReadModelObject(IStoreCache cache, XmlStoreReader reader, IEntity owner) {
			Debug.Assert(xmlReader.NodeType == XmlNodeType.Element);
			string modelObjectTag = xmlReader.Name;
			IEntityType entityType = cache.FindEntityTypeByElementName(modelObjectTag);
			if (entityType == null)
				throw new NShapeException("No model object type found for tag '{0}'.", modelObjectTag);
			XmlSkipStartElement(modelObjectTag);
			IModelObject modelObject = (IModelObject)entityType.CreateInstanceForLoading();
			reader.ResetFieldReading(entityType.PropertyDefinitions);
			reader.DoBeginObject();
			modelObject.AssignId(reader.ReadId());
			modelObject.LoadFields(reader, entityType.RepositoryVersion);
			xmlReader.Read(); // Reads out of attributes
			foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					modelObject.LoadInnerObjects(pi.Name, reader, entityType.RepositoryVersion);
			// Read the child ModelObjects
			if (xmlReader.NodeType == XmlNodeType.Element) {
				do {
					IModelObject m = ReadModelObject(cache, reader, modelObject);
				} while (xmlReader.Name != childrenTag && xmlReader.NodeType != XmlNodeType.EndElement);
				if (xmlReader.Name != childrenTag) throw new NShapeException("ModelObject children are invalid in XML document.");
				XmlReadEndElement(childrenTag);
			}
			// Reads the model object's end element
			XmlReadEndElement(modelObjectTag);
			// Insert entity into cache
			cache.LoadedModelObjects.Add(new EntityBucket<IModelObject>(modelObject, owner, ItemState.Original));

			return modelObject;
		}

		#endregion


		#region [Private] Methods: Write to XML file

		private void CreateFile(string path, bool overwrite) {
			string imageDirectoryName = CalcImageDirectoryName();
			if (!overwrite) {
				string tempPath = GetTemporaryProjectFilePath();
				if (File.Exists(path)) {
					if (path == tempPath) 
						File.Delete(path);
					else throw new IOException(string.Format("File {0} already exists.", path));
				}
				if (Directory.Exists(imageDirectoryName)) {
					if (imageDirectoryName == CalcImageDirectoryName(tempPath)) 
						Directory.Delete(imageDirectoryName, true);
					else throw new IOException(string.Format("Image directory {0} already exists.", imageDirectoryName));
				}
				// Image directory will be created on demand.
			}
		}


		private void OpenWriter(IStoreCache cache, Stream stream) {
			Debug.Assert(repositoryWriter == null);
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			xmlWriter = XmlWriter.Create(stream, settings);
			repositoryWriter = new XmlStoreWriter(xmlWriter, this, cache);
		}


		private void OpenReader(IStoreCache cache, Stream stream) {
			if (cache == null) throw new ArgumentNullException("cache");
			if (stream == null) throw new ArgumentNullException("stream");
			// Rewind stream if necessary
			if (stream.Position != 0) stream.Position = 0;
			// Create readers
			xmlReader = XmlReader.Create(stream, GetXmlReaderSettings());
			xmlReader.Read();
			repositoryReader = new XmlStoreReader(xmlReader, this, cache);
		}


		private void CloseReader() {
			repositoryReader = null;
			if (xmlReader != null) {
				try {
					xmlReader.Close();
				} finally { 
					xmlReader = null; 
				}
			}
			if (projectFileStream != null) {
				try {
					CloseFileStream(projectFileStream);
				} finally {
					projectFileStream = null;
				}
			}
			Debug.Assert(xmlWriter == null);
			Debug.Assert(repositoryWriter == null);
		}


		private void CloseWriter() {
			repositoryWriter = null;
			if (xmlWriter != null) {
				try {
					xmlWriter.Flush();
					xmlWriter.Close();
				} finally {
					xmlWriter = null;
				}
			}
			if (projectFileStream != null) {
				try {
					CloseFileStream(projectFileStream);
				} finally {
					projectFileStream = null;
				}
			}
			Debug.Assert(xmlReader == null);
			Debug.Assert(repositoryReader == null);
		}


		private XmlReaderSettings GetXmlReaderSettings() {
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.CloseInput = true;
			settings.IgnoreComments = true;
			settings.IgnoreWhitespace = true;
			settings.IgnoreProcessingInstructions = true;
			return settings;
		}


		private void WriteProject(IStoreCache cache) {
			xmlWriter.WriteStartDocument();
			XmlOpenElement(rootTag);
			xmlWriter.WriteAttributeString("version", version.ToString());
			WriteProjectSettings(cache);
			// Currently there is no other design than the project design
			// WriteDesigns();
			WriteModel(cache);
			WriteTemplates(cache);
			WriteDiagrams(cache);
			XmlCloseElement(); // project tag
			XmlCloseElement(); // NShape tag
			xmlWriter.WriteEndDocument();
		}


		private void WriteProjectSettings(IStoreCache cache) {
			IEntityType projectSettingsEntityType = cache.FindEntityTypeByName(ProjectSettings.EntityTypeName);
			IEntity entity = cache.Project;
			XmlOpenElement(projectTag);
			repositoryWriter.Reset(projectSettingsEntityType.PropertyDefinitions);
			// Save project settings
			repositoryWriter.Prepare(entity);
			if (entity.Id == null) entity.AssignId(Guid.NewGuid());
			repositoryWriter.WriteId(entity.Id);
			entity.SaveFields(repositoryWriter, projectSettingsEntityType.RepositoryVersion);
			// Save libraries
			foreach (EntityPropertyDefinition pi in projectSettingsEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					entity.SaveInnerObjects(pi.Name, repositoryWriter, projectSettingsEntityType.RepositoryVersion);
			// Save embedded images
			if (ImageLocation == ImageFileLocation.Embedded)
				WriteEmbeddedImages(cache);
			// Save the pseudo design
			entity = cache.ProjectDesign;
			if (entity.Id == null) entity.AssignId(Guid.NewGuid());
			WriteAllStyles(cache, cache.ProjectDesign);
		}


		private void WriteDesigns(IStoreCache cache) {
			IEntityType designEntityType = cache.FindEntityTypeByName(Design.EntityTypeName);
			string designCollectionTag = GetElementCollectionTag(designEntityType);
			string designTag = GetElementTag(designEntityType);
			//
			// Save designs and styles
			XmlOpenElement(designCollectionTag);
			repositoryWriter.Reset(designEntityType.PropertyDefinitions);
			foreach (Design design in GetLoadedDesigns(cache))
				WriteDesign(cache, design);
			foreach (Design design in GetNewDesigns(cache)) {
				AssignId(design);
				WriteDesign(cache, design);
			}
			XmlCloseElement();
		}


		private void WriteDesign(IStoreCache cache, Design design) {
			IEntityType designEntityType = cache.FindEntityTypeByName(Design.EntityTypeName);
			string designTag = GetElementTag(designEntityType);
			XmlOpenElement(designTag);
			repositoryWriter.Prepare(design);
			repositoryWriter.WriteId(((IEntity)design).Id);
			((IEntity)design).SaveFields(repositoryWriter, designEntityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in designEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)design).SaveInnerObjects(pi.Name, repositoryWriter, designEntityType.RepositoryVersion);
			WriteAllStyles(cache, design);
			XmlCloseElement();
		}


		private void WriteAllStyles(IStoreCache cache, Design design) {
			WriteStyles<ColorStyle>(cache, cache.FindEntityTypeByName(ColorStyle.EntityTypeName), design);
			WriteStyles<CapStyle>(cache, cache.FindEntityTypeByName(CapStyle.EntityTypeName), design);
			WriteStyles<CharacterStyle>(cache, cache.FindEntityTypeByName(CharacterStyle.EntityTypeName), design);
			WriteStyles<FillStyle>(cache, cache.FindEntityTypeByName(FillStyle.EntityTypeName), design);
			WriteStyles<LineStyle>(cache, cache.FindEntityTypeByName(LineStyle.EntityTypeName), design);
			WriteStyles<ParagraphStyle>(cache, cache.FindEntityTypeByName(ParagraphStyle.EntityTypeName), design);
		}


		private void WriteStyles<TStyle>(IStoreCache cache, IEntityType styleEntityType, Design design) {
			XmlOpenElement(GetElementCollectionTag(styleEntityType));
			repositoryWriter.Reset(styleEntityType.PropertyDefinitions);
			foreach (IStyle style in GetLoadedStyles(cache, design))
				if (style is TStyle) 
					WriteStyle(styleEntityType, style);
			foreach (IStyle style in GetNewStyles(cache, design))
				if (style is TStyle) {
					AssignId(style);
					WriteStyle(styleEntityType, style);
				}
			XmlCloseElement();
		}


		private void WriteStyle(IEntityType styleEntityType, IStyle style) {
			XmlOpenElement(GetElementTag(styleEntityType));
			repositoryWriter.Prepare(style);
			repositoryWriter.WriteId(style.Id);
			style.SaveFields(repositoryWriter, styleEntityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in styleEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					style.SaveInnerObjects(pi.Name, repositoryWriter, styleEntityType.RepositoryVersion);
			XmlCloseElement();
		}


		private void WriteTemplates(IStoreCache cache) {
			// Save Templates, Shape and ModelObject
			IEntityType entityType = cache.FindEntityTypeByName(Template.EntityTypeName);
			XmlOpenElement(GetElementCollectionTag(entityType));
			repositoryWriter.Reset(entityType.PropertyDefinitions);
			foreach (Template template in GetLoadedTemplates(cache))
				WriteTemplate(cache, entityType, template);
			foreach (Template template in GetNewTemplates(cache)) {
				AssignId(template);
				WriteTemplate(cache, entityType, template);
			}
			XmlCloseElement();
		}


		private void WriteTemplate(IStoreCache cache, IEntityType entityType, Template template) {
			XmlOpenElement(templateTag);
			// Save template definition
			repositoryWriter.Reset(entityType.PropertyDefinitions);
			repositoryWriter.Prepare(template);
			if (template.Id == null) template.AssignId(Guid.NewGuid());
			repositoryWriter.WriteId(template.Id);
			template.SaveFields(repositoryWriter, entityType.RepositoryVersion);
			XmlStoreWriter innerObjectsWriter = new XmlStoreWriter(xmlWriter, this, cache);
			if (template.Shape.ModelObject == null) {
				XmlOpenElement("no_model");
				XmlCloseElement();
			} else
				WriteModelObject(cache, template.Shape.ModelObject, innerObjectsWriter);
			WriteShape(cache, template.Shape, innerObjectsWriter);
			innerObjectsWriter = null;
			foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					template.SaveInnerObjects(pi.Name, repositoryWriter, entityType.RepositoryVersion);
			repositoryWriter.Finish();

			// Save template's model mappings
			WriteModelMappings(cache, template);

			XmlCloseElement(); // template tag
		}


		private void WriteModelMappings(IStoreCache cache, Template template) {
			XmlOpenElement(modelmappingsTag);
			foreach (IModelMapping modelMapping in GetLoadedModelMappings(cache, template))
				WriteModelMapping(cache, modelMapping);
			foreach (IModelMapping modelMapping in GetNewModelMappings(cache, template)) {
				AssignId(modelMapping);
				WriteModelMapping(cache, modelMapping);
			}
			XmlCloseElement();
		}


		private void WriteModelMapping(IStoreCache cache, IModelMapping modelMapping) {
			string entityTypeName;
			if (modelMapping is NumericModelMapping) entityTypeName = NumericModelMapping.EntityTypeName;
			else if (modelMapping is FormatModelMapping) entityTypeName = FormatModelMapping.EntityTypeName;
			else if (modelMapping is StyleModelMapping) entityTypeName = StyleModelMapping.EntityTypeName;
			else throw new NShapeUnsupportedValueException(modelMapping);

			IEntityType entityType = cache.FindEntityTypeByName(entityTypeName);
			// write Shape-Tag with EntityType
			XmlOpenElement(entityType.ElementName);
			repositoryWriter.Reset(entityType.PropertyDefinitions);
			repositoryWriter.Prepare(modelMapping);
			if (((IEntity)modelMapping).Id == null) ((IEntity)modelMapping).AssignId(Guid.NewGuid());
			repositoryWriter.WriteId(((IEntity)modelMapping).Id);
			((IEntity)modelMapping).SaveFields(repositoryWriter, entityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)modelMapping).SaveInnerObjects(pi.Name, repositoryWriter, entityType.RepositoryVersion);
			XmlCloseElement();
		}


		private void WriteEmbeddedImages(IStoreCache cache) {
			if (ImageLocation == ImageFileLocation.Embedded) {
			    XmlOpenElement(imagesTag);
		        
			    // Create XmlEmbeddedImageWriter
			    XmlEmbeddedImageWriter imgWriter = new XmlEmbeddedImageWriter(xmlWriter, this, cache);
			    
				// Write all images of all entities to the stream/file
				foreach (IEntity entity in GetAllDesigns(cache))
					WriteEmbeddedEntityImages(cache, imgWriter, entity, cache.FindEntityTypeByName(Design.EntityTypeName));
				foreach (IStyle style in GetAllStyles(cache, null)) {
					string entityTypeName = null;
					if (style is IColorStyle) entityTypeName = ColorStyle.EntityTypeName;
					else if (style is ICapStyle) entityTypeName = CapStyle.EntityTypeName;
					else if (style is ICharacterStyle) entityTypeName = CharacterStyle.EntityTypeName;
					else if (style is IFillStyle) entityTypeName = FillStyle.EntityTypeName;
					else if (style is ILineStyle) entityTypeName = LineStyle.EntityTypeName;
					else if (style is IParagraphStyle) entityTypeName = ParagraphStyle.EntityTypeName;
					else throw new ArgumentException("Unsupported style type!");
					WriteEmbeddedEntityImages(cache, imgWriter, style, cache.FindEntityTypeByName(entityTypeName));
				}
				foreach (IModelObject modelObject in GetAllModelObjects(cache, null))
					WriteEmbeddedEntityImages(cache, imgWriter, modelObject, cache.FindEntityTypeByName(modelObject.Type.FullName));
				foreach (IEntity entity in GetAllTemplates(cache))
					WriteEmbeddedEntityImages(cache, imgWriter, entity, cache.FindEntityTypeByName(Template.EntityTypeName));
				foreach (IEntity entity in GetAllDiagrams(cache))
					WriteEmbeddedEntityImages(cache, imgWriter, entity, cache.FindEntityTypeByName(Diagram.EntityTypeName));
				foreach (Shape shape in GetAllShapes(cache, null))
					WriteEmbeddedEntityImages(cache, imgWriter, shape, cache.FindEntityTypeByName(shape.Type.FullName));
				
			    XmlCloseElement();
			}
		}


		private void WriteEmbeddedEntityImages(IStoreCache cache, XmlEmbeddedImageWriter imgWriter, IEntity entity, IEntityType entityType) {
			imgWriter.Reset(entityType.PropertyDefinitions);
			imgWriter.Prepare(entity);
			imgWriter.WriteId(entity.Id);
			entity.SaveFields(imgWriter, entityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in entityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					entity.SaveInnerObjects(pi.Name, imgWriter, entityType.RepositoryVersion);
			imgWriter.Finish();
		}


		private void WriteModel(IStoreCache cache) {
			if (cache.ModelExists()) {
				IEntityType modelEntityType = cache.FindEntityTypeByName(Model.EntityTypeName);
				string modelTag = GetElementTag(modelEntityType);
				XmlOpenElement(modelTag);
				// Write model
				Model model = cache.GetModel();
				Debug.Assert(model != null);
				repositoryWriter.Prepare(model);
				if (model.Id == null) ((IEntity)model).AssignId(Guid.NewGuid());
				repositoryWriter.WriteId(((IEntity)model).Id);
				((IEntity)model).SaveFields(repositoryWriter, modelEntityType.RepositoryVersion);
				foreach (EntityPropertyDefinition pi in modelEntityType.PropertyDefinitions)
					if (pi is EntityInnerObjectsDefinition)
						((IEntity)model).SaveInnerObjects(pi.Name, repositoryWriter, modelEntityType.RepositoryVersion);

				// Write all model objects
				WriteModelObjects(cache, model);
				XmlCloseElement();
			}
		}


		private void WriteModelObjects(IStoreCache cache, Model model) {
			XmlOpenElement(modelObjectsTag);
			// We do not want to write template model objects here.
			foreach (IModelObject modelObject in GetLoadedModelObjects(cache, model))
				WriteModelObject(cache, modelObject, repositoryWriter);
			foreach (IModelObject modelObject in GetNewModelObjects(cache, model)) {
				AssignId(modelObject);
				WriteModelObject(cache, modelObject, repositoryWriter);
			}
			XmlCloseElement();
		}


		private void WriteModelObject(IStoreCache cache, IModelObject modelObject, RepositoryWriter writer) {
			IEntityType modelObjectEntityType = cache.FindEntityTypeByName(modelObject.Type.FullName);
			string modelObjectTag = GetElementTag(modelObjectEntityType);
			XmlOpenElement(modelObjectTag);
			writer.Reset(modelObjectEntityType.PropertyDefinitions);
			writer.Prepare(modelObject);
			if (modelObject.Id == null) modelObject.AssignId(Guid.NewGuid());
			writer.WriteId(modelObject.Id);
			modelObject.SaveFields(writer, modelObjectEntityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in modelObjectEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)modelObject).SaveInnerObjects(pi.Name, writer, modelObjectEntityType.RepositoryVersion);
			writer.Finish();
			// ToDo: Save model object's children
			if (cache is IRepository) {
				foreach (IModelObject child in ((IRepository)cache).GetModelObjects(modelObject))
					WriteModelObject(cache, child, writer);
			} else throw new NotImplementedException();
			XmlCloseElement();
		}


		private void WriteDiagrams(IStoreCache cache) {
			IEntityType diagramEntityType = cache.FindEntityTypeByName(Diagram.EntityTypeName);
			string diagramCollectionTag = GetElementCollectionTag(diagramEntityType);
			XmlOpenElement(diagramCollectionTag);
			foreach (Diagram diagram in GetLoadedDiagrams(cache))
				WriteDiagram(cache, diagramEntityType, diagram);
			foreach (Diagram diagram in GetNewDiagrams(cache)) {
				AssignId(diagram);
				WriteDiagram(cache, diagramEntityType, diagram);
			}
			XmlCloseElement();
		}


		private void WriteDiagram(IStoreCache cache, IEntityType diagramEntityType, Diagram diagram) {
			string diagramTag = GetElementTag(diagramEntityType);
			XmlOpenElement(diagramTag);
			repositoryWriter.Reset(diagramEntityType.PropertyDefinitions);
			repositoryWriter.Prepare(diagram);
			repositoryWriter.WriteId(((IEntity)diagram).Id);
			((IEntity)diagram).SaveFields(repositoryWriter, diagramEntityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in diagramEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)diagram).SaveInnerObjects(pi.Name, repositoryWriter, diagramEntityType.RepositoryVersion);
			WriteDiagramShapes(cache, diagram);
			WriteDiagramShapeConnections(diagram);
			XmlCloseElement();
		}


		private void WriteDiagramShapes(IStoreCache cache, Diagram diagram) {
			XmlOpenElement(shapesTag);
			foreach (Shape shape in GetLoadedShapes(cache, diagram))
				WriteShape(cache, shape, repositoryWriter);
			foreach (Shape shape in GetNewShapes(cache, diagram)) {
				AssignId(shape);
				WriteShape(cache, shape, repositoryWriter);
			}
			XmlCloseElement();
		}


		private void WriteShape(IStoreCache cache, Shape shape, RepositoryWriter writer) {
			IEntityType shapeEntityType = cache.FindEntityTypeByName(shape.Type.FullName);
			// write Shape-Tag with EntityType
			XmlOpenElement(shapeEntityType.ElementName);
			writer.Reset(shapeEntityType.PropertyDefinitions);
			writer.Prepare(shape);
			if (((IEntity)shape).Id == null) ((IEntity)shape).AssignId(Guid.NewGuid());
			writer.WriteId(((IEntity)shape).Id);
			((IEntity)shape).SaveFields(writer, shapeEntityType.RepositoryVersion);
			foreach (EntityPropertyDefinition pi in shapeEntityType.PropertyDefinitions)
				if (pi is EntityInnerObjectsDefinition)
					((IEntity)shape).SaveInnerObjects(pi.Name, writer, shapeEntityType.RepositoryVersion);
			// Write children
			if (shape.Children.Count > 0) {
				XmlOpenElement("children");
				foreach (Shape s in shape.Children)
					WriteShape(cache, s, writer);
				XmlCloseElement();
			}
			XmlCloseElement();
		}


		private void WriteDiagramShapeConnections(Diagram diagram) {
			XmlOpenElement(connectionTag + "s");
			WriteShapeConnections(diagram.Shapes);
			XmlCloseElement();
		}


		private void WriteShapeConnections(IEnumerable<Shape> shapes) {
			// Get all glue points of all shapes and store their connection infos (if connected)
			// Passive connection points can be ignored as their connection infos are redundant anyway.
			foreach (Shape shape in shapes) {
				foreach (ControlPointId gluePointId in shape.GetControlPointIds(ControlPointCapabilities.Glue)) {
					// Get connection info for the glue point (skip if empty)
					ShapeConnectionInfo sci = shape.GetConnectionInfo(gluePointId, null);
					if (sci.IsEmpty) continue;

					XmlOpenElement(connectionTag);
					xmlWriter.WriteAttributeString(activeShapeTag, ((IEntity)shape).Id.ToString());
					xmlWriter.WriteAttributeString(gluePointIdTag, gluePointId.ToString());
					xmlWriter.WriteAttributeString(passiveShapeTag, ((IEntity)sci.OtherShape).Id.ToString());
					xmlWriter.WriteAttributeString(connectionPointIdTag, sci.OtherPointId.ToString());
					XmlCloseElement();
				}
				// Process connections of child shapes, too
				if (shape.Children.Count > 0)
					WriteShapeConnections(shape.Children);
			}
		}

		#endregion


		#region [Private] Methods: Retrieving Entities

		private IEnumerable<TEntity> GetEntities<TEntity>(IEnumerable<EntityBucket<TEntity>> loadedEntities, IEntity owner) 
			where TEntity : IEntity 
		{
			foreach (EntityBucket<TEntity> item in loadedEntities) {
				if (item.State == ItemState.Deleted) continue;
				if (owner == null || owner == item.Owner)
					yield return item.ObjectRef;
			}
		}


		private IEnumerable<TEntity> GetEntities<TEntity>(IEnumerable<KeyValuePair<TEntity, IEntity>> newEntities, IEntity owner)
			where TEntity : IEntity 
		{
			foreach (KeyValuePair<TEntity, IEntity> item in newEntities) {
				if (owner == null || owner == item.Value)
					yield return item.Key;
			}
		}


		private void AssignId(IEntity entity) {
			Debug.Assert(entity.Id == null);
			entity.AssignId(Guid.NewGuid());
		}


		private IEnumerable<Design> GetLoadedDesigns(IStoreCache cache) {
			return GetEntities<Design>(cache.LoadedDesigns, null);
		}

		private IEnumerable<Design> GetNewDesigns(IStoreCache cache) {
			return GetEntities<Design>(cache.NewDesigns, null);
		}

		private IEnumerable<Design> GetAllDesigns(IStoreCache cache) {
			foreach (Design d in GetLoadedDesigns(cache)) yield return d;
			foreach (Design d in GetNewDesigns(cache)) yield return d;
		}


		private IEnumerable<Diagram> GetLoadedDiagrams(IStoreCache cache) {
			return GetEntities<Diagram>(cache.LoadedDiagrams, null);
		}
		
		private IEnumerable<Diagram> GetNewDiagrams(IStoreCache cache) {
			return GetEntities<Diagram>(cache.NewDiagrams, null);
		}

		private IEnumerable<Diagram> GetAllDiagrams(IStoreCache cache) {
			foreach (Diagram d in GetLoadedDiagrams(cache)) yield return d;
			foreach (Diagram d in GetNewDiagrams(cache)) yield return d;
		}

		
		private IEnumerable<IModelMapping> GetLoadedModelMappings(IStoreCache cache, Template owner) {
			// Original condition for getting loaded model mappings:
			//// Template shapes have a null Owner
			//if (eb.Owner != null && eb.Owner == template)
			return GetEntities<IModelMapping>(cache.LoadedModelMappings, owner);
		}

		private IEnumerable<IModelMapping> GetNewModelMappings(IStoreCache cache, Template owner) {
			return GetEntities<IModelMapping>(cache.NewModelMappings, owner);
		}

		private IEnumerable<IModelMapping> GetAllModelMappings(IStoreCache cache, Template owner) {
			foreach (IModelMapping m in GetLoadedModelMappings(cache, owner)) yield return m;
			foreach (IModelMapping m in GetNewModelMappings(cache, owner)) yield return m;
		}

				
		private IEnumerable<IModelObject> GetLoadedModelObjects(IStoreCache cache, Model owner) {
			// Original condition for retrieving loaded model objects:
			// if (mob.Owner == model || mob.Owner is IModelObject)
			return GetEntities<IModelObject>(cache.LoadedModelObjects, owner);
		}

		private IEnumerable<IModelObject> GetNewModelObjects(IStoreCache cache, Model owner) {
			// Original condition for retrieving new model objects:
			// if (mokvp.Value == model || mokvp.Value is IModelObject)
			return GetEntities<IModelObject>(cache.NewModelObjects, owner);
		}

		private IEnumerable<IModelObject> GetAllModelObjects(IStoreCache cache, Model owner) {
			foreach (IModelObject m in GetLoadedModelObjects(cache, owner)) yield return m;
			foreach (IModelObject m in GetNewModelObjects(cache, owner)) yield return m;
		}

		
		private IEnumerable<Model> GetLoadedModels(IStoreCache cache) {
			return GetEntities<Model>(cache.LoadedModels, null);
		}

		private IEnumerable<Model> GetNewModels(IStoreCache cache) {
			return GetEntities<Model>(cache.NewModels, null);
		}

		private IEnumerable<Model> GetAllModels(IStoreCache cache) {
			foreach (Model m in GetLoadedModels(cache)) yield return m;
			foreach (Model m in GetNewModels(cache)) yield return m;
		}

		
		private IEnumerable<Shape> GetLoadedShapes(IStoreCache cache, Diagram owner) {
			// Original condition for retrieving loaded shapes:
			// if (eb.Owner != null && eb.Owner == diagram && eb.State != ItemState.Deleted)
			return GetEntities<Shape>(cache.LoadedShapes, owner);
		}

		private IEnumerable<Shape> GetNewShapes(IStoreCache cache, Diagram owner) {
			return GetEntities<Shape>(cache.NewShapes, owner);
		}

		private IEnumerable<Shape> GetAllShapes(IStoreCache cache, Diagram owner) {
			foreach (Shape s in GetLoadedShapes(cache, owner)) yield return s;
			foreach (Shape s in GetNewShapes(cache, owner)) yield return s;
		}

		
		private IEnumerable<IStyle> GetLoadedStyles(IStoreCache cache, Design owner) {
			// Original condition for retrieving loaded styles:
			//if ((styleItem.Owner == design || AreEqual(styleItem.Owner.Id, ((IEntity)design).Id)) && styleItem.ObjectRef is TStyle)
			return GetEntities<IStyle>(cache.LoadedStyles, owner);
		}

		private IEnumerable<IStyle> GetNewStyles(IStoreCache cache, Design owner) {
			return GetEntities<IStyle>(cache.NewStyles, owner);
		}

		private IEnumerable<IStyle> GetAllStyles(IStoreCache cache, Design owner) {
			foreach (IStyle s in GetLoadedStyles(cache, owner)) yield return s;
			foreach (IStyle s in GetNewStyles(cache, owner)) yield return s;
		}


		private IEnumerable<Template> GetLoadedTemplates(IStoreCache cache) {
			return GetEntities<Template>(cache.LoadedTemplates, null);
		}

		private IEnumerable<Template> GetNewTemplates(IStoreCache cache) {
			return GetEntities<Template>(cache.NewTemplates, null);
		}

		private IEnumerable<Template> GetAllTemplates(IStoreCache cache) {
			foreach (Template t in GetLoadedTemplates(cache)) yield return t;
			foreach (Template t in GetNewTemplates(cache)) yield return t;
		}

		#endregion


		#region [Private] Methods: Obtain object tags and field structure

		// TODO 2: Replace this access in place.
		private string GetElementTag(IEntityType entityType) {
			return entityType.ElementName;
		}


		private string GetElementCollectionTag(IEntityType entityType) {
			return GetElementTag(entityType) + "s";
		}

		#endregion


		#region [Private] Methods: XML helper functions

		private void XmlOpenElement(string name) {
			xmlWriter.WriteStartElement(name);
		}


		private void XmlCloseElement() {
			xmlWriter.WriteFullEndElement();
		}


		// If the current element is a start element with the given name, the function reads
		// it and returns true. If it is not, the function does nothing and returns false.
		private bool XmlReadStartElement(string name) {
			if (xmlReader.IsStartElement(name)) {
				xmlReader.Read();
				return true;
			} else return false;
		}


		// The current element is either <x a1="1"... /x> or </x>
		private void XmlReadEndElement(string name) {
			if (string.Compare(xmlReader.Name, name, StringComparison.InvariantCultureIgnoreCase) == 0
				&& xmlReader.NodeType == XmlNodeType.EndElement)
				xmlReader.ReadEndElement();
		}


		private bool XmlSkipToElement(string nodeName) {
			if (string.Compare(xmlReader.Name, nodeName, StringComparison.InvariantCultureIgnoreCase) == 0)
				return true;
			else
				return xmlReader.ReadToFollowing(nodeName);
		}


		// Tests whether we are currently at the beginning of an element with the
		// given name. If so, read into it and return true. Otherwise false.
		private bool XmlSkipStartElement(string nodeName) {
			// In case we are at an attribute
			if (xmlReader.NodeType != XmlNodeType.Element && xmlReader.NodeType != XmlNodeType.EndElement) {
				xmlReader.Read();
				xmlReader.MoveToContent();
			}
			if (xmlReader.EOF || string.Compare(xmlReader.Name, nodeName, StringComparison.InvariantCultureIgnoreCase) != 0)
				return false;
			if (xmlReader.IsEmptyElement && !xmlReader.HasAttributes) {
				xmlReader.ReadStartElement(nodeName);
				return false;
			}
			if (!xmlReader.IsEmptyElement && !xmlReader.HasAttributes)
				xmlReader.ReadStartElement(nodeName);
			return true;
		}


		private void XmlSkipEndElement(string nodeName) {
			XmlSkipAttributes();
			if (string.Compare(xmlReader.Name, nodeName, StringComparison.InvariantCultureIgnoreCase) == 0) {
				// skip end element
				if (xmlReader.NodeType == XmlNodeType.EndElement)
					xmlReader.ReadEndElement();
				// skip empty element
				else if (xmlReader.NodeType == XmlNodeType.Element && !xmlReader.HasAttributes) {
					xmlReader.Read();
					xmlReader.MoveToContent();
				}
			}
		}


		private void XmlSkipAttributes() {
			if (xmlReader.NodeType == XmlNodeType.Attribute) {
				xmlReader.Read();
				xmlReader.MoveToContent();
			}
		}

		#endregion


		internal bool IsVersion2TemplateDefinition(IList<EntityPropertyDefinition> propertyDefinitions) {
			if (propertyDefinitions == null) throw new ArgumentNullException("propertyDefinitions");
			// Backwards compatibility workaround, see comment in "GetPropertyDefinitions" of class "Template".
			bool result = true;
			List<EntityPropertyDefinition> templatePropertyDefs = new List<EntityPropertyDefinition>(Template.GetPropertyDefinitions(2));
			if (propertyDefinitions.Count != templatePropertyDefs.Count)
				result = false;
			else {
				for (int i = propertyDefinitions.Count - 1; i >= 0; --i) {
					if (propertyDefinitions[i].Name != templatePropertyDefs[i].Name) {
						result = false;
						break;
					}
				}
			}
			return result;
		}


		#region Fields

		/// <ToBeCompleted></ToBeCompleted>
		[Obsolete]
		protected const string ProjectFileExtension = ".xml";

		private const string DefaultFileExtension = ".xml";

		// Predefined XML Element Tags
		private const string projectTag = "project";
		private const string imagesTag = "images";
		private const string imageTag = "image";
		private const string fileNameTag = "file_name";
		private const string shapesTag = "shapes";
		private const string modelObjectsTag = "model_objects";
		private const string rootTag = "dataweb_nshape";
		private const string templateTag = "template";
		private const string modelmappingsTag = "model_mappings";
		private const string connectionsTag = "shape_connections";
		private const string connectionTag = "shape_connection";
		private const string activeShapeTag = "active_shape";
		private const string gluePointIdTag = "glue_point";
		private const string passiveShapeTag = "passive_shape";
		private const string connectionPointIdTag = "connection_point";
		private const string childrenTag = "children";
		private const string shapeStyleTag = "shape_styles";
		// Format string for DateTimes
		private const string datetimeFormat = "yyyy-MM-dd HH:mm:ss";
		// Indicates the highest supported cache version of the built-in entities.
		private const int currentVersion = 100;

		// Directory name of project file. Always != null
		private string directoryName = string.Empty;
		// Name of the project. Always != null
		private string projectName = string.Empty;
		// File extension of project file. Maybe null.
		private string fileExtension = DefaultFileExtension;
		private string backupFileExtension = DefaultBackupFileExtension;
		// Specifies whether a backup file should be created when saving the contents to file.
		private BackupFileGenerationMode backupGenerationMode = BackupFileGenerationMode.BakFile;
		private ImageFileLocation defaultImageLocation = XmlStore.ImageFileLocation.Directory;
		private ImageFileLocation? storeImageLocation = null;

		private bool isOpen;

		// Repository version of the built-in entities.
		private int version;

		// File name of design cache file. Always != null
		private string designFileName = string.Empty;

		/// <summary>Indicates that the whole file is in memory.</summary>
		private bool isOpenComplete = false;

		// Specifies whether or not to load the XML repository completely
		private bool usePartialLoading = false;

		// element attributes
		private Dictionary<string, string[]> attributeFields = new Dictionary<string, string[]>();

		// Contains the path to the directory where images are stored (ImageLocation.Directory only)
		private string imageDirectory;
		
		// Contains the file path to the current store file (ProjectFilePath or temporary file path)
		private string storeFilePath;

		private Stream projectFileStream = null;
		private Stream externalStream = null;
		private SortedList<string, Image> embeddedImages = null;
		private List<Diagram> diagramsToLoad = null;

		private XmlReader xmlReader;
		private XmlWriter xmlWriter;
		private XmlStoreWriter repositoryWriter = null;
		private XmlStoreReader repositoryReader = null;

		private Type idType = typeof(Guid);

		#endregion
	}

}
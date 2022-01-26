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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using System.Data.SqlClient;

namespace NShapeTest {

	/// <summary>
	/// Summary description for UnitTest
	/// </summary>
	[TestClass]
	public class RepositoryTest : NShapeTestBase {

		public RepositoryTest() {
			// TODO: Add constructor logic here
		}


		#region Additional test attributes

		// Use TestInitialize to run code before running each test 
		[TestInitialize()]
		public override void TestInitialize() {
			base.TestInitialize();
		}


		// Use TestCleanup to run code after each test has run
		[TestCleanup()]
		public override void TestCleanup() {
			base.TestCleanup();
		}

		#endregion


		#region Test methods

		[TestMethod]
		public void XmlRepository_VersionCompatibility_Extended_Test() {
			// Test strategy: 
			// Compare stored repositories of older versions (one for each file version) 
			// with repositories of the same file version produced by the current code.
			string repositoriesDir = GetTestRepositoriesDirectory();
			string libDir = Path.GetDirectoryName(typeof(Shape).Assembly.Location);

			Func<XmlStore, string, Project> createProject = new Func<XmlStore, string, Project>((store, path) => {
				CachedRepository repository = new CachedRepository();
				repository.Store = store;
				Project result = new Project();
				result.Repository = repository;
				result.LibrarySearchPaths.Add(libDir);
				result.AutoLoadLibraries = true;
				store.ProjectFilePath = path;
				store.BackupGenerationMode = XmlStore.BackupFileGenerationMode.None;
				result.Name = Path.GetFileNameWithoutExtension(path);
				return result;
			});


			int cnt = 0;
			List<string> errors = new List<string>();
			foreach (string dirPath in Directory.GetDirectories(repositoriesDir)) {
				foreach (string filePath in Directory.GetFiles(dirPath)) {
					// Skip file types other than *.xml and *.nspj
					if (!(filePath.EndsWith(".xml", StringComparison.InvariantCultureIgnoreCase)
						|| filePath.EndsWith(XmlStore.DefaultProjectFileExtension, StringComparison.InvariantCultureIgnoreCase)))
						continue;

					XmlStore xmlStore = new XmlStore();
					Project project = createProject(xmlStore, filePath);
					try {
						try {
							project.Open();
							foreach (Diagram d in project.Repository.GetDiagrams())
								project.Repository.GetDiagramShapes(d);
						} catch (Exception exc) {
							Assert.Fail("Opening project file '{0}' failed:{1}{2}", filePath, Environment.NewLine, exc.Message);
						}

						// Ensure that there are no duplicate template names. Remove later!
						Dictionary<string, Template> dic = new Dictionary<string, Template>();
						foreach (Template t in project.Repository.GetTemplates())
							dic.Add(t.Name, t);

						// Prepare saving to an other location
						string saveDir = Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), Path.GetFileName(dirPath));
						if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
						string saveFilePath = Path.Combine(saveDir, Path.GetFileName(xmlStore.ProjectFilePath));
						// Prepare opening the saved project file
						XmlStore xmlStore2 = new XmlStore();
						Project project2 = createProject(xmlStore2, saveFilePath);
						try {
							// Save and re-open the original file
							xmlStore.DirectoryName = saveDir;
							try {
								project.Repository.SaveChanges();
							} catch (Exception exc) {
								Assert.Fail("Saving project file '{0}' failed:{1}{2}", xmlStore.ProjectFilePath, Environment.NewLine, exc.Message);
							}
							project.Close();
							xmlStore.DirectoryName = dirPath;
							project.Open();

							project2.Open();
							// Compare contents
							try {
								RepositoryComparer.Compare(project, project2);
							} catch (Exception exc) {
								Assert.Fail("Comparing project file '{0}' failed:{1}{2}", filePath, Environment.NewLine, exc.Message);
							}
							project2.Close();
							xmlStore2.Erase();
						} finally {
							project2.Close();
						}
					} catch (Exception exc) {
						errors.Add(exc.Message);
					} finally {
						project.Close();
					}
					++cnt;
				}
			}

			if (errors.Count > 0) {
				string msg = string.Empty;
				foreach (string err in errors)
					msg += string.Format("{0}{1}", err, Environment.NewLine);
				Assert.Fail(msg);
			}
		}


		[TestMethod]
		public void XmlRepository_VersionCompatibility_Test() {
			for (int version = Project.FirstSupportedSaveVersion; version <= Project.LastSupportedSaveVersion; ++version) {
				// Test inserting, modifying and deleting objects from repository
				string timerName = "XML Repository Version Compatibility Test Timer";
				BeginTimer(timerName);
				RepositoryCompatibilityTestCore(RepositoryHelper.CreateXmlStore(), version);
				EndTimer(timerName);
			}
		}


		[TestMethod]
		public void SQLRepository_VersionCompatibility_Test() {
			// Create empty databases for all repository save versions
			RepositoryCompatibilityTest_CleanupDatabases();
			RepositoryCompatibilityTest_ImportDatabases();

			const string timerName = "SQL Repository Version Compatibility Test Timer";
			List<string> warnings = new List<string>();
			for (int version = Project.FirstSupportedSaveVersion; version <= Project.LastSupportedSaveVersion; ++version) {
				try {
					BeginTimer(timerName);

					// Test inserting, modifying and deleting objects from repository
					string databaseName = string.Format("NShape_Repository_v{0}", version);
					RepositoryCompatibilityTestCore(RepositoryHelper.CreateSqlStore(databaseName), version);
				} catch (Exception exc) {
					warnings.Add(string.Format("SQL repositories version {0} are not compatible with version {1}: {2}", version, Project.LastSupportedSaveVersion, exc.Message));
				} finally {
					EndTimer(timerName);
				}
			}

			// Clean up the created databases 
			RepositoryCompatibilityTest_CleanupDatabases();

			// Issue warnings
			if (warnings.Count > 0)
				Assert.Inconclusive(Environment.NewLine + string.Join(Environment.NewLine, warnings));
		}


		[TestMethod]
		public void XMLRepository_EditWithContents_Test() {
			// Test inserting, modifying and deleting objects from repository
			string timerName = "RepositoryTest XMLStore Timer (edit with contents)";
			BeginTimer(timerName);
			RepositoryEditTestCore(RepositoryHelper.CreateXmlStore(), RepositoryHelper.CreateXmlStore(), true);
			EndTimer(timerName);
		}


		[TestMethod]
		public void XMLRepository_EditWithoutContents_Test() {
			// Test inserting, modifying and deleting objects from repository
			string timerName = "RepositoryTest XMLStore Timer (edit without contents)";
			BeginTimer(timerName);
			RepositoryEditTestCore(RepositoryHelper.CreateXmlStore(), RepositoryHelper.CreateXmlStore(), false);
			EndTimer(timerName);
		}


		[TestMethod]
		public void XmlStore_FeatureSet_Test() {
			// Test inserting, modifying and deleting objects from repository
			string timerName = "RepositoryTest XMLStore Timer (Function set comparison test)";

			List<String> shapeTypeNames = new List<String>(GeneralShapeTypeNames);
			XmlStore basicFeatureStore = RepositoryHelper.CreateXmlStore(false, false, false);

			BeginTimer(timerName);
			// Test all features OFF
			RepositoryFunctionSetTestCore(basicFeatureStore, RepositoryHelper.CreateXmlStore(false, false, false), shapeTypeNames);

			// Test each feature for its own: Lazy loading, embedded images, automatic backups
			RepositoryFunctionSetTestCore(basicFeatureStore, RepositoryHelper.CreateXmlStore(true, false, false), shapeTypeNames);
			RepositoryFunctionSetTestCore(basicFeatureStore, RepositoryHelper.CreateXmlStore(false, true, false), shapeTypeNames);
			RepositoryFunctionSetTestCore(basicFeatureStore, RepositoryHelper.CreateXmlStore(false, false, true), shapeTypeNames);

			// Test lazy loading in combination with the other features
			RepositoryFunctionSetTestCore(basicFeatureStore, RepositoryHelper.CreateXmlStore(true, true, false), shapeTypeNames);
			RepositoryFunctionSetTestCore(basicFeatureStore, RepositoryHelper.CreateXmlStore(true, false, true), shapeTypeNames);

			// Test embedded images in combination with the other features
			RepositoryFunctionSetTestCore(basicFeatureStore, RepositoryHelper.CreateXmlStore(false, true, true), shapeTypeNames);
			// Lazy loading + embedded images: See above

			// Test all features ON
			RepositoryFunctionSetTestCore(basicFeatureStore, RepositoryHelper.CreateXmlStore(true, true, true), shapeTypeNames);

			EndTimer(timerName);
		}


		[TestMethod]
		public void XMLRepository_FunctionSetComparison_Test() {
			// Test inserting, modifying and deleting objects from repository
			string timerName = "RepositoryTest XMLStore Timer (Feature set comparison test)";
			BeginTimer(timerName);
			RepositoryFunctionSetTestCore(RepositoryHelper.CreateXmlStore(), RepositoryHelper.CreateXmlStore());
			EndTimer(timerName);
		}


		[TestMethod]
		public void XMLRepository_LargeDiagram_Test() {
			// Test inserting large diagrams
			string timerName = "LargeDiagramTest XMLStore Timer";
			BeginTimer(timerName);
			LargeDiagramCore(RepositoryHelper.CreateXmlStore());
			EndTimer(timerName);
		}


		[TestMethod]
		public void SQLRepository_EditWithContents_Test() {
			try {
				RepositoryHelper.SQLCreateDatabase();
				// Test inserting, modifying and deleting objects from repository
				string timerName = "RepositoryTest SqlStore Timer (edit with contents)";
				BeginTimer(timerName);
				RepositoryEditTestCore(RepositoryHelper.CreateSqlStore(), RepositoryHelper.CreateSqlStore(), true);
				EndTimer(timerName);
			} finally {
				RepositoryHelper.SQLDropDatabase();
			}
		}


		[TestMethod]
		public void SQLRepository_EditWithoutContents_Test() {
			try {
				RepositoryHelper.SQLCreateDatabase();
				// Test inserting, modifying and deleting objects from repository
				string timerName = "RepositoryTest SqlStore Timer (edit without contents)";
				BeginTimer(timerName);
				RepositoryEditTestCore(RepositoryHelper.CreateSqlStore(), RepositoryHelper.CreateSqlStore(), false);
				EndTimer(timerName);
			} finally {
				RepositoryHelper.SQLDropDatabase();
			}
		}


		[TestMethod]
		public void SQLRepository_FunctionSetComparison_Test() {
			try {
				RepositoryHelper.SQLCreateDatabase();
				// Test inserting, modifying and deleting objects from repository
				string timerName = "RepositoryTest SqlStore Timer (Function set comparison test)";
				BeginTimer(timerName);
				RepositoryFunctionSetTestCore(RepositoryHelper.CreateSqlStore(), RepositoryHelper.CreateSqlStore());
				EndTimer(timerName);
			} finally {
				RepositoryHelper.SQLDropDatabase();
			}
		}


		[TestMethod]
		public void SQLRepository_LargeDiagram_Test() {
			try {
				RepositoryHelper.SQLCreateDatabase();
				// Test inserting large diagrams
				string timerName = "LargeDiagramTest SqlStore Timer";
				BeginTimer(timerName);
				LargeDiagramCore(RepositoryHelper.CreateSqlStore());
				EndTimer(timerName);
			} finally {
				RepositoryHelper.SQLDropDatabase();
			}
		}

		#endregion


		#region Repository test core methods

		private void RepositoryEditTestCore(Store store1, Store store2, bool withContents) {
			RepositoryComparer.CompareIds = true;
			Project project1 = new Project();
			Project project2 = new Project();
			try {
				const string projectName = "Repository Test";
				const int shapesPerRow = 10;
				// Create two projects and repositories, one for modifying and 
				// saving, one for loading and comparing the result
				project1.Name =
				project2.Name = projectName;
				project1.Repository = new CachedRepository();
				project2.Repository = new CachedRepository();
				((CachedRepository)project1.Repository).Store = store1;
				((CachedRepository)project2.Repository).Store = store2;
				project1.AutoGenerateTemplates = project2.AutoGenerateTemplates = false;
				project1.AutoLoadLibraries = project2.AutoLoadLibraries = true;

				// Delete the current project (if it exists)
				project1.Repository.Erase();
				project1.Create();
				project1.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly, true);

				// Create test data, populate repository and save repository
				string diagramName = "Diagram";
				DiagramHelper.CreateDiagram(project1, diagramName, shapesPerRow, shapesPerRow, true, true, true, true, true);
				Diagram diagram = project1.Repository.GetDiagram(diagramName);
				diagram.BackgroundImage = new NamedImage(new Bitmap(100, 100, PixelFormat.Format32bppPArgb), "TestImage");
				diagram.BackgroundImageLayout = ImageLayoutMode.Center;
				project1.Repository.SaveChanges();

				// Compare the saved data with the loaded data
				project2.Open();
				RepositoryComparer.Compare(project1, project2);
				project2.Close();

				// Modify (and insert) content of the repository and save it
				CreateAndInsertContent(project1, withContents);
				ModifyAndUpdateContent(project1, withContents);
				project1.Repository.SaveChanges();

				// Compare the saved data with the loaded data
				project2.Open();
				RepositoryComparer.Compare(project1, project2);
				project2.Close();

				// Delete various data from project
				DeleteContent(project1, withContents);
				project1.Repository.SaveChanges();

				// Compare the saved data with the loaded data
				project2.Open();
				RepositoryComparer.Compare(project1, project2);
				project2.Close();

				// If the store is a XML store, check files and file names
				if (store1 is XmlStore) {
					XmlStore xmlStore = (XmlStore)store1;
					// Change the backup file extension
					xmlStore.BackupFileExtension = ".backup";

					string projectFilePath = xmlStore.ProjectFilePath;
					string projectImageDir = xmlStore.ImageDirectory;
					string backupFilePath = xmlStore.ProjectFilePath + xmlStore.BackupFileExtension;
					string backupImageDir = xmlStore.ImageDirectory + xmlStore.BackupFileExtension;
					// Delete backup files before testing their (non)-existance
					if (File.Exists(backupFilePath)) File.Delete(backupFilePath);
					if (Directory.Exists(backupImageDir)) Directory.Delete(backupImageDir, true);


					// Save project multiple times in order to check 
					// a) whether the backup files are *not* created in BackupGenerationMode == BackupFileGenerationMode.None
					// b) whether the backup files *are* created in BackupGenerationMode == BackupFileGenerationMode.BakFile
					// c) whether the backup files can be overwritten
					foreach (XmlStore.BackupFileGenerationMode backupMode in Enum.GetValues(typeof(XmlStore.BackupFileGenerationMode))) {
						xmlStore.BackupGenerationMode = backupMode;
						// Save two times in order to check for file locks etc
						for (int i = 0; i < 2; ++i) {
							project1.Repository.SaveChanges();
							Assert.IsTrue(File.Exists(projectFilePath));
							Assert.IsTrue(Directory.Exists(projectImageDir));
							bool createBackup = (xmlStore.BackupGenerationMode == XmlStore.BackupFileGenerationMode.BakFile);
							Assert.AreEqual(createBackup, File.Exists(backupFilePath));
							Assert.AreEqual(createBackup, Directory.Exists(backupImageDir));
						}
					}
				}
			} finally {
				project1.Close();
				project2.Close();
			}
		}


		private void RepositoryFunctionSetTestCore(Store store1, Store store2) {
			RepositoryFunctionSetTestCore(store1, store2, null);
		}


		private void RepositoryFunctionSetTestCore(Store store1, Store store2, IList<String> shapeTypeNames) {
			RepositoryComparer.CompareIds = false;
			Project project1 = new Project();
			Project project2 = new Project();
			project1.AutoGenerateTemplates = project2.AutoGenerateTemplates = false;
			try {
				// Create two projects and repositories, one for modifying and 
				// saving, one for loading and comparing the result
				const string projectName = "Repository Test";
				const string diagramName = "Diagram";
				const int shapesPerRow = 10;
				const bool connectShapes = true;
				const bool withModels = true;
				const bool withTerminalMappings = true;
				const bool withModelMappings = true;
				const bool withLayers = true;
				Prepare(project1, store1, projectName + " 1", diagramName, shapesPerRow, connectShapes, withModels, withTerminalMappings, withModelMappings, withLayers, shapeTypeNames);
				Prepare(project2, store2, projectName + " 2", diagramName, shapesPerRow, connectShapes, withModels, withTerminalMappings, withModelMappings, withLayers, shapeTypeNames);
				SaveAndClose(project1);
				SaveAndClose(project2);

				// Compare repositories
				project1.Open();
				project2.Open();
				RepositoryComparer.Compare(project1, project2);

				// Modify (and insert) content of the repository and save it
				ModifyAndUpdateContent(project1, true);
				ModifyAndUpdateContent(project2, false);
				SaveAndClose(project1);
				SaveAndClose(project2);

				// Compare repositories
				project1.Open();
				project2.Open();
				RepositoryComparer.Compare(project1, project2);

				// Create new content
				CreateAndInsertContent(project1, true);
				CreateAndInsertContent(project2, false);
				SaveAndClose(project1);
				SaveAndClose(project2);

				// Compare repositories
				project1.Open();
				project2.Open();
				RepositoryComparer.Compare(project1, project2);

				// Delete various data from project
				DeleteContent(project1, true);
				DeleteContent(project2, false);
				SaveAndClose(project1);
				SaveAndClose(project2);

				// Compare the saved data with the loaded data
				project1.Open();
				project2.Open();
				RepositoryComparer.Compare(project1, project2);
			} finally {
				project1.Close();
				project2.Close();
			}
		}


		private void LargeDiagramCore(Store store) {
			string projectName = "Large Diagram Test";
			Project project = new Project();
			try {
				project.AutoLoadLibraries = true;
				project.AutoGenerateTemplates = false;
				project.Name = projectName;
				project.Repository = new CachedRepository();
				((CachedRepository)project.Repository).Store = store;
				project.Repository.Erase();
				project.Create();
				project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly, true);

				string diagramName = "Large Diagram";
				DiagramHelper.CreateLargeDiagram(project, diagramName);

				project.Repository.SaveChanges();
				Trace.WriteLine("Saved!");
			} finally {
				project.Close();
			}
		}


		private void RepositoryCompatibilityTestCore(Store store, int version) {
			Project project = null;
			try {
				CachedRepository cachedRepository = new CachedRepository();
				cachedRepository.Version = version;
				cachedRepository.Store = store;

				project = new Project();
				project.Name = string.Format("Shape Libraries (Version {0})", version);
				project.Repository = cachedRepository;
				project.AutoLoadLibraries = false;

				// Add all shape libraries
				project.AddLibraryByName("Dataweb.NShape.GeneralShapes", false);
				project.AddLibraryByName("Dataweb.NShape.ElectricalShapes", false);
				project.AddLibraryByName("Dataweb.NShape.FlowChartShapes", false);
				if (store is XmlStore || version >= 3) {
					// SqlStore does not support saving the inner objects (columns) of the "Entity" shape in version 2
					project.AddLibraryByName("Dataweb.NShape.SoftwareArchitectureShapes", false);
				}

				// Erase existing repository and create a new one
				if (store is XmlStore) {
					// Delete the file, create a new file and adjust the version
					project.Repository.Erase();
					project.Create();
					project.ChangeVersion(version);
				} else {
					// SQL stores neither support changing the version nor creating projects on levels other 
					// than MaxSupportedSaveVersion, so we have to open an empty project.
					project.Open();
				}

				//// Sort shape types
				//SortedList<string, ShapeType> shapeTypes = new SortedList<string,ShapeType>();
				//foreach (ShapeType shapeType in project.ShapeTypes)
				//    shapeTypes.Add(shapeType.FullName, shapeType);

				// Create a diagram containing a shape of each shape type
				Diagram diagram = new Diagram("Shape Libraries");

				//int margin = 20;
				//int currentLeft = 0, currentTop = 0;
				//string currentLibName = null;
				//foreach (KeyValuePair<string, ShapeType> shapeTypeItem in shapeTypes) {
				//    if (currentLibName != shapeTypeItem.Value.LibraryName) {
				//        TextBase textShape = (TextBase)project.Repository.GetTemplate("Text").CreateShape();
				//        textShape.CharacterStyle = project.Design.CharacterStyles.Heading1;
				//        textShape.Text = shapeTypeItem.Value.LibraryName;
				//        textShape.X = currentLeft + textShape.Width / 2;
				//        textShape.Y = currentTop + textShape.Height / 2;
				//        diagram.Shapes.Add(textShape, project.Repository.ObtainNewTopZOrder(diagram));

				//    }
				//}

				project.Repository.InsertAll(diagram);
				project.Repository.SaveChanges();
				Trace.WriteLine("Saved!");
			} finally {
				project.Close();
			}
		}


		private void RepositoryCompatibilityTest_ImportDatabases() {
			// Get temporary database directory and delete it (if it does exists)
			string sqlRepositoriesDir = GetSQLRepositoriesDirectory();
			string workDir = Path.Combine(GetCommonTempDir(), Path.GetFileName(sqlRepositoriesDir));
			if (Directory.Exists(workDir))
				Directory.Delete(workDir, true);

			// Restore SQL databases for each repository level
			foreach (string srcFilePath in Directory.GetFiles(sqlRepositoriesDir)) {
				// Skip all files that are not SQL server database files.
				if (!srcFilePath.EndsWith(".mdf", StringComparison.InvariantCultureIgnoreCase))
					continue;

				// Copy SQL server database file
				string workFilePath = Path.Combine(workDir, Path.GetFileName(srcFilePath));
				string databaseName = Path.GetFileNameWithoutExtension(srcFilePath);
				if (!Directory.Exists(workDir))
					Directory.CreateDirectory(workDir);
				File.Copy(srcFilePath, workFilePath, true);

				// Attach SQL server database file to SQL server
				string connectionString = string.Format("server={0};Integrated Security=True", RepositoryHelper.GetSqlServerName());
				try {
					using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connectionString)) {
						conn.Open();
						using (System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand()) {
							cmd.CommandText = string.Format("CREATE DATABASE [{0}] ON ( FILENAME = N'{1}' ) FOR ATTACH", databaseName, workFilePath);
							cmd.ExecuteNonQuery();
						}
					}
				} catch (SqlException exc) {
					if (exc.ErrorCode != RepositoryHelper.SqlErrorNewLogFileCreated)
						throw;
				}
			}
		}


		private void RepositoryCompatibilityTest_CleanupDatabases() {
			string sqlRepositoriesDir = GetSQLRepositoriesDirectory();
			foreach (string srcFilePath in Directory.GetFiles(sqlRepositoriesDir)) {
				// Skip all files that are not SQL server database files.
				if (!srcFilePath.EndsWith(".mdf", StringComparison.InvariantCultureIgnoreCase))
					continue;

				// Drop database
				string databaseName = Path.GetFileNameWithoutExtension(srcFilePath);
				try {
					RepositoryHelper.SQLDropDatabase(databaseName);
				} catch (Exception) {
					try {
						string serverName = Environment.MachineName + RepositoryHelper.SqlServerName;
						string connectionString = string.Format("server={0};Integrated Security=True", serverName);
						using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection(connectionString)) {
							conn.Open();
							using (System.Data.SqlClient.SqlCommand cmd = conn.CreateCommand()) {
								cmd.CommandText = string.Format("DROP DATABASE [{0}]", databaseName);
								cmd.ExecuteNonQuery();
							}
						}

					} catch (System.Data.SqlClient.SqlException) { }
				}
			}
		}


		private string GetSourceDirectory() {
			return Path.GetDirectoryName(Path.GetDirectoryName(TestContext.TestDir));
		}


		private string GetTestRepositoriesDirectory() {
			return Path.Combine(new string[] { GetSourceDirectory(), "Test", "Resources", "Repositories" });
		}


		private string GetSQLRepositoriesDirectory() {
			return Path.Combine(GetTestRepositoriesDirectory(), "SQL Repositories");
		}


		private string GetCommonTempDir() {
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Temp");
		}

		#endregion


		#region Repository test helper methods - Insert content

		private void CreateAndInsertContent(Project project, bool withContent) {
			IRepository repository = project.Repository;

			// Insert styles into the project's design
			// Insert new color styles at least in order to prevent other styles from using them (they will be deleted later)
			CreateAndInsertStyles(project.Design, repository);

			// ToDo: Currently, the XML repository does not support more than one design.
			if (repository is CachedRepository && ((CachedRepository)repository).Store is AdoNetStore) {
				List<Design> designs = new List<Design>(repository.GetDesigns());
				foreach (Design design in designs)
					CreateAndInsertDesign(project.Design, repository, withContent);
			}

			// Insert templates
			List<Template> templates = new List<Template>(repository.GetTemplates());
			foreach (Template template in templates)
				CreateAndInsertTemplate(template, repository, withContent);

			// Insert model objects
			List<IModelObject> modelObjects = new List<IModelObject>(repository.GetModelObjects(null));
			foreach (IModelObject modelObject in modelObjects)
				CreateAndInsertModelObject(modelObject, repository);

			// Insert diagrams and shapes
			List<Diagram> diagrams = new List<Diagram>(repository.GetDiagrams());
			foreach (Diagram diagram in diagrams)
				CreateAndInsertDiagram(diagram, repository, withContent);
		}


		private void CreateAndInsertDesign(Design sourceDesign, IRepository repository, bool withContent) {
			// Create new design from source design
			Design newDesign = new Design(GetName(sourceDesign.Name, EditContentMode.Insert));
			foreach (IColorStyle style in sourceDesign.ColorStyles)
				newDesign.AddStyle(CreateStyle(style, newDesign));
			foreach (ICapStyle style in sourceDesign.CapStyles)
				newDesign.AddStyle(CreateStyle(style, newDesign));
			foreach (ICharacterStyle style in sourceDesign.CharacterStyles)
				newDesign.AddStyle(CreateStyle(style, newDesign));
			foreach (IFillStyle style in sourceDesign.FillStyles)
				newDesign.AddStyle(CreateStyle(style, newDesign));
			foreach (ILineStyle style in sourceDesign.LineStyles)
				newDesign.AddStyle(CreateStyle(style, newDesign));
			foreach (IParagraphStyle style in sourceDesign.ParagraphStyles)
				newDesign.AddStyle(CreateStyle(style, newDesign));
			// Insert new design into repository
			if (withContent)
				repository.InsertAll(newDesign);
			else {
				repository.Insert(newDesign);
				foreach (IStyle style in newDesign.Styles)
					repository.Insert(newDesign, style);
			}
		}


		private void CreateAndInsertStyles(Design design, IRepository repository) {
			int newStylesCnt;

			newStylesCnt = CreateAndInsertStyles(design, design.ColorStyles, repository);
			Assert.AreEqual(newStylesCnt * 2, design.ColorStyles.Count);

			newStylesCnt = CreateAndInsertStyles(design, design.CapStyles, repository);
			Assert.AreEqual(newStylesCnt * 2, design.CapStyles.Count);

			newStylesCnt = CreateAndInsertStyles(design, design.CharacterStyles, repository);
			Assert.AreEqual(newStylesCnt * 2, design.CharacterStyles.Count);

			newStylesCnt = CreateAndInsertStyles(design, design.FillStyles, repository);
			Assert.AreEqual(newStylesCnt * 2, design.FillStyles.Count);

			newStylesCnt = CreateAndInsertStyles(design, design.LineStyles, repository);
			Assert.AreEqual(newStylesCnt * 2, design.LineStyles.Count);

			newStylesCnt = CreateAndInsertStyles(design, design.ParagraphStyles, repository);
			Assert.AreEqual(newStylesCnt * 2, design.ParagraphStyles.Count);
		}


		private int CreateAndInsertStyles<TStyle>(Design design, IEnumerable<TStyle> styles, IRepository repository) where TStyle : IStyle {
			List<TStyle> styleBuffer = new List<TStyle>();
			foreach (TStyle style in styles) {
				if (style is ICapStyle) styleBuffer.Add((TStyle)CreateStyle((ICapStyle)style, design));
				else if (style is IColorStyle) styleBuffer.Add((TStyle)CreateStyle((IColorStyle)style, design));
				else if (style is ICharacterStyle) styleBuffer.Add((TStyle)CreateStyle((ICharacterStyle)style, design));
				else if (style is IFillStyle) styleBuffer.Add((TStyle)CreateStyle((IFillStyle)style, design));
				else if (style is ILineStyle) styleBuffer.Add((TStyle)CreateStyle((ILineStyle)style, design));
				else if (style is IParagraphStyle) styleBuffer.Add((TStyle)CreateStyle((IParagraphStyle)style, design));
				else throw new NotSupportedException("Unsupported stye type");
			}
			Assert.AreEqual(EnumerationHelper.Count(styles), styleBuffer.Count);
			foreach (IStyle style in styleBuffer) {
				design.AddStyle(style);
				repository.Insert(design, style);
			}
			return styleBuffer.Count;
		}


		private ICapStyle CreateStyle(ICapStyle sourceStyle, Design design) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design);
			CapStyle newStyle = new CapStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert).ToLower();
			newStyle.CapShape = sourceStyle.CapShape;
			newStyle.CapSize = sourceStyle.CapSize;
			if (sourceStyle.ColorStyle != null) {
				string colorStyleName = GetName(sourceStyle.ColorStyle.Name, EditContentMode.Insert);
				if (!design.ColorStyles.Contains(colorStyleName))
					CreateStyle(sourceStyle.ColorStyle, design);
				newStyle.ColorStyle = design.ColorStyles[colorStyleName];
			}
			return newStyle;
		}


		private IColorStyle CreateStyle(IColorStyle sourceStyle, Design design) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design);
			ColorStyle newStyle = new ColorStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert).ToLower();
			newStyle.Color = sourceStyle.Color;
			newStyle.Transparency = sourceStyle.Transparency;
			newStyle.ConvertToGray = sourceStyle.ConvertToGray;
			return newStyle;
		}


		private IFillStyle CreateStyle(IFillStyle sourceStyle, Design design) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design);
			FillStyle newStyle = new FillStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert).ToLower();
			if (sourceStyle.AdditionalColorStyle != null) {
				string colorStyleName = GetName(sourceStyle.AdditionalColorStyle.Name, EditContentMode.Insert);
				if (!design.ColorStyles.Contains(colorStyleName))
					CreateStyle(sourceStyle.AdditionalColorStyle, design);
				newStyle.AdditionalColorStyle = design.ColorStyles[colorStyleName];
			}
			if (sourceStyle.BaseColorStyle != null) {
				string colorStyleName = GetName(sourceStyle.BaseColorStyle.Name, EditContentMode.Insert);
				if (!design.ColorStyles.Contains(colorStyleName))
					CreateStyle(sourceStyle.BaseColorStyle, design);
				newStyle.BaseColorStyle = design.ColorStyles[colorStyleName];
			}
			newStyle.ConvertToGrayScale = sourceStyle.ConvertToGrayScale;
			newStyle.FillMode = sourceStyle.FillMode;
			newStyle.FillPattern = sourceStyle.FillPattern;
			if (sourceStyle.Image != null) {
				NamedImage namedImg = new NamedImage((Image)sourceStyle.Image.Image.Clone(),
					GetName(sourceStyle.Image.Name, EditContentMode.Insert));
				newStyle.Image = namedImg;
			} else newStyle.Image = sourceStyle.Image;
			newStyle.ImageGammaCorrection = sourceStyle.ImageGammaCorrection;
			newStyle.ImageLayout = sourceStyle.ImageLayout;
			newStyle.ImageTransparency = sourceStyle.ImageTransparency;
			return newStyle;
		}


		private ICharacterStyle CreateStyle(ICharacterStyle sourceStyle, Design design) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design);
			CharacterStyle newStyle = new CharacterStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert).ToLower();
			if (sourceStyle.ColorStyle != null) {
				string colorStyleName = GetName(sourceStyle.ColorStyle.Name, EditContentMode.Insert);
				if (!design.ColorStyles.Contains(colorStyleName))
					CreateStyle(sourceStyle.ColorStyle, design);
				newStyle.ColorStyle = design.ColorStyles[colorStyleName];
			}
			newStyle.FontName = sourceStyle.FontName;
			newStyle.SizeInPoints = sourceStyle.SizeInPoints;
			newStyle.Style = sourceStyle.Style;
			return newStyle;
		}


		private ILineStyle CreateStyle(ILineStyle sourceStyle, Design design) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design);
			LineStyle newStyle = new LineStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert).ToLower();
			if (sourceStyle.ColorStyle != null) {
				string colorStyleName = GetName(sourceStyle.ColorStyle.Name, EditContentMode.Insert);
				if (!design.ColorStyles.Contains(colorStyleName))
					CreateStyle(sourceStyle.ColorStyle, design);
				newStyle.ColorStyle = design.ColorStyles[colorStyleName];
			}
			newStyle.DashCap = sourceStyle.DashCap;
			newStyle.DashType = sourceStyle.DashType;
			newStyle.LineJoin = sourceStyle.LineJoin;
			newStyle.LineWidth = sourceStyle.LineWidth;
			return newStyle;
		}


		private IParagraphStyle CreateStyle(IParagraphStyle sourceStyle, Design design) {
			if (sourceStyle == null) throw new ArgumentNullException("baseStyle");
			string newName = GetNewStyleName(sourceStyle, design);
			ParagraphStyle newStyle = new ParagraphStyle(newName);
			newStyle.Title = GetName(sourceStyle.Title, EditContentMode.Insert).ToLower();
			newStyle.Alignment = sourceStyle.Alignment;
			newStyle.Padding = sourceStyle.Padding;
			newStyle.Trimming = sourceStyle.Trimming;
			return newStyle;
		}


		private void CreateAndInsertTemplate(Template sourceTemplate, IRepository repository, bool withContent) {
			Template newTemplate = sourceTemplate.Clone();
			newTemplate.Description = GetName(sourceTemplate.Description, EditContentMode.Insert);
			newTemplate.Name = GetName(sourceTemplate.Name, EditContentMode.Insert);
			// Clone ModelObject and insert terminal mappings
			if (sourceTemplate.Shape.ModelObject != null) {
				// Copy terminal mapping of reference point
				TerminalId terminalId = sourceTemplate.GetMappedTerminalId(ControlPointId.Reference);
				if (terminalId != TerminalId.Invalid) newTemplate.MapTerminal(terminalId, ControlPointId.Reference);
				// Copy other terminal mappings
				foreach (ControlPointId pointId in sourceTemplate.Shape.GetControlPointIds(ControlPointCapabilities.All)) {
					terminalId = sourceTemplate.GetMappedTerminalId(pointId);
					if (terminalId != TerminalId.Invalid) newTemplate.MapTerminal(terminalId, pointId);
				}
			}
			if (withContent)
				repository.InsertAll(newTemplate);
			else {
				repository.Insert(newTemplate);
				if (newTemplate.Shape.ModelObject != null)
					repository.Insert(newTemplate.Shape.ModelObject, newTemplate);
				InsertShape(newTemplate.Shape, newTemplate, repository);
				// Insert property mappings
				foreach (IModelMapping newModelMapping in newTemplate.GetPropertyMappings())
					repository.Insert(newModelMapping, newTemplate);
			}
		}


		private void CreateAndInsertDiagram(Diagram sourceDiagram, IRepository repository, bool withContent) {
			Diagram newDiagram = new Diagram(GetName(sourceDiagram.Name, EditContentMode.Insert));
			newDiagram.BackgroundColor = sourceDiagram.BackgroundColor;
			newDiagram.BackgroundGradientColor = sourceDiagram.BackgroundGradientColor;
			newDiagram.BackgroundImageGamma = sourceDiagram.BackgroundImageGamma;
			newDiagram.BackgroundImageGrayscale = sourceDiagram.BackgroundImageGrayscale;
			newDiagram.BackgroundImageLayout = sourceDiagram.BackgroundImageLayout;
			newDiagram.BackgroundImageTransparency = sourceDiagram.BackgroundImageTransparency;
			newDiagram.BackgroundImageTransparentColor = sourceDiagram.BackgroundImageTransparentColor;
			newDiagram.Height = sourceDiagram.Height;
			newDiagram.Width = sourceDiagram.Width;
			newDiagram.Title = GetName(sourceDiagram.Title, EditContentMode.Insert).ToLower();
			foreach (Layer sourceLayer in sourceDiagram.Layers) {
				Layer newLayer = new Layer(GetName(sourceLayer.Name, EditContentMode.Insert));
				newLayer.Title = GetName(sourceLayer.Title, EditContentMode.Insert).ToLower();
				newLayer.UpperZoomThreshold = sourceLayer.UpperZoomThreshold;
				newLayer.LowerZoomThreshold = sourceLayer.LowerZoomThreshold;
				newDiagram.Layers.Add(newLayer);
			}
			foreach (Shape sourceShape in sourceDiagram.Shapes.BottomUp) {
				Shape newShape = sourceShape.Clone();
				newDiagram.Shapes.Add(newShape, sourceShape.ZOrder);
				newDiagram.AddShapeToLayers(newShape, sourceShape.HomeLayer, sourceShape.SupplementalLayers);
			}

			if (withContent)
				repository.InsertAll(newDiagram);
			else {
				// Insert diagram only
				repository.Insert(newDiagram);
				// Insert shapes
				foreach (Shape sourceShape in newDiagram.Shapes.BottomUp)
					InsertShape(sourceShape, newDiagram, repository);
			}
		}


		private void InsertShape(Shape shape, Diagram owner, IRepository repository) {
			repository.Insert(shape, owner);
			foreach (Shape childShape in shape.Children)
				InsertShape(childShape, shape, repository);
		}


		private void InsertShape(Shape shape, Template owner, IRepository repository) {
			repository.Insert(shape, owner);
			foreach (Shape childShape in shape.Children)
				InsertShape(childShape, shape, repository);
		}


		private void InsertShape(Shape shape, Shape parent, IRepository repository) {
			repository.Insert(shape, parent);
			foreach (Shape childShape in shape.Children)
				InsertShape(childShape, shape, repository);
		}


		private void CreateAndInsertModelObject(IModelObject sourceModelObject, IRepository repository) {
			IModelObject newModelObject = sourceModelObject.Clone();
			newModelObject.Name = GetName(sourceModelObject.Name, EditContentMode.Insert);
			repository.Insert(newModelObject);
		}

		#endregion


		#region Repository test helper methods - Modify content

		private void ModifyAndUpdateContent(Project project, bool withContent) {
			IRepository repository = project.Repository;
			//
			// Modify designs and styles
			foreach (Design design in repository.GetDesigns())
				ModifyAndUpdateDesign(design, repository);
			//
			// Modify templates
			foreach (Template template in repository.GetTemplates())
				ModifyAndUpdateTemplate(template, repository, withContent);
			//
			// Modify model objects
			foreach (IModelObject modelObject in repository.GetModelObjects(null))
				ModifyAndUpdateModelObject(modelObject, repository, withContent);
			//
			// Modify diagrams and shapes
			foreach (Diagram diagram in repository.GetDiagrams())
				ModifyAndUpdateDiagram(diagram, repository, withContent);
		}


		private void ModifyAndUpdateDesign(Design design, IRepository repository) {
			// Buffer for iterating through the styles while changing their names (StyleCollection's key).
			List<IStyle> styleBuffer = new List<IStyle>();
			// Anonymous delegate for performing the 
			Action<IEnumerable<IStyle>> modifyStylesAction = new Action<IEnumerable<IStyle>>((styleCollection) => {
				styleBuffer.AddRange(styleCollection);
				foreach (IStyle style in styleBuffer) {
					if (style is CapStyle) ModifyAndUpdateStyle((CapStyle)style, design, repository);
					else if (style is CharacterStyle) ModifyAndUpdateStyle((CharacterStyle)style, design, repository);
					else if (style is ColorStyle) ModifyAndUpdateStyle((ColorStyle)style, design, repository);
					else if (style is FillStyle) ModifyAndUpdateStyle((FillStyle)style, design, repository);
					else if (style is LineStyle) ModifyAndUpdateStyle((LineStyle)style, design, repository);
					else if (style is ParagraphStyle) ModifyAndUpdateStyle((ParagraphStyle)style, design, repository);
				}
				styleBuffer.Clear();
			});

			modifyStylesAction(design.CapStyles);
			modifyStylesAction(design.CharacterStyles);
			modifyStylesAction(design.FillStyles);
			modifyStylesAction(design.LineStyles);
			modifyStylesAction(design.ParagraphStyles);
			modifyStylesAction(design.ColorStyles);
		}


		private void ModifyAndUpdateBaseStyle(Style style, Design design, IRepository repository) {
			if (!design.IsStandardStyle(style))
				style.Name = GetName(style.Name, EditContentMode.Modify);
			style.Title = GetName(style.Title, EditContentMode.Modify).ToLower();
			repository.Update(style);
		}


		private void ModifyAndUpdateStyle(CapStyle style, Design design, IRepository repository) {
			bool use_standard_style = design.ColorStyles.IsStandardStyle((ColorStyle)style.ColorStyle);
			style.ColorStyle = GetNextValue(design.ColorStyles, style.ColorStyle, (s) => { return design.ColorStyles.IsStandardStyle((ColorStyle)s) == use_standard_style; });
			style.CapShape = Enum<CapShape>.GetNextValue(style.CapShape);
			style.CapSize += 1;
			ModifyAndUpdateBaseStyle(style, design, repository);
		}


		private void ModifyAndUpdateStyle(CharacterStyle style, Design design, IRepository repository) {
			bool use_standard_style = design.ColorStyles.IsStandardStyle((ColorStyle)style.ColorStyle);
			style.ColorStyle = GetNextValue(design.ColorStyles, style.ColorStyle, (s) => { return design.ColorStyles.IsStandardStyle((ColorStyle)s) == use_standard_style; });
			style.FontFamily = GetNextValue(FontFamily.Families, style.FontFamily);
			style.Size += 1;
			style.Style = Enum<FontStyle>.GetNextValue(style.Style);
			ModifyAndUpdateBaseStyle(style, design, repository);
		}


		private void ModifyAndUpdateStyle(ColorStyle style, Design design, IRepository repository) {
			int r = style.Color.R;
			int g = style.Color.G;
			int b = style.Color.B;
			style.Color = Color.FromArgb(g, b, r);
			style.ConvertToGray = !style.ConvertToGray;
			style.Transparency = (style.Transparency <= 50) ? (byte)75 : (byte)25;
			ModifyAndUpdateBaseStyle(style, design, repository);
		}


		private void ModifyAndUpdateStyle(FillStyle style, Design design, IRepository repository) {
			bool use_standard_style = design.ColorStyles.IsStandardStyle((ColorStyle)style.BaseColorStyle);
			style.BaseColorStyle = GetNextValue(design.ColorStyles, style.BaseColorStyle, (s) => { return design.ColorStyles.IsStandardStyle((ColorStyle)s) == use_standard_style; });
			style.AdditionalColorStyle = GetNextValue(design.ColorStyles, style.AdditionalColorStyle, (s) => { return design.ColorStyles.IsStandardStyle((ColorStyle)s) == use_standard_style; });
			style.ConvertToGrayScale = !style.ConvertToGrayScale;
			style.FillMode = Enum<FillMode>.GetNextValue(style.FillMode);
			style.FillPattern = Enum<System.Drawing.Drawing2D.HatchStyle>.GetNextValue(style.FillPattern);
			style.ImageGammaCorrection += 0.1f;
			style.ImageLayout = Enum<ImageLayoutMode>.GetNextValue(style.ImageLayout);
			style.ImageTransparency = (style.ImageTransparency < 100) ?
				(byte)(style.ImageTransparency + 1) : (byte)(style.ImageTransparency - 1);
			ModifyAndUpdateBaseStyle(style, design, repository);
		}


		private void ModifyAndUpdateStyle(LineStyle style, Design design, IRepository repository) {
			bool use_standard_style = design.ColorStyles.IsStandardStyle((ColorStyle)style.ColorStyle);
			style.ColorStyle = GetNextValue(design.ColorStyles, style.ColorStyle, (s) => { return design.ColorStyles.IsStandardStyle((ColorStyle)s) == use_standard_style; });
			style.DashCap = Enum<System.Drawing.Drawing2D.DashCap>.GetNextValue(style.DashCap);
			style.DashType = Enum<DashType>.GetNextValue(style.DashType);
			style.LineJoin = Enum<System.Drawing.Drawing2D.LineJoin>.GetNextValue(style.LineJoin);
			style.LineWidth += 1;
			ModifyAndUpdateBaseStyle(style, design, repository);
		}


		private void ModifyAndUpdateStyle(ParagraphStyle style, Design design, IRepository repository) {
			style.Alignment = Enum<ContentAlignment>.GetNextValue(style.Alignment);
			style.Padding = new TextPadding(style.Padding.Left + 1, style.Padding.Top + 1, style.Padding.Right + 1, style.Padding.Bottom + 1);
			style.Trimming = Enum<StringTrimming>.GetNextValue(style.Trimming);
			style.WordWrap = !style.WordWrap;
			ModifyAndUpdateBaseStyle(style, design, repository);
		}


		private void ModifyAndUpdateTemplate(Template template, IRepository repository, bool withContent) {
			template.Description = GetName(template.Description, EditContentMode.Modify);
			template.Name = GetName(template.Name, EditContentMode.Modify);

			// Assign a new child shape with a new child modelObject
			Shape newChildShape = template.Shape.Clone();
			if (newChildShape.ModelObject != null) newChildShape.ModelObject = null;
			template.Shape.Children.Add(newChildShape);
			if (template.Shape.ModelObject != null) {
				// ToDo: ModelObjects of child shapes and child model objects are not supported in the current version

				//newShape.ModelObject = template.Shape.ModelObject.Clone();
				//newShape.ModelObject.Parent = template.Shape.ModelObject;
				//repository.InsertModelObject(newShape.ModelObject);
			}

			// Modify property mappings
			foreach (IModelMapping modelMapping in template.GetPropertyMappings())
				ModifyModelMapping(modelMapping, repository.GetDesign(null));

			// Modify terminal mappings
			if (template.Shape.ModelObject != null) {
				// Get all mapped point- and terminal ids
				List<ControlPointId> pointIds = new List<ControlPointId>();
				List<TerminalId> terminalIds = new List<TerminalId>();
				foreach (ControlPointId pointId in template.Shape.GetControlPointIds(ControlPointCapabilities.All)) {
					TerminalId terminalId = template.GetMappedTerminalId(pointId);
					if (terminalId != TerminalId.Invalid) {
						pointIds.Add(pointId);
						terminalIds.Add(terminalId);
					}
				}
				// Now reverse the mappings
				Assert.AreEqual(pointIds.Count, terminalIds.Count);
				int maxIdx = pointIds.Count - 1;
				for (int i = 0; i <= maxIdx; ++i)
					template.MapTerminal(terminalIds[i], pointIds[maxIdx - i]);
				// If there are no terminal mappings, map all connection points to the generic terminal
				if (pointIds.Count == 0) {
					GenericModelObject genericModel = template.Shape.ModelObject as GenericModelObject;
					TerminalId terminalId = genericModel.Type.MaxTerminalId;
					foreach (ControlPointId pointId in template.Shape.GetControlPointIds(ControlPointCapabilities.Connect)) {
						template.MapTerminal(terminalId, pointId);
						terminalId = (terminalId > 1) ? (TerminalId)(terminalId - 1) : genericModel.Type.MaxTerminalId;
					}
				}
			}

			// Update repository
			//if (withContent) {
			//    repository.UpdateAll(template);
			//} else {
			repository.Insert(newChildShape, template.Shape);
			repository.Update(template);
			repository.Update(template.GetPropertyMappings());
			//}
		}


		private void ModifyModelMapping(IModelMapping modelMapping, Design design) {
			if (modelMapping is NumericModelMapping) {
				NumericModelMapping numericMapping = (NumericModelMapping)modelMapping;
				numericMapping.Intercept += 10;
				numericMapping.Slope += 1;
			} else if (modelMapping is FormatModelMapping) {
				FormatModelMapping formatMapping = (FormatModelMapping)modelMapping;
				formatMapping.Format = GetName(formatMapping.Format, EditContentMode.Modify);
			} else if (modelMapping is StyleModelMapping) {
				StyleModelMapping styleMapping = (StyleModelMapping)modelMapping;
				List<object> ranges = new List<object>(styleMapping.ValueRanges);
				foreach (object range in ranges) {
					IStyle currentStyle = null;
					if (range is float)
						currentStyle = styleMapping[(float)range];
					else if (range is int)
						currentStyle = styleMapping[(int)range];

					IStyle newStyle = null;
					if (currentStyle is CapStyle)
						newStyle = GetNextValue(design.CapStyles, (CapStyle)currentStyle);
					else if (currentStyle is CharacterStyle)
						newStyle = GetNextValue(design.CharacterStyles, (CharacterStyle)currentStyle);
					else if (currentStyle is ColorStyle)
						newStyle = GetNextValue(design.ColorStyles, (ColorStyle)currentStyle);
					else if (currentStyle is FillStyle)
						newStyle = GetNextValue(design.FillStyles, (FillStyle)currentStyle);
					else if (currentStyle is LineStyle)
						newStyle = GetNextValue(design.LineStyles, (LineStyle)currentStyle);
					else if (currentStyle is ParagraphStyle)
						newStyle = GetNextValue(design.ParagraphStyles, (ParagraphStyle)currentStyle);

					if (range is float) {
						styleMapping.RemoveValueRange((float)range);
						styleMapping.AddValueRange((float)range, newStyle);
					} else if (range is int) {
						styleMapping.RemoveValueRange((int)range);
						styleMapping.AddValueRange((int)range, newStyle);
					}
				}
			}
		}


		private void ModifyAndUpdateDiagram(Diagram diagram, IRepository repository, bool withContents) {
			// Modify "base" properties
			Color backColor = diagram.BackgroundColor;
			Color gradientColor = diagram.BackgroundGradientColor;
			diagram.BackgroundGradientColor = backColor;
			diagram.BackgroundColor = gradientColor;
			diagram.BackgroundImageGamma += 0.1f;
			diagram.BackgroundImageGrayscale = !diagram.BackgroundImageGrayscale;
			diagram.BackgroundImageLayout = Enum<ImageLayoutMode>.GetNextValue(diagram.BackgroundImageLayout);
			diagram.BackgroundImageTransparency = (diagram.BackgroundImageTransparency <= 50) ? (byte)75 : (byte)25;
			diagram.BackgroundImageTransparentColor = Color.FromArgb(
				diagram.BackgroundImageTransparentColor.G,
				diagram.BackgroundImageTransparentColor.B,
				diagram.BackgroundImageTransparentColor.R);
			diagram.Height += 200;
			diagram.Width += 200;
			diagram.Name = GetName(diagram.Name, EditContentMode.Modify);
			diagram.Title = GetName(diagram.Title, EditContentMode.Modify).ToLower();

			// Modify layers
			foreach (Layer layer in diagram.Layers) {
				layer.LowerZoomThreshold += 1;
				layer.Title = GetName(layer.Title, EditContentMode.Modify).ToLower();
				layer.UpperZoomThreshold += 1;
			}
			repository.Update(diagram);

			// Modify shapes
			foreach (Shape shape in diagram.Shapes)
				ModifyAndUpdateShape(shape, repository, withContents);
		}


		private void ModifyAndUpdateModelObject(IModelObject modelObject, IRepository repository, bool withContent) {
			if (modelObject is GenericModelObject)
				ModifyModelObject((GenericModelObject)modelObject);

			// ToDo: ModelObjects of child shapes and child model objects are not supported in the current version
			//if (repository is CachedRepository) {
			//    CachedRepository cachedRepository = (CachedRepository)repository;
			//    // Add child model objects only for non-XMLStores
			//    if (!(cachedRepository.Store is XmlStore)) {
			//        IModelObject child = modelObject.Clone();
			//        child.Parent = modelObject;

			//        // Update repository
			//        repository.AddModelObject(child);
			//        repository.UpdateOwner(child, modelObject);
			//    }
			//}

			// Update repository
			// ToDo: Implement ModelObjectWithContents methods in Repository
			repository.Update(modelObject);
		}


		private void ModifyModelObject(GenericModelObject modelObject) {
			modelObject.FloatValue += 1.1f;
			modelObject.IntegerValue += 1;
			modelObject.StringValue += " Modified";
		}


		private void ModifyAndUpdateShape(Shape shape, IRepository repository, bool withContent) {
			Design design = repository.GetDesign(null);

			Shape childShape = shape.Clone();
			shape.Children.Add(childShape);
			shape.LineStyle = GetNextValue(design.LineStyles, shape.LineStyle);
			shape.MoveBy(100, 100);
			shape.SecurityDomainName = (char)(((int)shape.SecurityDomainName) + 1);
			if (shape is ILinearShape)
				ModifyAndUpdateShape((ILinearShape)shape, design);
			else if (shape is IPlanarShape)
				ModifyAndUpdateShape((IPlanarShape)shape, design);
			else if (shape is ICaptionedShape)
				ModifyAndUpdateShape((ICaptionedShape)shape, design);

			// Update repository
			//if (withContent)
			//    repository.UpdateAll(shape);
			//else {
			repository.Insert(childShape, shape);
			repository.Update(shape);
			//}
		}


		private void ModifyAndUpdateShape(ILinearShape line, Design design) {
			// ToDo: Modify line specific properties
		}


		private void ModifyAndUpdateShape(IPlanarShape shape, Design design) {
			shape.Angle += 10;
			// In case the fill style is a styndard style, try to get a new/modified style
			IFillStyle fillStyle = null;
			if (design.IsStandardStyle(shape.FillStyle)) {
				// Get new style based on the current standard style
				string styleName = GetNewStyleName(shape.FillStyle, design);
				if (design.FillStyles.Contains(styleName))
					fillStyle = design.FillStyles[styleName];
			}
			shape.FillStyle = fillStyle ?? GetNextValue(design.FillStyles, shape.FillStyle);
		}


		private void ModifyAndUpdateShape(ICaptionedShape shape, Design design) {
			int cnt = shape.CaptionCount;
			for (int i = 0; i < cnt; ++i) {
				string txt = shape.GetCaptionText(i);
				shape.SetCaptionText(i, txt + "Modified");
				ICharacterStyle characterStyle = shape.GetCaptionCharacterStyle(i);
				shape.SetCaptionCharacterStyle(i, GetNextValue(design.CharacterStyles, characterStyle));
				IParagraphStyle paragraphStyle = shape.GetCaptionParagraphStyle(i);
				shape.SetCaptionParagraphStyle(i, GetNextValue(design.ParagraphStyles, paragraphStyle));
			}
		}

		#endregion


		#region Repository test helper methods - Delete content

		private void DeleteContent(Project project, bool withContents) {
			IRepository repository = project.Repository;

			// Delete diagrams and shapes
			DeleteDiagrams(repository, withContents);

			// Delete templates
			DeleteTemplates(repository, withContents);

			// Delete all additional designs
			DeleteDesigns(repository, withContents);

			// Delete model objects
			DeleteModelObjects(repository);
		}


		private void DeleteDesigns(IRepository repository, bool withContents) {
			Design projectDesign = repository.GetDesign(null);
			List<Design> designs = new List<Design>(repository.GetDesigns());
			foreach (Design design in designs) {
				if (design == projectDesign) continue;
				if (withContents)
					repository.DeleteAll(design);
				else {
					// Delete only non-standard styles
					DeleteStyles(design, design.CapStyles, repository);
					DeleteStyles(design, design.CharacterStyles, repository);
					DeleteStyles(design, design.FillStyles, repository);
					DeleteStyles(design, design.LineStyles, repository);
					DeleteStyles(design, design.ParagraphStyles, repository);
					DeleteStyles(design, design.ColorStyles, repository);
					// Detete design
					repository.Delete(design);
				}
			}
		}


		private void DeleteStyles<TStyle>(Design design, IEnumerable<TStyle> styles, IRepository repository) where TStyle : IStyle {
			List<TStyle> styleList = new List<TStyle>(styles);
			foreach (TStyle style in styleList) {
				if (!design.IsStandardStyle(style)) {
					design.RemoveStyle(style);
					repository.Delete(style);
				}
			}
		}


		private void DeleteTemplates(IRepository repository, bool withContent) {
			List<Template> templates = new List<Template>(repository.GetTemplates());
			foreach (Template t in templates) {
				if (IsNameOf(EditContentMode.Insert, t.Name)) {
					// If the template was inserted, delete it
					if (withContent)
						repository.DeleteAll(t);
					else {
						repository.Delete(t.GetPropertyMappings());
						IModelObject modelObj = t.Shape.ModelObject;
						if (modelObj != null) {
							t.Shape.ModelObject = null;
							repository.Delete(modelObj);
						}
						DeleteShape(t.Shape, repository, withContent);
						repository.Delete(t);
					}
				} else if (IsNameOf(EditContentMode.Modify, t.Name)) {
					// If the template was modified, delete some of it's content 
					//
					// Delete child model objects
					IModelObject modelObject = t.Shape.ModelObject;
					if (modelObject != null) {
						DeleteChildModelObjects(modelObject, repository);

						// Delete ModelMappings
						List<IModelMapping> modelMappings = new List<IModelMapping>(t.GetPropertyMappings());
						t.UnmapAllProperties();
						repository.Delete(modelMappings);
					}

					// Delete child shapes
					if (t.Shape.Children.Count > 0) {
						List<Shape> children = new List<Shape>(t.Shape.Children);
						if (withContent)
							repository.DeleteAll(children);
						else {
							foreach (Shape childShape in children) {
								DeleteShape(childShape, repository, withContent);
								t.Shape.Children.Remove(childShape);
							}
						}
						t.Shape.Children.Clear();
					}
					repository.Update(t);
				}
			}
		}


		private void DeleteModelObjects(IRepository repository) {
			List<IModelObject> modelObjects = new List<IModelObject>(repository.GetModelObjects(null));
			foreach (IModelObject m in modelObjects) {
				if (IsNameOf(EditContentMode.Insert, m.Name)) {
					// If the model object was inserted, delete it
					DeleteModelObject(m, repository);
				} else if (IsNameOf(EditContentMode.Modify, m.Name)) {
					// If the model object was modified, delete it's children 
					DeleteChildModelObjects(m, repository);
				}
			}
		}


		private void DeleteModelObject(IModelObject modelObject, IRepository repository) {
			Assert.IsNotNull(modelObject);
			DeleteChildModelObjects(modelObject, repository);
			List<Shape> shapes = new List<Shape>(modelObject.Shapes);
			for (int i = shapes.Count - 1; i >= 0; --i)
				shapes[i].ModelObject = null;
			repository.Delete(modelObject);
		}


		private void DeleteChildModelObjects(IModelObject parent, IRepository repository) {
			List<IModelObject> children = new List<IModelObject>(repository.GetModelObjects(parent));
			for (int i = children.Count - 1; i >= 0; --i)
				DeleteChildModelObjects(children[i], repository);
			repository.Delete(children);
		}


		private void DeleteDiagrams(IRepository repository, bool withContent) {
			List<Diagram> diagrams = new List<Diagram>(repository.GetDiagrams());
			for (int i = diagrams.Count - 1; i >= 0; --i) {
				if (IsNameOf(EditContentMode.Insert, diagrams[i].Name)) {
					if (withContent) {
						foreach (Shape shape in diagrams[i].Shapes)
							DetachModelObjects(shape);
						repository.DeleteAll(diagrams[i]);
					} else {
						// Delete diagram manually:
						// First, delete all shape connections and all shapes of the diagram
						List<Shape> shapes = new List<Shape>(diagrams[i].Shapes.BottomUp);
						foreach (Shape shape in shapes) {
							foreach (ShapeConnectionInfo sci in shape.GetConnectionInfos(ControlPointId.Any, null)) {
								if (shape.HasControlPointCapability(sci.OwnPointId, ControlPointCapabilities.Glue))
									repository.DeleteConnection(shape, sci.OwnPointId, sci.OtherShape, sci.OtherPointId);
							}
							DeleteShape(shape, repository, withContent);
						}
						// Afterwards, the diagram itself can be deleted.
						repository.Delete(diagrams[i]);
					}
				} else if (IsNameOf(EditContentMode.Modify, diagrams[i].Name)) {
					// Delete every second shapes
					DeleteSomeShapes(diagrams[i].Shapes.TopDown, repository, withContent);

					// Delete layers
					List<Layer> layers = new List<Layer>();
					for (int j = layers.Count - 1; j >= 0; --j) {
						if (j % 2 == 0) diagrams[j].Layers.Remove(layers[j]);
					}
				}
			}
		}


		private void DetachModelObjects(Shape shape) {
			foreach (Shape childShape in shape.Children)
				if (childShape.ModelObject != null) DetachModelObjects(childShape);
			if (shape.ModelObject != null) shape.ModelObject = null;
		}


		private void DeleteSomeShapes(IEnumerable<Shape> shapes, IRepository repository, bool withContent) {
			List<Shape> shapeList = new List<Shape>(shapes);
			for (int i = shapeList.Count - 1; i >= 0; --i) {
				if (i % 2 == 0)
					DeleteShape(shapeList[i], repository, withContent);
			}
		}


		private void DeleteShape(Shape shape, IRepository repository, bool withContent) {
			// Delete shape from repository (connections to other shapes will NOT be deleted automatically)
			List<ShapeConnectionInfo> connections = new List<ShapeConnectionInfo>(shape.GetConnectionInfos(ControlPointId.Any, null));
			foreach (ShapeConnectionInfo ci in connections) {
				Assert.IsFalse(ci == ShapeConnectionInfo.Empty);
				if (ci.OtherShape.Diagram == null) continue;    // Skip connections to shapes that are already deleted
				if (shape.HasControlPointCapability(ci.OwnPointId, ControlPointCapabilities.Glue)) {
					repository.DeleteConnection(shape, ci.OwnPointId, ci.OtherShape, ci.OtherPointId);
					shape.Disconnect(ci.OwnPointId);
				} else {
					repository.DeleteConnection(ci.OtherShape, ci.OtherPointId, shape, ci.OwnPointId);
					shape.Disconnect(ci.OtherPointId);
				}
			}

			if (withContent) {
				DetachModelObjects(shape);
				repository.DeleteAll(shape);
			} else {
				// Delete child shapes
				foreach (Shape childShape in shape.Children)
					DeleteShape(childShape, repository, withContent);
				// Delete shape
				DetachModelObjects(shape);
				repository.Delete(shape);
			}

			// Disconnect shapes
			foreach (ShapeConnectionInfo ci in connections) {
				if (shape.HasControlPointCapability(ci.OwnPointId, ControlPointCapabilities.Glue))
					shape.Disconnect(ci.OwnPointId);
				else ci.OtherShape.Disconnect(ci.OtherPointId);
			}
			// Clear children
			shape.Children.Clear();
			// Remove from diagram
			if (shape.Diagram != null)
				shape.Diagram.Shapes.Remove(shape);
			else if (shape.Parent != null)
				shape.Parent.Children.Remove(shape);
		}

		#endregion


		#region Repository test helper methods

		private static void Prepare(Project project, Store store, string projectName, string diagramName, int shapesPerRow, bool connectShapes, bool withModels, bool withTerminalMappings, bool withModelMappings, bool withLayers) {
			Prepare(project, store, projectName, diagramName, shapesPerRow, connectShapes, withModels, withTerminalMappings, withModelMappings, withLayers, null);
		}


		private static void Prepare(Project project, Store store, string projectName, string diagramName, int shapesPerRow, bool connectShapes, bool withModels, bool withTerminalMappings, bool withModelMappings, bool withLayers, IList<String> shapeTypeNames) {
			project.Name = projectName;
			project.AutoLoadLibraries = true;
			project.Repository = new CachedRepository();
			((CachedRepository)project.Repository).Store = store;

			// Delete the current project (if it exists)
			if (project.Repository.Exists())
				project.Repository.Erase();
			project.Create();
			project.AddLibrary(typeof(Dataweb.NShape.GeneralShapes.Circle).Assembly, true);

			// Create test data, populate repository and save repository
			DiagramHelper.CreateDiagram(project, diagramName, shapesPerRow, shapesPerRow, connectShapes, withModels, withTerminalMappings, withModelMappings, withLayers, shapeTypeNames);
		}


		private void SaveAndClose(Project project) {
			project.Repository.SaveChanges();
			project.Close();
		}


		private T GetNextValue<T>(IEnumerable<T> collection, T currentValue) where T : class {
			return GetNextValue(collection, currentValue, null);
		}


		private T GetNextValue<T>(IEnumerable<T> collection, T currentValue, Predicate<T> predicate)
			where T : class {
			T result = null;
			IEnumerator<T> enumerator = collection.GetEnumerator();
			while (enumerator.MoveNext()) {
				if (result == null) result = enumerator.Current;
				if (enumerator.Current == currentValue) {
					while (enumerator.MoveNext()) {
						if (predicate != null && !predicate(enumerator.Current))
							continue;
						result = enumerator.Current;
						break;
					}
				}
			}
			return result;
		}


		private string GetNewStyleName<TStyle>(TStyle style, Design design)
			where TStyle : IStyle {
			string result = GetName(style.Name, EditContentMode.Insert);
			if (design.ColorStyles.Contains(result)) {
				result = result + " ({0})";
				int i = 1;
				while (design.ColorStyles.Contains(string.Format(result, i))) ++i;
				result = string.Format(result, i);
			}
			return result;
		}


		private string GetName(string name, EditContentMode mode) {
			string result;
			switch (mode) {
				case EditContentMode.Insert:
					result = NewNamePrefix + " " + name; break;
				case EditContentMode.Modify:
					result = ModifiedNamePrefix + " " + name; break;
				default:
					Debug.Fail(string.Format("Unexpected {0} value '{1}'", typeof(EditContentMode).Name, mode));
					result = name; break;
			}
			return result;
		}


		private bool IsNameOf(EditContentMode mode, string name) {
			bool result;
			switch (mode) {
				case EditContentMode.Insert:
					result = name.StartsWith(NewNamePrefix); break;
				case EditContentMode.Modify:
					result = name.StartsWith(ModifiedNamePrefix);
					break;
				default:
					Debug.Fail(string.Format("Unexpected {0} value '{1}'", typeof(EditContentMode).Name, mode));
					result = false; break;
			}
			return result;
		}

		#endregion


		private enum EditContentMode { Insert, Modify };

		private String[] GeneralShapeTypeNames = new String[] {
			"Polyline", 
			//"RectangularLine", // Causes arithmetic overflow while MapInsert, ending up in an endless loop!
			"CircularArc", "Text", "Label", "RegularPolygone", "FreeTriangle", "IsoscelesTriangle",
			"Circle", "Ellipse", "Square", "Box", "RoundedBox", "Diamond", "ThickArrow", "Picture"
		};

		private const string NewNamePrefix = "Copy of";
		private const string ModifiedNamePrefix = "Modified";
	}

}

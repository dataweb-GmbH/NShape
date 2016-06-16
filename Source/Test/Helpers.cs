/******************************************************************************
  Copyright 2009-2014 dataweb GmbH
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
using System.Data.SqlClient;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.FlowChartShapes;
using Dataweb.NShape.GeneralModelObjects;
using Dataweb.NShape.GeneralShapes;


namespace NShapeTest {

	public static class DiagramHelper {

		public static void CreateLargeDiagram(Project project, string diagramName) {
			const int shapesPerSide = 100;
			CreateDiagram(project, diagramName, shapesPerSide, shapesPerSide, true, false, false, false, true);
		}


		public static void CreateDiagram(Project project, string diagramName, int shapesPerRow, int shapesPerColumn, bool connectShapes, bool withModels, bool withTerminalMappings, bool withModelMappings, bool withLayers) {
			CreateDiagram(project, diagramName, shapesPerRow, shapesPerColumn, connectShapes, withModels, withTerminalMappings, withModelMappings, withLayers, null);
		}


		public static void CreateDiagram(Project project, string diagramName, int shapesPerRow, int shapesPerColumn, bool connectShapes, bool withModels, bool withTerminalMappings, bool withModelMappings, bool withLayers, IList<String> shapeTypeNames) {
			const int shapeSize = 80;
			int lineLength = shapeSize / 2;
			//
			// Create the templates
			CreateTemplatesFromShapeTypes(project, shapeTypeNames, shapeSize, withModels, withTerminalMappings, withModelMappings, shapesPerRow * shapesPerColumn);
			//
			// Prepare the connection points
			ControlPointId leftPoint = withModels ? ControlPointId.Reference : 4;
			ControlPointId rightPoint = withModels ? ControlPointId.Reference : 5;
			ControlPointId topPoint = withModels ? ControlPointId.Reference : 2;
			ControlPointId bottomPoint = withModels ? ControlPointId.Reference : 7;
			//
			// Create the diagram
			Diagram diagram = new Diagram(diagramName);
			//
			// Create and add layers
			LayerIds planarLayer = LayerIds.None, linearLayer = LayerIds.None, oddRowLayer = LayerIds.None,
				evenRowLayer = LayerIds.None, oddColLayer = LayerIds.None, evenColLayer = LayerIds.None;
			if (withLayers) {
				const string planarLayerName = "PlanarShapesLayer";
				const string linearLayerName = "LinearShapesLayer";
				const string oddRowsLayerName = "OddRowsLayer";
				const string evenRowsLayerName = "EvenRowsLayer";
				const string oddColsLayerName = "OddColsLayer";
				const string evenColsLayerName = "EvenColsLayer";
				// Create Layers
				Layer planarShapesLayer = new Layer(planarLayerName);
				planarShapesLayer.Title = "Planar Shapes";
				planarShapesLayer.LowerZoomThreshold = 5;
				planarShapesLayer.UpperZoomThreshold = 750;
				diagram.Layers.Add(planarShapesLayer);
				Layer linearShapesLayer = new Layer(linearLayerName);
				linearShapesLayer.Title = "Linear Shapes";
				linearShapesLayer.LowerZoomThreshold = 10;
				linearShapesLayer.UpperZoomThreshold = 500;
				diagram.Layers.Add(linearShapesLayer);
				Layer oddRowsLayer = new Layer(oddRowsLayerName);
				oddRowsLayer.Title = "Odd Rows";
				oddRowsLayer.LowerZoomThreshold = 2;
				oddRowsLayer.UpperZoomThreshold = 1000;
				diagram.Layers.Add(oddRowsLayer);
				Layer evenRowsLayer = new Layer(evenRowsLayerName);
				evenRowsLayer.Title = "Even Rows";
				evenRowsLayer.LowerZoomThreshold = 2;
				evenRowsLayer.UpperZoomThreshold = 1000;
				diagram.Layers.Add(evenRowsLayer);
				Layer oddColsLayer = new Layer(oddColsLayerName);
				oddColsLayer.Title = "Odd Columns";
				oddColsLayer.LowerZoomThreshold = 2;
				oddColsLayer.UpperZoomThreshold = 1000;
				diagram.Layers.Add(oddColsLayer);
				Layer evenColsLayer = new Layer(evenColsLayerName);
				evenColsLayer.Title = "Even Columns";
				evenColsLayer.LowerZoomThreshold = 2;
				evenColsLayer.UpperZoomThreshold = 1000;
				diagram.Layers.Add(evenColsLayer);
				// Assign LayerIds
				planarLayer = diagram.Layers.FindLayer(planarLayerName).Id;
				linearLayer = diagram.Layers.FindLayer(linearLayerName).Id;
				oddRowLayer = diagram.Layers.FindLayer(oddRowsLayerName).Id;
				evenRowLayer = diagram.Layers.FindLayer(evenRowsLayerName).Id;
				oddColLayer = diagram.Layers.FindLayer(oddColsLayerName).Id;
				evenColLayer = diagram.Layers.FindLayer(evenColsLayerName).Id;
			}

			Template planarTemplate = null;
			Template linearTemplate = null;
			int searchRange = shapeSize / 2;
			for (int rowIdx = 0; rowIdx < shapesPerRow; ++rowIdx) {
				LayerIds rowLayer = ((rowIdx + 1) % 2 == 0) ? evenRowLayer : oddRowLayer;
				for (int colIdx = 0; colIdx < shapesPerRow; ++colIdx) {
					LayerIds colLayer = ((colIdx + 1) % 2 == 0) ? evenColLayer : oddColLayer;
					int shapePosX = shapeSize + colIdx * (lineLength + shapeSize);
					int shapePosY = shapeSize + rowIdx * (lineLength + shapeSize);

					planarTemplate = GetNextPlanarTemplate(project, planarTemplate);
					Shape planarShape = planarTemplate.CreateShape();
					// Apply shape specific property values
					if (planarShape is PictureBase)
						((PictureBase)planarShape).Image = new NamedImage(Properties.Resources.SamplePicture, "Sample Picture");
					if (planarShape is ICaptionedShape)
						((ICaptionedShape)planarShape).SetCaptionText(0, string.Format("{0} / {1}", rowIdx + 1, colIdx + 1));
					planarShape.MoveTo(shapePosX, shapePosY);
					if (withModels) {
						project.Repository.Insert(planarShape.ModelObject);
						((GenericModelObject)planarShape.ModelObject).IntegerValue = rowIdx;
					}

					diagram.Shapes.Add(planarShape, project.Repository.ObtainNewTopZOrder(diagram));
					if (withLayers) diagram.AddShapeToLayers(planarShape, planarLayer | rowLayer | colLayer);
					if (connectShapes) {
						linearTemplate = GetNextLinearTemplate(project, linearTemplate);
						if (rowIdx > 0) {
							Shape lineShape = linearTemplate.CreateShape();
							if (planarShape.HasControlPointCapability(topPoint, ControlPointCapabilities.Connect)) {
								lineShape.Connect(ControlPointId.FirstVertex, planarShape, topPoint);
								Assert.AreNotEqual(ControlPointId.None, lineShape.IsConnected(ControlPointId.FirstVertex, planarShape));
							}
							Shape otherShape = diagram.Shapes.FindShape(shapePosX, shapePosY - shapeSize, ControlPointCapabilities.None, searchRange, null);
							if (otherShape != null && otherShape.HasControlPointCapability(bottomPoint, ControlPointCapabilities.Connect)) {
								lineShape.Connect(ControlPointId.LastVertex, otherShape, bottomPoint);
								Assert.AreNotEqual(ControlPointId.None, lineShape.IsConnected(ControlPointId.LastVertex, otherShape));
							}
							// Add line shape if at least one connection was established.
							if (lineShape.IsConnected(ControlPointId.FirstVertex, null) != ControlPointId.None && lineShape.IsConnected(ControlPointId.LastVertex, null) != ControlPointId.None) {
								diagram.Shapes.Add(lineShape, project.Repository.ObtainNewBottomZOrder(diagram));
								if (withLayers) diagram.AddShapeToLayers(lineShape, linearLayer);
							}
						}
						if (colIdx > 0) {
							Shape lineShape = linearTemplate.CreateShape();
							if (planarShape.HasControlPointCapability(leftPoint, ControlPointCapabilities.Connect)) {
								lineShape.Connect(1, planarShape, leftPoint);
								Assert.AreNotEqual(ControlPointId.None, lineShape.IsConnected(ControlPointId.FirstVertex, planarShape));
							}
							Shape otherShape = diagram.Shapes.FindShape(shapePosX - shapeSize, shapePosY, ControlPointCapabilities.None, searchRange, null);
							if (otherShape != null && otherShape.HasControlPointCapability(rightPoint, ControlPointCapabilities.Connect)) {
								lineShape.Connect(2, otherShape, rightPoint);
								Assert.AreNotEqual(ControlPointId.None, lineShape.IsConnected(ControlPointId.LastVertex, otherShape));
							}
							// Add line shape if at least one connection was established.
							if (lineShape.IsConnected(ControlPointId.FirstVertex, null) != ControlPointId.None && lineShape.IsConnected(ControlPointId.LastVertex, null) != ControlPointId.None) {
								diagram.Shapes.Add(lineShape, project.Repository.ObtainNewBottomZOrder(diagram));
								if (withLayers) diagram.AddShapeToLayers(lineShape, linearLayer);
							}
						}
					}
				}
			}
			diagram.Width = (lineLength + shapeSize) * shapesPerRow + 2 * shapeSize;
			diagram.Height = (lineLength + shapeSize) * shapesPerColumn + 2 * shapeSize;
			project.Repository.InsertAll(diagram);
		}


		private static void CreateTemplatesFromShapeTypes(Project project, IList<String> shapeTypeNames, Int32 shapeSize, Boolean withModels, Boolean withTerminalMappings, Boolean withModelMappings, Int32 expectedShapeCount) {
			if (shapeTypeNames == null) {
				shapeTypeNames = new List<String>();
				shapeTypeNames.Add("Circle");
				shapeTypeNames.Add("PolyLine");
			}
			//
			foreach (String shapeTypeName in shapeTypeNames) {
				ShapeType shapeType = project.ShapeTypes[shapeTypeName];
				// Create a shape for the template
				Shape shape = shapeType.CreateInstance();
				shape.Fit(0, 0, shapeSize, shapeSize);
				// Create the template
				Template template = new Template(String.Format("{0} Template", shapeType.Name), shapeType.CreateInstance());
				if (shape is IPlanarShape) {
					// Add optional data
					if (withModels) {
						template.Shape.ModelObject = project.ModelObjectTypes["Core.GenericModelObject"].CreateInstance();
						template.MapTerminal(TerminalId.Generic, ControlPointId.Reference);
						if (withTerminalMappings) {
							foreach (ControlPointId id in template.Shape.GetControlPointIds(ControlPointCapabilities.Connect))
								template.MapTerminal(TerminalId.Generic, id);
						}
						if (withModelMappings) {
							//
							// Create ModelMappings
							List<IModelMapping> modelMappings = new List<IModelMapping>(3);
							// Create numeric- and format model mappings
							NumericModelMapping numericMapping = new NumericModelMapping(2, 4, NumericModelMapping.MappingType.FloatInteger, 10, 0);
							FormatModelMapping formatMapping = new FormatModelMapping(4, 2, FormatModelMapping.MappingType.StringString, "{0}");
							// Create style model mapping
							float range = expectedShapeCount / 15f;
							StyleModelMapping styleModelMapping = new StyleModelMapping(1, 4, StyleModelMapping.MappingType.FloatStyle);
							for (int i = 0; i < 15; ++i) {
								IStyle style = null;
								switch (i) {
									case 0: style = project.Design.LineStyles.None; break;
									case 1: style = project.Design.LineStyles.Dotted; break;
									case 2: style = project.Design.LineStyles.Dashed; break;
									case 3: style = project.Design.LineStyles.Special1; break;
									case 4: style = project.Design.LineStyles.Special2; break;
									case 5: style = project.Design.LineStyles.Normal; break;
									case 6: style = project.Design.LineStyles.Blue; break;
									case 7: style = project.Design.LineStyles.Green; break;
									case 8: style = project.Design.LineStyles.Yellow; break;
									case 9: style = project.Design.LineStyles.Red; break;
									case 10: style = project.Design.LineStyles.HighlightDotted; break;
									case 11: style = project.Design.LineStyles.HighlightDashed; break;
									case 12: style = project.Design.LineStyles.Highlight; break;
									case 13: style = project.Design.LineStyles.HighlightThick; break;
									case 14: style = project.Design.LineStyles.Thick; break;
									default: style = null; break;
								}
								if (style != null) styleModelMapping.AddValueRange(i * range, style);
							}
							modelMappings.Add(styleModelMapping);
							// 
							foreach (IModelMapping modelMapping in modelMappings)
								template.MapProperties(modelMapping);
						}
					}
				} else if (shape is ILinearShape) {
					// Nothing else to do
				} else {
					throw new NotImplementedException();
				}
				// Insert the template into the repository
				project.Repository.InsertAll(template);
			}
			Assert.AreEqual(Counter.GetCount(project.Repository.GetTemplates()), shapeTypeNames.Count);			
		}


		public static Template GetNextPlanarTemplate(Project project, Template currentPlanarShapeTemplate) {
			return GetNextTemplateofType<IPlanarShape>(project, currentPlanarShapeTemplate);
		}


		public static Template GetNextLinearTemplate(Project project, Template currentLinearShapeTemplate) {
			return GetNextTemplateofType<ILinearShape>(project, currentLinearShapeTemplate);
		}


		public static Template GetNextTemplateofType<TShapeInterface>(Project project, Template currentPlanarTemplate) {
			Template result = null;
			IEnumerator<Template> template_enumerator = project.Repository.GetTemplates().GetEnumerator();
			// Skip all templates until the current template is reached
			if (currentPlanarTemplate != null) {
				while (template_enumerator.Current != currentPlanarTemplate)
					if (template_enumerator.MoveNext() == false) break;
			}
			while (template_enumerator.MoveNext()) {
				// When the current template is reached, move on to the next template
				Template template = template_enumerator.Current;
				if (template.Shape is TShapeInterface) {
					result = template;
					break;
				}
			}
			return result ?? GetNextTemplateofType<TShapeInterface>(project, null);
		}

	}
	
	
	public static class RepositoryHelper {

		public static SqlStore CreateSqlStore() {
			return CreateSqlStore(DatabaseName);
		}


		public static SqlStore CreateSqlStore(string databaseName) {
			string server = Environment.MachineName + SqlServerName;
			return new SqlStore(server, databaseName);
		}


		public static XmlStore CreateXmlStore() {
			return new XmlStore(Path.GetTempPath(), ".nspj");
		}


		public static XmlStore CreateXmlStore(Boolean lazyLoading, Boolean embeddedImages, Boolean automaticBackup) {
			XmlStore result = CreateXmlStore();
			result.LazyLoading = lazyLoading;
			result.BackupGenerationMode = automaticBackup ? XmlStore.BackupFileGenerationMode.BakFile : XmlStore.BackupFileGenerationMode.None;
			result.ImageLocation = embeddedImages ? XmlStore.ImageFileLocation.Embedded : XmlStore.ImageFileLocation.Directory;
			return result;
		}


		public static void SQLCreateDatabase() {
			string[] libraryNames = new string[]{
				"Dataweb.NShape.GeneralShapes",
				"Dataweb.NShape.FlowChartShapes"
			};
			SQLCreateDatabase(DatabaseName, Project.LastSupportedSaveVersion, libraryNames);
		}


		public static void SQLCreateDatabase(string databaseName, int version, IEnumerable<string> libraryNames) {
			using (SqlStore sqlStore = CreateSqlStore(databaseName)) {
				// Create database
				string connectionString = string.Format("server={0};Integrated Security=True", sqlStore.ServerName);
				using (SqlConnection conn = new SqlConnection(connectionString)) {
					conn.Open();
					try {
						SqlCommand command = conn.CreateCommand();
						command.CommandText = string.Format("CREATE DATABASE {0}", databaseName);
						command.ExecuteNonQuery();
					} catch (SqlException exc) {
						// Ignore "Database already exists" error
						if (exc.ErrorCode != sqlErrDatabaseExists) throw exc;
					}
				}

				// Create Repository
				CachedRepository repository = new CachedRepository();
				repository.Version = version;
				repository.Store = CreateSqlStore(databaseName);

				// Create project
				Project project = new Project();
				project.AutoLoadLibraries = true;
				project.Name = "...";
				project.Repository = repository;

				// Add and register libraries
				project.RemoveAllLibraries();
				project.AddLibrary(typeof(ValueDevice).Assembly, true);
				foreach (string libName in libraryNames)
					project.AddLibraryByName(libName, true);
				project.RegisterEntityTypes();

				// Create schema
				sqlStore.CreateDbCommands(repository);
				sqlStore.CreateDbSchema(repository);

				// Close project
				project.Close();
			}
		}


		public static void SQLDropDatabase() {
			SQLDropDatabase(DatabaseName);
		}


		public static void SQLDropDatabase(string databaseName) {
			string connectionString = string.Empty;
			using (SqlStore sqlStore = CreateSqlStore(databaseName)) {
				connectionString = string.Format("server={0};Integrated Security=True", sqlStore.ServerName);
				try {
					sqlStore.DropDbSchema();
				} finally {
					// Drop all connections of the connection pool created by the store's connection
					using (SqlConnection conn = new SqlConnection(sqlStore.ConnectionString))
						SqlConnection.ClearPool(conn);
				}
			}

			// Drop database
			if (!string.IsNullOrEmpty(connectionString)) {
				using (SqlConnection conn = new SqlConnection(connectionString)) {
					conn.Open();
					try {
						using (SqlCommand command = conn.CreateCommand()) {
							command.CommandText = string.Format("DROP DATABASE {0}", databaseName);
							command.ExecuteNonQuery();
						}
					} catch (SqlException exc) {
						if (exc.ErrorCode != sqlErrDatabaseExists) throw exc;
					}
				}
			}
		}


		private const int sqlErrDatabaseExists = -2146232060;
		public const string SqlServerName = "\\SQLEXPRESS";
		public const string DatabaseName = "NShapeSQLTest";
	}


	// Enum helper class
	public static class Enum<T> where T : struct, IComparable {

		public static T Parse(string value) {
			return (T)Enum.Parse(typeof(T), value);
		}


		public static IList<T> GetValues() {
			IList<T> list = new List<T>();
			foreach (object value in Enum.GetValues(typeof(T)))
				list.Add((T)value);
			return list;
		}


		public static T GetNextValue(T currentValue) {
			T result = default(T);
			IList<T> values = Enum<T>.GetValues();
			int cnt = values.Count;
			for (int i = 0; i < cnt; ++i) {
				if (values[i].Equals(currentValue)) {
					if (i + 1 < cnt) result = values[i + 1];
					else result = values[0];
					break;
				}
			}
			return result;
		}

	}

}

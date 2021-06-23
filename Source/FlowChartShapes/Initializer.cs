/******************************************************************************
  Copyright 2009-2021 dataweb GmbH
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

using Dataweb.NShape.Advanced;


namespace Dataweb.NShape.FlowChartShapes {

	public static class NShapeLibraryInitializer {

		public static void Initialize(IRegistrar registrar) {
			registrar.RegisterLibrary(namespaceName, preferredRepositoryVersion);
			registrar.RegisterShapeType(new ShapeType("Terminator", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new TerminatorSymbol(shapeType, t); },
				TerminatorSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Process", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new ProcessSymbol(shapeType, t); },
				ProcessSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Decision", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new DecisionSymbol(shapeType, t); },
				DecisionSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("InputOutput", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new InputOutputSymbol(shapeType, t); },
				InputOutputSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Document", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new DocumentSymbol(shapeType, t); },
				DocumentSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("OffpageConnector", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new OffpageConnectorSymbol(shapeType, t); },
				OffpageConnectorSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Connector", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new ConnectorSymbol(shapeType, t); },
				ConnectorSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("PredefinedProcess", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new PredefinedProcessSymbol(shapeType, t); },
				PredefinedProcessSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Extract", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new ExtractSymbol(shapeType, t); },
				ExtractSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Merge", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new MergeSymbol(shapeType, t); },
				MergeSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("OnlineStorage", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new OnlineStorageSymbol(shapeType, t); },
				OnlineStorageSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("OfflineStorage", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new OfflineStorageSymbol(shapeType, t); },
				OfflineStorageSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("DrumStorage", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new DrumStorageSymbol(shapeType, t); },
				DrumStorageSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("DiskStorage", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new DiskStorageSymbol(shapeType, t); },
				DiskStorageSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("TapeStorage", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new TapeStorageSymbol(shapeType, t); },
				TapeStorageSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Preparation", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new PreparationSymbol(shapeType, t); },
				PreparationSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("ManualInput", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new ManualInputSymbol(shapeType, t); },
				ManualInputSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Core", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new CoreSymbol(shapeType, t); },
				CoreSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Display", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new DisplaySymbol(shapeType, t); },
				DisplaySymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Tape", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new TapeSymbol(shapeType, t); },
				TapeSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("ManualOperation", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new ManualOperationSymbol(shapeType, t); },
				ManualOperationSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Sort", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new SortSymbol(shapeType, t); },
				SortSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Collate", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new CollateSymbol(shapeType, t); },
				CollateSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("Card", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new CardSymbol(shapeType, t); },
				CardSymbol.GetPropertyDefinitions));
			registrar.RegisterShapeType(new ShapeType("CommLink", namespaceName, namespaceName,
				delegate(ShapeType shapeType, Template t) { return new CommLinkSymbol(shapeType, t); },
				CommLinkSymbol.GetPropertyDefinitions));
		}


		private const string namespaceName = "FlowChartShapes";
		private const int preferredRepositoryVersion = 6;
	}

}

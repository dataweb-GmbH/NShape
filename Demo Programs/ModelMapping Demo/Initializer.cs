using Dataweb.NShape.Advanced;


namespace ModelMapping_Demo
{


	/// <summary>
	/// The Libraryinitializer is needed for registering the IModelObject implementations with the NShape framework.
	/// </summary>
	public static class NShapeLibraryInitializer
	{

		public static void Initialize(IRegistrar registrar)
		{
			registrar.RegisterLibrary(namespaceName, preferredRepositoryVersion);

			registrar.RegisterModelObjectType(new GenericModelObjectType("MyBusinessObject", namespaceName, categoryTitle,
				(ModelObjectType type) => { return new MyBusinessObject(type); }, MyBusinessObject.GetPropertyDefinitions, 0));

			registrar.RegisterDiagramModelObjectType(new GenericDiagramModelObjectType("MyDiagramBusinessObject", namespaceName, categoryTitle,
				(DiagramModelObjectType type) => { return new MyDiagramBusinessObject(type); }, MyDiagramBusinessObject.GetPropertyDefinitions));
		}


		#region Fields

		private const string namespaceName = "ModelMappingDemo";
		private const string categoryTitle = "Model Mapping Demo";
		private const int preferredRepositoryVersion = 1;

		#endregion

	}


}

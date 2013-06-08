namespace CommandProcessing.Data.Tests.TestHelpers
{
    using System;
    using System.Collections.ObjectModel;
    using System.Data.Common;
    using System.Data.Metadata.Edm;

    public class FakeManifest : DbProviderManifest
    {
        protected override System.Xml.XmlReader GetDbInformation(string informationType)
        {

            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the namespace used by this provider manifest.
        /// </summary>
        /// <returns>
        /// The namespace used by this provider manifest.
        /// </returns>
        public override string NamespaceName
        {
            get
            {
                return "Edm";
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns the set of primative types supported by the data source.
        /// </summary>
        /// <returns>
        /// The set of types supported by the data source.
        /// </returns>
        public override ReadOnlyCollection<PrimitiveType> GetStoreTypes()
        {
            return new ReadOnlyCollection<PrimitiveType>(new PrimitiveType[0]);
        }

        /// <summary>
        /// When overridden in a derived class, returns a collection of EDM functions supported by the provider manifest.
        /// </summary>
        /// <returns>
        /// A collection of EDM functions.
        /// </returns>
        public override ReadOnlyCollection<EdmFunction> GetStoreFunctions()
        {
            return new ReadOnlyCollection<EdmFunction>(new EdmFunction[0]);
        }

        /// <summary>
        /// Returns the <see cref="T:System.Data.Metadata.Edm.FacetDescription"/> objects for a particular type.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Data.Metadata.Edm.FacetDescription"/> objects for the specified EDM type.
        /// </returns>
        /// <param name="edmType">The EDM type to return the facet description for.</param>
        public override ReadOnlyCollection<FacetDescription> GetFacetDescriptions(EdmType edmType)
        {
            return new ReadOnlyCollection<FacetDescription>(new FacetDescription[0]);
        }

        public override System.Data.Metadata.Edm.TypeUsage GetEdmType(System.Data.Metadata.Edm.TypeUsage storeType)
        {
            return TypeUsage.CreateDefaultTypeUsage(storeType.EdmType);
        }

        public override System.Data.Metadata.Edm.TypeUsage GetStoreType(System.Data.Metadata.Edm.TypeUsage edmType)
        {
            //if (edmType.EdmType.Name == "String")
            //{
            //    return TypeUsage.GetBuiltInType(BuiltInTypeKind.PrimitiveType);
            //}
            return TypeUsage.CreateDefaultTypeUsage(edmType.EdmType);
        }
    }
}
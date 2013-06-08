namespace CommandProcessing.Data.Tests.TestHelpers
{
    using System;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.EntityClient;
    using System.Reflection;

    internal class FakeDbServices : DbProviderServices
    {
        private static readonly DbProviderServices providerServices;

        static FakeDbServices()
        {
            Type providerType = typeof(EntityProviderFactory).Assembly.GetType("System.Data.EntityClient.EntityProviderServices");
            var fieldInfo = providerType.GetField("Instance", BindingFlags.Static| BindingFlags.NonPublic);
            var value = fieldInfo.GetValue(null);
            providerServices = (DbProviderServices)value;
        }
        
        /// <summary>
        /// Creates a command definition object for the specified provider manifest and command tree.
        /// </summary>
        /// <returns>
        /// An exectable command definition object.
        /// </returns>
        /// <param name="providerManifest">Provider manifest previously retrieved from the store provider.</param><param name="commandTree">Command tree for the statement.</param>
        protected override DbCommandDefinition CreateDbCommandDefinition(DbProviderManifest providerManifest, DbCommandTree commandTree)
        {
            return providerServices.CreateCommandDefinition(providerManifest, commandTree);
        }

        /// <summary>
        /// Returns provider manifest token given a connection.
        /// </summary>
        /// <returns>
        /// The provider manifest token for the specified connection.
        /// </returns>
        /// <param name="connection">Connection to provider.</param>
        protected override string GetDbProviderManifestToken(DbConnection connection)
        {
            return "fake"; // + providerServices.GetProviderManifestToken(connection);
        }

        /// <summary>
        /// When overridden in a derived class, returns an instance of a class that derives from the <see cref="T:System.Data.Common.DbProviderManifest"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Data.Common.DbProviderManifest"/> object that represents the provider manifest.
        /// </returns>
        /// <param name="manifestToken">The token information associated with the provider manifest.</param>
        protected override DbProviderManifest GetDbProviderManifest(string manifestToken)
        {
            return new FakeManifest();
        }
    }
}
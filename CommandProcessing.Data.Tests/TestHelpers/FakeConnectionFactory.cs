namespace CommandProcessing.Data.Tests.TestHelpers
{
    using System.Configuration;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

    public class FakeConnectionFactory : IDbConnectionFactory
    {
        private static IDbConnectionFactory defaultConnectionFactory;

        public static void RegisterFactory<TContext>() where TContext : DbContext
        {
            DataSet registeredProvidersDataSet = (DataSet)ConfigurationManager.GetSection("system.data");
            DataRowCollection registeredProviders = registeredProvidersDataSet.Tables[0].Rows;

            try
            {
                registeredProviders.Add(new object[] { "FakeDbProviderFactory", "Generates Fake Db Objects for use in test code.  Does not make any actual Db connections.", typeof(FakeDbProviderFactory).FullName, typeof(FakeDbProviderFactory).AssemblyQualifiedName });
            }
            catch (ConstraintException)
            {
                // already added
            }

            defaultConnectionFactory = Database.DefaultConnectionFactory;
            Database.DefaultConnectionFactory = new FakeConnectionFactory();

            Database.SetInitializer<TContext>(new DoNotCreateDatabaseInitializer<TContext>());
        }

        public static void UnregisterFactory<TContext>() where TContext : DbContext
        {
            Database.DefaultConnectionFactory = defaultConnectionFactory;
            Database.SetInitializer<TContext>(new CreateDatabaseIfNotExists<TContext>());
        }

        public DbConnection CreateConnection(string sqlServerYear)
        {
            return new FakeDbConnection(sqlServerYear);
        }
    }

    internal class DoNotCreateDatabaseInitializer<TContext> : IDatabaseInitializer<TContext> where TContext : DbContext
    {
        public void InitializeDatabase(TContext context)
        {
            //do nothing
        }
    }

}
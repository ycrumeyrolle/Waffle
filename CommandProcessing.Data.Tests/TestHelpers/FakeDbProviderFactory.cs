namespace CommandProcessing.Data.Tests.TestHelpers
{
    using System;
    using System.Data.Common;

    public class FakeDbProviderFactory : DbProviderFactory, IServiceProvider
    {
        public static readonly FakeDbProviderFactory Instance = new FakeDbProviderFactory();

        public override DbConnection CreateConnection()
        {
            return new FakeDbConnection("fake");
        }
        
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(DbProviderServices))
            {
                return new FakeDbServices();
            }

            throw new NotImplementedException();
        }
    }
}
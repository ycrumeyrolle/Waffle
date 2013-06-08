namespace CommandProcessing.Data.Tests.TestHelpers
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Data.Entity;

    internal class FakeDbConnection : DbConnection
    {
        private DbProviderFactory dbProviderFactory;

        private ConnectionState state;

        public FakeDbConnection(string nameOrConnectionString)
        {
            this.ConnectionString = nameOrConnectionString;
            this.state = ConnectionState.Closed;
        }
        
        public override sealed string ConnectionString { get; set; }
        
        public override string Database
        {
            get
            {
                return string.Empty;
            }
        }

        public override string DataSource
        {
            get
            {
                return string.Empty;
            }
        }

        public override string ServerVersion
        {
            get
            {
                return string.Empty;
            }
        }

        public override ConnectionState State
        {
            get { return this.state; }
        }

        protected override DbProviderFactory DbProviderFactory
        {
            get
            {
                if (this.dbProviderFactory == null)
                {
                    this.dbProviderFactory = FakeDbProviderFactory.Instance;
                }

                return this.dbProviderFactory;
            }
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }
        
        public override void Open()
        {
            this.state = ConnectionState.Open;
        }

        public override void Close()
        {
            this.state = ConnectionState.Closed;
        }

        protected override DbCommand CreateDbCommand()
        {
            throw new NotImplementedException();
        }
    }
}

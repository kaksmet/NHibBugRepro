using System;
using System.Collections.Generic;
using System.Threading;
using System.Transactions;
using Microsoft.Data.SqlClient;
using NHibernate;

namespace NHibBugRepro {
    class Program {
        static readonly string connectionString = @"Data Source=YOUR_SERVER\INSTANCE; initial catalog=DATABASE; User ID=USER; Password=PASSWORD; TrustServerCertificate=true;";
        public static readonly string tableName = "a_table";
        static readonly int numThreads = 16;
        static readonly int numRows = 5_000;

        static void Main(string[] args) {
            var cfg = new NHibernate.Cfg.Configuration()
                .AddXmlString(Entity.mapping)
                .SetProperties(new Dictionary<string, string> {
                    { "connection.connection_string", connectionString },
                    { "connection.driver_class", "NHibernate.Driver.MicrosoftDataSqlClientDriver" },
                    { "dialect", "NHibernate.Dialect.MsSql2012Dialect" },
                    { "default_flush_mode", "Manual" },
                    { "transaction.use_connection_on_system_prepare", "false" },
                });
            var sessionFactory = cfg.BuildSessionFactory();

            SetupDb(sessionFactory);

            for(var i = 0;i < numThreads;i++) {
                new Thread(() => ThreadProc(sessionFactory)).Start();
            }
        }

        static void ThreadProc(ISessionFactory sessionFactory) {
            var txOptions = new TransactionOptions {
                Timeout = TimeSpan.FromMilliseconds(1),
            };

            while(true) {
                Console.WriteLine($"running on thread {Thread.CurrentThread.ManagedThreadId}");
                try {
                    using(var txscope = new TransactionScope(TransactionScopeOption.Required, txOptions)) {
                        var session = sessionFactory.OpenSession();
                        var data = session.CreateCriteria<Entity>().List();
                        txscope.Complete();
                    }
                } catch(Exception) { }

                Thread.Sleep(new Random().Next(0, 100));
            }
        }

        static void SetupDb(ISessionFactory sessionFactory) {
            using(var txscope = new TransactionScope())
            using(var conn = new SqlConnection(connectionString)) {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @$"
DROP TABLE IF EXISTS {tableName};
CREATE TABLE {tableName}(
    [Id] [uniqueidentifier] NOT NULL,
    [Date] [datetime2](7) NOT NULL,
    [String1] NVARCHAR(MAX) NULL,
    [String2] NVARCHAR(MAX) NULL,
    [String3] NVARCHAR(MAX) NULL,
);
";
                cmd.ExecuteNonQuery();
                txscope.Complete();
            }

            using(var txscope = new TransactionScope()) {
                var session = sessionFactory.OpenSession();

                for(var i = 0; i < numRows; i++) {
                    var entity = new Entity() {
                        Id = Guid.NewGuid().ToString(),
                        Date = DateTime.UtcNow,
                        String1 = $"a{i}",
                        String2 = $"b{i}",
                        String3 = $"c{i}",
                    };
                    session.Save(entity);
                }

                Console.WriteLine("flushing");
                session.Flush();
                Console.WriteLine("flush complete");
                txscope.Complete();
            }
        }
    }

    public class Entity {
        public static readonly string mapping = @$"
<hibernate-mapping xmlns=""urn:nhibernate-mapping-2.2"">
  <class name=""NHibBugRepro.Entity, NHibBugRepro"" table=""{Program.tableName}"" lazy=""false"">

    <id name=""Id"" column=""Id"" type=""String"" access=""property"">
      <generator class=""assigned"" />
    </id>

    <property name=""Date"" column=""Date"" access=""property"" type=""UtcDateTime"" />
    <property name=""String1"" column=""String1"" access=""property"" type=""StringClob"" />
    <property name=""String2"" column=""String2"" access=""property"" type=""StringClob"" />
    <property name=""String3"" column=""String3"" access=""property"" type=""StringClob"" />
  </class>
</hibernate-mapping>
";

        public Entity() { }

        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string String1 { get; set; }
        public string String2 { get; set; }
        public string String3 { get; set; }
    }
}

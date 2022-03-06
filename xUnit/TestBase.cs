using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Sample.Entity;
using SqlToEntity;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace xUnitTests {
    public abstract class TestBase {
        #region Constructors

        protected TestBase() {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json").AddUserSecrets<TestBase>()
                .Build();

            SqlToEntityOptions = Options.Create(new SqlToEntityOptions {
                ConnectionStrings = Configuration.GetSection("ConnectionStrings").GetChildren().ToDictionary(connection => connection.Key, connection => connection.Value),
                DefaultCommandTimeout = Configuration.GetValue<int>("SqlToEntity:DefaultCommandTimeout"),
                DefaultDatabaseName = Configuration.GetValue<string>("SqlToEntity:DefaultDatabaseName")
            });

            PeopleShallow = JsonSerializer.Deserialize<List<Person>>(new StreamReader("TestData/PeopleShallow.json").ReadToEnd());
            PeopleDeep = JsonSerializer.Deserialize<List<Person>>(new StreamReader("TestData/PeopleDeep.json").ReadToEnd());
        }

        #endregion

        #region Protected Properties

        protected IConfiguration Configuration { get; }
        protected IOptions<SqlToEntityOptions> SqlToEntityOptions { get; }
        protected List<Person> PeopleShallow { get; }
        protected List<Person> PeopleDeep { get; }

        #endregion
    }
}
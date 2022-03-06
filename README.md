# SqlToEntity

A library that facilitates streamlined SQL Server database interaction and construction and population of entity objects.

1. Small footprint and fast performance.
2. Eliminates the need to write `SqlClient` code.
3. Automatic `SqlConnection` management with the flexibility to manage connections yourself when you want.
4. Retain full control over all aspects of the `DbCommand`.
5. Retain full control over how objects are populated from the data.
6. Can be mocked to allow for unit testing of methods that call the database.

Who's it for? Anyone that wants to interact with a database and eliminate the boilerplate code while retaining granular control and employing a consistent and structured design.

## Installation

### From the UI

1. In Visual Studio, go to __Tools > NuGet Package Manager > Package Manager Settings > Package Sources__ and make sure the nuget.org feed is present and selected.

2. Open your project or solution. Right click the project that will be accessing the database and select __Manage NuGet Packages__. From the __Package Source__ dropdown, make sure results from the nuget.org feed are being shown. Search for __SqlToEntity__, select the package, and click __Install__. Note, you do not have to install SqlClient or any other packages.

### From Package Manager Console

1. Run the following command: `Install-Package SqlToEntity -ProjectName MyProject`

## Usage

### Configuration

1. In __AppSettings.json__, create a configuration section as shown below.

``` JSON
"SqlToEntity": {
  "DefaultCommandTimeout": 60,
  "DefaultDatabaseName": "MyDatabase"
}
```

* _int_ __DefaultCommandTimeout__ - _optional_ - The duration that a `DbCommand` will be allowed to run before timing out. Defaults to 30 if not specified.
* _string_ __DefaultDatabaseName__ - _optional_ - Used for automatic connections only. The default `DatabaseName` used for identifying what connection string to use. See the _RepositoryBase_ and _Managing Connections > Automatically_ sections of this document for related info.

### IOptions\<SqlToEntityOptions>

1. In the `ConfigureServices` method of __Startup.cs__, call the following method on the `IServiceCollection` object, where `configuration` is an instance of `IConfiguration`. This will register an action used to configure an instance of `SqlToEntityOptions` with the values from your __AppSettings.json__.

``` C#
services.ConfigureSqlToEntity(configuration);
```

#### OR

2. Alternatively, you can manually configure the `IOptions<SqlToEntityOptions>` in __Startup.cs__ as shown below.

``` C#
services.Configure<SqlToEntityOptions>(options => {
    options.ConnectionStrings = Configuration.GetSection("ConnectionStrings").GetChildren().ToDictionary(connection => connection.Key, connection => connection.Value);
    options.DefaultCommandTimeout = Configuration.GetValue<int>("SqlToEntity:DefaultCommandTimeout");
    options.DefaultDatabaseName = Configuration.GetValue<string>("SqlToEntity:DefaultDatabaseName");
});
```

### RepositoryBase

1. Derive from `RepositoryBase` in the class you want to call the database from, and pass the `IOptions<SqlToEntityOptions>` to the base constructor.


``` C#
public class PersonRepository : RepositoryBase {

    public PersonRepository(IOptions<SqlToEntityOptions> sqlToEntityOptions) : base(sqlToEntityOptions) { }
  
    ...
}
```

2. For automatic connections, override the `DatabaseName` property to specify a value for the connection to use if it differs from what is defined in `SqlToEntityOptions.DefaultDatabaseName`.

``` C#
protected override string DatabaseName => "DatabaseForThisRepository";
```

### Managing Connections

#### Manually

To manually manage your database connections, assign a `SqlConnection` instance to the `Connection` property in the `configureOptions` method, and that connection will be used. You are responsible for opening and closing the connection. See the _Calling the Database_ section of this document for more info on `configureOptions`.

#### Automatically

The application will determine which connection string to use based on the following steps in the order they are listed.

1. If a `ConnectionStringKey` was provided in the `configureOptions` method, the connection string with that key will be used.
2. If there is a connection string with the key of `$"{DatabaseName}{ApplicationIntent}"`, it will be used.
3. If there is only one connection string, it will be used.
4. If a there is a connection string with the key of `$"{DatabaseName}"`, it will be used.
5. If there is only one connection string with a key that starts with `$"{DatabaseName}"`, it will be used.
6. If no connection string is identified from the above steps, a `NotSupportException` is thrown.

When connections are being managed automatically, a new connection will be created, opened, and closed each time a `RepositoryBase` method is called.

### Calling the Database

1. Use one of the following methods made available by the `RepositoryBase` class.

``` C#
Task<int> InsertAsync(Action<DbCommandOptions> configureOptions);

Task<int> InsertOneAsync<T>(Action<InsertOneOptions<T>> configureOptions) where T : class, new();

Task<int> InsertManyAsync<T>(Action<InsertManyOptions<T>> configureOptions) where T : class, new()

Task<T> GetOneAsync<T>(Action<DbCommandOptions<T>> configureOptions) where T : class, new();

IAsyncEnumerable<T> GetManyAsync<T>(Action<DbCommandOptions<T>> configureOptions) where T : class, new();

Task<T> GetOneDynamicAsync<T>(Action<GetOptionsDynamic<T>> configureOptions) where T : class, new();

IAsyncEnumerable<T> GetManyDynamicAsync<T>(Action<GetOptionsDynamic<T>> configureOptions) where T : class, new();

Task<int> UpdateAsync(Action<DbCommandOptions> configureOptions);

Task<int> DeleteAsync(Action<DbCommandOptions> configureOptions);

Task<int> ExecuteNonQueryAsync(Action<DbCommandOptions> configureOptions);

Task<T> ExecuteScalarAsync<T>(Action<DbCommandOptions> configureOptions);
```

* `InsertAsync` - A convenience method that calls `ExecuteScalarAsync<int>`. It expects the database operation to return an integer that is the identifier for the inserted row. An example using an output clause is provided in the _Examples > InsertAsync_ section of this document.

* `InsertOneAsync<T>` - Inserts the provided entity object into the database. See the _Mapping Objects to the Database_ section of this document for more details on how that is done.

* `InsertManyAsync<T>` - Inserts all of the provided entity objects into the database. See the _Mapping Objects to the Database_ section of this document for more details on how that is done.

* `GetOneAsync<T>` - Returns a single instance of `T` populated by the data as defined by the EntityPopulationAction method.

* `GetManyAsync<T>` - Like `GetOneAsync<T>`, but for retrieving one or more instances of `T`. Has a return type of `IAsyncEnumerable<T>`. If you want the instances of `T` as a `List<T>`, you can install the `System.Linq.Async` NuGet package and use the `ToListAsync()` method. An example is provided in the _Examples > GetManyAsync\<T>_ section of this document.

* `GetOneDynamicAsync<T>` - Like `GetOneAsync<T>`, but the parameters used for the `DbCommand` come from the properties on the dynamic `Where` object that is provided. See the _Mapping Objects to the Database_ section of this document for more details on how that is done.

* `GetManyDynamicAsync<T>` - Like `GetOneDynamicAsync<T>` but for retrieving one or more instances of `T`.

* `UpdateAsync`, `DeleteAsync` - Convenience methods that call `ExecuteNonQueryAsync`. They all return the number of rows that were affected by the operation.

* `ExecuteScalarAsync<T>` - Returns the value from the first field of the first row of the returned data as type `T`.

2. Pass a `configurOptions` method for configuring the `DbCommandOptions` or `DbCommandOptions<T>`, depending which `RepositoryBase` method is being called. The order in which the properties are assigned or the `SetParameters` method is called does not matter.

``` C#
options => {
    options.CommandText = "SELECT Id, FirstName, LastName, EmailAddress FROM Person WHERE Id = @personId";
    options.SetParameters(new SqlParameter("@personId", personId));
    options.EntityPopulationAction = (person, dataRecord) => {
        person.Id = dataRecord.GetInt32(0);
        person.FirstName = dataRecord.GetString(1);
        person.LastName = dataRecord.GetString(2);
        person.EmailAddress = dataRecord.GetString(3);
        person.City = dataRecord.GetString(4);
        person.JobTitle = dataRecord.GetString(5);
    };
}
```

#### DbCommandOptions

A configureOptions parameter of Action type returning one of the below detailed Options classes deriving from DbCommandOptions is required when calling methods on RepositoryBase.

##### Properties

* _ApplicationIntent_ __ApplicationIntent__ - _optional_ - Used for automatically managed connections only. If not provided, defaults to `ReadOnly` for `GetOneAsync<T>` and `GetManyAsync<T>`, and `ReadWrite` for all other methods.
* _string_ __CommandText__ - _optional for all but ExecuteScalarAsync_ - The parameterized SQL statement or the name of a stored procedure.
* _CommandType_ __CommandType__ - _optional_ - Defaults to `CommandType.Text`.
* _int_ __CommandTimeout__ - _optional_ - Defaults to the value from `SqlToEntityOptions.DefaultCommandTimeout`.
* _SqlConnection_ __Connection__ - _optional_ - Defaults as specified in the _Managing Connections > Automatically_ section of this document.
* _string_ __ConnectionStringKey__ - _optional_ - This is used as detailed in the _Managing Connections > Automatically_ section of this document.

#### ExecuteCommandOptions\<T>

Required when calling any of the Execute methods on RepositoryBase. This class derives from DbCommandOptions and adds a SetParameters method.

##### Methods

* _void_ __SetParameters__*(params IDataParameter[] parameters)* - _optional_ - Pass this method the parameters to be used by the underlying `DbCommand`.

#### GetOptionsBase

The base class for all versions of GetOptions

##### Properties

* _Action\<T, IDataRecord>_ __EntityPopulationAction__ - _required_ - A method that defines how the entity instance will be populated from the data.
* _int_ __EntityIdColumnIndex__ - _optional_ - This is only used by `GetOneAsync<T>` and `GetManyAsync<T>`, and only if a value of 0 or greater is assigned.

###### EntityIdColumnIndex

`EntityIdColumnIndex` is only used by `GetOneAsync<T>` and `GetManyAsync<T>`. It specifies the index of a column in the returned data, with an integer data type, which uniquely identifies one instance of `T`. If multiple rows are present where the value of that field is the same, the entity population action will be executed for each of those rows using same instance of `T`.

When calling `GetOneAsync<T>`, all of the rows must have the same value for the field with index of `EntityIdColumnIndex`, or an exception will occur.

When calling `GetManyAsync<T>`, all of the rows with the same value for the field with index of `EntityIdColumnIndex` must be sequential, or an exception will occur. This is by design so that entities can be returned one by one, immediately upon being fully populated, without having to wait for all entities to be created and populated first.

An example is provided in the _Examples > GetOneAsync\<T> > Using EntityIdColumnIndex_ section of this document.

#### GetOptions\<T>

Required when calling GetOneAsync\<T> and GetManyAsync\<T> on RepositoryBase. This class derives from DbCommandOptions and adds a SetParameters method.

##### Methods

* _void_ __SetParameters__*(params IDataParameter[] parameters)* - _optional_ - Pass this method the parameters to be used by the underlying `DbCommand`.

#### GetOptionsDynamic

Required when calling GetOneDynamicAsync\<T> and GetManyDynamicAsync\<T> on RepositoryBase. This class derives from GetOptions\<T> and adds a dynamic Where property. Parameters for the DbCommand are created from the properties of the dynamic Where object.

##### Properties

dynamic Where { get; set; }

#### InsertOneOptions\<T>

T Entity { get; set; }

#### InsertManyOptions<T>

IReadOnlyCollection<T> Entities { get; set; }

#### Mapping Objects to the Database

Mapping from an object's class or instance name and properties to a database table and columns occurs when using GetOneDynamicAsync, GetManyDynamicAsync, InsertOne, and InsertMany. By default, the entity class name or dynamic object instance name is expected to match the name of a table in the database, and the property names are expected to match the table's column names. That can be overriden by using a Table attribute on the class or a Column attributes on a properties. Those attributes are available in the `System.ComponentModel.DataAnnotations.Schema` namespace.

#### Examples

##### InsertAsync

``` C#
public async Task<int> InsertPersonAsync(Person newPerson) {
    var newPersonId = await UpdateAsync(options => {
        options.CommandText = @"INSERT INTO PERSON (FirstName, LastName, EmailAddress, City, JobTitle)
                                OUTPUT INSERTED.Id
                                VALUES(@firstName, @lastName, @emailAddress, @jobTitle)";
        options.SetParameters(
            new SqlParameter("@firstName", newPerson.FirstName),
            new SqlParameter("@lastName", newPerson.LastName),
            new SqlParameter("@emailAddress", newPerson.EmailAddress),
            new SqlParameter("@jobTitle", newPerson.JobTitle)
        );
    });
    return newPersonId;
}
```

##### GetOneAsync\<T>

###### Using a string of parameterized SQL

``` C#
public async Task<Person> GetPersonByIdAsync(int personId) {
    return await GetOneAsync<Person>(options => {
        options.CommandText = "SELECT FirstName, LastName, EmailAddress, City, JobTitle FROM Person WHERE Id = @personId";
        options.SetParameters(new SqlParameter("@personId", personId));
        options.EntityPopulationAction = (person, dataRecord) => {
            person.FirstName = dataRecord.GetString(0);
            person.LastName = dataRecord.GetString(1);
            person.EmailAddress = dataRecord.GetString(2);
            person.City = dataRecord.GetString(3);
            person.JobTitle = dataRecord.GetString(4);
        };
    });
}
```

###### Using a stored procedure

Later examples are only shown using a string of parameterized SQL, but they can be done using a stored procedure in a similar way as this.

``` C#
public async Task<Person> GetPersonByIdAsync(int personId) {
    return await GetOneAsync<Person>(options => {
        options.CommandText = "GetPersonById";
        options.CommandType = CommandType.StoredProcedure;
        options.SetParameters(new SqlParameter("@personId", personId));
        options.EntityPopulationAction = (person, dataRecord) => {
            person.FirstName = dataRecord.GetString(0);
            person.LastName = dataRecord.GetString(1);
            person.EmailAddress = dataRecord.GetString(2);
            person.City = dataRecord.GetString(3);
            person.JobTitle = dataRecord.GetString(4);
        };
    });
}
```

###### Using EntityIdColumnIndex

Sample data:
```
Id      FirstName   LastName    PhoneNumber
1001    John        Brown       555-123-0000
1001    John        Brown       555-456-4444
1001    John        Brown       555-789-8888
```

``` C#
public async Task<Person> GetPersonByIdAsync() {
    return await GetOneAsync<Person>(options => {
        options.CommandText = "SELECT Id, FirstName, LastName, PhoneNumber FROM Person WHERE Id = @personId";
        options.EntityPopulationAction = (person, dataRecord) => {
            if (person.Id == 0) {
                person.Id = dataRecord.GetInt32(0);
                person.FirstName = dataRecord.GetString(1);
                person.LastName = dataRecord.GetString(2);
                person.PhoneNumbers = new List<string>();
            }
            person.PhoneNumbers.Add(dataRecord.GetInt32(3));
        };
        options.EntityIdColumnIndex = 0;
    });
}
```

##### GetManyAsync\<T>

The following example uses `ToListAsync()` from the `System.Linq.Async` NuGet package to enumerate the `IAsyncEnumerable<T>` returned by `GetManyAsync<Person>` to a `List<Person>`.

``` C#
public async Task<List<Person>> GetPeopleByCityAndJobTitleAsync(string city, string jobTitle) {
    return await GetManyAsync<Person>(options => {
        options.CommandText = "SELECT Id, FirstName, LastName, EmailAddress, City, JobTitle FROM Person WHERE City = @city and JobTitle = @jobTitle";
        options.SetParameters(new SqlParameter("@city", city), new SqlParameter("@jobTitle", jobTitle));
        options.EntityPopulationAction = (person, dataRecord) => {
            person.Id = dataRecord.GetInt32(0);
            person.FirstName = dataRecord.GetString(1);
            person.LastName = dataRecord.GetString(2);
            person.EmailAddress = dataRecord.GetString(3);
            person.City = dataRecord.GetString(4);
            person.JobTitle = dataRecord.GetString(5);
        };
    }).ToListAsync();
}
```

##### UpdateAsync

``` C#
public async Task<int> UpdateCityByPersonIdAsync(int personId, string city) {
    var numberOfAffectedRows = await UpdateAsync(options => {
        options.CommandText = "UPDATE Person SET City = @city WHERE Id = @personId";
        options.SetParameters(new SqlParameter("@personId", personId), new SqlParameter("@city", city));
    });
    return numberOfAffectedRows;
}
```

##### DeleteAsync

``` C#
public async Task<int> DeletePersonByIdAsync(int personId) {
    var numberOfAffectedRows = await DeleteAsync(options => {
        options.CommandText = "DELETE FROM Person WHERE Id = @personId";
        options.SetParameters(new SqlParameter("@personId", personId));
    });
    return numberOfAffectedRows;
}
```

##### ExecuteScalarAsync\<T>

``` C#
public async Task<int> GetMaxPersonIdAsync() {
    return await ExecuteScalarAsync<int>(sqlOptions => sqlOptions.CommandText = "SELECT MAX(Id) FROM Person");
}
```

##### ExecuteNonQueryAsync

``` C#
public async Task<int> UpdatePersonEmailAddressByIdAsync(int personId, string emailAddress) {
    var numberOfAffectedRows = await ExecuteNonQueryAsync(options => {
        options.CommandText = "UpdateEmailAddressByPersonId";
        options.CommandType = CommandType.StoredProcedure;
        options.SetParameters(new SqlParameter("@personId", personId), new SqlParameter("@emailAddress", emailAddress));
    });
    return numberOfAffectedRows;
}
```

#### Unit Testing

To unit test methods that access the database by using `RepositoryBase`, mock `IDbContext` and set up the underlying methods associated with the `RepositoryBase` calls to return the desired results. The method associations are detailed below.

``` C#
RepositoryBase method called    IDbContext method to set up on mock
InsertAsync                     ExecuteScalarAsync<int>
GetOneAsync<T>                  GetOneAsync<T>
GetManyAsync<T>                 GetManyAsync<T>
UpdateAsync                     ExecuteNonQueryAsync<int>
DeleteAsync                     ExecuteNonQueryAsync<int>
ExecuteNonQueryAsync            ExecuteNonQueryAsync
ExecuteScalarAsync<T>           ExecuteScalarAsync<T>
```

Assign the mock `DbContext` object to the `DbContext` property of `SqlToEntityOptions`, and pass `IOptions<SqlToEntityOptions>` to the `RepositoryBase` constructor. An example using xUnit is shown below.

``` C#
[Fact]
public async Task TestMethodThatCallsRepositoryBase() {
    // Arrange
    var fixture = new Fixture();
    var expectedResult = fixture.CreateMany<Person>(50).ToList();
    var dbContextGetManyAsyncReturnValue = expectedResult.ToAsyncEnumerable();

    var dbContextMock = new Mock<IDbContext>();
    dbContextMock.Setup(context => context.GetManyAsync(It.IsAny<DbCommandOptions<Person>>())).Returns(dbContextGetManyAsyncReturnValue);

    var sqlToEntityOptions = Options.Create(new SqlToEntityOptions {
        ConnectionStrings = new Dictionary<string, string>(0),
        DbContext = dbContextMock.Object
    });

    var city = "Reston";
    var jobTitle = "Manager";
    var sut = new PersonRepository(sqlToEntityOptions);

    // Act
    var actualResult = await sut.GetPeopleByCityAndJobTitleAsync(city, jobTitle);

    // Assert
    Assert.Equal(expectedResult, actualResult);
}
```

* `ConnectionStrings` can't be null, or a null reference exception will occur, but it doesn't need to contain any elements since it won't be connecting to the database.

* The `GetPeopleByCityAndJobTitleAsync` method would be implemented as shown in the earlier example for `GetManyAsync<T>`.

* The `System.Linq.Async` NuGet package must be installed in order for `ToAsyncEnumerable()` to be available.

## Contributing

If you'd like to help make this library better, please submit a pull request with your changes. For major changes, please first open an issue to discuss the details.

## License

MIT License

Copyright (c) 2021 Protechnix

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

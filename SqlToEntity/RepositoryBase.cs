using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace SqlToEntity {
    public abstract class RepositoryBase {
        #region Private Fields

        private string _readOnlyConnectionString;
        private string _readWriteConnectionString;
        private readonly SqlToEntityOptions _sqlToEntityOptions;
        private readonly IDbContext _dbContext;

        #endregion

        #region Constructors

        protected RepositoryBase(IOptions<SqlToEntityOptions> sqlToEntityOptions) {
            _sqlToEntityOptions = sqlToEntityOptions.Value;
            _dbContext = _sqlToEntityOptions.DbContext ?? Utility.InternalDbContext;
            if (_sqlToEntityOptions.DefaultDatabaseName != null) DatabaseName = _sqlToEntityOptions.DefaultDatabaseName;
        }

        #endregion

        #region Protected Virtual Properties

        protected virtual string DatabaseName { get; }

        #endregion

        #region Public Methods

        public async Task<int> InsertAsync(Action<DbCommandOptions> configureOptions) {
            return await ExecuteAsync(configureOptions, _dbContext.ExecuteScalarAsync<int>);
        }

        public async Task<int> InsertOneAsync<T>(Action<InsertOneOptions<T>> configureOptions) where T : class, new() {
            var options = new InsertOneOptions<T>();
            configureOptions(options);

            var properties = typeof(T).GetProperties();
            var parameters = new IDataParameter[properties.Length];
            Utility.GetInsert(properties, out var insertBuilder, out var valuesBuilder, parameters, options.Entity);
            options.CommandText = $"{insertBuilder}{valuesBuilder}";
            options.Parameters = parameters;

            return await ExecuteInsertAsync(options);
        }

        public async Task<int> InsertOneAsync<T>(T entity) where T : class, new() {
            var properties = typeof(T).GetProperties();
            var parameters = new IDataParameter[properties.Length];
            Utility.GetInsert(properties, out var insertBuilder, out var valuesBuilder, parameters, entity);

            await using var sqlConnection = new SqlConnection(GetDefaultConnectionString(DatabaseName, _sqlToEntityOptions.ConnectionStrings, ApplicationIntent.ReadWrite));
            var options = new InsertOneOptions<T> {
                CommandText = $"{insertBuilder}{valuesBuilder}",
                CommandTimeout = _sqlToEntityOptions.DefaultCommandTimeout,
                Connection = sqlConnection,
                Entity = entity,
                Parameters = parameters
            };

            return await _dbContext.ExecuteScalarAsync<int>(options);
        }

        public async Task<int> InsertManyAsync<T>(Action<InsertManyOptions<T>> configureOptions) where T : class, new() {
            var options = new InsertManyOptions<T>();
            configureOptions(options);

            var properties = typeof(T).GetProperties();
            var parameters = new IDataParameter[properties.Length * options.Entities.Count];
            Utility.GetInsert(properties, out var insertBuilder, out var valuesBuilder, parameters, options.Entities.First(), options.Entities.Count);
            Utility.AddToInsert(options.Entities.Skip(1), properties, valuesBuilder, parameters);
            options.CommandText = $"{insertBuilder}{valuesBuilder}";
            options.Parameters = parameters;

            return await ExecuteInsertAsync(options);
        }

        public async Task<T> GetOneAsync<T>(Action<GetOptions<T>> configureOptions) where T : class, new() {
            var options = new GetOptions<T>();
            configureOptions(options);

            if (options.Connection != null) return await _dbContext.GetOneAsync(options);
            await using var sqlConnection = new SqlConnection(GetConnectionString(DatabaseName, _sqlToEntityOptions.ConnectionStrings, options.ConnectionStringKey, options.ApplicationIntent == ApplicationIntent.Default ? ApplicationIntent.ReadOnly : options.ApplicationIntent));
            SetCommandTimeout(options);
            options.Connection = sqlConnection;
            return await _dbContext.GetOneAsync(options);
        }

        public async IAsyncEnumerable<T> GetManyAsync<T>(Action<GetOptions<T>> configureOptions) where T : class, new() {
            var options = new GetOptions<T>();
            configureOptions(options);

            if (options.Connection == null) {
                await using var sqlConnection = new SqlConnection(GetConnectionString(DatabaseName, _sqlToEntityOptions.ConnectionStrings, options.ConnectionStringKey, options.ApplicationIntent == ApplicationIntent.Default ? ApplicationIntent.ReadOnly : options.ApplicationIntent));
                SetCommandTimeout(options);
                options.Connection = sqlConnection;
                await foreach (var entity in _dbContext.GetManyAsync(options)) yield return entity;
            }
            else await foreach (var entity in _dbContext.GetManyAsync(options)) yield return entity;
        }

        public async Task<T> GetOneDynamicAsync<T>(Action<GetOptionsDynamic<T>> configureOptions) where T : class, new() {
            var options = new GetOptionsDynamic<T>();
            configureOptions(options);

            options.Parameters = Utility.GetParametersDynamic(options.Where);

            if (options.Connection != null) return await _dbContext.GetOneAsync(options);
            await using var sqlConnection = new SqlConnection(GetConnectionString(DatabaseName, _sqlToEntityOptions.ConnectionStrings, options.ConnectionStringKey, options.ApplicationIntent == ApplicationIntent.Default ? ApplicationIntent.ReadOnly : options.ApplicationIntent));
            SetCommandTimeout(options);
            options.Connection = sqlConnection;
            return await _dbContext.GetOneAsync(options);
        }

        public async Task<T> GetOneDynamicAsync<T>(dynamic where) where T : class, new() {
            await using var sqlConnection = new SqlConnection(GetDefaultConnectionString(DatabaseName, _sqlToEntityOptions.ConnectionStrings, ApplicationIntent.ReadOnly));
            var options = new GetOptionsDynamic<T> {
                CommandText = @"select p.Id, p.FirstName, p.LastName, p.EmailAddress, p.City from Person p where p.Id = @Id",
                CommandTimeout = _sqlToEntityOptions.DefaultCommandTimeout,
                Connection = sqlConnection,
                EntityPopulationAction = DynamicEntityPopulationAction,
                EntityIdColumnIndex = -1,
                Where = where
            };
            options.Parameters = Utility.GetParametersDynamic(where);

            return await _dbContext.GetOneAsync(options);
        }

        private static void DynamicEntityPopulationAction<T>(T entity, IDataRecord dataRecord) {
            var properties = entity.GetType().GetProperties();

            foreach (var property in properties) {
                if (property.PropertyType.GetInterfaces().Any(p => p == typeof(IList))) continue;
                var propertyName = property.Name;
                property.SetValue(entity, dataRecord[propertyName] == DBNull.Value ? default : dataRecord[propertyName]);
            }
        }

        public async IAsyncEnumerable<T> GetManyDynamicAsync<T>(Action<GetOptionsDynamic<T>> configureOptions) where T : class, new() {
            var options = new GetOptionsDynamic<T>();
            configureOptions(options);

            options.Parameters = Utility.GetParametersDynamic(options.Where);

            if (options.Connection == null) {
                await using var sqlConnection = new SqlConnection(GetConnectionString(DatabaseName, _sqlToEntityOptions.ConnectionStrings, options.ConnectionStringKey, options.ApplicationIntent == ApplicationIntent.Default ? ApplicationIntent.ReadOnly : options.ApplicationIntent));
                SetCommandTimeout(options);
                options.Connection = sqlConnection;
                await foreach (var entity in _dbContext.GetManyAsync(options)) yield return entity;
            }
            else await foreach (var entity in _dbContext.GetManyAsync(options)) yield return entity;
        }

        public async Task<int> UpdateAsync(Action<DbCommandOptions> configureOptions) {
            return await ExecuteAsync(configureOptions, _dbContext.ExecuteNonQueryAsync);
        }

        public async Task<int> DeleteAsync(Action<DbCommandOptions> configureOptions) {
            return await ExecuteAsync(configureOptions, _dbContext.ExecuteNonQueryAsync);
        }

        public async Task<int> ExecuteNonQueryAsync(Action<DbCommandOptions> configureOptions) {
            return await ExecuteAsync(configureOptions, _dbContext.ExecuteNonQueryAsync);
        }

        public async Task<T> ExecuteScalarAsync<T>(Action<DbCommandOptions> configureOptions) {
            return await ExecuteAsync(configureOptions, _dbContext.ExecuteScalarAsync<T>);
        }

        #endregion

        #region Private Methods

        private async Task<T> ExecuteAsync<T>(Action<DbCommandOptions> configureOptions, Func<DbCommandOptions, Task<T>> dbContextFunc) {
            var options = new DbCommandOptions();
            configureOptions(options);

            if (options.Connection != null) return await dbContextFunc(options);
            await using var sqlConnection = new SqlConnection(GetConnectionString(DatabaseName, _sqlToEntityOptions.ConnectionStrings, options.ConnectionStringKey, options.ApplicationIntent == ApplicationIntent.Default ? ApplicationIntent.ReadWrite : options.ApplicationIntent));
            SetCommandTimeout(options);
            options.Connection = sqlConnection;
            return await dbContextFunc(options);
        }

        private async Task<int> ExecuteInsertAsync(DbCommandOptions options) {
            if (options.Connection != null) return await _dbContext.ExecuteScalarAsync<int>(options);
            await using var sqlConnection = new SqlConnection(GetConnectionString(DatabaseName, _sqlToEntityOptions.ConnectionStrings, options.ConnectionStringKey, options.ApplicationIntent == ApplicationIntent.Default ? ApplicationIntent.ReadWrite : options.ApplicationIntent));
            SetCommandTimeout(options);
            options.Connection = sqlConnection;
            return await _dbContext.ExecuteScalarAsync<int>(options);
        }

        private string GetConnectionString(string databaseName, IReadOnlyDictionary<string, string> connectionStrings, string connectionStringKey, ApplicationIntent applicationIntent) {
            if (connectionStringKey != null) return connectionStrings[connectionStringKey];

            return GetDefaultConnectionString(databaseName, connectionStrings, applicationIntent);
        }

        private string GetDefaultConnectionString(string databaseName, IReadOnlyDictionary<string, string> connectionStrings, ApplicationIntent applicationIntent) {
            if (applicationIntent == ApplicationIntent.ReadWrite) {
                if (_readWriteConnectionString != null) return _readWriteConnectionString;
                var defaultReadWriteConnectionStringKey = $"{databaseName}{Enum.GetName(typeof(ApplicationIntent), applicationIntent)}";
                if (connectionStrings.ContainsKey(defaultReadWriteConnectionStringKey)) return _readWriteConnectionString = connectionStrings[defaultReadWriteConnectionStringKey];
            }
            else if (applicationIntent == ApplicationIntent.ReadOnly) {
                if (_readOnlyConnectionString != null) return _readOnlyConnectionString;
                var defaultReadOnlyConnectionStringKey = $"{databaseName}{Enum.GetName(typeof(ApplicationIntent), applicationIntent)}";
                if (connectionStrings.ContainsKey(defaultReadOnlyConnectionStringKey)) return _readOnlyConnectionString = connectionStrings[defaultReadOnlyConnectionStringKey];
            }

            return Utility.GetDefaultConnectionString(databaseName, connectionStrings);
        }

        private void SetCommandTimeout(DbCommandOptions options) {
            if (options.CommandTimeout < 0) options.CommandTimeout = _sqlToEntityOptions.DefaultCommandTimeout;
        }

        #endregion
    }
}
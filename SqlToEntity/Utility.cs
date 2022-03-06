using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SqlToEntity {
    public static class Utility {
        #region Internal Constants

        internal const int DefaultCommandTimeout = 30;

        #endregion

        #region Internal Static Fields

        internal static DbContext InternalDbContext => InternalDbContextAccessor.Value;

        #endregion

        #region Private Static Class

        private static class InternalDbContextAccessor {
            static InternalDbContextAccessor() {
                Value = new DbContext();
            }

            internal static readonly DbContext Value;
        }

        #endregion

        #region Public Static Extension Methods

        public static void ConfigureSqlToEntity(this IServiceCollection services, IConfiguration configuration) {
            var sqlToEntityConfigurationSection = configuration.GetSection(nameof(SqlToEntity));
            services.Configure<SqlToEntityOptions>(options => {
                options.ConnectionStrings = configuration.GetSection("ConnectionStrings").GetChildren().ToDictionary(connection => connection.Key, connection => connection.Value);
                options.DefaultCommandTimeout = int.TryParse(sqlToEntityConfigurationSection["DefaultCommandTimeout"], out var defaultCommandTimeout) ? defaultCommandTimeout : DefaultCommandTimeout;
                options.DefaultDatabaseName = sqlToEntityConfigurationSection["DefaultDatabaseName"];
            });
        }

        #endregion

        #region Internal Static Methods

        internal static string GetDefaultConnectionString(string databaseName, IReadOnlyDictionary<string, string> connectionStrings) {
            if (connectionStrings.Count <= 1) return connectionStrings.Count == 1 ? connectionStrings[connectionStrings.First().Key] : null;
            if (connectionStrings.ContainsKey(databaseName)) return connectionStrings[databaseName];
            var connectionStringsForDatabase = connectionStrings.Where(connectionString => connectionString.Key.StartsWith(databaseName)).ToArray();
            if (connectionStringsForDatabase.Length == 1) return connectionStringsForDatabase[0].Value;
            throw new NotSupportedException($"The default connection string for \"{databaseName}\" could not be identified.");
        }

        internal static void GetInsert<T>(PropertyInfo[] properties, out StringBuilder insertBuilder, out StringBuilder valuesBuilder, IDataParameter[] parameters, T entity, int entityCount = 1) {
            insertBuilder = new StringBuilder(properties.Length * 14 + 52);
            valuesBuilder = new StringBuilder(properties.Length * 15 * entityCount + 3 * entityCount - 1);
            insertBuilder.Append($"INSERT INTO {(Attribute.IsDefined(typeof(T), typeof(TableAttribute)) ? ((TableAttribute) Attribute.GetCustomAttribute(typeof(T), typeof(TableAttribute)))!.Name : typeof(T).Name)} (");
            valuesBuilder.Append('(');

            for (var i = 0; i < properties.Length; i++) {
                var property = properties[i];
                if (property.Name == "Id") continue;
                if (Attribute.IsDefined(property, typeof(NotMappedAttribute))) continue;
                if (property.PropertyType.GetInterfaces().Any(p => p == typeof(IList))) continue;
                insertBuilder.Append(Attribute.IsDefined(property, typeof(ColumnAttribute)) ? ((ColumnAttribute) property.GetCustomAttribute(typeof(ColumnAttribute)))!.Name : property.Name);
                insertBuilder.Append(",");
                valuesBuilder.Append('@');
                valuesBuilder.Append(property.Name);
                valuesBuilder.Append(',');
                parameters[i] = new SqlParameter(property.Name, property.GetValue(entity));
            }

            insertBuilder.Length--;
            insertBuilder.Append(") OUTPUT INSERTED.ID VALUES ");
            valuesBuilder.Length--;
            valuesBuilder.Append(')');
        }

        internal static void AddToInsert<T>(IEnumerable<T> entities, PropertyInfo[] properties, StringBuilder valuesBuilder, IDataParameter[] parameters) {
            var entityIndex = 1;

            foreach (var entity in entities) {
                for (var i = 0; i <= properties.Length; i++) {
                    var property = properties[i];
                    if (Attribute.IsDefined(property, typeof(NotMappedAttribute))) continue;
                    var propertyName = $"{property.Name}{entityIndex}";
                    valuesBuilder.Append('@');
                    valuesBuilder.Append(propertyName);
                    valuesBuilder.Append(',');
                    parameters[properties.Length * entityIndex + i] = new SqlParameter(propertyName, property.GetValue(entity));
                }

                entityIndex++;
                valuesBuilder.Length--;
                valuesBuilder.Append("),");
            }

            valuesBuilder.Length--;
        }

        internal static IDataParameter[] GetParametersDynamic(dynamic where) {
            var properties = where.GetType().GetProperties();
            var parameters = new IDataParameter[properties.Length];

            for (var i = 0; i < properties.Length; i++) {
                var property = properties[i];
                parameters[i] = new SqlParameter($"@{property.Name}", property.GetValue(where));
            }
            return parameters;
        }

        internal static IDataParameter[] GetOptionsDynamic<T>(GetOptionsDynamic<T> options) {
            var properties = options.Where.GetType().GetProperties();
            var parameters = new IDataParameter[properties.Length];

            for (var i = 0; i < properties.Length; i++) {
                var property = properties[i];
                parameters[i] = new SqlParameter($"@{property.Name}", property.GetValue(options.Where));
            }
            return parameters;
        }

        //TODO: Handle these from properties as data to persist
        //BigInt				Int64
        //Binary				Byte[]
        //Bit					Boolean
        //Char				String
        //DateTime			DateTime
        //Decimal				Decimal
        //Float				Double
        //Image				Byte[]
        //Int					Int32
        //Money				decimal
        //NChar				string
        //NText				string
        //NVarChar			string
        //Real				Single
        //UniqueIdentifier	Guid
        //SmallDateTime		DateTime
        //SmallInt			Int16
        //SmallMoney			decimal
        //Text				string
        //Timestamp			Byte[]
        //TinyInt				Byte
        //VarBinary			Byte[]
        //VarChar				string
        //Variant				-
        //Xml					string or XmlReader
        //Udt					-
        //Structured			-
        //Date				DateTime
        //Time				DateTime
        //DateTime2			DateTime
        //DateTimeOffset		DateTimeOffset
        
        //bool IsSimple(Type type)
        //{
        //  var typeInfo = type.GetTypeInfo();
        //  if (typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>))
        //  {
        //    // nullable type, check if the nested type is simple.
        //    return IsSimple(typeInfo.GetGenericArguments()[0]);
        //  }
        //  return typeInfo.IsPrimitive 
        //    || typeInfo.IsEnum
        //    || type.Equals(typeof(string))
        //    || type.Equals(typeof(decimal));
        //}

        #endregion

    }
}
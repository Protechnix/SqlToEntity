using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace SqlToEntity {
    internal class DbContext : IDbContext {
        #region Public Methods

        public async Task<T> GetOneAsync<T>(GetOptionsBase<T> dbCommandOptions) where T : class, new() {
            var dbCommand = GetDbCommand(dbCommandOptions);

            await using (dbCommand) {
                await dbCommandOptions.Connection.OpenAsync();
                await using var dbDataReader = await dbCommand.ExecuteReaderAsync();

                if (dbCommandOptions.EntityIdColumnIndex == -1) {
                    await dbDataReader.ReadAsync();
                    return BuildNewEntity(dbCommandOptions.EntityPopulationAction, dbDataReader);
                }

                var firstRow = true;
                var entityId = 0;
                T entity = null;

                while (await dbDataReader.ReadAsync()) {
                    if (firstRow) {
                        entityId = dbDataReader.GetInt32(dbCommandOptions.EntityIdColumnIndex);
                        entity = BuildNewEntity(dbCommandOptions.EntityPopulationAction, dbDataReader);
                        firstRow = false;
                    }
                    else {
                        var rowEntityId = dbDataReader.GetInt32(dbCommandOptions.EntityIdColumnIndex);
                        if (entityId == rowEntityId) dbCommandOptions.EntityPopulationAction(entity, dbDataReader);
                        else throw new NotSupportedException("Unsupported data was encountered.");
                    }
                }

                return entity;
            }
        }

        public async IAsyncEnumerable<T> GetManyAsync<T>(GetOptionsBase<T> dbCommandOptions) where T : class, new() {
            var dbCommand = GetDbCommand(dbCommandOptions);

            await using (dbCommand) {
                await dbCommandOptions.Connection.OpenAsync();
                await using var dbDataReader = await dbCommand.ExecuteReaderAsync();

                if (dbCommandOptions.EntityIdColumnIndex == -1) while (await dbDataReader.ReadAsync()) yield return BuildNewEntity(dbCommandOptions.EntityPopulationAction, dbDataReader);
                else {
                    var firstRow = true;
                    var entityId = 0;
                    T entity = null;
                    var entityHashSet = new HashSet<int>();

                    while (await dbDataReader.ReadAsync()) {
                        if (firstRow) {
                            entityId = dbDataReader.GetInt32(dbCommandOptions.EntityIdColumnIndex);
                            entityHashSet.Add(entityId);
                            entity = BuildNewEntity(dbCommandOptions.EntityPopulationAction, dbDataReader);
                            firstRow = false;
                        }
                        else {
                            var rowEntityId = dbDataReader.GetInt32(dbCommandOptions.EntityIdColumnIndex);
                            if (entityId == rowEntityId) dbCommandOptions.EntityPopulationAction(entity, dbDataReader);
                            else {
                                yield return entity;
                                if (!entityHashSet.Add(rowEntityId)) throw new NotSupportedException("Unsupported data was encountered.");
                                entityId = rowEntityId;
                                entity = BuildNewEntity(dbCommandOptions.EntityPopulationAction, dbDataReader);
                            }
                        }
                    }

                    yield return entity;
                }
            }
        }

        public async Task<int> ExecuteNonQueryAsync(DbCommandOptions dbCommandOptions) {
            var dbCommand = GetDbCommand(dbCommandOptions);

            await using (dbCommand) {
                await dbCommandOptions.Connection.OpenAsync();
                return await dbCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task<T> ExecuteScalarAsync<T>(DbCommandOptions dbCommandOptions) {
            var dbCommand = GetDbCommand(dbCommandOptions);

            await using (dbCommand) {
                await dbCommandOptions.Connection.OpenAsync();
                return (T) await dbCommand.ExecuteScalarAsync();
            }
        }

        #endregion

        #region Private Static Methods

        private static DbCommand GetDbCommand(DbCommandOptions dbCommandOptions) {
            var dbCommand = dbCommandOptions.Connection.CreateCommand();
            dbCommand.CommandText = dbCommandOptions.CommandText;
            dbCommand.CommandType = dbCommandOptions.CommandType;
            dbCommand.Connection = dbCommandOptions.Connection;
            dbCommand.CommandTimeout = dbCommandOptions.CommandTimeout;
            if (dbCommandOptions.Parameters != null) foreach (var sqlParameter in dbCommandOptions.Parameters) if (sqlParameter != null) dbCommand.Parameters.Add(sqlParameter);
            return dbCommand;
        }

        private static T BuildNewEntity<T>(Action<T, IDataRecord> populateEntity, IDataRecord dataRecord) where T : class, new() {
            var entity = new T();
            populateEntity(entity, dataRecord);
            return entity;
        }

        #endregion
    }
}
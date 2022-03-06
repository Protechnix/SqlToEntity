using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlToEntity {
    public interface IDbContext {
        #region Public Methods

        Task<T> GetOneAsync<T>(GetOptionsBase<T> dbCommandOptions) where T : class, new();

        IAsyncEnumerable<T> GetManyAsync<T>(GetOptionsBase<T> dbCommandOptions) where T : class, new();

        Task<int> ExecuteNonQueryAsync(DbCommandOptions dbCommandOptions);

        Task<T> ExecuteScalarAsync<T>(DbCommandOptions dbCommandOptions);

        #endregion
    }
}
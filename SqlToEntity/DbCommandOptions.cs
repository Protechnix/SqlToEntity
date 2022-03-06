using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SqlToEntity {
    public class DbCommandOptions {
        #region Public Properties

        public ApplicationIntent ApplicationIntent { get; set; } = ApplicationIntent.Default;
        public string CommandText { get; set; }
        public CommandType CommandType { get; set; } = CommandType.Text;
        public int CommandTimeout { get; set; } = -1;
        public SqlConnection Connection { get; set; }
        public string ConnectionStringKey { get; set; }

        #endregion

        #region Internal Properties

        internal IDataParameter[] Parameters { get; set; }

        #endregion
    }

    public class ExecuteCommandOptions : DbCommandOptions {
        #region Public Methods

        public void SetParameters(params IDataParameter[] parameters) {
            Parameters = parameters;
        }

        #endregion
    }

    public abstract class GetOptionsBase<T> : DbCommandOptions {
        #region Public Properties

        public Action<T, IDataRecord> EntityPopulationAction { get; set; }
        public int EntityIdColumnIndex { get; set; } = -1;

        #endregion
    }

    public class GetOptions<T> : GetOptionsBase<T> {
        #region Public Methods

        public void SetParameters(params IDataParameter[] parameters) {
            Parameters = parameters;
        }

        #endregion
    }

    public class GetOptionsDynamic<T> : GetOptionsBase<T> {
        #region Public Properties

        public dynamic Where { get; set; }

        #endregion
    }

    public class InsertOneOptions<T> : DbCommandOptions {
        #region Public Properties

        public T Entity { get; set; }

        #endregion
    }

    public class InsertManyOptions<T> : DbCommandOptions {
        #region Public Properties

        public IReadOnlyCollection<T> Entities { get; set; }

        #endregion
    }
}
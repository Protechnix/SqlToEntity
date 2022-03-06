using System.Collections.Generic;

namespace SqlToEntity {
    public class SqlToEntityOptions {
        #region Public Properties

        public Dictionary<string, string> ConnectionStrings { get; set; }
        public IDbContext DbContext { get; set; }
        public int DefaultCommandTimeout { get; set; } = Utility.DefaultCommandTimeout;
        public string DefaultDatabaseName { get; set; }

        #endregion
    }
}
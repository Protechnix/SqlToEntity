using Microsoft.Extensions.Options;
using Sample.Entity;
using Sample.Sql;
using SqlToEntity;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Sample.Repository {
    public class PersonRepository : RepositoryBase {
        #region Constructors

        public PersonRepository(IOptions<SqlToEntityOptions> sqlToEntityOptions) : base(sqlToEntityOptions) { }

        #endregion

        #region INSERT Methods

        public async Task<int> InsertPersonAsync(Person person) {
            return await InsertOneAsync<Person>(options => {
                options.Entity = person;
            });
        }

        public async Task<int> InsertPersonSimpleAsync(Person person) {
            return await InsertOneAsync(person);
        }

        #endregion

        #region SELECT Methods

        public async Task<int> GetMaxWorkOrderIdAsync() {
            return await ExecuteScalarAsync<int>(sqlOptions => sqlOptions.CommandText = PersonSql.GetLatestPersonIdSql);
        }

        public async Task<Person> GetPersonByIdExplicitAsync(int personId) {
            return await GetOneAsync<Person>(options => {
                options.CommandText = PersonSql.GetPersonByIdSql;
                options.SetParameters(new SqlParameter("@personId", personId));
                options.EntityPopulationAction = PopulateEntity;
                options.EntityIdColumnIndex = 0;
            });
        }

        public async Task<Person> GetPersonByIdDynamicAsync(int personId) {
            return await GetOneDynamicAsync<Person>(new {Id = personId});
        }

        public async Task<List<Person>> GetPeopleByCityAsync(string city) {
            return await GetManyAsync<Person>(options => {
                options.CommandText = PersonSql.GetPeopleByCitySql;
                options.SetParameters(new SqlParameter("@city", city));
                options.EntityPopulationAction = PopulateEntity;
            }).ToListAsync();
        }

        #endregion

        #region Private Methods

        private static void PopulateEntity(Person person, IDataRecord dataRecord) {
            if (person.Id == 0) {
                person.Id = dataRecord.GetInt32(0);
                person.FirstName = dataRecord.GetString(1);
                person.LastName = dataRecord.IsDBNull(2) ? null : dataRecord.GetString(2);
                person.EmailAddress = dataRecord.GetString(3);
                person.City = dataRecord.GetString(4);
                person.PhoneList = new List<Phone>();
            }

            person.PhoneList.Add(new Phone {
                Number = dataRecord.GetString(5),
                Type = dataRecord.GetString(6)
            });
        }

        #endregion
    }
}
namespace Sample.Sql {
    internal class PersonSql {
        #region Internal Constants

        internal const string GetLatestPersonIdSql = "select max(Id) FROM Person";

        internal const string GetPersonByIdSql = @"
select
    p.Id,
    p.FirstName,
	p.LastName,
	p.EmailAddress,
    p.City,
    ph.Number,
    ph.Type
from
	Person p
    inner join Phone ph on ph.PersonId = p.Id
where
	p.Id = @personId";

        internal const string GetPeopleByCitySql = @"
select
	p.Id,
    p.FirstName,
	p.LastName,
	p.EmailAddress,
    p.City,
    ph.Number,
    ph.Type
from
	Person p
    inner join Phone ph on ph.PersonId = p.Id
where
	p.City = @city";

        #endregion
    }
}

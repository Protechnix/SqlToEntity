using Sample.Repository;
using System.Linq;
using System.Threading.Tasks;
using Sample.Entity;
using Xunit;

namespace xUnitTests {
    public class ExerciseSqlToEntity : TestBase {
        #region Public Static Member Data Properties

        public static TheoryData<int> PersonIdData => new TheoryData<int> { 1, 2, 3, 4 };

        #endregion

        #region INSERT Tests

        [Fact]
        public async Task InsertPersonAsync() {
            // Arrange
            var newPerson = new Person {
                FirstName = "Test",
                LastName = "Insert",
                EmailAddress = "testinsert@notreal.com",
                City = "Boston"
            };
            
            var personRepository = new PersonRepository(SqlToEntityOptions);
            var latestPersonId = await personRepository.GetMaxWorkOrderIdAsync();

            // Act
            var id = await personRepository.InsertPersonAsync(newPerson);

            // Assert
            Assert.Equal(latestPersonId + 1, id);
        }


        [Fact]
        public async Task InsertPersonSimpleAsync() {
            // Arrange
            var newPerson = new Person {
                FirstName = "Test",
                LastName = "Insert",
                EmailAddress = "testinsert@notreal.com",
                City = "Boston"
            };
            
            var personRepository = new PersonRepository(SqlToEntityOptions);
            var latestPersonId = await personRepository.GetMaxWorkOrderIdAsync();

            // Act
            var id = await personRepository.InsertPersonSimpleAsync(newPerson);

            // Assert
            Assert.Equal(latestPersonId + 1, id);
        }

        #endregion
        
        #region SELECT Tests

        [Fact]
        public async Task GetLatestPersonIdAsync() {
            // Arrange
            var personRepository = new PersonRepository(SqlToEntityOptions);

            // Act
            var latestPersonId = await personRepository.GetMaxWorkOrderIdAsync();

            // Assert
            Assert.Equal(4, latestPersonId);
        }

        [Theory]
        [MemberData(nameof(PersonIdData))]
        public async Task GetPersonByIdExplicitAsync(int personId) {
            // Arrange
            var personRepository = new PersonRepository(SqlToEntityOptions);

            // Act
            var person = await personRepository.GetPersonByIdExplicitAsync(personId);

            // Assert
            Assert.True(person.EqualValues(PeopleDeep.First(p => p.Id == personId)));
        }

        [Theory]
        [MemberData(nameof(PersonIdData))]
        public async Task GetPersonByIdAutoAsync(int personId) {
            // Arrange
            var personRepository = new PersonRepository(SqlToEntityOptions);

            // Act
            var person = await personRepository.GetPersonByIdDynamicAsync(personId);

            // Assert
            Assert.True(person.EqualValues(PeopleShallow.First(p => p.Id == personId)));
        }

        #endregion
    }
}
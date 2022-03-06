using Sample.Entity;
using System.Collections;
using System.Collections.Generic;

namespace xUnitTests.TestData {
    public class PersonGenerator : IEnumerable<object[]> {
        private readonly List<object[]> _people;

        public PersonGenerator() {
            var people = new[,] {
                {"Joe", "Shmoe", "jshmoe@notreal.com", "Los Angeles"},
                {"Steve", "Green", "sgreen@notreal.com", "Dublin"},
                {"Michelle", null, "msmith@notreal.com", "Los Angeles"},
                {"Joan", "Branch", "jbranch@notreal.com", "New York"}
            };

            _people = new List<object[]>(people.Length);

            for (var i = 0; i < people.Length; i++) {
                var parameters = new object[] {
                    new Person {
                        Id = i + 1,
                        FirstName = people[i, 1],
                        LastName = people[i, 2],
                        EmailAddress = people[i, 3],
                        City = people[i, 4]
                    }
                };

                _people.Add(parameters);
            }
        }

        public IEnumerator<object[]> GetEnumerator() => _people.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
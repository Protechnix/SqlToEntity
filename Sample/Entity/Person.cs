using System.Collections.Generic;

namespace Sample.Entity {
    public class Person : EntityBase {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
        public string City { get; set; }
        public List<Phone> PhoneList { get; set; }
        public List<Hobby> Hobbies { get; set; }
    }
}

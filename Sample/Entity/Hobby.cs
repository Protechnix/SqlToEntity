using System;
using System.Collections.Generic;

namespace Sample.Entity {
    public class Hobby : EntityBase {
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public List<Equipment> EquipmentList { get; set; }
    }
}

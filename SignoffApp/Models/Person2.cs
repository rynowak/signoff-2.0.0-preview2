using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignoffApp.Models
{
    public abstract class Person2
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? TeacherId { get; set; }

        public PersonFamily Family { get; set; }
    }

    public class PersonKid : Person2
    {
        public int Grade { get; set; }

        public PersonTeacher Teacher { get; set; }
    }

    public class PersonTeacher : Person2
    {
        public ICollection<PersonKid> Students { get; set; }
    }

    public class PersonFamily
    {
        public int Id { get; set; }

        public string LastName { get; set; }

        public ICollection<Person2> Members { get; set; }
    }
}

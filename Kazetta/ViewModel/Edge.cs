using System;

namespace Kazetta
{
    /// <summary>
    /// Represents a group of people that will attend a lesson together
    /// </summary>
    [Serializable]
    public class Group
    {
        public Person[] Persons { get; set; } = new Person[2];
        
        public override string ToString()
        {
            return String.Join<Person>(", ", Persons);
        }
    }
}

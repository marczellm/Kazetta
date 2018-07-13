using System;

namespace Kazetta
{
    [Serializable]
    public class Edge
    {
        public Person[] Persons { get; set; } = new Person[2];
        
        public override string ToString()
        {
            return String.Join<Person>(", ", Persons);
        }
    }
}

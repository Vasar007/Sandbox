using System;
using System.Linq;

namespace Sandbox.LINQ_Lab
{
    public static class Task5
    {
        public static Person[] Filter(Person[] persons)
        {
            return persons
                .Where(p => p.Name.Length > persons.ToList().IndexOf(p) + 1)
                .ToArray();
        }

        public static void RunExample()
        {
            var result = Filter(new[]
            {
                new Person("Alex"), new Person("Dan"),
                new Person("Hector"), new Person("John"),
                new Person("James"), new Person("Janice")
            });

            foreach (var person in result)
            {
                Console.WriteLine(person.Name);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LINQ_Lab
{
    public static class Task4
    {
        public static string Concat(IEnumerable<Person> persons, char delimiter)
        {
            return string.Join(delimiter,
                persons.Select(p => p.Name)
                    .Skip(3));
        }

        public static void RunExample()
        {
            var result = Concat(
                new[]
                {
                    new Person("Alex"), new Person("Dan"),
                    new Person("Hector"), new Person("John"),
                    new Person("James"), new Person("Janice")
                }, ' ');

            Console.WriteLine(result);
        }
    }

    public class Task4Test
    {
        public Task4Test()
        {
        }

        [Fact]
        public void TestTask4Simple()
        {
            IReadOnlyCollection<Person> users = new List<Person>
            {
                new Person("Mark"),
                new Person("August"),
                new Person("Arthur"),
                new Person("Vasily"),
                new Person("Anton")
            };

            String actual = Task4.Concat(users, ' ');
            String expected = "Vasily Anton";

            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }
    }
}

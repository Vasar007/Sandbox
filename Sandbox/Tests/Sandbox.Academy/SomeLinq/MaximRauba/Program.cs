using System;
using System.Text;

namespace Sandbox.LINQ_Lab
{
    public class Person
    {
        public string Name { get; }

        public Person(string name)
        {
            Name = name;
        }
    }

    class Program
    {
        static void TestMain()
        {
            Console.OutputEncoding = Encoding.UTF8;

            Task4.RunExample();
            //Task5.RunExample();
            //Task6.RunExample();
            //Task7.RunExample();

            Console.ReadKey();
        }
    }
}

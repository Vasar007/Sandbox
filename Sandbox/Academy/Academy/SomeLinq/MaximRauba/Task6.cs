using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LINQ_Lab
{
    public static class Task6
    {
        public static List<(int, string[])> GroupByLength(string str)
        {
            return Regex.Split(str, @"\W+")
                .GroupBy(s => s.Length)
                .OrderByDescending(g => g.Key)
                .Select(g => (g.Key, g.ToArray()))
                .ToList();
        }

        public static void RunExample()
        {
            var result= GroupByLength("Это что же получается: ходишь, ходишь в школу, а потом бац - вторая смена");

            foreach (var g in result)
            {
                Console.WriteLine($"Длина {g.Item1}. Количество {g.Item2.Length}");
                foreach (var str in g.Item2)
                {
                    Console.WriteLine(str);   
                }

                Console.WriteLine();
            }
        }
    }
}

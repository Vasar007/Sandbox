using System;
using System.Linq;
using System.Text.RegularExpressions;
using GoogleTranslateFreeApi;

namespace LINQ_Lab
{
    public static class Task7
    {
        private static GoogleTranslator Translator { get; } = new GoogleTranslator();

        public static string[][] Translate(string text, int pageWordsCount)
        {
            var words = Regex.Split(text, @"\W+")
                .Select(word => Translator
                    .TranslateAsync(word, Language.English, Language.Russian).Result
                    .FragmentedTranslation.First()
                    .ToUpper())
                .ToList();

            return words
                .GroupBy(word => words.IndexOf(word) / pageWordsCount)
                .Select(g => g.ToArray())
                .ToArray();
        }

        public static void RunExample()
        {
            var result = Translate("This dog eats too much vegetables after lunch", 3);

            foreach (var page in result)
            {
                foreach (var word in page)
                {
                    Console.Write($"{word} ");
                }

                Console.WriteLine();
            }
        }
    }
}

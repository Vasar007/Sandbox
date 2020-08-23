using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.DataflowPipeline
{
    internal static class WordFinder
    {
        public static string FindMostCommonWord(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException(nameof(text));

            var dictionary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (string word in text.Split(" "))
            {
                if (word.Length < 4) continue;

                if (dictionary.ContainsKey(word))
                {
                    ++dictionary[word];
                }
                else
                {
                    dictionary[word] = 1;
                }
            }

            int max = dictionary.Values.Max();
            return dictionary.First(pair => pair.Value == max).Key;
        }
    }
}

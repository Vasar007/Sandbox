using System;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.CommonTasksLINQ
{
    public static class STaskLINQ
    {
        public static bool IsEqualWithInvariantCulture(this string str, string other)
        {
            return string.Compare(str, other, StringComparison.InvariantCulture) == 0;
        }

        private static string CleanString(string value)
        {
            char[] replacement = {',', ':', '—', '–', ';', '"', '\'', '(', ')', '[', ']', '<', '>',
                                  '/', '@', '.', '\n', '\r', '\t'};
            return string.Join("", value.Trim().Split(replacement)).Replace(" - ", " ");
        }

        // 4. Для выборки элементов (предполагая, что у каждого элемента есть имя Name) произвести
        // конкатенацию имен всех элементов, кроме первых трех, в одну строку, разделенных заданным
        // параметром (символом).
        public static string Task4Concat(IReadOnlyCollection<CUserDTO> users, char delimiter,
            int shiftIndex)
        {
            return string.Join(delimiter, users.Skip(shiftIndex).Select(user => user.Name));
        }

        // 5. Найти все элементы в последовательности/выборке, длина имени (количество символов) у
        // которых больше, чем позиция, которую они занимают в последовательности/выборке.
        public static IList<CUserDTO> Task5Filter(IReadOnlyCollection<CUserDTO> users)
        {
            // Считаем, что позиция элемента понимается в обывательском смысле (т.е. элемент с
            // индексом 0 имеет позицию 1 и т.д.).
            return users.Where((user, index) => user.Name.Length > index + 1).ToList();
        }

        // 6. Для заданного предложения сгруппировать слова одинаковой длины, отсортировать группы
        // по убыванию количества элементов в каждой группе, вывести информацию по каждой группе:
        // длина (количество букв в словах группы), количество элементов. Знаки препинания не
        // учитывать.
        public static IList<List<string>> Task6GroupAndSort(string sentence)
        {
            string[] filteredSentence = CleanString(sentence).Split(' ');

            List<List<string>> grouping = filteredSentence
                .GroupBy(
                    word => word.Length,
                    (key, group) => new List<string>(group)
                )
                .OrderByDescending(x => x.Count)
                .ToList();

            return grouping;
        }

        // 7. Пусть есть англо-русский словарь. Есть некоторый текст на английском языке
        // (представлен в виде последовательности слов). Необходимо сверстать из этих предложений
        // книгу на русском языке для плохо видящих так, что на одной странице книги помещается не
        // более N слов и при этом каждое слово напечатано в верхнем регистре. Перевод необходимо
        // осуществлять пословно без учета грамматики. Считается, что каждое слово имеет перевод в
        // словаре.
        public static IList<string> Task7TranslateAndProcess(string text,
            IReadOnlyDictionary<string, string> translator, int n)
        {
            if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n), "'n' must be positive");

            string[] filteredText = CleanString(text).ToLowerInvariant().Split(' ');

            List<string> grouping = filteredText
                .Select((value, index) => (PageNum: index / n, Value: value))
                .GroupBy(
                    word => word.PageNum,
                    word => translator[word.Value].ToUpperInvariant(),
                    (key, group) => string.Join(" ", group)
                )
                .ToList();

            return grouping;
        }
    }
}

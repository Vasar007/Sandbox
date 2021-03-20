using System;
using System.Collections.Generic;
using Xunit;

namespace Sandbox.CommonTasksLINQ.Test
{
    public class CTask7Test
    {
        public CTask7Test()
        {
        }

        [Fact]
        public void TestTask7Simple()
        {
            IReadOnlyDictionary<string, string> translator = new Dictionary<string, string>
            {
                { "this", "этот" },
                { "dog", "собака" },
                { "dogs", "собаки" },
                { "eat", "есть" },
                { "eats", "ест" },
                { "too", "слишком" },
                { "much", "много" },
                { "vegetable", "овощь" },
                { "vegetables", "овощи" },
                { "after", "после" },
                { "lunch", "обед" }
            };
            string text = "This dog eats too much vegetables after lunch";

            IList<string> actual = STaskLINQ.Task7TranslateAndProcess(text, translator, 3);
            IReadOnlyList<string> expected = new List<string>
            {
               "ЭТОТ СОБАКА ЕСТ",
               "СЛИШКОМ МНОГО ОВОЩИ",
               "ПОСЛЕ ОБЕД"
            };

            Assert.NotNull(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.NotEmpty(actual);
            Assert.All(actual, item => Assert.NotNull(item));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestTask7OnePerPage()
        {
            IReadOnlyDictionary<string, string> translator = new Dictionary<string, string>
            {
                { "this", "этот" },
                { "dog", "собака" },
                { "dogs", "собаки" },
                { "eat", "есть" },
                { "eats", "ест" },
                { "too", "слишком" },
                { "much", "много" },
                { "vegetable", "овощь" },
                { "vegetables", "овощи" },
                { "after", "после" },
                { "lunch", "обед" }
            };
            string text = "This vegetable eats too much dogs after lunch";

            IList<string> actual = STaskLINQ.Task7TranslateAndProcess(text, translator, 1);
            IReadOnlyList<string> expected = new List<string>
            {
                "ЭТОТ", "ОВОЩЬ", "ЕСТ", "СЛИШКОМ", "МНОГО", "СОБАКИ", "ПОСЛЕ", "ОБЕД"
            };

            Assert.NotNull(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.NotEmpty(actual);
            Assert.All(actual, item => Assert.NotNull(item));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestTask7Exception()
        {
            IReadOnlyDictionary<string, string> translator = new Dictionary<string, string>
            {
                { "this", "этот" },
                { "dog", "собака" },
                { "eat", "есть" },
                { "eats", "ест" },
                { "too", "слишком" },
                { "much", "много" },
                { "vegetable", "овощ" },
                { "vegetables", "овощи" },
                { "after", "после" },
                { "lunch", "обед" }
            };
            string text = "This dog eats too much vegetables after lunch";

            Assert.Throws<ArgumentOutOfRangeException>(
                () => STaskLINQ.Task7TranslateAndProcess(text, translator, -1)
            );
        }
    }
}

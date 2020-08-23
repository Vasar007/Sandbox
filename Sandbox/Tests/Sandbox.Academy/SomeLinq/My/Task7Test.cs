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
            IReadOnlyDictionary<String, String> translator = new Dictionary<String, String>
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
            String text = "This dog eats too much vegetables after lunch";

            IList<String> actual = STaskLINQ.Task7TranslateAndProcess(text, translator, 3);
            IReadOnlyList<String> expected = new List<String>
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
            IReadOnlyDictionary<String, String> translator = new Dictionary<String, String>
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
            String text = "This vegetable eats too much dogs after lunch";

            IList<String> actual = STaskLINQ.Task7TranslateAndProcess(text, translator, 1);
            IReadOnlyList<String> expected = new List<String>
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
            IReadOnlyDictionary<String, String> translator = new Dictionary<String, String>
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
            String text = "This dog eats too much vegetables after lunch";

            Assert.Throws<ArgumentOutOfRangeException>(
                () => STaskLINQ.Task7TranslateAndProcess(text, translator, -1)
            );
        }
    }
}

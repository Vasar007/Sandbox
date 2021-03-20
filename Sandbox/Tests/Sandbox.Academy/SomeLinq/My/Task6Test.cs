using System;
using System.Collections.Generic;
using Xunit;

namespace Sandbox.CommonTasksLINQ.Test
{
    public class CTask6Test
    {
        public CTask6Test()
        {
        }

        [Fact]
        public void TestTask6Simple()
        {
            string sentence = "Это что же получается: ходишь, ходишь в школу, а потом бац - " +
                              "вторая смена";

            IList<List<string>> actual = STaskLINQ.Task6GroupAndSort(sentence);
            IReadOnlyList<List<string>> expected = new List<List<string>>
            {
                new List<string>
                {
                    "Это", "что", "бац"
                },
                new List<string>
                {
                    "ходишь", "ходишь", "вторая"
                },
                new List<string>
                {
                    "школу", "потом", "смена"
                },
                new List<string>
                {
                    "в", "а"
                },
                new List<string>
                {
                    "же"
                },
                new List<string>
                {
                    "получается"
                },
            };

            Assert.NotNull(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.NotEmpty(actual);
            Assert.All(actual, item => Assert.NotNull(item));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestTask6OneGroup()
        {
            string sentence = "Ты, мы, он - ок";

            IList<List<string>> actual = STaskLINQ.Task6GroupAndSort(sentence);
            IReadOnlyList<List<string>> expected = new List<List<string>>
            {
                new List<string>
                {
                    "Ты", "мы", "он", "ок"
                }
            };

            Assert.NotNull(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.NotEmpty(actual);
            Assert.All(actual, item => Assert.NotNull(item));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestTask6ThreeGroups()
        {
            string sentence = "жить или не жить - вот чем мы...";

            IList<List<string>> actual = STaskLINQ.Task6GroupAndSort(sentence);
            IReadOnlyList<List<string>> expected = new List<List<string>>
            {
                new List<string>
                {
                    "или", "вот", "чем"
                },
                new List<string>
                {
                    "жить", "жить"
                },
                new List<string>
                {
                    "не", "мы"
                }
            };

            Assert.NotNull(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.NotEmpty(actual);
            Assert.All(actual, item => Assert.NotNull(item));
            Assert.Equal(expected, actual);
        }
    }
}

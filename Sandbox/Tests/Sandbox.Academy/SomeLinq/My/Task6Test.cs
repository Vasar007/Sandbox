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
            String sentence = "Это что же получается: ходишь, ходишь в школу, а потом бац - " +
                              "вторая смена";

            IList<List<String>> actual = STaskLINQ.Task6GroupAndSort(sentence);
            IReadOnlyList<List<String>> expected = new List<List<String>>
            {
                new List<String>
                {
                    "Это", "что", "бац"
                },
                new List<String>
                {
                    "ходишь", "ходишь", "вторая"
                },
                new List<String>
                {
                    "школу", "потом", "смена"
                },
                new List<String>
                {
                    "в", "а"
                },
                new List<String>
                {
                    "же"
                },
                new List<String>
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
            String sentence = "Ты, мы, он - ок";

            IList<List<String>> actual = STaskLINQ.Task6GroupAndSort(sentence);
            IReadOnlyList<List<String>> expected = new List<List<String>>
            {
                new List<String>
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
            String sentence = "жить или не жить - вот чем мы...";

            IList<List<String>> actual = STaskLINQ.Task6GroupAndSort(sentence);
            IReadOnlyList<List<String>> expected = new List<List<String>>
            {
                new List<String>
                {
                    "или", "вот", "чем"
                },
                new List<String>
                {
                    "жить", "жить"
                },
                new List<String>
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

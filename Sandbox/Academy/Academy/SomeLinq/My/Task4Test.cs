using System;
using System.Collections.Generic;
using Xunit;

namespace CommonTasksLINQ.Test
{
    public class CTask4Test
    {
        public CTask4Test()
        {
        }

        [Fact]
        public void TestTask4Simple()
        {
            IReadOnlyCollection<CUserDTO> users = new List<CUserDTO>
            {
                new CUserDTO {Name = "Mark"},
                new CUserDTO {Name = "August"},
                new CUserDTO {Name = "Arthur"},
                new CUserDTO {Name = "Vasily"},
                new CUserDTO {Name = "Anton"}
            };

            String actual = STaskLINQ.Task4Concat(users, ' ', 3);
            String expected = "Vasily Anton";

            Assert.NotNull(actual);
            Assert.True(expected.IsEqualWithInvariantCulture(actual));
        }

        [Fact]
        public void TestTask4NotStandardArgs()
        {
            IReadOnlyCollection<CUserDTO> users = new List<CUserDTO>
            {
                new CUserDTO {Name = "Mark"},
                new CUserDTO {Name = "August"},
                new CUserDTO {Name = "Arthur"},
                new CUserDTO {Name = "Vasily"},
                new CUserDTO {Name = "Anton"}
            };

            String actual = STaskLINQ.Task4Concat(users, '-', 2);
            String expected = "Arthur-Vasily-Anton";

            Assert.NotNull(actual);
            Assert.True(expected.IsEqualWithInvariantCulture(actual));
        }

        [Fact]
        public void TestTask4ConcatAll()
        {
            IReadOnlyCollection<CUserDTO> users = new List<CUserDTO>
            {
                new CUserDTO {Name = "Mark"},
                new CUserDTO {Name = "August"},
                new CUserDTO {Name = "Arthur"},
                new CUserDTO {Name = "Vasily"},
                new CUserDTO {Name = "Anton"}
            };

            String actual = STaskLINQ.Task4Concat(users, '.', 0);
            String expected = "Mark.August.Arthur.Vasily.Anton";

            Assert.NotNull(actual);
            Assert.True(expected.IsEqualWithInvariantCulture(actual));
        }
    }
}
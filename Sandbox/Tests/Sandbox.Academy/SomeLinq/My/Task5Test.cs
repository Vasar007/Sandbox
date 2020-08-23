using System.Collections.Generic;
using Xunit;

namespace Sandbox.CommonTasksLINQ.Test
{
    public class CTask5Test
    {
        public CTask5Test()
        {
        }

        [Fact]
        public void TestTask5Simple()
        {
            IReadOnlyList<CUserDTO> users = new List<CUserDTO>
            {
                new CUserDTO {Name = "Mark"},
                new CUserDTO {Name = "August"},
                new CUserDTO {Name = "Arthur"},
                new CUserDTO {Name = "Anton"},
                new CUserDTO {Name = "Vasily"}
            };

            IList<CUserDTO> actual = STaskLINQ.Task5Filter(users);
            IReadOnlyList<CUserDTO> expected = users;

            Assert.NotNull(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.NotEmpty(actual);
            Assert.All(actual, item => Assert.NotNull(item));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestTask5Nothing()
        {
            IReadOnlyList<CUserDTO> users = new List<CUserDTO>
            {
                new CUserDTO {Name = "A"},
                new CUserDTO {Name = "Jo"},
                new CUserDTO {Name = "Ron"},
                new CUserDTO {Name = "Mark"},
                new CUserDTO {Name = "Anton"}
            };

            IList<CUserDTO> actual = STaskLINQ.Task5Filter(users);
            IReadOnlyList<CUserDTO> expected = new List<CUserDTO>();

            Assert.NotNull(actual);
            Assert.Empty(actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void TestTask5Complex()
        {
            IReadOnlyList<CUserDTO> users = new List<CUserDTO>
            {
                new CUserDTO {Name = "Mark"},
                new CUserDTO {Name = "August"},
                new CUserDTO {Name = "Arthur"},
                new CUserDTO {Name = "Vasily"},
                new CUserDTO {Name = "Anton"},
                new CUserDTO {Name = "Alexander"},
                new CUserDTO {Name = "Lachesis"},
                new CUserDTO {Name = "Jessica"},
                new CUserDTO {Name = "Isabella"},
                new CUserDTO {Name = "Amelia"}
            };

            IList<CUserDTO> actual = STaskLINQ.Task5Filter(users);
            IReadOnlyList<CUserDTO> expected = new List<CUserDTO>
            {
                users[0], // Mark
                users[1], // August
                users[2], // Arthur
                users[3], // Vasily
                users[5], // Alexander
                users[6]  // Lachesis
            };

            Assert.NotNull(actual);
            Assert.NotEmpty(actual);
            Assert.All(actual, item => Assert.NotNull(item));
            Assert.Equal(expected, actual);
        }
    }
}

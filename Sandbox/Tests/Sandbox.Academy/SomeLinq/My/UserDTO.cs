using System;

namespace Sandbox.CommonTasksLINQ
{
    public class CUserDTO
    {
        public string Name { get; set; } = string.Empty;

        #region Object Overridden Methods

        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}

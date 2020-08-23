using System;

namespace CommonTasksLINQ
{
    public class CUserDTO
    {
        public String Name { get; set; } = String.Empty;

        #region Object Overridden Methods

        public override String ToString()
        {
            return Name;
        }

        #endregion
    }
}

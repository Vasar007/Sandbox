using System;

namespace TestConsoleApp.RollbackEngine
{
    public sealed class BadAction : IStatefullTask<State>
    {
        public BadAction()
        {
        }

        #region IStatefullTask<State> Implementation

        public State DoAction(State state)
        {
            throw new NotImplementedException();
        }

        public State RollbackSafe(State state)
        {
            return state;
        }

        #endregion
    }
}

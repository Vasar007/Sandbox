namespace TestConsoleApp.RollbackEngine
{
    public sealed class ActionA : IStatefullTask<State>
    {
        private int _tempState;


        public ActionA()
        {
        }

        #region IStatefullTask<State> Implementation

        public State DoAction(State state)
        {
            _tempState = state.B;

            if (state.A > 10) return new State(state.A * 10, 42);

            return state;
        }

        public State RollbackSafe(State state)
        {
            if (state.A > 100) return new State(state.A / 10, _tempState);

            return state;
        }

        #endregion
    }
}

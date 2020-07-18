namespace TestConsoleApp.RollbackEngine
{
    public sealed class ActionB : IStatefullTask<State>
    {
        public ActionB()
        {
        }

        #region IStatefullTask<State> Imlementation

        public State DoAction(State state)
        {
            return new State(state.B, state.A);
        }

        public State RollbackSafe(State state)
        {
            return new State(state.B, state.A);
        }

        #endregion
    }
}

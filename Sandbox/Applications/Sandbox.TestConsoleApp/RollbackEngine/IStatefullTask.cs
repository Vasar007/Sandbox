using System.Diagnostics.Contracts;

namespace Sandbox.TestConsoleApp.RollbackEngine
{
    internal interface IStatefullTask<TState>
    {
        // Invariant: foreach state s : Rollback(DoAction(s)) == s

        [Pure] TState DoAction(TState state);
        [Pure] TState RollbackSafe(TState state); // Should be safe

    }
}

using System;
using System.Collections.Generic;

namespace TestConsoleApp.RollbackEngine
{
    internal sealed class TaskEngine
    {
        private List<IStatefullTask<State>> Actions { get; }


        public TaskEngine()
        {
            Actions = new List<IStatefullTask<State>>();
        }

        public void Demo()
        {
            Console.WriteLine($"Start task engine demo.");

            Actions.Add(new ActionA());
            Actions.Add(new ActionB());
            Actions.Add(new ActionA());
            Actions.Add(new ActionB());

            State initialState = new State(42, 1337);
            Console.WriteLine($"Initial state: {initialState}");

            var finalState = Perform(initialState);
            Console.WriteLine($"Final state: {finalState}");

            Actions.Add(new BadAction());

            var finalState2 = Perform(initialState);
            Console.WriteLine($"Final state 2: {finalState2}");
        }

        private State Perform(State initialState)
        {
            State currentState = initialState;
            var doneActions = new Stack<IStatefullTask<State>>();

            try
            {
                foreach (IStatefullTask<State> action in Actions)
                {
                    doneActions.Push(action);

                    currentState = action.DoAction(currentState);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured:{Environment.NewLine}{ex}");
                Console.WriteLine($"Perform rollback for {doneActions.Count.ToString()} actions.");

                foreach (IStatefullTask<State> action in doneActions)
                {
                    currentState = action.RollbackSafe(currentState);
                }
            }

            return currentState;
        }
    }
}

using HeatSheetHelper.Core.States;

namespace HeatSheetHelper.Core.Helpers
{
    public class StateContext
    {
        private State _state = null;

        public StateContext(State state)
        {
            this.TransitionTo(state);
        }

        // The Context allows changing the State object at runtime.
        public void TransitionTo(State state)
        {
            Console.WriteLine($"Context: Transition to {state?.GetType().Name}.");
            this._state = state;
            this._state?.SetContext(this);
        }
        public State GetCurrentState()
        {
            Console.WriteLine($"Current State: {this._state?.GetType().Name}");
            return this._state;
        }
    }
}

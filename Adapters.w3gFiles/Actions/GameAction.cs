using System;

namespace Adapters.w3gFiles.Actions
{
    public abstract class GameAction
    {
        protected GameAction(TimeSpan occuredOn)
        {
            OccuredOn = occuredOn;
        }

        protected GameAction()
        {
            OccuredOn = default;
        }

        public TimeSpan OccuredOn { get; }
    }
}
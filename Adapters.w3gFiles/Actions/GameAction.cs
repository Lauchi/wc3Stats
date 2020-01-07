using System;

namespace Adapters.w3gFiles.Actions
{
    public abstract class GameAction
    {
        protected GameAction(TimeSpan occuredOn)
        {
            OccuredOn = occuredOn;
        }

        public TimeSpan OccuredOn { get; }
    }
}
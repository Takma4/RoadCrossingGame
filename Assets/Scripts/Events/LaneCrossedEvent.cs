using Helpers;

namespace Events
{
    /// <summary>
    /// Event to be invoked when the player crosses a <see cref="TrafficLane"/>
    /// </summary>
    public record LaneCrossedEvent : GameEvent
    {
        /// <summary>
        /// The lane that was crossed.
        /// </summary>
        public TrafficLane Lane { get; }

        public LaneCrossedEvent(TrafficLane lane)
        {
            Lane = lane;
        }
    }
}
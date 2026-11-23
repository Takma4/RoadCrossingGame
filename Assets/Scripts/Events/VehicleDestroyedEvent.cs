using Enemies;
using UnityEngine;

namespace Events
{
    /// <summary>
    /// Event to be invoked when a <see cref="TrafficVehicle"/> reaches a destruction zone (the end of its lifetime).
    /// </summary>
    public record VehicleDestroyedEvent : GameEvent
    {
        /// <summary>
        /// The lane the destroyed vehicle belonged to.
        /// </summary>
        public Transform Lane { get; }

        /// <summary>
        /// The vehicle being destroyed.
        /// </summary>
        public TrafficVehicle Vehicle { get; }

        public VehicleDestroyedEvent(Transform lane, TrafficVehicle vehicle)
        {
            Lane = lane;
            Vehicle = vehicle;
        }
    }

}

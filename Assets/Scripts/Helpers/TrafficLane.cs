using UnityEngine;

namespace Helpers
{
    /// <summary>
    /// A traffic lane is a helper class that represents a lane in which vehicles spawn.
    /// The reference point acts as both the spawn point and the middle of the lane.
    /// </summary>
    [System.Serializable]
    public class TrafficLane
    {
        /// <summary>
        /// The point from which vehicles will spawn. Also acts as the middle of the lane (horizontally).
        /// </summary>
        public Transform referencePoint;
        
        /// <summary>
        /// The total width of the lane.
        /// </summary>
        public float width;

        /// <summary>
        /// Only used for debugging.
        /// </summary>
        public float laneLength = 50f;
    }
}

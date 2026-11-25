using Events;
using Helpers;
using Managers;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Keeps track of the player's position and fires an event when the player successfully crosses a lane.
    /// It prevents double-passing by design - no lane can be passed twice and no events will be invoked in that case.
    /// Depends on the existence of a <see cref="GameManager"/> in the scene.
    /// </summary>
    public class PlayerLaneTracker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform player;

        [Header("Debug")]
        [TextArea(3, 6)]
        [SerializeField] private string log;

        /// <summary>
        /// Read-only copy of the lanes grabbed from the <see cref="GameManager"/>.
        /// </summary>
        private IReadOnlyList<TrafficLane> lanes;
        
        /// <summary>
        /// The index of the last lane the player successfully crossed.
        /// </summary>
        private int lastLane = -1;

        private void Start()
        {
            // Grab the lanes from the GameManager.
            lanes = GameManager.Instance.Lanes;

            // Initialize the last lane count (we have not passed any lanes).
            lastLane = -1;
        }

        private void Update()
        {
            // Negative count means that we have not yet passed the first lane. In that case always check the lane at
            // index 0 (the first one).
            if (lastLane < 0)
            {
                if (IsPlayerPastLane(0)) IncreaseLaneCount();
            }
            // If we're not on the very last lane, check the next one.
            else if (lastLane != lanes.Count - 1)
            {
                if (IsPlayerPastLane(lastLane + 1)) IncreaseLaneCount();
            }
        }

        /// <summary>
        /// Checks if the player is past the lane at <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the lane.</param>
        /// <returns>False if the index is out of bounds or the player is not past the lane, true otherwise.</returns>
        private bool IsPlayerPastLane(int index)
        {
            // Return false if the index is out of bounds.
            if (index < 0 || index >= lanes.Count) return false;

            // Grab the lane at the index.
            TrafficLane lane = lanes[index];

            // Get the player's global horizontal position.
            float playerX = player.position.x;

            // Get the global X coordinate of the lane's end.
            // The reference point acts as the middle so we only need to add half of the lane width to get the end.
            float laneEnd = lane.referencePoint.position.x + (lane.width / 2f);

            // True if the player is past the end, false otherwise.
            return playerX >= laneEnd;
        }

        /// <summary>
        /// Increases the last lane count by one. Invokes a <see cref="LaneCrossedEvent"/>.
        /// </summary>
        private void IncreaseLaneCount()
        {
            lastLane++;

            EventBus.Publish(new LaneCrossedEvent(lanes[lastLane]));
        }
    }
}
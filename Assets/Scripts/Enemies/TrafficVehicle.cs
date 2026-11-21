using System.Drawing;
using UnityEngine;

namespace Enemies
{
    /// <summary>
    /// A <see cref="TrafficVehicle"/> should be attached to any instances of oncoming vehicles
    /// the player has to avoid. Moves where its forward axis is pointing until it hits a destruction zone.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class TrafficVehicle : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Rigidbody rigidbody;

        [Header("Settings")]
        [SerializeField] private float moveSpeed = 10f;

        private void Reset()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            // Calculate the movement vector and add it to the current position of the rigidbody.
            Vector3 movement = moveSpeed * Time.fixedDeltaTime * transform.forward;
            rigidbody.MovePosition(rigidbody.position + movement);
        }

        /// <summary>
        /// Sets the position and rotation to match the <paramref name="point"/> Transform using the vehicle's Rigidbody.
        /// </summary>
        /// <param name="point">The transform to match.</param>
        public void Initialize(Transform point)
        {
            rigidbody.position = point.position;
            rigidbody.rotation = point.rotation;
        }

        /// <summary>
        /// Sets the position and rotation of this vehicle using the vehicle's Rigidbody.
        /// </summary>
        /// <param name="worldPos">The world coordinates.</param>
        /// <param name="rotation">The rotation Quaternion.</param>
        public void Initialize(Vector3 worldPos, Quaternion rotation)
        {
            rigidbody.position = worldPos;
            rigidbody.rotation = rotation;
        }

        /// <summary>
        /// Sets the move speed of the vehicle.
        /// </summary>
        /// <param name="speed">The new speed of the vehicle.</param>
        public void SetMoveSpeed(float speed)
        {
            // Don't allow small speeds.
            if (speed < 2f) return;

            moveSpeed = speed;
        }
    }
}
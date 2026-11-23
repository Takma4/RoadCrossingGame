using Events;
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
        [Tooltip("Raycast searches for other vehicles will originate from this transform. Defaults to this transform if empty.")]
        [SerializeField] private Transform avoidanceOrigin;

        [Header("Settings")]
        [SerializeField] private float minMoveSpeed = 7f;
        [SerializeField] private float maxMoveSpeed = 20f;
        [Tooltip("Used when the player gets hit - all the other vehicles come to a full stop using this multiplier.")]
        [SerializeField] private float stoppingSpeed = 2f;

        [Header("Vehicle Avoidance")]
        [SerializeField] private float detectionDistance = 10f;
        [SerializeField] private LayerMask vehiclesMask;

        private bool isDriving = true;
        private bool isGameOver = false;
        
        /// <summary>
        /// The current move speed.
        /// </summary>
        private float moveSpeed;

        /// <summary>
        /// The target move speed.
        /// </summary>
        private float desiredSpeed;

        private const string PLAYER_TAG = "Player";
        private const string DESTRUCTION_ZONE_TAG = "DestructionZone";

        private void Start()
        {
            // Default to this object's transform if null.
            if (avoidanceOrigin == null) avoidanceOrigin = transform;
        }

        private void Reset()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameStateEvent>(OnGameStateChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameStateEvent>(OnGameStateChanged);
        }

        private void FixedUpdate()
        {
            // Don't do any physics calculations if we're not driving.
            if (!isDriving) return;

            moveSpeed = GetMoveSpeed();
            
            // Calculate the movement vector and add it to the current position of the rigidbody.
            Vector3 movement = moveSpeed * Time.fixedDeltaTime * transform.forward;
            rigidbody.MovePosition(rigidbody.position + movement);
        }

        /// <summary>
        /// Callback for when the game state changes.
        /// </summary>
        /// <param name="stateEvent">The event arguments.</param>
        private void OnGameStateChanged(GameStateEvent stateEvent)
        {
            isGameOver = stateEvent.CurrentState == Managers.GameManager.GameState.PlayerHit;
        }

        /// <summary>
        /// Sets the position and rotation to match the <paramref name="point"/> Transform using the vehicle's Rigidbody.
        /// </summary>
        /// <param name="point">The transform to match.</param>
        public void Initialize(Transform point)
        {
            rigidbody.position = point.position;
            rigidbody.rotation = point.rotation;

            isDriving = true;
            isGameOver = false;

            PickDesiredMoveSpeed();
        }

        /// <summary>
        /// Sets the position and rotation of this vehicle using the vehicle's Rigidbody.
        /// </summary>
        /// <param name="worldPos">The world coordinate.</param>
        /// <param name="rotation">The rotation Quaternion.</param>
        public void Initialize(Vector3 worldPos, Quaternion rotation)
        {
            rigidbody.position = worldPos;
            rigidbody.rotation = rotation;

            isDriving = true;
            isGameOver = false;

            PickDesiredMoveSpeed();
        }

        /// <summary>
        /// Sets the desired move speed of the vehicle. Note that the vehicle may not reach this speed under specific circumstances.
        /// For example, it might slow down if there is another <see cref="TrafficVehicle"/> in front of it.
        /// </summary>
        /// <param name="speed">The new speed of the vehicle.</param>
        public void SetDesiredSpeed(float speed)
        {
            // Don't allow small speeds.
            if (speed < 2f) return;

            desiredSpeed = speed;
        }

        /// <summary>
        /// Picks a random move speed between <see cref="minMoveSpeed"/> and <see cref="maxMoveSpeed"/> and applies it.
        /// </summary>
        private void PickDesiredMoveSpeed()
        {
            float speed = Random.Range(minMoveSpeed, maxMoveSpeed);

            SetDesiredSpeed(speed);
        }

        /// <summary>
        /// Gets the speed for the next physics update. Tries to reach <see cref="desiredSpeed"/> but may slow down if there
        /// are obstacles ahead.
        /// </summary>
        /// <returns>The move speed closest to the <see cref="desiredSpeed"/> that the situation allows.</returns>
        private float GetMoveSpeed()
        {
            // If the player got hit, gradually reduce speed until we come to a full stop.
            if (isGameOver)
            {
                return Mathf.Lerp(moveSpeed, 0f, Time.deltaTime * stoppingSpeed);
            }
            
            // Initialize the result to the desiredSpeed. We will return the desiredSpeed if the raycast below doesn't hit anything.
            float result = desiredSpeed;

            // Check if we hit another TrafficVehicle ahead.
            if (Physics.Raycast(avoidanceOrigin.position, transform.forward, out RaycastHit hit, detectionDistance, vehiclesMask))
            {
                // Get the distance to hit vehicle expressed as value between 0 and 1.
                float normalizedDistance = hit.distance / detectionDistance;

                // Interpolate between 0 and the desired speed based on how close we are to the vehicle the raycast hit.
                // The further away we are, the closest the speed will be to desiredSpeed.
                // The closest we are, the closest the speed  will be to 0.
                // That way, we're effectively slowing down as we approach another vehicle.
                result = Mathf.Lerp(0f, desiredSpeed, normalizedDistance);
            }
            
            return result;
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Stop driving if we collided with the player.
            if (collision.collider.CompareTag(PLAYER_TAG))
            {
                isDriving = false;

                // Fire a traffic collision event.
                EventBus.Publish(new TrafficCollisionEvent());
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Destroy this gameObject if we collide with a destruction zone trigger.
            if (other.CompareTag(DESTRUCTION_ZONE_TAG))
            {
                Destroy(gameObject);
            }
        }
    }
}
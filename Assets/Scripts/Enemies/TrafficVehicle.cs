using Enemies.Data;
using Events;
using Helpers;
using Managers;
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
        [SerializeField] private VehicleConfigurationSO configuration;

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

        /// <summary>
        /// Used by the <see cref="GameManager"/> to keep track of how many vehicles are in each lane.
        /// </summary>
        private TrafficLane lane;

        /// <summary>
        /// The speed value used in the last physics update.
        /// </summary>
        public float CurrentSpeed => moveSpeed;

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
            isGameOver = stateEvent.CurrentState == GameManager.GameState.PlayerHit;
        }

        /// <summary>
        /// Sets the position and rotation to match the given <paramref name="lane"/> using the vehicle's Rigidbody.
        /// </summary>
        /// <param name="lane">The lane to place in.</param>
        public void Initialize(TrafficLane lane)
        {
            rigidbody.position = lane.referencePoint.position;
            rigidbody.rotation = lane.referencePoint.rotation;

            isDriving = true;
            isGameOver = false;

            this.lane = lane;

            PickDesiredMoveSpeed();
        }

        /// <summary>
        /// Sets the position and rotation of this vehicle using the vehicle's Rigidbody.
        /// </summary>
        /// <param name="worldPos">The world coordinate.</param>
        /// <param name="rotation">The rotation Quaternion.</param>
        /// <param name="lane">The lane this vehicle will belong to.</param>
        public void Initialize(Vector3 worldPos, Quaternion rotation, TrafficLane lane)
        {
            rigidbody.position = worldPos;
            rigidbody.rotation = rotation;

            isDriving = true;
            isGameOver = false;

            this.lane = lane;

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
        /// Picks a random move speed between the min and max move speeds defined inside of <see cref="configuration"/> and applies it.
        /// </summary>
        private void PickDesiredMoveSpeed()
        {
            float speed = configuration.RandomMoveSpeed;

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
                return Mathf.Lerp(moveSpeed, 0f, Time.deltaTime * configuration.StoppingSpeed);
            }
            
            // Initialize the result to the desiredSpeed. We will return the desiredSpeed if the raycast below doesn't hit anything.
            float result = desiredSpeed;

            // Check if we hit another TrafficVehicle ahead.
            if (Physics.Raycast(avoidanceOrigin.position, transform.forward, out RaycastHit hit, 
                configuration.DetectionDistance, configuration.VehiclesMask))
            {
                // Get the distance to hit vehicle expressed as value between 0 and 1.
                float normalizedDistance = hit.distance / configuration.DetectionDistance;

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
                // Publish the event before destroying.
                EventBus.Publish(new VehicleDestroyedEvent(lane, this));

                // Destroy this vehicle.
                Destroy(gameObject);
            }
        }
    }
}
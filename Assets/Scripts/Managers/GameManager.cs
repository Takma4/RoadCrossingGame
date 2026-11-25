using Enemies;
using Events;
using Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// A singleton that handles game flow and is responsible for spawning oncoming traffic.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public enum GameState
        {
            Readying, Playing, PlayerHit, PlayerCrossed
        }

        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private List<TrafficVehicle> vehiclePrefabs = new();
        [SerializeField] private ParticleSystem collisionParticle;

        [Header("Spawning")]
        [SerializeField] private List<TrafficLane> trafficLanes = new();
        [SerializeField] private float spawnInterval = 2f;
        [Tooltip("How many vehicles in a lane before it is considered overpopulated?")]
        [SerializeField, Min(0)] private int overpopulationThreshold = 3;

        [Header("Settings")]
        [SerializeField] private float timeToStart = 3f;
        [SerializeField] private int scorePerLane = 10;
        [Tooltip("If the player crosses lanes fast, the score reward will be multiplied by the amount of " +
            "lanes they crossed. This timer acts as the limit before the progress resets to 0.")]
        [SerializeField] private float streakResetTime = 1f;
        [Tooltip("How many points will be deducted when the player is idle. Works based on intervals (see Score Reduction Interval). " +
            "Set to a negative number to bypass.")]
        [SerializeField] private int scoreReduction = 3;
        [Tooltip("The time interval before the score is decreased. Set to a negative number to bypass.")]
        [SerializeField] private float scoreReductionInterval = 3f;

        /// <summary>
        /// Since this is a singleton and there is some destruction logic that should be happening,
        /// we keep track of whether or not this instance of <see cref="GameManager"/> is the static <see cref="Instance"/>.
        /// That is because the destruction logic should only happen on the one <see cref="Instance"/>.
        /// </summary>
        private bool clearInstance = false;
        
        private GameState state;
        private float spawnTimer = 0f;
        private float startTimer = 0f;
        private float streakTimer = 0f;
        private float idleTimer = 0f;

        private int currentScore = 0;
        private int currentSteak = 0;

        private Dictionary<TrafficLane, int> lanePopulations = new();

        public IReadOnlyList<TrafficLane> Lanes => trafficLanes;
        public int Score => currentScore;

        private void Awake()
        {
            // Destroy any instances after the first one.
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            // Set the flag so when this object gets destroyed,
            // it sets the Instance reference back to null.
            clearInstance = true;

            // Set the instance to this instance of the class.
            Instance = this;

            // Set the initial state of the game.
            state = GameState.Readying;

            // Subscribe to events.
            EventBus.Subscribe<TrafficCollisionEvent>(OnTrafficCollision);
            EventBus.Subscribe<WinZoneReachedEvent>(OnPlayerReachedWinZone);
            EventBus.Subscribe<VehicleDestroyedEvent>(OnVehicleDestroyed);
            EventBus.Subscribe<LaneCrossedEvent>(OnLaneCrossed);

            // Initialize the dictionary - initially, every lane has 0 vehicles.
            foreach (TrafficLane point in trafficLanes)
            {
                lanePopulations.Add(point, 0);
            }
        }

        private void Start()
        {
            // Publish the CountdownStartedEvent on Start() to give other scripts time to subscribe to this event on Awake().
            EventBus.Publish(new CountdownStartedEvent(timeToStart));
        }

        private void Update()
        {
            UpdateStartTimer();
            UpdateSpawnTimer();
            UpdateStreakTimer();
            UpdateIdleTimer();
        }

        /// <summary>
        /// Updates the <see cref="idleTimer"/> and deducts <see cref="scoreReduction"/> points if 
        /// <see cref="scoreReductionInterval"/> seconds have passed. Also invokes a <see cref="ScoreChangedEvent"/> if neccessary.
        /// Will skip the method if one of <see cref="scoreReductionInterval"/> and <see cref="scoreReduction"/> is negative.
        /// </summary>
        private void UpdateIdleTimer()
        {
            // Don't accidentally deduct points if we're not playing.
            if (state != GameState.Playing) return;

            // Don't do anything if we want to bypass idle score reduction.
            if (scoreReductionInterval < 0 || scoreReduction < 0) return;

            // Tick the timer.
            idleTimer += Time.deltaTime;

            // If enough seconds have passed.
            if (idleTimer > scoreReductionInterval)
            {
                // Reduce the score and clamp if neccessary.
                currentScore -= scoreReduction;
                if (currentScore < 0) currentScore = 0;

                // Spawn a floating text.
                FloatingTextManager.Instance.SpawnText(player.position, scoreReduction.ToString());

                // Publish the event.
                EventBus.Publish(new ScoreChangedEvent(currentScore));

                // Reset the timer for the next interval.
                idleTimer = 0f;
            }
        }

        /// <summary>
        /// Updates the <see cref="streakTimer"/> and resets the <see cref="currentSteak"/> if
        /// <see cref="streakResetTime"/> seconds have passed.
        /// </summary>
        private void UpdateStreakTimer()
        {
            // No need to update the timer if we have no streak going.
            if (currentSteak == 0) return;

            // Tick the timer.
            streakTimer += Time.deltaTime;

            // If enough seconds have passed.
            if (streakTimer > streakResetTime)
            {
                // Reset the streak and the timer back to 0.
                currentSteak = 0;
                streakTimer = 0f;
            }
        }

        /// <summary>
        /// Counts <see cref="timeToStart"/> seconds and sets the <see cref="state"/>
        /// to <see cref="GameState.Playing"/>.
        /// </summary>
        private void UpdateStartTimer()
        {
            // Don't do anything if we're not in the Readying state.
            if (state != GameState.Readying) return;
            
            // Tick the timer.
            startTimer += Time.deltaTime;

            // If the time has passed, change the game state.
            if (startTimer > timeToStart)
            {
                ChangeStateTo(GameState.Playing);
            }
        }

        /// <summary>
        /// Ticks the <see cref="spawnTimer"/> and spawns a vehicle if the time intreval has passed.
        /// </summary>
        private void UpdateSpawnTimer()
        {
            // Don't spawn any more vehicles if the player got hit.
            if (state == GameState.PlayerHit) return;
            
            // Tick the timer.
            spawnTimer += Time.deltaTime;

            // If the desired interval has passed,
            // spawn a new vehicle and reset the timer for the next spawn.
            if (spawnTimer > spawnInterval)
            {
                SpawnTraffic();

                spawnTimer = 0f;
            }
        }

        /// <summary>
        /// Spawns a vehicle at a random point and initializes it.
        /// </summary>
        private void SpawnTraffic()
        {
            // Grab a traffic lane at random.
            TrafficLane randomLane = GetRandomLane();

            // Grab a random vehicle from the prefabs available.
            TrafficVehicle vehicle = PickVehiclePrefab();

            // Instantiate a vehicle prefab and initialize it.
            TrafficVehicle newVehicle = Instantiate(vehicle);
            newVehicle.Initialize(randomLane);

            // Count one vehicle on the lane.
            lanePopulations[randomLane]++;
        }

        /// <summary>
        /// Grabs a random entry from the <see cref="vehiclePrefabs"/>.
        /// </summary>
        /// <returns>A random <see cref="TrafficVehicle"/> prefab or null if there are no elements inside <see cref="vehiclePrefabs"/>.</returns>
        private TrafficVehicle PickVehiclePrefab()
        {
            if (vehiclePrefabs.Count <= 0) return null;

            int rand = Random.Range(0, vehiclePrefabs.Count);
            return vehiclePrefabs[rand];
        }

        /// <summary>
        /// Returns a random vehicle spawn point. Might regect certain points if they are overpopulated.
        /// </summary>
        /// <returns>Returns a random Transform from <see cref="trafficLanes"/> or null if the list is empty.</returns>
        private TrafficLane GetRandomLane()
        {
            // Return null if the list is empty.
            if (trafficLanes.Count <= 0) return null;

            bool accepted = false;
            int rand;
            TrafficLane point;
            bool allOverpopulated = AreAllLanesOverpopulated();

            do
            {
                // Get a random inside the bounds of the list.
                // Random.Range() upper range is exclusive when using integers,
                // so we don't need to subtract 1.
                rand = Random.Range(0, trafficLanes.Count);

                // The transform corresponding to the random.
                point = trafficLanes[rand];

                // Only accept low-populated lanes.
                // Exception to this is when all lanes are overpopulated.
                // In that case, accept the first random because otherwise
                // we would get stuck in an infinite loop.
                if (allOverpopulated || lanePopulations[point] < overpopulationThreshold) accepted = true;
            } while (!accepted);
            
            // Grab the transform from the list and return it.
            return point;
        }

        /// <summary>
        /// Checks if all of the <see cref="trafficLanes"/> have currently more vehicles than <see cref="overpopulationThreshold"/>.
        /// </summary>
        /// <returns>If all lanes are overpopulated.</returns>
        private bool AreAllLanesOverpopulated()
        {
            foreach (TrafficLane lane in trafficLanes)
            {
                if (lanePopulations[lane] < overpopulationThreshold) return false;
            }

            return true;
        }

        /// <summary>
        /// Callback for <see cref="TrafficCollisionEvent"/>s. Publishes a <see cref="GameStateEvent"/> if the state is not
        /// <see cref="GameState.PlayerHit"/> already.
        /// </summary>
        /// <param name="collision">The collision object.</param>
        private void OnTrafficCollision(TrafficCollisionEvent collision)
        {
            // Play the particle if there is one and if we're not already in the hit state.
            if (collisionParticle && state != GameState.PlayerHit)
            {
                collisionParticle.transform.position = collision.Position;
                collisionParticle.Play();
            }

            ChangeStateTo(GameState.PlayerHit);
        }

        /// <summary>
        /// Callback for when the player reaches a win zone.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        private void OnPlayerReachedWinZone(WinZoneReachedEvent e)
        {
            ChangeStateTo(GameState.PlayerCrossed);
        }

        /// <summary>
        /// Callback for when a <see cref="TrafficVehicle"/> reaches the end of its lifetime.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        private void OnVehicleDestroyed(VehicleDestroyedEvent e)
        {
            // Clear one off the lane.
            lanePopulations[e.Lane]--;
        }

        /// <summary>
        /// Callback for when the player crosses a <see cref="TrafficLane"/>. Publishes a <see cref="ScoreChangedEvent"/>.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        private void OnLaneCrossed(LaneCrossedEvent e)
        {
            // Increase the streak
            currentSteak++;

            // Reset the streak timer.
            streakTimer = 0f;

            // Reset the idle timer.
            idleTimer = 0f;

            // Calculate the score to add.
            int scoreToAdd = scorePerLane * currentSteak;

            // Increase the score.
            currentScore += scoreToAdd;

            // Publish the event.
            EventBus.Publish(new ScoreChangedEvent(currentScore));

            // Calculate the position the floating text should spawn.
            Vector3 lanePosition = e.Lane.referencePoint.position;
            Vector3 spawnPosition = new(lanePosition.x, lanePosition.y + 1f, 0f);
            
            // Spawn a floating text.
            FloatingTextManager.Instance.SpawnText(spawnPosition, scoreToAdd.ToString());
        }

        /// <summary>
        /// Changes the state of the game and publishes a <see cref="GameStateEvent"/> if the <paramref name="state"/> is not equal
        /// to the existing <see cref="state"/>.
        /// </summary>
        /// <param name="state">The target state.</param>
        private void ChangeStateTo(GameState state)
        {
            // Only change the state if we're not trying to transition into the existing state.
            if (this.state != state)
            {
                // Grab the current state.
                GameState previous = this.state;

                // Change the state.
                this.state = state;

                // Publish the event.
                EventBus.Publish(new GameStateEvent(previous, state));
            }
        }

        private void OnDestroy()
        {
            // Only do destruction logic if this object is the static Instance set in Awake().
            if (clearInstance)
            {
                Instance = null;

                // Unsubscribe from events.
                EventBus.Unsubscribe<TrafficCollisionEvent>(OnTrafficCollision);
                EventBus.Unsubscribe<WinZoneReachedEvent>(OnPlayerReachedWinZone);
                EventBus.Unsubscribe<VehicleDestroyedEvent>(OnVehicleDestroyed);
                EventBus.Unsubscribe<LaneCrossedEvent>(OnLaneCrossed);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Utility - visualize all traffic lanes.
            foreach (TrafficLane lane in trafficLanes)
            {
                // Continue with the next iteration of the loop if the reference point is null.
                // Prevents exceptions from getting spammed in the console when setting up the lanes.
                if (lane.referencePoint == null) continue;

                // The reference point acts as the middle of the lane.
                Vector3 middle = lane.referencePoint.position;

                // Grab the width of the lane.
                float width = lane.width;

                // Calculate the start positions of the two sides of the lane.
                Vector3 leftStart = new(middle.x - (width / 2f), middle.y, middle.z);
                Vector3 rightStart = new(middle.x + (width / 2f), middle.y, middle.z);

                // Calculate the end positions of the two sides of the lane using the start positions.
                Vector3 leftEnd = new(leftStart.x, leftStart.y, leftStart.z + lane.laneLength);
                Vector3 rightEnd = new(rightStart.x, rightStart.y, rightStart.z + lane.laneLength);

                // Set the draw color.
                Gizmos.color = Color.cyan;

                // Draw the two lines.
                Gizmos.DrawLine(leftStart, leftEnd);
                Gizmos.DrawLine(rightStart, rightEnd);
            }
        }
    }
}
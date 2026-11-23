using Enemies;
using Events;
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
        [SerializeField] private TrafficVehicle vehiclePrefab;

        [Header("Spawning")]
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private float spawnInterval = 2f;

        [Header("Settings")]
        [SerializeField] private float timeToStart = 3f;

        private bool clearInstance = false;
        
        private GameState state;
        private float spawnTimer = 0f;
        private float startTimer = 0f;

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
            // Grab a vehicle spawn transfrom at random.
            Transform randomPoint = GetRandomSpawnPoint();
            
            // Instantiate a vehicle prefab and initialize it.
            TrafficVehicle newVehicle = Instantiate(vehiclePrefab);
            newVehicle.Initialize(randomPoint);
        }

        /// <summary>
        /// Returns a random vehicle spawn point.
        /// </summary>
        /// <returns>Returns a random Transform from <see cref="spawnPoints"/> or null if the list is empty.</returns>
        private Transform GetRandomSpawnPoint()
        {
            // Return null if the list is empty.
            if (spawnPoints.Count <= 0) return null;
            
            // Get a random inside the bounds of the list.
            // Random.Range() upper range is exclusive when using integers,
            // so we don't need to subtract 1.
            int rand = Random.Range(0, spawnPoints.Count);
            
            // Grab the transform from the list and return it.
            return spawnPoints[rand];
        }

        /// <summary>
        /// Callback for <see cref="TrafficCollisionEvent"/>s. Publishes a <see cref="GameStateEvent"/> if the state is not
        /// <see cref="GameState.PlayerHit"/> already.
        /// </summary>
        /// <param name="collision">The collision object.</param>
        private void OnTrafficCollision(TrafficCollisionEvent collision)
        {
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
            }
        }
    }
}
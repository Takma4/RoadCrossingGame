using Enemies;
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
            Playing, PlayerHit, PlayerCrossed
        }

        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private TrafficVehicle vehiclePrefab;

        [Header("Spawning")]
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private float spawnInterval = 2f;

        private bool clearInstance = false;
        
        private GameState state;
        private float spawnTimer = 0f;

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
            state = GameState.Playing;
        }

        private void Update()
        {
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

        private void OnDestroy()
        {
            // Only clear the reference if this object is the static Instance set in Awake().
            if (clearInstance) Instance = null;
        }
    }
}
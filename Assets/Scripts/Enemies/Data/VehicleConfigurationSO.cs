using UnityEngine;

namespace Enemies.Data
{
    /// <summary>
    /// This <see cref="ScriptableObject"/> contains all the data for a <see cref="TrafficVehicle"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Vehicle Configuration", menuName = "Vehicles/Configuration")]
    public class VehicleConfigurationSO : ScriptableObject
    {
        [Header("Settings")]
        [SerializeField] private float minMoveSpeed = 7f;
        [SerializeField] private float maxMoveSpeed = 20f;
        [Tooltip("Used when the player gets hit - all the other vehicles come to a full stop using this multiplier.")]
        [SerializeField] private float stoppingSpeed = 2f;

        [Header("Vehicle Avoidance")]
        [SerializeField] private float detectionDistance = 10f;
        [SerializeField] private LayerMask vehiclesMask;

        /// <summary>
        /// Generates a random speed value between the minimum and maximum allowed speeds as
        /// defined inside this ScriptableObject.
        /// </summary>
        public float RandomMoveSpeed => Random.Range(minMoveSpeed, maxMoveSpeed);

        public float MinMoveSpeed => minMoveSpeed;
        public float MaxMoveSpeed => maxMoveSpeed;
        public float StoppingSpeed => stoppingSpeed;

        public float DetectionDistance => detectionDistance;
        public LayerMask VehiclesMask => vehiclesMask;
    }
}
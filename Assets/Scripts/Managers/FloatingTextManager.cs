using Effects;
using Pools;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// This singleton is the one managing all calls for text spawns. Every script that wishes to spawn
    /// a floating text should do so using <see cref="SpawnText(Vector3, string, Color)"/>.
    /// </summary>
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private FloatingTextPool pool;

        private void Awake()
        {
            // Destroy any duplicates.
            if (Instance != null)
            {
                Debug.LogWarning($"Found another instance of the FloatingTextManager class ({name}). Destroying the newest one.");
                Destroy(gameObject);

                return;
            }

            // Set the instance.
            Instance = this;
        }

        /// <summary>
        /// Spawns a text using the <see cref="pool"/>.
        /// </summary>
        /// <param name="position">The world position to spawn.</param>
        /// <param name="text">The string to be displayed by the text.</param>
        public void SpawnText(Vector3 position, string text, Color color)
        {
            // Grab a text from the pool.
            FloatingText floatingText = pool.Get();

            // Initialize it. Will automatically play the animation and return to pool.
            floatingText.Initialize(position, text, pool, color);
        }
    }
}

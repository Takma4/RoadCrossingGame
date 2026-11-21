using UnityEngine;

namespace Managers
{
    /// <summary>
    /// A singleton that handles game flow and is responsible for spawning oncoming traffic.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private bool clearInstance = false;

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
        }

        private void OnDestroy()
        {
            // Only clear the reference if this object is the static Instance set in Awake().
            if (clearInstance) Instance = null;
        }
    }
}
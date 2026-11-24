using Effects;
using UnityEngine;
using UnityEngine.Pool;

namespace Pools
{
    /// <summary>
    /// Uses Unity's <see cref="ObjectPool{T}"/> to handle the pooling of <see cref="FloatingText"/>s.
    /// </summary>
    public class FloatingTextPool : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FloatingText prefab;

        [Header("Settings")]
        [SerializeField] private bool collectionCheck = true;
        [SerializeField] private int defaultCapacity = 10;

        private ObjectPool<FloatingText> pool;

        private void Awake()
        {
            /* 
                We'll use Unity's ObjectPool<>.
                It needs to know:
            
                * 1.How to create an instance.
                * 2.What to do when someone requests an element from the pool.
                * 3.What to do when someone releases an instance of the pool.
                * 4.What to do when an instance must be destroyed.
                * 5.Whether or not to do collection checks.
                * 6.The default capacity of the pool. 
            */
            pool = new(
                Create,
                OnGet,
                OnRelease,
                OnDestroyInstance,
                collectionCheck,
                defaultCapacity
            );
        }

        /// <summary>
        /// Grab an instance from the pool.
        /// </summary>
        /// <returns>The new instance.</returns>
        public FloatingText Get()
        {
            return pool.Get();
        }

        /// <summary>
        /// Return a previously retrieved instance back to the pool.
        /// </summary>
        /// <param name="text">The instance to return.</param>
        public void Release(FloatingText text)
        {
            pool.Release(text);
        }

        /// <summary>
        /// Instantiates a <see cref="FloatingText"/> and returns it.
        /// </summary>
        /// <returns>The new <see cref="FloatingText"/>.</returns>
        private FloatingText Create()
        {
            FloatingText newText = Instantiate(prefab);
            
            return newText;
        }

        /// <summary>
        /// Called when an instance is requested by <see cref="Get"/>.
        /// </summary>
        /// <param name="text">The instance retrieved.</param>
        private void OnGet(FloatingText text)
        {
            text.gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when an instance returns to the pool.
        /// </summary>
        /// <param name="text">The instance being released.</param>
        private void OnRelease(FloatingText text)
        {
            text.gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when the pool decides to destroy a <see cref="FloatingText"/>.
        /// </summary>
        /// <param name="text">The <see cref="FloatingText"/> about to get destroyed.</param>
        private void OnDestroyInstance(FloatingText text)
        {
            if (text != null && text.gameObject != null) Destroy(text.gameObject);
        }
    }
}

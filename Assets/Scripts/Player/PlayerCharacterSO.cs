using UnityEngine;

namespace Player
{
    /// <summary>
    /// This <see cref="ScriptableObject"/> holds all the data for needed to represent a variation of the player's appearance.
    /// </summary>
    [CreateAssetMenu(fileName = "Player Character Preset", menuName = "Player/Character")]
    public class PlayerCharacterSO : ScriptableObject
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Vector3 spawnOffset = new(0f, -1f, 0f);

        public GameObject Prefab => prefab;
        public Vector3 SpawnOffset => spawnOffset;
    }
}

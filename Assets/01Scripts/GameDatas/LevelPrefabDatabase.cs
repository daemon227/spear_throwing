using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DongA.Levels
{
    /// <summary>
    /// Runtime prefab catalogue that mirrors the index order used by the Level Editor.
    /// Assign prefabs in the Inspector so that each list index matches the itemIndex
    /// saved in the level JSON.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelPrefabDatabase", menuName = "ScriptableObjects/LevelPrefabDatabase", order = 2)]
    public class LevelPrefabDatabase : ScriptableObject
    {
        [System.Serializable]
        public class PrefabEntry
        {
            public int id;
            [PreviewField] public GameObject prefab;
        }

        [Header("Spear Prefabs (id)")]
        public List<PrefabEntry> spearPrefabs = new();

        [Header("Character Prefabs (id)")]
        public List<PrefabEntry> characterPrefabs = new();

        [Header("Obstacle Prefabs (id)")]
        public List<PrefabEntry> obstaclePrefabs = new();

        [Header("PowerUp Prefabs (id)")]
        public List<PrefabEntry> powerUpPrefabs = new();

        [Header("Decor Prefabs (id)")]
        public List<PrefabEntry> decorPrefabs = new();

        [Header("Ground SpriteShape Prefabs (id)")]
        public List<PrefabEntry> groundSpriteShapePrefabs = new();

        [Header("MouseHole Prefabs (id)")]
        public List<PrefabEntry> mouseHolePrefabs = new();


        public GameObject GetSpear(int id) => GetById(spearPrefabs, id, "Spear");
        public GameObject GetCharacter(int id) => GetById(characterPrefabs, id, "Character");
        public GameObject GetObstacle(int id) => GetById(obstaclePrefabs, id, "Obstacle");
        public GameObject GetPowerUp(int id) => GetById(powerUpPrefabs, id, "PowerUp");
        public GameObject GetDecor(int id) => GetById(decorPrefabs, id, "Decor");
        public GameObject GetGroundSpriteShape(int id) => GetById(groundSpriteShapePrefabs, id, "GroundSpriteShape");
        public GameObject GetMouseHole(int id) => GetById(mouseHolePrefabs, id, "MouseHole");

        private static GameObject GetById(List<PrefabEntry> list, int id, string category)
        {
            if (list == null)
            {
                Debug.LogWarning($"[LevelPrefabDatabase] {category} list is null");
                return null;
            }
            foreach (var entry in list)
            {
                if (entry != null && entry.id == id)
                    return entry.prefab;
            }
            Debug.LogWarning($"[LevelPrefabDatabase] {category} id {id} not found");
            return null;
        }
    }
}

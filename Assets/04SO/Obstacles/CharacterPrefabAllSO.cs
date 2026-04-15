using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Obstacles
{
    [CreateAssetMenu(fileName = "CharacterPrefabAllSO", menuName = "ScriptableObjects/Obstacles/CharacterPrefabAllSO")]
    public class CharacterPrefabAllSO : ScriptableObject
    {
        public GameObject picklePrefab;
        public GameObject bananaPrefab;
        public GameObject grapePrefab;
        public GameObject watermelonPrefab;
    }
}
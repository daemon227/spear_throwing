using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDataSO", menuName = "ScriptableObjects/Character/CharacterDataSO", order = 1)]
public class CharacterDataSO : ScriptableObject
{
    public GameObject bloodEffectPrefab;
    public Color bloodEffectColor = Color.red;
    public GameObject burnEffectPrefab;
    public GameObject thunderEffectPrefab;
    public GameObject soulPrefab;
    public List<GameObject> sliceVerticalPrefabs;
    public List<GameObject> sliceHorizontalPrefabs;
    public float bodyMultiplier = 1f;

    [Header("Dead by Slice")]
    public List<sliceDeadCharacterPrefabs> sliceDeadCharacterPrefabs;
}

[System.Serializable]
public class sliceDeadCharacterPrefabs
{
    public string SkinID;
    public List<Sprite> sliceVerticalPrefabs;
    public List<Sprite> sliceHorizontalPrefabs;
}

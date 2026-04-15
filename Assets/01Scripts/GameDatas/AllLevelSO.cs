using System.Collections;
using System.Collections.Generic;
using DongA.Levels;
using UnityEngine;

[CreateAssetMenu(fileName = "AllLevelSO", menuName = "ScriptableObjects/AllLevelSO", order = 1)]
public class AllLevelSO : ScriptableObject
{
    public int AllLevelID;
    public List<LevelDataWrapper> AllLevelData;
}

[System.Serializable]
public class LevelDataWrapper
{
    public int levelID;
    public TextAsset levelJson;
    public GameObject levelPrefab;
}

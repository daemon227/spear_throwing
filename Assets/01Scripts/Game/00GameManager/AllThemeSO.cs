using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "AllTheme", menuName = "GameDatas/AllTheme")]
public class AllThemeSO : ScriptableObject
{
    public List<GameTheme> gameThemes;
}
[System.Serializable]
public class GameTheme
{
    public int ThemeID;
    [PreviewField] 
    public GameObject themePrefab;
} 


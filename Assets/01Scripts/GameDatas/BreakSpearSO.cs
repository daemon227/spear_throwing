using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DongA.UI
{
    [CreateAssetMenu(fileName = "BreakSpearSO", menuName = "GameDatas/BreakSpearSO")]

    public class BreakSpearSO : ScriptableObject
    {
        public List<BreakSpearData> BreakSpearDatas;
    }
    [System.Serializable]
    public class BreakSpearData
    {
        public string SpearSkinID;
        public List<Sprite> BreakSprites;
    }
}


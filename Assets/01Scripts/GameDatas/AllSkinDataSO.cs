using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DongA.GameDatas
{
    [CreateAssetMenu(fileName = "AllSkinDataSO", menuName = "GameDatas/AllSkinDataSO")]
    public class AllSkinDataSO : ScriptableObject
    {
        [SerializeField] private List<SkinData> allSkinDataList;

        public List<SkinData> AllSkinDataList => allSkinDataList;
    }
}

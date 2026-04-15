using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DongA.GameDatas
{
    [CreateAssetMenu(fileName = "AllTrailSkinDataSO", menuName = "GameDatas/AllTrailSkinDataSO")]
    public class AllTrailSkinDataSO : ScriptableObject
    {
        [SerializeField] private List<TrailSkinData> allTrailSkinDataList;

        public List<TrailSkinData> AllTrailSkinDataList => allTrailSkinDataList;
    }
    
    [System.Serializable]
    public class TrailSkinData : ISkinData
    {
        public string skinName;
        public string skinNameID;
        [PreviewField] public Sprite skinSprite;
        [PreviewField] public GameObject trailPrefab;
        public SkinType skinType;
        public bool isNew;

        public string SkinName     => skinName;
        public string SkinNameID   => skinNameID;
        public SkinType SkinType => skinType;
        public Sprite SkinSprite => skinSprite;
        public bool IsNew        => isNew;

        public Sprite DeadSprite => null;
    }
}

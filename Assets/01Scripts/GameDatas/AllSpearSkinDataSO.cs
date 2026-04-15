using System.Collections;
using System.Collections.Generic;
using DongA.SpearLogic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DongA.GameDatas
{
    [CreateAssetMenu(fileName = "AllSpearSkinDataSO", menuName = "GameDatas/AllSpearSkinDataSO")]   
    public class AllSpearSkinDataSO : ScriptableObject
    {
        [SerializeField] private List<SpearSkinData> allSpearSkinDataList;

        public List<SpearSkinData> AllSpearSkinDataList => allSpearSkinDataList;

    }

    [System.Serializable]
    public class SpearSkinData : ISkinData
    {
        public string skinName;
        public string skinNameID;
        [PreviewField] public Sprite skinSprite;
        [PreviewField] public Sprite deadSprite;
        [PreviewField] public Sprite skinInRewardUI;
        public SkinType skinType;
        public bool isNew;
        /// <summary>Nếu > 0: unlock khi đạt level này (miễn phí). Nếu = 0: mua bằng coin theo skinType.</summary>
        public int unlockLevel;
        public SpearType spearType;

        public string SkinName     => skinName;
        public string SkinNameID   => skinNameID;
        public SkinType SkinType => skinType;
        public Sprite SkinSprite => skinSprite;
        public bool IsNew        => isNew;
        public Sprite DeadSprite => deadSprite;
        public int UnlockLevel   => unlockLevel;
        public SpearType SpearType => spearType;
    }
}

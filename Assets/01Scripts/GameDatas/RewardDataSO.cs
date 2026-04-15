using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DongA.GameDatas
{
    [CreateAssetMenu(fileName ="RewardDataSO", menuName = "GameDatas/RewardDataSO")]
    public class RewardDataSO : ScriptableObject
    {
        public List<RewardData> rewardDatas;
    }
    [System.Serializable]
    public class RewardData
    {
        public int rewardId;
        [PreviewField]
        public Sprite rewardSprite;
        public RewardType rewardType;
        public int coinAmount;
    }
    public enum RewardType
    {
        Coin, CharacterSkin, SpearSkin, TrailSkin
    }
}

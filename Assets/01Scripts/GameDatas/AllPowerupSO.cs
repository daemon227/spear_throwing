using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using DongA.Powerups;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DongA.GameDatas
{
    [CreateAssetMenu(fileName = "AllPowerupDataSO", menuName = "GameDatas/AllPowerupDataSO")]
    public class AllPowerupSO : ScriptableObject
    {
        public List<PowerUpData> PowerUpDatas;
    }

    [System.Serializable]
    public class PowerUpData
    {
        public int PowerupID;
        public string PowerupName;
        [PreviewField]
        public Sprite powerupImage;
        [PreviewField]
        public GameObject powerupPrefab;
    }

}

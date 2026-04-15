using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DongA.GameDatas
{
    [CreateAssetMenu(fileName = "BallDataSO", menuName = "GameDatas/Obstacle/Ball")]
    public class BallSO : ScriptableObject
    {
        public List<BallData> ballDatas;
    }
    [System.Serializable]
    public class BallData
    {
        public int id;
        [PreviewField]
        public Sprite ballSprite;
    }
}


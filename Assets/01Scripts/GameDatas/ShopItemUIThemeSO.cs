using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DongA.GameDatas
{
    [CreateAssetMenu(fileName = "ShopItemUIThemeSO", menuName = "GameDatas/ShopItemUIThemeSO")]
    public class ShopItemUIThemeSO : ScriptableObject
    {
        public ShopItemUITheme shopItemUITheme;
    }

    [System.Serializable]
    public class ShopItemUITheme
    {
        public Sprite[] skinBackGroundSprites; // [0] = locked (not owned), [1] = unlocked (owned)
        public Sprite[] spearSkinBackgroundSprites; // [0] = locked (not owned), [1] = unlocked (owned)
        public Sprite[] buttonSprites; // 0: Ads - Use, 1: Buy, 2: Using
    }
}


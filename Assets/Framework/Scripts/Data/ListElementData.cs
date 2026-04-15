using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameBase
{
    [CreateAssetMenu(fileName = "ListElementData", menuName = "Custom/List Element Data")]
    public class ListElementData : GameData
    {
        [TableList]
        public List<ElementData> Elements;

#if UNITY_EDITOR
        [Space(50)]
        [ReadOnly]
        public bool Tool;

        [Button]
        public void CopyUnlocksString()
        {
            var levels = new int[Elements.Count];
            for (int i = 0; i < Elements.Count; i++)
            {
                levels[(int)Elements[i].Type] = Elements[i].Level;
            }
            GUIUtility.systemCopyBuffer = string.Join(",", levels);
            Debug.LogError(GUIUtility.systemCopyBuffer);
        }

        [Button]
        public void SortByLevel()
        {
            Elements.Sort((a, b) => a.Level - b.Level);
        }
#endif
    }

    [Serializable]
    public class ElementData
    {
        public Element Type;
        public string Name;
        [TextArea]
        public string Description;
        public int Level;
    }

    public enum Element
    {
        ClosedGrill,
        HiddenSkew,
        FrozenSkew,
        Order,
        Smoke,
        Door,
        Marinate,
        DoubleSkewer,
        OverheatSkewer,
        ShuffleGrill,
        ShinySkewer
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace SpineTools
{
    [CreateAssetMenu(menuName = "Spine/Skins/Skin Preset", fileName = "SpineSkinPreset")]
    public class SpineSkinPresetSO : ScriptableObject
    {
        public List<string> skins = new List<string>();
    }
}

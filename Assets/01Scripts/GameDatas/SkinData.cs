using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DongA.GameDatas
{
    public interface ISkinData
    {
        string SkinName { get; }
        string SkinNameID { get; }
        SkinType SkinType { get; }
        Sprite SkinSprite { get; }
        Sprite DeadSprite{get;}
        bool IsNew { get; }
    }

    [Serializable]
    public class SkinData : ISkinData
    {
        public string skinName;
        public string skinNameID;
        public SkinType skinType;
        [PreviewField] public Sprite skinSprite;
        public bool isNew;

        public string SkinName     => skinName;
        public string SkinNameID   => skinNameID;
        public SkinType SkinType => skinType;
        public Sprite SkinSprite => skinSprite;
        public Sprite DeadSprite => null;
        public bool IsNew        => isNew;
    }

    public enum SkinType
    {
        Ads,
        Common,
        Legendary,
    }
}




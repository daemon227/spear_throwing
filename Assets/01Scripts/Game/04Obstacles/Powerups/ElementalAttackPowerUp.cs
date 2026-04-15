using System.Collections;
using System.Collections.Generic;
using DongA.SpearLogic;
using Spine;
using Spine.Unity;
using UnityEngine;


namespace DongA.Powerups
{
    /// <summary>
    /// A powerup that grants the player fire abilities.
    /// </summary>
    public class ElementalAttackPowerUp : PowerupBase
    {
        [SerializeField] private ElementalAttackType _elementalAttackType;
        [SerializeField] private GameObject _elementalEffectPrefab;
        [SerializeField] private GameObject _bulletPrefab;

        public override void PowerupReceived(GameObject spearPrefab = null, Vector2 spearDirection = default)
        {
            if(spearPrefab != null)
            {
                var spearController = spearPrefab.GetComponent<SpearController>();
                if(spearController != null)
                {
                    if (spearController.currentElementalEffect != null)
                    {
                        Destroy(spearController.currentElementalEffect);
                    }
                    var elementalEffect = Instantiate(_elementalEffectPrefab);
                    SkeletonAnimation skeletonAnimation = spearController.GetComponentInChildren<SkeletonAnimation>();
                    if (skeletonAnimation != null)
                    {
                        elementalEffect.transform.SetParent(skeletonAnimation.transform);
                        
                        BoneFollower boneFollower = elementalEffect.GetComponent<BoneFollower>();
                        if (boneFollower != null)                        
                        {
                            boneFollower.skeletonRenderer = skeletonAnimation;
                            // boneFollower.boneName = spearController.FreeTip.gameObject.name;
                            boneFollower.boneName = "Dung Im";
                        }
                    }
                    else
                    {
                        elementalEffect.transform.SetParent(spearController.transform);

                    }
                    elementalEffect.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                    spearController.currentElementalEffect = elementalEffect;
                    spearController.spearType = SpearType.ElementalAttack;
                    spearController.elementalAttackType = _elementalAttackType;
                    spearController.elementalAttackProjectilePrefab = _bulletPrefab;
                }
            }
            base.PowerupReceived(spearPrefab, spearDirection);

        }
    }
    public enum ElementalAttackType
    {
        Fire,
        Thunder
    }
}
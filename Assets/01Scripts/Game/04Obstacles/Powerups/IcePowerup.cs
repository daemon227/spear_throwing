using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DongA.Characters;
using DongA.Events;
using DongA.SpearLogic;
using UnityEngine;

namespace DongA.Powerups
{
    /// <summary>
    /// A powerup that grants the player ice abilities.
    /// </summary>
    public class IcePowerup : PowerupBase
    {
        [SerializeField] private GameObject iceProjectilePrefab;
        [SerializeField] private GameObject iceEffectUIPrefab;
        public override void PowerupReceived(GameObject spearPrefab = null, Vector2 spearDirection = default)
        {
           FreezeEnemies(iceProjectilePrefab);
            base.PowerupReceived(spearPrefab, spearDirection);
        }
        private void FreezeEnemies(GameObject iceProjectile)
        {
            CharacterBase[] allCharacters = FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);
            foreach (var target in allCharacters)
            {
                if (target == null) continue;
                if (!target.gameObject.activeInHierarchy) continue;

                GameObject projectile = Instantiate(iceProjectilePrefab, transform.position, Quaternion.identity);
                var iceProjectileComponent = projectile.GetComponent<IceProjectile>();
                if (iceProjectileComponent != null)
                {
                    iceProjectileComponent.Initialize(target);
                }
            }
            GameEvents.OnRecivePowerup?.Invoke(GameEvents.PowerupType.Ice);
            GamePrefs.IsIceFrozen = true;
            DOTween.PauseAll();
        }
    }
}

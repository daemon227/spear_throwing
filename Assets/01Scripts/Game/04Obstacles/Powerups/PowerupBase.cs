using System.Collections;
using System.Collections.Generic;
using DongA.Events;
using GameBase;
using UnityEngine;

namespace DongA.Powerups
{
    public class PowerupBase : MonoBehaviour
    {
        [Header("Powerup Settings")]
        // [SerializeField] private PowerupType powerupType;
        [SerializeField] protected GameObject breakEffectPrefab;
        [SerializeField] private AudioClip breakSound;

        public virtual void PowerupReceived(GameObject spearPrefab = null, Vector2 spearDirection = default)
        {
            if (breakEffectPrefab != null)
            {
                GameEvents.OnSpawnEffect?.Invoke(breakEffectPrefab, transform.position, Color.white);
            }
            if (breakSound != null && SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySoundAtPoint(breakSound, Camera.main.transform.position);
            }
            Destroy(gameObject);
        }
    }

    // public enum PowerupType
    // {
    //     Clone_Spear,
    //     Fire,
    //     Ice,
    //     SpeedBoost
    // }
}

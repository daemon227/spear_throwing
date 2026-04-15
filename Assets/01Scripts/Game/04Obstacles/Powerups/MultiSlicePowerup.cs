using DongA.Events;
using GameBase;
using UnityEngine;
namespace DongA.Powerups
{
    public class MultiSlicePowerup : PowerupBase
    {
        [SerializeField] private GameObject multiSliceEffectPrefab;
        public override void PowerupReceived(GameObject spearPrefab = null, Vector2 spearDirection = default)
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentGameState == GameBase.GameState.Win) return;

            if (multiSliceEffectPrefab != null)
            {
                Instantiate(multiSliceEffectPrefab, Vector3.zero, Quaternion.identity);
                GameEvents.OnMultiSliceEffectStart?.Invoke();
                GamePrefs.IsUIBlocked = true;
            }
            if(SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayBGMSound(false);
            }
            base.PowerupReceived(spearPrefab, spearDirection);
        }
    }
}


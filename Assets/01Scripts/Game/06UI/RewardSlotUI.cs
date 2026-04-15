using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class RewardSlotUI : MonoBehaviour
    {
        public Image RewardIcon;
        public TextMeshProUGUI LevelRecive;
        public Image RecivedIcon;
        public TextMeshProUGUI CoinAmount;

        private static readonly Color GrayColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        public void SetupSlotState(bool received)
        {
            if (RecivedIcon != null)
                RecivedIcon.gameObject.SetActive(received);
            if (RewardIcon != null)
                RewardIcon.color = received ? GrayColor : Color.white;
        }

        public void SetCoinAmountText(string text)
        {
            if (CoinAmount != null)
                CoinAmount.text = text;
        }

        public void PlayMilestoneAnim(System.Action onComplete)
        {
            if (RewardIcon == null)
            {
                onComplete?.Invoke();
                return;
            }

            RewardIcon.transform.DOKill();
            RewardIcon.transform
                .DOPunchScale(Vector3.one * 0.35f, 0.6f, vibrato: 8, elasticity: 0.5f)
                .SetUpdate(true)
                .OnComplete(() => onComplete?.Invoke());
        }
    }
}

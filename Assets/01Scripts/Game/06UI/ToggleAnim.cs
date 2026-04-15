using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class ToggleAnim : MonoBehaviour
    {
        [SerializeField] private ToggleType toggleType;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private Image dotImage;
        [SerializeField] private Image fillImage;
        [SerializeField] private Vector2 xPos = new Vector2(-45, 45);
        
        [SerializeField] private bool isOn = true;
        private Button button;
        void Awake()
        {
            SetInitialState();
            button = GetComponent<Button>();
            if(button!= null)
            {
                button.onClick.AddListener(OnButtonClick);
            }
        }

        public void PlayAnim()
        {
            var dotImageRectTransform = dotImage.rectTransform;
            if (isOn)
            {
                dotImageRectTransform.DOAnchorPosX(xPos.x, duration).SetUpdate(true);
                fillImage.DOFillAmount(0, duration).SetEase(Ease.OutCubic).SetUpdate(true);
            }
            else
            {
                dotImageRectTransform.DOAnchorPosX(xPos.y, duration).SetUpdate(true);
                fillImage.DOFillAmount(1, duration).SetEase(Ease.OutCubic).SetUpdate(true);
            }
            isOn = !isOn;
        }

        private void SetInitialState()
        {
            switch (toggleType)
            {
                case ToggleType.Music:
                    isOn = GlobalValues.MusicEnable;
                    break;
                case ToggleType.Sound:
                    isOn = GlobalValues.SoundEnable;
                    break;
                case ToggleType.Haptic:
                    isOn = GlobalValues.HapticEnable;
                    break;
            }
            var dotImageRectTransform = dotImage.rectTransform;
            dotImageRectTransform.anchoredPosition = new Vector2(isOn ? xPos.y : xPos.x, dotImageRectTransform.anchoredPosition.y);
            fillImage.fillAmount = isOn ? 1 : 0;
        }

        private void OnButtonClick()
        {
            PlayAnim();
            switch (toggleType)
            {
                case ToggleType.Music:
                    GamePrefs.MusicEnable = isOn;
                    break;
                case ToggleType.Sound:
                    GamePrefs.SoundEnable = isOn;
                    break;
                case ToggleType.Haptic:
                    GamePrefs.HapticEnable = isOn;
                    if (isOn && VibrationsManager.Instance != null)
                        VibrationsManager.Instance.TriggerMediumImpact();
                    break;
            }
        }

    }
    public enum ToggleType
    {
        Music, Sound, Haptic
    }
}


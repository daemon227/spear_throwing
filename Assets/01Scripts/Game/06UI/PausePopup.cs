using DongA.Events;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class PausePopup : PopupBase
    {
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button replayLevelButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button closeButton2;

        private void Awake()
        {
            nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
            if(replayLevelButton != null) replayLevelButton.onClick.AddListener(OnReplayLevelButtonClicked);
            if(closeButton != null) closeButton.onClick.AddListener(OnResumeButtonClicked);
            if(closeButton2 != null) closeButton2.onClick.AddListener(OnCloseButtonClicked);
            InitializePopup();
        }

        void OnNextLevelButtonClicked()
        {
            if (AdsUtils.IsNoInternet)
            {
                UIEvent.OnShowPopup(PopupType.NoInternet);
                return;
            }
            AdsUtils.ShowInterstitial(ParameterValue.win_level);
            AnalyticsNavigation.Button("popup_win", ParameterValue.next_level);
            UIEvent.OnNextLevelButtonClicked?.Invoke();
            UIEvent.OnClosePopupButtonClicked?.Invoke();
        }
        void OnReplayLevelButtonClicked()
        {
            // Debug.Log("Replay Level Button Clicked");
            GlobalValues.InterrupPlay();
            AnalyticsNavigation.Button("popup_pause", ParameterValue.replay);
            AdsUtils.ShowInterstitial(ParameterValue.replay);
            UIEvent.OnReplayLevelButtonClicked?.Invoke();
            UIEvent.OnClosePopupButtonClicked?.Invoke();
        }
        void OnResumeButtonClicked()
        {
            AnalyticsNavigation.Button("popup_pause", ParameterValue.resume);
            AdsUtils.ShowInterstitial(ParameterValue.resume);
            UIEvent.OnClosePopupButtonClicked?.Invoke();
        }
        void OnCloseButtonClicked()
        {
            AnalyticsNavigation.Button("popup_pause", ParameterValue.close);
            AdsUtils.ShowInterstitial(ParameterValue.resume);
            UIEvent.OnClosePopupButtonClicked?.Invoke();
        }

    }
}
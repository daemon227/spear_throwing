using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DongA.Events;
using UnityEngine.UI;
using System;

namespace DongA.UI
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private GameObject winPopupPrefab;
        [SerializeField] private GameObject losePopupPrefab;
        [SerializeField] private GameObject shopPopupPrefab;
        [SerializeField] private GameObject pausePopupPrefab;
        [SerializeField] private GameObject noAdsPopupPrefab;
        [SerializeField] private GameObject rewardPopupPrefab;
        [SerializeField] private GameObject noInternetPopupPrefab;

        [SerializeField] private Button pauseButton;

        private Stack<PopupBase> popupStack = new Stack<PopupBase>();

        void OnEnable()
        {
            UIEvent.OnShowPopup += ShowPopup;
            UIEvent.OnClosePopupButtonClicked += ClosePopup;
            UIEvent.OnShowRewardPopup += ShowRewardPopup;
            UIEvent.OnShowCoinRewardPopup += ShowCoinRewardPopup;
            pauseButton.onClick.AddListener(OnPauseButtonClicked);
        }

        private void OnPauseButtonClicked()
        {
            if(GamePrefs.IsUIBlocked || GamePrefs.IsShopTutorialPanelActive) return;
            if (GameBase.GameManager.Instance != null && GameBase.GameManager.Instance.CurrentGameState != GameBase.GameState.Playing) return;
            ShowPopup(PopupType.Pause);
        }
        

        void OnDisable()
        {
            UIEvent.OnShowPopup -= ShowPopup;
            UIEvent.OnClosePopupButtonClicked -= ClosePopup;
            UIEvent.OnShowRewardPopup -= ShowRewardPopup;
            UIEvent.OnShowCoinRewardPopup -= ShowCoinRewardPopup;
        }

        private void ShowRewardPopup(string spearNameID)
        {
            if (rewardPopupPrefab == null) return;
            var popup = Instantiate(rewardPopupPrefab).GetComponent<PopupReward>();
            if (popup == null) return;
            popup.Setup(spearNameID);
            popupStack.Push(popup);
            popup.ShowPopup();
        }

        private void ShowCoinRewardPopup(int coinAmount)
        {
            if (rewardPopupPrefab == null) return;
            var popup = Instantiate(rewardPopupPrefab).GetComponent<PopupReward>();
            if (popup == null) return;
            popup.SetupCoinReward(coinAmount);
            popupStack.Push(popup);
            popup.ShowPopup();
        }
        public void ShowPopup(PopupType popupType)
        {
            PopupBase popupInstance = null;
            var popupLocation = "";
            switch (popupType)
            {
                case PopupType.Win:
                    popupInstance = Instantiate(winPopupPrefab).GetComponent<PopupBase>();
                    popupLocation = "popup_win";
                    break;
                case PopupType.Lose:
                    popupInstance = Instantiate(losePopupPrefab).GetComponent<PopupBase>();
                    popupLocation = "popup_lose";
                    break;
                case PopupType.Pause:
                    popupInstance = Instantiate(pausePopupPrefab).GetComponent<PopupBase>();
                    popupLocation = "popup_pause";
                    break;
                case PopupType.NoAds:
                    popupInstance = Instantiate(noAdsPopupPrefab).GetComponent<PopupBase>();
                    popupLocation = "popup_no_ad";
                    break;
                case PopupType.NoInternet:
                    popupInstance = Instantiate(noInternetPopupPrefab).GetComponent<PopupBase>();
                    popupLocation = "popup_no_internet";
                    break;
                case PopupType.Shop:
                    popupInstance = Instantiate(shopPopupPrefab).GetComponent<PopupBase>();
                    popupLocation = "popup_shop";
                    break;
            }

            if (popupInstance != null)
            {
                popupStack.Push(popupInstance);
                popupInstance.ShowPopup();
                // AdsUtils.HideBanner();
                if(popupType != PopupType.NoAds && popupType != PopupType.NoInternet) GameEvents.ShowMrecUI(popupLocation);
                AnalyticsNavigation.Popup(popupLocation, ParameterValue.in_game);
            }
        }

        private void ClosePopup()
        {
            if (popupStack.Count > 0)
            {
                PopupBase topPopup = popupStack.Pop();
                // AdsUtils.HideMRecAd();
                AdsUtils.ShowBanner();
                bool shouldResumeGame = popupStack.Count == 0;
                topPopup.HidePopup(shouldResumeGame);
                Destroy(topPopup.gameObject);
            }
        }
    }
}

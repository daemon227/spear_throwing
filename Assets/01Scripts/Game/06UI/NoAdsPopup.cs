using System;
using System.Collections;
using DG.Tweening;
using DongA.Events;
using Inwave.Production.Ads;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;

namespace DongA.UI
{
    public class NoAdsPopup : PopupBase
    {
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI fakePriceUI;
        public TextMeshProUGUI salePercentText;
        public GameObject saleTag;
        public Button buyButton;
        public Button closePopupButton;

        private float currentPrice = 4.99f;
        private float fakePrice;
        private string currentProductId = GameIAP.GameIAPManager.PRODUCT_NO_ADS;
        private string currencyString ="";

        void Start()
        {
            GameEvents.OnNoAdsChanged += OnNoAdsChanged;
            GameEvents.OnBuyProductIAPCompleted += OnBuyProductCompleted;

            closePopupButton.onClick.AddListener(OnClosePopupClicked);
            buyButton.onClick.AddListener(OnBuyButtonClicked);

            UpdatePriceText();
            AnalyticIAP.IAPAppear(GlobalValues.IAP_Location, ParameterValue.no_ads_popup);

            // buyButton.gameObject.SetActive(true);
        }

        private void OnClosePopupClicked()
        {
            AdsUtils.ShowBanner();
            AnalyticsNavigation.Button(ParameterValue.no_ads_popup, ParameterValue.close);
            UIEvent.OnClosePopupButtonClicked();
        }

        private void OnNoAdsChanged(bool isNoAds)
        {
            if (!isNoAds) return;

            AdsUtils.HideBanner();
            // IAPSuccess và UpdateIapRev đã được gọi trong GameIAPManager.ReceiveRewards()
            UIEvent.OnClosePopupButtonClicked();
        }

        private void OnBuyProductCompleted(string productId, bool success)
        {
            if (!GameIAP.GameIAPManager.IsNoAdsProduct(productId)) return;
            if (success)
            {
                // AnalyticManager.OnIapCount?.Invoke();
                return; // handled by OnNoAdsChanged
            }
            
            AnalyticIAP.IAPFailed(GlobalValues.IAP_Location, ParameterValue.no_ads_popup, currentPrice, currencyString, currentProductId, "fail");
        }

        private void OnDestroy()
        {
            GameEvents.OnNoAdsChanged -= OnNoAdsChanged;
            GameEvents.OnBuyProductIAPCompleted -= OnBuyProductCompleted;
        }
        
        private void UpdatePriceText()
        {
            currentPrice = GameIAP.GameIAPManager.Instance.GetNoAdsPrice();
            currencyString = GameIAP.GameIAPManager.Instance.GetCurrencyString();
            int type = 0; // 0: normal, 1: first sale, 2: last sale
            currentProductId = GameIAP.GameIAPManager.PRODUCT_NO_ADS;
            fakePrice = currentPrice;
            if(fakePriceUI != null) fakePriceUI.text = fakePrice +"";

            if(AnalyticManager.Instance.Data.DayLogin >= RemoteConfig.Instance.RemoveAdConfig.FirstSaleDay -1 
            && AnalyticManager.Instance.Data.TotalWatchedInterAds >= RemoteConfig.Instance.RemoveAdConfig.FirstSaleInter)
            {
                type = 1;
                currentPrice = GameIAP.GameIAPManager.Instance.GetFirstSalePrice();
                currentProductId = GameIAP.GameIAPManager.PRODUCT_FIRST_SALE_NO_ADS;
            }
            if(AnalyticManager.Instance.Data.DayLogin >= RemoteConfig.Instance.RemoveAdConfig.LastSaleDay -1
            && AnalyticManager.Instance.Data.TotalWatchedInterAds >= RemoteConfig.Instance.RemoveAdConfig.LastSaleInter)
            {
                type = 2;
                currentPrice = GameIAP.GameIAPManager.Instance.GetLastSalePrice();
                currentProductId = GameIAP.GameIAPManager.PRODUCT_LAST_SALE_NO_ADS;
            }

            //float salePercent = RemoteConfig.Instance.SalePercent;
            if (priceText != null)
            {
                if(type == 0)
                {
                    saleTag.gameObject.SetActive(false);
                    fakePriceUI.gameObject.SetActive(false);
                    priceText.text = GameIAP.GameIAPManager.Instance.GetNoAdsPriceString();
                }
                else if(type == 1)
                {
                    saleTag.gameObject.SetActive(true);
                    fakePriceUI.gameObject.SetActive(true);
                    priceText.text = GameIAP.GameIAPManager.Instance.GetFirstSalePriceString();
                }
                else if(type == 2)
                {
                    saleTag.gameObject.SetActive(true);
                    fakePriceUI.gameObject.SetActive(true);
                    priceText.text = GameIAP.GameIAPManager.Instance.GetLastSalePriceString();
                }
            }
            if (salePercentText != null)
            {
                var salePercent = Mathf.RoundToInt((1 - (currentPrice / fakePrice)) * 100);
                salePercentText.text = salePercent + "%";
            }
        }

        /// <summary>
        /// Called when clicking the No Ads icon/button to show the popup
        /// </summary>

        public void OnBuyButtonClicked()
        {
            // SettingManager.Instance?.PlaySFXButtonClicked();
            AnalyticsNavigation.Button(ParameterValue.no_ads_popup, ParameterValue.btn_buy);
            
            if (GameIAP.GameIAPManager.Instance != null)
            {
                // Buy the correct product based on current sale condition
                GameIAP.GameIAPManager.Instance.BuyProduct(currentProductId);
                AnalyticIAP.IAPClick(GlobalValues.IAP_Location, ParameterValue.no_ads_popup, ParameterValue.btn_buy, currentPrice, currencyString, currentProductId);  
            }
            else
            {
                Debug.LogError("[NoAdsManager] GameIAPManager not initialized!");
                // EventManager.ShowNoticeString?.Invoke("Store not available. Please try again later.");
                //GameEvents.ShowNoticeFloat?.Invoke("Store not available. Please try again later.");
            }
            // AdsManager.Instance.ResetAppOpenAdTimer();
        }
        
        /// <summary>
        /// Restore previous purchases (iOS requirement)
        /// </summary>
        public void OnRestorePurchaseClicked()
        {
            // SettingManager.Instance?.PlaySFXButtonClicked();
            
            if (GameIAP.GameIAPManager.Instance != null)
            {
                GameIAP.GameIAPManager.Instance.RestorePurchases();
                GameIAP.GameIAPManager.Instance.CheckAndRestoreNoAds();
                //GameEvents.ShowNoticeFloat?.Invoke("Restoring purchases...");
                // EventManager.ShowNoticeString?.Invoke("Restoring purchases...");
            }
        }
    }
}

//#define DEBUG_ADS_INTERNAL
#if ADMOB
using GoogleMobileAds.Api;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inwave.Production.Ads
{
    public class AdmobImageInterstitial : MonoBehaviour
    {
#if DEBUG_ADS
        private const string LogTag = "AdmobImageInterstitial";
#endif
        [SerializeField]
        private string _adUnitIdAndroid = "YOUR_AD_UNIT_ID";
        [SerializeField]
        private string _adUnitIdIos = "YOUR_AD_UNIT_ID";

        private string _adUnitId = "unused";

        [SerializeField] private float _microValueRatio = 1000000;

        [SerializeField] private bool _useTestId = false;
#if ADMOB
        private InterstitialAd _interstitialAd = null;
#endif
        public bool IsAvailable
        {
            get
            {
#if ADMOB
                return _interstitialAd != null;
#else
                return false;
#endif
            }
        }

        private bool _loading;
        public bool Loading
        {
             get => _loading;
             private set => _loading = value;
        }

#if DEBUG_ADS
        private int _loadCounter = 0;

#endif
        public void Load()
        {
            if (_loading)
            {
                return;
            }
#if UNITY_ANDROID
            _adUnitId = _adUnitIdAndroid;
#elif UNITY_IOS
            _adUnitId = _adUnitIdIos;
#endif
            if (_useTestId)
            {
                _adUnitId = "ca-app-pub-3940256099942544/1033173712";
            }
            Destroy();

#if DEBUG_ADS
            Log($"Loading the interstitial ad: {++_loadCounter}");
#endif
#if ADMOB
            var adRequest = new AdRequest();
            _loading = true;
            AdsEvents.AdmobImageInterstitial.AdRequestLoadEvent();
            InterstitialAd.Load(_adUnitId, adRequest,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    _loading = false;
                    if (error != null || ad == null)
                    {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                        LogError($"Interstitial ad failed to load an ad with error : {error}, {_loadCounter}");
#endif

                        AdsEvents.AdmobImageInterstitial.AdLoadFailedEvent(CreateAdError(error));
                        return;
                    }
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                    Log($"Interstitial ad loaded : {_loadCounter}");
#endif
                    AdsEvents.AdmobImageInterstitial.AdLoadedEvent(CreateAdInfo());
                    _interstitialAd = ad;
                    RegisterEventHandlers(_interstitialAd);
                });
#endif
        }

        private AdNetwork.AdInfo CreateAdInfo()
        {
            AdNetwork.AdInfo adInfo = new AdNetwork.AdInfo
            {
                Format = AdsValues.AD_FORMAT_INTERSTITIAL,
                Platform = AdsValues.AD_PLATFORM_ADMOB,
                UnitId = _adUnitId
            };
            return adInfo;
        }

#if ADMOB
        private AdNetwork.AdError CreateAdError(AdError error)
        {
            AdNetwork.AdError adError = new AdNetwork.AdError
            {
                Code = error.GetCode(),
                Message = error.GetMessage(),
                UnitId = _adUnitId
            };
            return adError;
        }
#endif
#if ADMOB
        private void RegisterEventHandlers(InterstitialAd interstitialAd)
        {
            interstitialAd.OnAdPaid += (AdValue adValue) =>
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                Log($"Interstitial ad paid {adValue.Value} {adValue.CurrencyCode}.");
#endif
                var impressionData = new AdNetwork.AdImpressionData
                {
                    Currency = AdsValues.AD_REVENUE_CURRENCY,
                    Format = AdsValues.AD_FORMAT_INTERSTITIAL,
                    Platform = AdsValues.AD_PLATFORM_ADMOB,
                    Source = AdsValues.AD_PLATFORM_ADMOB,
                    Revenue = adValue.Value/_microValueRatio
                };
                AdsEvents.AdmobImageInterstitial.AdPaidEvent(impressionData);
            };

            interstitialAd.OnAdImpressionRecorded += () =>
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                Log("Interstitial ad recorded an impression.");
#endif
            };

            interstitialAd.OnAdClicked += () =>
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                Log("Interstitial ad was clicked.");
#endif
                AdNetwork.AdInfo adInfo = CreateAdInfo();
                AdsEvents.AdmobImageInterstitial.AdClickedEvent(adInfo);
            };

            interstitialAd.OnAdFullScreenContentOpened += () =>
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                Log("Interstitial ad full screen content opened.");
#endif
                AdNetwork.AdInfo adInfo = CreateAdInfo();
                AdsEvents.AdmobImageInterstitial.AdShowedEvent(adInfo);
            };

            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                Log("Interstitial ad full screen content closed.");
#endif
                Load();
                AdNetwork.AdInfo adInfo = CreateAdInfo();
                AdsEvents.AdmobImageInterstitial.AdClosedEvent(adInfo);
            };

            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogError($"Interstitial ad failed to show with error : {error}");
#endif
                Load();

                AdsEvents.AdmobImageInterstitial.AdShowFailedEvent(CreateAdInfo(),CreateAdError(error));
            };

        }
#endif
        public void Show()
        {
#if ADMOB
            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
#if DEBUG_ADS
                Log("Showing interstitial ad.");
#endif
                AdsEvents.AdmobImageInterstitial.AdRequestShowEvent();
                _interstitialAd.Show();
            }
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            else
            {
                LogError("Interstitial ad is not ready yet.");
            }
#endif
#endif
        }

        public void Destroy()
        {
#if ADMOB
            if (_interstitialAd != null)
            {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
#endif
        }

#if DEBUG_ADS
        private void Log(string message)
        {
            Utils.LogUtils.LogWarning(LogTag,message);
        }

        private void LogError(string message)
        {
            Utils.LogUtils.LogError(LogTag,message);
        }
#endif
    }
}


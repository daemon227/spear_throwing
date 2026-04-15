//#define ADMOB_7_2
#define DEBUG_ADS_INTERNAL
using System;
using Inwave.Production.Utils;
using UnityEngine;
#if ADMOB
using GoogleMobileAds.Api;
#endif
namespace Inwave.Production.Ads
{
    public class AppOpenAdmob : AppOpenAdNetwork
    {
        private static string _adPlaform = "Google Admob";
        private static string _adMediation = "Admob";
        private static string _adPlacement = "AppOpen";

        [SerializeField] 
        private string _adUnitAndroid;
        [SerializeField]
        private string _adUnitIOS;

#if ADMOB
        private AppOpenAd ad = null;

        public override bool IsAdAvailable => ad != null;

        public override void Initialize()
        {
#if DEBUG_ADS
            LogTag = "AppOpenAdmob";
#endif
            AppStateEventNotifier.AppStateChanged += AppStateChanged;
        }

        private void AppStateChanged(GoogleMobileAds.Common.AppState state)
        {
            if (state == GoogleMobileAds.Common.AppState.Background)
            {
                OnAppStateChangedEvent(AppState.Background);
            }
            else if (state == GoogleMobileAds.Common.AppState.Foreground)
            {
                OnAppStateChangedEvent(AppState.Foreground);
            }
        }

        private string adUnit;
        public override void Load()
        {
#if UNITY_ANDROID
            adUnit = _adUnitAndroid;
#elif UNITY_IOS
            adUnit = _adUnitIOS;
#else
            
#endif
            AdRequest request = new AdRequest.Builder().Build();

#if ADMOB_7_2
            AppOpenAd.LoadAd(adUnit, ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
            {
                if (error != null)
                {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                    LogUtils.LogError(LogTag,$"Failed to load the ad. (reason: {error.LoadAdError.GetResponseInfo()})");
#endif
                    return;
                }

#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.Log(LogTag,"Load AppOpenAdd Done");
#endif
                ad = appOpenAd;
                AdNetwork.AdInfo adInfo = CreateAdInfo(adUnit);
                OnAdLoadedEvent(adInfo);
            }));
#else
            AppOpenAd.Load(adUnit, ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
            {
                if (error != null)
                {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                    LogUtils.LogError(LogTag,$"Failed to load the ad. (reason: {error.GetResponseInfo()})");
#endif
                    return;
                }

#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.Log(LogTag,"Load AppOpenAdd Done");
#endif
                ad = appOpenAd;
                
            }));
#endif

        }

        public override void Show()
        { 
            if (!IsAdAvailable || isShowingAd)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogError(LogTag,"not available!");
#endif
                return;
            }

#if ADMOB_7_2
            ad.OnPaidEvent += OnPaidEvent;
            ad.OnAdDidDismissFullScreenContent += OnAdDidDismissFullScreenContent;
            ad.OnAdDidPresentFullScreenContent += OnAdDidPresentFullScreenContent;
            ad.OnAdFailedToPresentFullScreenContent += OnAdFailedToPresentFullScreenContent;
            ad.OnAdDidRecordImpression += OnAdDidRecordImpression;
#else //Admob SDK >=7.3
            ad.OnAdClicked += Ad_OnAdClicked;
            ad.OnAdFullScreenContentClosed += OnAdFullScreenContentClosed;
            ad.OnAdFullScreenContentFailed += OnAdFullScreenContentFailed;
            ad.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
            ad.OnAdImpressionRecorded += OnAdImpressionRecorded;
            ad.OnAdPaid += OnAdPaid;
#endif
            ad.Show();
        }

        private void OnAdDidRecordImpression(object sender, EventArgs e)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.Log(LogTag,"OnAdImpressionRecorded");
#endif
        }

        public static AdNetwork.AdInfo CreateAdInfo(string adUnit)
        {
            AdNetwork.AdInfo adInfo = new AdNetwork.AdInfo
            {
                Format = AdsValues.AD_FORMAT_APPOPEN,
                Mediation = _adMediation,
                Placement = _adPlacement,
                Platform = _adPlaform,
                UnitId = adUnit
            };
            return adInfo;
        }
        private void OnAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs e)
        {
            AdNetwork.AdInfo adInfo = CreateAdInfo(adUnit);
            AdNetwork.AdError adError = new AdNetwork.AdError
            {
                Code = e.AdError.GetCode(),
                Message = e.AdError.GetMessage()
            };
            OnAdShowFailedEvent(adInfo,adError);
            ad = null;
            Load();
        }

        private void OnAdDidPresentFullScreenContent(object sender, EventArgs e)
        {
            AdNetwork.AdInfo adInfo = CreateAdInfo(adUnit);
            OnAdShowedEvent(adInfo);
        }

        private void OnAdDidDismissFullScreenContent(object sender, EventArgs e)
        {
            AdNetwork.AdInfo adInfo = CreateAdInfo(adUnit);
            OnAdClosedEvent(adInfo);
            ad = null;
            
            Load();
        }

        private void OnPaidEvent(object sender, AdValueEventArgs e)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.Log(LogTag,$"OnAdPaid: {e.AdValue.Value},{e.AdValue.CurrencyCode}");
#endif
            AdNetwork.AdImpressionData impressionData = new AdNetwork.AdImpressionData
            {
                Currency = e.AdValue.CurrencyCode,
                Format = AdsValues.AD_FORMAT_APPOPEN,
                Placement = _adPlacement,
                Platform = _adPlaform,
                Revenue = e.AdValue.Value,
                Source = _adMediation
            };
            OnAdPaidEvent(impressionData);
        }
#if ADMOB_7_2

#else
        private void OnAdPaid(AdValue value)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.Log(LogTag,$"OnAdPaid: {value.Value},{value.CurrencyCode}");
#endif
        }

        private void Ad_OnAdClicked()
        {
            AdNetwork.AdInfo adInfo = CreateAdInfo(adUnit);
            OnAdClickedEvent(adInfo);
        }

        private void OnAdImpressionRecorded()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.Log(LogTag,"OnAdImpressionRecorded");
#endif
        }

        private void OnAdFullScreenContentOpened()
        {
            AdNetwork.AdInfo adInfo = CreateAdInfo(adUnit);
            OnAdShowedEvent(adInfo);
            isShowingAd = true;
        }

        private void OnAdFullScreenContentFailed(AdError adErrorAdmob)
        {
            AdNetwork.AdInfo adInfo = CreateAdInfo(adUnit);
            AdNetwork.AdError adError = new AdNetwork.AdError();
            adError.Message = adErrorAdmob.GetMessage();
            OnAdShowFailedEvent(adInfo,adError);
            ad = null;
            Load();
        }

        private void OnAdFullScreenContentClosed()
        {
            AdNetwork.AdInfo adInfo = CreateAdInfo(adUnit);
            OnAdClosedEvent(adInfo);
            ad = null;
            isShowingAd = false;
            Load();
        }
#endif

#endif
        }

}
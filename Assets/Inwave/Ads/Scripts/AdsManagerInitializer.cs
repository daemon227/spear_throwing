#define DEBUG_ADS_INTERNAL
using System;
using Inwave.Production.Utils;
using UnityEngine;
using static Inwave.Production.Ads.AdNetwork;

namespace Inwave.Production.Ads
{
    public class AdsManagerInitializer : MonoBehaviour
    {
#if DEBUG_ADS
        protected string LogTag = "AdsManagerInitializer";
#endif
        protected virtual void Start()
        {
            Init();
        }

        private void Init()
        {
            #region Banner

            // banner ads
            AdsEvents.Banner.OnAdClicked += OnBannerAdClicked;
            AdsEvents.Banner.OnAdScreenPresented += OnBannerAdScreenPresented;
            AdsEvents.Banner.OnAdLoaded += OnBannerAdLoaded;
            AdsEvents.Banner.OnAdLoadFailed += OnBannerAdLoadFailed;
            AdsEvents.Banner.OnAdScreenDismissed += OnBannerAdScreenDismissed;

            #endregion

            #region Rewarded

            //rewarded Ad
            AdsEvents.Rewarded.OnRequest += OnRewardRequest;
            AdsEvents.Rewarded.OnRequestSuccess += OnRewardRequestSucess;
            AdsEvents.Rewarded.OnRequestFailed += OnRewardRequestFailed;
            AdsEvents.Rewarded.OnRequestShowRewardAd += OnRequestShowRewardAd;
            AdsEvents.Rewarded.OnShowRewardAd += OnShowRewardAd;
            AdsEvents.Rewarded.OnRewardedVideoAdClosed += OnRewardedVideoAdClosed;
            AdsEvents.Rewarded.OnRewardedVideoAdShowFailed += OnRewardedVideoAdShowFailed;
            AdsEvents.Rewarded.OnRewardedNotAvailable += OnRewardedNotAvailable;
            AdsEvents.Rewarded.OnRewardedAvailable += OnRewardedAvailable;
            AdsEvents.Rewarded.OnRewardedAdLoadSucceed += OnRewardedAdLoadSucceed;
            AdsEvents.Rewarded.OnRewardedAdLoadFailed += OnRewardedAdLoadFailed;
            AdsEvents.Rewarded.OnRewardedVideoAdClicked += OnRewardedVideoAdClicked;
            AdsEvents.Rewarded.OnRewardedVideoAdReceived += OnRewardedVideoAdReceived;
            AdsEvents.Rewarded.OnRewardedAdShowReady += OnRewardedAdShowReady;
            AdsEvents.Rewarded.OnRewardedAdShowNotReady += OnRewardedAdShowNotReady;

            #endregion

            #region Interstitial

            //Interstitial
            AdsEvents.Interstitial.OnRequestShow += OnInterstitialRequestShow;
            AdsEvents.Interstitial.OnShow += OnShowInterstitial;
            AdsEvents.Interstitial.OnAdShowFailed += OnInterstitialAdShowFailed;
            AdsEvents.Interstitial.OnAdClosed += OnInterstitialAdClosed;
            AdsEvents.Interstitial.OnReady += OnInterstitialReady;
            AdsEvents.Interstitial.OnAdLoadSucceed += OnInterstitialLoadSucceed;
            AdsEvents.Interstitial.OnAdLoadFailed += OnInterstitialAdLoadFailed;
            AdsEvents.Interstitial.OnAdClicked += OnInterstitialAdClicked;
            AdsEvents.Interstitial.OnAdShowSucceeded += OnInterstitialAdShowSucceeded;
            AdsEvents.Interstitial.OnRequestLoad += OnInterstitialRequestLoad;

            #endregion

            #region AOA

            //AppOpenAd
            AdsEvents.AppOpen.OnAdClicked += OnAppOpenAdClicked;
            AdsEvents.AppOpen.OnAdClosed += OnAppOpenAdClosed;
            AdsEvents.AppOpen.OnAdLoaded += OnAppOpenAdLoaded;
            AdsEvents.AppOpen.OnAdShowFailed += OnAppOpenAdShowFailed;
            AdsEvents.AppOpen.OnAdShowed += OnAppOpenAdShowed;
            AdsEvents.AppOpen.OnAppStateChanged += OnAppOpenAdStateChanged;

            #endregion

            #region MREC

            //MRec
            AdsEvents.MRec.OnAdClicked += OnMRecClicked;
            AdsEvents.MRec.OnAdLoaded += OnMRecLoaded;
            AdsEvents.MRec.OnAdLoadFailed += OnMRecLoadFailed;
            AdsEvents.MRec.OnAdExpanded += OnMRecExpanded;
            AdsEvents.MRec.OnAdCollapsed += OnMRecCollapsed;

            #endregion

            #region Banner Collapsible

            //Banner Collapsible
            AdsEvents.BannerCollapsible.OnRequestLoadAd += OnBannerCollapsibleRequestLoadAd;
            AdsEvents.BannerCollapsible.OnAdLoaded += OnBannerCollapsibleAdLoaded;
            AdsEvents.BannerCollapsible.OnAdLoadFailed += OnBannerCollapsibleAdLoadFailed;
            AdsEvents.BannerCollapsible.OnAdScreenPresented += OnBannerCollapsibleAdScreenPresented;
            AdsEvents.BannerCollapsible.OnAdScreenDismissed += OnBannerCollapsibleAdScreenDismissed;
            AdsEvents.BannerCollapsible.OnAdClicked += OnBannerCollapsibleAdClicked;
            AdsEvents.BannerCollapsible.OnAdImpressionRecorded += OnBannerCollapsibleAdImpressionRecorded;
            AdsEvents.BannerCollapsible.OnAdPaid += OnBannerCollapsibleAdPaid;
            AdsEvents.BannerCollapsible.OnStartShowCollapseBanner += OnBannerCollapsibleStartShow;

            #endregion

            #region Image Interstitial
            //Admob Image Interstitial
            AdsEvents.AdmobImageInterstitial.OnAdRequestLoad += OnAdmobImageInterstitialRequestLoad;
            AdsEvents.AdmobImageInterstitial.OnAdRequestShow += OnAdmobImageInterstitialRequestShow;
            AdsEvents.AdmobImageInterstitial.OnAdLoaded += OnAdmobImageInterstitialLoaded;
            AdsEvents.AdmobImageInterstitial.OnAdShowed += OnAdmobImageInterstitialShowed;
            AdsEvents.AdmobImageInterstitial.OnAdShowFailed += OnAdmobImageInterstitialShowFailed;
            AdsEvents.AdmobImageInterstitial.OnAdClicked += OnAdmobImageInterstitialClicked;
            AdsEvents.AdmobImageInterstitial.OnAdClosed += OnAdmobImageInterstitialClosed;
            AdsEvents.AdmobImageInterstitial.OnAdPaid += OnAdmobImageInterstitialPaid;
            AdsEvents.AdmobImageInterstitial.OnAdLoadFailed += OnAdmobImageInterstitialLoadFailed;
            #endregion

            #region Fake Interstitial

            AdsEvents.FakeInterstitial.OnAdRequestLoad += OnFakeInterstitialAdRequestLoad;
            AdsEvents.FakeInterstitial.OnAdLoaded += OnFakeInterstitialAdLoaded;
            AdsEvents.FakeInterstitial.OnAdLoadFailed += OnFakeInterstitialAdLoadFailed;
            AdsEvents.FakeInterstitial.OnAdRequestShow += OnFakeInterstitialAdRequestShow;
            AdsEvents.FakeInterstitial.OnAdShowed += OnFakeInterstitialAdShowed;
            AdsEvents.FakeInterstitial.OnAdClicked += OnFakeInterstitialAdClicked;
            AdsEvents.FakeInterstitial.OnAdContentOpened += OnFakeInterstitialAdContentOpened;
            AdsEvents.FakeInterstitial.OnAdContentClosed += OnFakeInterstitialAdContentClosed;
            AdsEvents.FakeInterstitial.OnAdPaid += OnFakeInterstitialAdPaid;
            AdsEvents.FakeInterstitial.OnAdClosed += OnFakeInterstitialAdClosed;
            #endregion

            //Ad impression
            AdsEvents.OnAdImpressionSuccess += OnImpressionSuccessEvent;

#if DEBUG_ADS&&DEBUG_ADS_INTERNAL
            if (AdsManager.Instance != null)
            {
                LogUtils.LogWarning(LogTag, $"InterstitialDelay: {AdsManager.Instance.InterstitialDelay}");
                LogUtils.LogWarning(LogTag, $"NextAdsDefaultTime: {AdsManager.Instance.NextAdsDefaultTime}");
                LogUtils.LogWarning(LogTag, $"NextAdsBeforeFirstFSTime: {AdsManager.Instance.NextAdsBeforeFirstFSTime}");
                LogUtils.LogWarning(LogTag, $"NextAdsBetweenFSTime: {AdsManager.Instance.NextAdsBetweenFSTime}");
            }

#endif
        }

        protected virtual void OnImpressionSuccessEvent(AdNetwork.AdImpressionData impressionData)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnImpressionSuccessEvent: {impressionData}");
#endif
        }

        #region MRec

        private void OnMRecCollapsed(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnMRecAdCollapsed");
#endif
        }

        private void OnMRecExpanded(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnMRecAdExpanded");
#endif
        }

        private void OnMRecLoadFailed(AdNetwork.AdError adError)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogError(LogTag, $"OnMRecAdLoadFailed: {adError.ToString()}");
#endif
        }

        private void OnMRecLoaded(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnMRecAdLoaded");
#endif
        }

        private void OnMRecClicked(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnMRecAdClicked");
#endif
        }

        #endregion

        #region Banner

        protected virtual void OnBannerAdScreenPresented(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnBannerAdScreenPresented");
#endif
        }

        protected virtual void OnBannerAdClicked(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnBannerAdClicked");
#endif
        }

        protected virtual void OnBannerAdScreenDismissed(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnBannerAdScreenDismissed");
#endif
        }

        protected virtual void OnBannerAdLoadFailed(AdNetwork.AdError adError)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnBannerAdLoadFailed: {adError}");
#endif
        }

        protected virtual void OnBannerAdLoaded(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnBannerAdLoaded");
#endif
        }
        #endregion

        #region Interstitial
        protected virtual void OnInterstitialRequestLoad()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRequestLoadInterstitial");
#endif
        }
        protected virtual void OnInterstitialAdShowSucceeded(string location,AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnInterstitialAdShowSucceeded: {location}");
#endif
        }

        protected virtual void OnInterstitialAdClicked(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnInterstitialAdClicked: {location}");
#endif
        }

        protected virtual void OnInterstitialAdLoadFailed(AdNetwork.AdError adError)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnInterstitialAdLoadFailed: {adError}");
#endif
        }

        protected virtual void OnInterstitialLoadSucceed(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnInterstitialLoadSucceed: {adInfo}");
#endif
        }

        [Obsolete]
        protected virtual void OnInterstitialReady(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnInterstitialReady: {adInfo}");
#endif
        }

        protected virtual void OnInterstitialAdClosed(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnInterstitialAdClosed: {location}");
#endif
        }

        protected virtual void OnInterstitialAdShowFailed(string location, AdNetwork.AdInfo adInfo,AdNetwork.AdError adError)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnInterstitialAdShowFailed: {location}");
            LogUtils.LogWarning(LogTag, $"OnInterstitialAdShowFailed: {adInfo}");
            LogUtils.LogWarning(LogTag, $"OnInterstitialAdShowFailed: {adError}");
#endif
        }

        protected virtual void OnShowInterstitial(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnShowInterstitial: {location}");
#endif
        }

        protected virtual void OnInterstitialRequestShow(string location,AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnInterstitialRequestShow: {location}");
#endif
        }

        #endregion

        #region Rewarded
        protected virtual void OnRewardRequest(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardRequest: {adInfo}, Location: {location}");
#endif
        }

        protected virtual void OnRewardRequestSucess(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardRequest: {adInfo}, Location: {location}");
#endif
        }

        protected virtual void OnRewardRequestFailed(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardRequestFailed: {adInfo}, Location: {location}");
#endif
        }
        protected void OnRewardedAdShowNotReady(string location, AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardedAdShowNotReady: {adInfo}, Location: {location}");
#endif
        }

        protected void OnRewardedAdShowReady(string location, AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardedAdShowReady: {adInfo}, Location: {location}");
#endif
        }

        protected void OnRewardedAdLoadFailed(AdNetwork.AdError adError)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardedAdLoadFailed: {adError}");
#endif
        }

        protected void OnRewardedAdLoadSucceed(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardedAdLoadSucceed: {adInfo}");
#endif
        }
        protected virtual void OnRewardedVideoAdClicked(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardedVideoAdClicked: {location}, {adInfo}");
#endif
        }
        protected virtual void OnRewardedVideoAdReceived(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardedVideoAdClicked: {location}, {adInfo}");
#endif
        }

        [Obsolete]
        protected virtual void OnRewardedAvailable(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardedAvailable: {adInfo}");
#endif
        }

        [Obsolete]
        protected virtual void OnRewardedNotAvailable(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardedNotAvailable: {adInfo}");
#endif
        }

        protected virtual void OnRewardedVideoAdShowFailed(string location, AdNetwork.AdInfo adInfo, AdNetwork.AdError adError)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardedVideoAdShowFailed: {location}, {adInfo}, {adError}");
#endif
        }

        protected virtual void OnRewardedVideoAdClosed(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRewardedVideoAdClosed: {location}, {adInfo}");
#endif
        }

        protected virtual void OnShowRewardAd(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnShowRewardAd: {location}, {adInfo}");
#endif
        }

        protected virtual void OnRequestShowRewardAd(string location, AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnRequestShowRewardAd: {location}, {adInfo}");
#endif
        }

        #endregion

        #region AppOpenAd

        protected virtual void OnAppOpenAdClicked(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnAppOpenAdClicked");
#endif
        }

        protected virtual void OnAppOpenAdClosed(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnAppOpenAdClosed");
#endif
        }

        protected virtual void OnAppOpenAdLoaded(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnAppOpenAdLoaded");
#endif
        }

        protected virtual void OnAppOpenAdShowFailed(AdNetwork.AdInfo adInfo,AdNetwork.AdError adError)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnAppOpenAdShowFailed: {adError}");
#endif
        }

        protected virtual void OnAppOpenAdShowed(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnAppOpenAdShowed");
#endif
        }

        protected virtual void OnAppOpenAdStateChanged(AppOpenAdNetwork.AppState state)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnAppStateChanged: {state}");
#endif
        }


        #endregion

        #region Banner Collapsible

        protected virtual void OnBannerCollapsibleAdClicked(AdNetwork.AdInfo adInfo, bool isCollapsible)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnBannerCollapsibleAdClicked - IsCollapsible: {isCollapsible}");
#endif
        }

        protected virtual void OnBannerCollapsibleAdScreenDismissed(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnBannerCollapsibleAdScreenDismissed");
#endif
        }

        protected virtual void OnBannerCollapsibleAdScreenPresented(AdNetwork.AdInfo adInfo, bool isCollapsible)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnBannerCollapsibleAdScreenPresented - IsCollapsible: {isCollapsible}");
#endif
        }

        protected virtual void OnBannerCollapsibleAdLoadFailed(AdNetwork.AdError adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnBannerCollapsibleAdLoadFailed");
#endif
        }

        protected virtual void OnBannerCollapsibleAdLoaded(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnBannerCollapsibleAdLoaded");
#endif
        }

        protected virtual void OnBannerCollapsibleAdImpressionRecorded(AdNetwork.AdInfo adInfo,bool isCollapsible)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnBannerCollapsibleAdImpressionRecorded - IsCollapsible: {isCollapsible}");
#endif
        }


        protected virtual void OnBannerCollapsibleAdPaid(AdNetwork.AdImpressionData impressionData,bool isCollapsible)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnBannerCollapsibleAdPaid - IsCollapsible: {isCollapsible}");
#endif
            //AdsEvents.OnAdImpressionSuccessEvent(impressionData);
        }

        protected virtual void OnBannerCollapsibleRequestLoadAd()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnBannerCollapsibleRequestLoadAd");
#endif
        }


        protected virtual void OnBannerCollapsibleStartShow()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnBannerCollapsibleStartShow");
#endif
        }


        #endregion

        #region Admob Image Interstitial

        protected virtual void OnAdmobImageInterstitialRequestLoad()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnAdmobImageInterstitialRequestLoad");
#endif
        }

        protected virtual void OnAdmobImageInterstitialRequestShow()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnAdmobImageInterstitialRequestShow");
#endif
        }

        protected virtual void OnAdmobImageInterstitialLoadFailed(AdError adError)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnAdmobImageInterstitialLoadFailed");
#endif
        }

        protected virtual void OnAdmobImageInterstitialPaid(AdImpressionData impressionData)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"OnAdmobImageInterstitialPaid: {impressionData.Revenue} {impressionData.Currency}");
#endif
        }

        protected virtual void OnAdmobImageInterstitialClosed(AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnAdmobImageInterstitialClosed");
#endif
        }

        protected virtual void OnAdmobImageInterstitialClicked(AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnAdmobImageInterstitialClicked");
#endif
        }

        protected virtual void OnAdmobImageInterstitialShowFailed(AdInfo adInfo, AdError adError)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnAdmobImageInterstitialShowFailed");
#endif
        }

        protected virtual void OnAdmobImageInterstitialShowed(AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnAdmobImageInterstitialShowed");
#endif
        }

        protected virtual void OnAdmobImageInterstitialLoaded(AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnAdmobImageInterstitialLoaded");
#endif
        }

        #endregion

        #region Fake Interstitial

        protected virtual void OnFakeInterstitialAdClosed(AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnFakeInterstitialAdClosed");
#endif
        }

        protected virtual void OnFakeInterstitialAdPaid(AdImpressionData adImpressionData)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnFakeInterstitialAdPaid");
#endif
        }

        protected virtual void OnFakeInterstitialAdContentClosed(AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnFakeInterstitialAdContentClosed");
#endif
        }

        protected virtual void OnFakeInterstitialAdContentOpened(AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnFakeInterstitialAdContentOpened");
#endif
        }

        protected virtual void OnFakeInterstitialAdClicked(AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnFakeInterstitialAdClicked");
#endif
        }

        protected virtual void OnFakeInterstitialAdShowed(AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnFakeInterstitialAdShowed");
#endif
        }

        protected virtual void OnFakeInterstitialAdRequestShow()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnFakeInterstitialAdRequestShow");
#endif
        }

        protected virtual void OnFakeInterstitialAdLoadFailed(AdError adError)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "FakeInterstitialAdLoadFailed");
#endif
        }

        protected virtual void OnFakeInterstitialAdLoaded(AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnFakeInterstitialAdLoaded");
#endif
        }

        protected virtual void OnFakeInterstitialAdRequestLoad()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "OnFakeInterstitialAdRequestLoad");
#endif
        }

        #endregion
    }
}


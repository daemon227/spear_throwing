using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inwave.Production.Ads
{
    public class AdsEvents
    {
        public static event Action<string, string> OnSetCustomSegmentData;

        public static event Action<AdNetwork.AdImpressionData> OnAdImpressionSuccess;

        public static void OnSetCustomSegmentDataEvent(string key, string value)
        {
            OnSetCustomSegmentData?.Invoke(key,value);
        }

        public static void OnAdImpressionSuccessEvent(AdNetwork.AdImpressionData adImpressionData)
        {
            OnAdImpressionSuccess?.Invoke(adImpressionData);
        }

        public static class Banner
        {
            #region events

            /// <summary>
            /// Even raise when Ad loaded
            /// </summary>
            public static event Action<AdNetwork.AdInfo> OnAdLoaded;
            /// <summary>
            /// Event raise when fail to load Ad
            /// </summary>
            public static event Action<AdNetwork.AdError> OnAdLoadFailed;
            /// <summary>
            /// Event raise when user click Ad
            /// </summary>
            public static event Action<AdNetwork.AdInfo> OnAdClicked;
            /// <summary>
            /// Event raise when banner show to the screen
            /// </summary>
            public static event Action<AdNetwork.AdInfo> OnAdScreenPresented;
            /// <summary>
            /// Event raise when banner was dismissed
            /// </summary>
            public static event Action<AdNetwork.AdInfo> OnAdScreenDismissed;

            #endregion

            #region event raisers

            public static void OnAdLoadedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdLoaded?.Invoke(adInfo);
            }

            public static void OnAdClickedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdClicked?.Invoke(adInfo);
            }

            public static void OnAdScreenPresentedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdScreenPresented?.Invoke(adInfo);
            }

            public static void OnAdScreenDismissedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdScreenDismissed?.Invoke(adInfo);
            }

            public static void OnAdLoadFailedEvent(AdNetwork.AdError adError)
            {
                OnAdLoadFailed?.Invoke(adError);
            }
            #endregion
        }

        public static class BannerCollapsible
        {
            #region events

            /// <summary>
            /// Even raise when Ad loaded
            /// </summary>
            public static event Action<AdNetwork.AdInfo> OnAdLoaded;
            /// <summary>
            /// Event raise when fail to load Ad
            /// </summary>
            public static event Action<AdNetwork.AdError> OnAdLoadFailed;
            /// <summary>
            /// Event raise when banner show to the screen
            /// </summary>
            public static event Action<AdNetwork.AdInfo,bool> OnAdScreenPresented;
            /// <summary>
            /// Event raise when banner was dismissed
            /// </summary>
            public static event Action<AdNetwork.AdInfo> OnAdScreenDismissed;
            /// <summary>
            /// Event raise when banner was paid
            /// </summary>
            public static event Action<AdNetwork.AdImpressionData,bool> OnAdPaid;
            /// <summary>
            /// Event raise when fail to load and need a replacement
            /// </summary>
            public static event Action<BannerCollapsibleNetwork.BannerCollapsiblePosition> OnNeedReplacement;
            /// <summary>
            /// Event raise when user click on the Ad
            /// </summary>
            public static event Action<AdNetwork.AdInfo,bool> OnAdClicked;
            /// <summary>
            /// Event raise when request loading an ad
            /// </summary>
            public static event Action OnRequestLoadAd;
            /// <summary>
            /// Event raise when ad was shown and impression was recorded
            /// </summary>
            public static event Action<AdNetwork.AdInfo, bool> OnAdImpressionRecorded;
            /// <summary>
            /// Event raise before showing a Banner. For calculating fill rate of Collapsible Banner
            /// start_show = Normal + Collap
            /// </summary>
            public static event Action OnStartShowCollapseBanner;
            #endregion

            #region event raisers

            public static void OnAdLoadedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdLoaded?.Invoke(adInfo);
            }

            public static void OnAdScreenPresentedEvent(AdNetwork.AdInfo adInfo,bool isCollapsible)
            {
                OnAdScreenPresented?.Invoke(adInfo,isCollapsible);
            }

            public static void OnAdScreenDismissedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdScreenDismissed?.Invoke(adInfo);
            }

            public static void OnAdPaidEvent(AdNetwork.AdImpressionData impressionData,bool isCollapsible)
            {
                OnAdPaid?.Invoke(impressionData,isCollapsible);
            }

            public static void OnAdLoadFailedEvent(AdNetwork.AdError adError)
            {
                OnAdLoadFailed?.Invoke(adError);
            }

            public static void OnNeedReplacementEvent(BannerCollapsibleNetwork.BannerCollapsiblePosition position)
            {
                OnNeedReplacement?.Invoke(position);
            }

            public static void OnAdClickedEvent(AdNetwork.AdInfo adInfo, bool isCollapsible)
            {
                OnAdClicked?.Invoke(adInfo, isCollapsible);
            }

            public static void OnRequestLoadAdEvent()
            {
                OnRequestLoadAd?.Invoke();
            }

            public static void OnAdImpressionRecordedEvent(AdNetwork.AdInfo adInfo, bool isCollapsible)
            {
                OnAdImpressionRecorded?.Invoke(adInfo,isCollapsible);
            }

            public static void OnStartShowCollapseBannerEvent()
            {
                OnStartShowCollapseBanner?.Invoke();
            }
            #endregion
        }

        public static class Interstitial
        {
            #region events

            /// <summary>
            /// Events raise when request loading a new interstitial
            /// fullads_request
            /// </summary>
            public static event Action OnRequestLoad;

            /// <summary>
            /// Event raise when Ad loading succeed. [Obsolete] Use OnAdLoadSucceed event instead
            /// fullads_success
            /// </summary>
            [Obsolete]
            public static event Action<AdNetwork.AdInfo> OnReady;

            /// <summary>
            /// Event raise when Ad loading succeed
            /// fullads_success
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnAdLoadSucceed;

            /// <summary>
            /// Event raise when Ad loading failed
            /// fullads_failed
            /// </summary>
            public static event Action<AdNetwork.AdError> OnAdLoadFailed;

            /// <summary>
            /// Event raise when request to show interstitial Ads
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnRequestShow;

            /// <summary>
            /// Event raise when showing interstitial Ads
            /// fullads_show
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnShow;

            public static event Action<string, AdNetwork.AdInfo> OnFinish;

            /// <summary>
            /// Event raise when failing to show interstitial Ads
            /// fullads_show_failed
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo, AdNetwork.AdError> OnAdShowFailed;

            /// <summary>
            /// Event raise when closing interstitial Ads(after watching)
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnAdClosed;

            /// <summary>
            /// Event raise when user click Ad
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnAdClicked;

            /// <summary>
            /// Event raise when Ad show succeeded
            /// fullads_finish
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnAdShowSucceeded;

            /// <summary>
            /// Event raise when Ad ready to show
            /// arg1: location
            /// arg2: adInfo
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnAdShowReady;

            /// <summary>
            /// Event raise when Ad not ready to show
            /// arg1: location
            /// arg2: adInfo
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnAdShowNotReady;
            #endregion

            #region event raisers

            public static void OnRequestLoadEvent()
            {
                OnRequestLoad?.Invoke();
            }

            [Obsolete]
            public static void OnReadyEvent(AdNetwork.AdInfo adInfo)
            {
                OnReady?.Invoke(adInfo);
            }

            public static void OnLoadSucceedEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnAdLoadSucceed?.Invoke(location, adInfo);
            }

            public static void OnLoadFailedEvent(AdNetwork.AdError adError)
            {
                OnAdLoadFailed?.Invoke(adError);
            }

            public static void OnRequestShowEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnRequestShow?.Invoke(location,adInfo);
            }

            public static void OnShowEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnShow?.Invoke(location,adInfo);
            }

            public static void OnShowFailedEvent(string location, AdNetwork.AdInfo adInfo, AdNetwork.AdError adError)
            {
                OnAdShowFailed?.Invoke(location,adInfo,adError);
            }

            public static void OnClosedEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnAdClosed?.Invoke(location,adInfo);
            }

            public static void OnClickedEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnAdClicked?.Invoke(location,adInfo);
            }

            public static void OnShowSucceededEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnAdShowSucceeded?.Invoke(location,adInfo);
            }

            public static void OnAdShowReadyEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnAdShowReady?.Invoke(location,adInfo);
            }

            public static void OnAdShowNotReadyEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnAdShowNotReady?.Invoke(location, adInfo);
            }
            #endregion
        }

        public static class Rewarded
        {
            #region events

            public static Action<string, AdNetwork.AdInfo> OnRequest;
            public static Action<string, AdNetwork.AdInfo> OnRequestFailed;
            public static Action<string, AdNetwork.AdInfo> OnRequestSuccess;
            public static Action<string, AdNetwork.AdInfo> OnShow;
            public static Action<string, AdNetwork.AdInfo> OnClosed;

            /// <summary>
            /// Event raise before showing rewarded Ad
            /// arg1: location
            /// agr2: adInfo
            /// </summary>

            public static event Action<string, AdNetwork.AdInfo> OnRequestShowRewardAd;

            ///// <summary>
            ///// Event raise when Ad ready to show
            ///// </summary>
            //public event Action<string, string> OnRewardAdsReady;

            /// <summary>
            /// Event raise when showing rewarded Ad
            /// videoads_show
            /// arg1: location
            /// agr2: adInfo
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnShowRewardAd;

            /// <summary>
            /// Event raise when closing rewarded Ad(after watching)
            /// arg1: location
            /// agr2: adInfo
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnRewardedVideoAdClosed;
            
            /// <summary>
            /// Event raise when received reward Ad(after watching)
            /// arg1: location
            /// agr2: adInfo
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnRewardedVideoAdReceived;

            /// <summary>
            /// Event raise when failing to show rewarded Ad
            /// videoads_show_failed
            /// arg1: location
            /// agr2: adInfo
            /// arg3: adError
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo, AdNetwork.AdError> OnRewardedVideoAdShowFailed;

            /// <summary>
            /// Event raise when Rewarded Ad load succeed
            /// arg1: mediation
            /// agr2: adError
            /// </summary>
            public static event Action<AdNetwork.AdInfo> OnRewardedAdLoadSucceed;

            /// <summary>
            /// Event raise when Rewarded Ad load failed
            /// arg1: mediation
            /// agr2: adError
            /// </summary>
            public static event Action<AdNetwork.AdError> OnRewardedAdLoadFailed;

            /// <summary>
            /// Event raise if rewarded Ad is available, [Obsolete] OnRewardedAdLoadSucceed event instead
            /// arg1: adInfo
            /// </summary>
            [Obsolete]
            public static event Action<AdNetwork.AdInfo> OnRewardedAvailable;

            /// <summary>
            /// Event raise if rewarded Ad is not available. [Obsolete] Use OnRewardedAdLoadFailed event instead
            /// </summary>
            [Obsolete]
            public static event Action<AdNetwork.AdInfo> OnRewardedNotAvailable;

            /// <summary>
            /// Event raise when user click on ad
            /// videoads_click
            /// arg1: location
            /// agr2: adInfo
            /// </summary>
            public static event Action<string, AdNetwork.AdInfo> OnRewardedVideoAdClicked;

            /// <summary>
            /// arg1: location
            /// arg2: adInfo
            /// </summary>
            public static event Action<string,AdNetwork.AdInfo> OnRewardedAdShowReady;

            /// <summary>
            /// arg1: location
            /// arg2: adInfo
            /// </summary>
            public static event Action<string,AdNetwork.AdInfo> OnRewardedAdShowNotReady;
            /// <summary>
            /// Event raise if rewarded Ad is not available when calling to show
            /// arg1: mediation
            /// agr2: location
            /// </summary>
            //public event Action<string, string> OnRewardedVideoAdEnded;

            #endregion

            #region event raisers

            public static void OnRequestShowRewardAdEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnRequestShowRewardAd?.Invoke(location,adInfo);
            }

            public static void OnShowRewardAdEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnShowRewardAd?.Invoke(location,adInfo);
            }

            public static void OnRewardedVideoAdClosedEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnRewardedVideoAdClosed?.Invoke(location,adInfo);
            }
            public static void OnRewardedVideoAdReceivedEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnRewardedVideoAdReceived?.Invoke(location,adInfo);
            }
                
            public static void OnRewardedVideoAdShowFailedEvent(string location, AdNetwork.AdInfo adInfo,AdNetwork.AdError adError)
            {
                OnRewardedVideoAdShowFailed?.Invoke(location,adInfo,adError);
            }

            public static void OnRewardedAdLoadSucceedEvent(AdNetwork.AdInfo adInfo)
            {
                OnRewardedAdLoadSucceed?.Invoke(adInfo);
            }

            public static void OnRewardedAdLoadFailedEvent(AdNetwork.AdError adError)
            {
                OnRewardedAdLoadFailed?.Invoke(adError);
            }

            public static void OnRewardedAvailableEvent(AdNetwork.AdInfo adInfo)
            {
                OnRewardedAvailable?.Invoke(adInfo);
            }

            public static void OnRewardedNotAvailableEvent(AdNetwork.AdInfo adInfo)
            {
                OnRewardedNotAvailable?.Invoke(adInfo);
            }

            public static void OnRewardedVideoAdClickedEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnRewardedVideoAdClicked?.Invoke(location,adInfo);
            }

            public static void OnRewardedAdShowReadyEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnRewardedAdShowReady?.Invoke(location,adInfo);
            }

            public static void OnRewardedAdShowNotReadyEvent(string location, AdNetwork.AdInfo adInfo)
            {
                OnRewardedAdShowNotReady?.Invoke(location, adInfo);
            }
            #endregion
        }

        public static class AppOpen
        {
            #region events

            public static event Action<AppOpenAdNetwork.AppState> OnAppStateChanged;
            public static event Action<AdNetwork.AdInfo> OnAdClicked;
            public static event Action<AdNetwork.AdInfo> OnAdClosed;
            public static event Action<AdNetwork.AdInfo> OnAdLoaded;
            public static event Action<AdNetwork.AdInfo> OnAdShowed;
            public static event Action<AdNetwork.AdInfo,AdNetwork.AdError> OnAdShowFailed;

            #endregion

            #region event raisers

            public static void OnAppStateChangedEvent(AppOpenAdNetwork.AppState appState)
            {
                OnAppStateChanged?.Invoke(appState);
            }

            public static void OnAdClickedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdClicked?.Invoke(adInfo);
            }

            public static void OnAdClosedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdClosed?.Invoke(adInfo);
            }

            public static void OnAdLoadedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdLoaded?.Invoke(adInfo);
            }

            public static void OnAdShowedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdShowed?.Invoke(adInfo);
            }

            public static void OnAdShowFailedEvent(AdNetwork.AdInfo adInfo, AdNetwork.AdError adError)
            {
                OnAdShowFailed?.Invoke(adInfo,adError);
            }
            #endregion
        }

        public static class MRec
        {
            #region events

            public static event Action<AdNetwork.AdInfo> OnAdLoaded;
            public static event Action OnAdShowed;

            public static event Action<AdNetwork.AdError> OnAdLoadFailed;

            public static event Action<AdNetwork.AdInfo> OnAdClicked;

            public static event Action<AdNetwork.AdInfo> OnAdExpanded;

            public static event Action<AdNetwork.AdInfo> OnAdCollapsed;
            #endregion

            #region event raisers

            public static void OnAdLoadedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdLoaded?.Invoke(adInfo);
                OnAdShowed?.Invoke();
            }

            public static void OnAdLoadFailedEvent(AdNetwork.AdError adError)
            {
                OnAdLoadFailed?.Invoke(adError);
            }

            public static void OnAdClickedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdClicked?.Invoke(adInfo);
            }

            public static void OnAdExpandedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdExpanded?.Invoke(adInfo);
            }

            public static void OnAdCollapsedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdCollapsed?.Invoke(adInfo);
            }
            #endregion
        }

        public static class AdmobImageInterstitial
        {
            #region events

            public static event Action OnAdRequestShow;
            public static event Action OnAdRequestLoad; 
            public static event Action<AdNetwork.AdInfo> OnAdClicked;
            public static event Action<AdNetwork.AdInfo> OnAdClosed;
            public static event Action<AdNetwork.AdInfo> OnAdLoaded;
            public static event Action<AdNetwork.AdError> OnAdLoadFailed;
            public static event Action<AdNetwork.AdInfo> OnAdShowed;
            public static event Action<AdNetwork.AdInfo, AdNetwork.AdError> OnAdShowFailed;
            public static event Action<AdNetwork.AdImpressionData> OnAdPaid;
            #endregion

            #region event raisers

            public static void AdRequestLoadEvent()
            {
                OnAdRequestLoad?.Invoke();
            }

            public static void AdRequestShowEvent()
            {
                OnAdRequestShow?.Invoke();
            }
            public static void AdClickedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdClicked?.Invoke(adInfo);
            }

            public static void AdClosedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdClosed?.Invoke(adInfo);
            }

            public static void AdLoadedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdLoaded?.Invoke(adInfo);
            }

            public static void AdLoadFailedEvent(AdNetwork.AdError adError)
            {
                OnAdLoadFailed?.Invoke(adError);
            }

            public static void AdShowedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdShowed?.Invoke(adInfo);
            }

            public static void AdShowFailedEvent(AdNetwork.AdInfo adInfo, AdNetwork.AdError adError)
            {
                OnAdShowFailed?.Invoke(adInfo,adError);
            }

            public static void AdPaidEvent(AdNetwork.AdImpressionData impressionData)
            {
                OnAdPaid?.Invoke(impressionData);
            }

            #endregion
        }

        public static class FakeInterstitial
        {
            #region events

            public static event Action OnAdRequestShow;
            public static event Action OnAdRequestLoad;
            public static event Action<AdNetwork.AdInfo> OnAdClicked;
            /// <summary>
            /// Raise when users click 'Close' button
            /// </summary>
            public static event Action<AdNetwork.AdInfo> OnAdClosed;
            public static event Action<AdNetwork.AdInfo> OnAdLoaded;
            public static event Action<AdNetwork.AdError> OnAdLoadFailed;
            public static event Action<AdNetwork.AdInfo> OnAdShowed;
            //public static event Action<AdNetwork.AdInfo, AdNetwork.AdError> OnAdShowFailed;
            public static event Action<AdNetwork.AdImpressionData> OnAdPaid;
            /// <summary>
            /// Raise when users click on Ad and is redirected to the store
            /// </summary>
            public static event Action<AdNetwork.AdInfo> OnAdContentOpened;
            /// <summary>
            /// Raise when users comeback from store
            /// </summary>
            public static event Action<AdNetwork.AdInfo> OnAdContentClosed; 

            #endregion

            #region event raisers

            public static void AdRequestLoadEvent()
            {
                OnAdRequestLoad?.Invoke();
            }

            public static void AdRequestShowEvent()
            {
                OnAdRequestShow?.Invoke();
            }
            public static void AdClickedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdClicked?.Invoke(adInfo);
            }

            public static void AdClosedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdClosed?.Invoke(adInfo);
            }

            public static void AdLoadedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdLoaded?.Invoke(adInfo);
            }

            public static void AdLoadFailedEvent(AdNetwork.AdError adError)
            {
                OnAdLoadFailed?.Invoke(adError);
            }

            public static void AdShowedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdShowed?.Invoke(adInfo);
            }

            public static void AdLinkOpenedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdContentOpened?.Invoke(adInfo);
            }

            public static void AdLinkClosedEvent(AdNetwork.AdInfo adInfo)
            {
                OnAdContentClosed?.Invoke(adInfo);
            }

            //public static void AdShowFailedEvent(AdNetwork.AdInfo adInfo, AdNetwork.AdError adError)
            //{
            //    OnAdShowFailed?.Invoke(adInfo, adError);
            //}

            public static void AdPaidEvent(AdNetwork.AdImpressionData impressionData)
            {
                OnAdPaid?.Invoke(impressionData);
            }

            #endregion
        }
    }
}


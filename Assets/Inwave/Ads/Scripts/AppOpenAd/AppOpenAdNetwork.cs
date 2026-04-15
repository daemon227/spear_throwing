using System;
using UnityEngine;

namespace Inwave.Production.Ads
{
    public abstract class AppOpenAdNetwork : MonoBehaviour
    {
        public enum AppState
        {
            Foreground,
            Background
        }

#if DEBUG_ADS
        protected string LogTag = "AppOpenAdNetwork";
#endif
        #region events

        public event Action<AdNetwork.AdInfo> OnAdLoaded;

        public event Action<AdNetwork.AdError> OnAdLoadFailed;

        public event Action<AdNetwork.AdInfo> OnAdClicked;

        public event Action<AdNetwork.AdInfo> OnAdClosed;

        public event Action<AdNetwork.AdInfo> OnAdShowed;

        public event Action<AdNetwork.AdInfo,AdNetwork.AdError> OnAdShowFailed;

        public event Action<AppState> OnAppStateChanged;

        public event Action<AdNetwork.AdImpressionData> OnAdPaid;

        #endregion

        protected bool isShowingAd = false;

        public bool IsShowingAd => isShowingAd;

        public virtual bool IsAdAvailable { get; }

        public virtual void Initialize()
        {

        }

        public virtual void Load(){}

        public virtual void Show(){}

        protected virtual void OnAdLoadedEvent(AdNetwork.AdInfo adInfo)
        {
            OnAdLoaded?.Invoke(adInfo);
        }

        protected virtual void OnAdLoadFailedEvent(AdNetwork.AdError adError)
        {
            OnAdLoadFailed?.Invoke(adError);
        }

        protected virtual void OnAdClickedEvent(AdNetwork.AdInfo adInfo)
        {
            OnAdClicked?.Invoke(adInfo);
        }

        protected virtual void OnAdClosedEvent(AdNetwork.AdInfo adInfo)
        {
            isShowingAd = false;
            OnAdClosed?.Invoke(adInfo);
        }

        protected virtual void OnAdShowedEvent(AdNetwork.AdInfo adInfo)
        {
            isShowingAd = true;
            OnAdShowed?.Invoke(adInfo);
        }

        protected virtual void OnAdShowFailedEvent(AdNetwork.AdInfo adInfo,AdNetwork.AdError adError)
        {
            isShowingAd = false;
            OnAdShowFailed?.Invoke(adInfo,adError);
        }

        protected virtual void OnAppStateChangedEvent(AppState state)
        {
            OnAppStateChanged?.Invoke(state);
        }

        protected virtual void OnAdPaidEvent(AdNetwork.AdImpressionData impressionData)
        {
            OnAdPaid?.Invoke(impressionData);
        }
    }


}


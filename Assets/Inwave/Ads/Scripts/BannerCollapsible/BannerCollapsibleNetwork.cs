//#define DEBUG_ADS_INTERNAL
using Inwave.Production.Utils;
using UnityEngine;

namespace Inwave.Production.Ads
{
    public abstract class BannerCollapsibleNetwork : MonoBehaviour
    {
        public enum BannerCollapsiblePosition
        {
            TOP,
            BOTTOM
        }

        public enum BannerCollapsibleType
        {
            None,
            Normal,
            Collapsible
        }
#if DEBUG_ADS
        protected string LogTag = "BannerCollapsibleNetwork";
#endif

        protected BannerCollapsiblePosition _collapPosition = BannerCollapsiblePosition.BOTTOM;

        public BannerCollapsiblePosition Position
        {
            get => _collapPosition;
            set => _collapPosition = value;
        }

        public virtual void Initialize()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log("Initialize");
#endif
        }

        public virtual void Create(BannerCollapsiblePosition position = BannerCollapsiblePosition.BOTTOM)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log("Create");
#endif
        }

        public virtual void Load(BannerCollapsiblePosition position = BannerCollapsiblePosition.BOTTOM,
            bool hideAfterLoad = true)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log("Load");
#endif
        }

        public virtual void Show(BannerCollapsiblePosition position = BannerCollapsiblePosition.BOTTOM)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log("Show");
#endif
        }

        public virtual void Hide()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log("Hide");
#endif
        }

        public virtual void DestroyAd()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log("DestroyAd");
#endif
        }
#if DEBUG_ADS
        protected void Log(string info)
        {
            LogUtils.LogWarning(LogTag,info);
        }
#endif

        protected virtual void OnRequestLoadEvent()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log("OnRequestLoadEvent");
#endif
            AdsEvents.BannerCollapsible.OnRequestLoadAdEvent();
        }

        protected virtual void OnAdLoadedEvent(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log("OnAdLoadedEvent");
#endif
            AdsEvents.BannerCollapsible.OnAdLoadedEvent(adInfo);
        }

        protected virtual void OnAdLoadFailedEvent(AdNetwork.AdError adError)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log("OnAdLoadFailedEvent");
#endif
            AdsEvents.BannerCollapsible.OnAdLoadFailedEvent(adError);
        }

        protected virtual void OnAdClickedEvent(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log($"OnAdClickedEvent - {IsCollapsible}");
#endif
            AdsEvents.BannerCollapsible.OnAdClickedEvent(adInfo,IsCollapsible);
        }

        protected virtual void OnAdClosedEvent(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log("OnAdClosedEvent");
#endif
            AdsEvents.BannerCollapsible.OnAdScreenDismissedEvent(adInfo);
        }

        protected virtual void OnAdShowedEvent(AdNetwork.AdInfo adInfo)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log($"OnAdShowedEvent - IsCollapsible: {IsCollapsible}");
#endif
            AdsEvents.BannerCollapsible.OnAdScreenPresentedEvent(adInfo, IsCollapsible);
        }

        protected virtual void OnAdImpressionRecordedEvent(AdNetwork.AdInfo adInfo, bool isCollapsible)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log($"OnAdImpressionRecordedEvent - {IsCollapsible}");
#endif
            AdsEvents.BannerCollapsible.OnAdImpressionRecordedEvent(adInfo,isCollapsible);
        }

        protected void OnAdPaidEvent(AdNetwork.AdImpressionData impressionData,bool isCollapsible)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log($"OnAdPaidEvent - {isCollapsible}");
#endif
            AdsEvents.BannerCollapsible.OnAdPaidEvent(impressionData, isCollapsible);
        }

        protected virtual void OnNeedReplacementEvent()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            Log("OnNeedReplacementEvent");
#endif
            AdsEvents.BannerCollapsible.OnNeedReplacementEvent(_collapPosition);
        }

        [SerializeField]
        protected bool _useReplacementWhenFailed = true;


        protected bool _isReplacement = false;

        protected bool _loaded = false;

        public bool IsCollapsible { get; protected set; }

        public virtual bool IsAvailable { get; }
    }
}


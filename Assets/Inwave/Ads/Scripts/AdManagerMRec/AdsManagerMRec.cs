#define DEBUG_ADS_INTERNAL
using Inwave.Production.Utils;
using System;
using UnityEngine;

namespace Inwave.Production.Ads
{
    //MRec
    public partial class AdsManager : MonoBehaviour
    {
#if DEBUG_ADS
        private const string LogTagAdsManagerMRec = "AdsManager MRec";
#endif
#if UNITY_IOS
        string mrecAdUnitId = "YOUR_IOS_AD_UNIT_ID";
#else // UNITY_ANDROID
        string mrecAdUnitId = "YOUR_ANDROID_AD_UNIT_ID";
#endif
        #region Properties

        [Header("+MRec------")]
        [SerializeField] 
        private bool _enableMRecAd = false;
        [SerializeField]
        private AdNetwork.MRecPosition _mRecPosition = AdNetwork.MRecPosition.BOTTOM_CENTERED;

        public bool EnableMRecAd=> _enableMRecAd;

        public AdNetwork.MRecPosition MRecPosition
        {
            get => _mRecPosition;
            set => _mRecPosition = value;
        }
        #endregion

        private void InitializeMRec()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagAdsManagerMRec, "InitializeMRec");
#endif
        }

//        public void LoadMRecAd()
//        {
//#if DEBUG_ADS && DEBUG_ADS_INTERNAL
//            LogUtils.LogWarning(LogTagAdsManagerMRec, "LoadMRecAd");
//#endif
//        }

        public void CreateMRecAd()
        {
            if (adn != null)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerMRec, "CreateMRecAd");
#endif
                adn.CreateMRecAd();
            }
        }

        public void ShowMRecAd()
        {
            if (adn != null)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerMRec, "ShowMRecAd");
#endif
                adn.ShowMRecAd();
            }
        }

        public void HideMRecAd()
        {
            if (adn != null)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerMRec, "HideMRecAd");
#endif
                adn.HideMRecAd();
            }
        }

        public void DestroyMRecAd()
        {
            if (adn != null)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerMRec, "DestroyMRecAd");
#endif
                adn.DestroyMRecAd();
            }
        }

        public void OnMRecAdLoadedEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.MRec.OnAdLoadedEvent(adInfo);
        }

        public void OnMRecAdLoadFailedEvent(AdNetwork.AdError adError)
        {
            AdsEvents.MRec.OnAdLoadFailedEvent(adError);
        }

        public void OnMRecAdClickedEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.MRec.OnAdClickedEvent(adInfo);
        }

        public void OnMRecAdExpandedEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.MRec.OnAdExpandedEvent(adInfo);
        }

        public void OnMRecAdCollapsedEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.MRec.OnAdCollapsedEvent(adInfo);
        }
    }
}


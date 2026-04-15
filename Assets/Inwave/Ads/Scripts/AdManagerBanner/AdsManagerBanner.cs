//#define DEBUG_ADS_INTERNAL
using System;
using Inwave.Production.Utils;
using UnityEngine;

namespace Inwave.Production.Ads
{
    //Banner
    public partial class AdsManager : MonoBehaviour
    {
#if DEBUG_ADS
        private const string LogTagAdsManagerBanner = "AdsManager Banner";
#endif
        [Header("+Banner------")]
        [SerializeField]
        private bool _enableBanner = true;
        [SerializeField]
        private AdNetwork.BannerSize _bannerSize = AdNetwork.BannerSize.SMART;

        [SerializeField]
        private AdNetwork.BannerPosition _bannerPosition = AdNetwork.BannerPosition.BOTTOM;

        public bool EnableBanner => _enableBanner;
        public AdNetwork.BannerSize BannerSize=> _bannerSize;

        public AdNetwork.BannerPosition BannerPosition
        {
            get => _bannerPosition;
            set
            {
                _bannerPosition = value;
                adn.SetBannerPosition(value);
            }
        }
        private void InitBanner()
        {
#if DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagAdsManagerBanner, "InitBanner");
#endif
            LoadBanner();
        }

        public void DelayLoadBanner()
        {
            //if (!Application.isEditor)
            //{
            //    _allowBanner = true;
            //}

            LoadBanner();
        }

        void LoadBanner()
        {
            if (_enableBanner)
            {
#if DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerBanner,"LoadBanner");
#endif
                adn.LoadBanner();
            }
        }

        public void HideBanner()
        {
            if (adn != null)
            {
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerBanner, "HideBanner");
#endif
                adn.HideBanner();
            }
        }

        public void DestroyBanner()
        {
            if (adn != null)
            {
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerBanner, "DestroyBanner");
#endif
                adn.DestroyBanner();
                adn.HideBanner();
            }
        }

        public void ShowBanner()
        {
            if (_enableBanner && adn != null)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerBanner, "ShowBanner");
#endif
                adn.ShowBanner();
            }
        }

        public bool IsBannerAvailable()
        {
            return _enableBanner && adn.BannerLoaded;
        }

        public void BannerAdLoadedEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.Banner.OnAdLoadedEvent(adInfo);
        }

        public void BannerAdLoadFailedEvent(AdNetwork.AdError adError)
        {
            AdsEvents.Banner.OnAdLoadFailedEvent(adError);
        }

        public void BannerAdClickedEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.Banner.OnAdClickedEvent(adInfo);
        }

        public void BannerAdScreenPresentedEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.Banner.OnAdScreenPresentedEvent(adInfo);
        }

        public void BannerAdScreenDismissedEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.Banner.OnAdScreenDismissedEvent(adInfo);
        }

        public void BannerAdLeftApplicationEvent()
        {

        }
    }
}


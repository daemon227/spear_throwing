#define DEBUG_ADS_INTERNAL
using System;
using System.Collections.Generic;
#if ADMOB
using GoogleMobileAds.Api;
#endif
using Inwave.Production.Utils;
using UnityEngine;
using static Inwave.Production.Ads.AdNetwork;

namespace Inwave.Production.Ads
{

    public class AdIronSource : AdNetwork
    {
#if DEBUG_ADS
        private const string LogTag = "AdIronSource";
#endif
        //IRON SOURCE
        [Header("API Keys:")]
        public string androidAppKey;
        public string iosAppKey;

        //[Header("Options:")] 
        //public bool AutoLoadRewardedAd = true;

        private bool _listenerAttached;

        private AdsManager _adsManager;

#if IRONSOURCE_ADS
        public override void Init()
        {
            _adsManager = AdsManager.Instance;
            if (!_listenerAttached)
            {
                IronSourceConfig.Instance.setClientSideCallbacks(true);
                IronSourceEvents.onImpressionDataReadyEvent += IronSourceEvents_onImpressionDataReadyEvent;

#if UNITY_ANDROID
                IronSource.Agent.init(androidAppKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL,
                    IronSourceAdUnits.BANNER);
#elif UNITY_IOS
                IronSource.Agent.init(iosAppKey, IronSourceAdUnits.REWARDED_VIDEO, IronSourceAdUnits.INTERSTITIAL, IronSourceAdUnits.BANNER);
#endif

                // Add Interstitial Events
                IronSourceInterstitialEvents.onAdReadyEvent += InterstitialAdLoadSucceedEvent;
                IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
                IronSourceInterstitialEvents.onAdClosedEvent += InterstitialAdClosedEvent;
                IronSourceInterstitialEvents.onAdClickedEvent += InterstitialAdClickedEvent;
                IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialAdShowFailedEvent;
                IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
                IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialAdOpenedEvent;

                //Rewarded Video Ad
                IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoAdClosedEvent;
                IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoAdRewardedEvent;
                IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoAdShowFailedEvent;
                IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoAdClickedEvent;
                IronSourceRewardedVideoEvents.onAdLoadFailedEvent += RewardedVideoAdLoadFailedEvent;
                IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoAdOpenedEvent;
                IronSourceRewardedVideoEvents.onAdAvailableEvent += OnRewardedVideoAdAvailableEvent;
                IronSourceRewardedVideoEvents.onAdUnavailableEvent += OnRewardedVideoAdUnavailableEvent;
                // Banner Events

                IronSourceBannerEvents.onAdLoadedEvent += BannerAdLoadedEvent;
                IronSourceBannerEvents.onAdLoadFailedEvent += BannerAdLoadFailedEvent;
                IronSourceBannerEvents.onAdClickedEvent += BannerAdClickedEvent;
                IronSourceBannerEvents.onAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
                IronSourceBannerEvents.onAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
                IronSourceBannerEvents.onAdLeftApplicationEvent += BannerAdLeftApplicationEvent;

                _listenerAttached = true;
                //IronSource.Agent.setManualLoadRewardedVideo(false);
                IronSourceEvents.onSegmentReceivedEvent += OnSegmentReceived;
            }
        }

        private void OnSegmentReceived(string obj)
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag,$"OnSegmentReceived: {obj}");
#endif
        }

        private void IronSourceEvents_onImpressionDataReadyEvent(IronSourceImpressionData impressionData)
        {
            if (impressionData != null && impressionData.revenue != null)
            {
                var adImpressionData = ISImpressionDataToImpressionData(impressionData);
                _adsManager.OnImpressionSuccessEvent(adImpressionData);
            }
        }

        public static AdImpressionData ISImpressionDataToImpressionData(IronSourceImpressionData impressionData)
        {
            AdImpressionData adImpressionData = new AdImpressionData
            {
                Platform = AdsValues.AD_PLATFORM_IRONSOURCE,
                Source = impressionData.adNetwork,
                UnitName = impressionData.instanceName,
                Format = impressionData.adUnit,
                Revenue = (double)impressionData.revenue,
                Currency = AdsValues.AD_REVENUE_CURRENCY
            };
            return adImpressionData;
        }
        public override void AppPause(bool pauseStatus)
        {
            IronSource.Agent.onApplicationPause(pauseStatus);
        }

        public override void SetSegmentCustomData(List<SegmentCustomData> segmentData)
        {
            IronSourceSegment segment = new IronSourceSegment();
            foreach (var data in segmentData)
            {
                segment.setCustom(data.Key, data.Value);
                OnSetCustomSegmentDataEvent(data.Key,data.Value);
            }
            
            IronSource.Agent.setSegment(segment);
        }

        public static AdInfo IsAdInfoToAdInfo(IronSourceAdInfo isAdInfo,string adFormat)
        {
            AdInfo adInfo = new AdInfo
            {
                Platform = AdsValues.AD_PLATFORM_IRONSOURCE,
                Format = adFormat,
                Mediation = isAdInfo.adNetwork,
                Revenue = (double)isAdInfo.revenue,
                UnitId = isAdInfo.adUnit
            };
            return adInfo;
        }

        public static AdError IsAdErrorToAdError(string unitId, IronSourceError isAdError)
        {
            AdError adError = new AdError
            {
                UnitId = unitId,
                Message = isAdError.getDescription(),
                Code = isAdError.getErrorCode()
            };
            return adError;
        }

        #region InterstitialAd

        private void InterstitialAdOpenedEvent(IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_INTERSTITIAL);
            _adsManager.InterstitialAdShowEvent(adInfo);
        }


        private void InterstitialAdShowSucceededEvent(IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_INTERSTITIAL);
            _adsManager.InterstitialAdShowSucceededEvent(adInfo);
        }

        public override bool IsInterstitialReady()
        {
            return IronSource.Agent.isInterstitialReady();
        }

        public override void LoadInterstitial()
        {
            IronSource.Agent.loadInterstitial();
        }

        public override void ShowInterstitial(AdInfo adInfo)
        {
            IronSource.Agent.showInterstitial();
            _adsManager.InterstitialAdShowEvent(adInfo);
        }

        private AdInfo _interstitialAdInfo;
        private void InterstitialAdLoadSucceedEvent(IronSourceAdInfo isAdInfo)
        {
            _interstitialAdInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_INTERSTITIAL);
            _adsManager.InterstitialAdLoadSucceedEvent(_interstitialAdInfo);
        }

        private void InterstitialAdClickedEvent(IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_INTERSTITIAL);
            _adsManager.InterstitialAdClickedEvent(adInfo);
        }

        private void InterstitialAdLoadFailedEvent(IronSourceError isAdError)
        {
            AdError adError = IsAdErrorToAdError(String.Empty, isAdError);
            _adsManager.InterstitialAdLoadFailedEvent(adError);
        }

        private void InterstitialAdShowFailedEvent(IronSourceError isAdError, IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_INTERSTITIAL);
            AdError adError = IsAdErrorToAdError(String.Empty, isAdError);
            _adsManager.InterstitialAdShowFailedEvent(adInfo,adError);
        }

        private void InterstitialAdClosedEvent(IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_INTERSTITIAL);
            _adsManager.InterstitialAdClosedEvent(adInfo);
        }

        #endregion

        #region RewardedVideoAd

        public override bool IsRewardedVideoAvailable()
        {
            return IronSource.Agent.isRewardedVideoAvailable();
        }

        public override void ShowRewardedVideo(string placement = null)
        {
            IronSource.Agent.showRewardedVideo(placement);
        }

        public override void LoadRewardedVideo(string placement = null)
        {
            IronSource.Agent.loadRewardedVideo();
        }

        private void OnRewardedVideoAdUnavailableEvent()
        {
            AdInfo adInfo = new AdInfo
            {
                Format = AdsValues.AD_FORMAT_REWARDED,
                Mediation = AdsValues.AD_MEDIATION_UNKNOWN,
                Platform = AdsValues.AD_PLATFORM_IRONSOURCE,
            };
            _adsManager.RewardedAdNotAvailableEvent(adInfo);
        }

        private AdInfo _rewardedAdInfo;
        private void OnRewardedVideoAdAvailableEvent(IronSourceAdInfo isAdInfo)
        {
            _rewardedAdInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_REWARDED);
            _adsManager.RewardedAdAvailableEvent(_rewardedAdInfo);
        }

        private void RewardedVideoAdOpenedEvent(IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_REWARDED);
            _adsManager.RewardedVideoAdOpenedEvent(adInfo);
        }
        //private void RewardedVideoAvailabilityChangedEvent(bool available)
        //{
        //    AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_REWARDED);
        //    _adsManager.RewardedVideoAvailabilityChangedEvent(available);
        //}
        private void RewardedVideoAdRewardedEvent(IronSourcePlacement ssp, IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_REWARDED);
            _adsManager.RewardedVideoAdRewardedEvent(adInfo);
        }

        private void RewardedVideoAdClosedEvent(IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_REWARDED);
            _adsManager.RewardedVideoAdClosedEvent(adInfo);
        }

        private void RewardedVideoAdEndedEvent()
        {
            //_adsManager.RewardedVideoAdEndedEvent();

        }

        private void RewardedVideoAdLoadFailedEvent(IronSourceError isAdError)
        {
            AdError adError = IsAdErrorToAdError(String.Empty, isAdError);
            _adsManager.RewardedVideoAdLoadFailedEvent(adError);
        }

        private void RewardedVideoAdShowFailedEvent(IronSourceError isAdError, IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_REWARDED);
            AdError adError = IsAdErrorToAdError(String.Empty, isAdError);
            _adsManager.RewardedVideoAdShowFailedEvent(adInfo,adError);
        }

        private void RewardedVideoAdClickedEvent(IronSourcePlacement ssp, IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_REWARDED);
            _adsManager.RewardedVideoClickedEvent(adInfo);
        }

        //private void RewardedVideoAdReadyEvent()
        //{
        //    _adsManager.RewardedVideoAdReadyEvent();
        //}

        #endregion

        #region BannerAd

        public override void LoadBanner(BannerSize size = BannerSize.SMART, BannerPosition position = BannerPosition.BOTTOM)
        {
            if (!BannerLoaded)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTag,$"LoadBanner - Size: {size}, Position: {position}");
#endif
                IronSourceBannerSize ironSourceBannerSize = GetBannerSize(size);
                IronSourceBannerPosition ironSourceBannerPosition = GetBannerPosition(position);

                IronSource.Agent.loadBanner(ironSourceBannerSize, ironSourceBannerPosition);
            }
        }

        private IronSourceBannerSize GetBannerSize(BannerSize size)
        {
            IronSourceBannerSize ironSourceBannerSize = IronSourceBannerSize.SMART;
            switch (size)
            {
                case BannerSize.BANNER:
                    ironSourceBannerSize = IronSourceBannerSize.BANNER;
                    break;
                case BannerSize.LARGE_BANNER:
                    ironSourceBannerSize = IronSourceBannerSize.LARGE;
                    break;
                case BannerSize.SMART:
                    break;
                default:
                    break;
            }

            return ironSourceBannerSize;
        }

        private IronSourceBannerPosition GetBannerPosition(BannerPosition position)
        {
            IronSourceBannerPosition ironSourceBannerPosition = IronSourceBannerPosition.BOTTOM;
            if (position == BannerPosition.TOP)
            {
                ironSourceBannerPosition = IronSourceBannerPosition.TOP;
            }

            return ironSourceBannerPosition;
        }
        public override void HideBanner()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "HideBanner");
#endif
            _BannerStatus = BannerStatus.HIDE;
            IronSource.Agent.hideBanner();
        }

        public override void ShowBanner()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, $"ShowBanner: {_BannerStatus}");
#endif
            if (_BannerStatus != BannerStatus.SHOWED)
            {
                if (!BannerLoaded)
                {
                    LoadBanner();
                }
                else
                {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                    LogUtils.LogWarning(LogTag,$"displayBanner: {IronSource.Agent}");
#endif
                    IronSource.Agent.displayBanner();
                }

                _BannerStatus = BannerStatus.SHOWED;
            }
        }

        public override void DestroyBanner()
        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTag, "DestroyBanner");
#endif
            _BannerStatus = BannerStatus.DESTROYED;
            BannerLoaded = false;
            IronSource.Agent.destroyBanner();
        }

        private void BannerAdLoadedEvent(IronSourceAdInfo isAdInfo)
        {
            BannerLoaded = true;
            if (_BannerStatus == BannerStatus.HIDE)
            {
                HideBanner();
            }
            else if (_BannerStatus == BannerStatus.DESTROYED)
            {
                DestroyBanner();
            }
            else
            {
                _BannerStatus = BannerStatus.SHOWED;
            }

            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_BANNER);
            _adsManager.BannerAdLoadedEvent(adInfo);
        }

        private void BannerAdLoadFailedEvent(IronSourceError isAdError)
        {
            BannerLoaded = false;
            AdError adError = IsAdErrorToAdError(string.Empty, isAdError);
            _adsManager.BannerAdLoadFailedEvent(adError);
        }

        private void BannerAdClickedEvent(IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_BANNER);
            _adsManager.BannerAdClickedEvent(adInfo);
        }

        private void BannerAdScreenPresentedEvent(IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_BANNER);
            _adsManager.BannerAdScreenPresentedEvent(adInfo);
        }

        private void BannerAdScreenDismissedEvent(IronSourceAdInfo isAdInfo)
        {
            AdInfo adInfo = IsAdInfoToAdInfo(isAdInfo, AdsValues.AD_FORMAT_BANNER);
            _adsManager.BannerAdScreenDismissedEvent(adInfo);
        }

        private void BannerAdLeftApplicationEvent(IronSourceAdInfo isAdInfo)
        {
            _adsManager.BannerAdLeftApplicationEvent();
        }

        #endregion

        #region MRec

        public override void CreateMRecAd()
        {
            if (_adsManager.EnableMRecAd &&!MRecLoaded)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTag, $"CreateMRecAd - Position: {_adsManager.MRecPosition}");
#endif
                IronSourceBannerPosition bannerPosition = IronSourceBannerPosition.TOP;
                if(_adsManager.MRecPosition == MRecPosition.BOTTOM_CENTERED)
                {
                    bannerPosition = IronSourceBannerPosition.BOTTOM;
                }
                IronSource.Agent.loadBanner(IronSourceBannerSize.RECTANGLE, bannerPosition);
            }
        }

        public override void ShowMRecAd()
        {
            if (_adsManager.EnableMRecAd)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTag, $"ShowMRecAd: {_MRecStatus}");
#endif
                if (_MRecStatus != MRecStatus.SHOWED)
                {
                    if (!MRecLoaded)
                    {
                        LoadBanner();
                    }
                    else
                    {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                        LogUtils.LogWarning(LogTag, $"displayMRec: {IronSource.Agent}");
#endif
                        IronSource.Agent.displayBanner();
                    }

                    _MRecStatus = MRecStatus.SHOWED;
                }
            }
        }

        public override void HideMRecAd()
        {
            if (_adsManager.EnableMRecAd)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTag, "HideMRecAd");
#endif
                _MRecStatus = MRecStatus.HIDE;
                IronSource.Agent.hideBanner();
            }
        }

        public override void DestroyMRecAd()
        {
            if(_adsManager.EnableMRecAd)
            {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTag, "DestroyMRecAd");
#endif
                _MRecStatus = MRecStatus.DESTROYED;
                MRecLoaded = false;
                IronSource.Agent.destroyBanner();
            }
        }

        #endregion
#endif
    }

}

//#define DEBUG_ADS_INTERNAL
using System;
using System.Collections;
using Inwave.Production.Utils;
using UnityEngine;

namespace Inwave.Production.Ads
{
    //Interstitial
    public partial class AdsManager : MonoBehaviour
    {
#if DEBUG_ADS
        private const string LogTagAdsManagerInter = "AdsManager Inter";
#endif
        [Header("+Interstitial------")]
        [SerializeField]
        private float _tryLoadInterstitialTime = 3;

        private string _interstitialLocation = "Init";
        private bool _tryLoadingInterstitial = false;
        private bool _isInterstitialAdShow = false;
        private bool _isCachingInterstitial = false;
        private float _nextInterstitialTime = 0;
        private WaitForSeconds _interstitialTime;

        #region Properties

        public float InterstitialDelay { get; set; } = 0;

        public float NextAdsBeforeFirstFSTime { get; set; } = 15;

        public float NextAdsBetweenFSTime { get; set; } = 30;

        public float NextAdsDefaultTime { get; set; } = 60;

        public bool IsInterstitialShowing => _isInterstitialAdShow;

        public float TryLoadInterstitialTime
        {
            set => _interstitialTime = new WaitForSeconds(value);
        }

        #endregion

        // opt==0: Ad_DelayBeforeFirstFS
        // opt==1: Ad_DelayBetweenFS
        // opt==2: Ad_DelayBetweenRVAndFS
        public void SetNextAdsTime(AdDelayOption opt)
        {
            switch (opt)
            {
                case AdDelayOption.BeforeFirstFS:
                    _nextInterstitialTime = Time.realtimeSinceStartup + NextAdsBeforeFirstFSTime;
                    break;

                case AdDelayOption.BetweenFS:
                    _nextInterstitialTime = Time.realtimeSinceStartup + NextAdsBetweenFSTime;
                    break;

                case AdDelayOption.InterstitialDelayConfig:
                    _nextInterstitialTime = Time.realtimeSinceStartup + InterstitialDelay;
                    break;

                default:
                    _nextInterstitialTime = Time.realtimeSinceStartup + NextAdsDefaultTime;
                    break;
            }
        }

        void InitInterstitial()
        {
            _interstitialTime = new WaitForSeconds(_tryLoadInterstitialTime);
            if (adn != null)
            {
                // InterstitialDelay = RemoteConfig.Instance.AdsConfig.Interstitial.Cooldown;
                LoadInterstitial(1);
            }
        }

        private Coroutine _loadInterstitialCoroutine = null;
        public void LoadInterstitial(float delay = 0)
        {
            if (!_isCachingInterstitial && !adn.IsInterstitialReady() &&_loadInterstitialCoroutine==null)
            {
                _loadInterstitialCoroutine = StartCoroutine(_LoadInterstitialAd(delay));
            }
        }

        private WaitForSeconds _waitLoadInterstitial = null;
        private int _maxTryLoadInterstitial = 3;
        private IEnumerator _LoadInterstitialAd(float delay = 0)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }

#if DEBUG_ADS&&DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagAdsManagerInter,"_LoadInterstitialAd - Start");
#endif

            AdsEvents.Interstitial.OnRequestLoadEvent();

            adn.LoadInterstitial();

            _isCachingInterstitial = true;
            int tryCounter = 0;
            _waitLoadInterstitial = new WaitForSeconds(15);

            while (tryCounter<_maxTryLoadInterstitial)
            {
                tryCounter++;
#if DEBUG_ADS&&DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagAdsManagerInter,$"_LoadInterstitialAd - Try: {tryCounter}");
#endif
                yield return _waitLoadInterstitial;
                if (adn.IsInterstitialReady())
                {
#if DEBUG_ADS&&DEBUG_ADS_INTERNAL
                    LogUtils.LogWarning(LogTagAdsManagerInter,"_LoadInterstitialAd - Ready");
#endif
                    break;
                }
                else
                {
                    void SendIntersAdLoadFailedEvent(string message)
                    {
                        AdNetwork.AdError adError = new AdNetwork.AdError()
                        {
                            Message = message
                        };

                        AdsEvents.Interstitial.OnLoadFailedEvent(adError);
                    }
                    if (!IsConnectedToInternet())
                    {
                        SendIntersAdLoadFailedEvent(AdsValues.AD_ERROR_DISCONNECTED);
                        break;
                    }
                    else
                    {
                        if (tryCounter == _maxTryLoadInterstitial)
                        {
                            SendIntersAdLoadFailedEvent(AdsValues.AD_ERROR_TWO_SLOW);
                        }
                    }
                }
            }
            CancelLoadInterstitial();
        }

        public bool RequestShowInterstitial(string location)
        {
            _isInterstitialAdShow = false;

            _interstitialLocation = location;
            AdsEvents.Interstitial.OnRequestShowEvent(location,_intersAdInfo);

            ShowInterstitial(location);
            return true;
        }

        private void ShowInterstitial(string location)
        {
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagAdsManagerInter,$"ShowInterstitial: {location}");
#endif

            bool isReady;
#if UNITY_EDITOR
            isReady = true;
#else
            isReady = adn.IsInterstitialReady();
#endif

            if (isReady)
            {
                AdsEvents.Interstitial.OnAdShowReadyEvent(location,_intersAdInfo);
                adn.ShowInterstitial(_intersAdInfo);
//#if UNITY_EDITOR
//                AdsEvents.Interstitial.OnShowEvent(location,_intersAdInfo);
//                InterstitialAdClosedEvent(_intersAdInfo);
//#endif
            }
            else
            {
                AnalyticsAds.FullAdsShowNotReady(location);
                AdsEvents.Interstitial.OnAdShowNotReadyEvent(location, _intersAdInfo);
                LoadInterstitial();
            }
        }

        #region event raisers
        internal AdNetwork.AdInfo _intersAdInfo;

        public void InterstitialAdLoadSucceedEvent(AdNetwork.AdInfo adInfo)
        {
            CancelLoadInterstitial();
            _intersAdInfo = adInfo;
            AdsEvents.Interstitial.OnReadyEvent(adInfo);
            AdsEvents.Interstitial.OnLoadSucceedEvent(_interstitialLocation, adInfo);
            AnalyticsAds.FullAdsShowReady(_interstitialLocation, _intersAdInfo.Mediation);
        }


        public void InterstitialAdShowEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.Interstitial.OnShowEvent(_interstitialLocation,adInfo);
        }

        public void InterstitialAdShowSucceededEvent(AdNetwork.AdInfo adInfo)
        {
            _lastTimeShowAds = Time.realtimeSinceStartup;
            _appOpenTimer = 0;
            _appOpenAdLastTime = Time.realtimeSinceStartupAsDouble;
            AdsEvents.Interstitial.OnShowSucceededEvent(_interstitialLocation,adInfo);
        }

        public bool IsInterstitialAvailable()
        {
            return adn.IsInterstitialReady();
        }

        public void InterstitialAdClickedEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.Interstitial.OnClickedEvent(_interstitialLocation,adInfo);
        }

        public void InterstitialAdLoadFailedEvent(AdNetwork.AdError adError)
        {
            CancelLoadInterstitial();
            AdsEvents.Interstitial.OnLoadFailedEvent(adError);

            //retry
            LoadInterstitial(_tryLoadInterstitialTime);
        }

        void CancelLoadInterstitial()
        {
            _isCachingInterstitial = false;
            if (_loadInterstitialCoroutine != null)
            {
                StopCoroutine(_loadInterstitialCoroutine);
                _loadInterstitialCoroutine = null;
            }
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL

            LogUtils.LogWarning(LogTagAdsManagerInter, $"LoadInterstitialAd - End");
#endif
        }

        public void InterstitialAdShowFailedEvent(AdNetwork.AdInfo adInfo, AdNetwork.AdError adError)
        {
            AdsEvents.Interstitial.OnShowFailedEvent(_interstitialLocation, adInfo, adError);
            //retry
            LoadInterstitial();
        }

        public void InterstitialAdClosedEvent(AdNetwork.AdInfo adInfo)
        {
            SetNextAdsTime(AdDelayOption.InterstitialDelayConfig);
            AdsEvents.Interstitial.OnClosedEvent(_interstitialLocation, adInfo);
            //caching
            LoadInterstitial();
        }
        #endregion
    }
}


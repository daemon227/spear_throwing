//#define DEBUG_ADS_INTERNAL
using System;
using System.Collections;
using Inwave.Production.Utils;
using UnityEngine;

namespace Inwave.Production.Ads
{
    public enum VIDEOREWARD
    {
        SKIPABLE,
        item,
        revive,
        currency,
        currency_x2,
        SKIPABLE_ENDGAME,
        LUCKY_SPIN,
        CHALLENGE,
    }

    //Rewarded Ad
    public partial class AdsManager : MonoBehaviour
    {
#if DEBUG_ADS
        private const string LogTagRewardedAd = "AdsManager RewardedAd";
#endif
        [Header("+Rewarded Ad------")]
        [SerializeField]
        private float _tryLoadRewardedTime = 1;
        [SerializeField]
        private VIDEOREWARD _rewardType = VIDEOREWARD.SKIPABLE;

        private string _rewardLocation;
        private bool _isCachingRewarded = false;
        private bool _isRewardedAdShowing = false;
        private bool _rewardReceived = false;
        private WaitForSeconds _rewardedTime;

        private Action<bool, string, AdNetwork.AdInfo> _onRewardCompleted;

        public bool IsRewardedAdShowing => _isRewardedAdShowing;

        public VIDEOREWARD RewardedType => _rewardType;

        public float TryLoadRewardedTime
        {
            set => _rewardedTime = new WaitForSeconds(value);
        }

        private void InitRewarded()
        {
            _rewardedTime = new WaitForSeconds(_tryLoadRewardedTime);
            LoadRewarded(1);
        }
        public bool IsRewardedAvailable()
        {
            return adn.IsRewardedVideoAvailable();
        }

        private Coroutine _loadRewardedCoroutine = null;
        public void LoadRewarded(float delay = 0, string location = "", string reward = "")
        {
            if (!_isCachingRewarded && !adn.IsRewardedVideoAvailable() && _loadRewardedCoroutine == null)
            {
                _loadRewardedCoroutine = StartCoroutine(_LoadRewarded(delay, location));
            }
        }
        private WaitForSeconds _waitLoadRewarded = null;
        private int _maxTryLoadRewarded = 3;
        private AdNetwork.AdInfo _rewardedAdInfo;
        IEnumerator _LoadRewarded(float delay = 0, string location = "")
        {
            yield return new WaitForSeconds(delay);
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagRewardedAd,$"_LoadRewarded:{location}");
#endif
            AdsEvents.Rewarded.OnRequest?.Invoke(_rewardLocation, _rewardedAdInfo);
            adn.LoadRewardedVideo(String.Empty);
            _rewardLocation = location;
            //_reward = reward;
            _isCachingRewarded = true;

            int tryCounter = 0;
            _waitLoadRewarded = new WaitForSeconds(15);

            while (tryCounter < _maxTryLoadRewarded)
            {
                tryCounter++;
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
                LogUtils.LogWarning(LogTagRewardedAd, $"_LoadRewarded - Try:{tryCounter}");
#endif
                yield return _waitLoadRewarded;
                if (adn.IsRewardedVideoAvailable())
                {
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
                    LogUtils.LogWarning(LogTagRewardedAd, "_LoadRewarded - Ready");
#endif
                    break;
                }
                else
                {
                    if (!IsConnectedToInternet())
                    {
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
                        LogUtils.LogWarning(LogTagRewardedAd, "_LoadRewarded - Disconnected");
#endif
                        AdNetwork.AdError adError = new AdNetwork.AdError
                        {
                            Message = AdsValues.AD_ERROR_DISCONNECTED
                        };
                        AdsEvents.Rewarded.OnRewardedAdLoadFailedEvent(adError);
                    }
                    else
                    {
                        if (tryCounter == _maxTryLoadRewarded)
                        {
#if DEBUG_ADS && DEBUG_ADS_INTERNAL
                            LogUtils.LogWarning(LogTagRewardedAd, "_LoadRewarded - Two slow!");
#endif
                            AdNetwork.AdError adError = new AdNetwork.AdError
                            {
                                Message = AdsValues.AD_ERROR_TWO_SLOW
                            };
                            AdsEvents.Rewarded.OnRewardedAdLoadFailedEvent(adError);
                        }
                    }
                }
            }
            CancelLoadRewarded();
        }

        void CancelLoadRewarded()
        {
            _isCachingRewarded = false;
            if (_loadRewardedCoroutine != null)
            {
                StopCoroutine(_loadRewardedCoroutine);
                _loadRewardedCoroutine = null;
            }
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagRewardedAd, $"LoadRewarded - End");
#endif
        }

        public void ShowRewardAds(Action<bool, string, AdNetwork.AdInfo> OnCompleted = null, string location = "", string reward = "")
        {
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
            LogUtils.LogWarning(LogTagRewardedAd, $"ShowRewardAds:{location},{reward}");
#endif
            _onRewardCompleted = OnCompleted;
            _rewardLocation = location;


            AdsEvents.Rewarded.OnRequestShowRewardAdEvent(_rewardLocation, _rewardedAdInfo);
            bool isAvailable = adn.IsRewardedVideoAvailable();
            if (isAvailable)
            {
                _isRewardedAdShowing = true;
                _rewardReceived = false;
                AdsEvents.Rewarded.OnRewardedAdShowReadyEvent(_rewardLocation, _rewardedAdInfo);
                //if (Placement == null)
                {
#if DEBUG_ADS &&DEBUG_ADS_INTERNAL
                    Debug.LogError("[AdsManager] - Placement is null. Please set a placement!");
                    
#endif
                    adn.ShowRewardedVideo(String.Empty);
                }
                //else
                //{
                //    adn.ShowRewardedVideo(Placement.GetValue(this));
                //}
            }
            else
            {
                AdsEvents.Rewarded.OnRewardedAdShowNotReadyEvent(_rewardLocation, _rewardedAdInfo);
                if (IsConnectedToInternet()) LoadRewarded();
            }
        }

        private void CompleteAdRewarded(AdNetwork.AdInfo adInfo, bool isSpecial = false)
        {
            _onRewardCompleted?.Invoke(true, _rewardLocation, adInfo);

        }

        #region Events Raiser

        public void RewardedVideoAdOpenedEvent(AdNetwork.AdInfo adInfo)
        {
            _lastTimeShowAds = Time.realtimeSinceStartup;
            AdsEvents.Rewarded.OnShowRewardAdEvent(_rewardLocation, adInfo);
        }

        public void RewardedVideoAvailabilityChangedEvent(bool available, AdNetwork.AdInfo adInfo)
        {
#if MAX_ADS
            _rewardedAdInfo = adInfo;
            if (available)
            {
                AdsEvents.Rewarded.OnRewardedAvailableEvent(adInfo);
                AdsEvents.Rewarded.OnRequestSuccess?.Invoke(_rewardLocation, adInfo);
            }
            else
            {
                AdsEvents.Rewarded.OnRewardedNotAvailableEvent(adInfo);
                AdsEvents.Rewarded.OnRequestFailed?.Invoke(_rewardLocation, adInfo);
            }
            CancelLoadRewarded();
#endif

        }

        public void RewardedVideoClickedEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.Rewarded.OnRewardedVideoAdClickedEvent(_rewardLocation, adInfo);
        }

#if IRONSOURCE_ADS
        /// <summary>
        /// This replace for 'RewardedVideoAvailabilityChangedEvent - true'
        /// </summary>
        public void RewardedAdAvailableEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.Rewarded.OnRewardedAvailableEvent(adInfo);
            CancelLoadRewarded();
        }

        /// <summary>
        /// This replace for 'RewardedVideoAvailabilityChangedEvent - false'
        /// </summary>
        public void RewardedAdNotAvailableEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.Rewarded.OnRewardedNotAvailableEvent(adInfo);
            CancelLoadRewarded();
        }
#endif

        public void RewardedVideoAdRewardedEvent(AdNetwork.AdInfo adInfo)
        {
            _rewardReceived = true;
            CompleteAdRewarded(adInfo);
            AdsEvents.Rewarded.OnRewardedVideoAdReceivedEvent(_rewardLocation, adInfo);
        }

        public void RewardedVideoAdClosedEvent(AdNetwork.AdInfo adInfo)
        {
            _isRewardedAdShowing = false;
            SetNextAdsTime(AdDelayOption.BetweenRVAndFS);
            if (!_rewardReceived)
            {
                _onRewardCompleted?.Invoke(false, _rewardLocation, _rewardedAdInfo);
            }
            AdsEvents.Rewarded.OnRewardedVideoAdClosedEvent(_rewardLocation, adInfo);
            LoadRewarded(0, _rewardLocation);


        }

        public void RewardedVideoAdSkippedEvent()
        {
            _onRewardCompleted?.Invoke(false, _rewardLocation, _rewardedAdInfo);
        }

        public void RewardedVideoAdShowFailedEvent(AdNetwork.AdInfo adInfo, AdNetwork.AdError adError)
        {
            _isRewardedAdShowing = false;
            _onRewardCompleted?.Invoke(false, _rewardLocation, adInfo);

            AdsEvents.Rewarded.OnRewardedVideoAdShowFailedEvent(_rewardLocation, adInfo, adError);
        }

        public void RewardedVideoAdLoadFailedEvent(AdNetwork.AdError adError)
        {
            AdsEvents.Rewarded.OnRewardedAdLoadFailedEvent(adError);
            CancelLoadRewarded();
        }

        public void RewardedVideoAdLoadSucceedEvent(AdNetwork.AdInfo adInfo)
        {
            AdsEvents.Rewarded.OnRewardedAdLoadSucceedEvent(adInfo);
        }

        #endregion
    }
}
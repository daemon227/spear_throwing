using System;
using System.Collections.Generic;
using UnityEngine;

namespace Inwave.Production.Ads
{
    public abstract class AdNetwork : MonoBehaviour
    {
        public enum BannerPosition
        {
            TOP,
            BOTTOM,
        }

        public enum BannerSize
        {
            BANNER,
            LARGE_BANNER,
            SMART
        }

        public enum BannerStatus
        {
            NONE,
            SHOWED,
            HIDE,
            DESTROYED
        }

        public enum MRecPosition
        {
            TOP_CENTER,
            //CENTERED,
            BOTTOM_CENTERED
        }
        public enum MRecStatus
        {
            NONE,
            SHOWED,
            HIDE,
            DESTROYED
        }
        public struct SegmentCustomData
        {
            public string Key;
            public string Value;
        }

        public struct AdImpressionData
        {
            public double Revenue;
            /// <summary>
            /// MAX(Applovin), IronSource
            /// </summary>
            public string Platform;
            /// <summary>
            /// Admob, Unity, Facebook ...
            /// </summary>
            public string Source;
            /// <summary>
            /// AdUnitIdentifier
            /// </summary>
            public string UnitName;
            /// <summary>
            /// Banner, Interstitial, Rewarded
            /// </summary>
            public string Format;
            /// <summary>
            /// USD, VND ...
            /// </summary>
            public string Currency;

            public string Placement;

            public override string ToString()
            {
                return
                    $"Platform: {Platform}, Source: {Source}, Format: {Format}, Unit: {UnitName}, Revenue: {Revenue} {Currency}";
            }
        }

        public struct AdInfo
        {
            public string Platform;

            public string Format;

            public string Mediation;

            public double Revenue;

            public string Placement;

            public string UnitId;

            public override string ToString()
            {
                return
                    $"Platform: {Platform}, Mediation: {Mediation}, Format: {Format}, Unit: {UnitId}, Revenue: {Revenue}";
            }
        }

        public struct AdError
        {
            public string UnitId;

            public int Code;

            public string Message;
            public override string ToString()
            {
                return
                    $"Unit: {UnitId}, Code: {Code}, Message: {Message}";
            }
        }


        #region Events

        public event Action<string,string> OnSetCustomSegmentData; 

        #endregion

        public virtual void Init()
        {

        }

        public virtual void SetSegmentCustomData(List<SegmentCustomData> segmentData)
        {

        }

        #region Rewarded
        public virtual bool IsRewardedVideoAvailable()
        {
            return false;
        }

        public virtual void ShowRewardedVideo(string placement = null)
        {
        }

        public virtual void LoadRewardedVideo(string placement = null)
        {

        }
        #endregion

        #region Interstitial
        public virtual bool IsInterstitialReady()
        {
            return false;
        }

        public virtual void LoadInterstitial()
        {

        }

        public virtual void ShowInterstitial(AdInfo adInfo)
        {

        }

        #endregion

        #region Banner
        [HideInInspector] public BannerStatus _BannerStatus = BannerStatus.NONE;
        [HideInInspector] public bool BannerLoaded = false;
        public virtual void LoadBanner(BannerSize size = BannerSize.SMART,BannerPosition position = BannerPosition.BOTTOM)
        {

        }

        public virtual void HideBanner()
        {

        }

        public virtual void DestroyBanner()
        {
        }

        public virtual void ShowBanner()
        {

        }

        public virtual void SetBannerPosition(BannerPosition position) { }
        #endregion

        #region MRec
        [HideInInspector] public MRecStatus _MRecStatus = MRecStatus.NONE;
        [HideInInspector] public bool MRecLoaded = false;
        public virtual void CreateMRecAd()
        {

        }

        public virtual void ShowMRecAd()
        {

        }

        public virtual void HideMRecAd()
        {

        }

        public virtual void DestroyMRecAd()
        {

        }

        #endregion

        public virtual void AppPause(bool pauseStatus)
        {

        }

        public void OnSetCustomSegmentDataEvent(string key,string value)
        {
            OnSetCustomSegmentData?.Invoke(key,value);
        }

        public virtual string GetAdPlatform()
        {
            return string.Empty;
        }
    }
}

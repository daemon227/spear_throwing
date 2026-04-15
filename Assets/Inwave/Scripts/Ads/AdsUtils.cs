using System;
using DongA.Events;
using Inwave.Production.Ads;
using UnityEngine;

public static class AdsUtils
{

    public static bool IsNoInternet => Application.internetReachability == NetworkReachability.NotReachable;

    public static void ShowReward(Action<bool, string, AdNetwork.AdInfo> onCompleted = null, string location = "", string reward = "")
    {
        GlobalValues.LocationAds = location;
        GlobalValues.RewardAds = reward;
        if (GamePrefs.IsAdsFree)
        {
            onCompleted?.Invoke(true, location, new AdNetwork.AdInfo());
            GamePrefs.TotalWatchedRewardAd ++;
            return;
        }

        // AnalyticsAds.VideoAdsShowReady(location, reward);

        if (IsNoInternet && !AdsManager.Instance.IsRewardedAvailable())
        {
            onCompleted?.Invoke(false, location, new AdNetwork.AdInfo());
            // GameEvents.ShowNoticeFloat?.Invoke("No internet!\nPlease check your network connection");
            // UIEvent.OnShowPopup(DongA.UI.PopupType.NoInternet);
            AnalyticsAds.VideoAdsShowNotReady(location, reward);
            return;
        }

        if (!AdsManager.Instance.IsRewardedAvailable())
        {
            onCompleted?.Invoke(true, location, new AdNetwork.AdInfo());
            AnalyticsAds.VideoAdsShowNotReady(location, reward);
            // GameEvents.ShowPopup?.Invoke(PopupId.PopupNotice, (go) =>
            // {
            //     go.GetComponent<PopupNotice>().Show("NOTICE", "Ad not ready");
            // });
        }

        AdsManager.Instance.ShowRewardAds(onCompleted, location, reward);
        GamePrefs.TotalWatchedRewardAd ++;
    }

    public static void ShowInterstitial(string location)
    {
        if (GamePrefs.IsNoAds || AdsManager.Instance == null) return;
        if (!RemoteConfig.Instance.AdsConfig.Interstitial.Enable) return;
        if (GamePrefs.Level < RemoteConfig.Instance.AdsConfig.Interstitial.Level) return;
        if (AnalyticManager.Instance.Data.DayLogin < RemoteConfig.Instance.AdsConfig.Interstitial.Day - 1) return;
        if (Time.realtimeSinceStartup < AdsManager.Instance.LastTimeShowAds + RemoteConfig.Instance.AdsConfig.Interstitial.Cooldown) return;

        //AdsManager.Instance.InterstitialDelay = RemoteConfig.Instance.AdsConfig.Interstitial.Cooldown;

        GlobalValues.LocationAds = location;
        // AnalyticsAds.FullAdsClick(location);
        AdsManager.Instance.RequestShowInterstitial(location);
        GamePrefs.TotalWatchedInterAd ++;
    }

    public static void ShowBanner()
    {
        if (GamePrefs.IsNoAds || AdsManager.Instance == null) return;
        if(GamePrefs.Level < RemoteConfig.Instance.AdsConfig.Banner.Level) return;
        if(AnalyticManager.Instance.Data.DayLogin < RemoteConfig.Instance.AdsConfig.Banner.Day - 1) return;
        if (!RemoteConfig.Instance.AdsConfig.Banner.Enable) return;

        AdsManager.Instance.ShowBanner();
    }
    public static void HideBanner()
    {
        if (AdsManager.Instance == null) return;
        AdsManager.Instance.HideBanner();
    }
    public static void ShowBanner(bool isInGame)
    {
        if (GamePrefs.IsNoAds || AdsManager.Instance == null) return;
        if (isInGame)
        {
            if (AdsManager.Instance.IsBannerCollapsibleAvailable)
            {
                //EventFirebase.BannerCollapShowReady();

                AdsManager.Instance.HideBanner();
                AdsManager.Instance.ShowBannerCollapsible(position);
            }
            else
            {
                //EventFirebase.BannerCollapShowNotReady();

                AdsManager.Instance.ShowBanner();
                AdsManager.Instance.LoadBannerCollapsible(position);
            }
        }
        else
        {
            AdsManager.Instance.HideBannerCollapsible();
            AdsManager.Instance.ShowBanner();
        }
    }

    public static void HideBanner(bool isCollapsible)
    {
        if (AdsManager.Instance == null) return;
        AdsManager.Instance.HideBannerCollapsible();
    }

    public static void LoadBannerCollapsible()
    {
        if (GamePrefs.IsNoAds || AdsManager.Instance == null) return;
        AdsManager.Instance.LoadBannerCollapsible(position);
    }

    public static void ShowAOA()
    {
        if (GamePrefs.IsNoAds || AdsManager.Instance == null) return;
        if(GamePrefs.Level < RemoteConfig.Instance.AdsConfig.AOA.Level) return;
        if(AnalyticManager.Instance.Data.DayLogin < RemoteConfig.Instance.AdsConfig.AOA.Day - 1) return;
        if (!RemoteConfig.Instance.AdsConfig.AOA.Enable) return;
        
        AdsManager.Instance.Show();
    }

    private static BannerCollapsibleNetwork.BannerCollapsiblePosition position =
        BannerCollapsibleNetwork.BannerCollapsiblePosition.BOTTOM;

    public static void ShowMRecAd(string location)
    {
        if (GamePrefs.IsNoAds || AdsManager.Instance == null) return;
        if(GamePrefs.Level < RemoteConfig.Instance.AdsConfig.Mrec.Level) return;
        if(AnalyticManager.Instance.Data.DayLogin < RemoteConfig.Instance.AdsConfig.Mrec.Day - 1) return;
        if (!RemoteConfig.Instance.AdsConfig.Mrec.Enable) return;

        Debug.Log("[AdsUtils] ShowMRecAd at location: " + location);
        GlobalValues.LocationAds = location;
        AdsManager.Instance.ShowMRecAd();
        // AdsEvents.MRec.OnAdShowed();
        AnalyticsAds.MRecAdsShow(location);
    }
    public static void HideMRecAd()
    {
        if (AdsManager.Instance == null) return;
        Debug.Log("[AdsUtils] HideMRecAd");
        AdsManager.Instance.HideMRecAd();
        //AnalyticsAds.MRecAdsShow(location);
    }
}

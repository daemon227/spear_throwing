using System.Collections.Generic;
using AppsFlyerSDK;
using Inwave.Production.Ads;
using UnityEngine;

public class AnalyticsAds: AnalyticsBase
{
    public static string Location => GlobalValues.LocationAds;
    public static string Reward => GlobalValues.RewardAds;

    public static int GetValue(string keyRequest)
    { 

        var countRequest = PlayerPrefs.GetInt(keyRequest, 0);

        countRequest++;

        PlayerPrefs.SetInt(keyRequest, countRequest);

        return countRequest;
    }

    public static void AdImpressionAdmobCollapsibleBanner(AdNetwork.AdImpressionData impressionData)
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            {ParameterName.value, impressionData.Revenue},
            {ParameterName.currency, impressionData.Currency},
            {ParameterName.ad_format, impressionData.Format},
            {ParameterName.ad_unit_name, impressionData.UnitName},
            {ParameterName.ad_source, impressionData.Source},
            {ParameterName.ad_source_id, impressionData.Source},
            {ParameterName.ad_platform, impressionData.Platform},
            {ParameterName.location, Location},
        };
        AnalyticHelper.LogEvent(EventName.ad_impression_collap, param);
    }
    public static void AdImpression(AdNetwork.AdImpressionData impressionData)
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            {ParameterName.value, impressionData.Revenue},
            {ParameterName.currency, impressionData.Currency},
            {ParameterName.ad_format, impressionData.Format},
            {ParameterName.ad_unit_name, impressionData.UnitName},
            {ParameterName.ad_source, impressionData.Source},
            {ParameterName.ad_source_id, impressionData.Source},
            {ParameterName.ad_platform, impressionData.Platform},
            {ParameterName.location, Location},
        };
        AnalyticHelper.LogEvent(EventName.ad_impression, param);
    }

    #region Ads Inter

    public static void FullAdsRequest()
    {
        var accumulatedCount = GetValue(EventName.fullads_request);

        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            // { ParameterName.location, Location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            // { ParameterName.mediation, ParameterValue.all },
        };
        AnalyticHelper.LogEvent(EventName.fullads_request, param);
    }

    public static void FullAdsRequestFailed()
    {
        var accumulatedCount = GetValue(EventName.fullads_request_failed);

        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            // { ParameterName.location, Location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            // { ParameterName.mediation, ParameterValue.all },
        };
        AnalyticHelper.LogEvent(EventName.fullads_request_failed, param);

    }
    public static void FullAdsRequestSuccess(string location, string mediation)
    {
        var accumulatedCount = GetValue(EventName.fullads_request_success);

        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            // { ParameterName.location, location },
            { ParameterName.accumulated_count, $"{accumulatedCount}"},
            // { ParameterName.mediation, mediation }
        };
        AnalyticHelper.LogEvent(EventName.fullads_request_success, param);
        AppsFlyer.sendEvent(EventName.af_inter_successfullyloaded,null);

    }
    public static void FullAdsShowReady(string location, string mediation)
    {
        var accumulatedCount = GetValue(EventName.fullads_show_ready);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            { ParameterName.mediation, mediation }
        };
        AnalyticHelper.LogEvent(EventName.fullads_show_ready, param);
        AppsFlyer.sendEvent(EventName.af_inter_logicgame,null);

    }
    public static void FullAdsShowNotReady(string location)
    {
        var accumulatedCount = GetValue(EventName.fullads_show_notready);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, location },
            { ParameterName.accumulated_count, $"{accumulatedCount}"},
            { ParameterName.mediation, ParameterValue.all },
        };
        AnalyticHelper.LogEvent(EventName.fullads_show_notready, param);
    }
    public static void FullAdsShow(string location, string mediation)
    { 
        var accumulatedCount = GetValue(EventName.fullads_show);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, location },
            { ParameterName.accumulated_count, $"{accumulatedCount}"},
            { ParameterName.mediation, mediation }

        };
        AnalyticHelper.LogEvent(EventName.fullads_show, param);
        AnalyticsFTUE.LogEventCountMax(EventName.FTUE_inter_ads_, accumulatedCount, 3);
    }

    public static void FullAdsShowFailed(string location, string mediation)
    { 
        var accumulatedCount = GetValue(EventName.fullads_show_failed);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            { ParameterName.mediation, mediation }
        };
        AnalyticHelper.LogEvent(EventName.fullads_show_failed, param);
    }
    public static void FullAdsShowFinish(string location, string mediation)
    { 
        var accumulatedCount = GetValue(EventName.fullads_finish);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, location },
            { ParameterName.accumulated_count, $"{accumulatedCount}"},
            { ParameterName.mediation, mediation }
        };
        AnalyticHelper.LogEvent(EventName.fullads_finish, param);
        AppsFlyer.sendEvent(EventName.af_inter_displayed,null);
    }
    public static void FullAdsClick(string location)
    { 
        var accumulatedCount = GetValue(EventName.fullads_click);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, location },
            { ParameterName.accumulated_count,$"{accumulatedCount}"},
            { ParameterName.mediation, ParameterValue.applovin }
        };
        AnalyticHelper.LogEvent(EventName.fullads_click, param);
    }
    public static void FullAdsClickCTA(string location, string mediation)
    {
        var accumulatedCount = GetValue(EventName.fullads_click_cta);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, location },
            { ParameterName.accumulated_count,$"{accumulatedCount}"},
            { ParameterName.mediation, mediation }
        };
        AnalyticHelper.LogEvent(EventName.fullads_click, param);
    }

    #endregion
    #region Ads Reward

    public static void VideoAdsShowReady(string location, string mediation)
    { 
        var accumulatedCount = GetValue(EventName.videoads_show_ready);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, Location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            { ParameterName.mediation, mediation },
            { ParameterName.reward, Reward }
        };
        AnalyticHelper.LogEvent(EventName.videoads_show_ready, param);
        AppsFlyer.sendEvent(EventName.af_rewarded_logicgame,null);

    }
    public static void VideoAdsShowNotReady(string location, string reward)
    { 
        var accumulatedCount = GetValue(EventName.videoads_show_notready);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, Location },
            { ParameterName.accumulated_count, $"{accumulatedCount}"},
            { ParameterName.mediation, "none" },
            { ParameterName.reward, Reward }
        };
        AnalyticHelper.LogEvent(EventName.videoads_show_notready, param);

    }

    public static void VideoAdsShow(string location, string mediation)
    { 
        var accumulatedCount = GetValue(EventName.videoads_show);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, Location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            { ParameterName.mediation, mediation },
            { ParameterName.reward, Reward }
        };
        AnalyticHelper.LogEvent(EventName.videoads_show, param);
        AnalyticsFTUE.RewardAds(accumulatedCount);
    }
    public static void VideoAdsShowFailed(string location, string mediation)
    { 
        var accumulatedCount = GetValue(EventName.videoads_show_failed);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, Location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            { ParameterName.mediation, mediation },
            { ParameterName.reward, Reward }
        };
        AnalyticHelper.LogEvent(EventName.videoads_show_failed, param);
    }
    public static void VideoAdsFinish(string location, string mediation)
    { 
        var accumulatedCount = GetValue(EventName.videoads_finish);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, Location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            { ParameterName.mediation, mediation },
            { ParameterName.reward, Reward }
        };
        
        AnalyticHelper.LogEvent(EventName.videoads_finish, param);
        AppsFlyer.sendEvent(EventName.af_rewarded_displayed,null);
        AdReward();
    }

    public static void AdReward()
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.location, Location }
        };
        AnalyticHelper.LogEvent(EventName.ad_reward, param);
    }

    public static void VideoAdsClick(string location, string mediation)
    { 
        var accumulatedCount = GetValue(EventName.videoads_click);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, Location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            { ParameterName.mediation, mediation },
            { ParameterName.reward, Reward }
        };
        AnalyticHelper.LogEvent(EventName.videoads_click, param);
    }

    public static void VideoAdsRequest(string location, string mediation)
    { 
        var accumulatedCount = GetValue(EventName.videoads_request);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, Location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            { ParameterName.mediation, mediation },
            { ParameterName.reward, Reward }
        };
        AnalyticHelper.LogEvent(EventName.videoads_request, param);
    }
    public static void VideoAdsRequestSuccess(string location, string mediation)
    { 
        var accumulatedCount = GetValue(EventName.videoads_success);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, Location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            { ParameterName.mediation, mediation },
            { ParameterName.reward, Reward }
        };
        AnalyticHelper.LogEvent(EventName.videoads_success, param);
        AppsFlyer.sendEvent(EventName.af_rewarded_successfullyloaded,null);
    }
    public static void VideoAdsRequestFailed(string location, string mediation)
    { 
        var accumulatedCount = GetValue(EventName.videoads_fail);
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, Location },
            { ParameterName.accumulated_count, $"{accumulatedCount}" },
            { ParameterName.mediation, mediation },
            { ParameterName.reward, Reward }
        };
        AnalyticHelper.LogEvent(EventName.videoads_fail, param);
    }

    public static void BannerAdsClick()
    {
        AnalyticHelper.LogEvent(EventName.bannerads_click);
    }

    public static void BannerAdsShow()
    {
        AnalyticHelper.LogEvent(EventName.bannerads_show);
    }

    #endregion

    #region Ads Banner Collapsible

    public static void BannerCollapRequest()
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState }
        };
        AnalyticHelper.LogEvent(EventName.banner_collap_request, param);
    }

    public static void BannerCollapRequestSuccess()
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
        };
        AnalyticHelper.LogEvent(EventName.banner_collap_request_success, param);
    }

    public static void BannerCollapRequestFailed()
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState }
        };
        AnalyticHelper.LogEvent(EventName.banner_collap_request_failed, param);
    }

    public static void BannerCollapShow(bool isCollapsible)
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.type, BannerCollapStringType(isCollapsible) }
        };
        AnalyticHelper.LogEvent(EventName.banner_collap_show, param);
#if DEBUG_ADS
        Debug.LogError(EventName.banner_collap_show);
#endif
    }

    public static void BannerCollapClick(bool isCollapsible)
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            { ParameterName.location, Location },
            { ParameterName.type, BannerCollapStringType(isCollapsible) }
        };
        AnalyticHelper.LogEvent(EventName.banner_collap_click, param);
#if DEBUG_ADS
        Debug.LogError(EventName.banner_collap_click);
#endif
    }

    public static void BannerCollapImpressionRecorded(AdNetwork.AdInfo adInfo, bool isCollapsible)
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState }, 
            { ParameterName.type, BannerCollapStringType(isCollapsible) }
        };
        AnalyticHelper.LogEvent(EventName.banner_collap_impression_recorded, param);
    }

    public static void BannerCollapPaid(AdNetwork.AdImpressionData impressionData, bool isCollapsible)
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
            //{ ParameterName.Mode, GameModeToString(DataManager.CurrentMode) },
            { ParameterName.type, BannerCollapStringType(isCollapsible) }
        };
        AnalyticHelper.LogEvent(EventName.banner_collap_paid, param);
    }

    public static void BannerCollapStartShow()
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.connection, UpdateNetWorkState },
        };
        AnalyticHelper.LogEvent(EventName.banner_collap_start_show, param);
    }

    private static string BannerCollapStringType(bool isCollapsible)
    {
        if (isCollapsible)
        {
            return "Collap";
        }

        return "Normal";
    }

    public static void AdPrompt()
    {
        if (AdsManager.Instance.IsRewardedAvailable())
            AnalyticHelper.LogEvent(EventName.ad_prompt);
    }
    public static void SpendRewardItem()
    {
        AnalyticHelper.LogEvent(EventName.spend_reward_item);
    }

    #endregion

    #region  MREC ADS
    public static void MRecAdsShow(string location)
    {
        Dictionary<string, object> param = new Dictionary<string, object>
            {
                { ParameterName.location, location }, 
            };
        AnalyticHelper.LogEvent(EventName.mrec_ads_show, param);
    }
    public static void MRecAdsClick(string location)
    {
        Dictionary<string, object> param = new Dictionary<string, object>
        {
            { ParameterName.location, location }, 
        };
        AnalyticHelper.LogEvent(EventName.mrec_ads_click, param);
    }

    #endregion
    public static string UpdateNetWorkState => Application.internetReachability == NetworkReachability.NotReachable ? ParameterValue.offline : ParameterValue.online;
}

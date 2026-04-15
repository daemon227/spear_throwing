using UnityEngine;

public struct EventName
{
    #region Product
    public static string level_start = "level_start";
    public static string level_fail = "level_fail";
    public static string level_win = "level_win";
    public static string level_replay = "level_replay";
    public static string level_retry = "level_retry";
    public static string level_revive = "level_revive";
    public static string booster_receive = "booster_{0}_receive";
    public static string booster_spend = "booster_{0}_spend";
    public static string add_more_time = "add_more_time";
    public static string move_merge = "move_merge";
    public static string solved_5_puzzle = "solved_5_puzzle";
    public static string level_no_space = "level_no_space";
    public static string buy_skin = "buy_skin";
    public static string buy_spear = "buy_spear";
    public static string buy_trail = "buy_trail";

    #endregion

    #region FTUE
    public static string FTUE1_loading_start = "FTUE1_loading_start";
    public static string FTUE1_loading_end = "FTUE1_loading_end";
    public static string FTUE_user_return_D = "FTUE_user_return_D{0}";
    public static string FTUE_user_return_S2 = "FTUE_user_return_S2";
    public static string FTUE_user_return_S3 = "FTUE_user_return_S3";
    public static string FTUE_user_return_S4 = "FTUE_user_return_S4";
    public static string FTUE_user_return_S5 = "FTUE_user_return_S5";
    public static string FTUE_user_playtime_mins = "FTUE_user_playtime_{0}mins";
    public static string FTUE_inter_ads_ = "FTUE_inter_ads_{0}";
    public static string FTUE_reward_ads_ = "FTUE_reward_ads_{0}";
    public static string FTUE_level_click_X = "FTUE_level_click_{0}";
    public static string FTUE_level_X_start = "FTUE_level_{0}_start";
    public static string FTUE_level_X_complete = "FTUE_level_{0}_complete";
    public static string FTUE_level_X_fail = "FTUE_level_{0}_fail";
    public static string FTUE_x2_claim = "FTUE_x2_claim";
    public static string FTUE_load_config = "FTUE_load_config";
    public static string FTUE_load_config_failed = "FTUE_load_config_failed";
    public static string FTUE_load_config_success = "FTUE_load_config_success";
    #endregion

    #region Resources
    public static string resource_income = "resource_income";
    public static string resource_outcome = "resource_outcome";
    #endregion

    #region ADS
    public static string fullads_request = "fullads_request";
    public static string fullads_request_failed = "fullads_request_failed";
    public static string fullads_request_success = "fullads_request_success";
    public static string fullads_click = "fullads_click";
    public static string fullads_show_ready = "fullads_show_ready";
    public static string fullads_show_notready = "fullads_show_notready";
    public static string fullads_show = "fullads_show";
    public static string fullads_show_failed = "fullads_show_failed";
    public static string fullads_finish = "fullads_finish";
    public static string fullads_click_cta = "fullads_click_cta";

    public static string videoads_request = "videoads_request";
    public static string videoads_success = "videoads_success";
    public static string videoads_fail = "videoads_fail";
    public static string videoads_click = "videoads_click";
    public static string videoads_show_ready = "videoads_show_ready";
    public static string videoads_show_notready = "videoads_show_notready";
    public static string videoads_show = "videoads_show";
    public static string videoads_show_failed = "videoads_show_failed";
    public static string videoads_finish = "videoads_finish";
    public static string videoads_click_cta = "videoads_click_cta";

    public static string bannerads_click = "bannerads_click";
    public static string bannerads_show = "bannerads_show";

    public static string fullads_show_x = "fullads_show_{0}"; //0-10
    public static string fullads_finish_x = "fullads_finish_{0}"; //0-10
    public static string videoads_show_x = "videoads_show_{0}"; //0-10
    public static string videoads_finish_x = "videoads_finish_{0}"; //0-10
    public static string totalads_show_x = "totalads_show_{0}"; //0-20
    public static string totalads_finish_x = "totalads_finish_{0}"; //0-20

    public static string ad_prompt = "ad_prompt";
    public static string ad_impression_rewarded = "ad_impression_rewarded";
    public static string ad_reward = "ad_reward";
    public static string spend_reward_item = "spend_reward_item"; // tru currency ra con lai ban het

    public static string banner_collap_request = "banner_collap_request";
    public static string banner_collap_request_success = "banner_collap_request_success";
    public static string banner_collap_request_failed = "banner_collap_request_failed";
    public static string banner_collap_start_show = "banner_collap_start_show";
    public static string banner_collap_show = "banner_collap_show";
    public static string banner_collap_click = "banner_collap_click";
    public static string banner_collap_show_ready = "banner_collap_show_ready";
    public static string banner_collap_show_notready = "banner_collap_show_notready";
    public static string banner_collap_impression_recorded = "banner_collap_impression_recorded";
    public static string banner_collap_paid = "banner_collap_paid";

    public static string ad_impression_collap = "ad_impression_collap";
    public static string ad_impression = "ad_impression";

    public static string mrec_ads_show = "mrec_ads_show";
    public static string mrec_ads_click = "mrec_ads_click";
    #endregion

    #region AppsFlyer
    public static string af_session = "af_session";
    public static string af_tutorial_completion = "af_tutorial_completion";
    public static string af_inter_logicgame = "af_inter_logicgame";
    public static string af_inter_successfullyloaded = "af_inter_successfullyloaded";
    public static string af_inter_displayed = "af_inter_displayed";
    public static string af_rewarded_logicgame = "af_rewarded_logicgame";
    public static string af_rewarded_successfullyloaded = "af_rewarded_successfullyloaded";
    public static string af_rewarded_displayed = "af_rewarded_displayed";
    #endregion

    #region Notification
    public static string notification_open_app = "notification_open_app";
    #endregion

    #region Remove Ads
    public static string iap_appear = "iap_apper";
    public static string iap_click = "iap_click";
    public static string iap_ui_pay = "iap_ui_pay";
    public static string iap_paying = "iap_paying";
    public static string iap_success = "iap_success";
     public static string iap_failed = "iap_failed";
    #endregion
}

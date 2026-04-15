using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnalyticsProduct : AnalyticsBase
{
    public static float TimeStartLevel;
    public static string PlayTime => Mathf.RoundToInt(Time.time - TimeStartLevel).ToString();
    public static string GameCountByLevel => AnalyticManager.Instance.Data.GameCountByLevel.ToString();
    public static string LocationStartLevel => GlobalValues.LocationStartLevel;
    public static string MoveTime => Mathf.RoundToInt(GlobalValues.MoveTime).ToString();
    public static string TimeLimit => Mathf.RoundToInt(GlobalValues.PlayTimeRemain).ToString();
    // public static string TotalSort => GlobalValues.CurrentLevelStage.TotalSort.ToString();
    // public static string SortCount => GlobalValues.CurrentLevelStage.SortCount.ToString();
    public static string MoveCount => GlobalValues.MoveCount.ToString();
    public static string Total_Shot => GlobalValues.Total_Shot.ToString();
    public static string Coins_Collected => GlobalValues.Coins_Collected.ToString();
    public static string FailManner => GlobalValues.FailManner;

    public static string LastPlayTime
    {
        get => PlayerPrefs.GetString(KeyPrefs.LastPlayTime, null);
        set => PlayerPrefs.SetString(KeyPrefs.LastPlayTime, value);
    }

    public static string LastTimeLimit
    {
        get => PlayerPrefs.GetString(KeyPrefs.LastTimeLimit, null);
        set => PlayerPrefs.SetString(KeyPrefs.LastTimeLimit, value);
    }

    public static string LastSortCount
    {
        get => PlayerPrefs.GetString(KeyPrefs.LastSortCount, null);
        set => PlayerPrefs.SetString(KeyPrefs.LastSortCount, value);
    }

    public static string LastFailManner
    {
        get => PlayerPrefs.GetString(KeyPrefs.LastFailManner, null);
        set => PlayerPrefs.SetString(KeyPrefs.LastFailManner, value);
    }


    public static void LevelStart()
    {
        TimeStartLevel = Time.time;

        var param = new Dictionary<string, object>(){
            { ParameterName.level, Level},
            { ParameterName.game_count, GameCountByLevel},
        };
        AnalyticHelper.LogEvent(EventName.level_start, param);

        if (AnalyticManager.Instance.Data.GameCountByLevel >= 2)
        {
            LevelRetry();
        }

        LastPlayTime = null;
        LastFailManner = null;
    }
    public static void LevelInterrupted()
    {
        LastPlayTime = PlayTime;
        LastTimeLimit = TimeLimit;
        // LastSortCount = SortCount;
        LastFailManner = FailManner;
    }
    public static void LevelNoSpace()
    {
        var param = new Dictionary<string, object>(){
            { ParameterName.level, Level},
            { ParameterName.game_count, GameCountByLevel},
            { ParameterName.playtime, PlayTime},
            { ParameterName.location, LocationStartLevel},
            { ParameterName.time_limit, TimeLimit},
            // { ParameterName.sort_count, SortCount},
            { ParameterName.manner, ParameterValue.no_space},
            { ParameterName.move_count, MoveCount},
            { ParameterName.move_time, MoveTime},
        };
        AnalyticHelper.LogEvent(EventName.level_no_space, param);
    }
    public static void LevelFail()
    {
        LastPlayTime = PlayTime;
        // LastTimeLimit = TimeLimit;
        // LastSortCount = SortCount;
        // LastFailManner = FailManner;
        var param = new Dictionary<string, object>(){
            { ParameterName.level, Level},
            { ParameterName.game_count, GameCountByLevel},
            { ParameterName.playtime, PlayTime},
            { ParameterName.total_shot, Total_Shot}
        };
        AnalyticHelper.LogEvent(EventName.level_fail, param);
    }
    public static void LevelWin()
    {
        var param = new Dictionary<string, object>(){
            { ParameterName.level, Level},
            { ParameterName.game_count, GameCountByLevel},
            { ParameterName.playtime, PlayTime},
            { ParameterName.total_shot, Total_Shot},
            {ParameterName.coins_collected, Coins_Collected}
            // { ParameterName.sort_count, SortCount},
        };
        AnalyticHelper.LogEvent(EventName.level_win, param);
    }
    public static void LevelReplay()
    {
        var param = new Dictionary<string, object>(){
            { ParameterName.level, Level},
            { ParameterName.game_count, GameCountByLevel},
            { ParameterName.playtime, PlayTime},
            { ParameterName.location, LocationStartLevel},
        };
        AnalyticHelper.LogEvent(EventName.level_replay, param);
    }
    public static void LevelRetry()
    {
        var param = new Dictionary<string, object>(){
            { ParameterName.level, Level},
            { ParameterName.game_count, GameCountByLevel},
            // { ParameterName.manner, LastFailManner},
        };
        AnalyticHelper.LogEvent(EventName.level_retry, param);
    }
    public static void LevelRevive()
    {
        var param = new Dictionary<string, object>(){
            { ParameterName.level, Level},
            { ParameterName.game_count, GameCountByLevel},
            { ParameterName.playtime, PlayTime},
            { ParameterName.time_limit, TimeLimit},
            // { ParameterName.sort_count, SortCount},
            { ParameterName.manner, FailManner},
        };
        AnalyticHelper.LogEvent(EventName.level_revive, param);
    }
    public static void AddMoreTime(string manner, int time_added)
    {
        var param = new Dictionary<string, object>(){
            { ParameterName.level, Level},
            { ParameterName.game_count, GameCountByLevel},
            { ParameterName.playtime, PlayTime},
            { ParameterName.location, LocationStartLevel},
            { ParameterName.manner, manner},
            { ParameterName.time_added, time_added.ToString()},
        };
        AnalyticHelper.LogEvent(EventName.add_more_time, param);
    }
    public static void BoosterReceive(string name, string location, string manner, int count)
    {
        var param = new Dictionary<string, object>(){
            { ParameterName.count, count.ToString()},
            { ParameterName.location, location},
            { ParameterName.manner, manner},
        };
        AnalyticHelper.LogEvent(string.Format(EventName.booster_receive, name), param);
    }
    public static void BoosterSpend(string name)
    {
        var param = new Dictionary<string, object>(){
            { ParameterName.level, Level},
            { ParameterName.game_count, GameCountByLevel},
            { ParameterName.playtime, PlayTime},
            { ParameterName.time_limit, TimeLimit},
            { ParameterName.type, name},
            { ParameterName.location, LocationStartLevel},
            // { ParameterName.sort_count, SortCount},
        };
        AnalyticHelper.LogEvent(string.Format(EventName.booster_spend, name), param);
    }
    
    public static void SolvedPuzzle()
    {
        var param = new Dictionary<string, object>(){
            { ParameterName.level, Level},
            { ParameterName.move_count, MoveCount},
            { ParameterName.playtime, PlayTime},
            { ParameterName.game_count, GameCountByLevel},
            // { ParameterName.sort_count, SortCount},
        };
        AnalyticHelper.LogEvent(EventName.solved_5_puzzle, param);
    }
    public static void BuySkin(string skinName, string manner)
    {
        var param = new Dictionary<string, object>()
        {
            { ParameterName.level, Level},
            { ParameterName.skin_name, skinName},
            { ParameterName.manner, manner},
        };
        AnalyticHelper.LogEvent(EventName.buy_skin, param);
    }
    public static void BuySpear(string skinName, string manner)
    {
        var param = new Dictionary<string, object>()
        {
            { ParameterName.level, Level},
            { ParameterName.skin_name, skinName},
            { ParameterName.manner, manner},
        };
        AnalyticHelper.LogEvent(EventName.buy_spear, param);
    }
    public static void BuyTrail(string skinName, string manner)
    {
        var param = new Dictionary<string, object>()
        {
            { ParameterName.level, Level},
            { ParameterName.skin_name, skinName},
            { ParameterName.manner, manner},
        };
        AnalyticHelper.LogEvent(EventName.buy_trail, param);
    }
}

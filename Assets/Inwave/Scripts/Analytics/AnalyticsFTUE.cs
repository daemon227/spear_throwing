using System.Collections.Generic;
using UnityEngine;

public class AnalyticsFTUE : AnalyticsBase
{
    public static void LogEvent(string eventName, Dictionary<string, object> param = null)
    {
        if (PlayerPrefs.HasKey(eventName)) return;

        PlayerPrefs.SetInt(eventName, 1);

        AnalyticHelper.LogEvent(eventName, param);
    }

    public static void LogEventCountMax(string eventName, int count, int max)
    {
        if (count <= max)
        {
            LogEvent(string.Format(eventName, count));
        }
    }

    public static void RewardAds(int count)
    {
        if (count <= 3)
        {
            var eventName = string.Format(EventName.FTUE_reward_ads_, count);
            var param = new Dictionary<string, object>()
            {
                { ParameterName.location, GlobalValues.LocationAds }
            };
            LogEvent(string.Format(eventName, count), param);
        }
    }

    public static void PlayGameDay(int day)
    {
        switch (day)
        {
            case 1:
                LogEvent(string.Format(EventName.FTUE_user_return_D, day));
                break;
            case 2:
                LogEvent(string.Format(EventName.FTUE_user_return_D, day));
                break;
            case 3:
                LogEvent(string.Format(EventName.FTUE_user_return_D, day));
                break;
            case 7:
                LogEvent(string.Format(EventName.FTUE_user_return_D, day));
                break;
        }
    }

    public static void PlayGameSession(int session)
    {
        switch (session)
        {
            case 2:
                LogEvent(EventName.FTUE_user_return_S2);
                break;
            case 3:
                LogEvent(EventName.FTUE_user_return_S3);
                break;
            case 4:
                LogEvent(EventName.FTUE_user_return_S4);
                break;
            case 5:
                LogEvent(EventName.FTUE_user_return_S5);
                break;
        }
    }

    public static void LevelAction(string eventName, int level)
    {
        if (level <= 10)
        {
            LogEvent(string.Format(eventName, level));
        }
    }
}

using UnityEngine;

public class AnalyticsBase
{
    public static string Level => (GamePrefs.Level + 1).ToString();
}

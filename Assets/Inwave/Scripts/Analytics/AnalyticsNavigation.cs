using System.Collections.Generic;
using UnityEngine;

public class AnalyticsNavigation : AnalyticsBase
{
    private static Dictionary<string, float> _times = new Dictionary<string, float>();

    public static void Button(string ui_name, string button_name)
    {
        if (!_times.ContainsKey(button_name)) _times.Add(button_name, 0f);

        if (Time.time - _times[button_name] < 3f) return;

        var param = new Dictionary<string, object>(){
            { ParameterName.ui_name, ui_name},
            { ParameterName.btn_name, button_name},
            { ParameterName.level, (GamePrefs.Level+1).ToString()}
        };
        AnalyticHelper.LogEvent($"bc_{button_name}", param);
    }

    public static void Popup(string ui_name, string screen_name)
    {
        if (!_times.ContainsKey(ui_name)) _times.Add(ui_name, 0f);

        if (Time.time - _times[ui_name] < 1f) return;

        var param = new Dictionary<string, object>(){
            { ParameterName.ui_name, ui_name},
            { ParameterName.screen_name, screen_name}
        };
        AnalyticHelper.LogEvent($"pa_{ui_name}", param);
    }
    // public static void ButtonShop(string button_name)
    // {
    //     if (!_times.ContainsKey(button_name)) _times.Add(button_name, 0f);

    //     if (Time.time - _times[button_name] < 3f) return;

    //     var param = new Dictionary<string, object>(){
    //         { ParameterName.level, (GamePrefs.Level+1).ToString()}
    //     };
    //     AnalyticHelper.LogEvent($"bc_{button_name}", param);
    // }
    
}

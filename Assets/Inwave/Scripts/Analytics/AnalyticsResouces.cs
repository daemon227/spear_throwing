using System.Collections.Generic;
using UnityEngine;

public class AnalyticsResouces : AnalyticsBase
{
    public static void InCome(string location, string manner, ResourceType resource_type, string resource_name, int resource_quantity)
    {
        if (string.IsNullOrEmpty(resource_name)) resource_name = resource_type.ToString();
        var param = new Dictionary<string, object>(){
            { ParameterName.location, location},
            { ParameterName.manner, manner},
            { ParameterName.resource_type, resource_type.ToString()},
            { ParameterName.resource_name, resource_name},
            { ParameterName.resource_value, resource_quantity}
        };
        AnalyticHelper.LogEvent(EventName.resource_income, param);
    }
    public static void OutCome(string location, string reward, ResourceType resource_type, int resource_quantity)
    {
        var param = new Dictionary<string, object>(){
            { ParameterName.location, location},
            { ParameterName.reward, reward},
            { ParameterName.resource_type, resource_type.ToString()},
            { ParameterName.resource_value, resource_quantity}
        };
        AnalyticHelper.LogEvent(EventName.resource_outcome, param);
    }
}

public enum ResourceType
{
    Coin,
    Skin,
    Powerup,
    Spear,
    Trail
}

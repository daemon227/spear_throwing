using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFirebase : MonoBehaviour
{
    //[Command("Log")]
    public void LogMessage(string message)
    {
        Dictionary<string, object> param = new Dictionary<string, object>();
        param.Add("message",message);
        AnalyticHelper.LogEvent("Log",param);
    }

   // [Command("DeviceInfo")]
    public void LogDeviceInfo()
    {
        AnalyticHelper.FireDeviceDetail();
    }

    //[Command("Event")]
    public void FireEvent(string eventName, string message)
    {
        Dictionary<string, object> param = new Dictionary<string, object>();
        param.Add("msg",message);
        AnalyticHelper.LogEvent(eventName,param);
    }
}

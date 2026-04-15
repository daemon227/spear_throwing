using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnalyticIAP : AnalyticsBase
{
    public static void IAPAppear(string source, string ui_name)
    {
        var param = new Dictionary<string, object>()
        {
            { ParameterName.source, source },
            { ParameterName.ui_name, ui_name }
        };
        AnalyticHelper.LogEvent(EventName.iap_appear, param);
    }

    public static void IAPClick(string source, string ui_name, string button_name, float price, string currency, string product_name)
    {
        var param = new Dictionary<string, object>()
        {
            { ParameterName.source, source },
            { ParameterName.ui_name, ui_name },
            { ParameterName.btn_name, button_name },
            { ParameterName.price, price },
            { ParameterName.currency, currency },
            { ParameterName.product_name, product_name }
        };
        AnalyticHelper.LogEvent(EventName.iap_click, param);
    }

    public static void IAPUiPay(string source, string ui_name, float price, string currency, string product_name)
    {
        var param = new Dictionary<string, object>()
        {
            { ParameterName.source, source },
            { ParameterName.ui_name, ui_name },
            { ParameterName.price, price },
            { ParameterName.currency, currency },
            { ParameterName.product_name, product_name }
        };
        AnalyticHelper.LogEvent(EventName.iap_ui_pay, param);
    }

    public static void IAPPaying(string source, string ui_name, float price, string currency, string product_name)
    {
        var param = new Dictionary<string, object>()
        {
            { ParameterName.source, source },
            { ParameterName.ui_name, ui_name },
            { ParameterName.price, price },
            { ParameterName.currency, currency },
            { ParameterName.product_name, product_name }
        };
        AnalyticHelper.LogEvent(EventName.iap_paying, param);
    }
    public static void IAPSuccess(string source, string ui_name, float price, string currency, int reward_quantity,string product_name, string status)
    {
        var param = new Dictionary<string, object>()
        {
            { ParameterName.source, source },
            { ParameterName.ui_name, ui_name },
            { ParameterName.price, price },
            { ParameterName.currency, currency },
            { ParameterName.reward_quantity, reward_quantity },
            { ParameterName.product_name, product_name },
            { ParameterName.status, status }

        };
        AnalyticHelper.LogEvent(EventName.iap_success, param);
    }
    public static void IAPFailed(string source, string ui_name, float price, string currency ,string product_name, string status)
    {
        var param = new Dictionary<string, object>()
        {
            { ParameterName.source, source },
            { ParameterName.ui_name, ui_name },
            { ParameterName.price, price },
            { ParameterName.currency, currency },
            { ParameterName.product_name, product_name },
            { ParameterName.status, status }

        };
        AnalyticHelper.LogEvent(EventName.iap_failed, param);
    }
}

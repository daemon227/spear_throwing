using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class RemoveAdConfig 
{
    public bool Enable;
    public int FirstSaleInter;
    public int FirstSaleDay;
    public int LastSaleInter;
    public int LastSaleDay;
    public int Duration;

    public bool RemoveInter;
    public bool RemoveBanner;
    public bool RemoveMREC;
    public bool RemoveAOA;

    public int BonusCoin;
    public bool EnableRemind;
    public int CountDownRemind;
}

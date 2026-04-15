using System;
using UnityEngine;

[Serializable]
public class AdsConfig
{
    public BannerConfig Banner;
    public InterstitialConfig Interstitial;
    public RewardConfig Reward;
    public MRECConfig Mrec;
    public AOAConfig AOA;
}

[Serializable]
public class BannerConfig
{
    public bool Enable;
    public int Day;
    public int Level;
}

[Serializable]
public class InterstitialConfig
{
    public bool Enable;
    public int Day;
    public int Level;
    public int Cooldown;
}
[Serializable]
public class RewardConfig
{
    public bool Enable;
    public int Day;
    public int Level;
}
[Serializable]
public class MRECConfig
{
    public bool Enable;
    public int Day;
    public int Level;
}
[Serializable]
public class AOAConfig
{
    public bool Enable;
    public int Day;
    public int Level;
}

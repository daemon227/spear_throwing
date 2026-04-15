using FoodMaster;
using UnityEngine;

public partial class RemoteConfig
{
    [Header("Game")]
    public bool AdminTool = true;
    public AdsConfig AdsConfig = new AdsConfig
    {
        Banner = new BannerConfig { Enable = true, Day = 0, Level = 0 },
        Interstitial = new InterstitialConfig { Enable = true, Day = 0, Level = 9, Cooldown = 15 },
        Reward = new RewardConfig{Enable = true, Day = 0, Level = 0},
        Mrec = new MRECConfig {Enable = true, Day = 0, Level = 0},
        AOA = new AOAConfig { Enable = true, Day = 0, Level = 9}
    };
    public RemoveAdConfig RemoveAdConfig = new RemoveAdConfig
    {
        Enable = true,
        FirstSaleInter = 20,
        FirstSaleDay = 3,
        LastSaleInter = 50,
        LastSaleDay  = 7,
        Duration = 0,
        RemoveInter = true,
        RemoveBanner = true,
        RemoveMREC = true,
        RemoveAOA = true,
        BonusCoin = 5000,
        EnableRemind = true,
        CountDownRemind = 30
    };

    [Header("Shop")]
    public int[] CharacterSkinPrices = new int[] { 300, 500};
    public int[] SpearSkinPrices = new int[] { 2000, 5000 };
    public int[] TrailSkinPrices = new int[] { 500, 800};
    public int CoinToShowShopTutorial = 300;

    public void InitConfigValues()
    {
        
    }

    [Header("Coin")]
    public int CoinPerCoin = 5;
    public int CoinClaimWinLevel = 80;
    public int Coin30 = 200;
    public int Coin60 = 350;
    public int Coin100 = 500;

    // Level
    public int LevelUnlockOffer = 5;

    [Header("Notification")]
    // Week-hour schedule (0=Mon 0h, 25=Tue 1h, 167=Sun 23h)
    // Default: Mon-Fri 12h, Sat-Sun 10h+12h
    public int[] NotificationSchedule = new int[] { 12, 36, 60, 84, 108, 130, 132, 154, 156 };
    public int NotificationMaxDaily = 2;
}

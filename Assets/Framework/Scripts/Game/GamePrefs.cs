// using GameBooster;
using System.Collections.Generic;
using UnityEngine;

public partial class GamePrefs
{
    /// <summary>
    /// Format số coin: >= 1000 hiển thị dạng 1.03k, ngược lại hiển thị nguyên.
    /// </summary>
    public static string FormatCoin(int amount)
    {
        if (amount < 1000) return amount.ToString();
        float k = amount / 1000f;
        return $"{k:0.##}k";
    }

    public static int Theme
    {
        get => PlayerPrefs.GetInt(KeyPrefs.Theme, 0);
        set => PlayerPrefs.SetInt(KeyPrefs.Theme, value);
    }

    // public static int Gold
    // {
    //     get => PlayerPrefs.GetInt(KeyPrefs.Gold, 0);
    //     set => PlayerPrefs.SetInt(KeyPrefs.Gold, value);
    // }

    public static int GoldWin
    {
        get => PlayerPrefs.GetInt(KeyPrefs.GoldWin, 0);
        set => PlayerPrefs.SetInt(KeyPrefs.GoldWin, value);
    }


    public static bool IsIAPFree
    {
        get => PlayerPrefs.GetInt(KeyPrefs.IsIAPFree, 0) == 1;
        set => PlayerPrefs.SetInt(KeyPrefs.IsIAPFree, value ? 1 : 0);
    }

    public static bool IsAdsFree
    {
        get => PlayerPrefs.GetInt(KeyPrefs.IsAdsFree, 0) == 1;
        set => PlayerPrefs.SetInt(KeyPrefs.IsAdsFree, value ? 1 : 0);
    }

    public static bool MKTCollectItem
    {
        get => PlayerPrefs.GetInt(KeyPrefs.MKTCollectItem, 0) == 1;
        set => PlayerPrefs.SetInt(KeyPrefs.MKTCollectItem, value ? 1 : 0);
    }


    public static bool IsNoAds
    {
        get => PlayerPrefs.GetInt(KeyPrefs.IsNoAds, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(KeyPrefs.IsNoAds, value ? 1 : 0);
            if (!value) AdsUtils.HideBanner();
            GameEvents.OnNoAdsChanged?.Invoke(value);
        }
    }

    public static bool IsCachedPopupWin
    {
        get => PlayerPrefs.GetInt(KeyPrefs.IsCachedPopupWin, 0) == 1;
        set => PlayerPrefs.SetInt(KeyPrefs.IsCachedPopupWin, value ? 1 : 0);
    }

    public static int UnlockedElement
    {
        get => PlayerPrefs.GetInt(KeyPrefs.UnlockedElement, 0);
        set => PlayerPrefs.SetInt(KeyPrefs.UnlockedElement, value);
    }

    public static bool MusicEnable
    {
        get => PlayerPrefs.GetInt(KeyPrefs.MusicEnable, 1) == 1;
        set
        {
            GlobalValues.MusicEnable = value;
            PlayerPrefs.SetInt(KeyPrefs.MusicEnable, value ? 1 : 0);
            GameEvents.OnMusicStateChanged?.Invoke(value);
        }
    }

    public static bool SoundEnable
    {
        get => PlayerPrefs.GetInt(KeyPrefs.SoundEnable, 1) == 1;
        set
        {
            GlobalValues.SoundEnable = value;
            PlayerPrefs.SetInt(KeyPrefs.SoundEnable, value ? 1 : 0);
            GameEvents.OnSoundStateChanged?.Invoke(value);
        }
    }

    public static bool HapticEnable
    {
        get => PlayerPrefs.GetInt(KeyPrefs.HapticEnable, 1) == 1;
        set
        {
            GlobalValues.HapticEnable = value;
            PlayerPrefs.SetInt(KeyPrefs.HapticEnable, value ? 1 : 0);
            GameEvents.OnHapticStateChanged?.Invoke(value);
        }
    }

    public static int Level
    {
        get => PlayerPrefs.GetInt(KeyPrefs.Level, 0);
        set
        {
            PlayerPrefs.SetInt(KeyPrefs.Level, value);
        }
    }

    public static bool IsGamePaused = false;
    public static bool IsIceFrozen = false;
    public static bool IsUIBlocked = false;
    public static bool IsShopTutorialPanelActive = false;
    public static int Coin
    {
        get { return PlayerPrefs.GetInt(KeyPrefs.Coin, 100);}
        set 
        { 
            PlayerPrefs.SetInt(KeyPrefs.Coin, value);
            PlayerPrefs.Save();
            GameEvents.OnCoinUpdated?.Invoke();
            if(AnalyticManager.Instance == null) return;
            AnalyticManager.Instance.OnCoinUpdated(value);
        }
    }

    public static HashSet<int> GetCollectedCoins(int level)
    {
        string data = PlayerPrefs.GetString($"CollectedCoins_{level}", "");
        var result = new HashSet<int>();
        if (string.IsNullOrEmpty(data)) return result;
        foreach (var s in data.Split(','))
            if (int.TryParse(s, out int id))
                result.Add(id);
        return result;
    }

    public static void MarkCoinCollected(int level, int coinId)
    {
        var set = GetCollectedCoins(level);
        set.Add(coinId);
        PlayerPrefs.SetString($"CollectedCoins_{level}", string.Join(",", set));
        PlayerPrefs.Save();
    }

    public static void ClearCollectedCoins(int level)
    {
        PlayerPrefs.DeleteKey($"CollectedCoins_{level}");
        PlayerPrefs.Save();
    }

    public static string CurrentCharacterSkinID
    {
        get {return PlayerPrefs.GetString(KeyPrefs.CurrentCharacterSkin, "Normal");}
        set {PlayerPrefs.SetString(KeyPrefs.CurrentCharacterSkin, value);}
    }
    public static string CurrentCharacterSkinName
    {
        get {return PlayerPrefs.GetString(KeyPrefs.CurrentCharacterSkinName, "Normal");}
        set {PlayerPrefs.SetString(KeyPrefs.CurrentCharacterSkinName, value);}
    }
    public static string CurrentSpearSkinID
    {
        get {return PlayerPrefs.GetString(KeyPrefs.CurrentSpearSkin, "Normal");}
        set {PlayerPrefs.SetString(KeyPrefs.CurrentSpearSkin, value);}
    }
    public static string CurrentSpearSkinName
    {
        get {return PlayerPrefs.GetString(KeyPrefs.CurrentSpearSkinName, "Normal");}
        set {PlayerPrefs.SetString(KeyPrefs.CurrentSpearSkinName, value);}
    }
    public static int CurrentSpearType
    {
        get => PlayerPrefs.GetInt(KeyPrefs.CurrentSpearType, 1);
        set => PlayerPrefs.SetInt(KeyPrefs.CurrentSpearType, value);
    }
    public static string CurrentTrailSkinID
    {
        get {return PlayerPrefs.GetString(KeyPrefs.CurrentTrailSkin, "Normal");}
        set {PlayerPrefs.SetString(KeyPrefs.CurrentTrailSkin, value);}
    }
    public static string CurrentTrailSkinName
    {
        get {return PlayerPrefs.GetString(KeyPrefs.CurrentTrailSkinName, "Normal");}
        set {PlayerPrefs.SetString(KeyPrefs.CurrentTrailSkinName, value);}
    }

    // Owned Skins
    public static bool IsCharacterSkinOwned(string skinName)
    {
        if (skinName == "Normal") return true;
        string raw = PlayerPrefs.GetString(KeyPrefs.OwnedCharacterSkins, "");
        return ("," + raw + ",").Contains("," + skinName + ",");
    }
    public static void SetCharacterSkinOwned(string skinName)
    {
        if (IsCharacterSkinOwned(skinName)) return;
        string raw = PlayerPrefs.GetString(KeyPrefs.OwnedCharacterSkins, "");
        PlayerPrefs.SetString(KeyPrefs.OwnedCharacterSkins, raw.Length > 0 ? raw + "," + skinName : skinName);
        PlayerPrefs.Save();
    }

    public static bool IsSpearSkinOwned(string skinName)
    {
        if (skinName == "Normal") return true;
        string raw = PlayerPrefs.GetString(KeyPrefs.OwnedSpearSkins, "");
        return ("," + raw + ",").Contains("," + skinName + ",");
    }
    public static void SetSpearSkinOwned(string skinName)
    {
        if (IsSpearSkinOwned(skinName)) return;
        string raw = PlayerPrefs.GetString(KeyPrefs.OwnedSpearSkins, "");
        PlayerPrefs.SetString(KeyPrefs.OwnedSpearSkins, raw.Length > 0 ? raw + "," + skinName : skinName);
        PlayerPrefs.Save();
    }

    public static bool IsTrailSkinOwned(string skinName)
    {
        if (skinName == "Normal") return true;
        string raw = PlayerPrefs.GetString(KeyPrefs.OwnedTrailSkins, "");
        return ("," + raw + ",").Contains("," + skinName + ",");
    }
    public static void SetTrailSkinOwned(string skinName)
    {
        if (IsTrailSkinOwned(skinName)) return;
        string raw = PlayerPrefs.GetString(KeyPrefs.OwnedTrailSkins, "");
        PlayerPrefs.SetString(KeyPrefs.OwnedTrailSkins, raw.Length > 0 ? raw + "," + skinName : skinName);
        PlayerPrefs.Save();
    }

    public static int ThemeID
    {
        get => PlayerPrefs.GetInt(KeyPrefs.ThemeID, 0);
        set => PlayerPrefs.SetInt(KeyPrefs.ThemeID, value);
    }

    public static int TotalWatchedInterAd
    {
        get =>PlayerPrefs.GetInt(KeyPrefs.TotalWatchedInterAds, 0);
        set 
        {
            PlayerPrefs.SetInt(KeyPrefs.TotalWatchedInterAds, value);
            if(AnalyticManager.Instance != null)
            {
                AnalyticManager.OnTotalWatchedInterAds?.Invoke(value);
            }
        }
    }
    public static int TotalWatchedRewardAd
    {
        get => PlayerPrefs.GetInt(KeyPrefs.TotalWatchedRewardAds, 0);
        set 
        {
            PlayerPrefs.SetInt(KeyPrefs.TotalWatchedRewardAds, value);
            if(AnalyticManager.Instance != null)
            {
                AnalyticManager.OnTotalWatchedRewardAds?.Invoke(value);
            }
        }
    }
    public static int OutOfLevel
    {
        get => PlayerPrefs.GetInt(KeyPrefs.OutOfLevel, 0);
        set => PlayerPrefs.GetInt(KeyPrefs.OutOfLevel, value);
    }
}
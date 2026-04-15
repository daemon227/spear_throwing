
using Firebase.Analytics;
using FoodMaster;
using System;
using UnityEngine;

[Serializable]
public class AnalyticData
{
    public int LastLevel;
    public int LevelMax;
    // public int TotalHeart;
    public int TotalBackGame;
    public int ValueBackGame;
    public int SessionBack;
    public int TotalCoinOutCome;
    public int TotalCoinInCome;
    public int IapCount;
    public float IapRev;
    public int TotalWatchedInterAds  = 0;
    public int TotalWatchedRewardAds = 0;
    public int Day;
    public int DayLogin;
    public int GameCount;
    public int GameCountByLevel;
    public float PlayTime;
    public float PlayTimeAds;
    public string CurrentSkin => GamePrefs.CurrentCharacterSkinName;
    public string CurrentSpear => GamePrefs.CurrentSpearSkinName;
    public string CurrentTrail => GamePrefs.CurrentTrailSkinName;

    public int GetPlayTime => (int)PlayTime;

    public void NewData()
    {
        LastLevel = 0;
        LevelMax = 0;
        // TotalHeart = 5;
        TotalCoinOutCome = 0;
        TotalCoinInCome = 0;
        IapCount = 0;
        IapRev = 0;
        TotalWatchedInterAds = 0;
        TotalWatchedRewardAds = 0;
        Day = 0;
        DayLogin = 0;
        PlayTime = 0;
        PlayTimeAds = 0;
        GameCount = 0;
        GameCountByLevel = 0;
    }
}

public class AnalyticManager : Singleton<AnalyticManager>
{
    public static Action OnIapCount;
    public static Action<int> OnTotalWatchedInterAds;
    public static Action<int> OnTotalWatchedRewardAds;
    public static Action<float> OnUpdatePlayTimeAds;

    public AnalyticData Data;

    private int _currentCoin;
    private int _playtimeCache = 0;
    private int _playtimeAdsCache = 0;
    private int _lastFTUEMins = 0;
    private float _timeStartLevel = 0f;

    private static DateTime startDate;
    private static DateTime today;
    private const string KeySave = "data_analytic";
    private bool _isLoad = false;
    private bool _hasFireNoSpace;

    public float TimeStartLevel
    {
        get => _timeStartLevel;
        set => _timeStartLevel = value;
    }

    protected override void Awake()
    {
        base.Awake();

        Init();

        OnIapCount += UpdateIapCount;
        OnTotalWatchedInterAds += UpdateTotalWatchedInterAds;
        OnTotalWatchedRewardAds += UpdateTotalWatchedRewardAds;
        OnUpdatePlayTimeAds += UpdatePlayTimeAds;

        GameEvents.OnGameStart += OnGameStart;
        GameEvents.OnGameFinish += OnGameFinish;
        GameEvents.OnGameInterrupted += OnGameInterrupted;
        GameEvents.OnSortCountUpdated += OnSortCountUpdated;
        // GameEvents.OnLevelShelfUnlocked += OnLevelShelfUnlocked;
        GameEvents.OnHintNoSpace += OnHintNoSpace;

        GameEvents.OnCharacterSkinChosen += OnCharacterSkinChanged;
        GameEvents.OnSpearSkinChosen += OnSpearSkinChanged;
        GameEvents.OnTrailSkinChosen += OnTrailSkinChanged;
    }

    protected void OnDestroy()
    {
        OnIapCount -= UpdateIapCount;
        OnTotalWatchedInterAds -= UpdateTotalWatchedInterAds;
        OnTotalWatchedRewardAds -= UpdateTotalWatchedRewardAds;
        OnUpdatePlayTimeAds -= UpdatePlayTimeAds;

        GameEvents.OnGameStart -= OnGameStart;
        GameEvents.OnGameFinish -= OnGameFinish;
        GameEvents.OnGameInterrupted -= OnGameInterrupted;
        GameEvents.OnSortCountUpdated -= OnSortCountUpdated;
        // GameEvents.OnLevelShelfUnlocked -= OnLevelShelfUnlocked;
        GameEvents.OnHintNoSpace -= OnHintNoSpace;

        GameEvents.OnCharacterSkinChosen -= OnCharacterSkinChanged;
        GameEvents.OnSpearSkinChosen -= OnSpearSkinChanged;
        GameEvents.OnTrailSkinChosen -= OnTrailSkinChanged;
    }

    private void Start()
    {
        LogUserProperties();
        AnalyticsFTUE.PlayGameDay(Data.DayLogin);
        AnalyticsFTUE.PlayGameSession(Data.SessionBack);

        _isLoad = true;
    }

    private void Init()
    {
        if (PlayerPrefs.HasKey(KeySave))
        {
            var jsonData = PlayerPrefs.GetString(KeySave);

            Data = JsonUtility.FromJson<AnalyticData>(jsonData);
        }
        else
        {
            Data = new AnalyticData();
            Data.NewData();
        }

        SetStartDate();
        GetDay();

        Data.SessionBack++;

        _currentCoin = GamePrefs.Coin;

        // TotalWatchedRewardAds = GamePrefs.TotalWatchedRewardAd;
        // TotalWatchedInterAds = GamePrefs.TotalWatchedInterAd;
    }

    public void OnCoinUpdated(int value)
    {
        int changedAmount = value - _currentCoin;
        if (changedAmount > 0)
        {
            Data.TotalCoinInCome += changedAmount;
            UpdateUserProperty(UserProperty.total_cash_income, Data.TotalCoinInCome);
        }
        else if (changedAmount < 0)
        {
            Data.TotalCoinOutCome -= changedAmount;
            UpdateUserProperty(UserProperty.total_cash_outcome, Data.TotalCoinOutCome);
        }
        _currentCoin = value;
    }

    // private void OnLifeUpdated(int value)
    // {
    //     Data.TotalHeart = value;
    //     UpdateUserProperty(UserProperty.current_heart, Data.TotalHeart);
    // }

    private void OnSortCountUpdated()
    {
        _hasFireNoSpace = false;
        // if (GlobalValues.CurrentLevelStage.SortCount % 5 == 0)
        // {
        //     AnalyticsProduct.SolvedPuzzle();
        // }
    }

    // private void OnLevelShelfUnlocked(LevelShelf shelf)
    // {
    //     _hasFireNoSpace = false;
    // }

    private void OnHintNoSpace()
    {
        if (!_hasFireNoSpace)
        {
            _hasFireNoSpace = true;
            AnalyticsProduct.LevelNoSpace();
        }
    }

    private void OnGameInterrupted()
    {
        Data.LastLevel = Data.LevelMax;
        UpdateUserProperty(UserProperty.last_level, Data.LastLevel);
        AnalyticsProduct.LevelInterrupted();
    }

    private void OnGameFinish(bool isWin)
    {
        Data.LastLevel = Data.LevelMax;
        UpdateUserProperty(UserProperty.last_level, Data.LastLevel);
        // Debug.Log("Last Level" + Data.LastLevel);

        if (isWin)
        {
            AnalyticsProduct.LevelWin();
        }
        else
        {
            AnalyticsProduct.LevelFail();
        }
    }

    private void OnGameStart()
    {
        Data.LevelMax = GamePrefs.Level + 1;
        Data.GameCount++;
        GlobalValues.Total_Shot = 0;
        GlobalValues.Coins_Collected = 0;
        if (Data.LevelMax == Data.LastLevel)
        {
            Data.GameCountByLevel++;
        }
        else
        {
            Data.GameCountByLevel = 1;
        }
        UpdateUserProperty(UserProperty.level_max, Data.LevelMax);
        UpdateUserProperty(UserProperty.accumulated_game_count, Data.GameCount);
        UpdateUserProperty(UserProperty.game_count, Data.GameCountByLevel);

        AnalyticsProduct.LevelStart();
    }

    public static void UpdateUserProperty(string userPropertyName, float value)
    {
        FirebaseAnalytics.SetUserProperty(userPropertyName, value.ToString());
    }
    public static void UpdateUserPropertyString(string userPropertyName, string value)
    {
        FirebaseAnalytics.SetUserProperty(userPropertyName, value);
    }

    private void UpdateIapCount()
    {
        Data.IapCount++;
        UpdateUserProperty(UserProperty.iap_count, Data.IapCount);
    }

    public void UpdateIapRev(float addedValue)
    {
        Data.IapRev += addedValue;
        UpdateUserProperty(UserProperty.iap_rev, Data.IapRev);
    }

    private void UpdateTotalWatchedInterAds(int value)
    {
        Data.TotalWatchedInterAds = value;
        UpdateUserProperty(UserProperty.total_watched_inter_ads, Data.TotalWatchedInterAds);
    }

    private void UpdateTotalWatchedRewardAds(int value)
    {
        Data.TotalWatchedRewardAds = value;
        UpdateUserProperty(UserProperty.total_watched_reward_ads, Data.TotalWatchedRewardAds);
    }

    private void OnCharacterSkinChanged() => UpdateUserPropertyString(UserProperty.current_skin, Data.CurrentSkin);
    private void OnSpearSkinChanged()     => UpdateUserPropertyString(UserProperty.current_spear, Data.CurrentSpear);
    private void OnTrailSkinChanged()     => UpdateUserPropertyString(UserProperty.current_trail, Data.CurrentTrail);

    private void OnSave()
    {
        if (!_isLoad) return;

        string data = JsonUtility.ToJson(Data);

        PlayerPrefs.SetString(KeySave, data);

        PlayerPrefs.Save();

        //Debug.Log("OnSave " + data);
    }

    private void SetStartDate()
    {
        if (PlayerPrefs.HasKey("DateInitialized")) //if we have the start date saved, we'll use that
            startDate = System.Convert.ToDateTime(PlayerPrefs.GetString("DateInitialized"));
        else //otherwise...
        {
            startDate = System.DateTime.Now; //save the start date ->
            PlayerPrefs.SetString("DateInitialized", startDate.ToString());
        }
    }
    private void GetDay()
    {
        Data.ValueBackGame = 0;

        var dayPass = DayPass;
        if (Data.Day != dayPass)
        {
            Data.Day = dayPass;
            Data.DayLogin++;
            Data.TotalBackGame = 0;
        }
    }

    public static string GetDaysPassed()
    {
        today = System.DateTime.Now;

        //days between today and start date -->
        System.TimeSpan elapsed = today.Subtract(startDate);

        double days = elapsed.TotalDays;

        return days.ToString("0");
    }

    public int DayPass => int.Parse(GetDaysPassed());

    private void OnApplicationPause(bool isPause)
    {
        OnSave();
        if (!isPause && _isLoad)
        {
            Data.ValueBackGame++;
            Data.TotalBackGame++;
        }
    }

    private void OnApplicationQuit()
    {
        OnSave();
    }

    private void Update()
    {
        Data.PlayTime += Time.deltaTime;
        Data.PlayTimeAds += Time.deltaTime;
        var iPlayTime = (int)Data.PlayTime;
        var iPlayTimeAds = (int)Data.PlayTimeAds;


        if (iPlayTime != _playtimeCache)
        {
            _playtimeCache = iPlayTime;
            UpdateUserProperty(UserProperty.time_in_app_no_ads, _playtimeCache);
        }
        if (iPlayTimeAds != _playtimeAdsCache)
        {
            _playtimeAdsCache = iPlayTimeAds;
            UpdateUserProperty(UserProperty.time_in_app, _playtimeAdsCache);
        }

        var iPlayTimeMins = iPlayTime / 60;
        if (iPlayTimeMins != _lastFTUEMins)
        {
            _lastFTUEMins = iPlayTimeMins;
            switch (iPlayTimeMins)
            {
                case 1:
                case 3:
                case 5:
                case 10:
                case 15:
                case 20:
                case 30:
                    AnalyticsFTUE.LogEvent(string.Format(EventName.FTUE_user_playtime_mins, iPlayTimeMins));
                    break;
            }
        }
    }

    public void UpdatePlayTimeAds(float adsDuration)
    {
        Data.PlayTimeAds += adsDuration;
        _playtimeAdsCache = (int)Data.PlayTimeAds;
        UpdateUserProperty(UserProperty.time_in_app, _playtimeAdsCache);
    }

    public void LogUserProperties()
    {
        UpdateUserProperty(UserProperty.last_level, Data.LastLevel);
        UpdateUserProperty(UserProperty.level_max, Data.LevelMax);
        UpdateUserProperty(UserProperty.total_cash_income, Data.TotalCoinInCome);
        UpdateUserProperty(UserProperty.total_cash_outcome, Data.TotalCoinOutCome);
        UpdateUserProperty(UserProperty.iap_count, Data.IapCount);
        UpdateUserProperty(UserProperty.iap_rev, Data.IapRev);
        UpdateUserProperty(UserProperty.total_watched_inter_ads, Data.TotalWatchedInterAds);
        UpdateUserProperty(UserProperty.total_watched_reward_ads, Data.TotalWatchedRewardAds);
        // UpdateUserProperty(UserProperty.day_diff, Data.Day);
        // UpdateUserProperty(UserProperty.playtime, Data.GetPlayTime);
        UpdateUserProperty(UserProperty.time_in_app, (int)Data.PlayTimeAds);
        UpdateUserProperty(UserProperty.time_in_app_no_ads, (int)Data.PlayTime);
        UpdateUserProperty(UserProperty.accumulated_game_count, Data.GameCount);
        UpdateUserProperty(UserProperty.game_count, Data.GameCountByLevel);

        UpdateUserPropertyString(UserProperty.current_skin, Data.CurrentSkin);
        UpdateUserPropertyString(UserProperty.current_spear, Data.CurrentSpear);
        UpdateUserPropertyString(UserProperty.current_trail, Data.CurrentTrail);
    }
}

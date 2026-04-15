using UnityEngine;
using FoodMaster;
using System;
using GameBase;

public class GlobalValues
{
    public static int MoveCount;
    public static bool SFXReady;
    public static bool InitAssetsReady;
    public static bool SoundEnable;
    public static bool MusicEnable;
    public static bool HapticEnable;
    public static bool IsRandomClear;
    public static bool IsTargetClear;
    public static bool IsShuffle;
    public static bool IsMagicWand;
    public static bool ShowUI = true;
    public static bool ShowEnemySound = true;
    public static bool ShowEnemeySoul = true;
    public static float PlayTimeRemain;
    public static float MoveTime;
    public static string LocationAds = ParameterValue.init;
    public static string RewardAds = "none";
    public static string LocationStartLevel = ParameterValue.main;
    public static string FailManner = ParameterValue.out_of_time;
    public static int Total_Shot = 0;
    public static int Coins_Collected = 0;
    public static string IAP_Location = "";
    public static GameState GameState;
    public static GameState LastGameState;
    // public static LevelStage CurrentLevelStage;
    // public static Customer CurrentCustomer;

    public static bool IsUsingBooster => IsRandomClear || IsTargetClear || IsShuffle || IsMagicWand;

    public static void Init()
    {
        SoundEnable = GamePrefs.SoundEnable;
        MusicEnable = GamePrefs.MusicEnable;
        HapticEnable = GamePrefs.HapticEnable;
    }


    public static void PrepareGame()
    {
        MoveCount = 0;
        MoveTime = 0f;
    }

    public static void StartGame()
    {
        GameState = GameState.Playing;
        GameEvents.OnGameStart?.Invoke();
        // Debug.Log("StartGame");
    }

    public static void UpdatePlayTimeRemain()
    {
        if (GameState == GameState.Playing)
        {
            PlayTimeRemain -= Time.deltaTime;
            if (PlayTimeRemain <= 0f)
            {
                PausePlay();
                PlayTimeRemain = 0f;
                // GameEvents.ShowPopup?.Invoke(PopupId.PopupOutOfTime, null);
            }
        }
    }

    public static void PausePlay(bool pauseTimeScale = true)
    {
        if (GameState == GameState.Paused) return;

        LastGameState = GameState;
        GameState = GameState.Paused;
        if (pauseTimeScale) Time.timeScale = 0f;
    }

    public static void ResumePlay()
    {
        GameState = LastGameState;
        Time.timeScale = 1f;
    }

    public static void InterrupPlay()
    {
        GameEvents.OnGameInterrupted?.Invoke();
    }

    public static void FinishPlay(bool isWin)
    {
        // if (GameState == GameState.Finished) return;
        // Time.timeScale = 1f;
        // LastGameState = GameState.Finished;
        // GameState = GameState.Finished;
        GameEvents.OnGameFinish?.Invoke(isWin);
        // Debug.Log("FinishPlay"+ isWin );
    }


    public static float GetSafeAreaUIDiffY(float fullRectHeight)
    {
        float diff = Screen.height - Screen.safeArea.height;
        return diff * fullRectHeight / Screen.height;
    }

    // public static void AddCoin(int value, string location, string manner)
    // {
    //     var newVal = GamePrefs.Coin += value;
    //     GameEvents.OnGoldUpdated?.Invoke(newVal);
    //     if (value > 0) AnalyticsResouces.InCome(location, manner, ResourceType.Gold, ParameterValue.gold, value);
    //     else AnalyticsResouces.OutCome(location, manner, ResourceType.Gold, -value);
    // }

}
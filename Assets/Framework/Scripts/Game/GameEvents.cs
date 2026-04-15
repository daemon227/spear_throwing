using UnityEngine;
using System;
using DongA.SpearManager;

public partial class GameEvents
{
    public static Action OnRemoteConfigLoaded;
    public static Action<bool> OnMusicStateChanged;
    public static Action<bool> OnSoundStateChanged;
    public static Action<bool> OnHapticStateChanged;
    public static Action<string> LoadScene;
    public static Action ShowSceneLoading;
    public static Action OnGameStart;
    public static Action OnGameInterrupted;
    public static Action<bool> OnGameFinish;
    public static Action<string> ShowNoticeFloat;
    public static Action<string, bool> OnBuyProductIAPCompleted;
    public static Action<int> OnGoldUpdated;
    public static Action<bool> OnShowUIStateChanged;
    public static Action OnSortCountUpdated;
    public static Action OnHintNoSpace;
    public static Action<bool> OnNoAdsChanged;


//spear events
    public static Action OnSpearStuck;
    public static Action OnSpearDragging;
    public static Action OnSpearLaunch;

    //effects events
    public static Action<Vector3> OnWallBreakEffect;
    public static Action<WallSide, Vector3> OnSpearHitWallEffect; // Show hole effect when spear hits wall
    public static Action<Vector3, GameObject,Color, GameObject> OnDeadEffect; // Thêm tham số BloodEffect, Blood Color ,và soulEffect
    public static Action<GameObject, Vector3, Color> OnSpawnEffect; // color them cung duoc.
    public static Action<PowerupType> OnRecivePowerup;

    public static Action OnMultiSliceEffectStart;
    public static Action<Vector3> OnMultiSliceEffectDrop;  // Vị trí top center lúc bắt đầu lao xuống
    public static Action OnMultiSliceEffectCompleted; // Hiệu ứng chém nhiều nhát

    // Kill Slow Mo Camera Events
    public static Action<Vector3> OnKillSlowMoStart;
    public static Action OnKillSlowMoEnd;

    //Game Events
    public static Action OnLevelLoadComplete;
    public static Action OnLevelStartLoading;

    //Shop Events
    public static Action OnCharacterSkinChosen;
    public static Action OnSpearSkinChosen;
    public static Action OnTrailSkinChosen;
    public static Action OnCloseShop;
    public static Action OnOpenShop;

    // Notification Events
    public static Action<int> OnTestNotification; // weekHour (0-167)

    // UI Events
    public static Action OnCoinUpdated;
    public static Action<int> OnIAPCoinReward; // Fired when IAP coin reward needs to be shown with animation
    public static Action<string> ShowMrecUI;
    public static Action HideMrecUI;
    public enum PowerupType
    {
        None,
        Ice,
        Clone,
        Fire,
        Thunder
    }
    
}

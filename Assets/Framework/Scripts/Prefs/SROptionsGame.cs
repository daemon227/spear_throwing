using System.ComponentModel;
using DongA.Game;
using GameBase;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements.Experimental;

public partial class SROptions
{
    int _selectedLevelId = 0;

    // MKT Theme color state (RGB 0-255)
    private int _themeR = 51;
    private int _themeG = 153;
    private int _themeB = 255;
    private int _themeA = 255;
    private string _themeHex = "#3399FF";

    [Category("Turn On MaxSDK Debugger")]
    [Description("Bật MaxSDK Debugger để xem log quảng cáo")]
    [SRDebugger.Sort(1)]
    public void EnableMaxSDKDebugger()
    {
        MaxSdk.ShowMediationDebugger();
    }

    [Category("Level Hack")]
    [Description("Nhập level ID để chơi (1-?)")]
    [SRDebugger.NumberRange(1, 100)]
    [SRDebugger.Increment(1)]
    public int SelectedLevelId
    {
        get { return _selectedLevelId; }
        set
        {
            _selectedLevelId = value;
            OnPropertyChanged(nameof(SelectedLevelId));
        }
    }
    [Category("Level Hack")]
    [Description("Load level được chọn")]
    [SRDebugger.Sort(1)]
    public void LoadSelectedLevel()
    {
        GamePrefs.Level = _selectedLevelId;
        GameEvents.OnLevelStartLoading?.Invoke();
    }


    [Category("Hack Tool")]
    public bool AdsFree
    {
        get => GamePrefs.IsAdsFree;
        set => GamePrefs.IsAdsFree = value;
    }

    [Category("Hack Tool")]
    public bool IsNoAds
    {
        get => GamePrefs.IsNoAds;
        set => GamePrefs.IsNoAds = value;
    }

    [Category("Hack Tool")]
    public int Coin
    {
        get => GamePrefs.Coin;
        set => GamePrefs.Coin = value;
    }

    [Category("MKT")]
    public bool ShowUI
    {
        get => GlobalValues.ShowUI;
        set
        {
            GlobalValues.ShowUI = value;
            GameEvents.OnShowUIStateChanged?.Invoke(value);
        }
    }
    [Category("MKT")]
    public bool EnableEnemySound
    {
        get => GlobalValues.ShowEnemySound;
        set
        {
            GlobalValues.ShowEnemySound = false;
        }
    }
    [Category("MKT")]
    public bool EnablEnemySoul
    {
        get => GlobalValues.ShowEnemeySoul;
        set
        {
            GlobalValues.ShowEnemeySoul = value;
        }
    }

    private int _notificationHours = 1;

    [Category("Notification Test")]
    [Description("Số giờ sau khi bắn notification (theo schedule)")]
    [SRDebugger.NumberRange(0, 167)]
    [SRDebugger.Increment(1)]
    [SRDebugger.Sort(0)]
    public int NotificationHours
    {
        get => _notificationHours;
        set
        {
            _notificationHours = value;
            OnPropertyChanged(nameof(NotificationHours));
        }
    }

    [Category("Notification Test")]
    [Description("Bắn test notification sau số giờ đã nhập")]
    [SRDebugger.Sort(1)]
    public void SendTestNotification()
    {
        GameEvents.OnTestNotification?.Invoke(_notificationHours);
        Debug.Log($"[SROptions] Fired OnTestNotification with {_notificationHours}h");
    }

    [Category("Hack Tool")]
    [Description("Xoá toàn bộ dữ liệu PlayerPrefs")]
    [SRDebugger.Sort(99)]
    public void DeleteAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[SROptions] Đã xoá toàn bộ PlayerPrefs");
    }

    // --- Helper ---
    private Color CurrentThemeColor => new Color(_themeR / 255f, _themeG / 255f, _themeB / 255f, _themeA / 255f);
    private void ApplyIfActive() { if (ThemeManager.Instance != null && ThemeManager.Instance.IsMKTThemeActive) ThemeManager.Instance.UpdateBackgroundColor(CurrentThemeColor); }

    // --- Toggle ---
    [Category("MKT Theme")]
    [Description("Bật/tắt MKT Theme")]
    [SRDebugger.Sort(0)]
    public bool MKTThemeEnabled
    {
        get => ThemeManager.Instance != null && ThemeManager.Instance.IsMKTThemeActive;
        set
        {
            if (ThemeManager.Instance == null) return;
            if (value) ThemeManager.Instance.UpdateBackgroundColor(CurrentThemeColor);
            else ThemeManager.Instance.DisableMKTTheme();
            OnPropertyChanged(nameof(MKTThemeEnabled));
        }
    }

    // --- Hex Input ---
    [Category("MKT Theme")]
    [Description("Nhập mã màu Hex (vd: #FF5533 hoặc FF5533)")]
    [SRDebugger.Sort(5)]
    public string ThemeHexColor
    {
        get => _themeHex;
        set
        {
            _themeHex = value;
            OnPropertyChanged(nameof(ThemeHexColor));
        }
    }

    [Category("MKT Theme")]
    [Description("Apply mã màu Hex đã nhập")]
    [SRDebugger.Sort(6)]
    public void ApplyHexColor()
    {
        string hex = _themeHex.Trim();
        if (!hex.StartsWith("#")) hex = "#" + hex;
        if (ColorUtility.TryParseHtmlString(hex, out Color color))
        {
            _themeR = Mathf.RoundToInt(color.r * 255f);
            _themeG = Mathf.RoundToInt(color.g * 255f);
            _themeB = Mathf.RoundToInt(color.b * 255f);
            _themeA = Mathf.RoundToInt(color.a * 255f);
            ApplyIfActive();
        }
        else
        {
            Debug.LogWarning($"[ThemeManager] Hex không hợp lệ: {_themeHex}");
        }
    }

}

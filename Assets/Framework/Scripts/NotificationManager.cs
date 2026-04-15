using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

#if UNITY_ANDROID
using Unity.Notifications.Android;
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class NotificationManager : MonoBehaviour
{
    [Header("Android Channel Settings")]
    public string channelId = "master_channel";
    public string channelName = "Master Notifications";
    public string channelDescription = "Daily kitchen alerts";

    private const string LastTrackedNotificationKey = "LastTrackedNotification";

    private int[] Schedule
    {
        get
        {
            var rc = RemoteConfig.Instance;
            if (rc != null && rc.NotificationSchedule != null && rc.NotificationSchedule.Length > 0)
                return rc.NotificationSchedule;
            return new int[] { 12, 36, 60, 84, 108, 130, 132, 154, 156 };
        }
    }

    private int MaxDaily
    {
        get
        {
            var rc = RemoteConfig.Instance;
            if (rc != null && rc.NotificationMaxDaily > 0)
                return rc.NotificationMaxDaily;
            return 2;
        }
    }

    #region Notification Content Keys
    // Localize keys for notification content
    private static readonly List<string> NotificationContentKeys = new List<string>()
    {
        "noti_zombie_pickle",
        "noti_crazy_banana",
        "noti_mutating_pickle",
        "noti_pickle_takeover",
        "noti_spear_stronger",
        "noti_giant_pickle",
        "noti_spear_stop",
        "noti_rare_fruit"
    };

    // Default text fallback (used when no localization system is integrated)
    private static readonly Dictionary<string, string> NotificationDefaultText = new Dictionary<string, string>()
    {
        { "noti_zombie_pickle", "A zombie pickle appeared! 🧟 Eliminate it before the infection spreads! ☣️" },
        { "noti_crazy_banana", "Your banana went crazy! 🍌 Stop it before it escapes!" },
        { "noti_mutating_pickle", "Pickle is mutating… 🥒 Destroy it now!" },
        { "noti_pickle_takeover", "Pickles are taking over! 🥒 Throw your spear and stop them!" },
        { "noti_spear_stronger", "Your spear got stronger! 💥 Test it on the pickles now!" },
        { "noti_giant_pickle", "A giant pickle appeared! 🥒 Take it down with your spear!" },
        { "noti_spear_stop", "Only your spear can stop this! 🗡️ Jump in and eliminate them!" },
        { "noti_rare_fruit", "A rare mutant fruit appeared! 🍇 Don’t miss your chance!" }
    };
    #endregion

    void Start()
    {
        CheckNotificationOpened();

#if UNITY_ANDROID
        if (IsAndroid13OrHigher())
        {
            StartCoroutine(RequestNotificationPermission());
        }
        else
        {
            InitializeNotifications();
        }
#elif UNITY_IOS
        InitializeNotifications();
#endif

        GameEvents.OnRemoteConfigLoaded += OnRemoteConfigLoaded;
        GameEvents.OnTestNotification += OnTestNotification;
    }

    void OnDestroy()
    {
        GameEvents.OnRemoteConfigLoaded -= OnRemoteConfigLoaded;
        GameEvents.OnTestNotification -= OnTestNotification;
    }

    private void OnTestNotification(int weekHour)
    {
        DateTime now = DateTime.Now;
        DateTime fireTime = now.AddHours(weekHour);
        string contentKey = GetRandomContentKey();
        string intentData = $"{weekHour}|{contentKey}";
        ScheduleNotification(fireTime, contentKey, intentData);
        Debug.Log($"[NotificationManager] Test notification scheduled in {weekHour}h at {fireTime}");
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            CheckNotificationOpened();
        }
    }

    private void OnRemoteConfigLoaded()
    {
        CancelAllNotifications();
        ScheduleWeeklyNotifications();
    }

    private void InitializeNotifications()
    {
        RegisterNotificationChannel();
        CancelAllNotifications();
        PlayerPrefs.DeleteKey(LastTrackedNotificationKey);
        ScheduleWeeklyNotifications();
    }

    void RegisterNotificationChannel()
    {
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = channelId,
            Name = channelName,
            Importance = Importance.Default,
            Description = channelDescription
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
    }

    void CancelAllNotifications()
    {
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllScheduledNotifications();
#elif UNITY_IOS
        iOSNotificationCenter.RemoveAllScheduledNotifications();
#endif
    }

    /// <summary>
    /// Schedule notifications for the upcoming week based on the week-hour schedule.
    /// Week hours: 0 = Monday 0:00, 24 = Tuesday 0:00, ..., 167 = Sunday 23:00
    /// Enforces maxDaily limit per day.
    /// </summary>
    void ScheduleWeeklyNotifications()
    {
        DateTime now = DateTime.Now;
        // Calculate this week's Monday 00:00
        int currentDayOfWeek = ((int)now.DayOfWeek + 6) % 7; // 0=Mon, 6=Sun
        DateTime mondayStart = now.Date.AddDays(-currentDayOfWeek);

        int[] schedule = Schedule;
        int maxDaily = MaxDaily;

        Dictionary<int, int> dailyCount = new Dictionary<int, int>();

        foreach (int weekHour in schedule)
        {
            if (weekHour < 0 || weekHour >= 168) continue;

            int dayIndex = weekHour / 24; // 0=Mon, 6=Sun
            int hourOfDay = weekHour % 24;

            if (!dailyCount.ContainsKey(dayIndex))
                dailyCount[dayIndex] = 0;

            if (dailyCount[dayIndex] >= maxDaily)
                continue;

            DateTime fireTime = mondayStart.AddDays(dayIndex).AddHours(hourOfDay);

            // If time already passed this week, schedule for next week
            if (fireTime <= now)
                fireTime = fireTime.AddDays(7);

            string contentKey = GetRandomContentKey();
            string intentData = $"{weekHour}|{contentKey}";

            ScheduleNotification(fireTime, contentKey, intentData);
            dailyCount[dayIndex]++;
        }
    }

    string GetRandomContentKey()
    {
        int index = UnityEngine.Random.Range(0, NotificationContentKeys.Count);
        return NotificationContentKeys[index];
    }

    string GetNotificationText(string key)
    {
        // TODO: Replace with localization system when available
        // e.g. return Localization.Get(key);
        if (NotificationDefaultText.TryGetValue(key, out string text))
            return text;
        return key;
    }

    public void ScheduleNotification(DateTime fireTime, string contentKey, string intentData)
    {
        string message = GetNotificationText(contentKey);

#if UNITY_ANDROID
        var notification = new AndroidNotification()
        {
            Text = message,
            FireTime = fireTime,
            ShowInForeground = false,
            SmallIcon = "icon_2",
            LargeIcon = "icon_1",
            Color = new Color(1f, .6f, 0f, 1f),
            IntentData = intentData
        };
        AndroidNotificationCenter.SendNotification(notification, channelId);
#elif UNITY_IOS
        var timeDiff = fireTime - DateTime.Now;
        if (timeDiff.TotalSeconds <= 0) return;

        var iosNotification = new iOSNotification()
        {
            Identifier = $"noti_{fireTime.Ticks}",
            Body = message,
            ShowInForeground = false,
            Data = intentData,
            Trigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = timeDiff,
                Repeats = false
            }
        };
        iOSNotificationCenter.ScheduleNotification(iosNotification);
#endif
    }

    #region Notification Tracking
    void CheckNotificationOpened()
    {
#if UNITY_ANDROID
        var intent = AndroidNotificationCenter.GetLastNotificationIntent();
        if (intent != null && !string.IsNullOrEmpty(intent.Notification.IntentData))
        {
            TrackNotificationFromIntent(intent.Notification.IntentData);
        }
#elif UNITY_IOS
        var notification = iOSNotificationCenter.GetLastRespondedNotification();
        if (notification != null && !string.IsNullOrEmpty(notification.Data))
        {
            TrackNotificationFromIntent(notification.Data);
        }
#endif
    }

    void TrackNotificationFromIntent(string intentData)
    {
        // Avoid tracking the same notification open twice
        // Store all tracked intents separated by ";;", not just the last one
        string allTracked = PlayerPrefs.GetString(LastTrackedNotificationKey, "");
        if (allTracked.Contains(intentData)) return;
        allTracked = string.IsNullOrEmpty(allTracked) ? intentData : allTracked + ";;" + intentData;
        PlayerPrefs.SetString(LastTrackedNotificationKey, allTracked);

        string[] parts = intentData.Split('|');
        if (parts.Length >= 2 && int.TryParse(parts[0], out int weekHour))
        {
            string notificationKey = parts[1];
            var param = AnalyticHelper.CreateParam(
                ParameterName.time, weekHour,
                ParameterName.notification_id, notificationKey
            );
            AnalyticHelper.LogEvent(EventName.notification_open_app, param);
        }
    }
    #endregion

    #region Android 13+ Permission
#if UNITY_ANDROID
    private bool IsAndroid13OrHigher()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            string os = SystemInfo.operatingSystem;
            int apiIndex = os.IndexOf("API-");
            if (apiIndex != -1 && int.TryParse(os.Substring(apiIndex + 4), out int apiLevel))
            {
                return apiLevel >= 33;
            }
        }
        return false;
    }

    private IEnumerator RequestNotificationPermission()
    {
        var request = new PermissionRequest();
        while (request.Status == PermissionStatus.RequestPending)
            yield return null;

        if (request.Status == PermissionStatus.Allowed)
        {
            InitializeNotifications();
        }
    }
#endif
    #endregion
}

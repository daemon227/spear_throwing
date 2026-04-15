using AppsFlyerSDK;
using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitScript : MonoBehaviour
{

    private void Awake()
    {
        Input.multiTouchEnabled = false;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 300;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        DOTween.defaultTimeScaleIndependent = false;

        GlobalValues.Init();

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        FirebaseSDK.Instance.OnFirebaseReady -= OnFirebaseReady;
    }

    private IEnumerator Start()
    {
        FirebaseSDK.Instance.OnFirebaseReady += OnFirebaseReady;

        float loadingStartTime = Time.time;
        while (Time.time - loadingStartTime < .5f)
            yield return null;

        //GlobalValues.InitByRemoteConfig();

        SceneManager.LoadScene("Game");
        GlobalValues.LocationAds = ParameterValue.in_game;

        AnalyticsFTUE.LogEvent(EventName.FTUE1_loading_end);
    }

    private void OnFirebaseReady()
    {
        AppsFlyer.sendEvent(EventName.af_session, null);
        AnalyticsFTUE.LogEvent(EventName.FTUE1_loading_start);
    }
}

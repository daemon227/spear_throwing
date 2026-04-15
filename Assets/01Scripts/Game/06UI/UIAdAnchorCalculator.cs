using UnityEngine;

public class UIAdAnchorCalculator : MonoBehaviour
{
    [Header("Ad size")]
    public float mrecHeightDp = 250f;
    public float bannerHeightDpPhone = 50f;
    public float bannerHeightDpTablet = 90f;

    [Header("Anchor")]
    public RectTransform rectTrans;
    public Transform scaleTrans;

    private void Start()
    {
        if(GamePrefs.IsNoAds) return;
        var dpi = Screen.dpi;
        if (dpi <= 0) dpi = 160f;
#if UNITY_EDITOR
        // dpi = RemoteConfig.Instance.FakeDPI;
#endif

        var screenWidthDp = 1f * Screen.width / (dpi / 160f);
        var isTablet = screenWidthDp >= 600f;

        var screenHeight = 2160f;
        var screenHeightDp = 1f * Screen.height / (dpi / 160f);
        var bannerHeightDp = isTablet ? bannerHeightDpTablet : bannerHeightDpPhone;
        var mrecHeightPx = mrecHeightDp / screenHeightDp * screenHeight;
        var bannerHeightPx = bannerHeightDp / screenHeightDp * screenHeight;

        var mrecTopY = -screenHeight / 2 + mrecHeightPx;
        var bannerBottomY = screenHeight / 2 - bannerHeightPx;
        var range = bannerBottomY - mrecTopY;
        var middleY = (mrecTopY + bannerBottomY) / 2f;

        if (rectTrans)
        {
            rectTrans.anchoredPosition = new Vector2(0, middleY);
        }

        if (scaleTrans)
        {
            var scale = Mathf.Clamp01(range / rectTrans.sizeDelta.y);
            scaleTrans.localScale = new Vector3(scale, scale, scale);
        }
    }
}
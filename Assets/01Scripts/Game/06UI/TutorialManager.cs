using AppsFlyerSDK;
using UnityEngine;

namespace DongA.UI
{
    public class TutorialManager : MonoBehaviour
    {
        [SerializeField] private GameObject spearControlTutorialPrefab;
        [SerializeField] private GameObject powerupTutorialPrefab;
        [SerializeField] private GameObject shopTutorial;
        [SerializeField] private GameObject shopTutorialPanel;

        private GameObject _spearControlTutorialInstance;
        private GameObject _powerupTutorialInstance;

        private const int SPEAR_TUTORIAL_LEVEL = 0;
        private const int POWERUP_TUTORIAL_LEVEL = 2;
        private  int _shopTutorialCoin = 300;
        private int _nextShopTutorialCoin = 300;
        private const string SHOP_PANEL_SEEN_KEY = "ShopTutorialPanelSeen";
        private const string SHOP_TUTORIAL_NEXT_COIN_KEY = "ShopTutorialNextCoin";
        private bool _showShopTutorial;


        private void Awake()
        {
            if (RemoteConfig.Instance != null)
            {
                _shopTutorialCoin = RemoteConfig.Instance.CoinToShowShopTutorial;
            }

            if (!PlayerPrefs.HasKey(SHOP_TUTORIAL_NEXT_COIN_KEY))
            {
                _nextShopTutorialCoin = _shopTutorialCoin;
                PlayerPrefs.SetInt(SHOP_TUTORIAL_NEXT_COIN_KEY, _nextShopTutorialCoin);
                PlayerPrefs.Save();
            }
            else
            {
                _nextShopTutorialCoin = PlayerPrefs.GetInt(SHOP_TUTORIAL_NEXT_COIN_KEY);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnLevelLoadComplete += OnLevelLoadComplete;
            GameEvents.OnSpearDragging += HideSpearControlTutorial;
            GameEvents.OnSpearDragging += HidePowerupTutorial;
            GameEvents.OnSpearDragging += HideShopTutorialPanel;
            GameEvents.OnCoinUpdated += OnCoinUpdated;
            GameEvents.OnGameFinish += OnGameFinish;
            GameEvents.OnOpenShop += HideShopTutorial;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelLoadComplete -= OnLevelLoadComplete;
            GameEvents.OnSpearDragging -= HideSpearControlTutorial;
            GameEvents.OnSpearDragging -= HidePowerupTutorial;
            GameEvents.OnSpearDragging -= HideShopTutorialPanel;
            GameEvents.OnCoinUpdated -= OnCoinUpdated;
            GameEvents.OnGameFinish -= OnGameFinish;
            GameEvents.OnOpenShop -= HideShopTutorial;
        }

        private void OnLevelLoadComplete()
        {
            int level = GamePrefs.Level;

            if (level == SPEAR_TUTORIAL_LEVEL && PlayerPrefs.GetInt(KeyPrefs.SpearTutorialSeen, 0) == 0)
                ShowSpearControlTutorial();
            else
                HideSpearControlTutorial();

            if (level == POWERUP_TUTORIAL_LEVEL && PlayerPrefs.GetInt(KeyPrefs.PowerupTutorialSeen, 0) == 0)
                ShowPowerupTutorial();
            else
                HidePowerupTutorial();

            if (shopTutorial != null )
                shopTutorial.SetActive(false);
            if (shopTutorialPanel != null )
                shopTutorialPanel.SetActive(false);

            ShowShopTutorial();
        }

        private void ShowSpearControlTutorial()
        {
            if (_spearControlTutorialInstance == null)
                _spearControlTutorialInstance = Instantiate(spearControlTutorialPrefab, transform);

            _spearControlTutorialInstance.SetActive(true);
        }

        private void HideSpearControlTutorial()
        {
            if (_spearControlTutorialInstance == null || !_spearControlTutorialInstance.activeSelf) return;

            PlayerPrefs.SetInt(KeyPrefs.SpearTutorialSeen, 1);
            PlayerPrefs.Save();
            _spearControlTutorialInstance.SetActive(false);
            AppsFlyer.sendEvent(EventName.af_tutorial_completion,null);
        }

        private void ShowPowerupTutorial()
        {
            if (_powerupTutorialInstance == null)
                _powerupTutorialInstance = Instantiate(powerupTutorialPrefab, transform);

            _powerupTutorialInstance.SetActive(true);
            PlayerPrefs.SetInt(KeyPrefs.PowerupTutorialSeen, 1);
            PlayerPrefs.Save();
        }

        private void HidePowerupTutorial()
        {
            if (_powerupTutorialInstance == null || !_powerupTutorialInstance.activeSelf) return;

            _powerupTutorialInstance.SetActive(false);
            AppsFlyer.sendEvent(EventName.af_tutorial_completion,null);
        }

        private void OnCoinUpdated()
        {
            if (GamePrefs.Coin >= _nextShopTutorialCoin)
            {
                _showShopTutorial = true;
                _nextShopTutorialCoin = GamePrefs.Coin + _shopTutorialCoin;
                PlayerPrefs.SetInt(SHOP_TUTORIAL_NEXT_COIN_KEY, _nextShopTutorialCoin);
                PlayerPrefs.Save();
            }
        }

        private void ShowShopTutorial()
        {
            if (!_showShopTutorial) return;
            _showShopTutorial = false;

            if (shopTutorial != null)
                shopTutorial.SetActive(true);

            if (shopTutorialPanel != null && PlayerPrefs.GetInt(SHOP_PANEL_SEEN_KEY, 0) == 0)
            {
                shopTutorialPanel.SetActive(true);
                GamePrefs.IsShopTutorialPanelActive = true;
                PlayerPrefs.SetInt(SHOP_PANEL_SEEN_KEY, 1);
                PlayerPrefs.Save();
                AppsFlyer.sendEvent(EventName.af_tutorial_completion,null);
            }
        }

        private void HideShopTutorial()
        {
            _showShopTutorial = false;
            if (shopTutorial != null)
                shopTutorial.SetActive(false);
            if (shopTutorialPanel != null)
                shopTutorialPanel.SetActive(false);
        }

        private void HideShopTutorialPanel()
        {
            GamePrefs.IsShopTutorialPanelActive = false;
            if (shopTutorialPanel != null)
                shopTutorialPanel.SetActive(false);
        }

        private void OnGameFinish(bool isWin)
        {
            if (shopTutorial != null)
                shopTutorial.SetActive(false);
            if (shopTutorialPanel != null)
                shopTutorialPanel.SetActive(false);
        }
    }
}

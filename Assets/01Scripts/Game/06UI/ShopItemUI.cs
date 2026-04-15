using DongA.Events;
using DongA.GameDatas;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace DongA.UI
{
    public class ShopItemUI : MonoBehaviour
    {
        [Header("Item")]
        [SerializeField] private ShopItemType shopItemType;
        [SerializeField] private Image background;
        [SerializeField] private GameObject newItemNoti;
        [SerializeField] private GameObject usedIcon;
        [SerializeField] private GameObject lockItem;
        [SerializeField] private GameObject coinIcon;
        [SerializeField] private Image skinImage;
        [Header("Character Skin")]
        [SerializeField] private Button actionButton;
        [SerializeField] private TextMeshProUGUI actionButtonText;
        [Header("Spear Skin")]
        [SerializeField] private TextMeshProUGUI skinNameText;
        [SerializeField] private Image deadImage;
        private ISkinData _data;
        private ShopItemUITheme _theme;
        private int _price;

        public void Setup(ISkinData data, ShopItemUITheme theme)
        {
            _data = data;
            _theme = theme;
            skinImage.sprite = data.SkinSprite;
            if (deadImage != null) deadImage.sprite = data.DeadSprite;
            if (skinNameText != null) skinNameText.text = data.SkinName;
            _price = GetPrice(data.SkinType);

            SubscribeEvents();
            Refresh();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        private void SubscribeEvents()
        {
            GameEvents.OnCharacterSkinChosen += Refresh;
            GameEvents.OnSpearSkinChosen     += Refresh;
            GameEvents.OnTrailSkinChosen     += Refresh;
            GameEvents.OnCoinUpdated         += Refresh;
        }

        private void UnsubscribeEvents()
        {
            GameEvents.OnCharacterSkinChosen -= Refresh;
            GameEvents.OnSpearSkinChosen     -= Refresh;
            GameEvents.OnTrailSkinChosen     -= Refresh;
            GameEvents.OnCoinUpdated         -= Refresh;
        }

        private void Refresh()
        {
            if (_data == null || this == null) return;

            // SpearSkin: auto-unlock nếu player đã đủ level
            if (_data is SpearSkinData spearData && spearData.UnlockLevel > 0)
            {
                if (!IsOwned() && GamePrefs.Level >= spearData.UnlockLevel)
                    GamePrefs.SetSpearSkinOwned(_data.SkinNameID);
            }

            bool owned   = IsOwned();
            bool current = IsCurrentSkin();

            if (newItemNoti != null)
                newItemNoti.SetActive(_data.IsNew && !owned && !current);

            if (lockItem != null)
                lockItem.SetActive(!owned);

            if (usedIcon != null)
                usedIcon.SetActive(current && shopItemType == ShopItemType.Skin);

            actionButton.onClick.RemoveAllListeners();

            SetCoinIConActive(false);
            if (current)
            {
                actionButtonText.text = "Using";
                if (shopItemType == ShopItemType.Skin)
                {
                    actionButton.interactable = false;
                }
                SetButtonSprite(2);
                SetBackgroundSprite(1);
            }
            else if (owned)
            {
                actionButtonText.text = "Use";
                if (shopItemType == ShopItemType.Skin)
                    actionButton.interactable = true;
                if(newItemNoti!=null) newItemNoti.gameObject.SetActive(false);
                actionButton.onClick.AddListener(SelectSkin);
                SetButtonSprite(0);
                SetBackgroundSprite(0);
            }
            else
            {
                if (_data is SpearSkinData sd && sd.UnlockLevel > 0)
                    // Có thể mua sớm bằng coin, hoặc chờ đủ level để free
                    actionButtonText.text = $"Lv.{sd.UnlockLevel} or {_price} coins";
                else
                {
                    if(_data.SkinType == SkinType.Ads)
                    {
                        actionButtonText.text = "Watch Ad";
                    }
                    else
                    {
                        actionButtonText.text = _price.ToString();
                        if(shopItemType == ShopItemType.SpearSkin)
                        {
                            actionButtonText.text = _price.ToString() + " coins";
                        }
                        SetCoinIConActive(true);
                    }
                }

                bool canBuy = _data.SkinType == SkinType.Ads || GamePrefs.Coin >= _price;
                actionButton.interactable = canBuy;
                actionButton.onClick.AddListener(OnBuyClicked);
                SetButtonSprite(_data.SkinType == SkinType.Ads ? 0 : 1);
                SetBackgroundSprite(0);
            }
            
        }
        private void SetCoinIConActive(bool isActive)
        {
            if(coinIcon!= null)
            {
                coinIcon.gameObject.SetActive(isActive);
            }
        }

        private void SetButtonSprite(int index)
        {
            if (_theme == null || _theme.buttonSprites == null) return;
            if(shopItemType == ShopItemType.SpearSkin) return;
            if (index < _theme.buttonSprites.Length)
                actionButton.image.sprite = _theme.buttonSprites[index];
        }
        private void SetBackgroundSprite(int index)
        {
            if (_theme == null ) return;
            switch (shopItemType)
            {
                case ShopItemType.Skin:
                    if(index < _theme.skinBackGroundSprites.Length)
                    {
                        background.sprite = _theme.skinBackGroundSprites[index];
                    }
                    break;
                case ShopItemType.SpearSkin:
                    if(index < _theme.spearSkinBackgroundSprites.Length)
                    {
                        background.sprite = _theme.spearSkinBackgroundSprites[index];
                    }
                    break;
            }
        }

        private void OnBuyClicked()
        {
            if (_data == null) return;

            if (_data.SkinType == SkinType.Ads)
            {
                if (AdsUtils.IsNoInternet)
                {
                    UIEvent.OnShowPopup(PopupType.NoInternet);
                    return;
                }

                AdsUtils.ShowReward((success, location, adInfo) =>
                {
                    if (success)
                    {
                        BuySkinConfirmed();
                        GameEvents.OnCloseShop?.Invoke();
                        return;
                    }
                    else return;
                }, ParameterName.shop, GetCurrentShopTab());
                return;
            }

            if (GamePrefs.Coin < _price) return;
            GamePrefs.Coin -= _price;
            AnalyticsResouces.OutCome(ParameterName.shop, GetCurrentShopTab(), ResourceType.Coin, _price);
            AnalyticsNavigation.Button(ParameterName.shop, ParameterValue.buy_button);
            
            GameEvents.OnCoinUpdated?.Invoke();
            BuySkinConfirmed();
        }

        private void BuySkinConfirmed()
        {
            var manner = ParameterValue.watch_ads;
            if(_data.SkinType != SkinType.Ads)
            {
                manner = ParameterValue.coin;
            }
            if (_data is SkinData)
            {
                GamePrefs.SetCharacterSkinOwned(_data.SkinNameID);
                AnalyticsProduct.BuySkin(_data.SkinName, GetBuyMethod());
                AnalyticsResouces.InCome(ParameterName.shop, manner, ResourceType.Skin, _data.SkinName, 1);
            }         
            else if (_data is SpearSkinData)
            {
                GamePrefs.SetSpearSkinOwned(_data.SkinNameID);
                AnalyticsProduct.BuySpear(_data.SkinName, GetBuyMethod());
                AnalyticsResouces.InCome(ParameterName.shop, manner, ResourceType.Spear, _data.SkinName, 1);
            } 
            else if (_data is TrailSkinData)
            {
                GamePrefs.SetTrailSkinOwned(_data.SkinNameID);
                AnalyticsProduct.BuyTrail(_data.SkinName, GetBuyMethod());
                AnalyticsResouces.InCome(ParameterName.shop, manner, ResourceType.Trail, _data.SkinName, 1);
            }
            SelectSkin();
        }

        private void SelectSkin()
        {
            if (_data is SkinData)
            {
                GamePrefs.CurrentCharacterSkinID = _data.SkinNameID;
                GamePrefs.CurrentCharacterSkinName = _data.SkinName;
                GameEvents.OnCharacterSkinChosen?.Invoke();
            }
            else if (_data is SpearSkinData sd)
            {
                GamePrefs.CurrentSpearSkinID = _data.SkinNameID;
                GamePrefs.CurrentSpearSkinName = _data.SkinName;
                GamePrefs.CurrentSpearType = (int)sd.SpearType;
                GameEvents.OnSpearSkinChosen?.Invoke();
            }
            else if (_data is TrailSkinData)
            {
                GamePrefs.CurrentTrailSkinID = _data.SkinNameID;
                GamePrefs.CurrentTrailSkinName = _data.SkinName;
                GameEvents.OnTrailSkinChosen?.Invoke();
            }
            AnalyticsNavigation.Button(ParameterName.shop, ParameterValue.use_button);
        }

        private bool IsOwned()
        {
            if (_data is SkinData)           return GamePrefs.IsCharacterSkinOwned(_data.SkinNameID);
            if (_data is SpearSkinData)      return GamePrefs.IsSpearSkinOwned(_data.SkinNameID);
            if (_data is TrailSkinData)      return GamePrefs.IsTrailSkinOwned(_data.SkinNameID);
            return false;
        }

        private bool IsCurrentSkin()
        {
            if (_data is SkinData)           return GamePrefs.CurrentCharacterSkinID == _data.SkinNameID;
            if (_data is SpearSkinData)      return GamePrefs.CurrentSpearSkinID == _data.SkinNameID;
            if (_data is TrailSkinData)      return GamePrefs.CurrentTrailSkinID == _data.SkinNameID;
            return false;
        }

        private int GetPrice(SkinType type)
        {
            if (_data is SkinData)
            {
                return type switch
                {
                    SkinType.Common    => RemoteConfig.Instance.CharacterSkinPrices[0],
                    SkinType.Legendary => RemoteConfig.Instance.CharacterSkinPrices[1],
                    _                  => 0
                };
            }
            else if (_data is SpearSkinData)
            {
                return type switch
                {
                    SkinType.Common    => RemoteConfig.Instance.SpearSkinPrices[0],
                    SkinType.Legendary => RemoteConfig.Instance.SpearSkinPrices[1],
                    _                  => 0
                };
            }
            else if (_data is TrailSkinData)
            {
                return type switch
                {
                    SkinType.Common    => RemoteConfig.Instance.TrailSkinPrices[0],
                    SkinType.Legendary => RemoteConfig.Instance.TrailSkinPrices[1],
                    _                  => 0
                };
            }
            return 0;
        }
        private string GetCurrentShopTab()
        {
            if(_data is SkinData skin) return "CharacterSkin";
            else if (_data is SpearSkinData spear) return "SpearSkin";
            else return "TrailSkin";
        }
        public string GetBuyMethod()
        {
            if (_data.SkinType == SkinType.Ads)
                return ParameterValue.buy_by_ads;
            else
                return ParameterValue.buy_by_coin;
        }
    }
    public enum ShopItemType
    {
        Skin, SpearSkin
    }
}

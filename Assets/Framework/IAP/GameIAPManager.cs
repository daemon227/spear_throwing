using DongA.Obstacles;
using Inwave.Production.Ads;
using UnityEngine;
using UnityEngine.Purchasing;

namespace GameIAP
{
    public class GameIAPManager : IAPManager
    {
        public static GameIAPManager Instance;
        
        // Product IDs
        public const string PRODUCT_NO_ADS = "com.spear.throwing.aim.hit.remove_ads_regular";
        public const string PRODUCT_FIRST_SALE_NO_ADS = "com.spear.throwing.aim.hit.remove_ads_firstsale";
        public const string PRODUCT_LAST_SALE_NO_ADS = "com.spear.throwing.aim.hit.remove_ads_lastsale";

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        protected virtual void Start()
        {
            GameEvents.OnRemoteConfigLoaded += OnRemoteConfigLoaded;

            if (RemoteConfig.Loaded) OnRemoteConfigLoaded();
        }

        private void OnDestroy()
        {
            GameEvents.OnRemoteConfigLoaded -= OnRemoteConfigLoaded;
        }

        private void OnRemoteConfigLoaded()
        {
            if (m_StoreController == null)
            {
                Initialize();
            }
        }
        

        protected override void AddProducts(ConfigurationBuilder builder)
        {
            base.AddProducts(builder);
            
            // Add No Ads product - Non-Consumable (one-time purchase)
            builder.AddProduct(PRODUCT_NO_ADS, ProductType.Consumable);
            builder.AddProduct(PRODUCT_FIRST_SALE_NO_ADS, ProductType.Consumable);
            builder.AddProduct(PRODUCT_LAST_SALE_NO_ADS, ProductType.Consumable);
        }

        public override void BuyProduct(string productId)
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                InvokeOnItemPurchased(productId, true);
            }
            else
            {
                if (AdsManager.Instance != null)
                    AdsManager.Instance.SetCanShowAppOpenAdd(false);
                base.BuyProduct(productId);
            }
        }

        protected override void InvokeOnItemPurchased(string id, bool success)
        {
            if (AdsManager.Instance != null)
                AdsManager.Instance.SetCanShowAppOpenAdd(true);
            base.InvokeOnItemPurchased(id, success);
            if (success)
            {
                ReceiveRewards(id);
                int buyCount = GetBuyCount(id);
                SetBuyCount(id, buyCount + 1);
                AnalyticManager.OnIapCount?.Invoke();
                // if (AnalyticManager.Instance != null) AnalyticManager.Instance.UpdateIapRev(GetPrice(id));
            }
            GameEvents.OnBuyProductIAPCompleted?.Invoke(id, success);
        }

        public int GetBuyCount(string id)
        {
            // return PlayerPrefs.GetInt(KeyPrefs.PackBuyCount, 0);
            return 0;
        }

        public void SetBuyCount(string id, int count)
        {
            // PlayerPrefs.SetInt(KeyPrefs.PackBuyCount, count);
        }


        private void ReceiveRewards(string id)
        {
            // Handle No Ads purchase (all 3 packages)
            if (IsNoAdsProduct(id))
            {
                GamePrefs.IsNoAds = true;
                int coin_Amount = 5000;
                if(RemoteConfig.Instance!= null)
                {
                    coin_Amount = RemoteConfig.Instance.RemoveAdConfig.BonusCoin;
                }

                // Dùng giá từ product thực tế (đã lưu trong ProcessPurchase), tránh trường hợp delay purchase không lấy được giá từ store
                float price = _lastPurchasePrice > 0 ? _lastPurchasePrice : GetPrice(id);
                string currency = !string.IsNullOrEmpty(_lastPurchaseCurrency) ? _lastPurchaseCurrency : GetCurrencyString();
                AnalyticIAP.IAPSuccess(GlobalValues.IAP_Location, ParameterValue.no_ads_popup, price, currency, coin_Amount, id, "success");
                AnalyticManager.Instance.UpdateIapRev(price);

                AnalyticsResouces.InCome(ParameterValue.no_ads_popup, ParameterValue.buy_iap_remove_ads, ResourceType.Coin, "coin", coin_Amount);
                GameEvents.OnIAPCoinReward?.Invoke(coin_Amount);
                return;
            }
        }
        
        /// <summary>
        /// Check if productId is any of the No Ads products
        /// </summary>
        public static bool IsNoAdsProduct(string productId)
        {
            return productId.Equals(PRODUCT_NO_ADS) || 
                   productId.Equals(PRODUCT_FIRST_SALE_NO_ADS) || 
                   productId.Equals(PRODUCT_LAST_SALE_NO_ADS);
        }
        
        /// <summary>
        /// Buy No Ads product (regular price)
        /// </summary>
        public void BuyNoAds()
        {
            BuyProduct(PRODUCT_NO_ADS);
        }
        
        /// <summary>
        /// Buy No Ads First Sale product
        /// </summary>
        public void BuyNoAdsFirstSale()
        {
            BuyProduct(PRODUCT_FIRST_SALE_NO_ADS);
        }
        
        /// <summary>
        /// Buy No Ads Last Sale product
        /// </summary>
        public void BuyNoAdsLastSale()
        {
            BuyProduct(PRODUCT_LAST_SALE_NO_ADS);
        }
        
        /// <summary>
        /// Check if No Ads has been purchased (for restore purchases)
        /// Checks all 3 No Ads products (regular, first sale, last sale)
        /// </summary>
        public void CheckAndRestoreNoAds()
        {
            if (m_StoreController != null)
            {
                string[] noAdsProducts = { PRODUCT_NO_ADS, PRODUCT_FIRST_SALE_NO_ADS, PRODUCT_LAST_SALE_NO_ADS };
                foreach (var productId in noAdsProducts)
                {
                    var product = m_StoreController.products.WithID(productId);
                    if (product != null && product.hasReceipt)
                    {
                        GamePrefs.IsNoAds = true;
                        Debug.Log($"[IAP] No Ads restored successfully! Product: {productId}");
                        return; // Found one, no need to check others
                    }
                }
            }
        }
        
        /// <summary>
        /// Get the localized price for No Ads
        /// </summary>
        
        // public float GetPrice(string productID)
        // {
        //     if (m_StoreController != null)
        //     {
        //         var product = m_StoreController.products.WithID(productID);
        //         if (product != null)
        //         {
        //             return (float)product.metadata.localizedPrice;
        //         }
        //     }
        //     return 0f;
        // }
        public float GetNoAdsPrice()
        {
            return GetPrice(PRODUCT_NO_ADS);
        }
        public string GetNoAdsPriceString()
        {
            return GetPriceString(PRODUCT_NO_ADS);
        }
        public float GetFirstSalePrice()
        {
            return GetPrice(PRODUCT_FIRST_SALE_NO_ADS);
        }
        public string GetFirstSalePriceString()
        {
            return GetPriceString(PRODUCT_FIRST_SALE_NO_ADS);
        }
        public float GetLastSalePrice()
        {
            return GetPrice(PRODUCT_LAST_SALE_NO_ADS);
        }
        public string GetLastSalePriceString()
        {
            return GetPriceString(PRODUCT_LAST_SALE_NO_ADS);
        }

        public string GetCurrencyString()
        {
            return GetCurrencyCode(PRODUCT_NO_ADS);
        }
    }
}
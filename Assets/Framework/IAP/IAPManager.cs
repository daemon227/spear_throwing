using System;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace GameIAP
{
    public abstract class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        [SerializeField]
        private bool useFakeStore;
        [SerializeField]
        private bool isShowLog;
        [SerializeField]
        private bool isTesting = false;

        public static IStoreController m_StoreController;
        private static IExtensionProvider m_StoreExtensionProvider;

        private Dictionary<string, Product> _pendingPurchases = new Dictionary<string, Product>();
        private Product _lastPurchasedProduct;
        private bool _isWaitingPendingPurchase = false;
        private float _lastSuccessTime;

        protected float _lastPurchasePrice;
        protected string _lastPurchaseCurrency;

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public float GetPrice(string productID)
        {
            if (!IsInitialized())
                return  9.99f;
            var product = m_StoreController.products.WithID(productID);
            if (product != null)
            {
                return (float)product.metadata.localizedPrice;
            }
            return 9.99f;
        }
        public string GetPriceString(string productID)
        {
            if (!IsInitialized())
                return "$9.99";
            var product = m_StoreController.products.WithID(productID);
            if (product != null)
            {
                return product.metadata.localizedPriceString;
            }

            return "$9.99";
        }

        public string GetCurrencySymbolFromCode(string isoCurrencyCode)
        {
            CultureInfo cultureInfo = CultureInfo.GetCultures(CultureTypes.SpecificCultures).FirstOrDefault(culture => new RegionInfo(culture.Name).ISOCurrencySymbol == isoCurrencyCode);
            return cultureInfo != null ? new RegionInfo(cultureInfo.Name).CurrencySymbol : isoCurrencyCode;
        }

        public string GetCurrencyCode(string productID)
        {
            if (!IsInitialized())
                return "$";
            var product = m_StoreController.products.WithID(productID);
            if (product != null)
            {
                return product.metadata.isoCurrencyCode;
            }

            return "$";
        }

        public ProductMetadata GetProductMetadata(string productID)
        {
            if (!IsInitialized())
                return null;
            var product = m_StoreController.products.WithID(productID);
            if (product != null)
            {
                return product.metadata;
            }

            return null;
        }

        protected async void Initialize()
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName("production");
                await UnityServices.InitializeAsync(options);
                InitializePurchasing();
            }
            catch (Exception e)
            {
                Debug.LogError($"UGS initialization failed: {e.StackTrace}");
            }
        }

        public virtual void InitializePurchasing()
        {
            if (IsInitialized())
            {
                return;
            }
#if UNITY_EDITOR
            if (useFakeStore)
            {
                StandardPurchasingModule.Instance().useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
                StandardPurchasingModule.Instance().useFakeStoreAlways = true;
            }
#endif

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            AddProducts(builder);

            //Debug.Log($"Initializing Unity IAP with {builder.products.Count} products");
            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
            UnityPurchasing.Initialize(this, builder);
        }

        protected virtual void AddProducts(ConfigurationBuilder builder)
        {

        }

        private bool IsInitialized()
        {
            // Only say we are initialized if both the Purchasing references are set.
            return m_StoreController != null && m_StoreExtensionProvider != null;
        }

        public virtual void BuyProduct(string productId)
        {
            if (_isWaitingPendingPurchase) return;

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                //GameEvents.ShowNoticeFloat?.Invoke("No Internet");
                // EventManager.ShowNoticeString?.Invoke("No Internet");
                return;
            }

            if (isTesting)
            {
                InvokeOnItemPurchased(productId, true);
                return;
            }

            if (IsInitialized())
            {
                Product product = m_StoreController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    if (isShowLog)
                        Debug.Log(string.Format("[IAP] Purchasing product asychronously: '{0}'", product.definition.id));
                    if (_pendingPurchases.ContainsKey(productId))
                    {
                        if (isShowLog)
                        Debug.Log(string.Format("[IAP] ConfirmPendingPurchase : '{0}'", productId));   
                        StartCoroutine(IEBuyPendingPurchase(product));
                    }
                    else
                    {
                        m_StoreController.InitiatePurchase(product);
                        // IAPUiPay: Màn hình thanh toán native đã hiện lên
                        AnalyticIAP.IAPUiPay(GlobalValues.IAP_Location, ParameterValue.no_ads_popup, GetPrice(product.definition.id), GameIAPManager.Instance.GetCurrencyString(), productId);
                    }
                }
                else
                {
                    if (isShowLog)
                        Debug.Log("[IAP]  BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    InvokeOnItemPurchased(productId, false);
                }
            }
            else
            {
                if (isShowLog)
                    Debug.Log("[IAP]  BuyProductID FAIL. Not initialized.");
                InvokeOnItemPurchased(productId, false);
            }
        }

        private IEnumerator IEBuyPendingPurchase(Product product)
        {
            _isWaitingPendingPurchase = true;
            foreach (var purchase in _pendingPurchases.Values)
            {
                m_StoreController.ConfirmPendingPurchase(purchase);
            }
            yield return new WaitForSecondsRealtime(.5f);
            _isWaitingPendingPurchase = false;
            m_StoreController.InitiatePurchase(product);
        }

        protected virtual void InvokeOnItemPurchased(string id, bool success)
        {

        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            if (isShowLog)
                Debug.Log("[IAP] OnInitialized: PASS");
            m_StoreController = controller;
            m_StoreExtensionProvider = extensions;
            Debug.Log("--------[IAP]----------");
            string str = string.Empty;
            foreach (var item in m_StoreController.products.all)
            {
                str += item.definition.id + " - " + item.metadata.localizedTitle + " - " + item.metadata.localizedPriceString + " - " + item.availableToPurchase + " - " + item.hasReceipt + "\n";
            }
            Debug.Log(str);
            Debug.Log("------------------");
        }


        public void OnInitializeFailed(InitializationFailureReason error)
        {
            // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
            if (isShowLog)
                Debug.Log("[IAP] OnInitializeFailed InitializationFailureReason:" + error);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log("[IAP] Parameter " + args);
            
            // IAPPaying: User đã bấm thanh toán và giao dịch đang được xử lý
            AnalyticIAP.IAPPaying(GlobalValues.IAP_Location, ParameterValue.no_ads_popup, GetPrice(args.purchasedProduct.definition.id), GameIAPManager.Instance.GetCurrencyString(), args.purchasedProduct.definition.id);
            
            _lastSuccessTime = Time.time;
            _lastPurchasedProduct = args.purchasedProduct;
            _lastPurchasePrice = (float)args.purchasedProduct.metadata.localizedPrice;
            _lastPurchaseCurrency = args.purchasedProduct.metadata.isoCurrencyCode;
            _pendingPurchases.Remove(args.purchasedProduct.definition.id);
            InvokeOnItemPurchased(args.purchasedProduct.definition.id, true);
            return PurchaseProcessingResult.Complete;
        }
        // Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
        // Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        public void RestorePurchases()
        {
            // If Purchasing has not yet been set up ...
            if (!IsInitialized())
            {
                // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }

            // If we are running on an Apple device ... 
            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                // ... begin restoring purchases
                Debug.Log("RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((result, msg) =>
                {
                    // no purchases are available to be restored.
                    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore." + msg);
                });
            }
            // Otherwise ...
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }


        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
            // this reason with the user to guide their troubleshooting actions.

            if (isShowLog)
                Debug.Log(string.Format("[IAP] OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason.ToString()));
            OnPurchaseFailed(product.definition.id);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            if (isShowLog)
                Debug.Log("[IAP] OnInitializeFailed InitializationFailureReason:" + error);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            if (isShowLog)
                Debug.Log(string.Format("[IAP] OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureDescription.message));
            var id = product.definition.id;
            OnPurchaseFailed(id);
        }

        private void OnPurchaseFailed(string id)
        {
            if (Time.time < _lastSuccessTime + .5f && id.Equals(_lastPurchasedProduct.definition.id))
            {
                if (_pendingPurchases.ContainsKey(id))
                    _pendingPurchases[id] = _lastPurchasedProduct;
                else _pendingPurchases.Add(id, _lastPurchasedProduct);
            }
            InvokeOnItemPurchased(id, false);
        }
        /// <summary>
        /// iOS Specific.
        /// This is called as part of Apple's 'Ask to buy' functionality,
        /// when a purchase is requested by a minor and referred to a parent
        /// for approval.
        /// 
        /// When the purchase is approved or rejected, the normal purchase events
        /// will fire.
        /// </summary>
        /// <param name="item">Item.</param>
        private void OnDeferred(Product item)
        {
            Debug.Log("Purchase deferred: " + item.definition.id);
        }
    }

    // [Serializable]
    // public class IAPItem
    // {
    //     public string ProductID;
    //     public ProductType ProductType;
    //     public float Price;
    // }
}
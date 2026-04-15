using System.Collections;
using GameBase;
using UnityEngine;

namespace FoodMaster
{
    public class InitAssetsScript : MonoBehaviour
    {
        // [SerializeField] private FeatureUnlockData _featureUnlockData;
        [SerializeField] private ListElementData _listElementData;
        // [SerializeField] private ItemListData _itemListData;

        private bool _loading = false;

        private void Awake()
        {
            GameEvents.OnRemoteConfigLoaded += OnRemoteConfigLoaded;

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            GameEvents.OnRemoteConfigLoaded -= OnRemoteConfigLoaded;
        }

        private void Start()
        {
            if (RemoteConfig.Loaded)
            {
                OnRemoteConfigLoaded();
            }
        }

        private void OnRemoteConfigLoaded()
        {
            if (_loading) return;
            _loading = true;

            StartCoroutine(IELoadAssets());
        }

        private IEnumerator IELoadAssets()
        {
            // _featureUnlockData.List.Sort((f1, f2) => f1.Level - f2.Level);

            // yield return _itemListData.LoadItemData();

            while (!RemoteConfig.Loaded) yield return null;


            GlobalValues.InitAssetsReady = true;
        }
    }
}

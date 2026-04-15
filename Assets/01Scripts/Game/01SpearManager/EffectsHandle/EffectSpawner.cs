using System.Collections;
using System.Collections.Generic;
using DongA.SpearManager;
using UnityEngine;
using static GameEvents;

public class EffectSpawner : MonoBehaviour
{
    [Header("Wall Effects")]
    [SerializeField] private GameObject wallBreakEffectPrefab;
    [SerializeField] private GameObject breakEffectPrefab;
    [SerializeField] private int poolSize = 20;
    [SerializeField] private float autoHideDelay = 2f;

    [Header("Powerup Effects")]
    [SerializeField] private GameObject freezeEffectObject;
    [SerializeField] private GameObject cloneEffectObject;
    [SerializeField] private GameObject fireEffectObject;
    [SerializeField] private GameObject thunderEffectObject;

    [Header("Other Effects")]
    [SerializeField] private GameObject iceBreakEffectPrefab;

    // Pool cho hiệu ứng vỡ tường
    private readonly List<GameObject> wallBreakPool = new List<GameObject>();
    private readonly List<GameObject> breakEffectPool = new List<GameObject>();
    private readonly Dictionary<GameObject, Coroutine> hideCoroutines = new Dictionary<GameObject, Coroutine>();
    private GameObject powerupEffectInstance;
    private int wallNextIndex = 0;
    private int breakEffectNextIndex = 0;
    private readonly List<GameObject> _dynamicEffects = new List<GameObject>();
    private void Awake()
    {
        InitializePool(wallBreakEffectPrefab, poolSize, wallBreakPool, hideCoroutines, ref wallNextIndex);
        InitializePool(breakEffectPrefab, poolSize, breakEffectPool, hideCoroutines, ref breakEffectNextIndex);
        // InitializePool(deadEffectPrefab, poolSize, deadEffectPool, deadHideCoroutines, deadNextIndex);
    }
    void OnEnable()
    {
        GameEvents.OnLevelLoadComplete += DisableEffect;
        GameEvents.OnWallBreakEffect += SpawnWallBreakEffect;
        GameEvents.OnSpearHitWallEffect += SpawnBreakEffect;
        GameEvents.OnDeadEffect += SpawnDeadEffect;
        GameEvents.OnSpawnEffect += EffectShowHandle;

        GameEvents.OnRecivePowerup += OnShowPowerupEffect;
        GameEvents.OnLevelStartLoading += OnLevelStartLoading;

    }
    void OnDisable()
    {
        GameEvents.OnLevelLoadComplete -= DisableEffect;
        GameEvents.OnWallBreakEffect -= SpawnWallBreakEffect;
        GameEvents.OnSpearHitWallEffect -= SpawnBreakEffect;
        GameEvents.OnDeadEffect -= SpawnDeadEffect;
        GameEvents.OnSpawnEffect -= EffectShowHandle;

        GameEvents.OnRecivePowerup -= OnShowPowerupEffect;
        GameEvents.OnLevelStartLoading -= OnLevelStartLoading;
    }

#region Helper Methods

    private void InitializePool(GameObject effectPrefab, int poolSize, List<GameObject> pool, Dictionary<GameObject, Coroutine> coroutines, ref int nextIndex)
    {
        pool.Clear();
        coroutines.Clear();
        nextIndex = 0;

        if (effectPrefab == null)
        {
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            var instance = Instantiate(effectPrefab, transform);
            instance.SetActive(false);
            pool.Add(instance);
        }
    }
    private GameObject GetAvailableEffect(List<GameObject> effectObjects, ref int index)
    {
        if (effectObjects.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < effectObjects.Count; i++)
        {
            int currentIndex = (index + i) % effectObjects.Count;
            var candidate = effectObjects[currentIndex];
            if (!candidate.activeSelf)
            {
                index = (currentIndex + 1) % effectObjects.Count;
                return candidate;
            }
        }

        // Nếu tất cả đang active, lấy tiếp theo theo vòng tròn
        var fallback = effectObjects[index];
        index = (index + 1) % effectObjects.Count;
        return fallback;
    }

    private IEnumerator HideAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.SetActive(false);
        hideCoroutines.Remove(effect);
    }
#endregion

    private void SpawnWallBreakEffect(Vector3 position)
    {
        var effect = GetAvailableEffect(wallBreakPool, ref wallNextIndex);
        if (effect == null)
        {
            return;
        }

        effect.transform.position = position;
        effect.transform.rotation = Quaternion.identity;
        effect.SetActive(true);

        var effectMain = effect.GetComponent<ParticleSystem>();
        effectMain.Play();

        if (hideCoroutines.TryGetValue(effect, out var running))
        {
            StopCoroutine(running);
            hideCoroutines.Remove(effect);
        }

        hideCoroutines[effect] = StartCoroutine(HideAfterDelay(effect, autoHideDelay));
    }

    private void SpawnBreakEffect(WallSide side ,Vector3 position)
    {
        var effect = GetAvailableEffect(breakEffectPool, ref breakEffectNextIndex);
        if (effect == null)
        {
            return;
        }
        effect.transform.position = position;
        switch (side)
        {
            case WallSide.Left:
                effect.transform.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case WallSide.Right:
                effect.transform.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case WallSide.Top:
                effect.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case WallSide.Bottom:
                effect.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            default:
                break;
        }
        effect.SetActive(true);
    }
    private void SpawnDeadEffect(Vector3 position, GameObject bloodEffect, Color bloodEffectColor, GameObject soulPrefab)
    {
        // if (!isFirstCollision)
        // {
        //     isFirstCollision = true;
        //     return;
        // }
        //EffectShowHandle(bloodEffect, position, bloodEffectColor);
        ShowBloodEffect(bloodEffect, position, bloodEffectColor);
        if (GlobalValues.ShowEnemeySoul)
        {
            EffectShowHandle(soulPrefab, position);
        }
    }
    

    private void EffectShowHandle(GameObject prefab, Vector3 position, Color effectColor = default)
    {
        var effect = Instantiate(prefab, transform);
        _dynamicEffects.Add(effect);

        effect.transform.position = position;
        effect.transform.rotation = Quaternion.identity;
        effect.SetActive(true);

        var effectMain = effect.GetComponent<ParticleSystem>();
        if (effectMain != null)
        {
            //effectMain.startColor = effectColor;
            var mainModule = effectMain.main;
            if (effectColor != default)
            {
                mainModule.startColor = effectColor;
            }
            effectMain.Play();
        }

        StartCoroutine(DeSpawnEffect(effect, autoHideDelay));
    }
    private void ShowBloodEffect(GameObject prefab, Vector3 position, Color effectColor)
    {
        var effect = Instantiate(prefab, transform);
        _dynamicEffects.Add(effect);

        effect.transform.position = position;
        effect.transform.rotation = Quaternion.identity;
        effect.SetActive(true);

        ParticleSystem[] allParticles = effect.GetComponentsInChildren<ParticleSystem>();

        foreach (var ps in allParticles)
        {
            var main = ps.main;
            main.startColor = effectColor;
        }

        StartCoroutine(DeSpawnEffect(effect, autoHideDelay));
    }
    private IEnumerator DeSpawnEffect(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        _dynamicEffects.Remove(effect);
        Destroy(effect);
    }

    private void DisableEffect()
    {
        foreach (var effect in wallBreakPool)
        {
            effect.SetActive(false);
        }
        foreach (var effect in breakEffectPool)
        {
            effect.SetActive(false);
        }
    }
    
    private void OnShowPowerupEffect(PowerupType type)
    {
        GameObject effectObject = null;
        switch (type)
        {
            case PowerupType.Ice:
                effectObject = freezeEffectObject;
                break;
            case PowerupType.Clone:
                effectObject = cloneEffectObject;
                break;
            case PowerupType.Fire:
                effectObject = fireEffectObject;
                break;
            case PowerupType.Thunder:
                effectObject = thunderEffectObject;
                break;
            default:
                return;
        }

        if (effectObject != null)
        {
            if (powerupEffectInstance != null)
            {
                OnDestroyPowerupEffect();
            }
            powerupEffectInstance = effectObject;
            powerupEffectInstance.SetActive(true);

            var effectMain = powerupEffectInstance.GetComponent<ParticleSystem>();
            if (effectMain != null)
            {
                effectMain.Play();
            }
        }
    }

    private void OnLevelStartLoading()
    {
        OnDestroyPowerupEffect();
        DestroyAllDynamicEffects();
    }

    private void OnDestroyPowerupEffect()
    {
        if (powerupEffectInstance != null)
        {
            powerupEffectInstance.SetActive(false);
            var effectMain = powerupEffectInstance.GetComponent<ParticleSystem>();
            if (effectMain != null)
            {
                effectMain.Stop();
            }
            powerupEffectInstance = null;
        }
    }

    private void DestroyAllDynamicEffects()
    {
        StopAllCoroutines();
        foreach (var effect in _dynamicEffects)
        {
            if (effect != null) Destroy(effect);
        }
        _dynamicEffects.Clear();
    }
}

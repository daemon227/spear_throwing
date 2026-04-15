
using UnityEngine;

public class IceObject : MonoBehaviour
{
    [SerializeField] private GameObject iceBreakEffectPrefab;
    // void OnDestroy()
    // {
    //     GameEvents.OnSpawnEffect?.Invoke(iceBreakEffectPrefab, transform.position, Color.white);
    // }
    void OnDisable()
    {
        GameEvents.OnSpawnEffect?.Invoke(iceBreakEffectPrefab, transform.position, Color.white);
    }
    
}

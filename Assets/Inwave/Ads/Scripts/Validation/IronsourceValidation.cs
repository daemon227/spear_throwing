using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronsourceValidation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
#if IRON_SOURCE
        Debug.Log("unity-script: IronSource.Agent.validateIntegration");
        IronSource.Agent.validateIntegration();
#endif
    }

}

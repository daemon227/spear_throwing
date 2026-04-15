using System.ComponentModel;
using UnityEngine;

public partial class SROptions
{
    [Category("Utilities")] // Options will be grouped by category
    public void ClearPlayerPrefs()
    {
        Debug.Log("Clearing PlayerPrefs");
        PlayerPrefs.DeleteAll();
    }

}
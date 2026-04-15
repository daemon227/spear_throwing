using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class AFUpdatePlist
{
    private static readonly string _skAdNetworkItems      = "SKAdNetworkItems";
    private static readonly string _skAdNetworkIdentifier = "SKAdNetworkIdentifier";
    public static           int    totalSkAdNet;
    
    [PostProcessBuildAttribute]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {

        if (target == BuildTarget.iOS)
        {
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;
            rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");
            Debug.Log("[PostProcessBuild] NSAdvertisingAttributionReportEndpoint Added in info.plist");
            
            // Check if SKAdNetworkItems already exists
            PlistElementArray skAdNetworkItems = null;
            if (rootDict.values.TryGetValue(_skAdNetworkItems, out PlistElement value)) {
                try {
                    skAdNetworkItems = value as PlistElementArray;
                } catch (Exception e) {
                    Debug.LogError($"[PostProcessBuild] Could not obtain SKAdNetworkItems PlistElementArray: {e.Message}");
                }
            }

            // If not exists, create it
            skAdNetworkItems ??= rootDict.CreateArray(_skAdNetworkItems);
            List<PlistElement> plistElements = skAdNetworkItems.values;

            // Add SKAdNetwork ID

            List<string> listSkAd = new();
            listSkAd.AddRange(GetSkAdFromPath(Application.dataPath + "/Inwave/Appsflyer/Editor/skadnetwork.txt"));

            //Start add
            listSkAd = listSkAd.Distinct().ToList();
            for (int index = 0, size = listSkAd.Count; index < size; index++) {
                string skAdNetwork = listSkAd[index];
                if (string.IsNullOrWhiteSpace(skAdNetwork)) {
                    continue;
                }

                if (IsContainsSkAd(plistElements, skAdNetwork)) {
                    continue;
                }

                skAdNetworkItems.AddDict().SetString(_skAdNetworkIdentifier, skAdNetwork);
            }

            totalSkAdNet = skAdNetworkItems.values.Count;
            Debug.Log("[PostProcessBuild] Total: " + totalSkAdNet + " SKAdNetworkItems in info.plist");
            

            File.WriteAllText(plistPath, plist.WriteToString());

            Debug.Log("[PostProcessBuild] Info.plist updated");
        }
    }
    private static List<string> GetSkAdFromPath(string path) {
        List<string> listSkAdNetwork = new(File.ReadAllLines(path.Replace(@"\","/")));
        Debug.Log($"[PostProcessBuild] Added {listSkAdNetwork.Count} SKAdNetworkItems from {path}");
        return listSkAdNetwork;
    }
    private static bool IsContainsSkAd(List<PlistElement> plistElements, string skAdNetwork) {
        foreach (PlistElement plistElement in plistElements) {
            IDictionary<string, PlistElement> variableValues = ((PlistElementDict) plistElement).values;
            string asString = variableValues[_skAdNetworkIdentifier].AsString();
            if (asString == skAdNetwork) {
                return true;
            }
        }
        return false;
    }
}
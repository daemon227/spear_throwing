using System;
using System.Reflection;
using UnityEditor;

public static class SearchGameData {
    [MenuItem("Tools/GameData")]
    public static void SearchItemData()
    {
        // Get the internal ProjectBrowser type
        var projectBrowserType = Type.GetType("UnityEditor.ProjectBrowser, UnityEditor");
        if (projectBrowserType == null) {
            UnityEngine.Debug.LogError("Could not find ProjectBrowser type.");
            return;
        }

        // Get or open the Project window
        var projectWindow = EditorWindow.GetWindow(projectBrowserType);

        // Find the internal SetSearch method
        var setSearchMethod = projectBrowserType.GetMethod("SetSearch", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(string) }, null);

        if (setSearchMethod == null) {
            UnityEngine.Debug.LogError("Could not find SetSearch method.");
            return;
        }

        // Call SetSearch with your filter string
        setSearchMethod.Invoke(projectWindow, new object[] { "data t:GameData" });
    }
}

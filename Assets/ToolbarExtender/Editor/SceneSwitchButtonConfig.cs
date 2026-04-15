using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;
using UnityEditor.SceneManagement;

internal static class ToolbarStyles {
    public static readonly GUIStyle commandButtonStyle;

    static ToolbarStyles() {
        commandButtonStyle = new GUIStyle("Command") {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,
            imagePosition = ImagePosition.ImageAbove,
            fontStyle = FontStyle.Normal,
            fixedWidth = 120f
        };
    }
}

[InitializeOnLoad]
public class SceneSwitchButtonConfig {
    static SceneSwitchButtonConfig() {
        ToolbarExtender.LeftToolbarGUI.Add(OnLeftToolbarGUI);
        ToolbarExtender.RightToolbarGUI.Add(OnRightToolbarGUI);
    }

    private static void OnLeftToolbarGUI() {
        GUILayout.FlexibleSpace();

        //if (GUILayout.Button(new GUIContent(UISupport.Decor, GetToolTipText(UISupport.Decor)), ToolbarStyles.commandButtonStyle))
        //{
        //    EditorUtility.FocusProjectWindow();
        //    var dataObject = AssetDatabase.LoadAssetAtPath(UISupport.PathDecorData, typeof(ScriptableObject));
        //    Selection.activeObject = dataObject;
        //}
    }
    
    private static void OnRightToolbarGUI() {
        if (GUILayout.Button(new GUIContent("Init", GetToolTipText("Init")), ToolbarStyles.commandButtonStyle)) {
            SceneHelper.OpenScene("Init");
        }
        if (GUILayout.Button(new GUIContent("Home", GetToolTipText("Home")), ToolbarStyles.commandButtonStyle)) {
            SceneHelper.OpenScene("Home");
        }
        if (GUILayout.Button(new GUIContent("Game", GetToolTipText("Game")), ToolbarStyles.commandButtonStyle)) {
            SceneHelper.OpenScene("Game");
        }
        if (GUILayout.Button(new GUIContent("Design", GetToolTipText("Design")), ToolbarStyles.commandButtonStyle)) {
            SceneHelper.OpenScene("Design");
        }
    }

    private static string GetToolTipText(string sceneName) {
        return "Open " + sceneName + " scene";
    }
}

internal static class SceneHelper {
    private static string sceneToOpen;

    public static void OpenScene(string sceneName) {
        if (EditorApplication.isPlaying) {
            EditorApplication.isPlaying = false;
        }

        sceneToOpen = sceneName;
        EditorApplication.update += OnUpdate;
    }

    private static void OnUpdate() {
        if (sceneToOpen == null ||
            EditorApplication.isPlaying || EditorApplication.isPaused ||
            EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode) {
            return;
        }

        EditorApplication.update -= OnUpdate;

        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
            // need to get scene via search because the path to the scene
            // file contains the package version so it'll change over time
            string[] guids = AssetDatabase.FindAssets("t:scene " + sceneToOpen, null);
            if (guids.Length == 0) {
                Debug.LogWarning("Couldn't find scene file");
            }
            else {
                string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                EditorSceneManager.OpenScene(scenePath);
            }
        }

        sceneToOpen = null;
    }
}
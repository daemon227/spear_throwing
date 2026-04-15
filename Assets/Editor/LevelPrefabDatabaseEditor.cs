using System.Collections.Generic;
using DongA.Levels;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Automatically populates a <see cref="LevelPrefabDatabase"/> asset
/// by scanning the same prefab folders and in the same order as
/// <see cref="LevelEditor"/>, so indices always match.
/// </summary>
[CustomEditor(typeof(LevelPrefabDatabase))]
public class LevelPrefabDatabaseEditor : OdinEditor
{
    private const string PREFAB_ROOT = "Assets/02Prefabs";

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Odin vẽ inspector

        EditorGUILayout.Space(8);

        GUI.backgroundColor = new Color(0.3f, 0.85f, 0.4f);
        if (GUILayout.Button("Auto-Populate from Prefab Folders", GUILayout.Height(30)))
        {
            AutoPopulate();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.HelpBox(
            "Click the button above to scan the same prefab folders used by the Level Editor " +
            "(Assets/02Prefabs/Spear, Characters, Obstacles, Powerups) and fill the lists " +
            "with the exact same index order. This ensures runtime indices match editor indices.",
            MessageType.Info);
    }


    private void AutoPopulate()
    {
        var db = (LevelPrefabDatabase)target;
        Undo.RecordObject(db, "Auto-Populate LevelPrefabDatabase");

        db.spearPrefabs = ScanFolderWithId($"{PREFAB_ROOT}/Spear");
        db.characterPrefabs = ScanFolderWithId($"{PREFAB_ROOT}/Characters");
        db.obstaclePrefabs = ScanFolderWithId($"{PREFAB_ROOT}/Obstacles");
        db.powerUpPrefabs = ScanFolderWithId($"{PREFAB_ROOT}/Powerups");

        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"[LevelPrefabDatabase] Auto-populated: " +
                  $"Spear={db.spearPrefabs.Count}, " +
                  $"Characters={db.characterPrefabs.Count}, " +
                  $"Obstacles={db.obstaclePrefabs.Count}, " +
                  $"PowerUps={db.powerUpPrefabs.Count}");
    }

    /// <summary>
    /// Scan a folder for prefabs and assign id tự động (0,1,2,...)
    /// </summary>
    private static List<LevelPrefabDatabase.PrefabEntry> ScanFolderWithId(string folderPath)
    {
        var result = new List<LevelPrefabDatabase.PrefabEntry>();
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            Debug.LogWarning($"[LevelPrefabDatabase] Folder not found: {folderPath}");
            return result;
        }
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
        int id = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                result.Add(new LevelPrefabDatabase.PrefabEntry { id = id++, prefab = prefab });
        }
        return result;
    }

    /// <summary>
    /// Scan a folder for prefabs using the same logic as LevelEditor.ScanFolder:
    /// AssetDatabase.FindAssets("t:Prefab") with sequential indexing.
    /// </summary>
    // Xoá hàm cũ ScanFolder vì không còn dùng nữa
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AllLevelSO))]
public class AllLevelSOEditor : OdinEditor
{
    private const string LEVELS_JSON_FOLDER   = "Assets/03Datas/Levels_Json";
    private const string LEVELS_PREFAB_FOLDER = "Assets/03Datas/Levels_Prefabs";

    private string _filePrefix = "C";
    private int _startID = 0;
    private bool _clearBeforeImport = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Auto Import Levels from JSON", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        _filePrefix = EditorGUILayout.TextField("File Prefix", _filePrefix);
        EditorGUILayout.HelpBox(
            $"Sẽ import các file có tên bắt đầu bằng \"{_filePrefix}\" trong thư mục:\n{LEVELS_JSON_FOLDER}\n" +
            $"Ví dụ: {_filePrefix}1.json, {_filePrefix}2.json, {_filePrefix}10.json, ...",
            MessageType.Info);

        _startID = EditorGUILayout.IntField("Start Level ID", _startID);
        _clearBeforeImport = EditorGUILayout.Toggle("Xóa list cũ trước khi import", _clearBeforeImport);

        EditorGUILayout.Space(4);

        // Preview
        var matchedFiles = FindMatchingFiles(_filePrefix);
        EditorGUILayout.LabelField($"Tìm thấy: {matchedFiles.Count} file", EditorStyles.miniLabel);

        if (matchedFiles.Count > 0)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            int previewId = _startID;
            foreach (var f in matchedFiles.Take(10))
            {
                EditorGUILayout.LabelField($"  ID {previewId}  ←  {Path.GetFileName(f)}");
                previewId++;
            }
            if (matchedFiles.Count > 10)
                EditorGUILayout.LabelField($"  ... và {matchedFiles.Count - 10} file nữa");
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(4);

        GUI.backgroundColor = new Color(0.3f, 0.7f, 1f);
        if (GUILayout.Button($"Import {matchedFiles.Count} file JSON", GUILayout.Height(32)))
        {
            ImportFiles(matchedFiles);
        }
        GUI.backgroundColor = Color.white;
    }

    private List<string> FindMatchingFiles(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return new List<string>();

        if (!AssetDatabase.IsValidFolder(LEVELS_JSON_FOLDER))
            return new List<string>();

        string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { LEVELS_JSON_FOLDER });

        var result = new List<(string path, int number)>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);

            if (!fileName.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase))
                continue;

            string remainder = fileName.Substring(prefix.Length);

            if (int.TryParse(remainder, out int num))
            {
                result.Add((assetPath, num));
            }
        }

        // Sort by number so C1, C2, ... C10, C11 are in order
        result.Sort((a, b) => a.number.CompareTo(b.number));
        return result.Select(r => r.path).ToList();
    }

    private void ImportFiles(List<string> filePaths)
    {
        if (filePaths.Count == 0)
        {
            EditorUtility.DisplayDialog("Import Levels", "Không tìm thấy file nào phù hợp!", "OK");
            return;
        }

        var so = (AllLevelSO)target;
        Undo.RecordObject(so, "Import Level JSONs");

        if (_clearBeforeImport || so.AllLevelData == null)
            so.AllLevelData = new List<LevelDataWrapper>();

        int id = _startID;
        int imported = 0;

        foreach (string path in filePaths)
        {
            var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
            if (textAsset == null) continue;

            string baseName = Path.GetFileNameWithoutExtension(path);
            string prefabPath = $"{LEVELS_PREFAB_FOLDER}/{baseName}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            so.AllLevelData.Add(new LevelDataWrapper
            {
                levelID = id,
                levelJson = textAsset,
                levelPrefab = prefab
            });

            id++;
            imported++;
        }

        EditorUtility.SetDirty(so);
        AssetDatabase.SaveAssets();

        Debug.Log($"[AllLevelSO] Imported {imported} levels (prefix=\"{_filePrefix}\", IDs {_startID}..{id - 1})");
        EditorUtility.DisplayDialog("Import Levels",
            $"Đã import {imported} level thành công!\nID từ {_startID} đến {id - 1}", "OK");
    }
}

using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Spine;
using Spine.Unity;

namespace SpineTools
{
    /// <summary>
    /// Add component này lên GameObject có SkeletonAnimation hoặc SkeletonGraphic.
    /// Cho phép chọn nhiều skins và apply (Editor + PlayMode).
    /// </summary>
    [DisallowMultipleComponent]
    public class SpineSkinApplyTool : MonoBehaviour
    {
        [Tooltip("Preset lưu danh sách skins.")]
        public SpineSkinPresetSO preset;

        [Tooltip("Danh sách skins sẽ được combine theo thứ tự.")]
        public List<string> selectedSkins = new List<string>();

        //[Tooltip("Nếu true: tự Apply khi OnEnable (hữu ích khi Play).")]
        private bool applyOnEnable = true;

        [Tooltip("Tên skin combine runtime (chỉ là label).")]
        public string combinedSkinName = "combined-editor";

        SkeletonRenderer _renderer;

        void Awake()
        {
            CacheRenderer();
        }

        void OnEnable()
        {
            if (applyOnEnable)
                Apply();
        }

        void CacheRenderer()
        {
            if (_renderer != null) return;

            _renderer = GetComponent<SkeletonAnimation>(); 
            if (_renderer == null) _renderer = GetComponent<SkeletonRenderer>(); // fallback
        }

        public SkeletonDataAsset GetSkeletonDataAsset()
        {
            CacheRenderer();
            if (_renderer == null) return null;
            return _renderer.SkeletonDataAsset;
        }

        public bool IsValid()
        {
            CacheRenderer();
            return _renderer != null && _renderer.SkeletonDataAsset != null;
        }

        public void Apply()
        {
            CacheRenderer();
            if (!IsValid()) return;

            // Ensure initialized so Skeleton exists in Edit Mode.
            _renderer.Initialize(true);

            var skeleton = _renderer.Skeleton;
            if (skeleton == null) return;

            var data = skeleton.Data;
            if (data == null) return;

            var combined = new Skin(string.IsNullOrEmpty(combinedSkinName) ? "combined-editor" : combinedSkinName);

            for (int i = 0; i < selectedSkins.Count; i++)
            {
                var skinName = selectedSkins[i];
                if (string.IsNullOrEmpty(skinName)) continue;

                var skin = data.FindSkin(skinName);
                if (skin == null) continue;

                combined.AddSkin(skin);
            }

            skeleton.SetSkin(combined);
            skeleton.SetSlotsToSetupPose();

            // Apply pose now
            if (_renderer is SkeletonAnimation sa)
            {
                sa.AnimationState?.Apply(skeleton);
                sa.LateUpdate();
            }
             
            else
            {
                // generic SkeletonRenderer
                _renderer.LateUpdate();
            }
        }

        public void ResetToSetupPose()
        {
            CacheRenderer();
            if (!IsValid()) return;

            _renderer.Initialize(true);
            var skeleton = _renderer.Skeleton;
            if (skeleton == null) return;

            skeleton.SetSkin((Skin)null);
            skeleton.SetSlotsToSetupPose();

            if (_renderer is SkeletonAnimation sa)
            {
                sa.AnimationState?.Apply(skeleton);
                sa.LateUpdate();
            }
            
            else
            {
                _renderer.LateUpdate();
            }
        }

        public void LoadFromPreset()
        {
            if (preset == null) return;
            selectedSkins.Clear();
            selectedSkins.AddRange(preset.skins);
        }

        public void SaveToPreset()
        {
            if (preset == null) return;
            preset.skins.Clear();
            preset.skins.AddRange(selectedSkins);
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(SpineSkinApplyTool))]
    public class SpineSkinApplyToolEditor : Editor
    {
        SerializedProperty presetProp;
        SerializedProperty selectedSkinsProp;
        //SerializedProperty applyOnEnableProp;
        SerializedProperty combinedSkinNameProp;

        string[] _allSkinNames = new string[0];
        bool _skinCacheDirty = true;

        void OnEnable()
        {
            presetProp = serializedObject.FindProperty("preset");
            selectedSkinsProp = serializedObject.FindProperty("selectedSkins");
            //applyOnEnableProp = serializedObject.FindProperty("applyOnEnable");
            //combinedSkinNameProp = serializedObject.FindProperty("combinedSkinName");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var tool = (SpineSkinApplyTool)target;

            DrawHeader(tool);
            EditorGUILayout.Space(6);

            //EditorGUILayout.PropertyField(applyOnEnableProp);
            //EditorGUILayout.PropertyField(combinedSkinNameProp);
            EditorGUILayout.Space(8);

            DrawPresetSection(tool);
            EditorGUILayout.Space(8);

            DrawSkinsSection(tool);
            EditorGUILayout.Space(10);

            DrawApplyButtons(tool);

            serializedObject.ApplyModifiedProperties();
        }

        void DrawHeader(SpineSkinApplyTool tool)
        {
            EditorGUILayout.LabelField("Spine Skin Apply Tool", EditorStyles.boldLabel);

            var dataAsset = tool.GetSkeletonDataAsset();
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField("Skeleton Data Asset", dataAsset, typeof(SkeletonDataAsset), true);
            }

            if (!tool.IsValid())
            {
                EditorGUILayout.HelpBox(
                    "Không tìm thấy SkeletonAnimation hoặc SkeletonGraphic (hoặc thiếu SkeletonDataAsset).\n" +
                    "Hãy add tool này lên cùng GameObject với SkeletonAnimation / SkeletonGraphic.",
                    MessageType.Warning);
            }
        }

        void DrawPresetSection(SpineSkinApplyTool tool)
        {
            //EditorGUILayout.LabelField("Preset", EditorStyles.boldLabel);

            //EditorGUILayout.PropertyField(presetProp);

            //using (new EditorGUILayout.HorizontalScope())
            //{
            //    using (new EditorGUI.DisabledScope(presetProp.objectReferenceValue == null))
            //    {
            //        if (GUILayout.Button("Load Preset"))
            //        {
            //            Undo.RecordObject(tool, "Load Skin Preset");
            //            tool.LoadFromPreset();
            //            EditorUtility.SetDirty(tool);
            //            _skinCacheDirty = true;
            //        }

            //        if (GUILayout.Button("Save -> Preset"))
            //        {
            //            Undo.RecordObject(tool, "Save Skin Preset");
            //            tool.SaveToPreset();
            //            EditorUtility.SetDirty(tool);
            //            EditorUtility.SetDirty(tool.preset);
            //            AssetDatabase.SaveAssets();
            //        }
            //    }

            //    if (GUILayout.Button("Create Preset Asset"))
            //    {
            //        CreatePresetAsset(tool);
            //    }
            //}
        }

        void DrawSkinsSection(SpineSkinApplyTool tool)
        {
            EditorGUILayout.LabelField("Selected Skins", EditorStyles.boldLabel);

            EnsureSkinCache(tool);

            if (_allSkinNames.Length == 0)
            {
                EditorGUILayout.HelpBox("Không lấy được danh sách skins từ SkeletonDataAsset.", MessageType.Info);
                return;
            }

            // List
            for (int i = 0; i < selectedSkinsProp.arraySize; i++)
            {
                var element = selectedSkinsProp.GetArrayElementAtIndex(i);
                string current = element.stringValue;

                using (new EditorGUILayout.HorizontalScope())
                {
                    int idx = IndexOfSkin(current);
                    int newIdx = EditorGUILayout.Popup(idx, _allSkinNames);
                    string picked = _allSkinNames[Mathf.Clamp(newIdx, 0, _allSkinNames.Length - 1)];

                    if (picked != current)
                        element.stringValue = picked;

                    if (GUILayout.Button("▲", GUILayout.Width(26)) && i > 0)
                    {
                        selectedSkinsProp.MoveArrayElement(i, i - 1);
                    }
                    if (GUILayout.Button("▼", GUILayout.Width(26)) && i < selectedSkinsProp.arraySize - 1)
                    {
                        selectedSkinsProp.MoveArrayElement(i, i + 1);
                    }

                    if (GUILayout.Button("X", GUILayout.Width(26)))
                    {
                        selectedSkinsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }

            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Add Skin"))
                {
                    selectedSkinsProp.arraySize++;
                    var last = selectedSkinsProp.GetArrayElementAtIndex(selectedSkinsProp.arraySize - 1);
                    last.stringValue = _allSkinNames.Length > 0 ? _allSkinNames[0] : "";
                }

                if (GUILayout.Button("Clear"))
                {
                    selectedSkinsProp.ClearArray();
                }

                if (GUILayout.Button("Refresh Skin List"))
                {
                    _skinCacheDirty = true;
                }
            }

            EditorGUILayout.HelpBox(
                "Thứ tự quan trọng: skins ở dưới cùng sẽ được AddSkin sau và có thể override attachments của skin trước.",
                MessageType.None);
        }

        void DrawApplyButtons(SpineSkinApplyTool tool)
        {
            //using (new EditorGUI.DisabledScope(!tool.IsValid()))
            //{
            //    using (new EditorGUILayout.HorizontalScope())
            //    {
            //        if (GUILayout.Button("Apply"))
            //        {
            //            Undo.RecordObject(tool.gameObject, "Apply Spine Skins");
            //            tool.Apply();
            //            EditorUtility.SetDirty(tool);
            //        }

            //        if (GUILayout.Button("Reset Setup Pose"))
            //        {
            //            Undo.RecordObject(tool.gameObject, "Reset Spine Setup Pose");
            //            tool.ResetToSetupPose();
            //            EditorUtility.SetDirty(tool);
            //        }
            //    }
            //}
        }

        void EnsureSkinCache(SpineSkinApplyTool tool)
        {
            if (!_skinCacheDirty) return;
            _skinCacheDirty = false;

            _allSkinNames = new string[0];

            var dataAsset = tool.GetSkeletonDataAsset();
            if (dataAsset == null) return;

            // SkeletonDataAsset có thể chưa load data ngay => GetSkeletonData(true)
            var skeletonData = dataAsset.GetSkeletonData(true);
            if (skeletonData == null) return;

            var skins = skeletonData.Skins;
            if (skins == null || skins.Count == 0) return;

            var list = new List<string>(skins.Count);
            for (int i = 0; i < skins.Count; i++)
            {
                var s = skins.Items[i];
                if (s != null) list.Add(s.Name);
            }

            list.Sort();
            _allSkinNames = list.ToArray();
        }

        int IndexOfSkin(string name)
        {
            if (string.IsNullOrEmpty(name) || _allSkinNames == null) return 0;
            for (int i = 0; i < _allSkinNames.Length; i++)
                if (_allSkinNames[i] == name) return i;
            return 0;
        }

        void CreatePresetAsset(SpineSkinApplyTool tool)
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "Create Spine Skin Preset",
                "SpineSkinPreset",
                "asset",
                "Chọn nơi lưu preset."
            );

            if (string.IsNullOrEmpty(path)) return;

            var preset = ScriptableObject.CreateInstance<SpineSkinPresetSO>();
            preset.skins = new List<string>(tool.selectedSkins);

            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Undo.RecordObject(tool, "Assign Preset");
            tool.preset = preset;
            EditorUtility.SetDirty(tool);
        }
    }
#endif
}

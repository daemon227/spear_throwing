#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DongA.Characters;
using DongA.EnemySpear;
using DongA.Game;
using DongA.Obstacles;
using Game.Obstacles;
using Spine.Unity;
using UnityEditor;
using UnityEngine;

namespace DongA.Levels
{
    public class LevelEditor : EditorWindow
    {
        private const string EDITOR_PARENT_NAME = "[LevelEditor_Root]";
        private const string EDITOR_LEVEL_PREFAB_ROOT = "[LevelEditor_Prefabs]";
        private const string LEVELS_JSON_FOLDER = "Assets/03Datas/Levels_Json";

        // ──────────────────────────── Prefab catalogue ─────────────────────
        // Sử dụng trực tiếp LevelPrefabDatabase.PrefabEntry

        [Header("Prefab Database")]
        private LevelPrefabDatabase prefabDatabase;
        private AllLevelSO _allLevelSO;
        private GameObject _spawnedLevelPrefab;

        private List<LevelPrefabDatabase.PrefabEntry> _spearPrefabs => prefabDatabase != null ? prefabDatabase.spearPrefabs : new List<LevelPrefabDatabase.PrefabEntry>();
        private List<LevelPrefabDatabase.PrefabEntry> _characterPrefabs => prefabDatabase != null ? prefabDatabase.characterPrefabs : new List<LevelPrefabDatabase.PrefabEntry>();
        private List<LevelPrefabDatabase.PrefabEntry> _obstaclePrefabs => prefabDatabase != null ? prefabDatabase.obstaclePrefabs : new List<LevelPrefabDatabase.PrefabEntry>();
        private List<LevelPrefabDatabase.PrefabEntry> _powerUpPrefabs => prefabDatabase != null ? prefabDatabase.powerUpPrefabs : new List<LevelPrefabDatabase.PrefabEntry>();
        private List<LevelPrefabDatabase.PrefabEntry> _decorPrefabs => prefabDatabase != null ? prefabDatabase.decorPrefabs : new List<LevelPrefabDatabase.PrefabEntry>();
        private List<LevelPrefabDatabase.PrefabEntry> _groundSpriteShapePrefabs => prefabDatabase != null ? prefabDatabase.groundSpriteShapePrefabs : new List<LevelPrefabDatabase.PrefabEntry>();
        private List<LevelPrefabDatabase.PrefabEntry> _mouseHolePrefabs => prefabDatabase != null ? prefabDatabase.mouseHolePrefabs : new List<LevelPrefabDatabase.PrefabEntry>();

        // ──────────────────────────── GUI state ────────────────────────────
        private Vector2 _scrollPos;
        private bool _foldSpear = true;
        private bool _foldCharacter = true;
        private bool _foldObstacle = true;
        private bool _foldPowerUp = true;
        private bool _foldDecor = true;
        private bool _foldGroundSpriteShape = true;
        private bool _foldMouseHole = true;
        private int _levelIndex;
        private string _loadedFileName = "";

        // ──────────────────────────── Styles (lazy init) ───────────────────
        private GUIStyle _headerStyle;
        private GUIStyle _prefabButtonStyle;

        // ──────────────────────────── Menu entry ───────────────────────────
        [MenuItem("Tools/Level Editor %#L")]
        public static void OpenWindow()
        {
            var wnd = GetWindow<LevelEditor>("Level Editor");
            wnd.minSize = new Vector2(300, 400);
            wnd.Show();
        }

        // ──────────────────────────── Lifecycle ────────────────────────────
        private void OnEnable()
        {
            // Tự động load ScriptableObject từ đường dẫn cố định
            if (prefabDatabase == null)
            {
                prefabDatabase = AssetDatabase.LoadAssetAtPath<LevelPrefabDatabase>("Assets/04SO/DatabaseSO/LevelPrefabDatabase.asset");
                if (prefabDatabase == null)
                {
                    Debug.LogError("[LevelEditor] Không tìm thấy LevelPrefabDatabase tại Assets/04SO/DatabaseSO/LevelPrefabDatabase.asset");
                }
            }
            if (_allLevelSO == null)
            {
                var guids = AssetDatabase.FindAssets("t:AllLevelSO");
                if (guids.Length > 0)
                    _allLevelSO = AssetDatabase.LoadAssetAtPath<AllLevelSO>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
            RefreshPrefabCatalogue();
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        // ──────────────────────────── Prefab scanning ──────────────────────
        private void RefreshPrefabCatalogue()
        {
            // Không cần scan folder nữa, chỉ cần reload lại từ ScriptableObject nếu cần
            // (các list _spearPrefabs, ... đã lấy trực tiếp từ prefabDatabase)
            // Nếu muốn có thể gọi EditorUtility.SetDirty(prefabDatabase) để đảm bảo Inspector cập nhật
            Repaint();
        }
        // Xoá hàm ScanFolder vì không còn dùng nữa

        // ──────────────────────────── Editor root ──────────────────────────
        /// <summary>Get or create a root GameObject to keep editor items organized.</summary>
        private Transform GetOrCreateRoot()
        {
            GameObject root = GameObject.Find(EDITOR_PARENT_NAME);
            if (root == null)
            {
                root = new GameObject(EDITOR_PARENT_NAME);
                Undo.RegisterCreatedObjectUndo(root, "Create LevelEditor Root");
            }
            return root.transform;
        }

        // ──────────────────────────── GUI ──────────────────────────────────
        private void InitStyles()
        {
            _headerStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.9f, 0.75f, 0.2f) }
            };
            _prefabButtonStyle ??= new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 28,
                fontSize = 12
            };
        }

        private void OnGUI()
        {
            InitStyles();

            // ── Title bar ──
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Spring Spear - Level Editor", _headerStyle);
            DrawSeparator();

            // ── Action buttons row ──
            EditorGUILayout.BeginHorizontal();
            {
                GUI.backgroundColor = new Color(0.3f, 0.85f, 0.4f);
                if (GUILayout.Button("Save Level", GUILayout.Height(30)))
                    SaveLevel();

                GUI.backgroundColor = new Color(0.4f, 0.7f, 1f);
                if (GUILayout.Button("Load Level", GUILayout.Height(30)))
                    LoadLevel();

                GUI.backgroundColor = new Color(1f, 0.45f, 0.4f);
                if (GUILayout.Button("Clear All", GUILayout.Height(30)))
                    ClearAll();

                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);

            if (GUILayout.Button("Refresh Prefab List"))
                RefreshPrefabCatalogue();

            DrawSeparator();

            // ── Prefab palette ──
            EditorGUILayout.LabelField("Prefab Palette", EditorStyles.boldLabel);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            {
                DrawPrefabCategory("Spear", ref _foldSpear, _spearPrefabs, LevelEditorItem.ItemType.Spear);
                DrawPrefabCategory("Characters", ref _foldCharacter, _characterPrefabs, LevelEditorItem.ItemType.Character);
                DrawPrefabCategory("Obstacles", ref _foldObstacle, _obstaclePrefabs, LevelEditorItem.ItemType.Obstacle);
                DrawPrefabCategory("Power-Ups", ref _foldPowerUp, _powerUpPrefabs, LevelEditorItem.ItemType.PowerUp);
                DrawPrefabCategory("Decors", ref _foldDecor, _decorPrefabs, LevelEditorItem.ItemType.Decor);
                DrawPrefabCategory("Ground SpriteShapes", ref _foldGroundSpriteShape, _groundSpriteShapePrefabs, LevelEditorItem.ItemType.GroundSpriteShape);
                DrawPrefabCategory("Mouse Holes", ref _foldMouseHole, _mouseHolePrefabs, LevelEditorItem.ItemType.MouseHole);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawPrefabCategory(string label, ref bool foldout, List<LevelPrefabDatabase.PrefabEntry> entries, LevelEditorItem.ItemType itemType)
        {
            foldout = EditorGUILayout.Foldout(foldout, $"{label}  ({entries.Count})", true, EditorStyles.foldoutHeader);
            if (!foldout) return;

            EditorGUI.indentLevel++;
            foreach (var entry in entries)
            {
                EditorGUILayout.BeginHorizontal();

                // Thumbnail
                Texture2D preview = AssetPreview.GetAssetPreview(entry.prefab)
                                    ?? AssetPreview.GetMiniThumbnail(entry.prefab);
                if (preview != null)
                    GUILayout.Label(preview, GUILayout.Width(26), GUILayout.Height(26));

                // Spawn button (dùng id thay vì index, name lấy từ prefab)
                string prefabName = entry.prefab != null ? entry.prefab.name : "(null)";
                if (GUILayout.Button($"  [id:{entry.id}]  {prefabName}", _prefabButtonStyle))
                {
                    SpawnPrefab(entry, itemType);
                }

                // Drag source - lets you drag from the palette into Scene view
                Rect lastRect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDrag && lastRect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new UnityEngine.Object[] { entry.prefab };
                    DragAndDrop.SetGenericData("LevelEditorEntry", entry);
                    DragAndDrop.SetGenericData("LevelEditorItemType", itemType);
                    DragAndDrop.StartDrag(prefabName);
                    Event.current.Use();
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);
        }

        // ──────────────────────────── Spawn ────────────────────────────────
        private void SpawnPrefab(LevelPrefabDatabase.PrefabEntry entry, LevelEditorItem.ItemType itemType)
        {
            // Place at scene camera's look point
            Vector3 spawnPos = SceneView.lastActiveSceneView != null
                ? SceneView.lastActiveSceneView.camera.transform.position
                  + SceneView.lastActiveSceneView.camera.transform.forward * 5f
                : Vector3.zero;
            spawnPos.z = 0f; // 2D game

            SpawnPrefabAt(entry, spawnPos, itemType);
        }

        private GameObject SpawnPrefabAt(LevelPrefabDatabase.PrefabEntry entry, Vector3 position, LevelEditorItem.ItemType itemType)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(entry.prefab);
            instance.transform.position = position;
            instance.transform.SetParent(GetOrCreateRoot());

            // Attach metadata
            LevelEditorItem item = instance.GetComponent<LevelEditorItem>();
            if (item == null)
                item = instance.AddComponent<LevelEditorItem>();

            item.itemType = itemType;
            item.itemIndex = entry.id;
            item.prefabPath = AssetDatabase.GetAssetPath(entry.prefab);

            Undo.RegisterCreatedObjectUndo(instance, $"Spawn {entry.prefab?.name}");
            Selection.activeGameObject = instance;

            return instance;
        }

        // ──────────────────────────── Scene GUI (drag-drop receiver) ──────
        private void OnSceneGUI(SceneView sceneView)
        {
            if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
            {
                object data = DragAndDrop.GetGenericData("LevelEditorEntry");
                if (data is LevelPrefabDatabase.PrefabEntry entry)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        // Convert mouse position to world
                        Vector3 worldPos = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
                        worldPos.z = 0f;

                        var dragItemType = DragAndDrop.GetGenericData("LevelEditorItemType") is LevelEditorItem.ItemType t
                            ? t : LevelEditorItem.ItemType.Spear;
                        SpawnPrefabAt(entry, worldPos, dragItemType);
                        Event.current.Use();
                    }
                    else
                    {
                        Event.current.Use();
                    }
                }
            }

            // Draw loaded file name in top-left corner of scene view
            if (!string.IsNullOrEmpty(_loadedFileName))
            {
                Handles.BeginGUI();
                GUIStyle fileNameStyle = new(EditorStyles.boldLabel)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = new Color(0.4f, 1f, 0.6f) }
                };
                float w = 300f, h = 24f;
                float x = (sceneView.position.width - w) * 0.5f;
                GUI.Label(new Rect(x, 8, w, h), $"Level: {_loadedFileName}", fileNameStyle);
                Handles.EndGUI();
            }

            // Draw labels on all editor items
            var items = FindObjectsByType<LevelEditorItem>(FindObjectsSortMode.None);
            foreach (var item in items)
            {
                string labelText = $"[{item.itemType}:{item.itemIndex}]";
                GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    normal = { textColor = GetItemColor(item.itemType) },
                    fontSize = 11
                };
                Handles.Label(item.transform.position + Vector3.up * 0.5f, labelText, labelStyle);

                if (item.itemType != LevelEditorItem.ItemType.Character) continue;

                // Draw MoveToPoint handle for characters
                var charBase = item.GetComponent<CharacterBase>();
                if (charBase != null && charBase.CharacterMoveType == MoveType.MoveToPoint)
                {
                    Vector3 targetPos = new Vector3(charBase.MoveTargetPoint.x, charBase.MoveTargetPoint.y, 0f);

                    Handles.color = new Color(1f, 0.8f, 0f, 0.8f);
                    Handles.DrawDottedLine(item.transform.position, targetPos, 4f);

                    EditorGUI.BeginChangeCheck();
                    Vector3 newPos = Handles.FreeMoveHandle(targetPos, 0.15f, Vector3.zero, Handles.SphereHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(charBase, "Move Target Point");
                        charBase.MoveTargetPoint = new Vector2(newPos.x, newPos.y);
                        EditorUtility.SetDirty(charBase);
                    }

                    GUIStyle targetLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        normal = { textColor = new Color(1f, 0.8f, 0f) },
                        fontSize = 10
                    };
                    Handles.Label(targetPos + Vector3.up * 0.25f, "Target", targetLabelStyle);
                }

                // Draw Wizard1 point B handle (A = wizard's own position)
                var wizard1 = item.GetComponent<Wizard1>();
                if (wizard1 != null)
                {
                    Vector3 posA = item.transform.position;
                    Vector3 posB = (Vector3)wizard1.PointB;

                    // Dotted line A <-> B
                    Handles.color = new Color(0f, 1f, 1f, 0.8f);
                    Handles.DrawDottedLine(posA, posB, 4f);

                    // Handle B (yellow)
                    Handles.color = Color.yellow;
                    EditorGUI.BeginChangeCheck();
                    Vector3 newB = Handles.FreeMoveHandle(posB, 0.15f, Vector3.zero, Handles.SphereHandleCap);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(wizard1, "Move Wizard1 Point B");
                        wizard1.PointB = new Vector2(newB.x, newB.y);
                        EditorUtility.SetDirty(wizard1);
                    }

                    GUIStyle w1Style = new GUIStyle(EditorStyles.boldLabel) { fontSize = 10 };
                    w1Style.normal.textColor = Color.cyan;
                    Handles.Label(posA + Vector3.up * 0.25f, "A", w1Style);
                    w1Style.normal.textColor = Color.yellow;
                    Handles.Label(posB + Vector3.up * 0.25f, "B", w1Style);
                }
            }
        }

        private Color GetItemColor(LevelEditorItem.ItemType type)
        {
            return type switch
            {
                LevelEditorItem.ItemType.Spear => Color.yellow,
                LevelEditorItem.ItemType.Character => Color.green,
                LevelEditorItem.ItemType.Obstacle => new Color(1f, 0.6f, 0.2f),
                LevelEditorItem.ItemType.PowerUp => Color.cyan,
                LevelEditorItem.ItemType.Decor => new Color(0.8f, 0.5f, 1f),
                LevelEditorItem.ItemType.GroundSpriteShape => new Color(0.4f, 0.9f, 0.6f),
                LevelEditorItem.ItemType.MouseHole => new Color(1f, 0.5f, 0.5f),
                _ => Color.white
            };
        }

        // ──────────────────────────── Save ─────────────────────────────────
        private void SaveLevel()
        {
            LevelData levelData = CollectLevelData();

            string defaultName = $"Level_";
            string path = EditorUtility.SaveFilePanel(
                "Save Level Data",
                Path.Combine(Application.dataPath, "../", LEVELS_JSON_FOLDER),
                defaultName,
                "json");

            if (string.IsNullOrEmpty(path)) return;

            string json = JsonUtility.ToJson(levelData, true);
            File.WriteAllText(path, json);
            AssetDatabase.Refresh();

            Debug.Log($"[LevelEditor] Level saved -> {path}");
            EditorUtility.DisplayDialog("Level Editor",
                $"Level {_levelIndex} saved successfully!\n{path}", "OK");
        }

        private LevelData CollectLevelData()
        {
            // Camera cam = GetSceneCamera();

            LevelData data = new LevelData
            {
                characters = new List<CharacterData>(),
                obstacles = new List<ObstacleData>(),
                powerUps = new List<PowerUpData>(),
                enemySpears = new List<SpearData>(),
                decorObjects = new List<DecorData>(),
                groundSpriteShapes = new List<GroundSpriteShapeData>(),
                mouseHoles = new List<MouseHoleData>()
            };

            // Record design-time camera parameters so loaders can convert back
            // if (cam != null && cam.orthographic)
            // {
            //     data.designOrthoSize = cam.orthographicSize;
            //     data.designAspectRatio = cam.aspect;
            // }

            var items = FindObjectsByType<LevelEditorItem>(FindObjectsSortMode.None);

            foreach (var item in items)
            {
                Transform t = item.transform;

                switch (item.itemType)
                {
                    case LevelEditorItem.ItemType.Spear:
                        var spearRb = item.GetComponent<Rigidbody2D>();
                        var spearData = new SpearData
                        {
                            spearIndex = item.itemIndex,
                            position = item.transform.position,
                            rotation = t.eulerAngles,
                            scale = t.localScale,
                            gravityScale = spearRb != null ? spearRb.gravityScale : 0f
                        };
                        if (item.GetComponent<SpearEnemyAI>() != null)
                        {
                            data.enemySpears.Add(spearData);
                        }
                        else
                        {
                            data.hasSpear = true;
                            data.spear = spearData;
                        }
                        break;

                    case LevelEditorItem.ItemType.Character:
                        var skeletonSave = item.GetComponent<SkeletonAnimation>();
                        var charRb = item.GetComponent<Rigidbody2D>();
                        var charBase = item.GetComponent<CharacterBase>();
                        var bigChar = item.GetComponent<BigCharacter>();
                        var wizard = item.GetComponent<Wizard>();
                        var wizard1 = item.GetComponent<Wizard1>();
                        data.characters.Add(new CharacterData
                        {
                            characterIndex = item.itemIndex,
                            animationState = skeletonSave != null ? skeletonSave.AnimationName : "",
                            position = item.transform.position,
                            rotation = t.eulerAngles,
                            scale = t.localScale,
                            gravityScale = charRb != null ? charRb.gravityScale : 0f,
                            moveType = charBase != null ? charBase.CharacterMoveType : MoveType.None,
                            usePhysicsFall = charBase != null && charBase.UsePhysicsFall,
                            moveDistance = charBase != null ? charBase.MoveDistance : 0f,
                            moveDuration = charBase != null ? charBase.MoveDuration : 0f,
                            waitTime = charBase != null ? charBase.WaitTime : 0f,
                            moveTargetPoint = charBase != null ? charBase.MoveTargetPoint : Vector2.zero,
                            spawnCount = bigChar != null ? bigChar.SpawnCount : 0,
                            spawnRadius = bigChar != null ? bigChar.SpawnRadius : 0f,
                            spawnForce = bigChar != null ? bigChar.SpawnForce : 0f,
                            childSizeMultiplier = bigChar != null ? bigChar.ChildSizeMultiplier : 0f,
                            numberTeleport = wizard != null ? wizard.NumberTeleport : 0,
                            wizard1PointB = wizard1 != null ? wizard1.PointB : Vector2.zero
                        });
                        break;

                    case LevelEditorItem.ItemType.Obstacle:
                        var obstacleBase = item.GetComponent<ObstacleBase>();
                        var obstRb = item.GetComponent<Rigidbody2D>();
                        float obstGravity = obstRb != null ? obstRb.gravityScale : 0f;
                        if (obstacleBase == null)
                        {
                            data.obstacles.Add(new ObstacleData
                            {
                                obstacleIndex = item.itemIndex,
                                position = item.transform.position,
                                rotation = t.eulerAngles,
                                scale = t.localScale,
                                gravityScale = obstGravity
                            });
                        }
                        else
                        {
                            if(obstacleBase is Fan fan)
                            {
                                data.obstacles.Add(new ObstacleData
                                {
                                    obstacleIndex = item.itemIndex,
                                    position = item.transform.position,
                                    rotation = t.eulerAngles,
                                    scale = t.localScale,
                                    gravityScale = obstGravity,
                                    moveType = fan.MoveType,
                                    moveDistance = fan.MoveDistance,
                                    moveDuration = fan.MoveDuration,
                                    waitTime = fan.WaitTime,
                                    windForce = fan.WindForce,
                                    windRange = fan.ColliderSize,
                                    windTransformOffset = fan.WindAreaTransform != null ? (Vector2)fan.WindAreaTransform.localPosition : Vector2.zero
                                });
                            }
                            else if(obstacleBase is Trampoline trampoline)
                            {
                                data.obstacles.Add(new ObstacleData
                                {
                                    obstacleIndex = item.itemIndex,
                                    position = item.transform.position,
                                    rotation = t.eulerAngles,
                                    scale = t.localScale,
                                    gravityScale = obstGravity,
                                    moveType = trampoline.MoveType,
                                    moveDistance = trampoline.MoveDistance,
                                    moveDuration = trampoline.MoveDuration,
                                    waitTime = trampoline.WaitTime,
                                    jumpForce = trampoline.JumpForce
                                });
                            }
                            else if(obstacleBase is Portal portal)
                            {
                                data.obstacles.Add(new ObstacleData
                                {
                                    obstacleIndex = item.itemIndex,
                                    position = item.transform.position,
                                    rotation = t.eulerAngles,
                                    scale = t.localScale,
                                    gravityScale = obstGravity,
                                    portalID = portal.PortalID,
                                    linkedPortalID = portal.LinkedPortalID,
                                    appearDelay = portal.AppearDelay,
                                    portalMoveSpeed = portal.PortalMoveSpeed
                                });
                            }
                            else if(obstacleBase is Ballon ballon)
                            {
                                data.obstacles.Add(new ObstacleData
                                {
                                    obstacleIndex = item.itemIndex,
                                    position = item.transform.position,
                                    rotation = t.eulerAngles,
                                    scale = t.localScale,
                                    gravityScale = obstGravity,
                                    moveType = ballon.MoveType,
                                    moveDistance = ballon.MoveDistance,
                                    moveDuration = ballon.MoveDuration,
                                    waitTime = ballon.WaitTime,
                                    ballonEnemyFollowOffset = ballon.EnemyFollowTarget != null ? (Vector2)ballon.EnemyFollowTarget.localPosition : Vector2.zero
                                });
                            }
                            else if (obstacleBase is WashingMachine washingMachine)
                            {
                                data.obstacles.Add(new ObstacleData
                                {
                                    obstacleIndex = item.itemIndex,
                                    position = item.transform.position,
                                    rotation = t.eulerAngles,
                                    scale = t.localScale,
                                    gravityScale = obstGravity,
                                    moveType = washingMachine.MoveType,
                                    moveDistance = washingMachine.MoveDistance,
                                    moveDuration = washingMachine.MoveDuration,
                                    waitTime = washingMachine.WaitTime,
                                    washingMachineCharacterType = (int)washingMachine.CharacterTypeInWashingMachine,
                                    washingMachineCount = washingMachine.NumberCharacterInWashingMachine,
                                    washingMachineScale = washingMachine.ScaleCharacterInWashingMachine
                                });
                            }
                            else if (obstacleBase is Egg egg)
                            {
                                data.obstacles.Add(new ObstacleData
                                {
                                    obstacleIndex = item.itemIndex,
                                    position = item.transform.position,
                                    rotation = t.eulerAngles,
                                    scale = t.localScale,
                                    gravityScale = obstGravity,
                                    eggEnemyLocalPosition = egg.EnemyLocalPosition,
                                    eggTopMoveDistance = egg.TopEggMoveDistance,
                                    eggTopMoveDuration = egg.TopEggMoveDuration
                                });
                            }
                            else if (obstacleBase is Ball ball)
                            {
                                data.obstacles.Add(new ObstacleData
                                {
                                    obstacleIndex = item.itemIndex,
                                    position = item.transform.position,
                                    rotation = t.eulerAngles,
                                    scale = t.localScale,
                                    gravityScale = obstGravity,
                                    moveType = ball.MoveType,
                                    moveDistance = ball.MoveDistance,
                                    moveDuration = ball.MoveDuration,
                                    waitTime = ball.WaitTime,
                                    ballMoveMode = (int)ball.BallMoveModeType,
                                    ballPointB = ball.PointB,
                                    ballCurveHeight = ball.CurveHeight,
                                    ballId = ball.BallId
                                });
                            }
                            else if (obstacleBase is Vehical vehical)
                            {
                                data.obstacles.Add(new ObstacleData
                                {
                                    obstacleIndex = item.itemIndex,
                                    position = item.transform.position,
                                    rotation = t.eulerAngles,
                                    scale = t.localScale,
                                    gravityScale = obstGravity,
                                    moveType = vehical.MoveType,
                                    moveDistance = vehical.MoveDistance,
                                    moveDuration = vehical.MoveDuration,
                                    waitTime = vehical.WaitTime,
                                    vehicalMoveMode = (int)vehical.VehicalMoveModeType,
                                    vehicalPointB = vehical.PointB,
                                    vehicalCurveHeight = vehical.CurveHeight,
                                    vehicalUsePhysicsFall = vehical.UsePhysicsFall,
                                    vehicalEnemyPosOffset = vehical.EnemyPos != null ? (Vector2)vehical.EnemyPos.localPosition : Vector2.zero
                                });
                            }
                            else
                                data.obstacles.Add(new ObstacleData
                                {
                                    obstacleIndex = item.itemIndex,
                                    position = item.transform.position,
                                    rotation = t.eulerAngles,
                                    scale = t.localScale,
                                    gravityScale = obstGravity,
                                    moveType = obstacleBase.MoveType,
                                    moveDistance = obstacleBase.MoveDistance,
                                    moveDuration = obstacleBase.MoveDuration,
                                    waitTime = obstacleBase.WaitTime,
                                });
                        }
                        break;
                    case LevelEditorItem.ItemType.PowerUp:
                        var puRb = item.GetComponent<Rigidbody2D>();
                        data.powerUps.Add(new PowerUpData
                        {
                            powerUpIndex = item.itemIndex,
                            position = item.transform.position,
                            rotation = t.eulerAngles,
                            scale = t.localScale,
                            gravityScale = puRb != null ? puRb.gravityScale : 0f
                        });
                        break;

                    case LevelEditorItem.ItemType.Decor:
                        var decorSkeleton = item.GetComponentInChildren<SkeletonAnimation>();
                        data.decorObjects.Add(new DecorData
                        {
                            decorIndex = item.itemIndex,
                            position = item.transform.position,
                            rotation = t.eulerAngles,
                            scale = t.localScale,
                            animName = decorSkeleton != null ? decorSkeleton.AnimationName : ""
                        });
                        break;

                    case LevelEditorItem.ItemType.GroundSpriteShape:
                        var groundShape = item.GetComponent<DongA.Decors.GroundSpriteShape>();
                        var gssData = new GroundSpriteShapeData
                        {
                            groundSpriteShapeIndex = item.itemIndex,
                            position = item.transform.position,
                            rotation = t.eulerAngles,
                            scale = t.localScale
                        };
                        groundShape?.CollectPoints(gssData);
                        data.groundSpriteShapes.Add(gssData);
                        break;

                    case LevelEditorItem.ItemType.MouseHole:
                        var mh = item.GetComponent<MouseHole>();
                        data.mouseHoles.Add(new MouseHoleData
                        {
                            mouseHoleIndex = item.itemIndex,
                            position = t.position,
                            rotation = t.eulerAngles,
                            scale = t.localScale,
                            mouseHoleType = mh != null ? (int)mh.HoleType : 0,
                            enemyCount = mh != null ? mh.EnemyCount : 5,
                            spawnScale = mh != null ? mh.SpawnScale : 0.12f,
                            spawnPointPositions = mh != null ? mh.GetSpawnPointPositions() : new List<Vector2>()
                        });
                        break;
                }
            }
            return data;
        }
        // ──────────────────────────── Load ─────────────────────────────────
        private void LoadLevel()
        {
            string path = EditorUtility.OpenFilePanel("Load Level Data", Path.Combine(Application.dataPath, "../", LEVELS_JSON_FOLDER), "json");
            if (string.IsNullOrEmpty(path)) return;

            string json = File.ReadAllText(path);
            LevelData levelData;

            try
            {
                levelData = JsonUtility.FromJson<LevelData>(json);
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Level Editor",
                    $"Failed to parse JSON:\n{ex.Message}", "OK");
                return;
            }

            // Convert viewport positions to world positions (handles backward compat)
            // Camera cam = GetSceneCamera();
            // ConvertViewportToWorld(levelData, cam);

            // Clear existing items (no confirmation for load)
            ClearAllNoConfirm();

            // Build quick lookups: id -> PrefabEntry
            Dictionary<int, LevelPrefabDatabase.PrefabEntry> spearLookup = _spearPrefabs.ToDictionary(p => p.id);
            Dictionary<int, LevelPrefabDatabase.PrefabEntry> charLookup = _characterPrefabs.ToDictionary(p => p.id);
            Dictionary<int, LevelPrefabDatabase.PrefabEntry> obstLookup = _obstaclePrefabs.ToDictionary(p => p.id);
            Dictionary<int, LevelPrefabDatabase.PrefabEntry> powerLookup = _powerUpPrefabs.ToDictionary(p => p.id);

            // Player Spear
            if (levelData.hasSpear
                && spearLookup.TryGetValue(levelData.spear.spearIndex, out var spearEntry))
            {
                var go = SpawnPrefabAt(spearEntry, Vector3.zero, LevelEditorItem.ItemType.Spear);
                ApplyTransform(go.transform, levelData.spear.position,
                    levelData.spear.rotation, levelData.spear.scale);
                ApplyGravityScale(go, levelData.spear.gravityScale);
            }

            // Enemy Spears
            if (levelData.enemySpears != null)
            {
                foreach (var ed in levelData.enemySpears)
                {
                    if (!spearLookup.TryGetValue(ed.spearIndex, out var enemyEntry)) continue;
                    var go = SpawnPrefabAt(enemyEntry, Vector3.zero, LevelEditorItem.ItemType.Spear);
                    ApplyTransform(go.transform, ed.position, ed.rotation, ed.scale);
                    ApplyGravityScale(go, ed.gravityScale);
                }
            }

            // Characters
            if (levelData.characters != null)
            {
                foreach (var cd in levelData.characters)
                {
                    if (!charLookup.TryGetValue(cd.characterIndex, out var entry)) continue;
                    var go = SpawnPrefabAt(entry, Vector3.zero, LevelEditorItem.ItemType.Character);
                    ApplyTransform(go.transform, cd.position, cd.rotation, cd.scale);
                    ApplyGravityScale(go, cd.gravityScale);

                    var editorItem = go.GetComponent<LevelEditorItem>();
                    if (editorItem != null) editorItem.animationState = cd.animationState;

                    // Apply animation to Spine component
                    var skeletonLoad = go.GetComponentInChildren<SkeletonAnimation>();
                    if (skeletonLoad != null && !string.IsNullOrEmpty(cd.animationState))
                    {
                        skeletonLoad.AnimationName = cd.animationState;
                    }

                    // Apply movement params
                    var charBase = go.GetComponent<CharacterBase>();
                    if (charBase != null)
                    {
                        charBase.CharacterMoveType = cd.moveType;
                        charBase.UsePhysicsFall = cd.usePhysicsFall;
                        charBase.MoveDistance = cd.moveDistance;
                        charBase.MoveDuration = cd.moveDuration;
                        charBase.WaitTime = cd.waitTime;
                        charBase.MoveTargetPoint = cd.moveTargetPoint;
                    }

                    // Apply BigCharacter params
                    var bigChar = go.GetComponent<BigCharacter>();
                    if (bigChar != null)
                    {
                        bigChar.SpawnCount = cd.spawnCount;
                        bigChar.SpawnRadius = cd.spawnRadius;
                        bigChar.SpawnForce = cd.spawnForce;
                        bigChar.ChildSizeMultiplier = cd.childSizeMultiplier;
                    }

                    // Apply Wizard params
                    var wizard = go.GetComponent<Wizard>();
                    if (wizard != null && cd.numberTeleport > 0)
                        wizard.NumberTeleport = cd.numberTeleport;

                    // Apply Wizard1 params
                    var wizard1 = go.GetComponent<Wizard1>();
                    if (wizard1 != null)
                        wizard1.PointB = cd.wizard1PointB;
                }
            }

            // Obstacles
            if (levelData.obstacles != null)
            {
                foreach (var od in levelData.obstacles)
                {
                    if (!obstLookup.TryGetValue(od.obstacleIndex, out var entry)) continue;
                    var go = SpawnPrefabAt(entry, Vector3.zero, LevelEditorItem.ItemType.Obstacle);
                    ApplyTransform(go.transform, od.position, od.rotation, od.scale);
                    ApplyGravityScale(go, od.gravityScale);
                    var obstacleBase = go.GetComponent<ObstacleBase>();
                    if (obstacleBase != null)
                    {
                        obstacleBase.MoveType = od.moveType;
                        obstacleBase.MoveDistance = od.moveDistance;
                        obstacleBase.MoveDuration = od.moveDuration;
                        obstacleBase.WaitTime = od.waitTime;
                        if (obstacleBase is Fan fan)
                        {
                            fan.WindForce = od.windForce;
                            fan.ColliderSize = od.windRange;
                            fan.WindAreaTransform.localPosition = od.windTransformOffset;
                        }
                        else if (obstacleBase is Trampoline trampoline)
                        {
                            trampoline.JumpForce = od.jumpForce;
                        }
                        else if (obstacleBase is Portal portal)
                        {
                            portal.PortalID = od.portalID;
                            portal.LinkedPortalID = od.linkedPortalID;
                            portal.AppearDelay = od.appearDelay;
                            portal.PortalMoveSpeed = od.portalMoveSpeed;
                        }
                        else if (obstacleBase is Ballon ballon)
                        {
                            ballon.MoveType = od.moveType;
                            ballon.MoveDistance = od.moveDistance;
                            ballon.MoveDuration = od.moveDuration;
                            ballon.WaitTime = od.waitTime;
                            if (ballon.EnemyFollowTarget != null)
                            {
                                ballon.EnemyFollowTarget.localPosition = od.ballonEnemyFollowOffset;
                            }
                        }
                        else if (obstacleBase is WashingMachine washingMachine)
                        {
                            washingMachine.CharacterTypeInWashingMachine = (CharacterTypeInWashingMachine)od.washingMachineCharacterType;
                            washingMachine.NumberCharacterInWashingMachine = od.washingMachineCount;
                            washingMachine.ScaleCharacterInWashingMachine = od.washingMachineScale;
                        }
                        else if (obstacleBase is Egg egg)
                        {
                            egg.TopEggMoveDistance = od.eggTopMoveDistance;
                            egg.TopEggMoveDuration = od.eggTopMoveDuration;
                            egg.EnemyLocalPosition = od.eggEnemyLocalPosition;
                        }
                        else if (obstacleBase is Ball ball)
                        {
                            ball.BallMoveModeType = (BallMoveMode)od.ballMoveMode;
                            ball.PointB = od.ballPointB;
                            ball.CurveHeight = od.ballCurveHeight;
                            ball.BallId = od.ballId;
                        }
                        else if (obstacleBase is Vehical vehical)
                        {
                            vehical.VehicalMoveModeType = (VehicalMoveMode)od.vehicalMoveMode;
                            vehical.PointB = od.vehicalPointB;
                            vehical.CurveHeight = od.vehicalCurveHeight;
                            vehical.UsePhysicsFall = od.vehicalUsePhysicsFall;
                            if (vehical.EnemyPos != null)
                                vehical.EnemyPos.localPosition = od.vehicalEnemyPosOffset;
                        }
                    }
                }
            }

            // PowerUps
            if (levelData.powerUps != null)
            {
                foreach (var pd in levelData.powerUps)
                {
                    if (!powerLookup.TryGetValue(pd.powerUpIndex, out var entry)) continue;
                    var go = SpawnPrefabAt(entry, Vector3.zero, LevelEditorItem.ItemType.PowerUp);
                    ApplyTransform(go.transform, pd.position, pd.rotation, pd.scale);
                    ApplyGravityScale(go, pd.gravityScale);
                }
            }

            // Decors
            Dictionary<int, LevelPrefabDatabase.PrefabEntry> decorLookup = _decorPrefabs.ToDictionary(p => p.id);
            if (levelData.decorObjects != null)
            {
                foreach (var dd in levelData.decorObjects)
                {
                    if (!decorLookup.TryGetValue(dd.decorIndex, out var entry)) continue;
                    var go = SpawnPrefabAt(entry, Vector3.zero, LevelEditorItem.ItemType.Decor);
                    ApplyTransform(go.transform, dd.position, dd.rotation, dd.scale);

                    var decorObj = go.GetComponent<DongA.Decorators.DecorObject>();
                    if (decorObj != null && !string.IsNullOrEmpty(dd.animName))
                        decorObj.AnimName = dd.animName;

                    var decorSkeleton = go.GetComponentInChildren<SkeletonAnimation>();
                    if (decorSkeleton != null && !string.IsNullOrEmpty(dd.animName))
                        decorSkeleton.AnimationName = dd.animName;
                }
            }

            // Mouse Holes
            Dictionary<int, LevelPrefabDatabase.PrefabEntry> mhLookup = _mouseHolePrefabs.ToDictionary(p => p.id);
            if (levelData.mouseHoles != null)
            {
                foreach (var md in levelData.mouseHoles)
                {
                    if (!mhLookup.TryGetValue(md.mouseHoleIndex, out var entry)) continue;
                    var go = SpawnPrefabAt(entry, Vector3.zero, LevelEditorItem.ItemType.MouseHole);
                    ApplyTransform(go.transform, md.position, md.rotation, md.scale);
                    var mh = go.GetComponent<MouseHole>();
                    if (mh != null)
                    {
                        mh.HoleType = (MouseHoleType)md.mouseHoleType;
                        mh.EnemyCount = md.enemyCount;
                        mh.SpawnScale = md.spawnScale;
                        mh.ApplySpawnPointPositions(md.spawnPointPositions);
                    }
                }
            }

            // Ground SpriteShapes
            Dictionary<int, LevelPrefabDatabase.PrefabEntry> gssLookup = _groundSpriteShapePrefabs.ToDictionary(p => p.id);
            if (levelData.groundSpriteShapes != null)
            {
                foreach (var gd in levelData.groundSpriteShapes)
                {
                    if (!gssLookup.TryGetValue(gd.groundSpriteShapeIndex, out var entry)) continue;
                    var go = SpawnPrefabAt(entry, Vector3.zero, LevelEditorItem.ItemType.GroundSpriteShape);
                    ApplyTransform(go.transform, gd.position, gd.rotation, gd.scale);
                    go.GetComponent<DongA.Decors.GroundSpriteShape>()?.ApplyPoints(gd);
                }
            }

            SpawnLevelPrefab(path);
            _loadedFileName = Path.GetFileName(path);
            Debug.Log($"[LevelEditor] Level loaded <- {path}");
            EditorUtility.DisplayDialog("Level Editor",
                $"Level {_levelIndex} loaded!\n{path}", "OK");
            Repaint();
        }

        private void SpawnLevelPrefab(string jsonAbsPath)
        {
            if (_allLevelSO == null) return;

            string assetPath = "Assets" + jsonAbsPath.Substring(Application.dataPath.Length).Replace('\\', '/');

            foreach (var wrapper in _allLevelSO.AllLevelData)
            {
                if (wrapper.levelPrefab == null || wrapper.levelJson == null) continue;
                if (AssetDatabase.GetAssetPath(wrapper.levelJson) != assetPath) continue;

                // Get or create the dedicated root
                GameObject prefabRoot = GameObject.Find(EDITOR_LEVEL_PREFAB_ROOT);
                if (prefabRoot == null)
                {
                    prefabRoot = new GameObject(EDITOR_LEVEL_PREFAB_ROOT);
                    Undo.RegisterCreatedObjectUndo(prefabRoot, "Create LevelPrefab Root");
                }

                _spawnedLevelPrefab = (GameObject)PrefabUtility.InstantiatePrefab(wrapper.levelPrefab);
                _spawnedLevelPrefab.transform.SetParent(prefabRoot.transform);
                Undo.RegisterCreatedObjectUndo(_spawnedLevelPrefab, "Spawn Level Prefab");
                break;
            }
        }

        private void ApplyTransform(Transform t, Vector2 position, Vector3 rotation, Vector2 scale)
        {
            t.position = new Vector3(position.x, position.y, 0f);
            t.eulerAngles = rotation;
            t.localScale = new Vector3(scale.x, scale.y, 1f);
        }

        private void ApplyGravityScale(GameObject go, float gravityScale)
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.gravityScale = gravityScale;
        }

        // ──────────────────────────── Clear ────────────────────────────────
        private void ClearAll()
        {
            if (!EditorUtility.DisplayDialog("Level Editor",
                    "Clear ALL editor items from the scene?", "Yes", "Cancel"))
                return;

            ClearAllNoConfirm();
        }

        private void ClearAllNoConfirm()
        {
            var prefabRoot = GameObject.Find(EDITOR_LEVEL_PREFAB_ROOT);
            if (prefabRoot != null)
                Undo.DestroyObjectImmediate(prefabRoot);
            _spawnedLevelPrefab = null;

            var root = GameObject.Find(EDITOR_PARENT_NAME);
            if (root != null)
            {
                Undo.DestroyObjectImmediate(root);
            }

            // Also catch any orphaned items
            var orphans = FindObjectsByType<LevelEditorItem>(FindObjectsSortMode.None);
            foreach (var item in orphans)
            {
                Undo.DestroyObjectImmediate(item.gameObject);
            }
        }

        // ──────────────────────────── Utility ──────────────────────────────
        private static void DrawSeparator()
        {
            EditorGUILayout.Space(2);
            Rect rect = EditorGUILayout.GetControlRect(false, 1);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            EditorGUILayout.Space(2);
        }
    }
}
#endif

using System;
using System.Collections.Generic;
using DongA.Characters;
using DongA.Decorators;
using DongA.Decors;
using DongA.Game;
using DongA.GameDatas;
using DongA.Levels;
using DongA.Obstacles;
using Game.Obstacles;
using GameBase;
using Spine.Unity;
using UnityEngine;

/// <summary>
/// Runtime level loader.
/// Reads JSON from <see cref="AllLevelSO"/> for the level indicated by
/// <see cref="GameValue.CurrentLevel"/>, destroys the previous level objects,
/// and spawns the new level.
/// </summary>
public class LoadLevelManager : MonoBehaviour
{
    //private const string LEVEL_ROOT_NAME = "[Level_Root]";

    [Header("Data")]
    [SerializeField] private AllLevelSO allLevelSO;
    [SerializeField] private LevelPrefabDatabase prefabDatabase;
    [Header("Default Skins")]
    [SerializeField] private AllSkinDataSO allSkinDataSO;
    [SerializeField] private AllSpearSkinDataSO allSpearSkinDataSO;
    [SerializeField] private AllTrailSkinDataSO allTrailSkinDataSO;
    [Header("Optional")]
    [Tooltip("Camera used for viewport → world conversion. Falls back to Camera.main.")]
    [SerializeField] private Camera levelCamera;
    [SerializeField] private AllThemeSO allGameThemeSO;
    
    private GameObject _themeInstance;

    private Transform _levelRoot;

    private int _loadedLevel = -1;
    private int _justPlayLevel = 0;
    private int _lastPlayerLevel = -1;
    private int _cachedRandomLevel = -1;

    void Start()
    {
        GameEvents.OnLevelStartLoading += LoadCurrentLevel;
        InitializeDefaultSkins();
        LoadGameTheme();
        LoadCurrentLevel();
    }

    private void InitializeDefaultSkins()
    {
        if (allSkinDataSO != null && allSkinDataSO.AllSkinDataList != null && allSkinDataSO.AllSkinDataList.Count > 0)
        {
            var first = allSkinDataSO.AllSkinDataList[0];
            GamePrefs.SetCharacterSkinOwned(first.SkinNameID);
            if (GamePrefs.CurrentCharacterSkinID == "Normal")
            {
                GamePrefs.CurrentCharacterSkinID = first.SkinNameID;
                GamePrefs.CurrentCharacterSkinName = first.SkinName;
            }
        }

        if (allSpearSkinDataSO != null && allSpearSkinDataSO.AllSpearSkinDataList != null && allSpearSkinDataSO.AllSpearSkinDataList.Count > 0)
        {
            var first = allSpearSkinDataSO.AllSpearSkinDataList[0];
            GamePrefs.SetSpearSkinOwned(first.SkinNameID);
            if (GamePrefs.CurrentSpearSkinID == "Normal")
            {
                GamePrefs.CurrentSpearSkinID = first.SkinNameID;
                GamePrefs.CurrentSpearSkinName = first.SkinName;
                GamePrefs.CurrentSpearType = (int)first.SpearType;
            }
        }

        if (allTrailSkinDataSO != null && allTrailSkinDataSO.AllTrailSkinDataList != null && allTrailSkinDataSO.AllTrailSkinDataList.Count > 0)
        {
            var first = allTrailSkinDataSO.AllTrailSkinDataList[0];
            GamePrefs.SetTrailSkinOwned(first.SkinNameID);
            if (GamePrefs.CurrentTrailSkinID == "Normal")
            {
                GamePrefs.CurrentTrailSkinID = first.SkinNameID;
                GamePrefs.CurrentTrailSkinName = first.SkinName;
            }
        }
    }

    // void OnEnable()
    // {
        
    // }

    void OnDisable()
    {
        GameEvents.OnLevelStartLoading -= LoadCurrentLevel;
    }
    public void LoadCurrentLevel()
    {
        if(GamePrefs.Level > allLevelSO.AllLevelData.Count - 1)
        {
            GamePrefs.OutOfLevel = 1;
            // Only pick a new random level when player level changes (next level),
            // reuse the cached one on replay
            if (_lastPlayerLevel != GamePrefs.Level || _cachedRandomLevel < 0)
            {
                _cachedRandomLevel = GetRandomLevel();
                _lastPlayerLevel = GamePrefs.Level;
            }
            LoadLevel(_cachedRandomLevel);
        }
        else
        {
            _lastPlayerLevel = GamePrefs.Level;
            LoadLevel(GamePrefs.Level);
        }
        GlobalValues.StartGame();
    }

    public void LoadLevel(int levelIndex)
    {
        _justPlayLevel = levelIndex;
        CharacterBase.ResetSkinSpawnCount();
        GamePrefs.IsIceFrozen = false;
        LevelDataWrapper wrapper = FindLevelWrapper(levelIndex);
        if (wrapper == null || wrapper.levelJson == null)
        {
            Debug.LogError($"[LoadLevelManager] No JSON found for level {levelIndex} in AllLevelSO.");
            return;
        }

        LevelData levelData;
        try
        {
            levelData = JsonUtility.FromJson<LevelData>(wrapper.levelJson.text);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LoadLevelManager] Failed to parse level {levelIndex}: {ex.Message}");
            return;
        }

        if (levelData == null)
        {
            Debug.LogError($"[LoadLevelManager] LevelData is null for level {levelIndex}.");
            return;
        }

        // Camera cam = levelCamera != null ? levelCamera : Camera.main;
        // LevelViewportHelper.ConvertToWorldPositions(levelData, cam);
        ClearCurrentLevel();

        _levelRoot = GetOrCreateRoot();

        SpawnSpear(levelData);
        SpawnCharacters(levelData);
        SpawnObstacles(levelData);
        SpawnPowerUps(levelData);
        SpawnDecors(levelData);
        SpawnGroundSpriteShapes(levelData);
        SpawnMouseHoles(levelData);
        SpawnLevelPrefab(wrapper);

        _loadedLevel = levelIndex;
        GameEvents.OnLevelLoadComplete?.Invoke();
    }
    private int GetRandomLevel()
    {
        int randomLevel =  UnityEngine.Random.Range(0, allLevelSO.AllLevelData.Count - 1);
        while (randomLevel == _justPlayLevel)
        {
            randomLevel =  UnityEngine.Random.Range(0, allLevelSO.AllLevelData.Count - 1);
        }
        return randomLevel;
    }
    public void LoadGameTheme()
    {
        if(allGameThemeSO == null || allGameThemeSO.gameThemes == null) return;
        foreach (var theme in allGameThemeSO.gameThemes)
        {
            if (theme.ThemeID == GamePrefs.ThemeID)
            {
                if (theme.themePrefab != null)
                {
                    if (_themeInstance != null)
                    {
                        Destroy(_themeInstance);
                    }
                    _themeInstance = Instantiate(theme.themePrefab);
                    if(ThemeManager.Instance != null)
                    {
                        ThemeManager.Instance.currentTheme = _themeInstance;
                    }
                }
                else
                {
                    Debug.LogWarning($"[LoadLevelManager] Theme prefab is null for theme ID {theme.ThemeID}.");
                }
                return;
            }
        }
    }

    /// <summary>Destroy every object belonging to the current level.</summary>
    public void ClearCurrentLevel()
    {
        if (_levelRoot != null)
        {
            Destroy(_levelRoot.gameObject);
            _levelRoot = null;
        }

        // Also destroy by name in case the reference was lost (e.g. scene reload).
        GameObject existing = GameObject.Find(KeyPrefs.LevelRootName);
        if (existing != null)
            Destroy(existing);

        _loadedLevel = -1;
    }

    private LevelDataWrapper FindLevelWrapper(int levelIndex)
    {
        if (allLevelSO == null || allLevelSO.AllLevelData == null) return null;

        foreach (var w in allLevelSO.AllLevelData)
        {
            if (w.levelID == levelIndex) return w;
        }

        return null;
    }

    private Transform GetOrCreateRoot()
    {
        //GameObject root = GameObject.Find(LEVEL_ROOT_NAME);
        // if (root == null)
        // {
        //     root = new GameObject(LEVEL_ROOT_NAME);
        // }
        GameObject root = new GameObject(KeyPrefs.LevelRootName);
        return root.transform;
    }


    private void SpawnSpear(LevelData data)
    {
        if (data.hasSpear && data.spear != null)
        {
            GameObject prefab = prefabDatabase.GetSpear(data.spear.spearIndex);
            if (prefab != null)
            {
                GameObject go = Instantiate(prefab, _levelRoot);
                ApplyTransform(go.transform, data.spear.position, data.spear.rotation, data.spear.scale);
                ApplyGravityScale(go, data.spear.gravityScale);
            }
        }

        if (data.enemySpears == null) return;
        foreach (var ed in data.enemySpears)
        {
            GameObject prefab = prefabDatabase.GetSpear(ed.spearIndex);
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, _levelRoot);
            ApplyTransform(go.transform, ed.position, ed.rotation, ed.scale);
            ApplyGravityScale(go, ed.gravityScale);
            if (GameManager.Instance != null)
                GameManager.Instance.IncreaseEnemyCount();
        }
    }

    private void SpawnCharacters(LevelData data)
    {
        if (data.characters == null) return;

        foreach (var cd in data.characters)
        {
            GameObject prefab = prefabDatabase.GetCharacter(cd.characterIndex);
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, _levelRoot);
            ApplyTransform(go.transform, cd.position, cd.rotation, cd.scale);
            ApplyGravityScale(go, cd.gravityScale);
            if (go.TryGetComponent<CharacterBase>(out var charBaseForScale)) charBaseForScale.MarkScaleReady();

            // Apply Spine animation state
            if (!string.IsNullOrEmpty(cd.animationState))
            {
                var skeleton = go.GetComponentInChildren<SkeletonAnimation>();
                if (skeleton != null)
                    skeleton.AnimationName = cd.animationState;
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

            if (GameManager.Instance != null)
                GameManager.Instance.IncreaseEnemyCount();
        }
    }
    

    private void SpawnObstacles(LevelData data)
    {
        if (data.obstacles == null) return;

        var collectedCoins = GamePrefs.GetCollectedCoins(GamePrefs.Level);
        int coinIndex = 0;

        foreach (var od in data.obstacles)
        {
            GameObject prefab = prefabDatabase.GetObstacle(od.obstacleIndex);
            if (prefab == null) continue;

            bool isCoin = prefab.GetComponent<Coin>() != null;
            int currentCoinId = coinIndex;
            if (isCoin)
            {
                coinIndex++;
                if (collectedCoins.Contains(currentCoinId)) continue;
            }

            GameObject go = Instantiate(prefab, _levelRoot);
            ApplyTransform(go.transform, od.position, od.rotation, od.scale);
            ApplyGravityScale(go, od.gravityScale);

            // Apply obstacle-specific runtime properties
            var obstacleBase = go.GetComponent<ObstacleBase>();
            if (obstacleBase == null) continue;

            obstacleBase.MoveType = od.moveType;
            obstacleBase.MoveDistance = od.moveDistance;
            obstacleBase.MoveDuration = od.moveDuration;
            obstacleBase.WaitTime = od.waitTime;

            if (obstacleBase is Fan fan)
            {
                fan.WindForce = od.windForce;
                fan.ColliderSize = od.windRange;
                if (fan.WindAreaTransform != null)
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
                vehical.Init();
            }
            else if (obstacleBase is Coin coin)
            {
                coin.CoinId = currentCoinId;
            }

            // Start movement if the obstacle has a move type
            if (od.moveType != ObstaclesMoveType.None)
            {
                obstacleBase.Move();
            }
        }
    }

    private void SpawnPowerUps(LevelData data)
    {
        if (data.powerUps == null) return;

        foreach (var pd in data.powerUps)
        {
            GameObject prefab = prefabDatabase.GetPowerUp(pd.powerUpIndex);
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, _levelRoot);
            ApplyTransform(go.transform, pd.position, pd.rotation, pd.scale);
            ApplyGravityScale(go, pd.gravityScale);
        }
    }
    private void SpawnDecors(LevelData data)
    {
        if (data.decorObjects == null) return;

        foreach (var dd in data.decorObjects)
        {
            GameObject prefab = prefabDatabase.GetDecor(dd.decorIndex);
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, _levelRoot);
            ApplyTransform(go.transform, dd.position, dd.rotation, dd.scale);

            var decorObj = go.GetComponent<DecorObject>();
            if (decorObj != null && !string.IsNullOrEmpty(dd.animName))
                decorObj.AnimName = dd.animName;

            var skeleton = go.GetComponentInChildren<SkeletonAnimation>();
            if (skeleton != null && !string.IsNullOrEmpty(dd.animName))
                skeleton.AnimationName = dd.animName;
        }
    }

        private void SpawnMouseHoles(LevelData data)
    {
        if (data.mouseHoles == null) return;

        foreach (var md in data.mouseHoles)
        {
            GameObject prefab = prefabDatabase.GetMouseHole(md.mouseHoleIndex);
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, _levelRoot);
            ApplyTransform(go.transform, md.position, md.rotation, md.scale);

            if (go.TryGetComponent<MouseHole>(out var mh))
            {
                mh.HoleType = (MouseHoleType)md.mouseHoleType;
                mh.EnemyCount = md.enemyCount;
                mh.SpawnScale = md.spawnScale;
                mh.ApplySpawnPointPositions(md.spawnPointPositions);
            }
        }
    }

    private void SpawnGroundSpriteShapes(LevelData data)
    {
        if (data.groundSpriteShapes == null) return;

        foreach (var gd in data.groundSpriteShapes)
        {
            GameObject prefab = prefabDatabase.GetGroundSpriteShape(gd.groundSpriteShapeIndex);
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, _levelRoot);
            ApplyTransform(go.transform, gd.position, gd.rotation, gd.scale);
            if (go.TryGetComponent<GroundSpriteShape>(out var groundShape))
                groundShape.ApplyPoints(gd);
        }
    }

    private void SpawnLevelPrefab(LevelDataWrapper wrapper)
    {
        if (wrapper.levelPrefab == null) return;
        Instantiate(wrapper.levelPrefab, _levelRoot);
    }

    // ───────────────────── Transform ──────────────────────

    private static void ApplyTransform(Transform t, Vector2 position, Vector3 rotation, Vector2 scale)
    {
        t.position = new Vector3(position.x, position.y, 0f);
        t.eulerAngles = rotation;
        t.localScale = new Vector3(scale.x, scale.y, 1f);
    }

    private static void ApplyGravityScale(GameObject go, float gravityScale)
    {
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.gravityScale = gravityScale;
    }
}

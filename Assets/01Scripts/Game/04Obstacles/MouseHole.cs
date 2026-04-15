using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DongA.Characters;
using Game.Obstacles;
using GameBase;
using TMPro;
using UnityEngine;

namespace DongA.Obstacles
{
    public class MouseHole : ObstacleBase
    {
        [Header("Spawn Settings")]
        [SerializeField] private MouseHoleType mouseHoleType;
        [SerializeField] private CharacterPrefabAllSO characterPrefabAllSO;
        private GameObject enemyPrefab;
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private int enemyCount = 5; // only used when spawnPoints.Count == 1
        [SerializeField] private float spawnScale = 0.12f;

        [Header("UI Settings")]
        [SerializeField] private TextMeshProUGUI mouseRemainText;
    
        [Header("Timing")]
        [SerializeField] private float popUpHeight = 1f;
        [SerializeField] private float popUpDuration = 0.25f;
        [SerializeField] private float minVisibleTime = 1f;
        [SerializeField] private float maxVisibleTime = 2.5f;
        [SerializeField] private float minHideDelay = 0.5f;
        [SerializeField] private float maxHideDelay = 2f;

        private readonly List<int> _freeHoleIndices = new();
        private readonly WaitForSeconds _waitRetry = new(0.5f);
        private readonly WaitForSeconds _waitFreeHole = new(0.2f);

        // Single-hole mode
        private int _remainingToSpawn;

        // Multi-hole mode
        private int _aliveCount;

        private int _killedCount;

        private bool IsMultiHole => spawnPoints != null && spawnPoints.Count > 1;

        // ── Public accessors for Level Editor / Loader ───────────────────────
        public MouseHoleType HoleType
        {
            get => mouseHoleType;
            set => mouseHoleType = value;
        }
        public int EnemyCount
        {
            get => enemyCount;
            set => enemyCount = value;
        }
        public float SpawnScale
        {
            get => spawnScale;
            set => spawnScale = value;
        }
        public List<Vector2> GetSpawnPointPositions()
        {
            var result = new List<Vector2>(spawnPoints?.Count ?? 0);
            if (spawnPoints == null) return result;
            foreach (var p in spawnPoints)
                if (p != null) result.Add(p.position);
            return result;
        }
        public void ApplySpawnPointPositions(List<Vector2> positions)
        {
            if (spawnPoints == null || positions == null) return;
            int count = Mathf.Min(spawnPoints.Count, positions.Count);
            for (int i = 0; i < count; i++)
                if (spawnPoints[i] != null)
                    spawnPoints[i].position = new Vector3(positions[i].x, positions[i].y, 0f);
        }

        public MouseHole() : base(ObstaclesMoveType.None, 0f, 0f, 0f) { }

        void Start()
        {
            SpawnEnemies();
            UpdateRemainText();
        }

        private void UpdateRemainText()
        {
            if (mouseRemainText != null)
                mouseRemainText.text = (enemyCount - _killedCount).ToString();
        }


        // ─────────────── Init ───────────────────────────────────────────────
        private void GetEnemyPrefab()
        {
            switch (mouseHoleType)
            {
                case MouseHoleType.Pickle:
                    enemyPrefab = characterPrefabAllSO.picklePrefab;
                    break;
                case MouseHoleType.Banana:
                    enemyPrefab = characterPrefabAllSO.bananaPrefab;
                    break;
                case MouseHoleType.Watermelon:
                    enemyPrefab = characterPrefabAllSO.watermelonPrefab;
                    break;
                case MouseHoleType.Grape:
                    enemyPrefab = characterPrefabAllSO.grapePrefab;
                    break;
                default:
                    Debug.LogWarning("[MouseHole] Unassigned MouseHoleType!", this);
                    break;
            }
        }
        private void SpawnEnemies()
        {
            GetEnemyPrefab();
            if (enemyPrefab == null)
            {
                Debug.LogWarning("[MouseHole] enemyPrefab is null!", this);
                return;
            }
            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                Debug.LogWarning("[MouseHole] spawnPoints is empty!", this);
                return;
            }
            if(GameManager.Instance != null)
                for (int i = 0; i < enemyCount; i++)
                    GameManager.Instance.IncreaseEnemyCount();

            for (int i = 0; i < spawnPoints.Count; i++)
                _freeHoleIndices.Add(i);

            if (!IsMultiHole)
                SpawnSingleHoleMode();
            else
            {
                _remainingToSpawn = enemyCount;
                SpawnWave();
            }
        }

        // ─────────────── Single-hole mode (original behaviour) ──────────────

        private void SpawnSingleHoleMode()
        {
            int initialCount = Mathf.Min(enemyCount, spawnPoints.Count);
            _remainingToSpawn = enemyCount - initialCount;
            for (int i = 0; i < initialCount; i++)
                SpawnAtRandomFreeHole(Random.Range(0f, maxHideDelay));
        }

        private void SpawnAtRandomFreeHole(float initialDelay = 0f)
        {
            if (_freeHoleIndices.Count == 0) return;

            int randIdx = Random.Range(0, _freeHoleIndices.Count);
            int holeIndex = _freeHoleIndices[randIdx];
            _freeHoleIndices.RemoveAt(randIdx);

            Transform point = spawnPoints[holeIndex];
            GameObject go = Instantiate(enemyPrefab, point.position + Vector3.down * popUpHeight,
                Quaternion.identity, transform.parent);
            go.transform.localScale = Vector3.one * spawnScale;

            if (go.TryGetComponent<CharacterBase>(out var charBase))
                charBase.MarkScaleReady();

            var col = go.GetComponentInChildren<Collider2D>();
            if (col != null) col.enabled = false;
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            
            StartCoroutine(SingleHolePopCycle(go, point, holeIndex, initialDelay));
        }

        private IEnumerator SingleHolePopCycle(GameObject enemy, Transform point, int holeIndex, float initialDelay)
        {
            enemy.TryGetComponent<CharacterBase>(out var charBase);
            var col = enemy.GetComponentInChildren<Collider2D>();

            // Let CharacterBase.Start() run first, then lock rb before stagger delay
            yield return null;

            if (enemy == null) { OnSingleHoleEnemyDied(holeIndex); yield break; }

            if (enemy.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            if (charBase != null) charBase.StopMoving();
            enemy.transform.position = point.position + Vector3.down * popUpHeight;

            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                if (IsEnemyGone(enemy, charBase)) { OnSingleHoleEnemyDied(holeIndex); yield break; }

                Vector3 hiddenPos = point.position + Vector3.down * popUpHeight;
                yield return new WaitForSeconds(Random.Range(minHideDelay, maxHideDelay));
                if (IsEnemyGone(enemy, charBase)) { OnSingleHoleEnemyDied(holeIndex); yield break; }

                enemy.transform.DOMove(point.position, popUpDuration).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(popUpDuration);
                if (IsEnemyGone(enemy, charBase)) { OnSingleHoleEnemyDied(holeIndex); yield break; }

                if (col != null) col.enabled = true;

                float visibleTime = Random.Range(minVisibleTime, maxVisibleTime);
                float elapsed = 0f;
                while (elapsed < visibleTime)
                {
                    if (IsEnemyGone(enemy, charBase))
                    {
                        if (col != null) col.enabled = false;
                        OnSingleHoleEnemyDied(holeIndex);
                        yield break;
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                if (IsEnemyGone(enemy, charBase)) { if (col != null) col.enabled = false; OnSingleHoleEnemyDied(holeIndex); yield break; }

                if (col != null) col.enabled = false;
                enemy.transform.DOMove(hiddenPos, popUpDuration).SetEase(Ease.InBack);
                yield return new WaitForSeconds(popUpDuration);
            }
        }

        private void OnSingleHoleEnemyDied(int holeIndex)
        {
            _killedCount++;
            UpdateRemainText();
            _freeHoleIndices.Add(holeIndex);
            if (_remainingToSpawn <= 0) return;
            _remainingToSpawn--;
            StartCoroutine(SingleHoleSpawnAfterDelay(Random.Range(minHideDelay, maxHideDelay)));
        }

        private IEnumerator SingleHoleSpawnAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            while (_freeHoleIndices.Count == 0)
                yield return _waitRetry;
            SpawnAtRandomFreeHole();
        }

        // ─────────────── Multi-hole mode (N-1 enemies, roaming holes) ───────

        private void SpawnWave()
        {
            int waveSize = Mathf.Min(spawnPoints.Count - 1, _remainingToSpawn);
            if (waveSize <= 0) return;

            _remainingToSpawn -= waveSize;
            _aliveCount = waveSize;

            for (int i = 0; i < waveSize; i++)
                SpawnMultiHoleEnemy(Random.Range(0f, maxHideDelay));
        }

        private void SpawnMultiHoleEnemy(float initialDelay = 0f)
        {
            // Start hidden below hole 0; coroutine will pick a real hole before first pop
            Vector3 hiddenPos = spawnPoints[0].position + Vector3.down * popUpHeight;
            GameObject go = Instantiate(enemyPrefab, hiddenPos, Quaternion.identity, transform.parent);
            go.transform.localScale = Vector3.one * spawnScale;

            if (go.TryGetComponent<CharacterBase>(out var charBase))
                charBase.MarkScaleReady();

            var col = go.GetComponentInChildren<Collider2D>();
            if (col != null) col.enabled = false;
            if (go.TryGetComponent<Rigidbody2D>(out var rb))
                rb.bodyType = RigidbodyType2D.Kinematic;

            StartCoroutine(MultiHolePopCycle(go, initialDelay));
        }

        private IEnumerator MultiHolePopCycle(GameObject enemy, float initialDelay)
        {
            enemy.TryGetComponent<CharacterBase>(out var charBase);
            var col = enemy.GetComponentInChildren<Collider2D>();

            // Let CharacterBase.Start() run first, then lock rb before stagger delay
            yield return null;

            if (enemy == null) { OnWaveEnemyDied(-1); yield break; }

            if (enemy.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
            if (charBase != null) charBase.StopMoving();

            yield return new WaitForSeconds(initialDelay);

            int currentHole = -1;

            while (true)
            {
                if (IsEnemyGone(enemy, charBase)) { OnWaveEnemyDied(currentHole); yield break; }

                yield return new WaitForSeconds(Random.Range(minHideDelay, maxHideDelay));
                if (IsEnemyGone(enemy, charBase)) { OnWaveEnemyDied(currentHole); yield break; }

                // Wait for a free hole
                while (_freeHoleIndices.Count == 0)
                    yield return _waitFreeHole;
                if (IsEnemyGone(enemy, charBase)) { OnWaveEnemyDied(currentHole); yield break; }

                // Pick random free hole
                int randIdx = Random.Range(0, _freeHoleIndices.Count);
                currentHole = _freeHoleIndices[randIdx];
                _freeHoleIndices.RemoveAt(randIdx);

                Transform point = spawnPoints[currentHole];
                Vector3 hiddenPos = point.position + Vector3.down * popUpHeight;

                // Teleport to chosen hole's hidden position
                enemy.transform.position = hiddenPos;

                // Pop up
                enemy.transform.DOMove(point.position, popUpDuration).SetEase(Ease.OutBack);
                yield return new WaitForSeconds(popUpDuration);
                if (IsEnemyGone(enemy, charBase))
                {
                    _freeHoleIndices.Add(currentHole);
                    OnWaveEnemyDied(-1);
                    yield break;
                }

                if (col != null) col.enabled = true;

                // Visible time
                float visibleTime = Random.Range(minVisibleTime, maxVisibleTime);
                float elapsed = 0f;
                while (elapsed < visibleTime)
                {
                    if (IsEnemyGone(enemy, charBase))
                    {
                        if (col != null) col.enabled = false;
                        OnWaveEnemyDied(currentHole);
                        yield break;
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                if (IsEnemyGone(enemy, charBase))
                {
                    if (col != null) col.enabled = false;
                    OnWaveEnemyDied(currentHole);
                    yield break;
                }

                if (col != null) col.enabled = false;

                // Pop down, then release hole
                enemy.transform.DOMove(hiddenPos, popUpDuration).SetEase(Ease.InBack);
                yield return new WaitForSeconds(popUpDuration);

                _freeHoleIndices.Add(currentHole);
                currentHole = -1;
            }
        }

        private void OnWaveEnemyDied(int holeIndex)
        {
            _killedCount++;
            UpdateRemainText();
            if (holeIndex >= 0)
                _freeHoleIndices.Add(holeIndex);

            _aliveCount--;
            if (_aliveCount <= 0 && _remainingToSpawn > 0)
                StartCoroutine(NextWaveAfterDelay(Random.Range(minHideDelay, maxHideDelay)));
        }

        private IEnumerator NextWaveAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SpawnWave();
        }

        // ─────────────── Shared ─────────────────────────────────────────────

        private static bool IsEnemyGone(GameObject enemy, CharacterBase charBase)
        {
            if (enemy == null) return true;
            if (!enemy.activeInHierarchy) return true;
            if (charBase != null && charBase.IsDead) return true;
            return false;
        }

        public override void BreakObstacle() { }

#if UNITY_EDITOR
        new private void OnDrawGizmos()
        {
            if (spawnPoints == null) return;
            foreach (var point in spawnPoints)
            {
                if (point == null) continue;
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(point.position, 0.2f);
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(point.position, point.position + Vector3.down * popUpHeight);
                Gizmos.DrawWireSphere(point.position + Vector3.down * popUpHeight, 0.15f);
            }
        }
#endif
    }
    public enum MouseHoleType
    {
        Pickle,
        Banana,
        Watermelon,
        Grape
    }
}

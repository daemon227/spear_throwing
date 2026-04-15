using System;
using System.Collections.Generic;
using DongA.Characters;
using UnityEngine;

namespace DongA.SpearManager
{
    /// <summary>
    /// Kiểu va chạm của trajectory
    /// </summary>
    public enum TrajectoryHitType
    {
        None,
        Enemy,
        Destructible,
        Mirror,
        BlindMirror,
        Wall,
        Spear
    }

    /// <summary>
    /// Quản lý đường trajectory của ngọn giáo
    /// Tách biệt: Raycast / Xử lý va chạm / Vẽ line
    /// </summary>
    public class SpearTrajectoryLine : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("Line Renderer")]
        [SerializeField] private GameObject _trajectoryLinePrefab;
        [SerializeField] private float _trajectoryWidth = 0.05f;
        [SerializeField] private Color _defaultColor = Color.red;
        [SerializeField] private Color _enemyHitColor = Color.green;

        [Header("Raycast Settings")]
        [SerializeField] private float _maxRayDistance = 100f;
        [SerializeField] private float _raycastRadius = 0.02f;
        [SerializeField] private int _maxReflections = 10;
        [SerializeField] private float _surfaceOffset = 0.02f;

        [Header("Near-90° Deflection")]
        [Tooltip("Khoảng cách góc (độ) từ 90° để kích hoạt deflection")]
        [SerializeField] private float _ninetyDegreeAngleThreshold = 5f;
        [Tooltip("Góc lệch (độ) được thêm vào khi bắn gần 90°")]
        [SerializeField] private float _ninetyDegreeDeflectionAngle = 5f;

        [Header("Hit Detection")]
        [Tooltip("Khoảng cách tối đa từ enemy đến đường trajectory để vẫn giữ hit state (tránh flicker khi collider bone dao động)")]
        [SerializeField] private float _enemyHitExitDistance = 0.5f;

        [Header("Layer Masks")]
        [SerializeField] private LayerMask _wallLayer;
        [SerializeField] private LayerMask _mirrorLayer;
        [SerializeField] private LayerMask _blindMirrorLayer;
        [SerializeField] private LayerMask _enemyLayer;
        [SerializeField] private LayerMask _destructibleLayer;
        [SerializeField] private LayerMask _spearLayer;

        #endregion

        #region Private Fields

        private LineRenderer _lineRenderer;
        private bool _initialized;
        private bool _hitEnemyThisFrame;
        private Transform _ownerRoot;

        /// <summary>
        /// Khi true, trajectory line không gọi ShowHitEffect/HideHitEffect trên enemy (dùng cho enemy spear)
        /// </summary>
        [HideInInspector] public bool disableEnemyHitEffects = false;

        /// <summary>
        /// Các điểm trajectory đã tính (dùng cho flight behavior)
        /// </summary>
        private readonly List<Vector3> _lastTrajectoryPoints = new List<Vector3>();
        public IReadOnlyList<Vector3> LastTrajectoryPoints => _lastTrajectoryPoints;
        
        // Hit tracking để tránh invoke event nhiều lần cho cùng 1 collider
        private readonly HashSet<Collider2D> _enemyHitsLastFrame = new HashSet<Collider2D>();
        private readonly HashSet<Collider2D> _enemyHitsCurrentFrame = new HashSet<Collider2D>();
        private readonly HashSet<Collider2D> _destructibleHitsLastFrame = new HashSet<Collider2D>();
        private readonly HashSet<Collider2D> _destructibleHitsCurrentFrame = new HashSet<Collider2D>();

        // Lưu mapping Collider -> CharacterBase để gọi HideHitEffect khi tia rời đi
        private readonly Dictionary<Collider2D, CharacterBase> _hitEnemyCharacters = new Dictionary<Collider2D, CharacterBase>();

        // Vị trí freeTip hiện tại, dùng để truyền cho ShowHitEffect
        private Vector3 _currentFreeTipPosition;


        #endregion

        #region Events

        public event Action<RaycastHit2D> OnEnemyHit;
        public event Action<RaycastHit2D> OnDestructibleHit;
        public event Action<RaycastHit2D> OnWallHit;
        public event Action<RaycastHit2D> OnMirrorHit;

        #endregion

        #region Public Methods

        public void Initialize(Transform parent)
        {
            if (_initialized) return;

            _ownerRoot = parent;
            CreateLineRenderer(parent);
            ConfigureLineRenderer();
            _lineRenderer.enabled = false;
            _initialized = true;
        }

        public void Show(bool show)
        {
            if (_lineRenderer != null)
            {
                _lineRenderer.enabled = show;
            }
        }

        public void UpdateLine(Vector3 stuckTip, Vector3 freeTip, LayerMask defaultWallLayer)
        {
            if (_lineRenderer == null) return;

            _currentFreeTipPosition = freeTip;
            Vector3 direction = (freeTip - stuckTip).normalized;
            if (direction.sqrMagnitude < 0.0001f) return;

            // Sử dụng wall layer mặc định nếu chưa set
            if (_wallLayer.value == 0)
            {
                _wallLayer = defaultWallLayer;
            }

            // Reset trạng thái
            _hitEnemyThisFrame = false;
            BeginHitTracking();

            // Tính toán trajectory points (đầy đủ cả BlindMirror, dùng cho spear bay)
            List<Vector3> fullTrajectoryPoints = CalculateTrajectoryPoints(freeTip, direction, out int visiblePointCount);

            // Lưu đầy đủ để spear flight dùng
            _lastTrajectoryPoints.Clear();
            _lastTrajectoryPoints.AddRange(fullTrajectoryPoints);

            // Vẽ line chỉ đến visiblePointCount (dừng tại BlindMirror hoặc Wall)
            List<Vector3> visiblePoints = fullTrajectoryPoints.GetRange(0, visiblePointCount);
            DrawLine(visiblePoints);

            EndHitTracking();

            // Cập nhật màu (sau EndHitTracking vì distance check có thể giữ _hitEnemyThisFrame = true)
            UpdateLineColor();
        }

        #endregion

        #region Trajectory Calculation

        /// <summary>
        /// Tính toán các điểm của trajectory
        /// - Tia xuyên qua Enemy/Obstacle nhưng vẫn detect
        /// - Tia dừng khi chạm Wall
        /// - Tia phản xạ khi chạm Mirror (góc phản xạ = góc tới)
        /// </summary>
        private List<Vector3> CalculateTrajectoryPoints(Vector3 startPoint, Vector3 direction, out int visiblePointCount)
        {
            List<Vector3> points = new List<Vector3> { startPoint };
            bool visibilityEnded = false;
            visiblePointCount = 0; // Sẽ set sau

            Vector3 currentOrigin = startPoint;
            Vector3 currentDirection = direction.normalized;
            int reflectionCount = 0;
            Collider2D lastReflectedCollider = null;

            while (reflectionCount <= _maxReflections)
            {
                RaycastResult result = ShootRay(currentOrigin, currentDirection, lastReflectedCollider);

                switch (result.StopType)
                {
                    case RayStopType.Wall:
                        points.Add(result.HitPoint);
                        HandleWallHit(result.Hit);
                        if (!visibilityEnded) visiblePointCount = points.Count;
                        return points;

                    case RayStopType.Mirror:
                        points.Add(result.HitPoint);
                        OnMirrorHit?.Invoke(result.Hit);

                        Vector2 normal = result.Hit.normal;
                        if (Vector2.Dot(currentDirection, normal) > 0)
                            normal = -normal;

                        Vector2 reflected = Vector2.Reflect(currentDirection, normal);
                        reflected = DeflectIfNearNinety(reflected, currentDirection);
                        float offset = Mathf.Max(_surfaceOffset, _raycastRadius * 1.1f);
                        currentOrigin = result.HitPoint + (Vector3)(reflected.normalized * offset);
                        currentDirection = reflected.normalized;
                        lastReflectedCollider = result.Hit.collider;
                        reflectionCount++;
                        break;

                    case RayStopType.BlindMirror:
                        points.Add(result.HitPoint);

                        // Line dừng ở đây nếu chưa bị ẩn trước đó
                        if (!visibilityEnded)
                        {
                            visiblePointCount = points.Count;
                            visibilityEnded = true;
                        }

                        // Vẫn tính phản xạ cho spear bay
                        Vector2 bNormal = result.Hit.normal;
                        if (Vector2.Dot(currentDirection, bNormal) > 0)
                            bNormal = -bNormal;

                        Vector2 bReflected = Vector2.Reflect(currentDirection, bNormal);
                        bReflected = DeflectIfNearNinety(bReflected, currentDirection);
                        float bOffset = Mathf.Max(_surfaceOffset, _raycastRadius * 1.1f);
                        currentOrigin = result.HitPoint + (Vector3)(bReflected.normalized * bOffset);
                        currentDirection = bReflected.normalized;
                        lastReflectedCollider = result.Hit.collider;
                        reflectionCount++;
                        break;

                    case RayStopType.None:
                        points.Add(currentOrigin + currentDirection * _maxRayDistance);
                        if (!visibilityEnded) visiblePointCount = points.Count;
                        return points;
                }
            }

            points.Add(currentOrigin + currentDirection * _maxRayDistance);
            if (!visibilityEnded) visiblePointCount = points.Count;
            return points;
        }

        #endregion

        #region Raycasting

        /// <summary>
        /// Nếu góc phản xạ gần 180° (bắn thẳng 90° vào gương), xoay nhẹ hướng để tránh nảy thẳng.
        /// </summary>
        private Vector2 DeflectIfNearNinety(Vector2 reflected, Vector2 incomingDir)
        {
            // dot ≈ -1 khi reflected gần như ngược chiều incomingDir (bắn 90° vào gương)
            float dot = Vector2.Dot(reflected.normalized, incomingDir.normalized);
            float cosThreshold = Mathf.Cos((180f - _ninetyDegreeAngleThreshold) * Mathf.Deg2Rad);
            if (dot < cosThreshold)
            {
                float rad = _ninetyDegreeDeflectionAngle * Mathf.Deg2Rad;
                float c = Mathf.Cos(rad);
                float s = Mathf.Sin(rad);
                return new Vector2(
                    c * reflected.x - s * reflected.y,
                    s * reflected.x + c * reflected.y
                );
            }
            return reflected;
        }

        private enum RayStopType { None, Wall, Mirror, BlindMirror }

        private struct RaycastResult
        {
            public RayStopType StopType;
            public RaycastHit2D Hit;
            public Vector3 HitPoint;
        }

        private RaycastResult ShootRay(Vector3 origin, Vector3 direction, Collider2D lastReflectedCollider = null)
        {
            LayerMask combinedMask = CombineAllLayers();
            List<RaycastHit2D> hits = CastRay(origin, direction, _maxRayDistance, combinedMask);

            if (hits.Count == 0)
            {
                return new RaycastResult { StopType = RayStopType.None };
            }

            // Sắp xếp theo khoảng cách từ gần -> xa
            hits.Sort((a, b) => a.distance.CompareTo(b.distance));

            // When we have a last reflected collider, use a smaller minDistance
            // since we skip that collider explicitly — allows detecting very close mirrors
            float minDistance = lastReflectedCollider != null
                ? 0.001f
                : Mathf.Max(_surfaceOffset, _raycastRadius * 1.1f);

            // Tìm điểm dừng gần nhất (Wall/Mirror)
            RayStopType stopType = RayStopType.None;
            RaycastHit2D stopHit = default;
            float stopDistance = _maxRayDistance;
            const float distanceEpsilon = 0.0001f;

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.collider == lastReflectedCollider) continue;

                if (hit.distance <= minDistance) continue;

                TrajectoryHitType hitType = DetermineHitType(hit.collider.gameObject.layer);
                if (hitType != TrajectoryHitType.Mirror && hitType != TrajectoryHitType.Wall && hitType != TrajectoryHitType.BlindMirror) continue;

                if (hit.distance + distanceEpsilon < stopDistance)
                {
                    stopDistance = hit.distance;
                    stopHit = hit;
                    if (hitType == TrajectoryHitType.Mirror)
                        stopType = RayStopType.Mirror;
                    else if (hitType == TrajectoryHitType.BlindMirror)
                        stopType = RayStopType.BlindMirror;
                    else
                        stopType = RayStopType.Wall;
                }
                else if (Mathf.Abs(hit.distance - stopDistance) <= distanceEpsilon
                         && stopType == RayStopType.Wall
                         && (hitType == TrajectoryHitType.Mirror || hitType == TrajectoryHitType.BlindMirror))
                {
                    // Cung mot khoang cach: uu tien Mirror
                    stopHit = hit;
                    stopType = hitType == TrajectoryHitType.Mirror ? RayStopType.Mirror : RayStopType.BlindMirror;
                }
            }

            // Xử lý tất cả Enemy/Obstacle trên đoạn tia (trước điểm dừng, nếu có)
            float effectiveDistance = stopType == RayStopType.None ? _maxRayDistance : stopDistance + distanceEpsilon;
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.collider == lastReflectedCollider) continue;
                if (hit.distance <= minDistance) continue;
                if (hit.distance > effectiveDistance) break;

                TrajectoryHitType hitType = DetermineHitType(hit.collider.gameObject.layer);
                switch (hitType)
                {
                    case TrajectoryHitType.Enemy:
                        _hitEnemyThisFrame = true;
                        HandleEnemyHit(hit);
                        break;

                    case TrajectoryHitType.Destructible:
                        HandleDestructibleHit(hit);
                        break;

                    case TrajectoryHitType.Spear:
                        // Bỏ qua spear của chính mình
                        if (hit.collider.transform.IsChildOf(_ownerRoot) || hit.collider.transform == _ownerRoot)
                            break;
                        // Chỉ đổi màu xanh khi chiếu vào enemy spear
                        var spearCtrl = hit.collider.GetComponentInParent<SpearLogic.SpearController>();
                        if (spearCtrl != null && spearCtrl.isEnemySpear)
                            _hitEnemyThisFrame = true;
                        break;
                }
            }

            // Nếu tia dừng tại BlindMirror thuộc về một CharacterBase (VD: khiên đang đỡ)
            // → track như enemy hit để không gọi HideHitEffect, tránh vòng lặp giật
            if (stopType == RayStopType.BlindMirror && !disableEnemyHitEffects && stopHit.collider != null)
            {
                var shieldCharacter = stopHit.collider.GetComponentInParent<CharacterBase>();
                if (shieldCharacter != null)
                {
                    _hitEnemyThisFrame = true;
                    _enemyHitsCurrentFrame.Add(stopHit.collider);
                    if (!_enemyHitsLastFrame.Contains(stopHit.collider))
                        _hitEnemyCharacters[stopHit.collider] = shieldCharacter;
                }
            }

            if (stopType == RayStopType.Mirror || stopType == RayStopType.Wall || stopType == RayStopType.BlindMirror)
            {
                return new RaycastResult
                {
                    StopType = stopType,
                    Hit = stopHit,
                    HitPoint = stopHit.point
                };
            }

            return new RaycastResult { StopType = RayStopType.None };
        }

        /// <summary>
        /// Thực hiện raycast hoặc circle cast
        /// </summary>
        private List<RaycastHit2D> CastRay(Vector3 start, Vector3 direction, float distance, LayerMask layerMask)
        {
            if (_raycastRadius > 0f)
            {
                return new List<RaycastHit2D>(Physics2D.CircleCastAll(start, _raycastRadius, direction, distance, layerMask));
            }
            return new List<RaycastHit2D>(Physics2D.RaycastAll(start, direction, distance, layerMask));
        }

        /// <summary>
        /// Xác định loại va chạm dựa trên layer
        /// Thứ tự ưu tiên: Mirror > Wall > Enemy > Destructible
        /// </summary>
        private TrajectoryHitType DetermineHitType(int layer)
        {
            if (IsInLayerMask(layer, _mirrorLayer)) return TrajectoryHitType.Mirror;
            if (IsInLayerMask(layer, _blindMirrorLayer)) return TrajectoryHitType.BlindMirror;
            if (IsInLayerMask(layer, _wallLayer)) return TrajectoryHitType.Wall;
            if (IsInLayerMask(layer, _enemyLayer)) return TrajectoryHitType.Enemy;
            if (IsInLayerMask(layer, _destructibleLayer)) return TrajectoryHitType.Destructible;
            if (IsInLayerMask(layer, _spearLayer)) return TrajectoryHitType.Spear;
            return TrajectoryHitType.None;
        }

        private LayerMask CombineAllLayers()
        {
            return _wallLayer.value | _mirrorLayer.value | _blindMirrorLayer.value | _enemyLayer.value | _destructibleLayer.value | _spearLayer.value;
        }

        private static bool IsInLayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        #endregion

        #region Hit Handlers

        /// <summary>
        /// Xử lý khi trúng Enemy - đổi line thành màu xanh
        /// </summary>
        private void HandleEnemyHit(RaycastHit2D hit)
        {
            if (hit.collider == null) return;

            // Luôn track collider để tránh bị mất tracking ở frame kế
            _enemyHitsCurrentFrame.Add(hit.collider);

            // Chỉ gọi effect/event lần đầu khi tia chạm enemy
            if (!_enemyHitsLastFrame.Contains(hit.collider))
            {
                if (!disableEnemyHitEffects)
                {
                    // Dùng GetComponentInParent vì CharacterBase có thể nằm ở parent
                    var characterBase = hit.collider.GetComponentInParent<CharacterBase>();
                    if (characterBase != null)
                    {
                        // Truyền vị trí freeTip để CharacterWithShield so sánh với collider bounds
                        characterBase.ShowHitEffect(hit.point, _currentFreeTipPosition);
                        _hitEnemyCharacters[hit.collider] = characterBase;
                    }
                }

                OnEnemyHit?.Invoke(hit);
            }
        }

        /// <summary>
        /// Xử lý khi trúng Destructible
        /// </summary>
        private void HandleDestructibleHit(RaycastHit2D hit)
        {
            if (hit.collider == null) return;

            _destructibleHitsCurrentFrame.Add(hit.collider);

            if (!_destructibleHitsLastFrame.Contains(hit.collider))
            {
                OnDestructibleHit?.Invoke(hit);
            }
        }

        /// <summary>
        /// Xử lý khi trúng Wall - dừng trajectory
        /// </summary>
        private void HandleWallHit(RaycastHit2D hit)
        {
            OnWallHit?.Invoke(hit);
        }

        #endregion

        #region Line Drawing

        /// <summary>
        /// Tạo LineRenderer
        /// </summary>
        private void CreateLineRenderer(Transform parent)
        {
            if (_trajectoryLinePrefab != null)
            {
                GameObject lineObj = Instantiate(_trajectoryLinePrefab, parent);
                _lineRenderer = lineObj.GetComponent<LineRenderer>();
                if (_lineRenderer == null)
                {
                    _lineRenderer = lineObj.AddComponent<LineRenderer>();
                }
            }
            else
            {
                GameObject lineObj = new GameObject("TrajectoryLine");
                lineObj.transform.SetParent(parent);
                _lineRenderer = lineObj.AddComponent<LineRenderer>();
            }
        }

        /// <summary>
        /// Cấu hình LineRenderer
        /// </summary>
        private void ConfigureLineRenderer()
        {
            if (_lineRenderer == null) return;

            if (_lineRenderer.material == null)
            {
                _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }

            _lineRenderer.positionCount = 2;
            _lineRenderer.startWidth = _trajectoryWidth;
            _lineRenderer.endWidth = _trajectoryWidth;
            _lineRenderer.startColor = _defaultColor;
            _lineRenderer.endColor = _defaultColor;
            _lineRenderer.useWorldSpace = true;
        }

        /// <summary>
        /// Vẽ đường trajectory
        /// </summary>
        private void DrawLine(List<Vector3> points)
        {
            if (_lineRenderer == null || points.Count < 2) return;

            _lineRenderer.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 point = points[i];
                point.z = 0f;
                _lineRenderer.SetPosition(i, point);
            }
        }

        /// <summary>
        /// Cập nhật màu line dựa trên trạng thái va chạm
        /// </summary>
        private void UpdateLineColor()
        {
            if (_lineRenderer == null) return;
            Color targetColor = _hitEnemyThisFrame ? _enemyHitColor : _defaultColor;
            _lineRenderer.startColor = targetColor;
            _lineRenderer.endColor = targetColor;
        }

        #endregion

        #region Hit Tracking

        private void BeginHitTracking()
        {
            _enemyHitsCurrentFrame.Clear();
            _destructibleHitsCurrentFrame.Clear();
        }

        private void EndHitTracking()
        {
            // Enemy rời khỏi raycast hit → kiểm tra khoảng cách từ enemy.position đến đường trajectory
            // Nếu vẫn gần (< _enemyHitExitDistance) → giữ hit state, không gọi HideHitEffect
            foreach (var collider in _enemyHitsLastFrame)
            {
                if (_enemyHitsCurrentFrame.Contains(collider)) continue;

                if (disableEnemyHitEffects)
                {
                    _hitEnemyCharacters.Remove(collider);
                    continue;
                }

                // Collider không bị hit frame này → kiểm tra distance-based
                if (_hitEnemyCharacters.TryGetValue(collider, out var character) && character != null)
                {
                    // Character vẫn đang bị hit qua collider khác (VD: BlindMirror của khiên đang đỡ)
                    // → không gọi HideHitEffect, tránh vòng lặp giật
                    bool stillHitByOtherCollider = false;
                    foreach (var kvp in _hitEnemyCharacters)
                    {
                        if (kvp.Key != collider && kvp.Value == character && _enemyHitsCurrentFrame.Contains(kvp.Key))
                        {
                            stillHitByOtherCollider = true;
                            break;
                        }
                    }
                    if (stillHitByOtherCollider)
                    {
                        _hitEnemyCharacters.Remove(collider);
                        _hitEnemyThisFrame = true;
                        continue;
                    }

                    float dist = DistanceToTrajectoryLine(character.transform.position);
                    if (dist <= _enemyHitExitDistance)
                    {
                        // Vẫn gần đường trajectory → giữ hit state, coi như vẫn hit
                        _enemyHitsCurrentFrame.Add(collider);
                        _hitEnemyThisFrame = true;
                        continue;
                    }

                    // Thực sự rời xa → HideHitEffect
                    character.HideHitEffect();
                    _hitEnemyCharacters.Remove(collider);
                }
            }

            _enemyHitsLastFrame.Clear();
            _enemyHitsLastFrame.UnionWith(_enemyHitsCurrentFrame);

            _destructibleHitsLastFrame.Clear();
            _destructibleHitsLastFrame.UnionWith(_destructibleHitsCurrentFrame);
        }

        /// <summary>
        /// Tính khoảng cách ngắn nhất từ một điểm đến đường trajectory (gồm nhiều đoạn thẳng)
        /// </summary>
        private float DistanceToTrajectoryLine(Vector3 point)
        {
            float minDist = float.MaxValue;
            for (int i = 0; i < _lastTrajectoryPoints.Count - 1; i++)
            {
                float dist = DistancePointToSegment(point, _lastTrajectoryPoints[i], _lastTrajectoryPoints[i + 1]);
                if (dist < minDist) minDist = dist;
            }
            return minDist;
        }

        /// <summary>
        /// Khoảng cách từ điểm P đến đoạn thẳng AB
        /// </summary>
        private static float DistancePointToSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float sqrLen = ab.sqrMagnitude;
            if (sqrLen < 0.0001f) return Vector3.Distance(p, a);

            float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / sqrLen);
            Vector3 closest = a + t * ab;
            return Vector3.Distance(p, closest);
        }

        #endregion
    }
}

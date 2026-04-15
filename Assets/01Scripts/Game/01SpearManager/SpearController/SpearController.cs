using DongA.Characters;
using DongA.Events;
using DongA.Powerups;
using DongA.SpearManager;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DongA.SpearLogic
{
    public partial class SpearController : MonoBehaviour
    {
        [Header("Spear Settings")]
        [SerializeField] private Rigidbody2D _spearRb;
        [SerializeField] public SpearType spearType = SpearType.Basic;
        [HideInInspector] public GameObject elementalAttackProjectilePrefab;
        [HideInInspector] public ElementalAttackType elementalAttackType;
        [HideInInspector] public GameObject currentElementalEffect;
        [HideInInspector] public List<CharacterBase> targetEnemyQueue = new List<CharacterBase>();
        [HideInInspector] public List<GameObject> targetEnemyMarkers = new List<GameObject>();
        [HideInInspector] public bool isTargetingInProgress;
        [HideInInspector] public float targetingSpeed;
        [HideInInspector] public float targetRotationSpeed = 720f;
        [HideInInspector] public float arrivalThreshold = 0.5f;
        private Vector3 _targetAllEnemyDestination;
        private CharacterBase _targetAllEnemyCurrentTarget;
        [SerializeField] private Transform _spearCenter;
        [SerializeField] private Transform _freeTip;
        [SerializeField] private Transform _rotatedTip;
        [SerializeField] private float _rotationOffset = 0f;
        [SerializeField] private float _launchSpeed = 15f;
        [SerializeField] private bool _startStuck = true;

        [Header("Reference")]
        [SerializeField] private SpearCollisionHandler _collisionTipHandler;
        [SerializeField] private GameObject _spearPrefab;
        [SerializeField] private GameObject breakPrefab;

        [Header("Flight Behavior")]
        [SerializeField] private SpearFlightBehavior _flightBehavior;

        [Header("Trajectory Line")]
        [SerializeField] private SpearTrajectoryLine _trajectoryLine;
        

        [Header("Rotation Constraint")]
        [SerializeField] private float _wallCheckFreeTipRadius = 0.1f;
        [SerializeField] private float _minAngleToStuckSurfacePlane = 15f;
        [SerializeField] private float _surfaceNormalProbeDistance = 0.25f;
        [SerializeField] private float _surfaceNormalProbeOutset = 0.05f;
        [SerializeField] private int _constraintBinarySearchIterations = 6;
        [SerializeField] private float _maxRotationSpeed = 360f; // Độ/giây. Set <=0 để dùng giới hạn per-frame
        [SerializeField] private bool _snapToTargetWhileDragging = true;
        [SerializeField] private bool _enableRotationSweepCheck = true;
        [SerializeField] private float _rotationSweepStepAngle = 2f; // Độ mỗi bước khi sweep
        [SerializeField] private LayerMask _wallLayer;
        [SerializeField] private float _maxRotationPerFrame = 5f; // Giới hạn góc xoay tối đa mỗi frame (độ)

        [Header("Mirror Auto-Launch")]
        [SerializeField] private LayerMask _mirrorLayer;
        [SerializeField] private float _mirrorStickDelay = 0.08f;

        private Vector3 _pivotPoint;
        private bool _isStuck = false;
        private bool _isDragging = false;
        private bool _isLaunched = false;
        private Camera _mainCamera;

        private Vector2 _dragMouseDownWorld;
        private Vector2 _dragFreeTipDownWorld;
        private float _dragTransformToSpearDirAngleOffset;

        private float _lastDragRotationDirection;
        private Vector2 _lastDragMouseDelta;

        private Collider2D _stuckSurfaceCollider;
        private Vector2 _stuckSurfaceNormal;
        private bool _hasStuckSurfaceNormal;

        private Collider2D _spearCollider;

        private float _lastLaunchedRotationZ;
        private bool _hasLastLaunchRotation;

        private bool _isAutoLaunchingFromMirror;

        public Transform FreeTip => _freeTip;
        public Transform RotatedTip => _rotatedTip;
        public bool StartStuck { get => _startStuck; set => _startStuck = value; }

        public GameObject SpearPrefab { get => _spearPrefab; set => _spearPrefab = value; }
        public bool IsStuck => _isStuck;
        public bool IsLaunched => _isLaunched;
        public LayerMask WallLayer => _wallLayer;

        [HideInInspector] public bool isEnemySpear = false;
        [HideInInspector] public bool isCloneSpear = false;
        

        private void Awake()
        {
            if (_spearRb == null)
            {
                _spearRb = GetComponent<Rigidbody2D>();
            }

            _spearCollider = GetComponent<Collider2D>();

            _mainCamera = Camera.main;

            if (_flightBehavior == null)
            {
                _flightBehavior = GetComponentInChildren<SpearFlightBehavior>();
            }
            _flightBehavior?.Initialize(this);

            if (_trajectoryLine == null)
            {
                _trajectoryLine = GetComponentInChildren<SpearTrajectoryLine>();
            }
            _trajectoryLine?.Initialize(transform);
        }

        private void Start()
        {
            // Initialize helpers after Awake so serialized fields are ready.
            _pivotPoint = _rotatedTip != null ? _rotatedTip.position : transform.position;
            _rotationSolverInstance = new SpearRotationSolver(this);
            _rotationInteractorInstance = new SpearRotationInteractor(this, _rotationSolverInstance);

            // Enemy spear trajectory line should not trigger enemy scare effects
            if (isEnemySpear && _trajectoryLine != null)
            {
                _trajectoryLine.disableEnemyHitEffects = true;
            }

            if (_startStuck)
            {
                _isStuck = true;
                _isLaunched = false;
                _isDragging = false;
                _pivotPoint = _rotatedTip != null ? _rotatedTip.position : transform.position;

                if (_spearRb != null)
                {
                    _spearRb.velocity = Vector2.zero;
                    _spearRb.angularVelocity = 0f;
                    _spearRb.bodyType = RigidbodyType2D.Static;
                }

                if (_spearCollider != null)
                {
                    _spearCollider.isTrigger = true;
                }

                if (_collisionTipHandler != null)
                {
                    _collisionTipHandler.EnableCollider = false;
                }
            }
            else
            {
                if (_spearCollider != null)
                {
                    _spearCollider.isTrigger = false;
                }
                if (_collisionTipHandler != null)
                {
                    _collisionTipHandler.EnableCollider = true;
                }
            }
        }

        private SpearRotationInteractor _rotationInteractorInstance;
        private SpearRotationSolver _rotationSolverInstance;

        private void Update()
        {
            if(GamePrefs.IsGamePaused)return;
            // Stuck: allow rotate/aim.
            if (_isStuck && !_isLaunched && !_isAutoLaunchingFromMirror)
            {
                // Enemy spear rotation is handled by SpearEnemyAI
                if (!isEnemySpear)
                {
                    _rotationInteractorInstance?.Tick();
                }
                return;
            }
        }

        private void FixedUpdate()
        {
            if (_isLaunched)
            {
                _lastLaunchedRotationZ = transform.eulerAngles.z;
                _hasLastLaunchRotation = true;
            }

            // Distance-based arrival check for TargetAllEnemy
            if (isTargetingInProgress && spearType == SpearType.TargetAllEnemy && _targetAllEnemyCurrentTarget != null)
            {
                // Update destination to track moving enemies
                if (_targetAllEnemyCurrentTarget.gameObject.activeInHierarchy)
                {
                    var col = _targetAllEnemyCurrentTarget.GetComponentInChildren<Collider2D>();
                    if (col != null)
                        _targetAllEnemyDestination = col.bounds.center;
                }

                float dist = Vector2.Distance(_spearCenter.position, _targetAllEnemyDestination);
                if (dist < arrivalThreshold)
                {
                    OnArriveAtEnemyTarget();
                }
            }
        }
        public void OnSpearTipCollision(Vector3 collisionPoint, Collider2D surfaceCollider, Vector2 surfaceNormal)
        {
            if (!_isLaunched || _isStuck)
                return;

            if (_freeTip == null || _rotatedTip == null)
                return;

            FlipAndEnterStuckState(collisionPoint, surfaceCollider, surfaceNormal);
        }

        /// <summary>
        /// Gọi khi spear tip va chạm gương.
        /// Stick vào gương như bình thường, sau đó tự động launch tới waypoint tiếp theo.
        /// </summary>
        public void OnMirrorTipCollision(Vector3 collisionPoint, Collider2D surfaceCollider, Vector2 surfaceNormal)
        {
            if (!_isLaunched || _isStuck)
                return;

            if (_freeTip == null || _rotatedTip == null)
                return;

            // Lưu hướng bay trước khi FlipAndEnterStuckState thay đổi state
            Vector2 incomingDirection = ((Vector2)_freeTip.position - (Vector2)_rotatedTip.position).normalized;

            FlipAndEnterStuckState(collisionPoint, surfaceCollider, surfaceNormal);

            // Luôn dùng phản xạ động từ collision normal thực tế thay vì waypoint cố định.
            // Waypoint có thể sai khi mirror/wall quá gần nhau (collision point lệch so với dự đoán)
            // dẫn đến spear bay sai hướng hoặc stuck trong surface.
            if (surfaceNormal.sqrMagnitude > 0.0001f)
            {
                Vector2 reflectedDirection = Vector2.Reflect(incomingDirection, surfaceNormal.normalized);
                _isAutoLaunchingFromMirror = true;
                StartCoroutine(AutoLaunchReflectedCoroutine(reflectedDirection));
            }
        }

        private IEnumerator AutoLaunchReflectedCoroutine(Vector2 reflectedDirection)
        {
            if (_mirrorStickDelay > 0f)
                yield return new WaitForSeconds(_mirrorStickDelay);
            else
                yield return null;

            if (reflectedDirection.sqrMagnitude < 0.0001f)
                yield break;

            AlignSpearForDirection(reflectedDirection);
            Launch(reflectedDirection);
        }


        /// <summary>
        /// Xoay spear quanh rotatedTip sao cho hướng (rotatedTip -> freeTip) trùng với direction
        /// </summary>
        private void AlignSpearForDirection(Vector2 direction)
        {
            if (_freeTip == null || _rotatedTip == null)
                return;

            Vector2 currentDir = ((Vector2)_freeTip.position - (Vector2)_rotatedTip.position).normalized;
            if (currentDir.sqrMagnitude < 0.0001f)
                return;

            float deltaAngle = Vector2.SignedAngle(currentDir, direction);
            transform.RotateAround(_rotatedTip.position, Vector3.forward, deltaAngle);
        }

        /// <summary>
        /// Redirect spear mid-flight toward a world position (used by TargetAllEnemy powerup).
        /// Does not go through the full Launch() to avoid storing mirror waypoints.
        /// </summary>
        public void RedirectToTarget(Vector3 targetWorldPos)
        {
            if (_freeTip == null || _rotatedTip == null || _spearRb == null)
                return;

            Vector2 direction = ((Vector2)targetWorldPos - (Vector2)_rotatedTip.position).normalized;
            if (direction.sqrMagnitude < 0.0001f)
                return;

            _isAutoLaunchingFromMirror = false;

            AlignSpearForDirection(direction);

            float speed = targetingSpeed > 0f ? targetingSpeed : _launchSpeed;
            if (_flightBehavior != null)
                _flightBehavior.Launch(_spearRb, direction, speed);
            else
                _spearRb.velocity = direction * speed;
        }

        private void FlipAndEnterStuckState(Vector3 collisionPoint, Collider2D surfaceCollider, Vector2 surfaceNormal)
        {
            SetRigidbodyToStatic(true);

            if (_spearCollider != null)
            {
                _spearCollider.isTrigger = true;
            }

            // Khôi phục góc bay ngay trước khi va chạm để giữ nguyên góc chéo.
            if (_hasLastLaunchRotation)
            {
                transform.rotation = Quaternion.Euler(0f, 0f, _lastLaunchedRotationZ);
            }

            _spearCenter.Rotate(0f, 0f, 180f);

            if (_rotatedTip != null)
            {
                Vector3 tipDelta = collisionPoint - _rotatedTip.position;
                if (tipDelta.sqrMagnitude > 0.000001f)
                {
                    transform.position += tipDelta;
                }
            }

            Physics2D.SyncTransforms();      

            _pivotPoint = _rotatedTip.position;

            _isStuck = true;
            _isLaunched = false;
            _isDragging = false;

            _hasLastLaunchRotation = false;

            _stuckSurfaceCollider = surfaceCollider;
            _hasStuckSurfaceNormal = surfaceNormal.sqrMagnitude > 0.0001f;
            _stuckSurfaceNormal = _hasStuckSurfaceNormal ? surfaceNormal.normalized : default;

            // While stuck, we don't want the free-tip collision handler to keep generating stuck events.
            if (_collisionTipHandler != null)
            {
                _collisionTipHandler.EnableCollider = false;
            }
        }
        private void ResetForLaunch()
        {
            var previousStuckSurface = _stuckSurfaceCollider;

            _isStuck = false;
            _isDragging = false;
            _stuckSurfaceCollider = null;
            _hasStuckSurfaceNormal = false;
            _stuckSurfaceNormal = default;

            if (_spearRb != null)
            {
                SetRigidbodyToStatic(false);     
            }

            if (_spearCollider != null)
            {
                _spearCollider.isTrigger = false;
            }

            if (_collisionTipHandler != null)
            {
                // When auto-launching from mirror, only ignore wall colliders (not mirrors)
                // to prevent skipping the NEXT mirror when mirrors are close together
                var overlappingColliders = GetOverlappingWallColliders(wallOnly: _isAutoLaunchingFromMirror);
                if (overlappingColliders != null && overlappingColliders.Length > 0)
                {
                    _collisionTipHandler.IgnoreCollidersOnNextHit(overlappingColliders);
                }
                else if (previousStuckSurface != null)
                {
                    _collisionTipHandler.IgnoreColliderOnNextHit(previousStuckSurface);
                }

                _collisionTipHandler.ResetSpearWhenLaunch();
            }
        }

        private void LaunchFromCurrentPose()
        {
            if (_spearRb == null || _freeTip == null || _rotatedTip == null)
                return;

            Vector2 dir = ((Vector2)_freeTip.position - (Vector2)_rotatedTip.position);
            Launch(dir.normalized);
        }

        public void Launch(Vector2 direction)
        {
            if (_spearRb == null)
                return;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            ResetForLaunch();
            _isAutoLaunchingFromMirror = false;
            _isLaunched = true;
            if (_spearCollider != null)
            {
                _spearCollider.enabled = true;
            }
            if (_flightBehavior != null)
            {
                _flightBehavior.Launch(_spearRb, direction.normalized, _launchSpeed);
            }
            else
            {
                _spearRb.velocity = direction.normalized * _launchSpeed;
            }
        }


        private void SetRigidbodyToStatic(bool value)
        {
            if (_spearRb != null)
            {
                _spearRb.velocity = Vector2.zero;
                _spearRb.angularVelocity = 0f;
                if(value)
                    _spearRb.bodyType = RigidbodyType2D.Static;
                else
                {
                    _spearRb.bodyType = RigidbodyType2D.Dynamic;
                    _spearRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                }
            }
        }

        private Collider2D[] GetOverlappingWallColliders(bool wallOnly = false)
        {
            var contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(wallOnly ? _wallLayer : (_wallLayer | _mirrorLayer));
            contactFilter.useTriggers = false;
            
            var results = new List<Collider2D>();
            
            // CHỈ check xung quanh rotatedTip (điểm spear đang STUCK)
            // Không check freeTip vì đó là hướng spear sẽ bay đi - không nên ignore những tường ở đó
            if (_rotatedTip != null)
            {
                var tipResults = new List<Collider2D>();
                // Sử dụng radius nhỏ hơn để chỉ detect tường mà spear thực sự stuck vào
                Physics2D.OverlapCircle(_rotatedTip.position, _wallCheckFreeTipRadius, contactFilter, tipResults);
                foreach (var col in tipResults)
                {
                    if (!results.Contains(col))
                        results.Add(col);
                }
            }
            
            return results.ToArray();
        }

        #region Enemy Spear AI Control

        /// <summary>
        /// Hiển thị/ẩn trajectory line (dùng cho SpearEnemyAI)
        /// </summary>
        public void ShowTrajectoryLine(bool show)
        {
            _trajectoryLine?.Show(show);
        }

        /// <summary>
        /// Cập nhật trajectory line từ bên ngoài (dùng cho SpearEnemyAI)
        /// </summary>
        public void UpdateTrajectoryFromOutside()
        {
            if (_trajectoryLine == null || _freeTip == null || _rotatedTip == null) return;
            _trajectoryLine.UpdateLine(_rotatedTip.position, _freeTip.position, _wallLayer);
        }

        /// <summary>
        /// Xoay spear quanh rotatedTip theo một góc delta (độ). Dùng cho AI control.
        /// Sử dụng SpearRotationSolver để block khi freeTip chạm tường (giống player).
        /// </summary>
        public void RotateAroundPivot(float deltaAngleDegrees)
        {
            if (_rotatedTip == null) return;
            _pivotPoint = _rotatedTip.position;

            if (_rotationSolverInstance != null && _isStuck)
            {
                float targetAngle = transform.eulerAngles.z + deltaAngleDegrees;
                _rotationSolverInstance.ApplyTargetRotationAroundPivot(targetAngle);
            }
            else
            {
                transform.RotateAround(_rotatedTip.position, Vector3.forward, deltaAngleDegrees);
            }
        }

        /// <summary>
        /// Lấy hướng hiện tại của spear (rotatedTip → freeTip)
        /// </summary>
        public Vector2 GetAimDirection()
        {
            if (_freeTip == null || _rotatedTip == null) return Vector2.right;
            return ((Vector2)_freeTip.position - (Vector2)_rotatedTip.position).normalized;
        }

        /// <summary>
        /// Launch spear theo hướng hiện tại (dùng cho SpearEnemyAI)
        /// </summary>
        public void LaunchCurrentDirection()
        {
            if (!_isStuck || _isLaunched) return;
            ShowTrajectoryLine(false);
            LaunchFromCurrentPose();
        }

        #endregion

        #region TargetAllEnemy Logic

        /// <summary>
        /// Tìm enemy gần nhất trong queue và bắt đầu bay tới tâm collider của nó.
        /// </summary>
        /// <summary>
        /// Bật/tắt trigger trên cả body collider và tip collider.
        /// Khi targeting, set trigger = true để spear xuyên qua tường về mặt vật lý.
        /// </summary>
        private void SetCollidersForTargeting(bool targeting)
        {
            if (_spearCollider != null)
                _spearCollider.isTrigger = targeting;

            if (_collisionTipHandler != null)
            {
                var tipCol = _collisionTipHandler.GetComponent<Collider2D>();
                if (tipCol != null) tipCol.isTrigger = targeting;
            }
        }

        public void StartTargetingNearestEnemy()
        {
            PurgeStaleTargets();

            CharacterBase nearest = FindNearestEnemy();
            if (nearest != null)
            {
                isTargetingInProgress = true;
                SetCollidersForTargeting(true);
                _targetAllEnemyCurrentTarget = nearest;

                var col = nearest.GetComponentInChildren<Collider2D>();
                _targetAllEnemyDestination = col != null ? col.bounds.center : nearest.transform.position;

                RedirectToTarget(_targetAllEnemyDestination);
            }
            else
            {
                FinishTargetAllEnemy();
            }
        }

        private CharacterBase FindNearestEnemy()
        {
            if (targetEnemyQueue.Count == 0) return null;

            Vector2 spearPos = _freeTip != null ? (Vector2)_freeTip.position : (Vector2)transform.position;
            CharacterBase nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var enemy in targetEnemyQueue)
            {
                if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;
                var col = enemy.GetComponentInChildren<Collider2D>();
                Vector2 enemyPos = col != null ? (Vector2)col.bounds.center : (Vector2)enemy.transform.position;
                float dist = Vector2.Distance(spearPos, enemyPos);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }
            return nearest;
        }

        private void OnArriveAtEnemyTarget()
        {
            // Kill the current target
            if (_targetAllEnemyCurrentTarget != null && _targetAllEnemyCurrentTarget.gameObject.activeInHierarchy)
            {
                Vector2 spearDir = ((Vector2)_freeTip.position - (Vector2)_rotatedTip.position).normalized;
                _targetAllEnemyCurrentTarget.DeadAndSlice(spearDir);
            }

            // Remove from queue + marker
            RemoveFromTargetQueue(_targetAllEnemyCurrentTarget);
            _targetAllEnemyCurrentTarget = null;

            // Stop flight, then find next
            StopFlight();

            PurgeStaleTargets();
            CharacterBase next = FindNearestEnemy();

            if (next != null)
            {
                StartCoroutine(TargetNextEnemyCoroutine(next));
            }
            else
            {
                // Hết enemy — bay tiếp theo hướng hiện tại cho đến khi chạm tường
                FinishTargetAllEnemyAndContinueFlight();
            }
        }

        private IEnumerator TargetNextEnemyCoroutine(CharacterBase next)
        {
            // 1 frame pause tại vị trí enemy vừa kill
            yield return null;

            // Check lại enemy còn valid không
            if (next == null || !next.gameObject.activeInHierarchy)
            {
                PurgeStaleTargets();
                next = FindNearestEnemy();
                if (next == null)
                {
                    FinishTargetAllEnemyAndContinueFlight();
                    yield break;
                }
            }

            _targetAllEnemyCurrentTarget = next;
            var col = next.GetComponentInChildren<Collider2D>();
            _targetAllEnemyDestination = col != null ? col.bounds.center : next.transform.position;

            // Smooth rotate quanh tâm spear hướng về enemy tiếp theo
            Vector2 toTarget = ((Vector2)_targetAllEnemyDestination - (Vector2)_spearCenter.position).normalized;
            float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

            while (true)
            {
                Vector2 currentDir = GetAimDirection();
                float currentAngle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;
                float remaining = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));

                if (remaining < 1f)
                {
                    // Snap cuối cùng quanh tâm spear
                    float snapDelta = Mathf.DeltaAngle(currentAngle, targetAngle);
                    transform.RotateAround(_spearCenter.position, Vector3.forward, snapDelta);
                    break;
                }

                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, targetRotationSpeed * Time.deltaTime);
                float delta = Mathf.DeltaAngle(currentAngle, newAngle);
                transform.RotateAround(_spearCenter.position, Vector3.forward, delta);

                yield return null;
            }

            // Bay tới enemy tiếp theo
            RedirectToTarget(_targetAllEnemyDestination);
        }

        private void StopFlight()
        {
            if (_flightBehavior != null)
                _flightBehavior.Stop(_spearRb);
            else if (_spearRb != null)
            {
                _spearRb.velocity = Vector2.zero;
                _spearRb.angularVelocity = 0f;
            }
        }

        private void RemoveFromTargetQueue(CharacterBase character)
        {
            int idx = targetEnemyQueue.IndexOf(character);
            if (idx >= 0)
            {
                targetEnemyQueue.RemoveAt(idx);
                if (idx < targetEnemyMarkers.Count)
                {
                    var marker = targetEnemyMarkers[idx];
                    if (marker != null) Destroy(marker);
                    targetEnemyMarkers.RemoveAt(idx);
                }
            }
        }

        private void PurgeStaleTargets()
        {
            for (int i = targetEnemyQueue.Count - 1; i >= 0; i--)
            {
                if (targetEnemyQueue[i] == null || !targetEnemyQueue[i].gameObject.activeInHierarchy)
                {
                    if (i < targetEnemyMarkers.Count)
                    {
                        var m = targetEnemyMarkers[i];
                        if (m != null) Destroy(m);
                        targetEnemyMarkers.RemoveAt(i);
                    }
                    targetEnemyQueue.RemoveAt(i);
                }
            }
        }

        public void FinishTargetAllEnemy()
        {
            isTargetingInProgress = false;
            _targetAllEnemyCurrentTarget = null;
            spearType = SpearType.Basic;
            SetCollidersForTargeting(false);
            if (currentElementalEffect != null)
            {
                Destroy(currentElementalEffect);
                currentElementalEffect = null;
            }
        }

        /// <summary>
        /// Hết enemy — reset state nhưng tiếp tục bay theo hướng hiện tại cho đến khi chạm tường.
        /// </summary>
        private void FinishTargetAllEnemyAndContinueFlight()
        {
            isTargetingInProgress = false;
            _targetAllEnemyCurrentTarget = null;
            spearType = SpearType.Basic;
            SetCollidersForTargeting(false);
            if (currentElementalEffect != null)
            {
                Destroy(currentElementalEffect);
                currentElementalEffect = null;
            }

            // Tiếp tục bay theo hướng hiện tại — spear sẽ tự stuck khi chạm tường
            Vector2 currentDir = GetAimDirection();
            float speed = targetingSpeed > 0f ? targetingSpeed : _launchSpeed;
            if (_flightBehavior != null)
                _flightBehavior.Launch(_spearRb, currentDir, speed);
            else if (_spearRb != null)
                _spearRb.velocity = currentDir * speed;
        }

        #endregion

        private void OnDrawGizmos()
        {
            if (_freeTip == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_freeTip.position, _wallCheckFreeTipRadius);
        }

        public void DestroyWithBreakEffect()
        {
            if (breakPrefab != null)
            {
                var breakObj = Instantiate(breakPrefab, transform.position, transform.rotation);
                var root = GameObject.Find(KeyPrefs.LevelRootName);
                if (root != null)
                    breakObj.transform.SetParent(root.transform);
                var breakSpear = breakObj.GetComponent<BreakSpear>();
                if (breakSpear != null)
                {
                    breakSpear.Initialize(isEnemySpear);
                }
            }
            Destroy(gameObject);
        }

        #region MultiSlice Integration

        private void OnEnable()
        {
            GameEvents.OnMultiSliceEffectStart += OnMultiSliceStart;
            GameEvents.OnMultiSliceEffectDrop  += OnMultiSliceDrop;
        }

        private void OnDisable()
        {
            GameEvents.OnMultiSliceEffectStart -= OnMultiSliceStart;
            GameEvents.OnMultiSliceEffectDrop  -= OnMultiSliceDrop;
        }

        // Ẩn spear, dừng hoàn toàn trong khi hiệu ứng chạy
        private void OnMultiSliceStart()
        {
            if (isEnemySpear) return;

            StopMoving();
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;

            if (_spearRb != null)
            {
                _spearRb.velocity        = Vector2.zero;
                _spearRb.angularVelocity = 0f;
                _spearRb.bodyType        = RigidbodyType2D.Static;
            }
            if (_spearCollider != null)    _spearCollider.enabled = false;
            if (_collisionTipHandler != null) _collisionTipHandler.EnableCollider = false;
        }

        private void OnMultiSliceDrop(Vector3 topPosition)
        {
            if (isEnemySpear) return;

            // Hiện lại spear
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = true;

            // Teleport về vị trí top (z giữ nguyên)
            transform.position = new Vector3(topPosition.x, topPosition.y, transform.position.z);

            // Xoay spear xuống dưới (freeTip chỉ xuống)
            transform.rotation = Quaternion.Euler(0f, 0f, 180f);

            // Tắt trajectory line trước khi launch
            ShowTrajectoryLine(false);

            // Kích hoạt lại colliders rồi launch thẳng xuống
            if (_spearCollider != null)    _spearCollider.enabled = true;

            Launch(Vector2.down);
        }

        private void StopMoving()
        {
            DOTween.Kill(transform);
            DOTween.Kill(_spearRb);
        }

        #endregion
    }
    
    public enum SpearType
    {
        Basic,
        Cut,
        ElementalAttack,
        Ice,
        Thunder,
        TargetAllEnemy,
        Fire
    }
}


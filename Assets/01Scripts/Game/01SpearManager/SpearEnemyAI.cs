using DongA.SpearLogic;
using DongA.SpearManager;
using Spine.Unity;
using UnityEngine;

namespace DongA.EnemySpear
{
    /// <summary>
    /// Companion component đặt cùng SpearController trên spear enemy.
    /// Kéo prefab spear vào scene, thêm component này → spear tự xoay, bắn, ghim tường, lặp lại.
    ///
    /// Lifecycle: Stuck → Xoay (anim dragging) → Bắn → Bay → Ghim tường → Stuck → Lặp lại
    /// </summary>
    [RequireComponent(typeof(SpearController))]
    public class SpearEnemyAI : MonoBehaviour
    {
        [Header("Aim Settings")]
        [SerializeField] private float _minShootInterval = 2f;
        [SerializeField] private float _maxShootInterval = 5f;
        [SerializeField] private float _rotationSpeed = 90f;
        [Tooltip("0 = hoàn toàn random, 1 = luôn nhắm player")]
        [SerializeField][Range(0f, 1f)] private float _playerTrackingWeight = 0.7f;

        private SpearController _spear;
        private SkeletonAnimation _skeletonAnimation;
        private SpearAnimManager _spearAnimManager;
        private float _shootTimer;
        private float _randomTargetAngle;
        private SpearController _cachedPlayerSpear;
        private bool _isAiming;

        private void Awake()
        {
            _spear = GetComponent<SpearController>();
            _spear.isEnemySpear = true;

            _skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();

            // Tắt SpearAnimManager nếu có (tránh nó react global events của player spear)
            _spearAnimManager = GetComponent<SpearAnimManager>();
            if (_spearAnimManager != null)
            {
                _spearAnimManager.enabled = false;
            }
        }

        private void Start()
        {
            PickRandomTargetAngle();
        }

        private void Update()
        {
            if (GamePrefs.IsGamePaused) return;
            if (_spear == null) return;

            // Chỉ hoạt động khi spear đang stuck (không bay)
            if (!_spear.IsStuck || _spear.IsLaunched)
            {
                _isAiming = false;
                return;
            }

            // Bắt đầu aim nếu chưa
            if (!_isAiming)
            {
                StartAiming();
            }

            CachePlayerSpear();
            RotateSpear();
            _spear.UpdateTrajectoryFromOutside();

            // Countdown → bắn
            _shootTimer -= Time.deltaTime;
            if (_shootTimer <= 0f)
            {
                Shoot();
            }
        }

        #region Aiming Phase

        private void StartAiming()
        {
            _isAiming = true;
            _shootTimer = Random.Range(_minShootInterval, _maxShootInterval);
            PickRandomTargetAngle();

            // Hiện trajectory line + chạy anim dragging
            _spear.ShowTrajectoryLine(true);
            PlayAnimation(SpearAnimValue.dragging, true);
        }

        private void Shoot()
        {
            _isAiming = false;

            // Chạy anim idle (giống khi player bắn)
            PlayAnimation(SpearAnimValue.idle, true);

            // Launch spear theo hướng hiện tại
            _spear.LaunchCurrentDirection();

            // Sau khi launch → spear bay → ghim tường → IsStuck = true → StartAiming() lại
        }

        #endregion

        #region Rotation

        private void RotateSpear()
        {
            // Tính hướng hiện tại
            Vector2 currentDir = _spear.GetAimDirection();
            float currentAngle = Mathf.Atan2(currentDir.y, currentDir.x) * Mathf.Rad2Deg;

            // Target angle: blend giữa random và hướng player spear
            float targetAngle = _randomTargetAngle;
            if (_cachedPlayerSpear != null)
            {
                Vector2 toPlayer = (Vector2)_cachedPlayerSpear.transform.position
                                 - (Vector2)_spear.RotatedTip.position;
                float playerAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
                targetAngle = Mathf.LerpAngle(_randomTargetAngle, playerAngle, _playerTrackingWeight);
            }

            // Xoay mượt
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, _rotationSpeed * Time.deltaTime);
            float deltaAngle = Mathf.DeltaAngle(currentAngle, newAngle);

            _spear.RotateAroundPivot(deltaAngle);
        }

        private void PickRandomTargetAngle()
        {
            _randomTargetAngle = Random.Range(0f, 360f);
        }

        #endregion

        #region Player Tracking

        private void CachePlayerSpear()
        {
            // Chỉ tìm lại khi cache bị null hoặc reference sai
            if (_cachedPlayerSpear != null && !_cachedPlayerSpear.isEnemySpear) return;

            _cachedPlayerSpear = null;
            var allSpears = FindObjectsOfType<SpearController>();
            foreach (var s in allSpears)
            {
                if (!s.isEnemySpear)
                {
                    _cachedPlayerSpear = s;
                    break;
                }
            }
        }

        #endregion

        #region Animation

        private void PlayAnimation(string animName, bool loop)
        {
            if (_skeletonAnimation == null) return;
            _skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
        }

        #endregion
    }
}

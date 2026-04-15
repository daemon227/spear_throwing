using System.Collections.Generic;
using DongA.Characters;
using DongA.Obstacles;
using DongA.Powerups;
using DongA.SpearLogic;
using GameBase;
using Kilt.EasyRopes2D;
using UnityEngine;


namespace DongA.SpearManager
{
public class SpearCollisionHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _collisionLayerMask; 
    [SerializeField] private LayerMask _mirrorLayer;
    [SerializeField] private LayerMask _blindMirrorLayer;
    [SerializeField] private Vector2 _boxColliderSize = new Vector2(0.1f, 0.1f);
    [SerializeField] private float _clearIgnoreAfterDistance = 1.0f; // Distance after which ignored colliders are cleared
    [SerializeField] private float _obstacleHitForce = 2f;
    private bool _enableCollider = true;
    private Collider2D _spearBodyCollider;
    private Collider2D _lastCollider;
    private Collider2D _lastMirrorCollider;
    private SpearController _spearController;
    private HashSet<Collider2D> _ignoredColliders = new HashSet<Collider2D>();
    private Vector3 _launchPosition; // Position when spear was launched
    private bool _hasLaunchPosition = false;
    public bool EnableCollider { get => _enableCollider; set => _enableCollider = value; }
    public HashSet<Collider2D> IgnoredColliders => _ignoredColliders;

    private Collider2D _tipCollider;

    private void Awake()
    {
        _spearController = GetComponentInParent<SpearController>();
        _spearBodyCollider = _spearController.GetComponent<Collider2D>();
        _tipCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Bật/tắt physics ignore giữa spear colliders và một collider khác.
    /// Khi ignore = true, spear đi xuyên qua collider đó (không bị chặn vật lý).
    /// Chỉ áp dụng cho mirror/blindMirror — wall vẫn giữ chặn vật lý bình thường
    /// để spear không thể xuyên qua tường mà nó đang stuck.
    /// </summary>
    private void SetPhysicsIgnore(Collider2D col, bool ignore)
    {
        if (col == null) return;
        // Chỉ physics-ignore mirror/blindMirror, KHÔNG ignore wall
        // Wall cần giữ chặn vật lý để spear không xuyên qua
        if (!IsInMirrorLayer(col) && !IsInBlindMirrorLayer(col)) return;
        if (_spearBodyCollider != null)
            Physics2D.IgnoreCollision(_spearBodyCollider, col, ignore);
        if (_tipCollider != null && _tipCollider != _spearBodyCollider)
            Physics2D.IgnoreCollision(_tipCollider, col, ignore);
    }

    private void ClearIgnoredCollidersInternal()
    {
        foreach (var col in _ignoredColliders)
            SetPhysicsIgnore(col, false);
        _ignoredColliders.Clear();
    }

    private void AddIgnoredCollider(Collider2D col)
    {
        if (col == null) return;
        _ignoredColliders.Add(col);
        SetPhysicsIgnore(col, true);
    }

        #region Collider2D Methods

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(!EnableCollider) return;

        // Ignore wall/mirror while spear is chaining between enemies
        if (_spearController.isTargetingInProgress)
            return;

        var hitCollider = collision.collider;
        var contact = collision.contacts[0];

        // Gương / BlindMirror ưu tiên trước tường
        if (IsInMirrorLayer(hitCollider) || IsInBlindMirrorLayer(hitCollider))
        {
            if (hitCollider == _lastCollider) return;
            if (_ignoredColliders.Contains(hitCollider)) return;
            _lastCollider = hitCollider;
            // Debug.Log($"[SpearCollision] OnCollisionEnter2D -> Mirror/BlindMirror hit: {hitCollider.name}, point: {contact.point}, normal: {contact.normal}");

            // Nếu object vừa là blindMirror vừa có tag Obstacle → gọi BreakObstacle + đẩy lực trước khi phản xạ
            if (hitCollider.CompareTag(GameTags.TObstacle))
            {
                var obstacle = hitCollider.GetComponentInParent<ObstacleBase>();
                if (obstacle != null)
                {
                    obstacle.BreakObstacle();

                    // Dùng -contact.normal (hướng spear đâm vào) * lực cố định để tạo độ văng ngay lần đầu bị bắn
                    var obstacleRb = obstacle.GetComponent<Rigidbody2D>();
                    if (obstacleRb != null)
                    {
                        obstacleRb.velocity = -contact.normal * _obstacleHitForce;
                    }
                }
            }

            HandleMirrorCollision(contact.point, hitCollider, contact.normal);
            return;
        }
        GameEvents.OnSpearHitWallEffect?.Invoke(GetWallSide(contact.normal), contact.point);
        HandleHit(hitCollider, contact.point, contact.normal);
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.collider == _lastCollider)
        {
            _lastCollider = null;
        }
    }

    void FixedUpdate()
    {
        if(EnableCollider)
        {
            CheckColliderOverlap();
            CheckClearIgnoredCollidersAfterDistance();
        }
    }

    /// <summary>
    /// Clear ignored colliders after the spear has traveled a safe distance from launch point.
    /// This prevents the spear from passing through walls that should be solid.
    /// </summary>
    private void CheckClearIgnoredCollidersAfterDistance()
    {
        if (!_hasLaunchPosition || _ignoredColliders.Count == 0)
            return;
            
        float distanceTraveled = Vector3.Distance(transform.position, _launchPosition);
        if (distanceTraveled >= _clearIgnoreAfterDistance)
        {
            ClearIgnoredCollidersInternal();
            _lastMirrorCollider = null;
            _hasLaunchPosition = false;
        }
    }

    private void CheckColliderOverlap()
    {
        // Sử dụng LayerMask để chỉ detect những layer cần thiết
        Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(
            transform.position,
            _boxColliderSize,
            _spearController.transform.eulerAngles.z
        );

        foreach (var collider in overlappingColliders)
        {
            if (collider == _spearBodyCollider)
                continue;

            if (_ignoredColliders.Contains(collider))
                continue;

            if (collider == _lastCollider)
                continue;

            // --- Spear-vs-Spear collision ---
            var otherSpear = collider.GetComponentInParent<SpearController>();
            if (otherSpear != null && otherSpear != _spearController)
            {
                // Enemy spear trúng player spear → chỉ destroy player spear, enemy spear sống
                if (_spearController.isEnemySpear && !otherSpear.isEnemySpear)
                {
                    otherSpear.DestroyWithBreakEffect();
                    // Only trigger lose when no player spears remain
                    bool anyPlayerSpearLeft = false;
                    foreach (var spear in FindObjectsByType<SpearController>(FindObjectsSortMode.None))
                    {
                        if (!spear.isEnemySpear && spear != otherSpear)
                        {
                            anyPlayerSpearLeft = true;
                            break;
                        }
                    }
                    if (!anyPlayerSpearLeft && GameManager.Instance != null)
                        GameManager.Instance.TriggerLose();
                    return;
                }
                // Player spear trúng enemy spear → destroy enemy spear
                else if (!_spearController.isEnemySpear && otherSpear.isEnemySpear)
                {
                    Vector3 enemySpearPos = otherSpear.transform.position;
                    otherSpear.DestroyWithBreakEffect();
                    if (GameManager.Instance != null)
                        GameManager.Instance.DecreaseEnemyCount(enemySpearPos);
                }
                continue;
            }

            // Enemy spear không tương tác với enemy/obstacle/powerup
            if (_spearController.isEnemySpear)
                continue;

            if(collider.CompareTag(GameTags.TEnemy))
            {
                // During chain attack, distance-based arrival in SpearController handles kills
                if (_spearController.spearType == SpearType.TargetAllEnemy && _spearController.isTargetingInProgress)
                    continue;

                var character = collider.gameObject.GetComponentInParent<CharacterBase>();
                if(character != null && !character.IsSpawnInvincible)
                {
                    ColliderHandler(character);
                    // Debug.Log("Check Collider Overlap: Hit Enemy.");
                }
            }

            if (collider.CompareTag(GameTags.TObstacle))
            {
                var obstacle = collider.GetComponentInParent<ObstacleBase>();
                if (obstacle != null)
                {
                    obstacle.BreakObstacle();
                }
            }
            
            if(collider.CompareTag(GameTags.TPowerup))
            {
                var powerup = collider.GetComponentInParent<PowerupBase>();
                if(powerup != null)
                {
                    Rigidbody2D rb = _spearController.GetComponent<Rigidbody2D>();
                    Vector2 dir = (rb != null && rb.velocity.sqrMagnitude > 0.01f)
                        ? rb.velocity.normalized
                        : (Vector2)(_spearController.FreeTip.position - _spearController.RotatedTip.position).normalized;
                    powerup.PowerupReceived(_spearController.SpearPrefab, dir);
                }
            }

            // Cắt rope khi spear bay trúng node của dây
            var node = collider.GetComponent<Node2D>();
            if (node != null)
            {
                node.Cut();
            }
        }
    }

#endregion

    #region Collision Handling
    private void HandleCollision(Vector3 collisionPoint, Collider2D hitCollider, Vector2 hitNormal)
    {
        if(_spearController == null) return;
        
        // Check for any enemies/obstacles at impact point before disabling collider
        // This ensures enemies near walls are still processed
        CheckColliderOverlap();
        
        _spearController.OnSpearTipCollision(collisionPoint, hitCollider, hitNormal);
        ResetCollision();
        GameEvents.OnSpearStuck?.Invoke();
        if (VibrationsManager.Instance != null) VibrationsManager.Instance.TriggerMediumImpact();
    }


    public void ResetCollision()
    {
        //_collider.enabled = true; // Kích hoạt lại collider khi reset
        EnableCollider = false;
        // Clear ignored colliders when spear sticks to prevent issues on next launch
        ClearIgnoredCollidersInternal();
        _lastCollider = null;
    }
    public void ResetSpearWhenLaunch()
    {
        EnableCollider = true;
        // If spear was previously reflected by a mirror, keep ignoring it until far enough away
        if (_lastMirrorCollider != null)
        {
            AddIgnoredCollider(_lastMirrorCollider);
        }
        // Store launch position to calculate distance traveled
        _launchPosition = transform.position;
        _hasLaunchPosition = true;
    }
    private void HandleHit(Collider2D hit, Vector2 collisionPoint, Vector2 collisionNormal)
    {
        if (hit == null)
            return;
        // if (_collisionLayerMask.value != 0 && ((_collisionLayerMask.value & (1 << hit.gameObject.layer)) == 0))
        //     return;
        if( hit == _lastCollider)
            return;
        if (_ignoredColliders.Contains(hit))
            return;
        if (hit.CompareTag(GameTags.TWall))
        {
            _lastCollider = hit;
            HandleCollision(collisionPoint, hit, collisionNormal);
            GameEvents.OnWallBreakEffect?.Invoke(collisionPoint);
            if(SoundManager.Instance != null && !_spearController.isCloneSpear)
            {
                var audioSource = GetComponent<AudioSource>();
                if(audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
                audioSource.volume = 0.5f;
                SoundManager.Instance.PlaySoundOnShot(audioSource, SoundManager.Instance.soundSO.spearStuck);
            }
        }
        else if (hit.CompareTag(GameTags.TObstaclePin))
        {
            transform.SetParent(hit.transform);
            _lastCollider = hit;
            HandleCollision(collisionPoint, hit, collisionNormal);
            GameEvents.OnWallBreakEffect?.Invoke(collisionPoint);
            if(SoundManager.Instance != null && !_spearController.isCloneSpear)
            {
                var audioSource = GetComponent<AudioSource>();
                if(audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
                audioSource.volume = 1f;
                SoundManager.Instance.PlaySoundOnShot(audioSource, SoundManager.Instance.soundSO.spearStuck);
            }
        }
        
        else if (IsInMirrorLayer(hit) || IsInBlindMirrorLayer(hit))
        {
            _lastCollider = hit;
            HandleMirrorCollision(collisionPoint, hit, collisionNormal);
        }
    }

    private bool IsInMirrorLayer(Collider2D col)
    {
        return _mirrorLayer.value != 0 && (_mirrorLayer.value & (1 << col.gameObject.layer)) != 0;
    }

    private bool IsInBlindMirrorLayer(Collider2D col)
    {
        return _blindMirrorLayer.value != 0 && (_blindMirrorLayer.value & (1 << col.gameObject.layer)) != 0;
    }

    private void HandleMirrorCollision(Vector3 collisionPoint, Collider2D hitCollider, Vector2 hitNormal)
    {
        if (_spearController == null) return;

        _lastMirrorCollider = hitCollider;
        CheckColliderOverlap();
        _spearController.OnMirrorTipCollision(collisionPoint, hitCollider, hitNormal);
        ResetCollision();
        GameEvents.OnSpearStuck?.Invoke();
        // if (VibrationsManager.Instance != null) VibrationsManager.Instance.TriggerLightImpact();
        if (SoundManager.Instance != null) SoundManager.Instance.PlayReflectSound();
    }

    public void IgnoreColliderOnNextHit(Collider2D collider)
    {
        _lastCollider = collider;
    }

    public void IgnoreCollidersOnNextHit(IEnumerable<Collider2D> colliders)
    {
        ClearIgnoredCollidersInternal();
        foreach (var collider in colliders)
        {
            if (collider != null)
            {
                AddIgnoredCollider(collider);
            }
        }
    }

    public void ClearIgnoredColliders()
    {
        ClearIgnoredCollidersInternal();
    }

    private WallSide GetWallSide(Vector2 normal)
    {
        if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
        {
            if (normal.x > 0)
            {
                // Debug.Log("Wall Side: Left");
                return WallSide.Left;
            }   
            else
            {
                // Debug.Log("Wall Side: Right");
                return WallSide.Right;
            }
        }
        else
        {
            if (normal.y > 0)
            {
                // Debug.Log("Wall Side: Bottom");
                return WallSide.Bottom; 
            }
                
            else
            {
                // Debug.Log("Wall Side: Top");
                return WallSide.Top;
            }    
        }
    }
#endregion
#region Enemy Collision Handling
    private void ColliderHandler(CharacterBase character)
    {
        var spearType = _spearController.spearType;
        switch (spearType)
        {
            case SpearType.Basic:
                character.DeadAndPin(transform);
                BasicSpearHandler(character, _spearBodyCollider);
                break;
            case SpearType.Cut:
                CutSpearHandler(character);
                break;
            case SpearType.Thunder:
                 character.DeadByThunder();
                break;
            case SpearType.Fire:
                character.DeadByExplosion();
                break;
            case SpearType.ElementalAttack:
                ElementalAttackSpearHandler(character);
                break;
            case SpearType.TargetAllEnemy:
                TargetAllEnemyFirstHitHandler(character);
                break;
            default:
                break;
        }
    }
    private void BasicSpearHandler(CharacterBase character, Collider2D collision)
    {
        Vector2 pinPoint;
        if (_spearBodyCollider != null)
        {
            pinPoint = _spearBodyCollider.ClosestPoint(collision.bounds.center);
        }
        else
        {
            pinPoint = collision.ClosestPoint(transform.position);
        }

        character.PinToSpearAtPoint(transform, pinPoint);
        //GameEvents.OnDeadEffect?.Invoke(character.transform.position);
    }
    private void CutSpearHandler(CharacterBase character)
    {
        character.DeadAndSlice(transform.right);
    }
    private void ElementalAttackSpearHandler(CharacterBase character)
    {
        Vector3 hitPosition = character.transform.position;
        var elementType = _spearController.elementalAttackType;
        character.DeadByElementalAttack(elementType);

        if (_spearController.elementalAttackProjectilePrefab == null) return;

        CharacterBase[] allCharacters = FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);
        foreach (var target in allCharacters)
        {
            if (target == null || target == character) continue;
            if (!target.gameObject.activeInHierarchy) continue;

            GameObject projectile = Instantiate(
                _spearController.elementalAttackProjectilePrefab,
                hitPosition,
                Quaternion.identity
            );
            var elementalAttackProjectile = projectile.GetComponent<DeadByElementalAttack>();
            if (elementalAttackProjectile != null)
            {
                elementalAttackProjectile.Initialize(target, elementType);
            }
        }
    }
    private void TargetAllEnemyFirstHitHandler(CharacterBase character)
    {
        // Kill the first enemy via normal collision
        Vector2 spearDir = ((Vector2)_spearController.FreeTip.position - (Vector2)_spearController.RotatedTip.position).normalized;
        character.DeadAndSlice(spearDir);

        // Remove from queue + marker
        int idx = _spearController.targetEnemyQueue.IndexOf(character);
        if (idx >= 0)
        {
            _spearController.targetEnemyQueue.RemoveAt(idx);
            if (idx < _spearController.targetEnemyMarkers.Count)
            {
                var marker = _spearController.targetEnemyMarkers[idx];
                if (marker != null) Destroy(marker);
                _spearController.targetEnemyMarkers.RemoveAt(idx);
            }
        }

        // Start chain attack on remaining enemies
        _spearController.StartTargetingNearestEnemy();
    }
#endregion

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector2 center = transform.position;
        Vector2 size = _boxColliderSize;

        float angle = _spearController != null
            ? _spearController.transform.eulerAngles.z
            : transform.eulerAngles.z;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            center,
            Quaternion.Euler(0, 0, angle),
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, size);

        Gizmos.matrix = oldMatrix;
    }

    }

    public enum WallSide
    {
        Left, Right, Top, Bottom
    }
}
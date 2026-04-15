using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DongA.Powerups;
using GameBase;
using Spine.Unity;
using UnityEngine;

namespace DongA.Characters
{
    public class CharacterBase : MonoBehaviour
    {
        #region Fields & Properties
        [SerializeField] protected CharacterDataSO characterData;
        [SerializeField] protected CharacterType characterType;
        [SerializeField] protected MoveType moveType;
        [SerializeField] protected bool usePhysicsFall = false;
        [SerializeField] protected float moveDistance = 0f;
        [SerializeField] protected float moveDuration = 0f;
        [SerializeField] protected float waitTime = 0f;
        [SerializeField] protected Vector2 moveTargetPoint = Vector2.zero;
        protected CharacterState currentState;
        protected string currentSkin;
        //protected CharacterState _stateBeforeHit;
        protected string _animBeforeHit;
        protected bool _isHitByTrajectory;
        protected string _naturalAnim = "";
        protected bool _isFrozen = false;
        public bool IsFrozen => _isFrozen;
        protected bool _isSpawnInvincible;
        public bool IsSpawnInvincible => _isSpawnInvincible;
        public bool IsDead => currentState == CharacterState.Dead;
        private bool _isScaleReady;
        public bool IsScaleReady => _isScaleReady;
        public void MarkScaleReady() => _isScaleReady = true;
        protected SkeletonAnimation skeletonAnimation;
        protected CharacterColliderControllerPosition colliderController;
        protected Rigidbody2D rb;
        protected Collider2D boxCollider2D;
        private Vector2 _initialScale;
        bool isCanChangeState = true;
        private static int _skinSpawnCount = 0;
        private static bool _skinWearerAssigned = false;
        private bool _wearsSkin;


        // Velocity-based movement (dùng khi usePhysicsFall = true)
        private float _tweenVelocityX = 0f;
        private bool _isVelocityControlled = false;

        // Pinning
        private bool _isPinnedToSpear = false;
        private Transform _spearTransform;
        private Vector3 _pinnedLocalPosition;
        private Quaternion _pinnedLocalRotation;

        public bool IsCanChangeState { get => isCanChangeState; set => isCanChangeState = value; }
        public bool UsePhysicsFall { get => usePhysicsFall; set => usePhysicsFall = value; }
        public MoveType CharacterMoveType { get => moveType; set => moveType = value; }
        public float MoveDistance { get => moveDistance; set => moveDistance = value; }
        public float MoveDuration { get => moveDuration; set => moveDuration = value; }
        public float WaitTime { get => waitTime; set => waitTime = value; }
        public Vector2 MoveTargetPoint { get => moveTargetPoint; set => moveTargetPoint = value; }
        #endregion

        #region Unity Lifecycle
        protected virtual void Awake()
        {
            _skinSpawnCount++;
            _wearsSkin = Random.value < 0.7f;
            InitializeStates();
            rb = GetComponent<Rigidbody2D>();
            skeletonAnimation = GetComponent<SkeletonAnimation>();
            boxCollider2D = GetComponentInChildren<Collider2D>();
            colliderController = GetComponentInChildren<CharacterColliderControllerPosition>();
            ChangeState(CharacterState.Idle);
        }

        void OnEnable()
        {
            GameEvents.OnLevelLoadComplete += SetInitialScale;
            GameEvents.OnMultiSliceEffectStart += OnMultiSliceEffectStart;
            GameEvents.OnMultiSliceEffectCompleted += DeadByVerticalSlice;
            GameEvents.OnCharacterSkinChosen += ChangeSkin;
            GameEvents.OnLevelStartLoading += ResetSkinSpawnCount;
        }

        void OnDisable()
        {
            GameEvents.OnLevelLoadComplete -= SetInitialScale;
            GameEvents.OnMultiSliceEffectStart -= OnMultiSliceEffectStart;
            GameEvents.OnMultiSliceEffectCompleted -= DeadByVerticalSlice;
            GameEvents.OnCharacterSkinChosen -= ChangeSkin;
            GameEvents.OnLevelStartLoading -= ResetSkinSpawnCount;
        }

        public static void ResetSkinSpawnCount()
        {
            _skinSpawnCount = 0;
            _skinWearerAssigned = false;
        }

        // Gọi sau khi spawn nhóm enemy mới (vd: BigCharacter sinh con)
        // để đảm bảo nhóm mới luôn có ít nhất 1 con mặc skin
        public static void ResetSkinWearerForSpawn() => _skinWearerAssigned = false;

        protected virtual void Start()
        {
            _naturalAnim = skeletonAnimation != null ? (skeletonAnimation.AnimationName ?? AnimValue.idle) : AnimValue.idle;
            if(_naturalAnim == AnimValue.rideHorse)
            {
                isCanChangeState = false;
            }
            // Enemy sinh ra sau OnLevelLoadComplete (từ WashingMachine, MouseHole, BigCharacter)
            // chưa được gọi SetInitialScale → fallback ở đây
            if (_initialScale == Vector2.zero)
                _initialScale = transform.localScale;
            _isScaleReady = true;
            ChangeSkin();
            Move();
        }

        protected virtual void Update()
        {

        }

        protected virtual void FixedUpdate()
        {
            if (!_isVelocityControlled) return;
            rb.velocity = new Vector2(_tweenVelocityX, rb.velocity.y);
        }

        protected virtual void LateUpdate()
        {
            if (!_isPinnedToSpear || _spearTransform == null) return;
            transform.localPosition = _pinnedLocalPosition;
            transform.localRotation = _pinnedLocalRotation;
        }

        protected virtual void InitializeStates()
        {

        }

        protected void SetInitialScale()
        {
            _initialScale = transform.localScale;
        }

        public void SetSpawnInvincible(float duration = 0.3f)
        {
            _isSpawnInvincible = true;
            StartCoroutine(RemoveSpawnInvincible(duration));
        }

        private IEnumerator RemoveSpawnInvincible(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isSpawnInvincible = false;
        }
        #endregion

        #region State Management
        public virtual void ChangeState(CharacterState newState)
        {
            if (!IsCanChangeState) return;
            if (currentState == newState)
            {
                return;
            }
            ExitAnim();
            currentState = newState;
            EnterAnim();
        }

        public virtual void ExitAnim()
        {
            // if (skeletonAnimation == null) return;
            // skeletonAnimation.AnimationState.ClearTracks();
        }

        public virtual void EnterAnim()
        {
            if (skeletonAnimation == null) return;
            switch (currentState)
            {
                case CharacterState.Idle:
                    string idleAnim = string.IsNullOrEmpty(_naturalAnim) ? AnimValue.idle : _naturalAnim;
                    skeletonAnimation.AnimationState.SetAnimation(0, idleAnim, true);
                    break;
                case CharacterState.Hang_idle:
                    skeletonAnimation.AnimationState.SetAnimation(0, AnimValue.hangIdle, true);
                    break;
                case CharacterState.Scared:
                    skeletonAnimation.AnimationState.SetAnimation(0, AnimValue.scared, true);
                    break;
                case CharacterState.Dead:
                    //skeletonAnimation.AnimationState.SetAnimation(0, CharacterAnimValue.dead, true);
                    break;
                case CharacterState.Driving:
                    skeletonAnimation.AnimationState.SetAnimation(0, AnimValue.driving,true);
                    break;
                case CharacterState.Driving2:
                    skeletonAnimation.AnimationState.SetAnimation(0, AnimValue.driving2,true);
                    break;

                default:
                    break;
            }
            if (gameObject.activeInHierarchy)
                StartCoroutine(SnapColliderThenDisable());
        }

        private IEnumerator SnapColliderThenDisable()
        {
            colliderController.ActiveBoneFollower(true);
            yield return null;
            colliderController.ActiveBoneFollower(false);
        }

        public string GetRandomIdleAnim()
        {
            if (skeletonAnimation == null) return AnimValue.idle;
            var idleAnims = new[] { AnimValue.idle, AnimValue.idle1, AnimValue.idle2, AnimValue.idle3, AnimValue.idle4, AnimValue.idle5, AnimValue.idle6, AnimValue.idle7 };
            int randomIndex = Random.Range(0, idleAnims.Length);
            return idleAnims[randomIndex];
        }

        public void EnterAnimByName(string animName, bool loop = true)
        {
            if (skeletonAnimation == null) return;
            skeletonAnimation.AnimationState.SetAnimation(0, animName, loop);
        }

        public void ChangeToRandomIdleAnim()
        {
            string randomIdle = GetRandomIdleAnim();
            EnterAnimByName(randomIdle, true);
        }

        protected void OnMultiSliceEffectStart()
        {
           Freeze();
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = GameTags.SLTopEffect;
            }
        }
        #endregion
        

        #region Skin Handling
        protected virtual void ChangeSkin()
        {
            // Đảm bảo ít nhất 1 character mặc skin mỗi level
            if (!_wearsSkin && !_skinWearerAssigned)
                _wearsSkin = true;
            if (!_wearsSkin) return;
            string thisSkin = GetSkinName() + "/" + GamePrefs.CurrentCharacterSkinID;
            if (thisSkin == currentSkin) return;
            if (skeletonAnimation == null) return;
            if (skeletonAnimation.Skeleton.Data.FindSkin(thisSkin) == null) return;
            currentSkin = thisSkin;
            skeletonAnimation.Skeleton.SetSkin(thisSkin);
            skeletonAnimation.Skeleton.SetSlotsToSetupPose();
            _skinWearerAssigned = true;
        }
        protected string GetSkinName()
        {
            var currentSkin = " ";
            switch (characterType)
            {
                
                case CharacterType.Pickle:
                    currentSkin = "Pickle";
                    break;
                case CharacterType.Banana:
                    currentSkin = "Banana";
                    break;
                case CharacterType.WaterMelon:
                    currentSkin = "WaterMelon";
                    break;
                case CharacterType.Grape:
                    currentSkin = "Grape";
                    break;
                default:
                    currentSkin = "Pickle";
                    break;
            }
            return currentSkin;
        }


        #endregion

        #region Dead & Hit Handling
        private void PlayDeadSound(AudioClip clip)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayEnemyDeadSound(clip);
        }
        public virtual void DeadAndPin(Transform spearTransform = null)
        {
            StopMoving();
            boxCollider2D.isTrigger = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            boxCollider2D.enabled = false;

            if (spearTransform != null && !_isPinnedToSpear)
            {
                PinToSpear(spearTransform);
            }

            ChangeState(CharacterState.Dead);

            if (characterData != null)
            {
                GameEvents.OnDeadEffect?.Invoke(boxCollider2D.bounds.center, characterData.bloodEffectPrefab, characterData.bloodEffectColor, characterData.soulPrefab);
            }

            if (SoundManager.Instance != null) PlayDeadSound(SoundManager.Instance.soundSO.deadBySlash);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.DecreaseEnemyCount(boxCollider2D.bounds.center);
            }
        }

        public virtual void DeadAndSlice(Vector3 spearDirection)
        {
            if (characterData == null) return;

            // Determine cut direction based on spear direction
            bool isHorizontalCut = Mathf.Abs(spearDirection.x) > Mathf.Abs(spearDirection.y);

            // Get skin sprites if skeleton has a matching entry
            List<Sprite> skinSprites = null;
            if (characterData.sliceDeadCharacterPrefabs != null && characterData.sliceDeadCharacterPrefabs.Count > 0
                && skeletonAnimation != null && skeletonAnimation.Skeleton.Skin != null)
            {
                string fullSkinName = skeletonAnimation.Skeleton.Skin.Name; // e.g. "Banana/SkinID"
                int slashIdx = fullSkinName.IndexOf('/');
                string skinID = slashIdx >= 0 ? fullSkinName.Substring(slashIdx + 1) : fullSkinName;
                var skinEntry = characterData.sliceDeadCharacterPrefabs.Find(s => s.SkinID == skinID);
                if (skinEntry != null)
                    skinSprites = isHorizontalCut ? skinEntry.sliceHorizontalPrefabs : skinEntry.sliceVerticalPrefabs;
            }

            // Always spawn from default prefabs
            List<GameObject> prefabsToSpawn = isHorizontalCut ? characterData.sliceHorizontalPrefabs : characterData.sliceVerticalPrefabs;

            if (prefabsToSpawn != null && prefabsToSpawn.Count > 0)
            {
                var levelRoot = GameObject.Find(KeyPrefs.LevelRootName);
                Transform pieceParent = levelRoot != null ? levelRoot.transform : null;
                for (int i = 0; i < prefabsToSpawn.Count; i++)
                {
                    var prefab = prefabsToSpawn[i];
                    if (prefab == null) continue;
                    GameObject piece = Instantiate(prefab, boxCollider2D.bounds.center, transform.rotation);
                    piece.transform.localScale = characterData.bodyMultiplier * (Vector2)transform.lossyScale;
                    piece.transform.SetParent(pieceParent);

                    // Override sprite if skin sprites are available for this index
                    if (skinSprites != null && i < skinSprites.Count && skinSprites[i] != null)
                    {
                        SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();
                        if (sr != null) sr.sprite = skinSprites[i];
                    }

                    Rigidbody2D pieceRb = piece.GetComponent<Rigidbody2D>();
                    if (pieceRb != null)
                    {
                        Vector2 randomDir = Random.insideUnitCircle.normalized;
                        pieceRb.AddForce(randomDir * 5f, ForceMode2D.Impulse);
                    }
                }
            }

            if (characterData != null)
            {
                GameEvents.OnDeadEffect?.Invoke(boxCollider2D.bounds.center, characterData.bloodEffectPrefab, characterData.bloodEffectColor, characterData.soulPrefab);
            }
            if (SoundManager.Instance != null) PlayDeadSound(SoundManager.Instance.soundSO.deadBySlash);
            gameObject.SetActive(false); // Hide the original character immediately
            // Hide or destroy the original character
            if(GameManager.Instance == null) return;
            GameManager.Instance.DecreaseEnemyCount(boxCollider2D.bounds.center);
            // Destroy(this.gameObject);
        }

        /// <summary>
        /// Bổ dọc: spawn sliceVerticalPrefabs rồi animate 2 nửa bay trái/phải bằng DOTween.
        /// Prefab index 0 = nửa trái, index 1 = nửa phải.
        /// </summary>
        public virtual void DeadByVerticalSlice()
        {
           DeadAndSlice(Vector3.right);
        }

        public virtual void DeadByExplosion()
        {
            if(GameManager.Instance != null)
            {
                GameManager.Instance.DecreaseEnemyCount(transform.position);
            }
            if(characterData != null)
            {
                GameEvents.OnDeadEffect?.Invoke(boxCollider2D.bounds.center, characterData.burnEffectPrefab, Color.white, characterData.soulPrefab);
            }
            if (SoundManager.Instance != null) PlayDeadSound(SoundManager.Instance.soundSO.deadByBurn);
            Destroy(gameObject);
        }

        public virtual void DeadNormal()
        {
            if(GameManager.Instance != null)
            {
                GameManager.Instance.DecreaseEnemyCount(transform.position);
            }
            if(characterData != null)
            {
                GameEvents.OnDeadEffect?.Invoke(boxCollider2D.bounds.center, characterData.bloodEffectPrefab, characterData.bloodEffectColor, characterData.soulPrefab);
            }
            if (SoundManager.Instance != null) PlayDeadSound(SoundManager.Instance.soundSO.deadBySlash);
            Destroy(gameObject);
        }

        public virtual void DeadByElementalAttack(ElementalAttackType elementType)
        {
            switch (elementType)
            {
                case ElementalAttackType.Fire:
                    if (SoundManager.Instance != null) PlayDeadSound(SoundManager.Instance.soundSO.deadByBurn);
                    DeadByElemental(characterData != null ? characterData.burnEffectPrefab : null);
                    break;
                case ElementalAttackType.Thunder:
                    if (SoundManager.Instance != null) PlayDeadSound(SoundManager.Instance.soundSO.deadByElectric);
                    DeadByElemental(characterData != null ? characterData.thunderEffectPrefab : null);
                    break;
                default:
                    break;
            }
        }

        public virtual void DeadByElemental(GameObject deadEffectPrefab = null)
        {
            if(GameManager.Instance != null)
            {
                GameManager.Instance.DecreaseEnemyCount(transform.position);
            }
            if(characterData != null)
            {
                GameEvents.OnDeadEffect?.Invoke(boxCollider2D.bounds.center, deadEffectPrefab ?? characterData.burnEffectPrefab, Color.white, characterData.soulPrefab);
            }
            Destroy(gameObject);
        }

        public virtual void DeadByThunder()
        {
            if(GameManager.Instance != null)
            {
                GameManager.Instance.DecreaseEnemyCount(transform.position);
            }
            if(characterData != null)
            {
                GameEvents.OnDeadEffect?.Invoke(boxCollider2D.bounds.center, characterData.thunderEffectPrefab, Color.white, characterData.soulPrefab);
            }
            if (SoundManager.Instance != null) PlayDeadSound(SoundManager.Instance.soundSO.deadByElectric);
            Destroy(gameObject);
        }

        public virtual void Freeze()
        {
            _isFrozen = true;
            skeletonAnimation.timeScale = 0f;
            IsCanChangeState = false;
            StopMoving();
            rb.bodyType = RigidbodyType2D.Static;
        }

        public virtual void ShowHitEffect(Vector3 hitPoint, Vector3 trajectoryDirection = default)
        {
            if (!_isHitByTrajectory)
            {
                _animBeforeHit = skeletonAnimation.AnimationName;
                _isHitByTrajectory = true;

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayEnemyScaredSound();
                }
            }
            // Debug.Log("Show Hit Effect");
            ChangeState(CharacterState.Scared);
        }

        /// <summary>
        /// Gọi khi tia trajectory không còn chiếu vào enemy nữa → về lại anim trước đó
        /// </summary>
        public virtual void HideHitEffect()
        {
            if (!_isHitByTrajectory) return;
            _isHitByTrajectory = false;
            // Debug.Log("Hide Hit Effect - Restore previous state");
            if (!IsCanChangeState) return;
            currentState = CharacterState.Idle;
            EnterAnimByName(_animBeforeHit);
        }
        #endregion

        #region Spear Pinning
        public virtual void PinToSpearAtPoint(Transform spear, Vector2 worldPinPoint)
        {
            PinToSpearInternal(spear, worldPinPoint);
        }

        public void PinToSpear(Transform spear)
        {
            PinToSpearInternal(spear, null);
        }

        private void PinToSpearInternal(Transform spear, Vector2? worldPinPoint)
        {
            if (spear == null) return;

            _spearTransform = spear;
            _isPinnedToSpear = true;

            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = false;

            transform.SetParent(spear, true);

            if (worldPinPoint.HasValue)
            {
                transform.position = worldPinPoint.Value;
            }

            _pinnedLocalPosition = transform.localPosition;
            _pinnedLocalRotation = transform.localRotation;
        }

        public void UnpinFromSpear()
        {
            if (!_isPinnedToSpear) return;

            _isPinnedToSpear = false;
            _spearTransform = null;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.simulated = true;
            transform.SetParent(null, true);
        }
        #endregion

        #region Movement
        public virtual void Move()
        {
            if (usePhysicsFall)
                MoveWithPhysics();
            else
                MoveWithoutPhysics();
        }

        private void MoveWithoutPhysics()
        {
            var sequence = DOTween.Sequence().SetTarget(transform);
            switch (moveType)
            {
                case MoveType.Vertical:
                    rb.bodyType = RigidbodyType2D.Static;
                    sequence.Append(transform.DOMoveY(transform.position.y + MoveDistance, MoveDuration).SetEase(Ease.Linear));
                    sequence.SetLoops(-1, LoopType.Yoyo).SetDelay(WaitTime);
                    break;
                case MoveType.Horizontal:
                    rb.bodyType = RigidbodyType2D.Static;
                    sequence.Append(transform.DOMoveX(transform.position.x + MoveDistance, MoveDuration).SetEase(Ease.Linear));
                    sequence.SetLoops(-1, LoopType.Yoyo).SetDelay(WaitTime);
                    break;
                case MoveType.MoveToPoint:
                {
                    rb.bodyType = RigidbodyType2D.Static;
                    Vector3 startPos = transform.position;
                    Vector3 targetPos = new Vector3(moveTargetPoint.x, moveTargetPoint.y, 0f);
                    var seq = DOTween.Sequence().SetTarget(transform);
                    seq.AppendInterval(WaitTime);
                    seq.Append(transform.DOMove(targetPos, MoveDuration).SetEase(Ease.Linear));
                    seq.AppendInterval(WaitTime);
                    seq.Append(transform.DOMove(startPos, MoveDuration).SetEase(Ease.Linear));
                    seq.SetLoops(-1, LoopType.Restart);
                    break;
                }
                case MoveType.SideToSide:
                {
                    rb.bodyType = RigidbodyType2D.Static;
                    float centerX = transform.position.x;
                    Vector3 initScale = transform.localScale;
                    float absScaleX = Mathf.Abs(initScale.x);
                    float leftX = centerX - MoveDistance;
                    float rightX = centerX + MoveDistance;

                    var initSeq = DOTween.Sequence().SetTarget(transform);
                    initSeq.Append(transform.DOMoveX(leftX, MoveDuration).SetEase(Ease.Linear));
                    initSeq.OnComplete(() =>
                    {
                        var loopSeq = DOTween.Sequence().SetTarget(transform);
                        loopSeq.AppendInterval(WaitTime);
                        loopSeq.AppendCallback(() =>
                            transform.localScale = new Vector3(-absScaleX, initScale.y, initScale.z));
                        loopSeq.Append(transform.DOMoveX(rightX, MoveDuration).SetEase(Ease.Linear));
                        loopSeq.AppendInterval(WaitTime);
                        loopSeq.AppendCallback(() =>
                            transform.localScale = new Vector3(absScaleX, initScale.y, initScale.z));
                        loopSeq.Append(transform.DOMoveX(leftX, MoveDuration).SetEase(Ease.Linear));
                        loopSeq.SetLoops(-1, LoopType.Restart);
                    });
                    break;
                }
                case MoveType.None:
                    break;
                default:
                    break;
            }
        }

        private void MoveWithPhysics()
        {
            switch (moveType)
            {
                case MoveType.None:
                    // Rơi tự do ngay lập tức
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    break;

                case MoveType.Vertical:
                    // Kinematic để DOTween control Y không bị gravity cản
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    var vSeq = DOTween.Sequence().SetTarget(transform);
                    vSeq.Append(rb.DOMoveY(rb.position.y + MoveDistance, MoveDuration).SetEase(Ease.Linear));
                    vSeq.SetLoops(-1, LoopType.Yoyo).SetDelay(WaitTime);
                    break;

                case MoveType.Horizontal:
                {
                    // Dynamic: DOTween control velocity X, physics xử lý Y (gravity + collision)
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    _isVelocityControlled = true;
                    float speed = MoveDistance / MoveDuration;

                    var initSeq = DOTween.Sequence().SetTarget(transform);
                    initSeq.AppendInterval(WaitTime);
                    initSeq.OnComplete(() =>
                    {
                        _tweenVelocityX = speed;
                        var loopSeq = DOTween.Sequence().SetTarget(transform);
                        loopSeq.AppendInterval(MoveDuration);
                        loopSeq.AppendCallback(() => _tweenVelocityX = -speed);
                        loopSeq.AppendInterval(MoveDuration);
                        loopSeq.AppendCallback(() => _tweenVelocityX = speed);
                        loopSeq.SetLoops(-1, LoopType.Restart);
                    });
                    break;
                }

                case MoveType.MoveToPoint:
                    // Kinematic để theo đúng path, có thể trigger collision
                    rb.bodyType = RigidbodyType2D.Kinematic;
                    rb.DOMove(new Vector2(moveTargetPoint.x, moveTargetPoint.y), MoveDuration)
                        .SetEase(Ease.Linear)
                        .SetDelay(WaitTime);
                    break;

                case MoveType.SideToSide:
                {
                    // Dynamic: velocity X đảo chiều theo thời gian, physics xử lý Y
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    _isVelocityControlled = true;
                    float speed = MoveDistance / MoveDuration;
                    Vector3 initScale = transform.localScale;
                    float absScaleX = Mathf.Abs(initScale.x);

                    // Bắt đầu đi trái
                    _tweenVelocityX = -speed;
                    transform.localScale = new Vector3(-absScaleX, initScale.y, initScale.z);

                    var initSeq = DOTween.Sequence().SetTarget(transform);
                    initSeq.AppendInterval(MoveDuration);
                    initSeq.OnComplete(() =>
                    {
                        _tweenVelocityX = 0f;
                        var loopSeq = DOTween.Sequence().SetTarget(transform);
                        loopSeq.AppendInterval(WaitTime);
                        loopSeq.AppendCallback(() =>
                        {
                            _tweenVelocityX = speed;
                            transform.localScale = new Vector3(absScaleX, initScale.y, initScale.z);
                        });
                        loopSeq.AppendInterval(MoveDuration);
                        loopSeq.AppendCallback(() => _tweenVelocityX = 0f);
                        loopSeq.AppendInterval(WaitTime);
                        loopSeq.AppendCallback(() =>
                        {
                            _tweenVelocityX = -speed;
                            transform.localScale = new Vector3(-absScaleX, initScale.y, initScale.z);
                        });
                        loopSeq.AppendInterval(MoveDuration);
                        loopSeq.AppendCallback(() => _tweenVelocityX = 0f);
                        loopSeq.SetLoops(-1, LoopType.Restart);
                    });
                    break;
                }

                default:
                    break;
            }
        }

        /// <summary>
        /// Dừng di chuyển: kill tất cả DOTween và reset velocity control.
        /// </summary>
        public void StopMoving()
        {
            transform.DOKill();
            rb.DOKill();
            _isVelocityControlled = false;
            _tweenVelocityX = 0f;
        }
        #endregion

        #region Gizmos
        protected void OnDrawGizmos()
        {
            if (moveType == MoveType.None || moveDistance == 0f)
                return;

            Gizmos.color = Color.red;
            Vector3 start = transform.position;
            Vector3 end = start;
            switch (moveType)
            {
                case MoveType.Vertical:
                    end += Vector3.up * moveDistance;
                    break;
                case MoveType.Horizontal:
                    end += Vector3.right * moveDistance;
                    break;
                case MoveType.SideToSide:
                    end += Vector3.left * moveDistance;
                    break;
            }
            Gizmos.DrawLine(start, end);
            Gizmos.DrawSphere(start, 0.1f);
            Gizmos.DrawSphere(end, 0.1f);
        }
        #endregion
    }

    public enum CharacterState
    {
        Idle,
        Hang_idle,
        Driving,
        Driving2,
        Jumping,
        Scared,
        Dead
    }
    public enum CharacterType
    {
        Pickle,
        Banana,
        WaterMelon,
        Grape,
        // Thêm các loại nhân vật khác ở đây
    }
    public enum MoveType
    {
        None,
        Vertical,
        Horizontal,
        MoveToPoint,
        SideToSide
    }
}

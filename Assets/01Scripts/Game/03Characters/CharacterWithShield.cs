using DG.Tweening;
using UnityEngine;

namespace DongA.Characters
{
    public class CharacterWithShield : CharacterBase
    {
        [SerializeField] private BoxCollider2D shieldCollider;
        public override void EnterAnim()
        {
            if (skeletonAnimation == null) return;
            if (currentState == CharacterState.Idle)
                EnterAnimByName(AnimValue.shield, true);
            else
                base.EnterAnim();
        }

        public override void ShowHitEffect(Vector3 hitPoint, Vector3 freeTipPosition = default)
        {
            if (_isFrozen) return;
            _isHitByTrajectory = true;
            transform.DOPause();

            // So sánh vị trí freeTip với điểm cao nhất của collider enemy
            bool isFreeTipAboveCollider = freeTipPosition != default
                ? freeTipPosition.y > boxCollider2D.bounds.max.y
                : hitPoint.y > boxCollider2D.bounds.center.y;

            if (isFreeTipAboveCollider)
                EnterAnimByName(AnimValue.shieldGuard, true);
            else
                EnterAnimByName(AnimValue.scared, true);
        }

        public override void HideHitEffect()
        {
            if (!_isHitByTrajectory) return;
            _isHitByTrajectory = false;
            if (!IsCanChangeState) return;
            transform.DOPlay();
            EnterAnimByName(AnimValue.shield, true);
        }
    }
}

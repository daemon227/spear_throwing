using UnityEngine;

namespace DongA.SpearLogic
{
    public partial class SpearController
    {
        private sealed class SpearRotationSolver
        {
            private readonly SpearController _owner;

            public SpearRotationSolver(SpearController owner)
            {
                _owner = owner;
            }

            public void ApplyTargetRotationAroundPivot(float targetAngle)
            {
                float currentAngle = _owner.transform.eulerAngles.z;
                float deltaAngle = Mathf.DeltaAngle(currentAngle, targetAngle);
                deltaAngle = ClampRotationDelta(deltaAngle);

                if (Mathf.Abs(deltaAngle) < 0.0001f)
                    return;

                if (!_owner._isStuck || _owner._isLaunched)
                {
                    RotateAroundPivot(deltaAngle);
                    return;
                }

                ApplyRotationWithConstraints(deltaAngle);
            }

            public float CheckWallCollisionAndGetBlockDirection()
            {
                if (_owner._freeTip == null || _owner._rotatedTip == null)
                    return 0f;

                Vector2 freeEndPosition = _owner._freeTip.position;
                Vector2 stuckEndPosition = _owner._rotatedTip.position;

                Vector2 pivotToFree = (freeEndPosition - (Vector2)_owner._pivotPoint).normalized;
                if (pivotToFree.sqrMagnitude < 0.0001f)
                {
                    return 0f;
                }

                Collider2D hitFree = Physics2D.OverlapCircle(freeEndPosition, _owner._wallCheckFreeTipRadius, _owner._wallLayer);
                if (hitFree == null)
                {
                    return 0f;
                }

                Vector2 closestPointOnWall = hitFree.ClosestPoint(freeEndPosition);
                Vector2 freeToWall = (closestPointOnWall - freeEndPosition).normalized;
                float cross = pivotToFree.x * freeToWall.y - pivotToFree.y * freeToWall.x;

                if (Mathf.Abs(cross) < 0.001f)
                {
                    return 1f;
                }

                return cross > 0 ? 1f : -1f;
            }

            private float ClampRotationDelta(float deltaAngle)
            {
                if (_owner._snapToTargetWhileDragging)
                    return deltaAngle;

                float maxDelta = _owner._maxRotationSpeed > 0f ? _owner._maxRotationSpeed * Time.deltaTime : _owner._maxRotationPerFrame;

                if (_owner._maxRotationSpeed > 0f && _owner._maxRotationPerFrame > 0f)
                {
                    maxDelta = Mathf.Min(maxDelta, _owner._maxRotationPerFrame);
                }

                if (maxDelta <= 0f)
                    return deltaAngle;

                return Mathf.Clamp(deltaAngle, -maxDelta, maxDelta);
            }

            private void ApplyRotationWithConstraints(float desiredDeltaAngle)
            {
                bool wasColliding = IsFreeTipColliding();
                float penetrationBefore = wasColliding ? GetFreeTipPenetrationDepthOrZero() : 0f;

                bool hadSurfaceAngle = TryGetAngleToStuckSurfacePlane(out float angleToPlaneBefore);
                float rotationDirection = Mathf.Sign(desiredDeltaAngle);

                if (TryApplyBestRotation(desiredDeltaAngle, wasColliding, penetrationBefore, hadSurfaceAngle, angleToPlaneBefore, out float appliedAbs))
                {
                    // if (appliedAbs < Mathf.Abs(desiredDeltaAngle) - 0.0001f)
                    // {
                    //     _owner._blockedDirection = rotationDirection;
                    // }
                    return;
                }

                
            }

            private bool TryApplyBestRotation(float desiredDeltaAngle, bool wasColliding, float penetrationBefore,
                bool hadSurfaceAngle, float angleToPlaneBefore, out float appliedAbs)
            {
                appliedAbs = 0f;

                PoseSnapshot original = PoseSnapshot.Capture(_owner.transform);

                float desiredAbs = Mathf.Abs(desiredDeltaAngle);
                float sign = Mathf.Sign(desiredDeltaAngle);

                if (TryApplyRotationCandidate(original, sign, desiredAbs, wasColliding, penetrationBefore, hadSurfaceAngle, angleToPlaneBefore))
                {
                    appliedAbs = desiredAbs;
                    return true;
                }

                float bestAbs = FindBestAbsByBinarySearch(original, sign, desiredAbs, wasColliding, penetrationBefore, hadSurfaceAngle, angleToPlaneBefore);
                if (bestAbs <= 0.0001f)
                {
                    original.Restore(_owner.transform);
                    return false;
                }

                original.Restore(_owner.transform);
                RotateAroundPivot(sign * bestAbs);
                appliedAbs = bestAbs;
                return true;
            }

            private float FindBestAbsByBinarySearch(PoseSnapshot original, float sign, float desiredAbs,
                bool wasColliding, float penetrationBefore, bool hadSurfaceAngle, float angleToPlaneBefore)
            {
                float bestAbs = 0f;
                float lo = 0f;
                float hi = desiredAbs;

                for (int i = 0; i < _owner._constraintBinarySearchIterations; i++)
                {
                    float mid = (lo + hi) * 0.5f;
                    if (mid < 0.0001f)
                        break;

                    if (TryApplyRotationCandidate(original, sign, mid, wasColliding, penetrationBefore, hadSurfaceAngle, angleToPlaneBefore))
                    {
                        bestAbs = mid;
                        lo = mid;
                    }
                    else
                    {
                        hi = mid;
                    }
                }

                return bestAbs;
            }

            private bool TryApplyRotationCandidate(PoseSnapshot original, float sign, float absDelta, bool wasColliding,
                float penetrationBefore, bool hadSurfaceAngle, float angleToPlaneBefore)
            {
                if (!IsSweepClear(original, sign, absDelta, wasColliding, penetrationBefore))
                    return false;

                original.Restore(_owner.transform);
                RotateAroundPivot(sign * absDelta);

                return IsPoseValid(wasColliding, penetrationBefore, hadSurfaceAngle, angleToPlaneBefore);
            }

            private bool IsPoseValid(bool wasColliding, float penetrationBefore, bool hadSurfaceAngle, float angleToPlaneBefore)
            {
                if (hadSurfaceAngle && TryGetAngleToStuckSurfacePlane(out float angleAfter))
                {
                    if (angleToPlaneBefore < _owner._minAngleToStuckSurfacePlane)
                    {
                        if (angleAfter < angleToPlaneBefore - 0.0001f)
                            return false;
                    }
                    else
                    {
                        if (angleAfter < _owner._minAngleToStuckSurfacePlane)
                            return false;
                    }
                }

                float newBlockDir = CheckWallCollisionAndGetBlockDirection();
                if (newBlockDir != 0f)
                {
                    if (!wasColliding)
                        return false;

                }
                else
                {
                }

                if (wasColliding)
                {
                    float penetrationAfter = GetFreeTipPenetrationDepthOrZero();
                    if (penetrationAfter > penetrationBefore + 0.0001f)
                        return false;
                }

                return true;
            }

            private bool IsSweepClear(PoseSnapshot original, float sign, float absDelta, bool wasColliding, float penetrationBefore)
            {
                if (!_owner._enableRotationSweepCheck || absDelta <= _owner._rotationSweepStepAngle)
                    return true;

                float step = Mathf.Max(_owner._rotationSweepStepAngle, 0.0001f);
                int steps = Mathf.Max(1, Mathf.CeilToInt(absDelta / step));
                float stepAngle = absDelta / steps;

                original.Restore(_owner.transform);

                for (int i = 0; i < steps; i++)
                {
                    RotateAroundPivot(sign * stepAngle);

                    if (!wasColliding && IsFreeTipColliding())
                    {
                        original.Restore(_owner.transform);
                        return false;
                    }

                    if (wasColliding)
                    {
                        float penetrationAfter = GetFreeTipPenetrationDepthOrZero();
                        if (penetrationAfter > penetrationBefore + 0.0001f)
                        {
                            original.Restore(_owner.transform);
                            return false;
                        }
                    }
                }

                original.Restore(_owner.transform);
                return true;
            }

            private void RotateAroundPivot(float deltaAngle)
            {
                _owner.transform.RotateAround(
                    _owner._pivotPoint,
                    Vector3.forward,
                    deltaAngle
                );
            }

            private bool IsFreeTipColliding()
            {
                if (_owner._freeTip == null)
                    return false;

                Collider2D hitFree = Physics2D.OverlapCircle(_owner._freeTip.position, _owner._wallCheckFreeTipRadius, _owner._wallLayer);
                return hitFree != null;
            }

            private float GetFreeTipPenetrationDepthOrZero()
            {
                return TryGetFreeTipPenetrationDepth(out float depth) ? depth : 0f;
            }

            private bool TryGetFreeTipPenetrationDepth(out float penetrationDepth)
            {
                penetrationDepth = 0f;

                if (_owner._freeTip == null)
                    return false;

                Vector2 freeEndPosition = _owner._freeTip.position;
                Collider2D[] hits = Physics2D.OverlapCircleAll(freeEndPosition, _owner._wallCheckFreeTipRadius, _owner._wallLayer);
                if (hits == null || hits.Length == 0)
                {
                    return false;
                }

                float minDistance = float.MaxValue;
                for (int i = 0; i < hits.Length; i++)
                {
                    Vector2 closest = hits[i].ClosestPoint(freeEndPosition);
                    float dist = Vector2.Distance(freeEndPosition, closest);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                    }
                }

                if (minDistance == float.MaxValue)
                {
                    return false;
                }

                penetrationDepth = Mathf.Max(0f, _owner._wallCheckFreeTipRadius - minDistance);
                return true;
            }

            private bool TryGetAngleToStuckSurfacePlane(out float angleToPlaneDeg)
            {
                angleToPlaneDeg = 0f;

                if (!_owner._isStuck || _owner._isLaunched) return false;
                if (_owner._stuckSurfaceCollider == null) return false;
                if (_owner._freeTip == null || _owner._rotatedTip == null) return false;

                Vector2 spearDir = ((Vector2)_owner._freeTip.position - (Vector2)_owner._rotatedTip.position);
                if (spearDir.sqrMagnitude < 0.0001f) return false;
                spearDir.Normalize();

                if (!TryGetStuckSurfaceNormal(spearDir, out Vector2 surfaceNormal)) return false;

                float angleToNormal = Vector2.Angle(spearDir, surfaceNormal);
                angleToPlaneDeg = 90f - angleToNormal;
                return true;
            }

            private bool TryGetStuckSurfaceNormal(Vector2 spearDirFromPivotToFree, out Vector2 surfaceNormal)
            {
                surfaceNormal = default;

                if (_owner._hasStuckSurfaceNormal)
                {
                    surfaceNormal = _owner._stuckSurfaceNormal;
                    return surfaceNormal.sqrMagnitude > 0.0001f;
                }

                if (_owner._stuckSurfaceCollider == null) return false;

                Vector2 origin = (Vector2)_owner._pivotPoint + spearDirFromPivotToFree * _owner._surfaceNormalProbeOutset;
                Vector2 castDir = -spearDirFromPivotToFree;

                RaycastHit2D[] hits = Physics2D.RaycastAll(origin, castDir, _owner._surfaceNormalProbeDistance, _owner._wallLayer);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].collider == _owner._stuckSurfaceCollider)
                    {
                        surfaceNormal = hits[i].normal;
                        if (surfaceNormal.sqrMagnitude > 0.0001f)
                        {
                            surfaceNormal.Normalize();
                            return true;
                        }
                    }
                }

                return false;
            }

            private readonly struct PoseSnapshot
            {
                private readonly Vector3 _position;
                private readonly Quaternion _rotation;

                private PoseSnapshot(Vector3 position, Quaternion rotation)
                {
                    _position = position;
                    _rotation = rotation;
                }

                public static PoseSnapshot Capture(Transform transform)
                {
                    return new PoseSnapshot(transform.position, transform.rotation);
                }

                public void Restore(Transform transform)
                {
                    transform.SetPositionAndRotation(_position, _rotation);
                }
            }
        }
    }
}

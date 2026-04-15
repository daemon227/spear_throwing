using DongA.Events;
using DongA.SpearManager;
using GameBase;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace DongA.SpearLogic
{
    public partial class SpearController
    {
        private sealed class SpearRotationInteractor
        {
            private readonly SpearController _owner;
            private readonly SpearRotationSolver _rotationSolver;
            bool _isDragging = false;
            private bool _dragAnimStarted = false;
            private const float DragStartThreshold = 0.02f; // world units
            public SpearRotationInteractor(SpearController owner, SpearRotationSolver rotationSolver)
            {
                _owner = owner;
                _rotationSolver = rotationSolver;
            }

            public void Tick()
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (IsPointerOverBlockingUI())
                        return;

                    BeginDrag();
                    if (!_isDragging)
                    {
                        _isDragging = true;
                        GameEvents.OnSpearDragging?.Invoke();
                        if(SoundManager.Instance != null && !_owner.isCloneSpear)
                        {
                            var audioSource = _owner.GetComponent<AudioSource>();
                            if(audioSource == null)
                            {
                                audioSource = _owner.gameObject.AddComponent<AudioSource>();
                            }
                            SoundManager.Instance.PlaySoundOnShot(audioSource, SoundManager.Instance.soundSO.spearActive);
                        }
                    }
                    return;
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (_isDragging)
                    {
                        _isDragging = false;
                        _dragAnimStarted = false;
                        GameEvents.OnSpearLaunch?.Invoke();
                        if(!_owner.isCloneSpear) GlobalValues.Total_Shot += 1;
                        EndDragAndLaunch();
                        if(SoundManager.Instance != null && !_owner.isCloneSpear)
                        {
                            var audioSource = _owner.GetComponent<AudioSource>();
                                if(audioSource == null)
                                {
                                    audioSource = _owner.gameObject.AddComponent<AudioSource>();
                                }
                                SoundManager.Instance.PlaySoundOnShot(audioSource, SoundManager.Instance.soundSO.spearLaunch);
                        }
                    }
                    return;
                }

                if (_owner._isDragging && Input.GetMouseButton(0))
                {
                    UpdateDrag();
                }
            }

            private void BeginDrag()
            {
                if (_owner._freeTip == null || _owner._rotatedTip == null)
                    return;

                _owner._isDragging = true;
                _owner._pivotPoint = _owner._rotatedTip.position;

                _owner._dragMouseDownWorld = GetInputWorldPosition();
                _owner._dragFreeTipDownWorld = _owner._freeTip.position;

                CacheDragAngleOffset(_owner._freeTip.position, _owner._rotatedTip.position);
                ResetDragFlipState();

                _owner._trajectoryLine?.Show(true);
            }

            private void CacheDragAngleOffset(Vector2 freeTip, Vector2 stuckTip)
            {
                Vector2 spearDir = freeTip - stuckTip;
                if (spearDir.sqrMagnitude < 0.0001f)
                    return;

                float spearDirAngle = Mathf.Atan2(spearDir.y, spearDir.x) * Mathf.Rad2Deg;
                float transformAngle = _owner.transform.eulerAngles.z;
                _owner._dragTransformToSpearDirAngleOffset = Mathf.DeltaAngle(transformAngle, spearDirAngle);
            }

            private void ResetDragFlipState()
            {
                _owner._lastDragRotationDirection = 0f;
                _owner._lastDragMouseDelta = Vector2.zero;
            }

            private void EndDragAndLaunch()
            {
                if (!_owner._isDragging)
                    return;

                _owner._isDragging = false;
                _owner._trajectoryLine?.Show(false);
                _owner.LaunchFromCurrentPose();
            }

            private void UpdateDrag()
            {
                if (_owner._freeTip == null || _owner._rotatedTip == null)
                    return;

                _owner._pivotPoint = _owner._rotatedTip.position;

                Vector2 mouseWorld = GetInputWorldPosition();
                Vector2 delta = mouseWorld - _owner._dragMouseDownWorld;

                if (!_dragAnimStarted && delta.sqrMagnitude >= DragStartThreshold * DragStartThreshold)
                {
                    _dragAnimStarted = true;
                }

                if (ShouldRebaseDrag(delta))
                {
                    RebaseDrag(mouseWorld);
                    return;
                }

                float targetAngle = ComputeTargetTransformAngle(delta, _owner._pivotPoint);
                _rotationSolver.ApplyTargetRotationAroundPivot(targetAngle);

                UpdateTrajectoryLine();
                _owner._lastDragMouseDelta = delta;
            }

            private bool ShouldRebaseDrag(Vector2 delta)
            {
                const float rebaseEpsilon = 0.0005f;

                bool xFlip = Mathf.Abs(delta.x) > rebaseEpsilon && Mathf.Abs(_owner._lastDragMouseDelta.x) > rebaseEpsilon && Mathf.Sign(delta.x) != Mathf.Sign(_owner._lastDragMouseDelta.x);
                bool yFlip = Mathf.Abs(delta.y) > rebaseEpsilon && Mathf.Abs(_owner._lastDragMouseDelta.y) > rebaseEpsilon && Mathf.Sign(delta.y) != Mathf.Sign(_owner._lastDragMouseDelta.y);
                if (xFlip || yFlip)
                    return true;

                float currentAngle = _owner.transform.eulerAngles.z;
                float targetAngle = ComputeTargetTransformAngle(delta, _owner._pivotPoint);
                float desiredDelta = Mathf.DeltaAngle(currentAngle, targetAngle);
                if (Mathf.Abs(desiredDelta) <= 0.0001f)
                    return false;

                float desiredDir = Mathf.Sign(desiredDelta);
                bool rotationDirFlip = _owner._lastDragRotationDirection != 0f && desiredDir != _owner._lastDragRotationDirection;
                if (rotationDirFlip)
                    return true;

                _owner._lastDragRotationDirection = desiredDir;
                return false;
            }

            private void RebaseDrag(Vector2 mouseWorld)
            {
                _owner._dragMouseDownWorld = mouseWorld;
                _owner._dragFreeTipDownWorld = _owner._freeTip.position;
                CacheDragAngleOffset(_owner._freeTip.position, _owner._rotatedTip.position);
                ResetDragFlipState();
            }

            private float ComputeTargetTransformAngle(Vector2 mouseDelta, Vector2 stuckTip)
            {
                Vector2 targetFreeTipWorld = _owner._dragFreeTipDownWorld + mouseDelta;
                Vector2 targetDir = targetFreeTipWorld - stuckTip;
                if (targetDir.sqrMagnitude < 0.0001f)
                    return _owner.transform.eulerAngles.z;

                float targetSpearDirAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;
                return targetSpearDirAngle - _owner._dragTransformToSpearDirAngleOffset + _owner._rotationOffset;
            }

            private void UpdateTrajectoryLine()
            {
                if (_owner._trajectoryLine == null || _owner._freeTip == null || _owner._rotatedTip == null)
                    return;

                _owner._trajectoryLine.UpdateLine(_owner._rotatedTip.position, _owner._freeTip.position, _owner._wallLayer);
            }

            private static readonly List<RaycastResult> _uiRaycastResults = new List<RaycastResult>();
  

            private bool IsPointerOverBlockingUI()
            {
                if (EventSystem.current == null) return false;

                var pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
                _uiRaycastResults.Clear();
                EventSystem.current.RaycastAll(pointerData, _uiRaycastResults);

                foreach (var result in _uiRaycastResults)
                {
                    if (result.gameObject.CompareTag(GameTags.TBlockSpearInput))
                        return true;
                }
                return false;
            }

            private Vector2 GetInputWorldPosition()
            {
                if (_owner._mainCamera == null)
                    return default;

                Vector3 mousePos = Input.mousePosition;
                mousePos.z = Mathf.Abs(_owner._mainCamera.transform.position.z - _owner._pivotPoint.z);
                Vector3 worldPos = _owner._mainCamera.ScreenToWorldPoint(mousePos);
                worldPos.z = 0f;
                return worldPos;
            }
        }
    }
}

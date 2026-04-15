using DongA.EnemySpear;
using DongA.Events;
using DongA.SpearLogic;
using Spine.Unity;
using UnityEngine;

namespace DongA.SpearManager
{
	public class SpearAnimManager : MonoBehaviour
	{
		[SerializeField] private SkeletonAnimation _skeletonAnimation;
		private SpearAnimStateId currentStateId = SpearAnimStateId.Idle;
		private string currentSkin = "";
		private bool _isEnemySpear;
		private SpearController _spearController;

		protected virtual void Awake()
		{
			_isEnemySpear = GetComponentInParent<SpearEnemyAI>() != null;
			_spearController = GetComponentInParent<SpearController>();
			InitializeStates();
			OnSpearSkinChosen();
		}
		private void OnEnable()
		{
			GameEvents.OnSpearDragging += OnSpearDragging;
			GameEvents.OnSpearLaunch += OnSpearLaunch;
			GameEvents.OnSpearSkinChosen += OnSpearSkinChosen;
		}
		private void OnDisable()
		{
			GameEvents.OnSpearDragging -= OnSpearDragging;
			GameEvents.OnSpearLaunch -= OnSpearLaunch;
			GameEvents.OnSpearSkinChosen -= OnSpearSkinChosen;
		}
		void Start()
		{
			ChangeState(SpearAnimStateId.Idle);
		}

		private void InitializeStates()
		{
			if (_skeletonAnimation == null)
				_skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
		}

		protected void ChangeState(SpearAnimStateId newState)
		{
			if (currentStateId == newState) return;
			currentStateId = newState;
			EnterAnimState();
		}

		private void EnterAnimState()
		{
			_skeletonAnimation.AnimationState.SetAnimation(0, GetAnimClipName(currentStateId), true);
		}

		private string GetAnimClipName(SpearAnimStateId stateId)
		{
			switch (stateId)
			{
				case SpearAnimStateId.Idle:     return SpearAnimValue.idle;
				case SpearAnimStateId.Dragging: return SpearAnimValue.dragging;
				default:                        return SpearAnimValue.idle;
			}
		}

		protected void OnSpearDragging()
		{
			ChangeState(SpearAnimStateId.Dragging);
		}
		protected void OnSpearLaunch()
		{
			ChangeState(SpearAnimStateId.Idle);
		}
		private void OnSpearSkinChosen()
		{
			if (_isEnemySpear) return;
			if (_spearController != null)
				_spearController.spearType = (SpearType)GamePrefs.CurrentSpearType;
			ChangeSkin();
		}
		protected void ChangeSkin()
		{
			if (_isEnemySpear) return;
			string thisSkin = GamePrefs.CurrentSpearSkinID;
			if (thisSkin == currentSkin) return;
			if (_skeletonAnimation == null) return;
			if (_skeletonAnimation.Skeleton.Data.FindSkin(thisSkin) == null) return;
			currentSkin = thisSkin;
			_skeletonAnimation.Skeleton.SetSkin(thisSkin);
			_skeletonAnimation.Skeleton.SetSlotsToSetupPose();
		}
	}
	public enum SpearAnimStateId
	{
		Idle,
		Dragging,
		Launch,
	}
}

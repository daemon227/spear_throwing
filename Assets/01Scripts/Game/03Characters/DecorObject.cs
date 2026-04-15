using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace DongA.Decorators
{
    public class DecorObject : MonoBehaviour
    {
        [SerializeField] private string dogAnimName;
        private SkeletonAnimation _animator;

        public string AnimName
        {
            get => dogAnimName;
            set => dogAnimName = value;
        }

        void Start()
        {
            _animator = GetComponentInChildren<SkeletonAnimation>();
            if (_animator != null && !string.IsNullOrEmpty(dogAnimName))
                _animator.AnimationName = dogAnimName;
        }
    }
}


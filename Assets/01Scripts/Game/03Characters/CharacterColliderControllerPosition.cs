using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Spine.Unity;
using UnityEngine;

namespace DongA.Characters
{
    public class CharacterColliderControllerPosition : MonoBehaviour
    {
        private BoneFollower boneFollower;

        private void Awake()
        {
            boneFollower = GetComponent<BoneFollower>();
        }
        void Start()
        {
            ActiveBoneFollower(false);
        }

        public void ActiveBoneFollower(bool isActive)
        {
            boneFollower.followXYPosition = isActive;
            boneFollower.followZPosition = isActive;
        }
    }
}


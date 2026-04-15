using UnityEngine;

namespace GameBase
{
    public class AnimationId : MonoBehaviour
    {
        public static int Alive;

        private void Awake()
        {
            Alive = Animator.StringToHash("Alive");
        }
    }
}
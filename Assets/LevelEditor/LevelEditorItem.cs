using DongA.Obstacles;
using UnityEngine;

namespace DongA.Levels
{
    public class LevelEditorItem : MonoBehaviour
    {
        public enum ItemType
        {
            Spear,
            Character,
            Obstacle,
            PowerUp,
            Decor,
            GroundSpriteShape,
            MouseHole
        }

        [Header("Level Editor Metadata")]
        public ItemType itemType;

        public int itemIndex;

        public string prefabPath;

        [Header("Character Specific")]
        public string animationState = "";

        [Header("Obstacle Specific")]
        public ObstaclesMoveType moveType = ObstaclesMoveType.None;
        public float moveDistance = 0f;
        public float moveDuration = 0f;
        public float moveSpeed = 0f;

        [Header("Fan Specific")]
        public float windForce = 10f;
        public Vector2 windRange = new Vector2(0.6f, 2.5f); //x: width, y: height
        public Vector2 windTransformOffset = new Vector2(0f, 1f);

        [Header("Trampoline Specific")]
        public float jumpForce = 350f;
    }
    
}

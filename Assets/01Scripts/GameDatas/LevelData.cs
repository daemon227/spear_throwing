using System;
using System.Collections.Generic;
using DongA.Characters;
using DongA.Obstacles;
using UnityEngine;
using UnityEngine.U2D;

namespace DongA.Levels
{
    [Serializable]
    public class LevelData
    {
        // Design-time camera info for viewport position conversion.
        // When designOrthoSize > 0, positions are stored as viewport coordinates (0-1).
        // Old files (designOrthoSize == 0) keep raw world positions for backward compatibility.
        public float designOrthoSize;
        public float designAspectRatio;

        public bool hasSpear;
        public SpearData spear;
        public List<SpearData> enemySpears = new List<SpearData>();
        public List<CharacterData> characters = new List<CharacterData>();
        public List<ObstacleData> obstacles = new List<ObstacleData>();
        public List<PowerUpData> powerUps = new List<PowerUpData>();
        public List<DecorData> decorObjects = new List<DecorData>();
        public List<GroundSpriteShapeData> groundSpriteShapes = new List<GroundSpriteShapeData>();
        public List<MouseHoleData> mouseHoles = new List<MouseHoleData>();
    }

    [Serializable]
    public class CharacterData
    {
        public int characterIndex;
        public string animationState;
        public Vector2 position;
        public Vector3 rotation;
        public Vector2 scale;
        public float gravityScale;

        // Movement
        public MoveType moveType;
        public bool usePhysicsFall;
        public float moveDistance;
        public float moveDuration;
        public float waitTime;
        public Vector2 moveTargetPoint;

        // BigCharacter
        public int spawnCount;
        public float spawnRadius;
        public float spawnForce;
        public float childSizeMultiplier;

        // Wizard
        public int numberTeleport;

        // Wizard1
        public Vector2 wizard1PointB;
    }

    [Serializable]
    public class ObstacleData
    {
        public int obstacleIndex;
        public Vector2 position;
        public Vector3 rotation;
        public Vector2 scale;

        public ObstaclesMoveType moveType;
        public float moveDistance;
        public float moveDuration;
        public float waitTime;
        
        public float gravityScale;

        public float windForce; // For Fan
        public Vector2 windRange; // For Fan
        public Vector2 windTransformOffset; // For Fan
        public float jumpForce; // For Trampoline
        public Vector2 ballonEnemyFollowOffset; // For Ballon

        // Portal
        public int portalID;
        public int linkedPortalID;
        public float appearDelay;
        public float portalMoveSpeed;

        // WashingMachine
        public int washingMachineCharacterType;
        public int washingMachineCount;
        public float washingMachineScale;

        // Egg
        public Vector2 eggEnemyLocalPosition;
        public float eggTopMoveDistance;
        public float eggTopMoveDuration;

        // Ball
        public int ballMoveMode; // BallMoveMode as int
        public Vector3 ballPointB;
        public float ballCurveHeight;
        public int ballId;

        // Vehical
        public int vehicalMoveMode; // VehicalMoveMode as int
        public Vector3 vehicalPointB;
        public float vehicalCurveHeight;
        public bool vehicalUsePhysicsFall;
        public Vector2 vehicalEnemyPosOffset;
    }

    [Serializable]
    public class PowerUpData
    {
        public int powerUpIndex;
        public Vector2 position;
        public Vector3 rotation;
        public Vector2 scale;
        public float gravityScale;
    }

    [Serializable]
    public class SpearData
    {
        public int spearIndex;
        public Vector2 position;
        public Vector3 rotation;
        public Vector2 scale;
        public float gravityScale;
    }

    [Serializable]
    public class GroundSpriteShapePointData
    {
        public Vector3 position;
        public Vector3 leftTangent;
        public Vector3 rightTangent;
        public int tangentMode; // ShapeTangentMode as int
        public float height;
        public bool corner;
    }

    [Serializable]
    public class GroundSpriteShapeData
    {
        public int groundSpriteShapeIndex;
        public Vector2 position;
        public Vector3 rotation;
        public Vector2 scale;
        public List<GroundSpriteShapePointData> points = new List<GroundSpriteShapePointData>();
    }

    [Serializable]
    public class MouseHoleData
    {
        public int mouseHoleIndex;
        public Vector2 position;
        public Vector3 rotation;
        public Vector2 scale;
        public int mouseHoleType;
        public int enemyCount;
        public float spawnScale;
        public List<Vector2> spawnPointPositions = new();
    }

    [Serializable]
    public class DecorData
    {
        public int decorIndex;
        public Vector2 position;
        public Vector3 rotation;
        public Vector2 scale;
        public string animName;
    }
}


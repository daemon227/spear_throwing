using UnityEngine;

namespace DongA.Levels
{
    /// <summary>
    /// Runtime utility for converting LevelData viewport positions to world positions.
    /// Use this when loading levels at runtime on any device/screen size.
    /// </summary>
    public static class LevelViewportHelper
    {
        /// <summary>
        /// Convert all viewport positions in a LevelData to world positions
        /// using the given camera. Call this after deserializing a level JSON
        /// and before spawning objects.
        /// <para>
        /// Old level files (designOrthoSize == 0) store raw world positions
        /// and are returned as-is for backward compatibility.
        /// </para>
        /// </summary>
        public static void ConvertToWorldPositions(LevelData data, Camera cam)
        {
            if (data == null || cam == null) return;
            if (data.designOrthoSize <= 0f) return; // old format, already world space

            if (data.hasSpear && data.spear != null)
                data.spear.position = ViewportToWorld(cam, data.spear.position);

            if (data.characters != null)
                foreach (var c in data.characters)
                    c.position = ViewportToWorld(cam, c.position);

            if (data.obstacles != null)
                foreach (var o in data.obstacles)
                    o.position = ViewportToWorld(cam, o.position);

            if (data.powerUps != null)
                foreach (var p in data.powerUps)
                    p.position = ViewportToWorld(cam, p.position);
        }

        /// <summary>Convert a single viewport (0-1) coordinate to world 2D position.</summary>
        public static Vector2 ViewportToWorld(Camera cam, Vector2 viewportPos)
        {
            Vector3 world = cam.ViewportToWorldPoint(new Vector3(viewportPos.x, viewportPos.y, 0f));
            return new Vector2(world.x, world.y);
        }

        /// <summary>Convert a world position to viewport (0-1) coordinates.</summary>
        public static Vector2 WorldToViewport(Camera cam, Vector3 worldPos)
        {
            Vector3 vp = cam.WorldToViewportPoint(worldPos);
            return new Vector2(vp.x, vp.y);
        }
    }
}

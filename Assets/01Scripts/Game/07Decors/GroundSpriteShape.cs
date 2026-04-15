using System.Collections.Generic;
using DongA.Levels;
using UnityEngine;
using UnityEngine.U2D;

namespace DongA.Decors
{
    [RequireComponent(typeof(SpriteShapeController))]
    public class GroundSpriteShape : MonoBehaviour
    {
        public void CollectPoints(GroundSpriteShapeData data)
        {
            var spline = GetComponent<SpriteShapeController>().spline;
            int count = spline.GetPointCount();

            data.points = new List<GroundSpriteShapePointData>(count);
            for (int i = 0; i < count; i++)
            {
                data.points.Add(new GroundSpriteShapePointData
                {
                    position     = spline.GetPosition(i),
                    leftTangent  = spline.GetLeftTangent(i),
                    rightTangent = spline.GetRightTangent(i),
                    tangentMode  = (int)spline.GetTangentMode(i),
                    height       = spline.GetHeight(i),
                    corner       = spline.GetCorner(i)
                });
            }
        }

        public void ApplyPoints(GroundSpriteShapeData data)
        {
            if (data?.points == null || data.points.Count == 0) return;

            var controller = GetComponent<SpriteShapeController>();
            var spline = controller.spline;

            spline.Clear();
            for (int i = 0; i < data.points.Count; i++)
            {
                var p = data.points[i];
                spline.InsertPointAt(i, p.position);
                spline.SetTangentMode(i, (ShapeTangentMode)p.tangentMode);
                spline.SetLeftTangent(i, p.leftTangent);
                spline.SetRightTangent(i, p.rightTangent);
                spline.SetHeight(i, p.height);
                spline.SetCorner(i, p.corner);
            }
        }
    }
}

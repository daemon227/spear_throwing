using System.Collections.Generic;
using UnityEngine;

namespace DongA.SpearManager
{
    public abstract class SpearFlightBehavior : MonoBehaviour
    {
        protected Component Controller { get; private set; }
        protected List<Vector3> TrajectoryPoints { get; private set; } = new List<Vector3>();

        public virtual void Initialize(Component controller)
        {
            Controller = controller;
        }

        /// <summary>
        /// Gán các điểm trajectory đã tính sẵn từ SpearTrajectoryLine trước khi Launch
        /// </summary>
        public void SetTrajectoryPoints(IReadOnlyList<Vector3> points)
        {
            TrajectoryPoints.Clear();
            if (points != null)
            {
                for (int i = 0; i < points.Count; i++)
                    TrajectoryPoints.Add(points[i]);
            }
        }

        public abstract void Launch(Rigidbody2D rb, Vector2 direction, float speed);

        public virtual void Stop(Rigidbody2D rb)
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        public virtual void Tick(Component controller)
        {
        }
    }
}

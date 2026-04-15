using DongA.SpearLogic;
using UnityEngine;

namespace DongA.Powerups
{
    public class CloneSpearPowerup : PowerupBase
    {
        [SerializeField] private int numberOfClones = 2;
        [SerializeField] private float angleBetweenSpears = 20f;
        
        public override void PowerupReceived(GameObject spearPrefab = null, Vector2 spearDirection = default)
        {
            // Debug.Log($"Cloning spear into {numberOfClones} clones!");
            SpawnCloneSpears(spearPrefab, spearDirection);
            base.PowerupReceived(spearPrefab, spearDirection);
        }

       
        private void SpawnCloneSpears(GameObject spearPrefab, Vector2 originalDirection)
        {
            if (spearPrefab == null)
            {
                Debug.LogWarning("Spear prefab is null. Cannot clone spear.");
                return;
            }

            if (originalDirection.sqrMagnitude < 0.001f)
            {
                Debug.LogWarning("Original spear direction is zero. Cannot determine clone directions.");
                return;
            }

            originalDirection = originalDirection.normalized;

            float[] offsets = CalculateAngleOffsets(numberOfClones, angleBetweenSpears);

            for (int i = 0; i < numberOfClones; i++)
            {
                float offsetAngle = offsets[i];
                Vector2 cloneDirection = RotateVector2(originalDirection, offsetAngle);

                // Instantiate với rotation mặc định của prefab
                GameObject clone = Instantiate(spearPrefab, transform.position, spearPrefab.transform.rotation);
                clone.transform.SetParent(transform.parent);

                SpearController cloneController = clone.GetComponent<SpearController>();
                if (cloneController != null)
                {
                    cloneController.StartStuck = false;
                    cloneController.isCloneSpear = true;
                    cloneController.Launch(cloneDirection);

                    // Align rotation trực tiếp trên transform (MoveRotation bị delay đến FixedUpdate)
                    AlignSpearToDirection(clone, cloneController, cloneDirection);
                }

                // Debug.Log($"Spawned clone spear {i + 1}/{numberOfClones}, offset angle: {offsetAngle:F1}°");
            }
        }

        /// <summary>
        /// Tính mảng góc lệch cho N spear clone, phân bố đối xứng quanh 0°.
        /// Ví dụ: N=2, step=20 → [-20, 20]
        ///         N=3, step=20 → [-20, 0, 20]
        ///         N=4, step=20 → [-40, -20, 20, 40]
        /// </summary>
        private float[] CalculateAngleOffsets(int count, float step)
        {
            float[] offsets = new float[count];

            if (count % 2 == 0)
            {
                int half = count / 2;
                for (int i = 0; i < half; i++)
                {
                    float multiplier = half - i;
                    offsets[i] = -multiplier * step;
                    offsets[count - 1 - i] = multiplier * step;
                }
            }
            else
            {
                // Lẻ: có 1 clone ở góc 0, còn lại đối xứng
                int mid = count / 2;
                offsets[mid] = 0f;
                for (int i = 1; i <= mid; i++)
                {
                    offsets[mid - i] = -i * step;
                    offsets[mid + i] = i * step;
                }
            }

            return offsets;
        }

        private Vector2 RotateVector2(Vector2 v, float angleDegrees)
        {
            float rad = angleDegrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

        /// <summary>
        /// Xoay transform của spear clone sao cho trục freeTip→rotatedTip trùng với direction.
        /// Dùng transform.rotation trực tiếp thay vì MoveRotation (bị delay).
        /// </summary>
        private void AlignSpearToDirection(GameObject clone, SpearController controller, Vector2 direction)
        {
            Transform freeTip = controller.FreeTip;
            Transform rotatedTip = controller.RotatedTip;

            if (freeTip == null || rotatedTip == null)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                clone.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                return;
            }

            Vector2 tipDir = ((Vector2)freeTip.position - (Vector2)rotatedTip.position).normalized;
            if (tipDir.sqrMagnitude < 0.0001f)
                return;

            float deltaAngle = Vector2.SignedAngle(tipDir, direction.normalized);
            float targetZ = clone.transform.eulerAngles.z + deltaAngle;
            clone.transform.rotation = Quaternion.Euler(0f, 0f, targetZ);
        }
    }
}

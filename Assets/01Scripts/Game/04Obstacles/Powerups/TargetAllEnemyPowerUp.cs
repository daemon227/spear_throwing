using DongA.Characters;
using DongA.SpearLogic;
using UnityEngine;

namespace DongA.Powerups
{
    public class TargetAllEnemyPowerUp : PowerupBase
    {
        [SerializeField] private GameObject targetPrefab;
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private float targetingSpeed = 25f;
        [SerializeField] private float targetRotationSpeed = 720f;

        public override void PowerupReceived(GameObject spearPrefab = null, Vector2 spearDirection = default)
        {
            if (spearPrefab != null)
            {
                var spearController = spearPrefab.GetComponent<SpearController>();
                if (spearController != null)
                {
                    // Replace existing elemental effect with our effect
                    if (spearController.currentElementalEffect != null)
                        Destroy(spearController.currentElementalEffect);

                    if (effectPrefab != null)
                    {
                        var effect = Instantiate(effectPrefab, spearController.FreeTip.position, spearController.FreeTip.rotation);
                        effect.transform.SetParent(spearController.FreeTip);
                        spearController.currentElementalEffect = effect;
                    }

                    // Build target queue from all active enemies
                    spearController.targetEnemyQueue.Clear();
                    spearController.targetEnemyMarkers.Clear();

                    var levelRoot = GameObject.Find(KeyPrefs.LevelRootName);
                    Transform markerParent = levelRoot != null ? levelRoot.transform : null;

                    CharacterBase[] allCharacters = FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);
                    foreach (var character in allCharacters)
                    {
                        if (character == null || !character.gameObject.activeInHierarchy) continue;

                        // Use child collider position (collider is on child, CharacterBase on parent)
                        var enemyCollider = character.GetComponentInChildren<Collider2D>();
                        Vector3 spawnPos = enemyCollider != null ? enemyCollider.bounds.center : character.transform.position;

                        GameObject marker = null;
                        if (targetPrefab != null)
                        {
                            marker = Instantiate(targetPrefab, spawnPos, Quaternion.identity);
                            marker.transform.SetParent(markerParent);
                        }

                        spearController.targetEnemyQueue.Add(character);
                        spearController.targetEnemyMarkers.Add(marker);
                    }

                    spearController.targetingSpeed = targetingSpeed;
                    spearController.targetRotationSpeed = targetRotationSpeed;
                    spearController.spearType = SpearType.TargetAllEnemy;
                }
            }

            base.PowerupReceived(spearPrefab, spearDirection);
        }
    }
}

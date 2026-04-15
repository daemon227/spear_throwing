using System.Collections;
using System.Collections.Generic;
using DongA.UI;
using UnityEngine;

namespace DongA.SpearLogic
{
    public class BreakSpear : MonoBehaviour
    {
        [SerializeField] private BreakSpearSO breakSpearSO;
        [SerializeField] private List<GameObject> breakPieces;
        [SerializeField] private float autoDestroyTime = 2f;

        private void Start()
        {
            Destroy(gameObject, autoDestroyTime);
        }

        public void Initialize(bool isEnemySpear)
        {
            if (isEnemySpear) return;
            if (breakSpearSO == null) return;

            string skinID = GamePrefs.CurrentSpearSkinID;
            BreakSpearData data = breakSpearSO.BreakSpearDatas.Find(d => d.SpearSkinID == skinID);
            if (data == null) return;

            for (int i = 0; i < breakPieces.Count && i < data.BreakSprites.Count; i++)
            {
                var sr = breakPieces[i].GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = data.BreakSprites[i];
                }
            }
        }
    }

}

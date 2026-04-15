using System.Collections;
using System.Collections.Generic;
using DongA.GameDatas;
using UnityEngine;

namespace DongA.SpearLogic
{
    public class SpearTrailManager : MonoBehaviour
    {
        [SerializeField] private Transform _trailTransfrom;

        [SerializeField] private AllTrailSkinDataSO _data;
        private GameObject _currentTrail;
        private string _currentTrailID = "";
        private SpearController _spearController;
        void Awake()
        {
            _spearController = GetComponent<SpearController>();
        }

        void Start()
        {
            if(_spearController == null)return;
            if(_spearController.isEnemySpear) return;
            ChangeTrail();
        }
        void OnEnable()
        {
            GameEvents.OnTrailSkinChosen += ChangeTrail;
        }
        void OnDisable()
        {
            GameEvents.OnTrailSkinChosen -= ChangeTrail;
        }

        private void ChangeTrail()
        {
            if(_currentTrailID == GamePrefs.CurrentTrailSkinID) return;
            if(_data == null) return;
            if(_currentTrail != null) Destroy(_currentTrail);
            foreach (var trail in _data.AllTrailSkinDataList)
            {
                if(trail.skinNameID == GamePrefs.CurrentTrailSkinID)
                {
                    _currentTrail = Instantiate(trail.trailPrefab, _trailTransfrom.transform.position, Quaternion.identity);
                    _currentTrail.transform.SetParent(_trailTransfrom);
                    _currentTrailID = GamePrefs.CurrentTrailSkinID;
                    _currentTrail.transform.localScale = Vector3.one;
                }
            }
        }
    }
}


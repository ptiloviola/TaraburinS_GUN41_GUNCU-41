using System;
using UnityEngine;

namespace Gameplay.Spawning.Data
{
    [Serializable]
    public class EnemySpawnData
    {
        [Tooltip("ID врага, который мы пишем в WaveConfig (например: enemy_1)")]
        [SerializeField] private string _enemyId;
        
        [Tooltip("Префаб этого врага")]
        [SerializeField] private GameObject _prefab;

        public EnemySpawnData() { }

        public EnemySpawnData(string enemyId, GameObject prefab)
        {
            _enemyId = enemyId;
            _prefab = prefab;
        }

        public string EnemyId => _enemyId;
        public GameObject Prefab => _prefab;
    }
}
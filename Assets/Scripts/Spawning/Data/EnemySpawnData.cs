using System;
using UnityEngine;

namespace Gameplay.Spawning.Data
{
    [Serializable]
    public class EnemySpawnData
    {
        [Tooltip("ID врага, который мы пишем в WaveConfig (например: enemy_1)")]
        public string EnemyId;
        
        [Tooltip("Префаб этого врага")]
        public GameObject Prefab;
    }

}
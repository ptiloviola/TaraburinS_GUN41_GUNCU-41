using System;
using UnityEngine;
using Gameplay.Grid;
using UnityEngine.Serialization;

namespace Gameplay.Spawning.Data
{
    [Serializable]
    public class SquadData
    {
        [EnemyId]
        [Tooltip("ID врага из реестра (выбирается из списка)")]
        [FormerlySerializedAs("EnemyId")]
        [SerializeField] private string _enemyId;
        
        [Tooltip("Количество врагов в этом отряде")]
        [SerializeField] private int _count = 5;
        
        [Tooltip("Пауза между спавном каждого врага в отряде")]
        [SerializeField] private float _spawnInterval = 1.0f;

        [GridPointId(NodeType.Spawn)] 
        [Tooltip("ID точки старта")]
        [SerializeField] private string _spawnPointId = ""; 
        
        [GridPointId(NodeType.Base)] 
        [Tooltip("ID Базы (оставь пустым для ближайшей)")]
        [SerializeField] private string _targetBaseId = "";

        


        public SquadData() { }

        // 2. Конструктор для процедурной генерации рогалика
        public SquadData(string enemyId, int count, float spawnInterval, string spawnPointId, string targetBaseId = "")
        {
            _enemyId = enemyId;
            _count = count;
            _spawnInterval = spawnInterval;
            _spawnPointId = spawnPointId;
            _targetBaseId = targetBaseId;
        }


        public string EnemyId => _enemyId;
        public int Count => _count;
        public float SpawnInterval => _spawnInterval;
        public string SpawnPointId => _spawnPointId;
        public string TargetBaseId => _targetBaseId;
    }
}
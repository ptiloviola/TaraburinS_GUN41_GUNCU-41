using System;
using UnityEngine;

namespace Gameplay.Spawning.Data
{
    [Serializable]
    public class SquadData
    {
        [Tooltip("ID врага из реестра (например, 'goblin_basic')")]
        public string EnemyId; 
        
        [Tooltip("Количество врагов в этом отряде")]
        public int Count = 5;
        
        [Tooltip("Пауза между спавном каждого врага в отряде")]
        public float SpawnInterval = 1.0f;
        
        [Tooltip("Точка старта (для будущей многолинейности)")]
        public string SpawnPointId = "MainSpawner"; 
    }
}
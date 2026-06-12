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
        
        [Tooltip("ID точки старта (например: Spawn_0_4)")]
        public string SpawnPointId = "Spawn_0_0"; 
        
        [Tooltip("ID Базы, которую пойдут атаковать (оставь пустым для ближайшей/главной)")]
        public string TargetBaseId = ""; // НОВОЕ

    }
}
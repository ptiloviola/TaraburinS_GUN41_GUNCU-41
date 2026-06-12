using System;
using UnityEngine;
using Gameplay.Grid;

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
        
        // [Tooltip("ID точки старта (например: Spawn_0_4)")]
        // public string SpawnPointId = "Spawn_0_0"; 
        
        // [Tooltip("ID Базы, которую пойдут атаковать (оставь пустым для ближайшей/главной)")]
        // public string TargetBaseId = ""; // НОВОЕ

        [GridPointId(NodeType.Spawn)] // НОВОЕ: Говорим инспектору искать Спавны
        [Tooltip("ID точки старта")]
        public string SpawnPointId = ""; 
        
        [GridPointId(NodeType.Base)] // НОВОЕ: Говорим инспектору искать Базы
        [Tooltip("ID Базы (оставь пустым для ближайшей)")]
        public string TargetBaseId = "";

    }
}
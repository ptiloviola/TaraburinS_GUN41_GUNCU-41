using System;
using System.Collections.Generic;
using UnityEngine;

namespace TpsShooter.Enemies.Configs
{
    [Serializable]
    public class EnemySpawnData
    {
        [Tooltip("Базовый префаб врага (твоя настроенная капсула TestEnemy)")]
        public GameObject BasePrefab;
        
        [Tooltip("Конфиг, определяющий логику (Милишник/Стрелок, пушка, ХП)")]
        public EnemyConfig Config;
        
        [Tooltip("Количество таких врагов в волне")]
        public int Count = 1;
        [Tooltip("Индекс маршрута патрулирования из Спавнера (0 - первый маршрут, 1 - второй и т.д.)")]
        public int RouteIndex = 0;
    }

    [Serializable]
    public class WaveData
    {
        [Tooltip("Задержка перед началом спавна этой волны (сек)")]
        public float StartDelay = 2f;
        
        [Tooltip("Интервал между появлением врагов внутри волны (сек)")]
        public float SpawnInterval = 1f;

        [Tooltip("Группы врагов для спавна")]
        public List<EnemySpawnData> Enemies;
    }

    [CreateAssetMenu(fileName = "LevelWavesConfig", menuName = "TpsShooter/Enemies/LevelWavesConfig")]
    public class LevelWavesConfig : ScriptableObject
    {
        [Tooltip("Список волн на уровне")]
        public List<WaveData> Waves;
    }
}
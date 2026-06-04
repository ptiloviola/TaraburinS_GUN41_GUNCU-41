using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Spawner
{
    // Serializable нужен, чтобы Unity могла отобразить эту структуру в Инспекторе
    [System.Serializable]
    public class WaveData
    {
        [Tooltip("Количество врагов в волне")]
        public int EnemyCount = 5;
        [Tooltip("Пауза между спавном каждого врага")]
        public float SpawnInterval = 2.0f;
        [Tooltip("Задержка ПЕРЕД началом этой волны (передышка для игрока)")]
        public float DelayBeforeWave = 5.0f;
    }
    [CreateAssetMenu(fileName = "NewWaveConfig", menuName = "TD/Wave Config", order = 51)]
    public class WaveConfig : ScriptableObject
    {
        [Header("Настройки всех волн уровня")]
        public List<WaveData> Waves = new List<WaveData>();
    }
}



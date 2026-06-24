using System;
using System.Collections.Generic;
using UnityEngine;
using VacuumSim.Trash; // Чтобы видеть TrashType

namespace VacuumSim.Spawning
{
    // 1. Режимы появления мусора
    public enum SpawnMode 
    { 
        FixedInterval, // Строго через равные промежутки (например, каждые 2 секунды)
        Randomized     // Случайно выплевывать мусор в течение времени волны
    }

    // 2. Задача спавна (одна строка в волне)
    [Serializable]
    public class TrashSpawnTask
    {
        [Tooltip("Какой мусор спавним")]
        public TrashType TrashType;
        
        [Tooltip("Сколько штук нужно выбросить")]
        public int Amount = 5;
        
        [Tooltip("Как именно они будут появляться")]
        public SpawnMode Mode = SpawnMode.Randomized;
    }

    // 3. Сама Волна
    [Serializable]
    public class TrashWave
    {
        [Tooltip("Время тишины ПЕРЕД началом этой волны (в секундах)")]
        public float DelayBeforeWave = 5f;
        
        [Tooltip("Сколько секунд длится сама волна (внутри этого времени работает SpawnMode)")]
        public float Duration = 20f;
        
        [Tooltip("Что именно спавним в этой волне")]
        public List<TrashSpawnTask> SpawnTasks;
    }

    // 4. Главный ассет уровня
    [CreateAssetMenu(fileName = "NewLevelWaves", menuName = "VacuumSim/Level Waves Config")]
    public class TrashWavesConfig : ScriptableObject
    {
        [Tooltip("Список всех волн на уровне")]
        public List<TrashWave> Waves;
        
        [Tooltip("Начать заново после последней волны?")]
        public bool LoopWaves = true; 
    }
}
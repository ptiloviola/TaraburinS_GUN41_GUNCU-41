using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Spawning.Data
{
    [Serializable]
    public class WaveData
    {
        [Header("Логика старта волны")]
        [Tooltip("Как должна начаться эта волна?")]
        public WaveStartMode StartMode = WaveStartMode.TimeAfterPrevious;
        [Tooltip("Сколько секунд длится бой ДО того, как появится таймер следующей волны (Кнопка отключена)")]
        public float ActiveWaveDuration = 15f; // НОВОЕ ПОЛЕ
        [Tooltip("Таймер ПЕРЕД началом этой волны (Кнопка 'Досрочно' активна)")]
        public float DelayBeforeWave = 10f;
 
        [Tooltip("Отряды, из которых состоит эта волна")]
        public List<SquadData> Squads = new List<SquadData>();
        
        [Tooltip("Награда за полную зачистку волны")]
        public int ClearReward = 50;
    }
}

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
        [SerializeField] private WaveStartMode _startMode = WaveStartMode.TimeAfterPrevious;
        
        [Tooltip("Сколько секунд длится бой ДО того, как появится таймер следующей волны")]
        [SerializeField] private float _activeWaveDuration = 15f;
        
        [Tooltip("Таймер ПЕРЕД началом этой волны")]
        [SerializeField] private float _delayBeforeWave = 10f;
 
        [Tooltip("Отряды, из которых состоит эта волна")]
        [SerializeField] private List<SquadData> _squads = new List<SquadData>();
        
        [Tooltip("Награда за полную зачистку волны")]
        [SerializeField] private int _clearReward = 50;

        // 1. Пустой конструктор для сериализации Unity Инспектора
        public WaveData() { }

        // 2. Конструктор для процедурной генерации рогалика
        public WaveData(WaveStartMode startMode, float delay, float duration, int reward, List<SquadData> squads)
        {
            _startMode = startMode;
            _delayBeforeWave = delay;
            _activeWaveDuration = duration;
            _clearReward = reward;
            _squads = squads ?? new List<SquadData>();
        }

        // Публичные свойства только для чтения
        public WaveStartMode StartMode => _startMode;
        public float ActiveWaveDuration => _activeWaveDuration;
        public float DelayBeforeWave => _delayBeforeWave;
        public int ClearReward => _clearReward;
        
        // Защищенная коллекция отрядов
        public IReadOnlyList<SquadData> Squads => _squads;
    }
}
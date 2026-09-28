using System.Collections.Generic;
using UnityEngine;
using Gameplay.Grid;

namespace Gameplay.Spawning.Data
{
    [CreateAssetMenu(fileName = "NewLevelWaves", menuName = "TD/Level Waves Config")]
    public class LevelWavesConfig : ScriptableObject
    {
        [Header("Связь с картой")]
        [Tooltip("Карта (сетка), для которой настраиваются эти волны. Нужна для выпадающего списка точек.")]
        [SerializeField] private GridConfig _targetGrid;
        
        [Header("Сценарий классического уровня")]
        [SerializeField] private List<WaveData> _waves = new List<WaveData>();

        public IReadOnlyList<WaveData> Waves => _waves;
        
        public GridConfig TargetGrid => _targetGrid; 
    }
}
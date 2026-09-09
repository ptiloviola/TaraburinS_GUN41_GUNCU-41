using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Spawning.Data
{
    [CreateAssetMenu(fileName = "NewLevelWaves", menuName = "TD/Level Waves Config")]
    public class LevelWavesConfig : ScriptableObject
    {
        [Header("Сценарий классического уровня")]
        [SerializeField] private List<WaveData> _waves = new List<WaveData>();

        // Отдаем только для чтения. Защита от изменения структуры волн в рантайме.
        public IReadOnlyList<WaveData> Waves => _waves;
    }
}
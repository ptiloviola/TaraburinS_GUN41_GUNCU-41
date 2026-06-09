using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Spawning.Data
{
    [CreateAssetMenu(fileName = "NewLevelWaves", menuName = "TD/Level Waves Config")]
    public class LevelWavesConfig : ScriptableObject
    {
        [Header("Сценарий классического уровня")]
        public List<WaveData> Waves = new List<WaveData>();
    }
}

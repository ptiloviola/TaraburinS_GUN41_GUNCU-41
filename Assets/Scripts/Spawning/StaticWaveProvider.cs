using Gameplay.Spawning.Data;
using System.Collections.Generic;


namespace Gameplay.Spawning
{
    public class StaticWaveProvider : IWaveProvider
    {
        private readonly LevelWavesConfig _config;
        private int _currentWaveIndex = 0;


        public int TotalWaves => _config != null ? _config.Waves.Count : 0;

        public StaticWaveProvider(LevelWavesConfig config)
        {
            _config = config;
        }

        public bool HasNextWave()
        {
            return _config != null && _currentWaveIndex < _config.Waves.Count;
        }

        public WaveData GetNextWave()
        {
            if (HasNextWave())
            {
                return _config.Waves[_currentWaveIndex++];
            }
            return null;
        }

        public WaveData PeekNextWave()
        {
            if (HasNextWave())
            {
                return _config.Waves[_currentWaveIndex];
            }
            return null;
        }

        public IEnumerable<WaveData> GetAllWaves()
        {
            if (_config == null || _config.Waves == null) 
                return System.Linq.Enumerable.Empty<WaveData>();
                
            return _config.Waves;
        }
    }
}
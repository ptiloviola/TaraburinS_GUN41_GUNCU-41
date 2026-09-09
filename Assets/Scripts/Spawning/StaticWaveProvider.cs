using Gameplay.Spawning.Data;

namespace Gameplay.Spawning
{
    /// <summary>
    /// Провайдер, который берет волны из статичного ScriptableObject.
    /// Идеально для классических (созданных вручную) уровней.
    /// </summary>
    public class StaticWaveProvider : IWaveProvider
    {
        private readonly LevelWavesConfig _config;
        private int _currentWaveIndex = 0;

        // Реализация свойства из интерфейса
        public int TotalWaves => _config != null ? _config.Waves.Count : 0;

        public StaticWaveProvider(LevelWavesConfig config)
        {
            _config = config;
        }

        public bool HasNextWave()
        {
            // Защита от пустого конфига
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
    }
}
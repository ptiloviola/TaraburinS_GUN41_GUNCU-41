using Gameplay.Spawning.Data;

namespace Gameplay.Spawning
{
    public class StaticWaveProvider : IWaveProvider
{
    private readonly LevelWavesConfig _config;
    private int _currentWaveIndex = 0;

    public int TotalWaves => _config.Waves.Count;

    public StaticWaveProvider(LevelWavesConfig config)
    {
        _config = config;
    }
    public WaveData GetNextWave()
    {
        if (HasNextWave())
        {
            return _config.Waves[_currentWaveIndex++];
        }
        return null;
    }

    public bool HasNextWave()
    {
        return _currentWaveIndex < _config.Waves.Count;
    }
}
}


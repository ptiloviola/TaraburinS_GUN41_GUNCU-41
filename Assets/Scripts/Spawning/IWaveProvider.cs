using Gameplay.Spawning.Data;

namespace Gameplay.Spawning
{
    public interface IWaveProvider
    {
        int TotalWaves { get; } // Новое свойство
        bool HasNextWave();
        WaveData GetNextWave();
    }
}
using Gameplay.Spawning.Data;
namespace Gameplay.Spawning
{
    public interface IWaveProvider
    {
        bool HasNextWave();
        WaveData GetNextWave();
    }
}

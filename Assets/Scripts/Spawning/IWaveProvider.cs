using Gameplay.Spawning.Data;
using System.Collections.Generic;

namespace Gameplay.Spawning
{
    public interface IWaveProvider
    {
        int TotalWaves { get; }
        bool HasNextWave();
        WaveData GetNextWave();
        WaveData PeekNextWave();

        IEnumerable<WaveData> GetAllWaves();
    }
}
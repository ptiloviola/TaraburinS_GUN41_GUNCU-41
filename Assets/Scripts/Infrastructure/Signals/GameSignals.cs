using System.Collections.Generic;
using Gameplay.Interaction;

namespace Gameplay.Infrastructure.Signals
{
    public struct SignalBaseDamaged 
    {
        public int Damage;
        public int CurrentLives;
        
        public SignalBaseDamaged(int damage, int currentLives)
        {
            Damage = damage;
            CurrentLives = currentLives;
        }
    }

    public struct SignalEnemySpawned { }


    public struct SignalEnemyKilled
    {
        public int Reward;
    }

    public struct SignalEnemyReachedBase { }

    public struct SignalAllEnemiesCleared { }

    public struct SignalGameOver
    {
        
    }

    public struct SignalBalanceChanged
    {
        public int CurrentBalance;
    }

    public struct SignalWaveStarted
    {
        public int CurrentWaveIndex;
        public int TotalWaves;
    }

    public struct SignalWaveTimerUpdated
    {
        public float TimeLeft;
        public float Progress;
    }

    public struct SignalForceStartWave { }

    public struct SignalWaveStateChanged
    {
        public int CurrentWave;
        public int TotalWaves;
    }

    public struct SignalWaveForecastUpdated
    {
        public Dictionary<string, int> EnemyCounts; 
    }

    public class SignalStartCombat { }

    public class SignalInteractionModeChanged { public Gameplay.Interaction.InteractionMode Mode; }
    public class SignalTacticalClaimsUpdated { public int Available; public int Max; }

    public class SignalAllWavesSpawned { }
    public class SignalLevelWon { }
    public class SignalLevelLost { }

    

}

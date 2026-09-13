using System.Collections.Generic;
namespace Gameplay.Infrastructure.Signals
{
    // Сигнал вызывается, когда база получает урон
    public struct SignalBaseDamaged 
    {
        public int Damage;
        public int CurrentLives; // Добавляем для UI
        
        public SignalBaseDamaged(int damage, int currentLives)
        {
            Damage = damage;
            CurrentLives = currentLives;
        }
    }

    // Сигнал: Враг только что появился на карте
    public struct SignalEnemySpawned { }


    // Сигнал вызывается, когда любой враг погибает или деспавнится
    public struct SignalEnemyKilled
    {
        public int Reward; // Банк будет читать это поле
    }

    // Сигнал: Враг успешно дошел до базы и исчез
    public struct SignalEnemyReachedBase { }

    // Сигнал: Счетчик живых врагов опустился до нуля (карта чиста)
    public struct SignalAllEnemiesCleared { }

    // Сигнал: Жизни упали до 0
    public struct SignalGameOver
    {
        
    }

    // Сигнал: Изменился баланс кошелька (для UI)
    public struct SignalBalanceChanged
    {
        public int CurrentBalance;
    }

    public struct SignalWaveStarted
    {
        public int CurrentWaveIndex;
        public int TotalWaves;
    }

    // Сигнал, чтобы обновлять кружок таймера в UI
    public struct SignalWaveTimerUpdated
    {
        public float TimeLeft; // Сколько секунд осталось
        public float Progress; // Прогресс от 0.0 до 1.0 (для Fill Amount)
    }

    // Сигнал от кнопки UI к Режиссеру: "Хватит ждать, запускай!"
    public struct SignalForceStartWave { }

    // Сигнал для изменения текста волны (если его еще нет)
    public struct SignalWaveStateChanged
    {
        public int CurrentWave;
        public int TotalWaves;
    }

    // Сигнал с прогнозом грядущей волны
    public struct SignalWaveForecastUpdated
    {
        // Ключ: ID врага (например "goblin"), Значение: общее количество в волне
        public Dictionary<string, int> EnemyCounts; 
    }

    

}

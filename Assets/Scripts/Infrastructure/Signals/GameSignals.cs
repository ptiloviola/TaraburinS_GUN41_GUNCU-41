namespace Infrastructure.Signals
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

    // Сигнал вызывается, когда любой враг погибает или деспавнится
    public struct SignalEnemyKilled
    {
        public int Reward; // Банк будет читать это поле
    }

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

}

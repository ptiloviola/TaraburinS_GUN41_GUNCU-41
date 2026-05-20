namespace Infrastructure.Signals
{
    // Сигнал вызывается, когда база получает урон
    public struct SignalBaseDamaged 
    {
        public int Damage;
        
        public SignalBaseDamaged(int damage)
        {
            Damage = damage;
        }
    }

    // Сигнал вызывается, когда любой враг погибает или деспавнится
    public struct SignalEnemyDied { }
}

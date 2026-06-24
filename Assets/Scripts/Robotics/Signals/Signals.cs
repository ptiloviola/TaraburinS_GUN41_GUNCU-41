using VacuumSim.Trash;

namespace VacuumSim.Robotics.Signals
{
    // Сигнал: Мусор был успешно всосан
    public struct TrashCollectedSignal
    {
        // Передаем данные о мусоре, чтобы системы знали его вес и цену
        public TrashType TrashData; 
    }

    // Сигнал: Состояние батареи изменилось (для UI и Мозга)
    public struct BatteryStateSignal
    {
        public float CurrentCharge;
        public float MaxCharge;
        public float Percentage => CurrentCharge / MaxCharge;
        public bool IsEmpty => CurrentCharge <= 0;
    }

    // Сигнал: Состояние бака изменилось
    public struct DustbinStateSignal
    {
        public float CurrentFill;
        public float MaxCapacity;
        public float Percentage => CurrentFill / MaxCapacity;
        public bool IsFull => CurrentFill >= MaxCapacity;
    }
}

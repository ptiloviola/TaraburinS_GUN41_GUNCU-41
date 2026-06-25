namespace VacuumSim.Robotics.Signals
{
    // Сигнал: Состояние батареи изменилось (для UI и Мозга)
    public struct BatteryStateSignal
    {
        public float CurrentCharge;
        public float MaxCharge;
        public float Percentage => CurrentCharge / MaxCharge;
        public bool IsEmpty => CurrentCharge <= 0;
        public bool IsLow;
    }

}
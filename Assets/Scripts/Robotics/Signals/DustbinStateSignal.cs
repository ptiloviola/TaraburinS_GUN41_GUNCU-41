namespace VacuumSim.Robotics.Signals
{

    // Сигнал: Состояние бака изменилось
    public struct DustbinStateSignal
    {
        public float CurrentFill;
        public float MaxCapacity;
        public float Percentage => CurrentFill / MaxCapacity;
        public bool IsFull => CurrentFill >= MaxCapacity;
    }
}

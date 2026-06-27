namespace VacuumSim.Robotics.Signals
{
    public struct DustbinStateSignal
    {
        public float CurrentFill;
        public float MaxCapacity;
        public float Percentage => CurrentFill / MaxCapacity;
        public bool IsFull => CurrentFill >= MaxCapacity;
    }
}

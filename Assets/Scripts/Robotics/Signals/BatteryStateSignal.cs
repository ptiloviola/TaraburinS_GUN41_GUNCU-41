namespace VacuumSim.Robotics.Signals
{
    public struct BatteryStateSignal
    {
        public float CurrentCharge;
        public float MaxCharge;
        public float Percentage => CurrentCharge / MaxCharge;
        public bool IsEmpty => CurrentCharge <= 0;
        public bool IsLow;
    }

}
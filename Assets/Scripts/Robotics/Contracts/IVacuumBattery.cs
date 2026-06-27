namespace VacuumSim.Robotics.Contracts
{
    public interface IVacuumBattery
    {
        float CurrentCharge { get; }
        bool IsEmpty { get; }
        bool IsFull { get; }
        void Charge(float amount);
        void SetLoadMultiplier(float multiplier); 
    }

}
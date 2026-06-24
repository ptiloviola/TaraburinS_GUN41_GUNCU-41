namespace VacuumSim.Robotics.Contracts
{
    public interface IVacuumBattery
    {
        float CurrentCharge { get; }
        bool IsEmpty { get; }
        // Метод для влияния кота
        void SetLoadMultiplier(float multiplier); 
    }

}
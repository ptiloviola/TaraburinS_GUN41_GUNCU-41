namespace VacuumSim.Robotics.Contracts
{
    public interface IVacuumBattery
    {
        float CurrentCharge { get; }
        bool IsEmpty { get; }
        bool IsFull { get; } // Добавили проверку на полный заряд
        void Charge(float amount); // Добавили метод зарядки
        // Метод для влияния кота
        void SetLoadMultiplier(float multiplier); 
    }

}
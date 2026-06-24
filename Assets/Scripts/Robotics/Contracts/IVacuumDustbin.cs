namespace VacuumSim.Robotics.Contracts
{
    public interface IVacuumDustbin
    {
        float CurrentFill { get; }
        bool IsFull { get; }
        // Метод для будущей станции очистки
        void EmptyBin(); 
    }
}
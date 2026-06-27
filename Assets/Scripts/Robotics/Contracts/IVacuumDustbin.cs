namespace VacuumSim.Robotics.Contracts
{
    public interface IVacuumDustbin
    {
        float CurrentFill { get; }
        bool IsFull { get; }
        void EmptyBin(); 
    }
}
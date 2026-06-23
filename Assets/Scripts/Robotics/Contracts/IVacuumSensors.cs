namespace VacuumSim.Robotics.Contracts
{
    public interface IVacuumSensors
    {
        bool IsObstacleAhead();
        bool IsObstacleLeft();
        bool IsObstacleRight();
    }
}

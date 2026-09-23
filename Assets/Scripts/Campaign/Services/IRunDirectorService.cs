using Gameplay.Combat.Data;

namespace Gameplay.Combat.Services
{
    public interface IRunDirectorService
    {
        bool HasNextNode(RunProgressModel progress);
        MapNode GetNextNode(RunProgressModel progress);
    }
}
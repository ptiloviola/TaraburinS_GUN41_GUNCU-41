using Gameplay.Campaign.Data;

namespace Gameplay.Campaign.Services
{
    public interface IRunDirectorService
    {
        bool HasNextNode(RunProgressModel progress);
        MapNode GetNextNode(RunProgressModel progress);
    }
}
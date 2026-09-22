using Gameplay.Core.Data;
using Gameplay.Campaign.Data;

namespace Gameplay.Core.Services
{
    public interface IRunDirectorService
    {
        bool HasNextNode(RunProgressModel progress);
        MapNode GetNextNode(RunProgressModel progress);
    }
}
using Gameplay.Core.Data;
using Gameplay.Levels.Data;

namespace Gameplay.Core.Services
{
    public interface IRunDirectorService
    {
        bool HasNextLevel(RunProgressModel progress);
        LevelBlueprintConfig GetNextLevel(RunProgressModel progress);
    }
}
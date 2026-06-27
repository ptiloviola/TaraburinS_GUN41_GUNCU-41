using System.Threading;
using Cysharp.Threading.Tasks;

namespace VacuumSim.Robotics.Brain.Strategies
{
    public interface ICleaningStrategy
    {
        UniTask ExecuteAsync(CancellationToken token);
    }
}
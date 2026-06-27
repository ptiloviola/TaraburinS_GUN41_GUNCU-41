using System.Threading;
using Cysharp.Threading.Tasks;

namespace VacuumSim.Robotics.Brain.States
{
    public interface IVacuumState
    {
        UniTask ExecuteAsync(CancellationToken token);
    }
}
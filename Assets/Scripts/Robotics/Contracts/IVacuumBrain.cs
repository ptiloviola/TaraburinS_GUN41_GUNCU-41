using System.Threading;
using Cysharp.Threading.Tasks;

namespace VacuumSim.Robotics.Contracts
{
    public interface IVacuumBrain
    {
        UniTask StartCleaningAsync(CancellationToken token);
    }
}

using System.Threading;
using Cysharp.Threading.Tasks;

namespace VacuumSim.Robotics.Brain.States
{
    public interface IVacuumState
    {
        // Каждое состояние — это асинхронный процесс, который выполняется во времени
        UniTask ExecuteAsync(CancellationToken token);
    }
}
using System.Threading;
using Cysharp.Threading.Tasks;

namespace VacuumSim.Robotics.Brain.Strategies
{
    public interface ICleaningStrategy
    {
        // Каждая стратегия просто выполняет свой специфичный цикл движения
        UniTask ExecuteAsync(CancellationToken token);
    }
}
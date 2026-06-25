using System.Threading;
using Cysharp.Threading.Tasks;
using VacuumSim.Robotics.Brain.Strategies;

namespace VacuumSim.Robotics.Brain.States
{
    public class CleaningState : IVacuumState
    {
        private readonly ICleaningStrategy _activeStrategy;

        public CleaningState(ICleaningStrategy activeStrategy)
        {
            _activeStrategy = activeStrategy;
        }

        public async UniTask ExecuteAsync(CancellationToken token)
        {
            // Запускаем алгоритм движения и ждем его выполнения/отмены
            await _activeStrategy.ExecuteAsync(token);
        }
    }
}
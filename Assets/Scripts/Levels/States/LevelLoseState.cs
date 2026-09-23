using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Infrastructure.Signals;
using Zenject;

namespace Gameplay.Levels.States
{
    public class LevelLoseState : ILevelState
    {
        private readonly SignalBus _signalBus;

        public LevelLoseState(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {
            _signalBus.Fire<SignalLevelLost>();
            return UniTask.CompletedTask;
        }

        public UniTask ExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
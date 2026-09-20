using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Infrastructure.Signals;
using Zenject;

namespace Gameplay.Levels.States
{
    public class LevelWinState : ILevelState
    {
        private readonly SignalBus _signalBus;

        public LevelWinState(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {
            _signalBus.Fire<SignalLevelWon>();
            return UniTask.CompletedTask;
        }

        public UniTask ExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
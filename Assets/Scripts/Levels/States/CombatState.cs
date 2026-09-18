using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Spawning.Services;
using Gameplay.Interaction;
using Gameplay.Infrastructure.Signals;
using Zenject; // Не забудь Zenject для SignalBus

namespace Gameplay.Levels.States
{
    public class CombatState : ILevelState
    {
        private readonly WaveStateController _waveController;
        private readonly InteractionStateModel _interactionState;
        private readonly SignalBus _signalBus; // ДОБАВЛЕНО

        public CombatState(
            WaveStateController waveController, 
            InteractionStateModel interactionState, 
            SignalBus signalBus) // ДОБАВЛЕНО В КОНСТРУКТОР
        {
            _waveController = waveController;
            _interactionState = interactionState;
            _signalBus = signalBus;
        }

        public UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {
            _interactionState.CurrentMode = InteractionMode.Normal;
            
            // Теперь _signalBus не null, краша не будет!
            _signalBus.Fire(new SignalInteractionModeChanged { Mode = InteractionMode.Normal });
            
            _waveController.RunWavesLoopAsync(ct).Forget();
            return UniTask.CompletedTask;
        }

        public UniTask ExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
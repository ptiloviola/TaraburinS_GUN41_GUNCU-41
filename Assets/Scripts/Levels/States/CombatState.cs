using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Spawning.Services;
using Gameplay.Interaction; // НОВОЕ

namespace Gameplay.Levels.States
{
    public class CombatState : ILevelState
    {
        private readonly WaveStateController _waveController;
        private readonly InteractionStateModel _interactionState; // НОВОЕ

        public CombatState(WaveStateController waveController, InteractionStateModel interactionState)
        {
            _waveController = waveController;
            _interactionState = interactionState;
        }

        public UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {
            // Возвращаем обычный режим (можно открывать магазин и строить на фундаментах)
            _interactionState.CurrentMode = InteractionMode.Normal;
            
            _waveController.RunWavesLoopAsync(ct).Forget();
            return UniTask.CompletedTask;
        }

        public UniTask ExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
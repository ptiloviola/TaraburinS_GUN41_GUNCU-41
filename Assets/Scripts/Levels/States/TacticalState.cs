using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Infrastructure.Signals;
using Gameplay.Interaction; // НОВОЕ
using Zenject;

namespace Gameplay.Levels.States
{
    public class TacticalState : ILevelState
    {
        private readonly SignalBus _signalBus;
        private readonly InteractionStateModel _interactionState; // НОВОЕ
        
        private LevelStateMachine _cachedStateMachine; 

        public TacticalState(SignalBus signalBus, InteractionStateModel interactionState)
        {
            _signalBus = signalBus;
            _interactionState = interactionState;
        }

        public UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {
            _cachedStateMachine = stateMachine;
            
            // Включаем режим разметки и выдаем 5 фундаментов для теста!
            _interactionState.CurrentMode = InteractionMode.TacticalClaim;
            _interactionState.AvailableClaims = 5;
            
            _signalBus.Subscribe<SignalStartCombat>(OnCombatStarted);
            return UniTask.CompletedTask;
        }

        public UniTask ExitAsync(CancellationToken ct)
        {
            _signalBus.Unsubscribe<SignalStartCombat>(OnCombatStarted);
            _cachedStateMachine = null; 
            return UniTask.CompletedTask;
        }

        private void OnCombatStarted()
        {
            _cachedStateMachine?.ChangeStateAsync<CombatState>().Forget();
        }
    }
}
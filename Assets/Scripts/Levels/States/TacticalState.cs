using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Infrastructure.Signals;
using Gameplay.Interaction;
using Zenject;

namespace Gameplay.Combat.States
{
    public class TacticalState : ILevelState
    {
        private readonly SignalBus _signalBus;
        private readonly InteractionStateModel _interactionState;
        private readonly TacticalClaimSystem _claimSystem;
        private LevelStateMachine _cachedStateMachine; 

        public TacticalState(SignalBus signalBus, InteractionStateModel interactionState, TacticalClaimSystem claimSystem)
        {
            _signalBus = signalBus;
            _interactionState = interactionState;
            _claimSystem = claimSystem;
        }

        public UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {
            _cachedStateMachine = stateMachine;
            _interactionState.CurrentMode = InteractionMode.TacticalClaim;
            _interactionState.AvailableClaims = 5;
            
            _claimSystem.SetMaxClaims(5); // Передаем данные для UI
            
            // Включаем нужные экраны
            _signalBus.Fire(new SignalInteractionModeChanged { Mode = InteractionMode.TacticalClaim });
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
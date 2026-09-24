using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Infrastructure.Signals;
using Gameplay.Interaction;
using Gameplay.Levels.Data;
using Zenject;

namespace Gameplay.Levels.States
{
    public class TacticalState : ILevelState
    {
        private readonly SignalBus _signalBus;
        private readonly InteractionStateModel _interactionState;
        private readonly TacticalClaimSystem _claimSystem;
        private readonly LevelRuntimeModel _runtimeModel;
        private LevelStateMachine _cachedStateMachine; 

        public TacticalState(
            SignalBus signalBus, 
            InteractionStateModel interactionState, 
            TacticalClaimSystem claimSystem,
            LevelRuntimeModel runtimeModel) 
        {
            _signalBus = signalBus;
            _interactionState = interactionState;
            _claimSystem = claimSystem;
            _runtimeModel = runtimeModel;
        }

        public UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {
            _cachedStateMachine = stateMachine;
            _interactionState.CurrentMode = InteractionMode.TacticalClaim;
            
            _interactionState.AvailableClaims = _runtimeModel.FoundationQuota;
            _claimSystem.SetMaxClaims(_runtimeModel.FoundationQuota);
            
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
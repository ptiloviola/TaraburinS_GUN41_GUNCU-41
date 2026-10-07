using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Spawning.Services;
using Gameplay.Interaction;
using Gameplay.Infrastructure.Signals;
using Zenject;
using Gameplay.Enemies.Services;

namespace Gameplay.Levels.States
{
    public class CombatState : ILevelState
    {
        private readonly WaveStateController _waveController;
        private readonly InteractionStateModel _interactionState;
        private readonly SignalBus _signalBus;
        private readonly EnemyTrackerService _enemyTracker;
        
        private LevelStateMachine _cachedStateMachine;
        private bool _isAllWavesSpawned;

        private CancellationTokenSource _combatCts;

        public CombatState(
            WaveStateController waveController, 
            InteractionStateModel interactionState, 
            SignalBus signalBus,
            EnemyTrackerService enemyTracker)
        {
            _waveController = waveController;
            _interactionState = interactionState;
            _signalBus = signalBus;
            _enemyTracker = enemyTracker;
        }

        public UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {
            _cachedStateMachine = stateMachine;
            _isAllWavesSpawned = false;

            _combatCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            
            _interactionState.CurrentMode = InteractionMode.Normal;
            _signalBus.Fire(new SignalInteractionModeChanged { Mode = InteractionMode.Normal });
            
            _signalBus.Subscribe<SignalGameOver>(OnGameOver);
            _signalBus.Subscribe<SignalAllWavesSpawned>(OnAllWavesSpawned);
            _signalBus.Subscribe<SignalAllEnemiesCleared>(OnEnemyClearedCheck);

            _waveController.RunWavesLoopAsync(_combatCts.Token).Forget();
            
            return UniTask.CompletedTask;
        }

        public UniTask ExitAsync(CancellationToken ct)
        {
            if (_combatCts != null)
            {
                _combatCts.Cancel();
                _combatCts.Dispose();
                _combatCts = null;
            }
            
            _signalBus.TryUnsubscribe<SignalGameOver>(OnGameOver);
            _signalBus.TryUnsubscribe<SignalAllWavesSpawned>(OnAllWavesSpawned);
            _signalBus.TryUnsubscribe<SignalAllEnemiesCleared>(OnEnemyClearedCheck);
            
            _cachedStateMachine = null;
            return UniTask.CompletedTask;
        }

        private void OnGameOver()
        {
            _cachedStateMachine?.ChangeStateAsync<LevelLoseState>().Forget();
        }

        private void OnAllWavesSpawned()
        {
            _isAllWavesSpawned = true;
            CheckWinCondition();
        }

        private void OnEnemyClearedCheck()
        {
            CheckWinCondition();
        }

        private void CheckWinCondition()
        {
            if (_isAllWavesSpawned && _enemyTracker.IsMapClear)
            {
                _cachedStateMachine?.ChangeStateAsync<LevelWinState>().Forget();
            }
        }
    }
}
using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Infrastructure.Services;
using Zenject;
using Gameplay.Infrastructure.Signals;

namespace Gameplay.Levels.States
{
    public class TacticalState : ILevelState
    {
        private readonly IPauseService _pauseService;
        private readonly SignalBus _signalBus;
        
        private LevelStateMachine _cachedStateMachine; // Сохраняем временно

        // ИСПРАВЛЕНИЕ: Убрали LevelStateMachine из конструктора
        public TacticalState(IPauseService pauseService, SignalBus signalBus)
        {
            _pauseService = pauseService;
            _signalBus = signalBus;
        }

        public UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {
            _cachedStateMachine = stateMachine; // Кешируем для колбека
            
            _signalBus.Subscribe<SignalStartCombat>(OnCombatStarted);
            
            return UniTask.CompletedTask;
        }

        public UniTask ExitAsync(CancellationToken ct)
        {
            _signalBus.Unsubscribe<SignalStartCombat>(OnCombatStarted);
            _cachedStateMachine = null; // Очищаем ссылку
            return UniTask.CompletedTask;
        }

        private void OnCombatStarted()
        {
            _cachedStateMachine?.ChangeStateAsync<CombatState>().Forget();
        }
    }
}
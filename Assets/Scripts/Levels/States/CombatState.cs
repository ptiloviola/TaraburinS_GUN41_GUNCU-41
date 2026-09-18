using System.Threading;
using Cysharp.Threading.Tasks;
using Gameplay.Infrastructure.Services;
using Gameplay.Spawning.Services;

namespace Gameplay.Levels.States
{
    public class CombatState : ILevelState
    {
        private readonly IPauseService _pauseService;
        private readonly WaveStateController _waveController;

        // ИСПРАВЛЕНИЕ: Убрали LevelStateMachine из конструктора
        public CombatState(IPauseService pauseService, WaveStateController waveController)
        {
            _pauseService = pauseService;
            _waveController = waveController;
        }

        public UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {
            _waveController.RunWavesLoopAsync(ct).Forget();
            
            return UniTask.CompletedTask;
        }

        public UniTask ExitAsync(CancellationToken ct)
        {
            return UniTask.CompletedTask;
        }
    }
}
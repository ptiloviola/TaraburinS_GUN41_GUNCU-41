using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Levels;
using Gameplay.Infrastructure.Services;

namespace Gameplay.Levels.States
{
    public class LevelInitState : ILevelState
    {
        private readonly GridDataInitializer _gridInitializer;
        private readonly GridVisualBuilder _visualBuilder;
        private readonly LevelEntitySpawner _entitySpawner;
        private readonly NavMeshBakeService _navMeshBaker;
        private readonly ITimeScaleService _timeScaleService;

        public LevelInitState(
            GridDataInitializer gridInitializer, 
            GridVisualBuilder visualBuilder,
            LevelEntitySpawner entitySpawner,
            NavMeshBakeService navMeshBaker,
            ITimeScaleService timeScaleService)
        {
            _gridInitializer = gridInitializer;
            _visualBuilder = visualBuilder;
            _entitySpawner = entitySpawner;
            _navMeshBaker = navMeshBaker;
            _timeScaleService = timeScaleService;
        }

        public async UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {
            _timeScaleService.ResetSpeed();

            _gridInitializer.Initialize();
            
            _visualBuilder.Initialize();
            
            _entitySpawner.Initialize();
            
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            
            _navMeshBaker.Initialize();
            
            stateMachine.ChangeStateAsync<TacticalState>().Forget();
        }

        public UniTask ExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
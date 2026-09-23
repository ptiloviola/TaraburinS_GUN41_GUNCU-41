using System.Threading;
using Cysharp.Threading.Tasks;
using Infrastructure.Levels;

namespace Gameplay.Levels.States
{
    public class LevelInitState : ILevelState
    {
        private readonly GridDataInitializer _gridInitializer;
        private readonly GridVisualBuilder _visualBuilder;
        private readonly LevelEntitySpawner _entitySpawner;
        private readonly NavMeshBakeService _navMeshBaker;

        public LevelInitState(
            GridDataInitializer gridInitializer, 
            GridVisualBuilder visualBuilder,
            LevelEntitySpawner entitySpawner,
            NavMeshBakeService navMeshBaker)
        {
            _gridInitializer = gridInitializer;
            _visualBuilder = visualBuilder;
            _entitySpawner = entitySpawner;
            _navMeshBaker = navMeshBaker;
        }

        public async UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct)
        {

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
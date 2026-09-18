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
            // 1. Создаем математическую сетку
            _gridInitializer.Initialize();
            
            // 2. Спавним 3D-модели (кубики земли/дорог)
            _visualBuilder.Initialize();
            
            // 3. Спавним Базу и Точки появления врагов
            _entitySpawner.Initialize();
            
            // 4. Обязательно ждем 1 кадр, чтобы Unity обновила Transform и Collider у заспавненных объектов
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
            
            // 5. Запекаем NavMesh поверх готовых физических объектов
            _navMeshBaker.Initialize();
            
            // 6. Переходим в фазу тактической разметки
            stateMachine.ChangeStateAsync<TacticalState>().Forget();
        }

        public UniTask ExitAsync(CancellationToken ct) => UniTask.CompletedTask;
    }
}
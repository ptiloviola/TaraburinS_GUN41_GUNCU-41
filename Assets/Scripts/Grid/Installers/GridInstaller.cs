using UnityEngine;
using Zenject;
using Gameplay.Grid;
using Infrastructure.Levels;

namespace Gameplay.Grid.Installers
{
    public class GridInstaller : MonoInstaller
    {
        [SerializeField] private GridConfig _gridConfig;

        public override void InstallBindings()
        {
            Container.Bind<IGridService>().To<GridService>().AsSingle();

            // Ссылки на сцену и конфиг
            Container.Bind<GridSceneReferences>().FromComponentInHierarchy().AsSingle();
            Container.Bind<GridConfig>().FromInstance(_gridConfig).AsSingle();

            // Конвейер инициализации уровня
            Container.BindInterfacesTo<GridDataInitializer>().AsSingle();
            Container.BindInterfacesTo<GridVisualBuilder>().AsSingle();
            Container.BindInterfacesTo<LevelEntitySpawner>().AsSingle();
            Container.BindInterfacesTo<NavMeshBakeService>().AsSingle();

            // Жесткий порядок инициализации
            Container.BindExecutionOrder<GridDataInitializer>(-40);
            Container.BindExecutionOrder<GridVisualBuilder>(-30);
            Container.BindExecutionOrder<LevelEntitySpawner>(-20);
            Container.BindExecutionOrder<NavMeshBakeService>(-10);

            Debug.Log("<color=green>[Zenject] GridInstaller: Сетка и конвейер загрузки зарегистрированы.</color>");
        }
    }
}
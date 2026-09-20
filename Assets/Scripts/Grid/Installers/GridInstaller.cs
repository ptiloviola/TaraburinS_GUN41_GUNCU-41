using UnityEngine;
using Zenject;
using Gameplay.Grid;
using Infrastructure.Levels;

namespace Gameplay.Grid.Installers
{
    public class GridInstaller : MonoInstaller
    {


        public override void InstallBindings()
        {
            Container.Bind<IGridService>().To<GridService>().AsSingle();

            // Ссылки на сцену и конфиг
            Container.Bind<GridSceneReferences>().FromComponentInHierarchy().AsSingle();


            // Конвейер инициализации уровня
            Container.Bind<GridDataInitializer>().AsSingle();
            Container.Bind<GridVisualBuilder>().AsSingle();
            Container.Bind<LevelEntitySpawner>().AsSingle();
            Container.Bind<NavMeshBakeService>().AsSingle();


            

            Debug.Log("<color=green>[Zenject] GridInstaller: Сетка и конвейер загрузки зарегистрированы.</color>");
        }
    }
}
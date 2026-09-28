using UnityEngine;
using Zenject;
using Infrastructure.Levels;

namespace Gameplay.Grid.Installers
{
    public class GridInstaller : MonoInstaller
    {


        public override void InstallBindings()
        {
            Container.Bind<IGridService>().To<GridService>().AsSingle();

            Container.Bind<GridSceneReferences>().FromComponentInHierarchy().AsSingle();


            Container.Bind<GridDataInitializer>().AsSingle();
            Container.Bind<GridVisualBuilder>().AsSingle();
            Container.Bind<LevelEntitySpawner>().AsSingle();
            Container.Bind<NavMeshBakeService>().AsSingle();


            Gameplay.Tools.GameLogger.Log("<color=green>[Zenject] GridInstaller: Сетка и конвейер загрузки зарегистрированы.</color>");
        }
    }
}
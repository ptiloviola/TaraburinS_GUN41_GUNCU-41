using UnityEngine;
using Zenject;
using Gameplay.Towers;
using Gameplay.Towers.Data;
using Gameplay.Towers.Factories;
using Gameplay.Interaction;
using Gameplay.Projectiles.Factories;

namespace Gameplay.Towers.Installers
{
    public class TowerDefenseInstaller : MonoInstaller
    {
        [SerializeField] private TowerPlacementSystem.Settings _placementSettings;
        [SerializeField] private TowerRegistry _towerRegistry;
        [SerializeField] private LayerMask _towerLayerMask;

        public override void InstallBindings()
        {
            // Данные и настройки
            Container.BindInstance(_placementSettings).IfNotBound();
            Container.BindInstance(_towerRegistry).AsSingle();

            Container.Bind<ProjectileFactory>().AsSingle();

            // Фабрики
            Container.Bind<TowerFactory>().AsSingle();
            Container.Bind<DefenderFactory>().AsSingle(); // Дефендеры относятся к башням (Казармы)

            // Системы взаимодействия
            Container.BindInterfacesAndSelfTo<TowerPlacementSystem>().AsSingle();
            Container.BindInterfacesAndSelfTo<TowerSelectionService>()
                     .AsSingle()
                     .WithArguments(Camera.main, _towerLayerMask)
                     .NonLazy();

            Debug.Log("<color=green>[Zenject] TowerDefenseInstaller: Системы башен зарегистрированы.</color>");
        }
    }
}
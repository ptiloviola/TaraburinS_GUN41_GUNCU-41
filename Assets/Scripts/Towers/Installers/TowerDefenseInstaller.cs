using UnityEngine;
using Zenject;
using Gameplay.Towers;
using Gameplay.Towers.Data;
using Gameplay.Towers.Services;
using Gameplay.Towers.Factories;
using Gameplay.Interaction;
using Gameplay.Projectiles.Factories;

namespace Gameplay.Towers.Installers
{
    public class TowerDefenseInstaller : MonoInstaller
    {
        [SerializeField] private TowerPlacementSystem.Settings _placementSettings;
        [SerializeField] private TowerRegistry _towerRegistry;

        public override void InstallBindings()
        {
            // Данные и настройки
            Container.BindInstance(_placementSettings).IfNotBound();
            Container.BindInstance(_towerRegistry).AsSingle();

            Container.Bind<ProjectileFactory>().AsSingle();

            Container.Bind<TowerLifecycleService>().AsSingle();

            // Фабрики
            Container.Bind<TowerFactory>().AsSingle();
            Container.Bind<DefenderFactory>().AsSingle();

            Debug.Log("<color=green>[Zenject] TowerDefenseInstaller: Системы башен зарегистрированы.</color>");
        }
    }
}
using UnityEngine;
using Zenject;
using TpsShooter.Player;
using TpsShooter.Player.Inventory;
using TpsShooter.Effects;
using TpsShooter.Player.Configs;
using TpsShooter.Environment;
using TpsShooter.Enemies.Core;
using TpsShooter.UI;
using TpsShooter.UI.Presenters;

namespace TpsShooter.Infrastructure
{
    public class LevelInstaller : MonoInstaller
    {
        [Header("Configs")]
        [SerializeField] private PlayerInventoryConfig _inventoryConfig;
        [SerializeField] private PlayerConfig _playerConfig;

        public override void InstallBindings()
        {
            Container.BindInstance(_playerConfig);
            Container.BindInstance(_inventoryConfig);

            Container.BindInterfacesAndSelfTo<PlayerInventoryModel>().AsSingle();

            Container.Bind<PlayerFacade>().FromComponentInHierarchy().AsSingle().NonLazy();
                     
            Container.Bind<DecalManager>().FromComponentInHierarchy().AsSingle();

            Container.Bind<PlayerHUDView>().FromComponentInHierarchy().AsSingle();

            Container.BindInterfacesAndSelfTo<PlayerHUDPresenter>().AsSingle();
            
            Container.Bind<LootFactory>().AsSingle();

            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();
            Container.Bind<EnemyWaveSpawner>().FromComponentInHierarchy().AsSingle();
            Container.Bind<LevelFlowManager>().FromComponentInHierarchy().AsSingle().NonLazy();

            Container.Bind<ExtractionPoint>().FromComponentInHierarchy().AsSingle();
            
            Container.Bind<ExtractionUIView>().FromComponentInHierarchy().AsSingle();
            
            Container.BindInterfacesAndSelfTo<ExtractionUIPresenter>().AsSingle();

            Container.Bind<LevelFlowUIView>().FromComponentInHierarchy().AsSingle();
            
            Container.BindInterfacesAndSelfTo<LevelFlowUIPresenter>().AsSingle();
            Container.Bind<IVFXService>().To<VFXManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}
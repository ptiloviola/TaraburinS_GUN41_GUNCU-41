using UnityEngine;
using Zenject;
using TpsShooter.Player;
using TpsShooter.Player.Inventory;
using TpsShooter.Effects;
using TpsShooter.Player.Configs; // Подключаем пространство имен с конфигами
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
        
        // Примечание: Если твой PlayerConfig сейчас биндится в другом месте 
        // (например, в GameSettingsInstaller), то всё отлично. 
        // Если он нигде не биндится, его нужно добавить сюда аналогичным образом.

        public override void InstallBindings()
        {
            Container.BindInstance(_playerConfig);
            Container.BindInstance(_inventoryConfig);

            // 1. Биндим Модель Инвентаря (InterfacesAndSelfTo автоматически вызовет Initialize)
            Container.BindInterfacesAndSelfTo<PlayerInventoryModel>().AsSingle();

            // 2. Фасад
            Container.Bind<PlayerFacade>().FromComponentInHierarchy().AsSingle().NonLazy();
                     
            Container.Bind<DecalManager>().FromComponentInHierarchy().AsSingle();

            // 3. Биндим View с Канваса
            Container.Bind<PlayerHUDView>().FromComponentInHierarchy().AsSingle();

            // 4. Биндим Presenter (Zenject сам его создаст и свяжет с View и Model)
            Container.BindInterfacesAndSelfTo<PlayerHUDPresenter>().AsSingle();
            
            Container.Bind<LootFactory>().AsSingle();

            Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();
            Container.Bind<EnemyWaveSpawner>().FromComponentInHierarchy().AsSingle();
            Container.Bind<LevelFlowManager>().FromComponentInHierarchy().AsSingle().NonLazy();

            // 1. Биндим Точку Эвакуации (Модель)
            Container.Bind<ExtractionPoint>().FromComponentInHierarchy().AsSingle();
            
            // 2. Биндим UI Точки (View)
            Container.Bind<ExtractionUIView>().FromComponentInHierarchy().AsSingle();
            
            // 3. Биндим Презентер
            Container.BindInterfacesAndSelfTo<ExtractionUIPresenter>().AsSingle();

            // 4. Биндим UI перехода уровня (View)
            Container.Bind<LevelFlowUIView>().FromComponentInHierarchy().AsSingle();
            
            // 5. Биндим Презентер перехода уровня
            Container.BindInterfacesAndSelfTo<LevelFlowUIPresenter>().AsSingle();
        }
    }
}
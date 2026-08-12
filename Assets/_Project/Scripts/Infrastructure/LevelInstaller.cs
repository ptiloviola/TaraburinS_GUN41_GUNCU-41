using UnityEngine;
using Zenject;
using TpsShooter.Player;
using TpsShooter.Effects;
using TpsShooter.Player.Configs; // Подключаем пространство имен с конфигами
using TpsShooter.Environment;

namespace TpsShooter.Infrastructure
{
    public class LevelInstaller : MonoInstaller
    {
        [Header("Configs")]
        [SerializeField] private PlayerInventoryConfig _inventoryConfig;
        
        // Примечание: Если твой PlayerConfig сейчас биндится в другом месте 
        // (например, в GameSettingsInstaller), то всё отлично. 
        // Если он нигде не биндится, его нужно добавить сюда аналогичным образом.

        public override void InstallBindings()
        {
            Container.BindInstance(_inventoryConfig);

            // 1. Биндим Модель Инвентаря (InterfacesAndSelfTo автоматически вызовет Initialize)
            Container.BindInterfacesAndSelfTo<TpsShooter.Player.Inventory.PlayerInventoryModel>().AsSingle();

            // 2. Фасад
            Container.Bind<PlayerFacade>().FromComponentInHierarchy().AsSingle().NonLazy();
                     
            Container.Bind<DecalManager>().FromComponentInHierarchy().AsSingle();

            // 3. Биндим View с Канваса
            Container.Bind<TpsShooter.UI.PlayerHUDView>().FromComponentInHierarchy().AsSingle();

            // 4. Биндим Presenter (Zenject сам его создаст и свяжет с View и Model)
            Container.BindInterfacesAndSelfTo<TpsShooter.UI.Presenters.PlayerHUDPresenter>().AsSingle();
            
            Container.Bind<LootFactory>().AsSingle();
        }
    }
}
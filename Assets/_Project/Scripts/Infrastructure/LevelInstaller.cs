using UnityEngine;
using Zenject;
using TpsShooter.Player;
using TpsShooter.Effects;
using TpsShooter.Player.Configs; // Подключаем пространство имен с конфигами

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
            // 1. Биндим конфиг инвентаря
            // BindInstance берет конкретный объект из Инспектора и кладет его в контейнер
            Container.BindInstance(_inventoryConfig);

            // 2. Биндим фасад игрока
            Container.Bind<PlayerFacade>()
                     .FromComponentInHierarchy()
                     .AsSingle()
                     .NonLazy();
                     
            // 3. Биндим менеджер декалей
            Container.Bind<DecalManager>()
                     .FromComponentInHierarchy()
                     .AsSingle();
        }
    }
}
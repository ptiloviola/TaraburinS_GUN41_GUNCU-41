using MeatMushrooms.Mushroom.Components;
using MeatMushrooms.Mushroom.Configs;
using MeatMushrooms.Mushroom.Signals;
using UnityEngine;
using Zenject;

namespace MeatMushrooms.Core.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [Header("Configs")]
        [SerializeField] private MushroomConfig _mushroomConfig;
        [SerializeField] private MushroomSpawnerConfig _spawnerConfig;

        public override void InstallBindings()
        {
            // 1. Шина сигналов
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<MushroomSpawnedSignal>();
            Container.DeclareSignal<MushroomDestroyedSignal>();

            // 2. Биндим конфиги по отдельности
            Container.BindInstance(_mushroomConfig).IfNotBound();
            Container.BindInstance(_spawnerConfig).IfNotBound();

            // 3. РЕГИСТРАЦИЯ СПАВНЕРА (добавили)
            // BindInterfacesTo означает, что Zenject найдет у MushroomSpawner 
            // интерфейсы IInitializable и IDisposable и вызовет их в нужный момент.
            Container.BindInterfacesTo<MushroomSpawner>().AsSingle();
        }
    }
}
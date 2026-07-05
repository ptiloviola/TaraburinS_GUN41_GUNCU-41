using MeatMushrooms.Mushroom.Components;
using MeatMushrooms.Mushroom.Configs;
using MeatMushrooms.Mushroom.Signals;
using UnityEngine;
using Zenject;
using MeatMushrooms.Player.Components;
using MeatMushrooms.Player;
using MeatMushrooms.Environment;

namespace MeatMushrooms.Core.Installers
{
    public class GameInstaller : MonoInstaller
    {
        [Header("Configs")]
        [SerializeField] private MushroomConfig _mushroomConfig;
        [SerializeField] private MushroomSpawnerConfig _spawnerConfig;
        [SerializeField] private PlayerController _playerInstance; 
        [SerializeField] private StageConfig _stageConfig;

        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);
            Container.DeclareSignal<MushroomSpawnedSignal>();
            Container.DeclareSignal<MushroomDestroyedSignal>();

            Container.BindInstance(_mushroomConfig).IfNotBound();
            Container.BindInstance(_spawnerConfig).IfNotBound();
            Container.BindInstance(_stageConfig).IfNotBound();

            Container.BindInterfacesTo<MushroomSpawner>().AsSingle();

            Container.Bind<PlayerRegistry>().AsSingle();
        }
    }
}
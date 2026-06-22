using Zenject;
using Bowling.Gameplay; 
using Bowling.BowlingPins;
using Bowling.UI;
using Bowling.Ball;
using UnityEngine;

namespace Bowling.Installers
{
    public class GameInstaller : MonoInstaller
    {

        [SerializeField] private PhysicsConfig _physicsConfig;

        public override void InstallBindings()
        {
            Container.Bind<BowlingScoreCalculator>().AsSingle();
            Container.Bind<BowlingGameLoop>().AsSingle();
            Container.Bind<BowlingInputActions>().AsSingle();

            Container.BindInstance(_physicsConfig).AsSingle();
            Container.Bind<BaseThrowMechanic>().FromComponentsInHierarchy().AsCached();

            Container.Bind<GameStateManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PlayerController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PinDeckManager>().FromComponentInHierarchy().AsSingle();

            Container.Bind<EffectsPresenter>().FromComponentInHierarchy().AsSingle();
        }
    }
}
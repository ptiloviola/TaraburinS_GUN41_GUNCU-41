using Zenject;
using Bowling.Gameplay; 
using Bowling.BowlingPins;
using Bowling.UI;

namespace Bowling.Installers
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<BowlingScoreCalculator>().AsSingle();
            Container.Bind<BowlingGameLoop>().AsSingle();
            Container.Bind<BowlingInputActions>().AsSingle();

            Container.Bind<GameStateManager>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PlayerController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<PinDeckManager>().FromComponentInHierarchy().AsSingle();

            Container.Bind<EffectsPresenter>().FromComponentInHierarchy().AsSingle();
        }
    }
}
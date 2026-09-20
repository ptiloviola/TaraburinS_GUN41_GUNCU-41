using UnityEngine;
using Zenject;
using Gameplay.Economy;
using Gameplay.Base;
using Gameplay.Levels.States;
using Gameplay.Levels.Services;
using Gameplay.Levels.Data;

namespace Gameplay.Core.Installers
{
    public class LevelStateInstaller : MonoInstaller
    {
        [SerializeField] private TacticalForecastService.Settings _forecastSettings;
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BankService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerHealthService>().AsSingle();

            Container.BindInstance(_forecastSettings).IfNotBound();
            Container.Bind<TacticalForecastService>().AsSingle();

            Container.Bind<LevelRuntimeModel>().AsSingle();


            Container.Bind<ILevelState>().To<LevelInitState>().AsSingle();
            Container.Bind<ILevelState>().To<TacticalState>().AsSingle();
            Container.Bind<ILevelState>().To<CombatState>().AsSingle();
            Container.Bind<ILevelState>().To<LevelWinState>().AsSingle();
            Container.Bind<ILevelState>().To<LevelLoseState>().AsSingle();


            Container.BindInterfacesAndSelfTo<LevelStateMachine>().AsSingle();



            Debug.Log("<color=green>[Zenject] LevelStateInstaller: Экономика и База зарегистрированы.</color>");
        }
    }
}
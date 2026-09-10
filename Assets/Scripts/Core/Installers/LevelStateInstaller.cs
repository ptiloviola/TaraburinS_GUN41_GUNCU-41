using UnityEngine;
using Zenject;
using Gameplay.Economy;
using Gameplay.Base;
using Gameplay.UI;
using Gameplay.Infrastructure.Input;
using Gameplay.Interaction;

namespace Gameplay.Core.Installers
{
    public class LevelStateInstaller : MonoInstaller
    {
        [SerializeField] private ForecastIconView _forecastIconPrefab;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BankService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerHealthService>().AsSingle();


            Container.BindFactory<ForecastIconView, ForecastIconView.Factory>()
                     .FromComponentInNewPrefab(_forecastIconPrefab);

            // Регистрируем систему ввода, привязывая ее к интерфейсу и всем интерфейсам Zenject (IInitializable, IDisposable)
            Container.BindInterfacesTo<StandaloneInputService>().AsSingle();

            // Регистрируем модель состояния взаимодействия как Singleton
            Container.Bind<InteractionStateModel>().AsSingle();


            Debug.Log("<color=green>[Zenject] LevelStateInstaller: Экономика и UI зарегистрированы.</color>");
        }
    }
}
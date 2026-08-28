using UnityEngine;
using Zenject;
using Gameplay.Economy;
using Gameplay.Base;
using Gameplay.UI;

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


            Debug.Log("<color=green>[Zenject] LevelStateInstaller: Экономика и UI зарегистрированы.</color>");
        }
    }
}
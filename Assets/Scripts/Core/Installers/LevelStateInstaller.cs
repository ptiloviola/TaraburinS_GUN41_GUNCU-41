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

            // Потенциально проблемное место: BaseCore у нас теперь спавнится динамически! 
            // Если кто-то запросит BaseCore в конструкторе, Zenject упадет, так как базы еще нет на сцене во время фазы инжекта.
            // Пока оставляем как было, но в будущем лучше использовать BaseRegistry для поиска базы.
            Container.Bind<BaseCore>().FromComponentInHierarchy().AsSingle();

            Container.BindFactory<ForecastIconView, ForecastIconView.Factory>()
                     .FromComponentInNewPrefab(_forecastIconPrefab);

            Debug.Log("<color=green>[Zenject] LevelStateInstaller: Экономика и UI зарегистрированы.</color>");
        }
    }
}
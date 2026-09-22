using UnityEngine;
using Zenject;
using Gameplay.Economy;
using Gameplay.Base;
using Gameplay.Levels.States;
using Gameplay.Levels.Services;
using Gameplay.Levels.Data;
using Gameplay.Core.Data;
using Gameplay.Modifiers.Data;      
using Gameplay.Modifiers.Services;  

namespace Gameplay.Core.Installers
{
    public class LevelStateInstaller : MonoInstaller
    {
        [SerializeField] private TacticalForecastService.Settings _forecastSettings;
        [SerializeField] private LevelBlueprintConfig _debugFallbackBlueprint;
        
        [Header("База данных")]
        [SerializeField] private ItemRegistry _itemRegistry; 

        [Inject] private RunProgressModel _progressModel; 

        public override void InstallBindings()
        {
            LevelBlueprintConfig activeBlueprint = _progressModel.CurrentNode?.CombatLevel ?? _debugFallbackBlueprint;
            
            if (activeBlueprint == null)
            {
                Debug.LogError("[LevelStateInstaller] Критическая ошибка: Не передан LevelBlueprintConfig!");
                return;
            }


            Container.BindInstance(activeBlueprint.GridConfig).AsSingle();
            Container.BindInstance(activeBlueprint.WavesConfig).AsSingle();

            // --- РЕГИСТРАЦИЯ БАЗ ДАННЫХ И МОДИФИКАТОРОВ ---
            Container.BindInstance(_itemRegistry).AsSingle();
            Container.Bind<StatsModifierService>().AsSingle();
            
            // Регистрируем бутстраппер и ставим ему наивысший приоритет запуска
            Container.BindInterfacesTo<RunModifiersBootstrapper>().AsSingle();
            Container.BindExecutionOrder<RunModifiersBootstrapper>(-100);

            // --- БАЗОВЫЕ СИСТЕМЫ ---
            Container.BindInterfacesAndSelfTo<BankService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerHealthService>().AsSingle();
            
            Container.BindInstance(_forecastSettings).IfNotBound();
            Container.Bind<TacticalForecastService>().AsSingle();
            Container.Bind<LevelRuntimeModel>().AsSingle();

            // --- СТЕЙТ-МАШИНА ---
            Container.Bind<ILevelState>().To<LevelInitState>().AsSingle();
            Container.Bind<ILevelState>().To<TacticalState>().AsSingle();
            Container.Bind<ILevelState>().To<CombatState>().AsSingle();
            Container.Bind<ILevelState>().To<LevelWinState>().AsSingle();
            Container.Bind<ILevelState>().To<LevelLoseState>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelStateMachine>().AsSingle();

            Container.BindInterfacesAndSelfTo<RunResultProcessor>().AsSingle();
        }
    }
}
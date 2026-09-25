using UnityEngine;
using Zenject;
using Gameplay.Economy;
using Gameplay.Base;
using Gameplay.Levels.States;
using Gameplay.Levels.Services;
using Gameplay.Levels.Data;
using Gameplay.Modifiers.Data;      
using Gameplay.Modifiers.Services;  
using Gameplay.Campaign.Services;
using Gameplay.Campaign.Data;

namespace Gameplay.Levels.Installers
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
            LevelBlueprintConfig activeBlueprint = null;

            if (_progressModel.CurrentNode == null)
            {
                Debug.LogError("[LevelStateInstaller] КРИТИЧЕСКАЯ ОШИБКА: CurrentNode равен NULL! Либо ProjectInstaller перезатер прогресс, либо мы пришли не из Хаба.");
            }
            // Достаем конфигурацию из Стратегии узла
            else if (_progressModel.CurrentNode.Encounter is CombatEncounter combat)
            {
                activeBlueprint = combat.Config;
                if (activeBlueprint == null)
                {
                    Debug.LogError($"[LevelStateInstaller] КРИТИЧЕСКАЯ ОШИБКА: У узла '{_progressModel.CurrentNode.Id}' пустой конфиг боя!");
                }
            }
            else
            {
                Debug.LogError($"[LevelStateInstaller] ОШИБКА: Узел '{_progressModel.CurrentNode.Id}' не является боевым узлом (CombatEncounter)!");
            }

            // Фоллбэк, если конфиг так и не найден
            activeBlueprint ??= _debugFallbackBlueprint;
            
            if (activeBlueprint == null)
            {
                Debug.LogError("[LevelStateInstaller] Критическая ошибка: Не передан LevelBlueprintConfig и нет Fallback-конфига!");
                return;
            }

            Container.BindInstance(activeBlueprint.GridConfig).AsSingle();
            Container.BindInstance(activeBlueprint.WavesConfig).AsSingle();

            Container.BindInstance(_itemRegistry).AsSingle();
            Container.Bind<StatsModifierService>().AsSingle();
            
            Container.BindInterfacesTo<RunModifiersBootstrapper>().AsSingle();
            Container.BindExecutionOrder<RunModifiersBootstrapper>(-100);

            Container.BindInterfacesAndSelfTo<BankService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerHealthService>().AsSingle();
            
            Container.BindInstance(_forecastSettings).IfNotBound();
            Container.Bind<TacticalForecastService>().AsSingle();
            Container.Bind<LevelRuntimeModel>().AsSingle().WithArguments(activeBlueprint);

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
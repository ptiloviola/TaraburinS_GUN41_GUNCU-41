using UnityEngine;
using Zenject;
using Gameplay.Economy;
using Gameplay.Base;
using Gameplay.Levels.States;
using Gameplay.Levels.Services;
using Gameplay.Levels.Data;
using Gameplay.Core.Data;
using Gameplay.Grid;
using Gameplay.Spawning.Data;

namespace Gameplay.Core.Installers
{
    public class LevelStateInstaller : MonoInstaller
    {
        [SerializeField] private TacticalForecastService.Settings _forecastSettings;
        

        [Inject] private RunProgressModel _progressModel; 


        [SerializeField] private LevelBlueprintConfig _debugFallbackBlueprint;

        public override void InstallBindings()
        {

            LevelBlueprintConfig activeBlueprint = _progressModel.CurrentLevelBlueprint ?? _debugFallbackBlueprint;
            
            if (activeBlueprint == null)
            {
                Debug.LogError("[LevelStateInstaller] Критическая ошибка: Не передан LevelBlueprintConfig!");
                return;
            }


            _progressModel.CurrentLevelBlueprint = activeBlueprint;


            Container.BindInstance(activeBlueprint.GridConfig).AsSingle();
            Container.BindInstance(activeBlueprint.WavesConfig).AsSingle();


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

            Container.BindInterfacesAndSelfTo<RunResultProcessor>().AsSingle();

            Debug.Log($"<color=green>[Zenject] LevelStateInstaller: Уровень '{activeBlueprint.DisplayName}' успешно инициализирован из Blueprint.</color>");
        }
    }
}
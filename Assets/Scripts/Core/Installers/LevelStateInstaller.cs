using UnityEngine;
using Zenject;
using Gameplay.Economy;
using Gameplay.Base;
using Gameplay.Levels.States;
using Gameplay.Levels.Services;
using Gameplay.Levels.Data;
using Gameplay.Core.Data; // НОВОЕ
using Gameplay.Grid; // НОВОЕ
using Gameplay.Spawning.Data; // НОВОЕ

namespace Gameplay.Core.Installers
{
    public class LevelStateInstaller : MonoInstaller
    {
        [SerializeField] private TacticalForecastService.Settings _forecastSettings;
        
        // Получаем глобальную модель из ProjectContext
        [Inject] private RunProgressModel _progressModel; 

        // ВРЕМЕННО ДЛЯ ТЕСТОВ (Пока нет главного меню):
        // Если мы запускаем сцену боя напрямую из редактора, глобальная модель будет пустой.
        // Закинь сюда конфиг первого уровня в инспекторе, чтобы тест не ломался.
        [SerializeField] private LevelBlueprintConfig _debugFallbackBlueprint;

        public override void InstallBindings()
        {
            // 1. ПОДГОТОВКА ДАННЫХ
            LevelBlueprintConfig activeBlueprint = _progressModel.CurrentLevelBlueprint ?? _debugFallbackBlueprint;
            
            if (activeBlueprint == null)
            {
                Debug.LogError("[LevelStateInstaller] Критическая ошибка: Не передан LevelBlueprintConfig!");
                return;
            }

            // Кладем сам чертеж в модель, чтобы она его прочитала
            _progressModel.CurrentLevelBlueprint = activeBlueprint;

            // 2. ДИНАМИЧЕСКИЙ БИНДИНГ КОНФИГОВ УРОВНЯ
            // Мы достаем конфиги сетки и волн из чертежа и отдаем их контейнеру!
            Container.BindInstance(activeBlueprint.GridConfig).AsSingle();
            Container.BindInstance(activeBlueprint.WavesConfig).AsSingle();

            // 3. БАЗОВЫЕ СИСТЕМЫ
            Container.BindInterfacesAndSelfTo<BankService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerHealthService>().AsSingle();
            Container.BindInstance(_forecastSettings).IfNotBound();
            Container.Bind<TacticalForecastService>().AsSingle();
            Container.Bind<LevelRuntimeModel>().AsSingle();

            // 4. СТЕЙТ-МАШИНА
            Container.Bind<ILevelState>().To<LevelInitState>().AsSingle();
            Container.Bind<ILevelState>().To<TacticalState>().AsSingle();
            Container.Bind<ILevelState>().To<CombatState>().AsSingle();
            Container.Bind<ILevelState>().To<LevelWinState>().AsSingle();
            Container.Bind<ILevelState>().To<LevelLoseState>().AsSingle();
            Container.BindInterfacesAndSelfTo<LevelStateMachine>().AsSingle();

            Debug.Log($"<color=green>[Zenject] LevelStateInstaller: Уровень '{activeBlueprint.DisplayName}' успешно инициализирован из Blueprint.</color>");
        }
    }
}
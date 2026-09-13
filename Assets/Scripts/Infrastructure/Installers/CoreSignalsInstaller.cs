using UnityEngine;
using Zenject;
using Infrastructure.Signals;

namespace Gameplay.Infrastructure.Installers
{
    public class CoreSignalsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // УДАЛЕНО: SignalBusInstaller.Install(Container); <-- Шина уже есть в ProjectContext

            // Декларируем только локальные геймплейные сигналы уровня
            Container.DeclareSignal<SignalBaseDamaged>();
            Container.DeclareSignal<SignalEnemyKilled>();
            Container.DeclareSignal<SignalGameOver>();
            Container.DeclareSignal<SignalBalanceChanged>();
            Container.DeclareSignal<SignalWaveStarted>();
            Container.DeclareSignal<SignalWaveTimerUpdated>();
            Container.DeclareSignal<SignalForceStartWave>().OptionalSubscriber();
            Container.DeclareSignal<SignalWaveStateChanged>();
            
            Container.DeclareSignal<SignalEnemySpawned>();
            Container.DeclareSignal<SignalAllEnemiesCleared>();
            Container.DeclareSignal<SignalEnemyReachedBase>();
            Container.DeclareSignal<SignalWaveForecastUpdated>();

            Container.DeclareSignal<SignalSpawnEnemyRequest>();

            Debug.Log("<color=green>[Zenject] CoreSignalsInstaller: Локальные сигналы успешно зарегистрированы.</color>");
        }
    }
}
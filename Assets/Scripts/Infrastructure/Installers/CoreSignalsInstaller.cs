using UnityEngine;
using Zenject;
using Infrastructure.Signals;

namespace Infrastructure.Installers
{
    public class CoreSignalsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<SignalBaseDamaged>();
            Container.DeclareSignal<SignalEnemyKilled>();
            Container.DeclareSignal<SignalGameOver>();
            Container.DeclareSignal<SignalBalanceChanged>();
            Container.DeclareSignal<SignalWaveStarted>();
            Container.DeclareSignal<SignalWaveTimerUpdated>();
            Container.DeclareSignal<SignalForceStartWave>();
            Container.DeclareSignal<SignalWaveStateChanged>();
            
            Container.DeclareSignal<SignalEnemySpawned>();
            Container.DeclareSignal<SignalAllEnemiesCleared>();
            Container.DeclareSignal<SignalEnemyReachedBase>();
            Container.DeclareSignal<SignalWaveForecastUpdated>();

            Container.DeclareSignal<SignalSpawnEnemyRequest>();

            Debug.Log("<color=green>[Zenject] CoreSignalsInstaller: Сигналы успешно зарегистрированы.</color>");
        }
    }
}
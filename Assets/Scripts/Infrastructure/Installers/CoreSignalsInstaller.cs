using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;

namespace Gameplay.Infrastructure.Installers
{
    public class CoreSignalsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {

            Container.DeclareSignal<SignalBaseDamaged>().OptionalSubscriber();;
            Container.DeclareSignal<SignalEnemyKilled>();
            Container.DeclareSignal<SignalGameOver>();
            Container.DeclareSignal<SignalBalanceChanged>().OptionalSubscriber();;
            Container.DeclareSignal<SignalWaveStarted>();
            Container.DeclareSignal<SignalWaveTimerUpdated>();
            Container.DeclareSignal<SignalForceStartWave>().OptionalSubscriber();
            Container.DeclareSignal<SignalWaveStateChanged>();
            
            Container.DeclareSignal<SignalEnemySpawned>();
            Container.DeclareSignal<SignalAllEnemiesCleared>();
            Container.DeclareSignal<SignalEnemyReachedBase>();
            Container.DeclareSignal<SignalWaveForecastUpdated>();

            Container.DeclareSignal<SignalSpawnEnemyRequest>();

            Container.DeclareSignal<SignalStartCombat>();

            Container.DeclareSignal<SignalInteractionModeChanged>();
            Container.DeclareSignal<SignalTacticalClaimsUpdated>();

            Container.DeclareSignal<SignalAllWavesSpawned>();
            Container.DeclareSignal<SignalLevelWon>();
            Container.DeclareSignal<SignalLevelLost>();

            


            Gameplay.Tools.GameLogger.Log("<color=green>[Zenject] CoreSignalsInstaller: Локальные сигналы успешно зарегистрированы.</color>");
        }
    }
}
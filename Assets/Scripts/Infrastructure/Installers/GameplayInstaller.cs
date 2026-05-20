using UnityEngine;
using Zenject;
using Infrastructure.Signals;

namespace Infrastructure.Installers
{
    public class GameplayInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Инициализируем встроенную шину сигналов Zenject
            SignalBusInstaller.Install(Container);

            // Регистрируем наши кастомные сигналы в системе
            Container.DeclareSignal<SignalBaseDamaged>();
            Container.DeclareSignal<SignalEnemyDied>();

            Debug.Log("<color=green>[Zenject] Базовая инфраструктура и SignalBus успешно настроены!</color>");
        }
    }
}
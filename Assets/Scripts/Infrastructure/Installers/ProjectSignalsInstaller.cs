using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;

namespace Gameplay.Infrastructure.Installers
{
    public class ProjectSignalsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            // Инициализируем ШИНУ ЗДЕСЬ, на самом верхнем уровне!
            SignalBusInstaller.Install(Container);

            // Декларируем глобальные сигналы
            Container.DeclareSignal<SignalPauseStateChanged>();

            Debug.Log("<color=green>[Zenject] ProjectSignalsInstaller: Глобальная шина инициализирована.</color>");
        }
    }
}
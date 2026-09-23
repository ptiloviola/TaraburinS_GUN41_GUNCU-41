using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;

namespace Gameplay.Infrastructure.Installers
{
    public class ProjectSignalsInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {

            SignalBusInstaller.Install(Container);

            Container.DeclareSignal<SignalPauseStateChanged>();

            Debug.Log("<color=green>[Zenject] ProjectSignalsInstaller: Глобальная шина инициализирована.</color>");
        }
    }
}
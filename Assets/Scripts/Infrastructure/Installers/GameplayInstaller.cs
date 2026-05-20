using UnityEngine;
using Zenject;
using Infrastructure.Signals;
using Gameplay.Grid; // Подключаем нашу сетку

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

            // 3. Регистрируем сервис сетки
            // Bind<Интерфейс>().To<Реализация>().КакОдиночка()
            Container.Bind<IGridService>().To<GridService>().AsSingle();



            Debug.Log("<color=green>[Zenject] Базовая инфраструктура и SignalBus успешно настроены!</color>");
        }
    }
}
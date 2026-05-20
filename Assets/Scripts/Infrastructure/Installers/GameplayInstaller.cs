using UnityEngine;
using Zenject;
using Infrastructure.Signals;
using Gameplay.Grid; // Подключаем нашу сетку

namespace Infrastructure.Installers
{
    public class GameplayInstaller : MonoInstaller
    {

        // Ссылка на наш конфиг, которую мы укажем в инспекторе SceneContext
        [SerializeField] private GridConfig gridConfig;


        public override void InstallBindings()
        {
            // Инициализируем встроенную шину сигналов Zenject
            SignalBusInstaller.Install(Container);

            // Регистрируем наши кастомные сигналы в системе
            Container.DeclareSignal<SignalBaseDamaged>();
            Container.DeclareSignal<SignalEnemyDied>();

            // Регистрируем экземпляр нашего ScriptableObject в контейнере.
            // Теперь любой класс может написать [Inject] private GridConfig _config;
            Container.Bind<GridConfig>().FromInstance(gridConfig).AsSingle();

            Container.Bind<IGridService>().To<GridService>().AsSingle();

            Debug.Log("<color=green>[Zenject] Сетка и её конфигурация успешно зарегистрированы!</color>");
        
        }
    }
}
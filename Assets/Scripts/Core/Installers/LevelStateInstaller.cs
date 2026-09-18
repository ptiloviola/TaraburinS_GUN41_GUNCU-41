using UnityEngine;
using Zenject;
using Gameplay.Economy;
using Gameplay.Base;
using Gameplay.Levels.States;

namespace Gameplay.Core.Installers
{
    public class LevelStateInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BankService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerHealthService>().AsSingle();

            // Регистрируем стейты как список (чтобы StateMachine мог принять их в конструктор)
            Container.Bind<ILevelState>().To<LevelInitState>().AsSingle();
            Container.Bind<ILevelState>().To<TacticalState>().AsSingle();
            Container.Bind<ILevelState>().To<CombatState>().AsSingle();

            // Регистрируем саму машину (она единственная должна быть IInitializable)
            Container.BindInterfacesAndSelfTo<LevelStateMachine>().AsSingle();

            Debug.Log("<color=green>[Zenject] LevelStateInstaller: Экономика и База зарегистрированы.</color>");
        }
    }
}
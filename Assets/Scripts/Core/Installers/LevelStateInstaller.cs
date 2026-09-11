using UnityEngine;
using Zenject;
using Gameplay.Economy;
using Gameplay.Base;

namespace Gameplay.Core.Installers
{
    public class LevelStateInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BankService>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerHealthService>().AsSingle();

            Debug.Log("<color=green>[Zenject] LevelStateInstaller: Экономика и База зарегистрированы.</color>");
        }
    }
}
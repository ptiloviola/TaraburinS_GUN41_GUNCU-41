using UnityEngine;
using Zenject;
using Gameplay.UI.Views;
using Gameplay.UI.Presenters;

namespace Gameplay.MapScene.Installers
{
    public class MapSceneInstaller : MonoInstaller
    {
        [SerializeField] private HubUIView _hubView;
        [SerializeField] private ShopUIView _shopView;

        public override void InstallBindings()
        {
            Container.BindInstance(_hubView).IfNotBound();
            
            Container.BindInterfacesTo<HubUIPresenter>().AsSingle();

            Container.BindInstance(_shopView).IfNotBound();
            Container.BindInterfacesAndSelfTo<ShopUIPresenter>().AsSingle();
        }
    }
}
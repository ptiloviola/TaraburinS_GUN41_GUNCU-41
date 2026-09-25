using UnityEngine;
using Zenject;
using Gameplay.UI.Views;
using Gameplay.UI.Presenters;
using Gameplay.MapScene.Data;
using Gameplay.MapScene.Services;

namespace Gameplay.MapScene.Installers
{
    public class MapSceneInstaller : MonoInstaller
    {
        [SerializeField] private HubUIView _hubView;
        [SerializeField] private ShopUIView _shopView;
        
        [Header("Настройки 2D Карты")]
        [SerializeField] private MapSceneConfig _mapConfig;
        [SerializeField] private Transform _mapRoot;

        public override void InstallBindings()
        {

            Container.BindInstance(_hubView).IfNotBound();
            Container.BindInstance(_shopView).IfNotBound();
            Container.BindInterfacesAndSelfTo<ShopUIPresenter>().AsSingle();


            Container.BindInstance(_mapConfig).AsSingle();
            Container.BindInstance(_mapRoot).WithId("MapRoot");
            Container.BindInterfacesAndSelfTo<MapSceneBuilder>().AsSingle();


            Container.BindInterfacesTo<HubUIPresenter>().AsSingle();
            Container.BindInterfacesAndSelfTo<MapInteractionController>().AsSingle();
        }
    }
}
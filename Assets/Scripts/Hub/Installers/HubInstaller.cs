using UnityEngine;
using Zenject;
using Gameplay.UI.Views;
using Gameplay.UI.Presenters;

namespace Gameplay.Hub.Installers
{
    public class HubInstaller : MonoInstaller
    {
        [SerializeField] private HubUIView _hubView;

        public override void InstallBindings()
        {
            Container.BindInstance(_hubView).IfNotBound();
            
            Container.BindInterfacesTo<HubUIPresenter>().AsSingle();
        }
    }
}
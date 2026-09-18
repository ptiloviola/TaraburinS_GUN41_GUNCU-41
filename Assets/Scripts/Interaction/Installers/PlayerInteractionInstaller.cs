using UnityEngine;
using Zenject;
using Gameplay.Interaction;
using Gameplay.Cameras.Data;
using Gameplay.Cameras.Controllers;
using Gameplay.Grid.Services;
using Gameplay.Grid.Data;

namespace Infrastructure.Installers
{
    public class PlayerInteractionInstaller : MonoInstaller
    {
        [Header("Настройки камеры")]
        [SerializeField] private CameraSettingsConfig _cameraSettings;
        [SerializeField] private Camera _mainCamera; 
        
        [Header("Настройки взаимодействия")]
        [SerializeField] private LayerMask _towerLayerMask;

        [Header("Тактическая разметка (Новое)")]
        [SerializeField] private GridPlacementRulesConfig _placementRulesConfig;
        [SerializeField] private TacticalClaimSystem.Settings _tacticalSettings;

        public override void InstallBindings()
        {

            Container.Bind<InteractionStateModel>().AsSingle();

            Container.BindInterfacesAndSelfTo<TowerPlacementSystem>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<TowerSelectionService>()
                     .AsSingle()
                     .WithArguments(_towerLayerMask);


            Container.BindInstance(_cameraSettings).IfNotBound();
            Container.BindInstance(_mainCamera).IfNotBound();
            Container.BindInterfacesTo<RTSCameraController>().AsSingle();

            Container.BindInstance(_placementRulesConfig).IfNotBound(); 
            

            Container.Bind<GridValidationService>().AsSingle();
            
            Container.BindInstance(_tacticalSettings).IfNotBound();

            Container.BindInterfacesAndSelfTo<TacticalClaimSystem>().AsSingle();
        }
    }
}
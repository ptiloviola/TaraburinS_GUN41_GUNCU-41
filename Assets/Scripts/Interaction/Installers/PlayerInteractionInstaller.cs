using UnityEngine;
using Zenject;
using Gameplay.Interaction;
using Gameplay.Cameras.Data;
using Gameplay.Cameras.Controllers;

namespace Infrastructure.Installers
{
    public class PlayerInteractionInstaller : MonoInstaller
    {
        [Header("Настройки камеры")]
        [SerializeField] private CameraSettingsConfig _cameraSettings;
        [SerializeField] private Camera _mainCamera; 
        
        [Header("Настройки взаимодействия")]
        [SerializeField] private LayerMask _towerLayerMask; // Добавили маску для кликов по башням

        public override void InstallBindings()
        {
            // 1. Модель состояний (Общая шина)
            Container.Bind<InteractionStateModel>().AsSingle();

            // 2. Исполнительные системы взаимодействия
            Container.BindInterfacesAndSelfTo<TowerPlacementSystem>().AsSingle();
            
            // Прокидываем маску слоев точечно, только в этот сервис!
            Container.BindInterfacesAndSelfTo<TowerSelectionService>()
                     .AsSingle()
                     .WithArguments(_towerLayerMask);

            // 3. Контроллер камеры
            Container.BindInstance(_cameraSettings).IfNotBound();
            Container.BindInstance(_mainCamera).IfNotBound();
            Container.BindInterfacesTo<RTSCameraController>().AsSingle();
        }
    }
}
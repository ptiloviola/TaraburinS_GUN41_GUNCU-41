using Gameplay.Grid;
using UnityEngine;
using Zenject;
using Gameplay.Economy;
using Gameplay.Towers.Data;
using Gameplay.Towers.Factories;
using Gameplay.Towers.Visuals;
using System;
using UnityEngine.EventSystems;
using Gameplay.Infrastructure.Input;
using Gameplay.Grid.Services;


namespace Gameplay.Interaction
{
    public class TowerPlacementSystem : ITickable, IInitializable 
    {
        private const float MaxRaycastDistance = 100f;

        private readonly IGridService _gridService;
        private readonly GridSceneReferences _sceneReferences;
        private readonly Camera _mainCamera;
        private readonly Settings _settings;
        private readonly IInstantiator _instantiator;
        private readonly BankService _bankService;
        private readonly TowerRegistry _towerRegistry;
        private readonly TowerFactory _towerFactory;
        private readonly GridValidationService _validationService;
        
        private readonly IInputService _inputService;
        private readonly InteractionStateModel _interactionState;

        private TowerConfig _selectedTowerConfig;
        private PlacementVisualizer _visualizer;

        public event Action OnTowerDeselected;

        [System.Serializable]
        public class Settings
        {
            public LayerMask GridLayerMask;
            public GameObject ValidCursorPrefab;
            public GameObject InvalidCursorPrefab;
            public GameObject RadiusIndicatorPrefab;
            public GameObject MinRadiusIndicatorPrefab;
            public float HeightOffset = 0.05f; 
        }

        public TowerPlacementSystem(
            IGridService gridService, 
            GridSceneReferences sceneReferences,
            Settings settings,
            IInstantiator instantiator,
            BankService bankService,
            TowerRegistry towerRegistry,
            TowerFactory towerFactory,
            GridValidationService validationService,
            IInputService inputService,
            InteractionStateModel interactionState)
        {
            _gridService = gridService;
            _sceneReferences = sceneReferences;
            _settings = settings;
            _instantiator = instantiator;
            _bankService = bankService;
            _towerRegistry = towerRegistry;
            _towerFactory = towerFactory;
            _validationService = validationService;
            _inputService = inputService;
            _interactionState = interactionState;
            _mainCamera = Camera.main;
        }

        public void Initialize()
        {
            var valid = _instantiator.InstantiatePrefab(_settings.ValidCursorPrefab);
            var invalid = _instantiator.InstantiatePrefab(_settings.InvalidCursorPrefab);
            var radius = _instantiator.InstantiatePrefab(_settings.RadiusIndicatorPrefab);
            var minRadius = _instantiator.InstantiatePrefab(_settings.MinRadiusIndicatorPrefab);

            valid.SetActive(false);
            invalid.SetActive(false);
            radius.SetActive(false);
            minRadius.SetActive(false);

            _visualizer = new PlacementVisualizer(valid, invalid, radius, minRadius, _settings.HeightOffset);
        }

        public void SelectTower(string towerId)
        {
            _selectedTowerConfig = _towerRegistry.GetTowerById(towerId);
            if (_selectedTowerConfig != null)
            {
                _interactionState.CurrentMode = InteractionMode.Building;
                _visualizer.SetSelectedTower(_selectedTowerConfig);
            }
        }

        public void DeselectTower()
        {
            _selectedTowerConfig = null;
            _interactionState.CurrentMode = InteractionMode.Normal;
            
            _visualizer.Hide();
            OnTowerDeselected?.Invoke(); 
        }

        public void Tick()
        {
            if (_interactionState.CurrentMode != InteractionMode.Building)
            {
                _visualizer?.Hide();
                return;
            }
            if (_inputService.IsCancelActionDown)
            {
                if (_selectedTowerConfig != null) DeselectTower();
            }

            if (_selectedTowerConfig == null) return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                _visualizer.Hide();
                return;
            }

            HandleMouseInteraction();
        }

        private void HandleMouseInteraction()
        {
            Ray ray = _mainCamera.ScreenPointToRay(_inputService.PointerPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, MaxRaycastDistance, _settings.GridLayerMask))
            {
                int gridX = Mathf.RoundToInt(hit.transform.position.x / _sceneReferences.Spacing);
                int gridZ = Mathf.RoundToInt(hit.transform.position.z / _sceneReferences.Spacing);
                Vector2Int gridPos = new Vector2Int(gridX, gridZ);

                bool isCellFree = _validationService.CanBuildTower(gridPos);
                bool hasEnoughMoney = _bankService.CurrentBalance >= _selectedTowerConfig.BaseCost;
                bool canBuild = isCellFree && hasEnoughMoney;

                _visualizer.UpdateVisuals(hit.collider.transform.position, hit.collider.bounds.max.y, canBuild);

                if (_inputService.IsPrimaryActionDown && canBuild)
                {
                    Vector3 spawnPosition = new Vector3(hit.collider.transform.position.x, hit.collider.bounds.max.y, hit.collider.transform.position.z);
                    
                    if (_towerFactory.TryBuildTower(_selectedTowerConfig, gridPos, spawnPosition))
                    {
                        _visualizer.UpdateVisuals(hit.collider.transform.position, hit.collider.bounds.max.y, false); 
                    }
                }
            }
            else
            {
                _visualizer.Hide();
            }
        }
    }
}
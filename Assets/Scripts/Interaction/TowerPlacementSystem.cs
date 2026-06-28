using Gameplay.Grid;
using UnityEngine;
using Zenject;
using Gameplay.Economy;
using Gameplay.Towers.Data;
using Gameplay.Towers.Factories;
using Gameplay.Towers.Visuals;
using System;
using UnityEngine.EventSystems;

namespace Gameplay.Interaction
{
    public class TowerPlacementSystem : ITickable, IInitializable 
    {
        private readonly IGridService _gridService;
        private readonly GridGenerator _gridGenerator;
        private readonly Camera _mainCamera;
        private readonly Settings _settings;
        private readonly IInstantiator _instantiator;
        private readonly BankService _bankService;
        private readonly TowerRegistry _towerRegistry;
        private readonly TowerFactory _towerFactory;

        private TowerShopData _selectedTowerData;
        private PlacementVisualizer _visualizer;

        public bool IsBuildingMode => _selectedTowerData != null;

        public event Action OnTowerDeselected;

        [System.Serializable]
        public class Settings
        {
            public LayerMask GridLayerMask;
            public GameObject ValidCursorPrefab;
            public GameObject InvalidCursorPrefab;
            public GameObject RadiusIndicatorPrefab;
            public float HeightOffset = 0.05f; 
        }

        public TowerPlacementSystem(
            IGridService gridService, 
            GridGenerator gridGenerator, 
            Settings settings,
            IInstantiator instantiator,
            BankService bankService,
            TowerRegistry towerRegistry,
            TowerFactory towerFactory)
        {
            _gridService = gridService;
            _gridGenerator = gridGenerator;
            _settings = settings;
            _instantiator = instantiator;
            _bankService = bankService;
            _towerRegistry = towerRegistry;
            _towerFactory = towerFactory;
            _mainCamera = Camera.main;
        }

        public void Initialize()
        {
            // Фабрикуем объекты один раз через Zenject
            var valid = _instantiator.InstantiatePrefab(_settings.ValidCursorPrefab);
            var invalid = _instantiator.InstantiatePrefab(_settings.InvalidCursorPrefab);
            var radius = _instantiator.InstantiatePrefab(_settings.RadiusIndicatorPrefab);
            
            valid.SetActive(false);
            invalid.SetActive(false);
            radius.SetActive(false);

            // Создаем наш вспомогательный визуализатор
            _visualizer = new PlacementVisualizer(valid, invalid, radius, _settings.HeightOffset);
        }

        public void SelectTower(string towerId)
        {
            _selectedTowerData = _towerRegistry.GetTowerById(towerId);
            if (_selectedTowerData != null)
            {
                _visualizer.SetSelectedTower(_selectedTowerData);
            }
        }

        public void DeselectTower()
        {
            _selectedTowerData = null;
            _visualizer.Hide();
            OnTowerDeselected?.Invoke(); 
        }

        public void Tick()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (_selectedTowerData != null) DeselectTower();
            }

            if (_selectedTowerData == null) return;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                _visualizer.Hide();
                return;
            }

            HandleMouseInteraction();
        }

        private void HandleMouseInteraction()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _settings.GridLayerMask))
            {
                int gridX = Mathf.RoundToInt(hit.transform.position.x / _gridGenerator.Spacing);
                int gridZ = Mathf.RoundToInt(hit.transform.position.z / _gridGenerator.Spacing);
                Vector2Int gridPos = new Vector2Int(gridX, gridZ);

                bool isCellFree = _gridService.CanBuildAt(gridPos);
                bool hasEnoughMoney = _bankService.CurrentBalance >= _selectedTowerData.Cost;
                bool canBuild = isCellFree && hasEnoughMoney;

                _visualizer.UpdateVisuals(hit.collider.transform.position, hit.collider.bounds.max.y, canBuild);

                if (Input.GetMouseButtonDown(0) && canBuild)
                {
                    Vector3 spawnPosition = new Vector3(hit.collider.transform.position.x, hit.collider.bounds.max.y, hit.collider.transform.position.z);
                    
                    // Делегируем постройку фабрике!
                    if (_towerFactory.TryBuildTower(_selectedTowerData, gridPos, spawnPosition))
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
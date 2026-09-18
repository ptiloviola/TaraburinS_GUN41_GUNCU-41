using Gameplay.Grid;
using UnityEngine;
using Zenject;
using System;
using UnityEngine.EventSystems;
using Gameplay.Infrastructure.Input;
using Gameplay.Grid.Services;

namespace Gameplay.Interaction
{
    public class TacticalClaimSystem : ITickable, IInitializable 
    {
        private const float MaxRaycastDistance = 100f;

        private readonly IGridService _gridService;
        private readonly GridValidationService _validationService;
        private readonly GridSceneReferences _sceneReferences;
        private readonly Camera _mainCamera;
        private readonly Settings _settings;
        private readonly IInstantiator _instantiator;
        private readonly IInputService _inputService;
        private readonly InteractionStateModel _interactionState;

        // Отдельный простой визуализатор (можно переиспользовать класс PlacementVisualizer, 
        // просто передав null вместо радиусов)
        private GameObject _validCursor;
        private GameObject _invalidCursor;

        public event Action<int> OnClaimsCountChanged;

        [System.Serializable]
        public class Settings
        {
            public LayerMask GridLayerMask;
            public GameObject ValidCursorPrefab;
            public GameObject InvalidCursorPrefab;
            public GameObject FoundationPrefab; // Префаб бетонной плиты
            public float HeightOffset = 0.05f; 
        }

        public TacticalClaimSystem(
            IGridService gridService, 
            GridValidationService validationService,
            GridSceneReferences sceneReferences,
            Settings settings,
            IInstantiator instantiator,
            IInputService inputService,
            InteractionStateModel interactionState)
        {
            _gridService = gridService;
            _validationService = validationService;
            _sceneReferences = sceneReferences;
            _settings = settings;
            _instantiator = instantiator;
            _inputService = inputService;
            _interactionState = interactionState;
            _mainCamera = Camera.main;
        }

        public void Initialize()
        {
            _validCursor = _instantiator.InstantiatePrefab(_settings.ValidCursorPrefab);
            _invalidCursor = _instantiator.InstantiatePrefab(_settings.InvalidCursorPrefab);
            
            _validCursor.SetActive(false);
            _invalidCursor.SetActive(false);
        }

        public void Tick()
        {
            // Система РАБОТАЕТ ТОЛЬКО в тактическом режиме!
            if (_interactionState.CurrentMode != InteractionMode.TacticalClaim)
            {
                HideCursors();
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                HideCursors();
                return;
            }

            HandleClaiming();
        }

        private void HandleClaiming()
        {
            Ray ray = _mainCamera.ScreenPointToRay(_inputService.PointerPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, MaxRaycastDistance, _settings.GridLayerMask))
            {
                Vector2Int gridPos = GetGridPosition(hit.point);
                
                // Запрашиваем валидацию: можно ли тут застолбить место?
                bool canClaim = _validationService.CanClaimFoundation(gridPos) && _interactionState.AvailableClaims > 0;
                
                UpdateVisuals(hit.collider.transform.position, hit.collider.bounds.max.y, canClaim);

                if (_inputService.IsPrimaryActionDown && canClaim)
                {
                    // 1. Бронируем сетку
                    GridNode node = _gridService.GetNode(gridPos);
                    node.IsClaimed = true;
                    
                    // 2. Списываем квоту
                    _interactionState.AvailableClaims--;
                    OnClaimsCountChanged?.Invoke(_interactionState.AvailableClaims);

                    // 3. Спавним визуал фундамента
                    Vector3 spawnPos = new Vector3(hit.collider.transform.position.x, hit.collider.bounds.max.y, hit.collider.transform.position.z);
                    _instantiator.InstantiatePrefab(_settings.FoundationPrefab, spawnPos, Quaternion.identity, null);
                    
                    HideCursors();
                }
            }
            else
            {
                HideCursors();
            }
        }

        private void UpdateVisuals(Vector3 basePos, float topY, bool isValid)
        {
            Vector3 finalPos = new Vector3(basePos.x, topY + _settings.HeightOffset, basePos.z);
            
            _validCursor.transform.position = finalPos;
            _invalidCursor.transform.position = finalPos;

            _validCursor.SetActive(isValid);
            _invalidCursor.SetActive(!isValid);
        }

        private void HideCursors()
        {
            _validCursor.SetActive(false);
            _invalidCursor.SetActive(false);
        }

        private Vector2Int GetGridPosition(Vector3 hitPoint)
        {
            int gridX = Mathf.RoundToInt(hitPoint.x / _sceneReferences.Spacing);
            int gridZ = Mathf.RoundToInt(hitPoint.z / _sceneReferences.Spacing);
            return new Vector2Int(gridX, gridZ);
        }
    }
}
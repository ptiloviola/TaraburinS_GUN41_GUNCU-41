using Gameplay.Grid;
using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Input;
using Gameplay.Grid.Services;
using Gameplay.Infrastructure.Signals;
using UnityEngine.EventSystems;

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
        private readonly SignalBus _signalBus; // НОВОЕ: Для общения с UI

        private GameObject _validCursor;
        private GameObject _invalidCursor;
        
        private int _maxClaims; // Запоминаем максимум для UI

        [System.Serializable]
        public class Settings
        {
            public LayerMask GridLayerMask;
            public GameObject ValidCursorPrefab;
            public GameObject InvalidCursorPrefab;
            public GameObject FoundationPrefab; 
            public float HeightOffset = 0.05f; 
        }

        public TacticalClaimSystem(
            IGridService gridService, 
            GridValidationService validationService,
            GridSceneReferences sceneReferences,
            Settings settings,
            IInstantiator instantiator,
            IInputService inputService,
            InteractionStateModel interactionState,
            SignalBus signalBus)
        {
            _gridService = gridService;
            _validationService = validationService;
            _sceneReferences = sceneReferences;
            _settings = settings;
            _instantiator = instantiator;
            _inputService = inputService;
            _interactionState = interactionState;
            _signalBus = signalBus;
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
            HandleCanceling(); // Вызов отмены
        }

        private void HandleClaiming()
        {
            Ray ray = _mainCamera.ScreenPointToRay(_inputService.PointerPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, MaxRaycastDistance, _settings.GridLayerMask))
            {
                // ИСПРАВЛЕНО: Берем позицию строго по центру коллайдера
                Vector2Int gridPos = GetGridPosition(hit.collider.transform.position);
                
                bool canClaim = _validationService.CanClaimFoundation(gridPos) && _interactionState.AvailableClaims > 0;
                UpdateVisuals(hit.collider.transform.position, hit.collider.bounds.max.y, canClaim);

                if (_inputService.IsPrimaryActionDown && canClaim)
                {
                    GridNode node = _gridService.GetNode(gridPos);
                    node.IsClaimed = true;
                    _interactionState.AvailableClaims--;
                    
                    Vector3 spawnPos = new Vector3(hit.collider.transform.position.x, hit.collider.bounds.max.y, hit.collider.transform.position.z);
                    node.FoundationVisual = _instantiator.InstantiatePrefab(_settings.FoundationPrefab, spawnPos, Quaternion.identity, null);
                    
                    UpdateUI();
                    HideCursors();
                }
            }
            else HideCursors();
        }

        private void HandleCanceling()
        {
            // Отменяем либо через инпут, либо жестко по ПКМ
            if (_inputService.IsCancelActionDown || Input.GetMouseButtonDown(1))
            {
                Ray ray = _mainCamera.ScreenPointToRay(_inputService.PointerPosition);
                if (Physics.Raycast(ray, out RaycastHit hit, MaxRaycastDistance, _settings.GridLayerMask))
                {
                    // ИСПРАВЛЕНО: Берем позицию строго по центру коллайдера
                    Vector2Int gridPos = GetGridPosition(hit.collider.transform.position);
                    GridNode node = _gridService.GetNode(gridPos);

                    if (node != null && node.IsClaimed)
                    {
                        node.IsClaimed = false;
                        if (node.FoundationVisual != null)
                        {
                            GameObject.Destroy(node.FoundationVisual);
                            node.FoundationVisual = null;
                        }
                        
                        _interactionState.AvailableClaims++;
                        UpdateUI();
                    }
                }
            }
        }

        // Вызывается из стейта при старте фазы
        public void SetMaxClaims(int max)
        {
            _maxClaims = max;
            UpdateUI();
        }

        private void UpdateUI()
        {
            _signalBus.Fire(new SignalTacticalClaimsUpdated { Available = _interactionState.AvailableClaims, Max = _maxClaims });
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

        private Vector2Int GetGridPosition(Vector3 position)
        {
            return new Vector2Int(Mathf.RoundToInt(position.x / _sceneReferences.Spacing), Mathf.RoundToInt(position.z / _sceneReferences.Spacing));
        }
    }
}
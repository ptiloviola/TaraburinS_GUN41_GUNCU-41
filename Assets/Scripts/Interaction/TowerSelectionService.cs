using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Zenject;
using Gameplay.Towers;
using Gameplay.Infrastructure.Input; // НОВОЕ

namespace Gameplay.Interaction
{
    public class TowerSelectionService : ITickable
    {
        private const float MaxRaycastDistance = 100f; // Избавляемся от магических чисел

        private readonly Camera _mainCamera;
        private readonly LayerMask _towerLayerMask;

        private readonly IInputService _inputService;
        private readonly InteractionStateModel _interactionState;

        public event Action<TowerFacade> OnTowerSelected;
        public event Action OnTowerDeselected;

        public TowerFacade CurrentSelectedTower { get; private set; }

        public TowerSelectionService(
            Camera mainCamera, 
            LayerMask towerLayerMask,
            IInputService inputService,
            InteractionStateModel interactionState)
        {
            _mainCamera = mainCamera;
            _towerLayerMask = towerLayerMask;
            _inputService = inputService;
            _interactionState = interactionState;
        }

        public void Tick()
        {
            // 1. ИДЕАЛЬНАЯ РАЗВЯЗКА: Мы проверяем только стейт модели, не трогая систему постройки
            if (_interactionState.CurrentMode == InteractionMode.Building) return;

            // 2. Отмена выбора (ПКМ/Esc)
            if (_inputService.IsCancelActionDown)
            {
                Deselect();
            }

            // 3. Выделение (ЛКМ/Тап)
            if (_inputService.IsPrimaryActionDown)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;
                    
                HandleSelectionClick();
            }
        }

        private void HandleSelectionClick()
        {
            Ray ray = _mainCamera.ScreenPointToRay(_inputService.PointerPosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, MaxRaycastDistance, _towerLayerMask))
            {
                // ОПТИМИЗАЦИЯ: GetComponentInParent проверяет и сам объект, и всех родителей за один вызов
                TowerFacade clickedTower = hit.collider.GetComponentInParent<TowerFacade>();
                
                if (clickedTower != null)
                {
                    Select(clickedTower);
                }
            }
            else
            {
                Deselect();
            }
        }

        public void Select(TowerFacade tower)
        {
            if (CurrentSelectedTower == tower) return; 

            if (CurrentSelectedTower != null)
            {
                Deselect(); 
            }

            CurrentSelectedTower = tower;
            OnTowerSelected?.Invoke(tower);
            
#if UNITY_EDITOR
            Debug.Log($"<color=orange>[SelectionService] Выделена построенная башня: {tower.Config.DisplayName} (Уровень {tower.CurrentLevel})</color>");
#endif
        }

        public void Deselect()
        {
            if (CurrentSelectedTower != null)
            {
                CurrentSelectedTower = null;
                OnTowerDeselected?.Invoke();
                
#if UNITY_EDITOR
                Debug.Log("<color=orange>[SelectionService] Башня снята с выделения.</color>");
#endif
            }
        }
    }
}
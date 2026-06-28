using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Zenject;
using Gameplay.Towers;

namespace Gameplay.Interaction
{
    public class TowerSelectionService : ITickable
    {
        private readonly Camera _mainCamera;
        private readonly LayerMask _towerLayerMask;// Слой, на котором лежат башни

        // НОВОЕ: Ссылка на систему постройки
        private readonly TowerPlacementSystem _placementSystem;

        // События для UI контекстного меню
        public event Action<TowerFacade> OnTowerSelected;
        public event Action OnTowerDeselected;

        public TowerFacade CurrentSelectedTower { get; private set; }

        public TowerSelectionService(Camera mainCamera, LayerMask towerLayerMask,
            TowerPlacementSystem placementSystem)
        {
            _mainCamera = mainCamera;
            _towerLayerMask = towerLayerMask;
            _placementSystem = placementSystem;
        }

        
        public void Tick()
        {
            // НОВОЕ: ГЛАВНАЯ ЗАЩИТА! 
            // Если игрок сейчас держит в руках башню для постройки — мы вообще не пытаемся никого выделять.
            if (_placementSystem.IsBuildingMode) return;
            // Сброс выбора по ПКМ или Esc
            if (Input.GetMouseButton(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                Deselect();
            }
            // Если кликнули ЛКМ
            if (Input.GetMouseButtonDown(0))
            {
                // Защита от кликов сквозь UI
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;
                HandleSelectionClick();
            }


        }

        private void HandleSelectionClick()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _towerLayerMask))
            {
                if (hit.collider.TryGetComponent(out TowerFacade clickedTower) ||
                hit.collider.GetComponentInParent<TowerFacade>() != null)
                {
                    TowerFacade foundTower = clickedTower != null 
                    ? clickedTower : hit.collider.GetComponentInParent<TowerFacade>();
                    Select(foundTower);
                }
            }
            else
            {
                // Теперь мы безопасно сбрасываем выбор, зная, что мы точно не в режиме постройки
                Deselect();
            }
        }

        public void Select(TowerFacade tower)
        {
            if (CurrentSelectedTower == tower) return; // Уже выделена

            // НОВОЕ: Если мы выбрали новую башню, но старая еще в фокусе — сбрасываем старую!
            if (CurrentSelectedTower != null)
            {
                Deselect(); 
            }

            CurrentSelectedTower = tower;
            OnTowerSelected?.Invoke(tower);
            Debug.Log($"<color=orange>[SelectionService] Выделена построенная башня: {tower.Config.DisplayName} (Уровень {tower.CurrentLevel})</color>");
        }

        public void Deselect()
        {
            if (CurrentSelectedTower != null)
            {
                CurrentSelectedTower = null;
                OnTowerDeselected?.Invoke();
                Debug.Log("<color=orange>[SelectionService] Башня снята с выделения.</color>");
            }
        }

    }

}

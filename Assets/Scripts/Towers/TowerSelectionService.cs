using UnityEngine;
using System;
using UnityEngine.EventSystems;
using Zenject;

namespace Gameplay.Towers
{
    public class TowerSelectionService : ITickable
    {
        private readonly Camera _mainCamera;
        private readonly LayerMask _towerLayerMask;// Слой, на котором лежат башни

        // События для UI контекстного меню
        public event Action<TowerFacade> OnTowerSelected;
        public event Action OnTowerDeselected;

        public TowerFacade CurrentSelectedTower { get; private set; }

        public TowerSelectionService(Camera mainCamera, LayerMask towerLayerMask)
        {
            _mainCamera = mainCamera;
            _towerLayerMask = towerLayerMask;
        }

        
        public void Tick()
        {
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
            // Пускаем луч, ищем только объекты на слое башен
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _towerLayerMask))
            {
                // Если попали в объект башни
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
                // Если кликнули мимо (в землю) - сбрасываем выбор
                // Но только если мы не находимся в режиме постройки (это мы свяжем позже)
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

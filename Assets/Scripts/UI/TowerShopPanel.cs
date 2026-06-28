using UnityEngine;
using Zenject;
using Gameplay.Towers.Data;
using Gameplay.Towers; // Для доступа к TowerPlacementSystem
using System.Collections.Generic;
using TMPro; 
using Gameplay.Towers.Data.Modules;
using Gameplay.Interaction;

namespace Gameplay.UI
{
    public class TowerShopPanel : MonoBehaviour
    {
        [Header("Настройки UI")]
        [SerializeField] private TowerButtonView _buttonPrefab;
        [SerializeField] private Transform _buttonsContainer; 

        [Header("Настройки Тултипа")]
        [SerializeField] private GameObject _tooltipPanel; 
        [SerializeField] private TMP_Text _tooltipNameText; 
        [SerializeField] private TMP_Text _tooltipStatsText; 

        private TowerRegistry _towerRegistry;
        
        // НОВОЕ: Теперь мы зависим от чистой системы постройки
        private TowerPlacementSystem _placementSystem;

        private List<TowerButtonView> _spawnedButtons = new List<TowerButtonView>();
        private string _currentSelectedId = null;

        [Inject]
        public void Construct(TowerRegistry towerRegistry, TowerPlacementSystem placementSystem)
        {
            _towerRegistry = towerRegistry;
            _placementSystem = placementSystem; // Сохраняем новую систему
        }

        private void Start()
        {
            GenerateShopButtons();
            
            // Подписываемся на новую систему
            _placementSystem.OnTowerDeselected += HandleDeselectFromGrid;

            if (_tooltipPanel != null) _tooltipPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_placementSystem != null)
            {
                _placementSystem.OnTowerDeselected -= HandleDeselectFromGrid;
            }
        }

        private void GenerateShopButtons()
        {
            if (_towerRegistry == null || _towerRegistry.Towers.Count == 0)
            {
                Debug.LogWarning("[TowerShopPanel] Каталог башен пуст или не найден!");
                return;
            }

            foreach (TowerShopData towerData in _towerRegistry.Towers)
            {
                TowerButtonView newButton = Instantiate(_buttonPrefab, _buttonsContainer);
                newButton.Init(towerData, OnTowerButtonClicked, OnButtonHoverEnter, OnButtonHoverExit);
                _spawnedButtons.Add(newButton); 
                newButton.transform.SetSiblingIndex(_buttonsContainer.childCount - 2);
            }
            
            Debug.Log($"<color=cyan>[TowerShopPanel] Сгенерировано {_towerRegistry.Towers.Count} кнопок магазина.</color>");
        }

        private void HandleDeselectFromGrid()
        {
            _currentSelectedId = null;
            
            foreach (var btn in _spawnedButtons)
            {
                btn.SetSelected(false);
            }
        }

        private void OnTowerButtonClicked(string clickedTowerId)
        {
            if (_currentSelectedId == clickedTowerId)
            {
                // Используем новую систему
                _placementSystem.DeselectTower();
                return;
            }

            _currentSelectedId = clickedTowerId;
            
            Debug.Log($"<color=yellow>[TowerShopPanel] Игрок выбрал башню: {clickedTowerId}</color>");
            
            // Используем новую систему
            _placementSystem.SelectTower(clickedTowerId);
            
            foreach (var btn in _spawnedButtons)
            {
                btn.SetSelected(btn.TowerId == clickedTowerId); 
            }
        }

        private void OnButtonHoverEnter(string hoveredTowerId)
        {
            if (_tooltipPanel == null) return;
            TowerShopData data = _towerRegistry.GetTowerById(hoveredTowerId);
            if (data != null && data.TowerConfig != null && data.TowerConfig.Levels.Count > 0)
            {
                TowerLevelData baseLevel = data.TowerConfig.Levels[0];
                _tooltipNameText.text = data.TowerConfig.DisplayName;
                string statsStr = "";
                
                foreach (IModuleDescriptor module in baseLevel.GetActiveModules())
                {
                    statsStr += module.GetStatsDescription();
                }

                _tooltipStatsText.text = statsStr.Trim();
                _tooltipPanel.SetActive(true);
            }
        }

        private void OnButtonHoverExit()
        {
            if (_tooltipPanel != null) _tooltipPanel.SetActive(false);
        }
    }
}
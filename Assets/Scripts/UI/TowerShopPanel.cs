using UnityEngine;
using Zenject;
using Gameplay.Towers.Data;
using Gameplay.Towers; 
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
        private TowerPlacementSystem _placementSystem;

        private List<TowerButtonView> _spawnedButtons = new List<TowerButtonView>();
        private string _currentSelectedId = null;

        [Inject]
        public void Construct(TowerRegistry towerRegistry, TowerPlacementSystem placementSystem)
        {
            _towerRegistry = towerRegistry;
            _placementSystem = placementSystem; 
        }

        private void Start()
        {
            GenerateShopButtons();
            
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

            // ИЗМЕНЕНО: Перебираем TowerConfig
            foreach (TowerConfig config in _towerRegistry.Towers)
            {
                TowerButtonView newButton = Instantiate(_buttonPrefab, _buttonsContainer);
                newButton.Init(config, OnTowerButtonClicked, OnButtonHoverEnter, OnButtonHoverExit);
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
                _placementSystem.DeselectTower();
                return;
            }

            _currentSelectedId = clickedTowerId;
            
            Debug.Log($"<color=yellow>[TowerShopPanel] Игрок выбрал башню: {clickedTowerId}</color>");
            
            _placementSystem.SelectTower(clickedTowerId);
            
            foreach (var btn in _spawnedButtons)
            {
                btn.SetSelected(btn.TowerId == clickedTowerId); 
            }
        }

        private void OnButtonHoverEnter(string hoveredTowerId)
        {
            if (_tooltipPanel == null) return;
            
            // ИЗМЕНЕНО: Сразу достаем TowerConfig
            TowerConfig config = _towerRegistry.GetTowerById(hoveredTowerId);
            
            if (config != null && config.Levels != null && config.Levels.Count > 0)
            {
                TowerLevelData baseLevel = config.Levels[0];
                _tooltipNameText.text = config.DisplayName;
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
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Gameplay.UI.Views
{
    public class HubUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nextLevelTitleText;
        [SerializeField] private TextMeshProUGUI _runGoldText;
        
        [Header("Всплывающие кнопки (Модальные)")]
        [SerializeField] private Button _startBattleButton;
        
        [Header("HUD Кнопки (Постоянные)")]
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _abandonRunButton;

        public event Action OnStartBattleClicked;
        public event Action OnMainMenuClicked;
        public event Action OnAbandonRunClicked;

        private void Awake()
        {
            _startBattleButton.onClick.AddListener(() => OnStartBattleClicked?.Invoke());
            _mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());
            if (_abandonRunButton != null) _abandonRunButton.onClick.AddListener(() => OnAbandonRunClicked?.Invoke());

            _nextLevelTitleText.gameObject.SetActive(false);
            _startBattleButton.gameObject.SetActive(false);
            
        }

        private void OnDestroy()
        {
            _startBattleButton.onClick.RemoveAllListeners();
            _mainMenuButton.onClick.RemoveAllListeners();
            if (_abandonRunButton != null) _abandonRunButton.onClick.RemoveAllListeners();
        }

        public void ShowNextLevelInfo(string levelName)
        {
            _nextLevelTitleText.gameObject.SetActive(true);
            _nextLevelTitleText.text = $"NEXT FIGHT:\n{levelName}";
            _startBattleButton.gameObject.SetActive(true);
        }

        public void ShowCampaignCompleted()
        {
            gameObject.SetActive(true);
            _nextLevelTitleText.gameObject.SetActive(true);
            _nextLevelTitleText.text = "CAMPAIGN COMPLETED!";
            
            _startBattleButton.gameObject.SetActive(false);
            _mainMenuButton.gameObject.SetActive(true);
            
            if (_abandonRunButton != null) _abandonRunButton.gameObject.SetActive(false);
        }

        public void SetInteractable(bool interactable)
        {
            _startBattleButton.interactable = interactable;
            _mainMenuButton.interactable = interactable;
            if (_abandonRunButton != null) _abandonRunButton.interactable = interactable;
        }

        public void UpdateRunInventory(int currentGold)
        {
            if (_runGoldText != null)
            {
                _runGoldText.text = $"GOLD: {currentGold}";
            }
        }
    }
}
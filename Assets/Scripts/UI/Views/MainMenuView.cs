using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Gameplay.UI.Views
{
    public class MainMenuView : MonoBehaviour
    {
        [Header("Кнопки")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _loadSaveButton;
        [SerializeField] private Button _settingsButton;

        [Header("Тексты Статистики")]
        [SerializeField] private TextMeshProUGUI _runsCountText;
        [SerializeField] private TextMeshProUGUI _maxLevelText;

        public event Action OnPlayClicked;
        public event Action OnLoadSaveClicked;
        public event Action OnSettingsClicked;

        private void Awake()
        {
            _playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
            _loadSaveButton.onClick.AddListener(() => OnLoadSaveClicked?.Invoke());
            _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveAllListeners();
            _loadSaveButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
        }

        public void SetInteractable(bool isInteractable)
        {
            if (_playButton != null) _playButton.interactable = isInteractable;
            if (_loadSaveButton != null) _loadSaveButton.interactable = isInteractable;
            if (_settingsButton != null) _settingsButton.interactable = isInteractable;
        }

        public void UpdateStatsDisplay(int runsPlayed, int maxLevel)
        {
            if (_runsCountText != null) 
                _runsCountText.text = $"RUNS PLAYED: {runsPlayed}";
                
            if (_maxLevelText != null) 
                _maxLevelText.text = $"RECORD (LEVEL): {maxLevel}";
        }
    }
}
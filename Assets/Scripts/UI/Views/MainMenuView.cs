using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Views
{
    public class MainMenuView : MonoBehaviour
    {
        [Header("Кнопки")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _loadSaveButton;
        [SerializeField] private Button _settingsButton;

        // События, на которые подпишется Presenter
        public event Action OnPlayClicked;
        public event Action OnLoadSaveClicked;
        public event Action OnSettingsClicked;

        private void Awake()
        {
            // Транслируем Unity-события в чистые C# Actions
            _playButton.onClick.AddListener(() => OnPlayClicked?.Invoke());
            _loadSaveButton.onClick.AddListener(() => OnLoadSaveClicked?.Invoke());
            _settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
        }

        private void OnDestroy()
        {
            // Обязательная отписка для предотвращения утечек
            _playButton.onClick.RemoveAllListeners();
            _loadSaveButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
        }

        // Метод для блокировки интерфейса во время загрузки сцены
        public void SetInteractable(bool isInteractable)
        {
            if (_playButton != null) _playButton.interactable = isInteractable;
            if (_loadSaveButton != null) _loadSaveButton.interactable = isInteractable;
            if (_settingsButton != null) _settingsButton.interactable = isInteractable;
        }
    }
}
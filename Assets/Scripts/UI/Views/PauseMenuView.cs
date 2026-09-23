using System;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Views
{
    public class PauseMenuView : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;

        public event Action OnResumeClicked;
        public event Action OnMainMenuClicked;

        private void Awake()
        {
            _resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
            _mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());
        }

        private void OnDestroy()
        {
            _resumeButton.onClick.RemoveAllListeners();
            _mainMenuButton.onClick.RemoveAllListeners();
        }

        public void Show() => _panel.SetActive(true);
        public void Hide() => _panel.SetActive(false);
    }
}
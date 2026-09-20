using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace Gameplay.UI.Views
{
    public class EndGameUIView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Button _actionButton;

        public event Action OnActionClicked;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            _actionButton.onClick.AddListener(() => OnActionClicked?.Invoke());
        }

        public void Show(string title, string buttonLabel)
        {
            _titleText.text = title;
            _buttonText.text = buttonLabel;
            
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnDestroy() => _actionButton.onClick.RemoveAllListeners();
    }
}
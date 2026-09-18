using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Gameplay.UI.Views
{
    public class TacticalUIView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private TextMeshProUGUI _claimsText;
        [SerializeField] private Button _startCombatButton;

        public event Action OnStartCombatClicked;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            _startCombatButton.onClick.AddListener(() => OnStartCombatClicked?.Invoke());
        }

        public void Show()
        {
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

        public void UpdateClaimsText(int available, int max)
        {
            _claimsText.text = $"Доступно фундаментов: {available} / {max}";
        }

        private void OnDestroy()
        {
            _startCombatButton.onClick.RemoveAllListeners();
        }
    }
}
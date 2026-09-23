using UnityEngine;
using TMPro;
using System;

namespace Gameplay.UI.Views
{
    public class TowerShopView : MonoBehaviour
    {
        [Header("Контейнеры")]
        [SerializeField] private Transform _buttonsContainer;

        [Header("Тултип")]
        [SerializeField] private GameObject _tooltipPanel;
        [SerializeField] private TMP_Text _tooltipNameText;
        [SerializeField] private TMP_Text _tooltipStatsText;

        [Header("Блокировка UI")]
        [SerializeField] private CanvasGroup _canvasGroup;

        public Transform ButtonsContainer => _buttonsContainer;

        public event Action<string> OnTowerClicked;
        public event Action<string> OnTowerHoverEntered;
        public event Action OnTowerHoverExited;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void ShowTooltip(string towerName, string stats)
        {
            _tooltipNameText.text = towerName;
            _tooltipStatsText.text = stats;
            _tooltipPanel.SetActive(true);
        }

        public void HideTooltip()
        {
            _tooltipPanel.SetActive(false);
        }

        public void SetInteractable(bool isInteractable)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = isInteractable;
                _canvasGroup.blocksRaycasts = isInteractable; 
            }
        }

        public void HandleClick(string id) => OnTowerClicked?.Invoke(id);
        public void HandleHoverEnter(string id) => OnTowerHoverEntered?.Invoke(id);
        public void HandleHoverExit() => OnTowerHoverExited?.Invoke();
    }
}
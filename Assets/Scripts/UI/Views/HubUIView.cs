using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Gameplay.UI.Views
{
    public class HubUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nextLevelTitleText;
        [SerializeField] private Button _startBattleButton;
        [SerializeField] private Button _mainMenuButton;

        public event Action OnStartBattleClicked;
        public event Action OnMainMenuClicked;

        private void Awake()
        {
            _startBattleButton.onClick.AddListener(() => OnStartBattleClicked?.Invoke());
            _mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());
        }

        private void OnDestroy()
        {
            _startBattleButton.onClick.RemoveAllListeners();
            _mainMenuButton.onClick.RemoveAllListeners();
        }

        public void ShowNextLevelInfo(string levelName)
        {
            _nextLevelTitleText.text = $"NEXT FIGHT:\n{levelName}";
            _startBattleButton.gameObject.SetActive(true);
        }

        public void ShowCampaignCompleted()
        {
            _nextLevelTitleText.text = "CAMPAIGN COMPLETED!";
            _startBattleButton.gameObject.SetActive(false);
        }

        public void SetInteractable(bool interactable)
        {
            _startBattleButton.interactable = interactable;
            _mainMenuButton.interactable = interactable;
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Bowling.Ball;

namespace Bowling.UI
{
    [System.Serializable]
    public class BallConfig
    {
        public string Name;
        public Sprite Icon;         
        public GameObject BallPrefab;
        public float Mass = 10f;
    }

    public class BallSelectorUI : MonoBehaviour
    {
        [SerializeField] private BallController _ballController;
        
        [Header("База шаров")]
        [SerializeField] private BallConfig[] _availableBalls;

        [Header("Ссылки на UI Кнопки")]
        [SerializeField] private Button[] _ballButtons; 
    
        [SerializeField] private Image[] _buttonIcons;
        [SerializeField] private TMP_Text[] _buttonTexts;

        private void Start()
        {
            for (int i = 0; i < _ballButtons.Length; i++)
            {
                if (i >= _availableBalls.Length) break;

                BallConfig config = _availableBalls[i];
                int index = i;
                if (_buttonIcons[i] != null) _buttonIcons[i].sprite = config.Icon;
                if (_buttonTexts[i] != null) _buttonTexts[i].text = $"{config.Name}\n{config.Mass} кг";
                _ballButtons[i].onClick.AddListener(() => OnBallButtonClicked(index));
            }
            if (_availableBalls.Length > 0)
            {
                OnBallButtonClicked(0); 
            }
        }

        private void OnBallButtonClicked(int index)
        {
            BallConfig selectedConfig = _availableBalls[index];
            
            if (selectedConfig.BallPrefab != null)
            {
                _ballController.ApplyBallPrefab(selectedConfig.BallPrefab, selectedConfig.Mass);
            }
            
            Debug.Log($"Выбран префаб шара: {selectedConfig.Name}");
        }
    }
}
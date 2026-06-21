using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Bowling.UI.MVP
{
    public class BallSelectorView : MonoBehaviour
    {
        [Header("Ссылки на UI элементы")]
        [SerializeField] private Button[] _ballButtons; 
        [SerializeField] private Image[] _buttonIcons;
        [SerializeField] private TMP_Text[] _buttonTexts;


        public event Action<int> OnBallClicked;

        public void SetupView(BallConfig[] balls)
        {
            for (int i = 0; i < _ballButtons.Length; i++)
            {
                if (i >= balls.Length)
                {
                    _ballButtons[i].gameObject.SetActive(false);
                    continue;
                }

                int index = i;
                BallConfig config = balls[i];

                if (_buttonIcons[i] != null) _buttonIcons[i].sprite = config.Icon;
                if (_buttonTexts[i] != null) _buttonTexts[i].text = $"{config.Name}\n{config.Mass} кг";

                _ballButtons[i].onClick.RemoveAllListeners();
                _ballButtons[i].onClick.AddListener(() => 
                {

                    OnBallClicked?.Invoke(index);
                });
            }
        }
    }
}

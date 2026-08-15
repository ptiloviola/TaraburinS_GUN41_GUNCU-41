using UnityEngine;
using TMPro;

namespace TpsShooter.UI
{
    public class ExtractionUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private float _warningTimeThreshold = 5f;

        public void Show() => gameObject.SetActive(true);
        public void Hide() => gameObject.SetActive(false);

        public void UpdateTime(float timeRemaining)
        {
            if (_timerText == null) return;

            _timerText.text = $"ЭВАКУАЦИЯ: {Mathf.CeilToInt(timeRemaining)}";
            _timerText.color = timeRemaining <= _warningTimeThreshold ? _warningColor : _normalColor;
        }
    }
}
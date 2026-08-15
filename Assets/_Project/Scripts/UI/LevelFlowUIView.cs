using UnityEngine;
using TMPro;
using DG.Tweening;

namespace TpsShooter.UI
{
    public class LevelFlowUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _transitionText;
        [SerializeField] private CanvasGroup _canvasGroup; // Используем для плавного появления

        public void ShowVictoryMessage(int nextLevel)
        {
            gameObject.SetActive(true);
            
            if (_transitionText != null)
            {
                _transitionText.text = $"ЭВАКУАЦИЯ УСПЕШНА!\n<size=70%>ПЕРЕХОД НА УРОВЕНЬ {nextLevel}</size>";
            }

            // Плавное проявление текста через DOTween
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.DOFade(1f, 0.5f);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _canvasGroup?.DOKill();
        }
    }
}
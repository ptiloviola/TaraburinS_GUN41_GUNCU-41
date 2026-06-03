using UnityEngine;
using TMPro; // Для TextMeshPro
using Zenject;
using Infrastructure.Signals;

namespace Gameplay.UI
{
    public class BaseUI : MonoBehaviour
    {
        private TMP_Text _textComponent;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            // Подписываемся на радиочастоту "Урон по базе"
            _signalBus.Subscribe<SignalBaseDamaged>(UpdateLivesText);
        }

        private void OnDisable()
        {
            // ОБЯЗАТЕЛЬНО отписываемся, чтобы избежать ошибок утечки памяти
            _signalBus.Unsubscribe<SignalBaseDamaged>(UpdateLivesText);
        }

        // Этот метод сработает АВТОМАТИЧЕСКИ, когда кто-то крикнет в эфир
        private void UpdateLivesText(SignalBaseDamaged signal)
        {
            Debug.Log($"<color=yellow>[BaseUI] Услышал сигнал! Обновляю текст на экране.</color>");
            _textComponent.text = $"Жизни: {signal.CurrentLives}";
        }
    }
}

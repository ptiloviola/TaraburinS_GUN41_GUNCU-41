using UnityEngine;
using TMPro; // Для TextMeshPro
using Zenject;
using Infrastructure.Signals;

namespace Gameplay.UI
{
    public class BaseUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _textLivesComponent;
        [SerializeField]
        private TMP_Text _textBalanceComponent;
        

        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void OnEnable()
        {
            // Подписываемся на радиочастоту "Урон по базе"
            _signalBus.Subscribe<SignalBaseDamaged>(UpdateLivesText);
            _signalBus.Subscribe<SignalBalanceChanged>(UpdateBalanceText);
        }

        private void OnDisable()
        {
            // ОБЯЗАТЕЛЬНО отписываемся, чтобы избежать ошибок утечки памяти
            _signalBus?.Unsubscribe<SignalBaseDamaged>(UpdateLivesText);
            _signalBus?.Unsubscribe<SignalBalanceChanged>(UpdateBalanceText);
        }

        // Этот метод сработает АВТОМАТИЧЕСКИ, когда кто-то крикнет в эфир
        private void UpdateLivesText(SignalBaseDamaged signal)
        {
            Debug.Log($"<color=yellow>[BaseUI] Услышал сигнал! Обновляю текст(жизни) на экране.</color>");
            _textLivesComponent.text = $"Жизни: {signal.CurrentLives}";
        }

        private void UpdateBalanceText(SignalBalanceChanged signal)
        {
            Debug.Log($"<color=yellow>[BaseUI] Услышал сигнал! Обновляю текст(баланс) на экране.</color>");
            _textBalanceComponent.text = $"Баланс: {signal.CurrentBalance}$";
        }
    }
}

using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;

namespace Gameplay.Tools
{
    // Временный класс-помощник для тестов фазы тактики
    public class DebugCombatStarter : MonoBehaviour
    {
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Update()
        {
            // Нажимаем Пробел, чтобы дать команду к началу боя
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("<color=orange>[Debug] Отправлен сигнал SignalStartCombat!</color>");
                _signalBus.Fire<SignalStartCombat>();
            }
        }
    }
}
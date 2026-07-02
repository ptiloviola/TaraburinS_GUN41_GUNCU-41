using UnityEngine;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfAnimator : MonoBehaviour
    {
        private Animator _animator;
        private WolfLocomotion _locomotion;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int EatTriggerHash = Animator.StringToHash("EatStart");
        private static readonly int StopEatTriggerHash = Animator.StringToHash("EatStop");

        private static readonly int SnapTriggerHash = Animator.StringToHash("Aggro");

        // Таймер для спама логами, чтобы не забить консоль
        private float _logTimer;

        private void Awake()
        {
            // Ищем Аниматор в дочерних объектах
            _animator = GetComponentInChildren<Animator>();
            _locomotion = GetComponent<WolfLocomotion>(); 

            // ДАТЧИК 1: Нашли ли мы Аниматор?
            if (_animator == null)
            {
                Debug.LogError("[WolfAnimator] ОШИБКА: Компонент Animator не найден в дочерних объектах!");
            }
            else
            {
                Debug.Log($"[WolfAnimator] УСПЕХ: Аниматор найден на объекте {_animator.gameObject.name}");
            }
        }

        private void Update()
        {
            if (_animator == null) return;

            float currentSpeed = _locomotion.CurrentSpeed;
            
            // Отправляем скорость в Аниматор
            _animator.SetFloat(SpeedHash, currentSpeed);

            // ДАТЧИК 2: Какую скорость мы отправляем? (Пишем в лог раз в секунду)
            _logTimer += Time.deltaTime;
            if (_logTimer >= 1f)
            {
                _logTimer = 0f;
                // Debug.Log($"[WolfAnimator] Передаем скорость: {currentSpeed:F2}. Состояние Аниматора: {_animator.GetCurrentAnimatorStateInfo(0).IsName("Locomotion")}");
            }
        }

        public void PlayEatStart()
        {
            Debug.Log("[WolfAnimator] Отправлен триггер EatStart");
            _animator?.SetTrigger(EatTriggerHash);
        }

        public void PlayEatStop()
        {
            Debug.Log("[WolfAnimator] Отправлен триггер EatStop");
            _animator?.SetTrigger(StopEatTriggerHash);
        }



        public void PlayAggro()
        {
            _animator?.SetTrigger(SnapTriggerHash);
        }

    }
}
using UnityEngine;
using Zenject;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfSocial : MonoBehaviour
    {
        private WolfAnimator _animator;
        private WolfStats _stats;
        
        // Таймер, чтобы волки не рычали каждый кадр как пулемет
        private float _cooldownTimer;
        private const float Cooldown = 3f; 

        [Inject]
        public void Construct(WolfAnimator animator, WolfStats stats)
        {
            _animator = animator;
            _stats = stats;
        }

        private void Update()
        {
            if (_cooldownTimer > 0)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }

        // Этот метод автоматически вызывает Unity, когда в нашу сферу-триггер кто-то входит
        private void OnTriggerEnter(Collider other)
        {
            if (_cooldownTimer > 0) return;

            // Проверяем, волк ли это? (Ищем компонент WolfSocial на чужаке)
            if (other.TryGetComponent(out WolfSocial otherWolf))
            {
                // Если наш волк голоден (например, больше 50), он агрессивный и огрызается!
                if (_stats.Hunger > 10f)
                {
                    Debug.Log("[WolfSocial] Волк огрызается на сородича из-за еды!");
                    _animator.PlayAggro();
                    _cooldownTimer = Cooldown;
                }
            }
        }
    }
}
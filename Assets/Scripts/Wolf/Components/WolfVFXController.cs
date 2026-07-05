using UnityEngine;
using Zenject;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfVFXController : MonoBehaviour
    {
        [Header("Эффекты")]
        public ParticleSystem EatVFX;

        private WolfEventBus _eventBus;

        [Inject]
        private void Construct(WolfEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void OnEnable()
        {
            if (_eventBus == null) return;
            
            // Подписываемся на событие поедания
            _eventBus.OnEat += PlayEatVFX;
        }

        private void OnDisable()
        {
            if (_eventBus == null) return;
            
            // Отписываемся при уничтожении объекта
            _eventBus.OnEat -= PlayEatVFX;
        }

        private void PlayEatVFX()
        {
            if (EatVFX != null)
            {
                EatVFX.Play();
                // Лог для проверки, что шина отработала четко
                Debug.Log($"<color=orange>[VFX]</color> Волк {gameObject.name} разбросал ошметки гриба!");
            }
        }
    }
}
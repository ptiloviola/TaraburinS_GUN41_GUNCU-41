using Cysharp.Threading.Tasks;
using MeatMushrooms.Mushroom.Configs;
using MeatMushrooms.Mushroom.Signals;
using UnityEngine;
using Zenject;
using MeatMushrooms.Mushroom.Contracts;

namespace MeatMushrooms.Mushroom.Components
{
    // Требуем, чтобы Unity автоматически добавил эти компоненты на префаб
    [RequireComponent(typeof(MushroomAroma), typeof(MushroomHealth), typeof(MushroomAnimator))]
    public class MushroomEntity : MonoBehaviour
    {
        public MushroomAroma Aroma { get; private set; }
        public MushroomHealth Health { get; private set; }
        private MushroomAnimator _animator;
        private SignalBus _signalBus;

        private void Awake()
        {
            Aroma = GetComponent<MushroomAroma>();
            Health = GetComponent<MushroomHealth>();
            _animator = GetComponent<MushroomAnimator>();
        }

        // Вызывается спавнером в момент создания
        public void Init(MushroomConfig config, SignalBus signalBus)
        {
            _signalBus = signalBus;
            
            Aroma.Init(config);
            Health.Init(config, OnDeath);
            
            _animator.PlaySpawnAnimation();
        }

        private void OnDeath()
        {
            DieAsync().Forget();
        }

        private async UniTaskVoid DieAsync()
        {
            // 1. Отключаем запах, чтобы новые волки не бежали сюда
            Aroma.enabled = false;
            
            // 2. МГНОВЕННО кричим в эфир, что гриба нет (чтобы волки, которые его ели, остановились)
            _signalBus.Fire(new MushroomDestroyedSignal { DestroyedTransform = transform });
            
            // 3. Красиво схлопываемся
            await _animator.PlayDeathAnimation();
            
            // 4. Удаляем объект со сцены
            Destroy(gameObject);
        }
    }
}
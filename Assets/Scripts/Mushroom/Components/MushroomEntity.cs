using Cysharp.Threading.Tasks;
using MeatMushrooms.Mushroom.Configs;
using MeatMushrooms.Mushroom.Signals;
using UnityEngine;
using Zenject;

namespace MeatMushrooms.Mushroom.Components
{
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
            Aroma.enabled = false;
            _signalBus.Fire(new MushroomDestroyedSignal { DestroyedTransform = transform });
            await _animator.PlayDeathAnimation();
            Destroy(gameObject);
        }
    }
}
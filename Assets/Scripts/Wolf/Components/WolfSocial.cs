using System;
using Cysharp.Threading.Tasks;
using MeatMushrooms.Wolf.Configs; // Обновленный неймспейс
using UnityEngine;
using Zenject;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfSocial : MonoBehaviour
    {
        private WolfAnimator _animator;
        private WolfStats _stats;
        private WolfLocomotion _locomotion;
        private WolfConfig _config;
        
        private float _cooldownTimer;

        [Inject]
        public void Construct(WolfAnimator animator, WolfStats stats, WolfLocomotion locomotion, WolfConfig config)
        {
            _animator = animator;
            _stats = stats;
            _locomotion = locomotion;
            _config = config;
        }

        private void Update()
        {
            if (_cooldownTimer > 0)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_cooldownTimer > 0) return;

            WolfSocial otherWolf = other.GetComponentInParent<WolfSocial>();
            
            if (otherWolf != null && otherWolf != this)
            {
                if (_stats.Hunger > _config.Social.AggroHungerThreshold)
                {
                    Debug.Log($"[WolfSocial] {gameObject.name} агрессивно рычит на сородича!");
                    ReactAsync().Forget(); 
                }
            }
        }

        private async UniTaskVoid ReactAsync()
        {
            _cooldownTimer = _config.Social.Cooldown;

            _locomotion.SetStun(true);
            _animator.PlayAggro();

            await UniTask.Delay(TimeSpan.FromSeconds(_config.Social.StunDuration));

            if (_locomotion != null)
            {
                _locomotion.SetStun(false);
            }
        }
    }
}
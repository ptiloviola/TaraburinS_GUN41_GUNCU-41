using System;
using Cysharp.Threading.Tasks;
using MeatMushrooms.Wolf.Configs; 
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

        private WolfEventBus _eventBus;

        

        public bool IsReacting { get; private set; }
        public bool IsHowling { get; set; }

        [Inject]
        public void Construct(WolfAnimator animator, WolfStats stats, 
            WolfLocomotion locomotion, WolfConfig config, WolfEventBus eventBus)
        {
            _animator = animator;
            _stats = stats;
            _locomotion = locomotion;
            _config = config;
            _eventBus = eventBus;
        }

        private void Start()
        {
            Debug.Log($"[WolfSocial] Скрипт запущен на {gameObject.name}! Конфиг загружен: {_config != null}");
        }

        private void Update()
        {
            if (_cooldownTimer > 0)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (_cooldownTimer > 0 || IsReacting) return;

            WolfSocial otherWolf = other.GetComponentInParent<WolfSocial>();
            
            if (otherWolf != null && otherWolf != this)
            {
                if (otherWolf.IsReacting) return;

                if (_stats.Hunger > _config.Social.AggroHungerThreshold)
                {
                    Debug.Log($"[WolfSocial] {gameObject.name} доминирует над {otherWolf.gameObject.name}!");
                    InitiateAggression(otherWolf); 
                }
            }
        }

        private void InitiateAggression(WolfSocial targetWolf)
        {
            IsReacting = true;
            _cooldownTimer = _config.Social.Cooldown;
            targetWolf.GetIntimidated(_config.Social.StunDuration);
            ReactAsync().Forget();
        }

        public void GetIntimidated(float duration)
        {
            _cooldownTimer = _config.Social.Cooldown; 
            IntimidateAsync(duration).Forget();
        }

        private async UniTaskVoid IntimidateAsync(float duration)
        {
            _locomotion.SetStun(true);
            
            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            if (_locomotion != null)
            {
                _locomotion.SetStun(false);
            }
        }

        private async UniTaskVoid ReactAsync()
        {
            _locomotion.SetStun(true);
            _animator.PlayAggro(); 
            _eventBus.FireCombatGrowl();

            await UniTask.Delay(TimeSpan.FromSeconds(_config.Social.StunDuration));

            if (_locomotion != null)
            {
                _locomotion.SetStun(false);
            }

            IsReacting = false;
        }
    }
}
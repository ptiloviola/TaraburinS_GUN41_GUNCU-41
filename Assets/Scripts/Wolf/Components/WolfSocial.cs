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

        

        // ПУБЛИЧНЫЙ ФЛАГ: Этот волк уже начал рычать?
        public bool IsReacting { get; private set; }
        public bool IsHowling { get; set; } // ДОБАВИЛИ ФЛАГ ВОЯ

        [Inject]
        public void Construct(WolfAnimator animator, WolfStats stats, WolfLocomotion locomotion, WolfConfig config)
        {
            _animator = animator;
            _stats = stats;
            _locomotion = locomotion;
            _config = config;
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

        // ИСПОЛЬЗУЕМ STAY для сканирования внутри радара
        private void OnTriggerStay(Collider other)
        {
            // Если я сам на кулдауне или УЖЕ рычу — игнорирую всех
            if (_cooldownTimer > 0 || IsReacting) return;

            WolfSocial otherWolf = other.GetComponentInParent<WolfSocial>();
            
            if (otherWolf != null && otherWolf != this)
            {
                // ПРОВЕРКА НА ДОМИНАНТА: Если сородич успел начать рычать первым, я пасую
                if (otherWolf.IsReacting) return;

                if (_stats.Hunger > _config.Social.AggroHungerThreshold)
                {
                    Debug.Log($"[WolfSocial] {gameObject.name} доминирует над {otherWolf.gameObject.name}!");
                    
                    // Я первый! Перехватываю инициативу
                    InitiateAggression(otherWolf); 
                }
            }
        }

        // Мы стали инициатором грызни
        private void InitiateAggression(WolfSocial targetWolf)
        {
            IsReacting = true; // Занимаем флаг, чтобы второй нас не перебил
            _cooldownTimer = _config.Social.Cooldown;

            // Заставляем ВТОРОГО волка испуганно застыть 
            targetWolf.GetIntimidated(_config.Social.StunDuration);

            // Сами рычим
            ReactAsync().Forget();
        }

        // Этот метод вызывает Волк-Агрессор у Волка-Жертвы
        public void GetIntimidated(float duration)
        {
            // Вешаем жертве кулдаун, чтобы она не огрызнулась в ответ сразу после стана
            _cooldownTimer = _config.Social.Cooldown; 
            IntimidateAsync(duration).Forget();
        }

        // Асинхронный стан для испугавшегося волка
        private async UniTaskVoid IntimidateAsync(float duration)
        {
            _locomotion.SetStun(true);
            
            // Ждем окончание стана (Аниматор сам поставит волка в позу A Wait из-за нулевой скорости)
            await UniTask.Delay(TimeSpan.FromSeconds(duration));

            if (_locomotion != null)
            {
                _locomotion.SetStun(false);
            }
        }

        // Асинхронный рык для волка-доминанта
        private async UniTaskVoid ReactAsync()
        {
            _locomotion.SetStun(true);
            _animator.PlayAggro(); 

            await UniTask.Delay(TimeSpan.FromSeconds(_config.Social.StunDuration));

            if (_locomotion != null)
            {
                _locomotion.SetStun(false);
            }

            IsReacting = false; // Освобождаем флаг после того, как прорычались
        }
    }
}
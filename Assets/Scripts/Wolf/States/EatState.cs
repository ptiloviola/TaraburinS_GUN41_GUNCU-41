using MeatMushrooms.Mushroom.Contracts;
using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;

namespace MeatMushrooms.Wolf.States
{
    public class EatState : IWolfState
    {
        private readonly WolfStats _stats;
        private readonly WolfLocomotion _locomotion;
        private readonly WolfSenses _senses;
        private readonly WolfAnimator _animator; // ДОБАВИЛИ
        
        private IEdible _targetFood;
        private float _biteTimer;
        private const float BiteInterval = 1f; // Кусает раз в секунду
        private const float BiteDamage = 25f;  // Сколько "жизней" откусывает за раз

        public EatState(WolfStats stats, WolfLocomotion locomotion, WolfSenses senses, WolfAnimator animator)
        {
            _stats = stats;
            _locomotion = locomotion;
            _senses = senses;
            _animator = animator; 
        }

        public float CalculateScore()
        {
            _targetFood = _senses.GetClosestFood();
            
            if (_targetFood == null || _stats.Hunger <= 0f)
                return 0f;

            float distance = Vector3.Distance(_locomotion.transform.position, _targetFood.Transform.position);
            
            // Если еда рядом - это абсолютный приоритет!
            if (distance <= 1.5f)
                return 100f;

            return 0f;
        }

        public void Enter()
        {
            Debug.Log("[EatState] Волк начал есть!");
            _locomotion.Stop();
            _locomotion.SetAvoidancePriority(10); 
            
            // Запускаем анимацию еды!
            _animator.PlayEatStart();
            
            _biteTimer = BiteInterval;
        }

        public void Tick()
        {
            if (_targetFood == null || _targetFood.Transform == null) return;

            // --- ДОБАВЛЕНО: Плавный поворот к еде ---
            // 1. Вычисляем направление от волка к грибу
            Vector3 directionToFood = _targetFood.Transform.position - _locomotion.transform.position;
            
            // 2. Обнуляем разницу по высоте (Y), чтобы волк не пытался "задрать нос" или клюнуть в землю
            directionToFood.y = 0; 
            
            // 3. Если вектор не нулевой, плавно поворачиваем волка
            if (directionToFood.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToFood);
                
                // Скорость поворота (5f). Можно менять для более быстрого/медленного разворота
                _locomotion.transform.rotation = Quaternion.Slerp(
                    _locomotion.transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * 5f
                );
            }
            // -----------------------------------------

            _biteTimer -= Time.deltaTime;
            if (_biteTimer <= 0)
            {
                _biteTimer = BiteInterval;
                float calories = _targetFood.Consume(BiteDamage);
                _stats.Eat(calories); 
            }
        }

        public void Exit()
        {
            Debug.Log("[EatState] Волк закончил трапезу.");
            _locomotion.SetAvoidancePriority(50);
            
            // Возвращаемся в режим ходьбы/стояния!
            _animator.PlayEatStop();
        }
    }
}
using MeatMushrooms.Mushroom.Contracts;
using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Configs;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;

namespace MeatMushrooms.Wolf.States
{
    public class EatState : IWolfState
    {
        private readonly WolfStats _stats;
        private readonly WolfLocomotion _locomotion;
        private readonly WolfSenses _senses;
        private readonly WolfAnimator _animator;
        private readonly WolfConfig _config;
        
        private IEdible _targetFood;
        private float _biteTimer;
        private readonly WolfEventBus _eventBus;

        public EatState(WolfStats stats, WolfLocomotion locomotion, WolfSenses senses, 
            WolfAnimator animator, WolfConfig config,  WolfEventBus eventBus)
        {
            _stats = stats;
            _locomotion = locomotion;
            _senses = senses;
            _animator = animator;
            _config = config;
            _eventBus = eventBus;
        }

        public float CalculateScore()
        {
            _targetFood = _senses.GetClosestFood();
            
            if (_targetFood == null || _stats.Hunger <= 0f)
                return 0f;

            float distance = Vector3.Distance(_locomotion.transform.position, _targetFood.Transform.position);
            
            if (distance <= _config.Eat.MaxDistanceToEat)
                return _config.Eat.BaseScore;

            return 0f;
        }

        public void Enter()
        {
            Debug.Log("[EatState] Волк начал есть!");
            _locomotion.Stop();
            _locomotion.SetAvoidancePriority(10); 
            
            _animator.PlayEatStart();
            _eventBus.FireEat();
            
            _biteTimer = _config.Eat.BiteInterval;
        }

        public void Tick()
        {
            if (_targetFood == null || _targetFood.Transform == null) return;

            Vector3 directionToFood = _targetFood.Transform.position - _locomotion.transform.position;
            directionToFood.y = 0; 
            
            if (directionToFood.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToFood);
                _locomotion.transform.rotation = Quaternion.Slerp(
                    _locomotion.transform.rotation, 
                    targetRotation, 
                    Time.deltaTime * _config.Eat.RotationSpeed
                );
            }

            _biteTimer -= Time.deltaTime;
            if (_biteTimer <= 0)
            {
                _biteTimer = _config.Eat.BiteInterval;
                float calories = _targetFood.Consume(_config.Eat.BiteDamage);
                _stats.Eat(calories); 
            }
        }

        public void Exit()
        {
            Debug.Log("[EatState] Волк закончил трапезу.");
            
            _stats.RecordMeal(); 
            
            _locomotion.SetAvoidancePriority(50);
            _animator.PlayEatStop();
        }
    }
}
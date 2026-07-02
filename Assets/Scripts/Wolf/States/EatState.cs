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
        
        private IEdible _targetFood;
        private float _biteTimer;
        private const float BiteInterval = 1f; // Кусает раз в секунду
        private const float BiteDamage = 25f;  // Сколько "жизней" откусывает за раз

        public EatState(WolfStats stats, WolfLocomotion locomotion, WolfSenses senses)
        {
            _stats = stats;
            _locomotion = locomotion;
            _senses = senses;
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
            
            // Делаем волка "тяжелым" для системы RVO, чтобы другие его обтекали
            _locomotion.SetAvoidancePriority(10); 
            
            _biteTimer = BiteInterval;
        }

        public void Tick()
        {
            if (_targetFood == null || _targetFood.Transform == null) return;

            _biteTimer -= Time.deltaTime;
            if (_biteTimer <= 0)
            {
                _biteTimer = BiteInterval;
                
                // Наносим урон и получаем калории!
                float caloriesConsumed = _targetFood.Consume(BiteDamage);
                
                // Передаем калории в желудок
                _stats.Eat(caloriesConsumed); 
            }
        }

        public void Exit()
        {
            Debug.Log("[EatState] Волк закончил трапезу.");
            // Возвращаем стандартный приоритет толпы
            _locomotion.SetAvoidancePriority(50); 
        }
    }
}
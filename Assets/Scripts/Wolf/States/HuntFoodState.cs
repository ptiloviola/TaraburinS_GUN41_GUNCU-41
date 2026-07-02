using MeatMushrooms.Mushroom.Contracts;
using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;

namespace MeatMushrooms.Wolf.States
{
    public class HuntFoodState : IWolfState
    {
        private readonly WolfStats _stats;
        private readonly WolfLocomotion _locomotion;
        private readonly WolfSenses _senses;
        
        private IEdible _targetFood;

        public HuntFoodState(WolfStats stats, WolfLocomotion locomotion, WolfSenses senses)
        {
            _stats = stats;
            _locomotion = locomotion;
            _senses = senses;
        }

        public float CalculateScore()
        {
            _targetFood = _senses.GetClosestFood();
            
            // Если еды нет, ИЛИ волк достаточно сыт (голод < 30) - охоты не будет
            if (_targetFood == null || _stats.Hunger < 30f)
                return 0f;

            float distance = Vector3.Distance(_locomotion.transform.position, _targetFood.Transform.position);
            if (distance <= 1.5f)
                return 0f;

            // Желание охотиться от 50 до 90
            return 50f + (_stats.Hunger * 0.4f);
        }

        public void Enter()
        {
            Debug.Log("[HuntFoodState] Волк почуял еду и побежал к ней!");
            if (_targetFood != null)
            {
                _locomotion.MoveTo(_targetFood.Transform.position);
            }
        }

        public void Tick()
        {
            // Если кто-то другой уже съел наш гриб, цель исчезнет.
            // Мозг сам пересчитает Score в следующем кадре и выкинет нас из этого состояния.
            if (_targetFood == null || _targetFood.Transform == null)
                return;

            // Динамическое обновление цели (если бы еда двигалась, это было бы критично)
            _locomotion.MoveTo(_targetFood.Transform.position);
        }

        public void Exit()
        {
            Debug.Log("[HuntFoodState] Охота прекращена.");
            _locomotion.Stop();
        }
    }
}
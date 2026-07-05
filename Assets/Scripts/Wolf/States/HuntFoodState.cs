using MeatMushrooms.Mushroom.Contracts;
using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Configs;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;

namespace MeatMushrooms.Wolf.States
{
    public class HuntFoodState : IWolfState
    {
        private readonly WolfStats _stats;
        private readonly WolfLocomotion _locomotion;
        private readonly WolfSenses _senses;
        private readonly WolfConfig _config;
        
        private IEdible _targetFood;

        public HuntFoodState(WolfStats stats, WolfLocomotion locomotion, WolfSenses senses, WolfConfig config)
        {
            _stats = stats;
            _locomotion = locomotion;
            _senses = senses;
            _config = config;
        }

        public float CalculateScore()
        {
            _targetFood = _senses.GetClosestFood();
            
            if (_targetFood == null || _stats.Hunger < _config.Hunt.MinHungerToHunt)
                return 0f;

            float distance = Vector3.Distance(_locomotion.transform.position, _targetFood.Transform.position);
            if (distance <= _config.Hunt.DistanceToSwitchToEat)
                return 0f;

            return _config.Hunt.BaseScore + (_stats.Hunger * _config.Hunt.HungerMultiplier);
        }

        public void Enter()
        {
            Debug.Log("[HuntFoodState] Волк почуял еду и побежал к ней!");
            _locomotion.SetSpeed(_config.Locomotion.RunSpeed);
            
            if (_targetFood != null)
            {
                _locomotion.MoveTo(_targetFood.Transform.position);
            }
        }

        public void Tick()
        {
            if (_targetFood == null || _targetFood.Transform == null)
                return;

            _locomotion.MoveTo(_targetFood.Transform.position);
        }

        public void Exit()
        {
            Debug.Log("[HuntFoodState] Охота прекращена.");
            _locomotion.Stop();
        }
    }
}
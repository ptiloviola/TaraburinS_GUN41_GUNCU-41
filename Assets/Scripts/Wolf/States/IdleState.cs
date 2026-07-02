using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Configs; // Обновленный неймспейс
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;

namespace MeatMushrooms.Wolf.States
{
    public class IdleState : IWolfState
    {
        private readonly WolfStats _stats;
        private readonly WolfConfig _config;

        public IdleState(WolfStats stats, WolfConfig config)
        {
            _stats = stats;
            _config = config;
        }

        public float CalculateScore()
        {
            float score = _config.Idle.BaseScore - (_stats.Hunger * _config.Idle.HungerPenaltyMultiplier); 
            return Mathf.Max(0, score); 
        }

        public void Enter()
        {
            Debug.Log("[IdleState] Волк лег отдыхать.");
        }

        public void Tick()
        {
        }

        public void Exit()
        {
            Debug.Log("[IdleState] Волк поднимается.");
        }
    }
}
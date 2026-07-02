using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;

namespace MeatMushrooms.Wolf.States
{
    public class IdleState : IWolfState
    {
        private readonly WolfStats _stats;

        public IdleState(WolfStats stats)
        {
            _stats = stats;
        }

        public float CalculateScore()
        {
            // Если волк ОЧЕНЬ голоден (больше 80), он не хочет отдыхать (оценка 0).
            // Если он сыт (голод 0), оценка максимальная для отдыха (например, 40).
            float score = 40f - (_stats.Hunger * 0.5f); 
            
            // Защита от отрицательных значений
            return Mathf.Max(0, score); 
        }

        public void Enter()
        {
            Debug.Log("[IdleState] Волк лег отдыхать.");
            // Здесь мы будем запускать анимацию Wait_Random
        }

        public void Tick()
        {
            // Волк просто дышит и переваривает еду
        }

        public void Exit()
        {
            Debug.Log("[IdleState] Волк поднимается.");
        }
    }
}
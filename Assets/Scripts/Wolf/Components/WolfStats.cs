using UnityEngine;
using Zenject;
using System.Collections.Generic;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfStats : ITickable
    {

        public float Hunger { get; private set; } 
        
        private const float HungerRate = 2f; 

        private readonly Queue<float> _mealTimestamps = new Queue<float>();

        public WolfStats()
        {
            Hunger = 30f; 
        }

        public void Tick()
        {
            if (Hunger < 100f)
            {
                Hunger += HungerRate * Time.deltaTime;
                if (Hunger > 100f) Hunger = 100f;
            }
        }

        public void Eat(float nutritionValue)
        {
            Hunger -= nutritionValue;
            if (Hunger < 0) Hunger = 0;
            
            // Debug.Log($"[WolfStats] Волк поел! Текущий голод: {Hunger:F1}");
        }

        public void RecordMeal()
        {
            _mealTimestamps.Enqueue(Time.time);
        }

        public bool IsFoodComa(int requiredMeals, float timeWindow)
        {
            while (_mealTimestamps.Count > 0 && Time.time - _mealTimestamps.Peek() > timeWindow)
            {
                _mealTimestamps.Dequeue();
            }
            return _mealTimestamps.Count >= requiredMeals;
        }
    }
}
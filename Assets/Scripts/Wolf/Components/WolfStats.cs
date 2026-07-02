using UnityEngine;
using Zenject;
using System.Collections.Generic;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfStats : ITickable
    {

        // Уровень голода от 0 (полностью сыт) до 100 (умирает от голода)
        public float Hunger { get; private set; } 
        
        // Насколько быстро растет голод (единиц в секунду)
        private const float HungerRate = 2f; 

        // Очередь, хранящая время каждого приема пищи
        private readonly Queue<float> _mealTimestamps = new Queue<float>();

        public WolfStats()
        {
            // При спавне волк немного голоден, чтобы не стоять столбом
            Hunger = 30f; 
        }

        public void Tick()
        {
            // Постепенно увеличиваем голод, если он меньше 100
            if (Hunger < 100f)
            {
                Hunger += HungerRate * Time.deltaTime;
                if (Hunger > 100f) Hunger = 100f;
            }
        }

        // Метод для состояний еды: сброс голода
        public void Eat(float nutritionValue)
        {
            Hunger -= nutritionValue;
            if (Hunger < 0) Hunger = 0;
            
            Debug.Log($"[WolfStats] Волк поел! Текущий голод: {Hunger:F1}");
        }

        // Метод, который мы будем вызывать, когда волк поел
        public void RecordMeal()
        {
            _mealTimestamps.Enqueue(Time.time);
        }

        // Проверяем, наступила ли пищевая кома
        public bool IsFoodComa(int requiredMeals, float timeWindow)
        {
            // Выкидываем из памяти старые приемы пищи (которые были давно)
            while (_mealTimestamps.Count > 0 && Time.time - _mealTimestamps.Peek() > timeWindow)
            {
                _mealTimestamps.Dequeue();
            }
            
            // Если за нужное время поели достаточно раз - пора спать
            return _mealTimestamps.Count >= requiredMeals;
        }
    }
}
using System.Collections.Generic;
using MeatMushrooms.Mushroom.Contracts;
using MeatMushrooms.Mushroom.Signals;
using UnityEngine;
using Zenject;

namespace MeatMushrooms.Wolf.Components
{
    public class WolfSenses : MonoBehaviour
    {
        [Header("Perception")]
        [Tooltip("Базовый радиус чутья волка (его личный радар)")]
        public float BaseScentRadius = 5f;
        
        private SignalBus _signalBus;
        
        // Структура, объединяющая еду и её запах в один пакет данных
        private struct MushroomData
        {
            public IEdible Edible;
            public IHasAroma Aroma;
        }

        // Теперь база данных хранит наши структуры, а не просто интерфейс еды
        private readonly List<MushroomData> _trackedMushrooms = new List<MushroomData>();

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Start()
        {
            _signalBus.Subscribe<MushroomSpawnedSignal>(OnFoodSpawned);
            _signalBus.Subscribe<MushroomDestroyedSignal>(OnFoodDestroyed);
        }

        private void OnDestroy()
        {
            _signalBus.TryUnsubscribe<MushroomSpawnedSignal>(OnFoodSpawned);
            _signalBus.TryUnsubscribe<MushroomDestroyedSignal>(OnFoodDestroyed);
        }

        private void OnFoodSpawned(MushroomSpawnedSignal signal)
        {
            // Сохраняем сразу оба контракта, прилетевших из сигнала
            _trackedMushrooms.Add(new MushroomData 
            { 
                Edible = signal.EdibleComponent, 
                Aroma = signal.AromaComponent 
            });
        }

        private void OnFoodDestroyed(MushroomDestroyedSignal signal)
        {
            // Удаляем гриб из памяти по его Transform
            _trackedMushrooms.RemoveAll(m => m.Edible.Transform == signal.DestroyedTransform);
        }

        public IEdible GetClosestFood()
        {
            IEdible closest = null;
            float minDistance = float.MaxValue;

            // Чистим список от уничтоженных объектов
            _trackedMushrooms.RemoveAll(item => item.Edible == null || item.Edible.Transform == null);

            foreach (var mushroom in _trackedMushrooms)
            {
                // Считаем дистанцию от волка до гриба
                float dist = Vector3.Distance(transform.position, mushroom.Aroma.Transform.position);
                
                // РАДАР: Складываем дальность носа волка и силу запаха гриба
                float totalDetectionRadius = BaseScentRadius + mushroom.Aroma.CurrentRadius;

                // Проверяем, пересеклись ли их ауры
                if (dist <= totalDetectionRadius)
                {
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closest = mushroom.Edible;
                    }
                }
            }
            
            return closest;
        }

        // Рисуем радар в редакторе
        private void OnDrawGizmos()
        {
            // Полупрозрачный желтый цвет для чутья волка
            Gizmos.color = new Color(1f, 0.9f, 0.1f, 0.3f); 
            Gizmos.DrawWireSphere(transform.position, BaseScentRadius);
        }
    }
}
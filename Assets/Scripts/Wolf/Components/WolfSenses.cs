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
        
        private struct MushroomData
        {
            public IEdible Edible;
            public IHasAroma Aroma;
        }

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
            _trackedMushrooms.Add(new MushroomData 
            { 
                Edible = signal.EdibleComponent, 
                Aroma = signal.AromaComponent 
            });
        }

        private void OnFoodDestroyed(MushroomDestroyedSignal signal)
        {
            _trackedMushrooms.RemoveAll(m => m.Edible.Transform == signal.DestroyedTransform);
        }

        public IEdible GetClosestFood()
        {
            IEdible closest = null;
            float minDistance = float.MaxValue;

            _trackedMushrooms.RemoveAll(item => item.Edible == null || item.Edible.Transform == null);

            foreach (var mushroom in _trackedMushrooms)
            {
                float dist = Vector3.Distance(transform.position, mushroom.Aroma.Transform.position);
                float totalDetectionRadius = BaseScentRadius + mushroom.Aroma.CurrentRadius;
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

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.9f, 0.1f, 0.3f); 
            Gizmos.DrawWireSphere(transform.position, BaseScentRadius);
        }
    }
}
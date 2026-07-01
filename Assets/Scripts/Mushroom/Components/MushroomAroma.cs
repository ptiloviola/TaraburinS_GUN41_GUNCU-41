using MeatMushrooms.Mushroom.Configs;
using MeatMushrooms.Mushroom.Contracts; // Подключили
using UnityEngine;

namespace MeatMushrooms.Mushroom.Components
{
    // Реализуем IHasAroma
    public class MushroomAroma : MonoBehaviour, IHasAroma
    {
        private float _maxRadius;
        private float _speed;
        
        public float CurrentRadius { get; private set; }
        
        // Реализация требования
        public Transform Transform => transform;

        public void Init(MushroomConfig config)
        {
            _maxRadius = config.MaxAromaRadius;
            _speed = config.AromaSpeed;
            CurrentRadius = 0f;
        }

        private void Update()
        {
            if (CurrentRadius < _maxRadius)
            {
                CurrentRadius += _speed * Time.deltaTime;
                if (CurrentRadius > _maxRadius) CurrentRadius = _maxRadius;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, CurrentRadius);
        }
    }
}
using UnityEngine;
using Infrastructure.Interfaces;

namespace Enemy
{
    
    [RequireComponent(typeof(EnemyMover))]
    public class EnemyFacade : MonoBehaviour, IDamageable
    {
        private EnemyAnimator _animator;
        [Header("Settings")]
        [SerializeField] private EnemyConfig config;
        
        private EnemyMover _mover;

        private void Awake()
        {
            _mover = GetComponent<EnemyMover>();
            _animator = GetComponent<EnemyAnimator>(); // Добавили ссылку

            _mover.Initialize(config);
            _animator.Initialize(config); // Инициализируем аниматор
        }

        private void Start()
        {
            // Запускаем логику
            _mover.StartWandering();
            // Прячем врага на старте (он проявится, только если игрок рядом)
            _animator.SetInvisibleInstant();
        }

        private void Update()
        {
            float speed = _mover.GetCurrentSpeed();
            float currentYRotation = transform.eulerAngles.y; // Берем текущий угол поворота агента
            
            _animator.UpdateAnimation(speed, currentYRotation);
        }
        // Этот метод будет вызывать PlayerVision
        public void SetVisibility(bool isVisible)
        {
            _animator.AnimateVisibility(isVisible);
        }

        // ДОБАВЛЯЕМ ЭТОТ БЛОК:
        // Вызывается из RevolverController при попадании шарика
        public void TakeDamage(int amount, Vector3 hitPoint)
        {
            _animator.PlayHitReaction();
            Debug.Log($"Снеговик получил {amount} урона в точку {hitPoint}!");
        }
    }
}
using UnityEngine;

namespace Enemy
{
    
    [RequireComponent(typeof(EnemyMover))]
    public class EnemyFacade : MonoBehaviour
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
        }

        private void Update()
        {
            float speed = _mover.GetCurrentSpeed();
            float currentYRotation = transform.eulerAngles.y; // Берем текущий угол поворота агента
            
            _animator.UpdateAnimation(speed, currentYRotation);
        }
    }
}
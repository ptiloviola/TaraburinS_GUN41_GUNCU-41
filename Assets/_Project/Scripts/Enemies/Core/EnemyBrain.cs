using UnityEngine;
using UnityEngine.AI;
using TpsShooter.Combat;
using TpsShooter.Enemies.Configs;
using TpsShooter.Player;
using TpsShooter.Enemies.Vision; // Добавлено для доступа к EnemySensor
using TpsShooter.Enemies.States;

namespace TpsShooter.Enemies.Core
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBrain : MonoBehaviour, IDamageable
    {
        [SerializeField] private EnemyConfig _config;

        [Header("Patrol Settings")]
        [SerializeField] private Transform[] _patrolPoints;
        
        // Компоненты (Контекст для стейтов)
        public NavMeshAgent Agent { get; private set; }
        public EnemyConfig Config => _config;
        public Transform[] PatrolPoints => _patrolPoints;
        public PlayerFacade Target { get; private set; } 
        public Vector3 LastKnownTargetPosition { get; set; }

        // Системы (Чистый C#)
        public EnemyStateMachine StateMachine { get; private set; }
        public EnemySensor Sensor { get; private set; }
        
        private float _currentHealth;
        private float _lastSensorTickTime;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            StateMachine = new EnemyStateMachine();
            Sensor = new EnemySensor(this);
            
            // Временно ищем игрока на сцене. Позже это будет выдавать SpawnManager
            Target = FindObjectOfType<PlayerFacade>();
        }

        private void Start()
        {
            if (_config != null)
            {
                _currentHealth = _config.MaxHealth;
                
                // ВНИМАНИЕ: Назначение скорости убрано отсюда!
                // Ею будут управлять классы состояний (PatrolState / CombatState)
                
                // TODO: Инициализировать стартовое состояние (Patrol)
                StateMachine.Initialize(new EnemyPatrolState(this));
            }
        }

        private void Update()
        {
            if (_currentHealth <= 0) return;
            
            // 1. Оптимизированный опрос сенсора зрения
            if (Time.time - _lastSensorTickTime >= _config.SensorTickRate)
            {
                _lastSensorTickTime = Time.time;
                Sensor.Tick();
            }

            // 2. Обновление текущего состояния
            StateMachine.Tick();
        }

        public void TakeDamage(float amount)
        {
            if (_currentHealth <= 0) return;

            _currentHealth -= amount;
            Debug.Log($"<color=orange>[Enemy]</color> Получил {amount} урона. Осталось: {_currentHealth}");

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log($"<color=red>[Enemy]</color> Умер!");
            Agent.isStopped = true;
            // TODO: Переключиться в EnemyDeadState, заспавнить лут из Фабрики
        }

        private void OnDrawGizmosSelected()
        {
            if (_config == null) return;

            // Цвет зависит от состояния (пока сделаем желтый по умолчанию, красный если видим цель)
            Gizmos.color = (Sensor != null && Sensor.IsTargetVisible) ? Color.red : Color.yellow;

            // Рисуем радиус обзора
            Gizmos.DrawWireSphere(transform.position, _config.VisionRadius);

            // Рисуем радиус атаки (отдельным цветом)
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, _config.AttackRange);

            // Рисуем конус зрения
            Gizmos.color = Color.blue;
            Vector3 leftBoundary = Quaternion.Euler(0, -_config.ViewAngle / 2f, 0) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, _config.ViewAngle / 2f, 0) * transform.forward;
            
            Gizmos.DrawRay(transform.position, leftBoundary * _config.VisionRadius);
            Gizmos.DrawRay(transform.position, rightBoundary * _config.VisionRadius);

            // Линия к цели, если видим её
            if (Sensor != null && Sensor.IsTargetVisible && Target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, Target.transform.position + Vector3.up * 1.5f);
            }
        }
    }
}
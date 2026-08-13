using UnityEngine;
using UnityEngine.AI;
using TpsShooter.Combat;
using TpsShooter.Enemies.Configs;
using TpsShooter.Player;
using TpsShooter.Enemies.Vision; // Добавлено для доступа к EnemySensor
using TpsShooter.Enemies.States;
using TpsShooter.Enemies.Weapons;
using Zenject;
using TpsShooter.Environment;

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
        public EnemyWeaponController WeaponController { get; private set; }

        public EnemyAnimator Animator { get; private set; }
        
        public HealthEngine Health { get; private set; }
        public LootFactory LootSpawner { get; private set; }
        private float _lastSensorTickTime;


        [Inject]
        public void Construct(LootFactory lootFactory)
        {
            LootSpawner = lootFactory;
        }

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            StateMachine = new EnemyStateMachine();
            Sensor = new EnemySensor(this);
            // Пытаемся получить контроллер. Если его нет — добавляем на лету
            WeaponController = GetComponent<EnemyWeaponController>();
            Animator = GetComponentInChildren<EnemyAnimator>();
            if (WeaponController == null)
            {
                WeaponController = gameObject.AddComponent<EnemyWeaponController>();
            }
            
            // Временно ищем игрока на сцене. Позже это будет выдавать SpawnManager
            Target = FindObjectOfType<PlayerFacade>();
        }

        private void Start()
        {
            if (_config != null)
            {
                // 3. ИНИЦИАЛИЗИРУЕМ ЗДОРОВЬЕ ВРАГА
                Health = new HealthEngine(_config.MaxHealth);
                Health.OnDeath += Die; // Подписываемся на собственную смерть

                WeaponController.Initialize(_config);
                
                // ВНИМАНИЕ: Назначение скорости убрано отсюда!
                // Ею будут управлять классы состояний (PatrolState / CombatState)
                
                // TODO: Инициализировать стартовое состояние (Patrol)
                StateMachine.Initialize(new EnemyPatrolState(this));
            }
        }

        private void Update()
        {
            // 4. ПРОВЕРЯЕМ СТАТУС СМЕРТИ
            if (Health == null || Health.IsDead) return;
            
            if (Time.time - _lastSensorTickTime >= _config.SensorTickRate)
            {
                _lastSensorTickTime = Time.time;
                Sensor.Tick();
            }
            StateMachine.Tick();
        }

        public void TakeDamage(float amount)
        {
            Health?.TakeDamage(amount);
            Debug.Log($"<color=orange>[Enemy]</color> Получил {amount} урона. Осталось ХП: {Health?.CurrentHealth}");

            if (Health != null && !Health.IsDead)
            {
                // ПРОИГРЫВАЕМ АНИМАЦИЮ ПОПАДАНИЯ
                Animator?.PlayHit(); 

                // Если гуляли - переключаемся в поиск
                if (StateMachine.CurrentState is States.EnemyPatrolState)
                {
                    if (Target != null)
                    {
                        LastKnownTargetPosition = Target.transform.position;
                        StateMachine.ChangeState(new States.EnemySearchState(this));
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (Health != null) Health.OnDeath -= Die;
        }

        private void Die()
        {
            Debug.Log($"<color=black>[Enemy]</color> УМЕР!");
            StateMachine.ChangeState(new EnemyDeadState(this));
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
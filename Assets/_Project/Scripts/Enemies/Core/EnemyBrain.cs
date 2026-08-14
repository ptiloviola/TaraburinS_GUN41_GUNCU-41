using UnityEngine;
using UnityEngine.AI;
using Zenject;
using TpsShooter.Combat;
using TpsShooter.Enemies.Configs;
using TpsShooter.Player;
using TpsShooter.Enemies.Vision;
using TpsShooter.Enemies.States;
using TpsShooter.Enemies.Weapons;
using TpsShooter.Environment;

namespace TpsShooter.Enemies.Core
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBrain : MonoBehaviour, IDamageable
    {

        // Компоненты (Контекст для стейтов)
        public NavMeshAgent Agent { get; private set; }
        
        // Конфиг теперь задается строго через Фабрику
        public EnemyConfig Config { get; private set; }
        
        public Transform[] PatrolPoints { get; private set; }
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

        // 1. СТРОГИЙ DI: Zenject прокинет Фабрику Лута и Игрока прямо сюда
        [Inject]
        public void Construct(LootFactory lootFactory, PlayerFacade playerFacade)
        {
            LootSpawner = lootFactory;
            Target = playerFacade; 
        }

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            StateMachine = new EnemyStateMachine();
            Sensor = new EnemySensor(this);
            WeaponController = GetComponent<EnemyWeaponController>();
            Animator = GetComponentInChildren<EnemyAnimator>();
            
            if (WeaponController == null)
            {
                WeaponController = gameObject.AddComponent<EnemyWeaponController>();
            }
        }

        // 2. ИНИЦИАЛИЗАЦИЯ: Вызывается Фабрикой при спавне
        public void Initialize(EnemyConfig config, Transform[] patrolPoints)
        {
            Config = config;
            
            // Защита: если точки не передали, пусть враг считает точкой патруля место своего спавна
            PatrolPoints = (patrolPoints != null && patrolPoints.Length > 0) ? patrolPoints : new Transform[] { transform };

            Health = new HealthEngine(Config.MaxHealth);
            Health.OnDeath += Die;

            WeaponController.Initialize(Config);
            
            StateMachine.Initialize(new EnemyPatrolState(this));
        }

        private void Update()
        {
            if (Health == null || Health.IsDead) return;
            
            if (Time.time - _lastSensorTickTime >= Config.SensorTickRate)
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
                Animator?.PlayHit(); 

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
            if (Config == null) return;

            Gizmos.color = (Sensor != null && Sensor.IsTargetVisible) ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Config.VisionRadius);
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, Config.AttackRange);
            
            Gizmos.color = Color.blue;
            Vector3 leftBoundary = Quaternion.Euler(0, -Config.ViewAngle / 2f, 0) * transform.forward;
            Vector3 rightBoundary = Quaternion.Euler(0, Config.ViewAngle / 2f, 0) * transform.forward;
            Gizmos.DrawRay(transform.position, leftBoundary * Config.VisionRadius);
            Gizmos.DrawRay(transform.position, rightBoundary * Config.VisionRadius);

            if (Sensor != null && Sensor.IsTargetVisible && Target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position + Vector3.up * 1.5f, Target.transform.position + Vector3.up * 1.5f);
            }
        }
    }
}
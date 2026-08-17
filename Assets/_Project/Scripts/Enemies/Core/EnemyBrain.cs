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
using TpsShooter.Audio;
using TpsShooter.Core; // Обновленный неймспейс

namespace TpsShooter.Enemies.Core
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBrain : MonoBehaviour, IDamageable
    {
        public NavMeshAgent Agent { get; private set; }
        public EnemyConfig Config { get; private set; }
        
        public Transform[] PatrolPoints { get; private set; }
        public PlayerFacade Target { get; private set; } 
        public Vector3 LastKnownTargetPosition { get; set; }

        public EnemyStateMachine StateMachine { get; private set; }
        public EnemySensor Sensor { get; private set; }
        public EnemyAnimator Animator { get; private set; }
        
        // НОВАЯ АРХИТЕКТУРА: Абстрактный обработчик боя
        public IEnemyCombatHandler CombatHandler { get; private set; }
        
        public HealthEngine Health { get; private set; }
        public LootFactory LootSpawner { get; private set; }
        
        private float _lastSensorTickTime;
        private FootstepAudioSystem _footstepAudio;
        private CharacterAnimationEvents _animEvents; 
        
        [Inject] private IAudioService _audioService;
        public IAudioService AudioService => _audioService;

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
            Animator = GetComponentInChildren<EnemyAnimator>();
            
            if (Animator != null)
            {
                _animEvents = Animator.gameObject.GetComponent<CharacterAnimationEvents>();
                if (_animEvents == null) 
                    _animEvents = Animator.gameObject.AddComponent<CharacterAnimationEvents>();
            }

            // ИДЕАЛЬНАЯ СТРАТЕГИЯ: Мозг просто берет компонент, который мы повесили на префаб
            CombatHandler = GetComponent<IEnemyCombatHandler>();
            
            if (CombatHandler == null)
            {
                Debug.LogError($"<color=red>[EnemyBrain]</color> На префабе {gameObject.name} нет скрипта боевки (IEnemyCombatHandler)!");
            }
        }

        public void Initialize(EnemyConfig config, Transform[] patrolPoints)
        {
            Config = config;
            PatrolPoints = (patrolPoints != null && patrolPoints.Length > 0) ? patrolPoints : new Transform[] { transform };

            Health = new HealthEngine(Config.MaxHealth);
            Health.OnDeath += Die;

            // Просто инициализируем ту стратегию, которую нашли в Awake
            CombatHandler?.Initialize(Config);
            
            StateMachine.Initialize(new EnemyPatrolState(this));
            
            if (Config.FootstepAudioConfig != null && _audioService != null)
            {
                _footstepAudio = new FootstepAudioSystem(_audioService, transform, _animEvents, Config.FootstepAudioConfig);
            }
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
            Debug.Log($"<color=orange>[Enemy]</color> Получил {amount} урона. ХП: {Health?.CurrentHealth}");

            if (Health != null && !Health.IsDead)
            {
                Animator?.PlayHit(); 
                // ДЕЛЕГИРОВАНИЕ: Мозг больше не решает, куда переходить. Это делает Стейт.
                StateMachine.CurrentState?.OnDamageTaken();
            }
        }

        private void OnDestroy()
        {
            if (Health != null) Health.OnDeath -= Die;
            _footstepAudio?.Dispose();
        }

        private void Die()
        {
            StateMachine.ChangeState(new EnemyDeadState(this));
        }

        private void OnDrawGizmosSelected()
        {
            if (Config == null) return;

            // ИСПРАВЛЕНИЕ ТЗ: Цвет берется строго из текущего стейта
            Gizmos.color = StateMachine?.CurrentState?.StateGizmoColor ?? Color.white;
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
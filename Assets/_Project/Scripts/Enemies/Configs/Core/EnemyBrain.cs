using UnityEngine;
using UnityEngine.AI;
using Zenject;
using TpsShooter.Combat;
using TpsShooter.Enemies.Configs;
using TpsShooter.Player;
using TpsShooter.Enemies.Vision;
using TpsShooter.Enemies.States;
using TpsShooter.Environment;
using TpsShooter.Audio;
using TpsShooter.Core;

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
            Animator = GetComponentInChildren<EnemyAnimator>();

            Sensor = new EnemySensor(this);
            Sensor.OnHeardNoise += HandleNoiseHeard;
            
            if (Animator != null)
            {
                _animEvents = Animator.gameObject.GetComponent<CharacterAnimationEvents>();
                if (_animEvents == null) 
                    _animEvents = Animator.gameObject.AddComponent<CharacterAnimationEvents>();
            }

            CombatHandler = GetComponent<IEnemyCombatHandler>();
            
            if (CombatHandler == null)
            {
                DevLogger.LogError($"<color=red>[EnemyBrain]</color> На префабе {gameObject.name} нет скрипта боевки (IEnemyCombatHandler)!");
            }
        }

        public void Initialize(EnemyConfig config, Transform[] patrolPoints)
        {
            Config = config;
            PatrolPoints = (patrolPoints != null && patrolPoints.Length > 0) ? patrolPoints : new Transform[] { transform };

            Health = new HealthEngine(Config.MaxHealth);
            Health.OnDeath += Die;

            CombatHandler?.Initialize(Config);
            
            StateMachine.Initialize(Config.CreatePatrolState(this));
            
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
            DevLogger.Log($"<color=orange>[Enemy]</color> Получил {amount} урона. ХП: {Health?.CurrentHealth}");

            if (Health != null && !Health.IsDead)
            {
                Animator?.PlayHit(); 
                StateMachine.CurrentState?.OnDamageTaken();
            }
        }

        private void HandleNoiseHeard(Vector3 noisePosition)
        {
            if (!(StateMachine.CurrentState is EnemyCombatState))
            {
                StateMachine.ChangeState(new EnemySearchState(this));
            }
        }

        private void OnDestroy()
        {
            if (Health != null) Health.OnDeath -= Die;
            if (Sensor != null) 
            {
                Sensor.OnHeardNoise -= HandleNoiseHeard;
                Sensor.Dispose();
            }
            _footstepAudio?.Dispose();
        }

        private void Die()
        {
            StateMachine.ChangeState(new EnemyDeadState(this));
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Config == null) return;

            Vector3 center = transform.position;
            Vector3 eyePos = center + Vector3.up * Config.EyeHeight;

            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(center, Config.HearingRadius);

            Gizmos.color = new Color(1f, 0.9f, 0f, 0.5f);
            Gizmos.DrawWireSphere(center, Config.VisionRadius);

            Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
            Gizmos.DrawWireSphere(center, Config.AttackRange);

            Vector3 leftRay = Quaternion.Euler(0, -(Config.ViewAngle / 2f), 0) * transform.forward;
            Vector3 rightRay = Quaternion.Euler(0, (Config.ViewAngle / 2f), 0) * transform.forward;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(eyePos, leftRay * Config.VisionRadius);
            Gizmos.DrawRay(eyePos, rightRay * Config.VisionRadius);

            if (StateMachine != null && StateMachine.CurrentState != null)
            {
                Gizmos.color = StateMachine.CurrentState.StateGizmoColor;
                Gizmos.DrawSphere(center + Vector3.up * Config.StateIndicatorHeight, 0.2f);
            }
        }
#endif
    }
}
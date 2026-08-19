using UnityEngine;
using UnityEngine.AI;
using Zenject;
using TpsShooter.Combat;
using TpsShooter.Enemies.Configs;
using TpsShooter.Player;
using TpsShooter.Enemies.Vision;
using TpsShooter.Enemies.States;
using TpsShooter.Enemies.Combat;
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

            Sensor = new EnemySensor(this);
            Sensor.OnHeardNoise += HandleNoiseHeard;
            
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
            Debug.Log($"<color=orange>[Enemy]</color> Получил {amount} урона. ХП: {Health?.CurrentHealth}");

            if (Health != null && !Health.IsDead)
            {
                Animator?.PlayHit(); 
                // ДЕЛЕГИРОВАНИЕ: Мозг больше не решает, куда переходить. Это делает Стейт.
                StateMachine.CurrentState?.OnDamageTaken();
            }
        }

        private void HandleNoiseHeard(Vector3 noisePosition)
        {
            // Если мы не в бою, переключаемся на поиск источника шума!
            if (!(StateMachine.CurrentState is EnemyCombatState))
            {
                StateMachine.ChangeState(new EnemySearchState(this));
            }
        }

        private void OnDestroy()
        {
            if (Health != null) Health.OnDeath -= Die;
            if (Sensor != null) Sensor.OnHeardNoise -= HandleNoiseHeard; // Отписка
            _footstepAudio?.Dispose();
        }

        private void Die()
        {
            StateMachine.ChangeState(new EnemyDeadState(this));
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos() // <--- УБРАЛИ СЛОВО Selected
        {
            if (Config == null) return;

            Vector3 center = transform.position;
            Vector3 eyePos = center + Vector3.up * 1.5f;

            // 1. Радиус Слуха (Синий круг)
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(center, Config.HearingRadius);

            // 2. Радиус Зрения (Желтый круг)
            Gizmos.color = new Color(1f, 0.9f, 0f, 0.5f);
            Gizmos.DrawWireSphere(center, Config.VisionRadius);

            // 3. Дистанция Атаки (Красный круг)
            Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
            Gizmos.DrawWireSphere(center, Config.AttackRange);

            // 4. Угол Зрения (Два желтых луча, образующих сектор)
            Vector3 leftRay = Quaternion.Euler(0, -(Config.ViewAngle / 2f), 0) * transform.forward;
            Vector3 rightRay = Quaternion.Euler(0, (Config.ViewAngle / 2f), 0) * transform.forward;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(eyePos, leftRay * Config.VisionRadius);
            Gizmos.DrawRay(eyePos, rightRay * Config.VisionRadius);

            // 5. Текущий Стейт (Цветная сфера над головой)
            if (StateMachine != null && StateMachine.CurrentState != null)
            {
                Gizmos.color = StateMachine.CurrentState.StateGizmoColor;
                Gizmos.DrawSphere(center + Vector3.up * 2.5f, 0.2f);
            }
        }
#endif
    }
}
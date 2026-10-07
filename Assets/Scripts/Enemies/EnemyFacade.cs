using System;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Gameplay.Enemies.Data;
using Gameplay.Combat.Statuses;
using Gameplay.Base;
using Gameplay.Combat;
using Gameplay.Enemies.Visuals;
using Gameplay.Enemies.FSM;

namespace Gameplay.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyFacade : MonoBehaviour
    {
        [Header("Внутренние компоненты")]
        [SerializeField] private HealthComponent _health;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Collider _collider;
        [SerializeField] private DamageReceiver _damageReceiver;

        private EnemyVisualsBase _visuals;
        private EnemyStateMachineFactory _fsmFactory;
        private BaseArrivalHandler _arrivalHandler;
        private EnemyController _controller;

        public bool IsTargetable => _controller.IsTargetable;
        public TargetType TargetType => _controller.TargetType;
        public Vector3 Position => transform.position;
        public Vector3 Velocity => _controller.Velocity;
        public bool IsDespawned => _controller.IsDespawned;

        public event Action<EnemyFacade> OnDespawnRequested;
        public event Action<EnemyStateType> OnStateChanged;

        [Inject]
        public void Construct(EnemyStateMachineFactory fsmFactory, BaseArrivalHandler arrivalHandler)
        {
            _fsmFactory = fsmFactory;
            _arrivalHandler = arrivalHandler;
        }

        private void Awake()
        {
            if (_agent == null) _agent = GetComponent<NavMeshAgent>();
            if (_health == null) _health = GetComponent<HealthComponent>();
            if (_collider == null) _collider = GetComponent<Collider>();
            if (_damageReceiver == null) _damageReceiver = GetComponent<DamageReceiver>();
            _visuals = GetComponent<EnemyVisualsBase>();

            _controller = new EnemyController(_health, _agent, _collider, _damageReceiver, _visuals, _fsmFactory, _arrivalHandler);

            _controller.OnDespawnRequested += HandleDespawnRequested;
            _controller.OnStateChanged += HandleStateChanged;
        }

        public void Initialize(EnemyConfig config, IMovementStrategy movement, Vector3 spawnPosition)
        {
            _visuals.BindMovement(movement);
            _controller.Initialize(transform, config, movement, spawnPosition);
        }

        private void Update() => _controller.Tick(Time.deltaTime);

        private void OnDisable() => _controller.Deactivate();

        private void OnDestroy()
        {

            if (_controller != null)
            {
                _controller.OnDespawnRequested -= HandleDespawnRequested;
                _controller.OnStateChanged -= HandleStateChanged;
            }
        }

        public void ApplyStatus(IStatusEffect effect) => _controller.ApplyStatus(effect);

        public void RequestDespawn() => _controller.RequestDespawn();

        private void HandleDespawnRequested() => OnDespawnRequested?.Invoke(this);

        private void HandleStateChanged(EnemyStateType state) => OnStateChanged?.Invoke(state);

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out BaseCore baseCore) || other.transform.parent != null && other.transform.parent.TryGetComponent(out baseCore))
            {
                _controller.TryReachBase(baseCore);
            }
        }

        public class Pool : MonoMemoryPool<EnemyFacade> { }
    }
}
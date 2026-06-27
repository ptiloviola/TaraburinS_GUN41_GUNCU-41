using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Gameplay.Units.Data;
using System;

namespace Gameplay.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class DefenderFacade : MonoBehaviour
    {
        public enum DefenderState 
        { 
            Idle, 
            MovingToRallyPoint 
        }

        private Pool _pool;
        private NavMeshAgent _agent;
        private DefenderConfig _config;

        public DefenderState CurrentState { get; private set; }

        // Добавляем событие. Передаем самих себя, чтобы казарма знала, кого именно вычеркивать.
        public event Action<DefenderFacade> OnDespawned;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            // Изначально отключаем агента, чтобы он не пытался искать NavMesh 
            // пока болтается где-то в пуле.
            _agent.enabled = false; 
        }

        public void SetPool(Pool pool)
        {
            _pool = pool;
        }

        // НОВОЕ: Бронебойный метод телепортации для пулов!
        public void WarpTo(Vector3 position)
        {
            _agent.enabled = false;       // 1. Усыпляем агента
            transform.position = position; // 2. Мгновенно переносим
            _agent.enabled = true;        // 3. Будим. При включении он жестко привязывается к NavMesh!
        }

        public void InitConfig(DefenderConfig config)
        {
            _config = config;
            
            _agent.speed = _config.MoveSpeed;
            _agent.angularSpeed = _config.AngularSpeed;
            
            SetState(DefenderState.Idle);
        }

        public void SendToRallyPoint(Vector3 destination)
        {
            // Жесткая проверка: агент должен быть включен и стоять на сетке
            if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.SetDestination(destination);
                SetState(DefenderState.MovingToRallyPoint);
            }
            else
            {
                Debug.LogWarning($"<color=orange>[DefenderFacade] {gameObject.name} не на NavMesh! Не могу пойти на точку.</color>");
            }
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case DefenderState.Idle:
                    break;
                    
                case DefenderState.MovingToRallyPoint:
                    // Проверяем, добежали ли мы
                    if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        SetState(DefenderState.Idle);
                    }
                    break;
            }
        }

        private void SetState(DefenderState newState)
        {
            CurrentState = newState;
        }

        public void Despawn()
        {
            if (_pool != null)
            {
                _agent.enabled = false; // Обязательно выключаем агента перед возвратом в пул!
                _pool.Despawn(this);
                // Оповещаем всех подписчиков (казарму), что мы выбыли
                OnDespawned?.Invoke(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public class Pool : MonoMemoryPool<DefenderFacade> { }
    }
}
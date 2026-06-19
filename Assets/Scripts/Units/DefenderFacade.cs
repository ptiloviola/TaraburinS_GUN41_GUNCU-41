using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Gameplay.Units.Data;
using Unity.VisualScripting;

namespace Gameplay.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class DefenderFacade : MonoBehaviour
    {
        // Базовые состояния нашего защитника
        public enum DefenderState
        {
            Idle,
            MovingToRallyPoint
            // Позже добавим: Combat, Returning
        }

        private Pool _pool;
        private NavMeshAgent _agent;
        private DefenderConfig _config;

        public DefenderState CurrentState { get; private set; }

        private void Awake()
        {
            // Кешируем компонент один раз при рождении объекта
            _agent = GetComponent<NavMeshAgent>();
        }

        // Метод для Zenject MemoryPool
        public void SetPool(Pool pool)
        {
            _pool = pool;
        }

        // Вызывается Казармами (BarracksBehavior) при спавне бойца
        public void InitConfig(DefenderConfig config)
        {
            _config = config;
            _agent.speed = _config.MoveSpeed;
            _agent.angularSpeed = _config.AngularSpeed;
            // Сбрасываем стейт при новом появлении
            SetState(DefenderState.Idle);
        }

        // Приказ от башни: бежать на точку сбора
        public void SendToRallyPoint(Vector3 destination)
        {
            _agent.SetDestination(destination);
            SetState(DefenderState.MovingToRallyPoint);
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case DefenderState.Idle:
                    // Пока ничего не делаем, ждем приказов или врагов
                    break;
                case DefenderState.MovingToRallyPoint:
                    // Проверяем, добежали ли мы до точки (с небольшой погрешностью)
                    if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        // Добежали, встаем в караул
                        SetState(DefenderState.Idle);
                    }
                    break;

            }
        }

        private void SetState(DefenderState newState)
        {
            CurrentState = newState;
            // Здесь в будущем мы будем переключать анимации!
            // Например: _animator.SetTrigger(newState.ToString());
        }

        // Возврат в пул (вызовем, когда боец умрет или башню продадут)
        public void Despawn()
        {
            if (_pool != null)
            {
                _agent.ResetPath();
                _pool.Despawn(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }


        // Zenject Пул для Защитников
        public class Pool : MonoMemoryPool<DefenderFacade> { }
    }
}


using UnityEngine;
using System;
using Gameplay.Towers.Behaviors.Weapons;
using Gameplay.Towers.Behaviors.Aiming;
using Gameplay.Towers.Behaviors.Targeting;

namespace Gameplay.Towers.Behaviors
{
    public class WeaponAdapter : MonoBehaviour, IBehaviorAdapter
    {
        [Header("Прицеливание")]
        [SerializeField] private Transform _logicalRotator;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private float _turnSpeed = 10f;

        public event Action OnBuildStarted;
        public event Action<Vector3> OnShotFired;

        // Открываем доступ для чистого класса через свойства
        public Transform LogicalRotator => _logicalRotator;
        public Transform FirePoint => _firePoint;
        public LayerMask EnemyMask => _enemyLayerMask;
        public float TurnSpeed => _turnSpeed;

        public IAttackExecutor Executor { get; private set; }
        public IAimStrategy Aiming { get; private set; }
        public ITargetingStrategy Targeting { get; private set; }

        private void Awake()
        {
            // Кэшируем зависимости один раз при старте игры (Оптимизация)
            Executor = GetComponentInChildren<IAttackExecutor>();
            Aiming = GetComponentInChildren<IAimStrategy>();
            Targeting = GetComponentInChildren<ITargetingStrategy>();
        }

        // Выполняем контракт IBehaviorAdapter
        public ITowerBehavior CreateBehavior()
        {
            // Рождаем чистую логику и отдаем Фасаду
            return new AttackController(this);
        }

        public void TriggerBuildStarted() => OnBuildStarted?.Invoke();
        public void TriggerShotFired(Vector3 pos) => OnShotFired?.Invoke(pos);

        private void OnDrawGizmos()
        {
            float drawRange = 5f;
            var facade = GetComponentInParent<TowerFacade>();
            if (Application.isPlaying && facade != null && facade.Config != null)
            {
                drawRange = facade.GetCurrentStats().Attack.Range;
            }

            Vector3 center = _logicalRotator != null ? _logicalRotator.position : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(center, drawRange);

            var aim = GetComponentInChildren<IAimStrategy>();
            aim?.DrawAimGizmo(_logicalRotator != null ? _logicalRotator : transform, drawRange);
        }
    }
}
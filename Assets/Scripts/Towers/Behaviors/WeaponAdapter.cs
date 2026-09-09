using UnityEngine;
using System;
using Zenject;
using Gameplay.Towers.Behaviors.Aiming;
using Gameplay.Projectiles.Factories; // Подключаем пространство имен нашей фабрики

namespace Gameplay.Towers.Behaviors
{
    public class WeaponAdapter : MonoBehaviour, IBehaviorAdapter
    {
        [Header("Прицеливание")]
        [SerializeField] private Transform _logicalRotator;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private float _turnSpeed = 10f;
        [SerializeField] private LayerMask _environmentLayerMask;

        public event Action OnBuildStarted;
        public event Action<Vector3> OnShotFired;

        public Transform LogicalRotator => _logicalRotator;
        public Transform FirePoint => _firePoint;
        public Transform ElevationPivot; // Для зенитки. У обычных башен оставляем None!
        public LayerMask EnemyMask => _enemyLayerMask;
        public float TurnSpeed => _turnSpeed;
        public LayerMask EnvironmentMask => _environmentLayerMask;

        // 1. УБИРАЕМ IInstantiator, ДОБАВЛЯЕМ ProjectileFactory
        private ProjectileFactory _projectileFactory;
        
        public AttackController ActiveController { get; private set; }

        [Inject]
        public void Construct(ProjectileFactory projectileFactory)
        {
            // 2. Zenject прокинет сюда нашу глобальную фабрику снарядов при спавне башни
            _projectileFactory = projectileFactory;
        }

        public ITowerBehavior CreateBehavior()
        {
            // 3. Передаем фабрику в контроллер
            ActiveController = new AttackController(this, _projectileFactory);
            return ActiveController;
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

            if (Application.isPlaying && ActiveController != null)
            {
                ActiveController.AimStrategy?.DrawAimGizmo(_logicalRotator != null ? _logicalRotator : transform, drawRange);
            }
        }
    }
}
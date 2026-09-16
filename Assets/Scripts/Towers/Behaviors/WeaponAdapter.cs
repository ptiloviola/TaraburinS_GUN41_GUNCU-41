using UnityEngine;
using System;
using Zenject;
using Gameplay.Towers.Behaviors.Aiming;
using Gameplay.Projectiles.Factories;
using Gameplay.Auras.Factories;
using Gameplay.Towers.Data.Modules;

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
        public Transform ElevationPivot;
        public LayerMask EnemyMask => _enemyLayerMask;
        public float TurnSpeed => _turnSpeed;
        public LayerMask EnvironmentMask => _environmentLayerMask;

        private ProjectileFactory _projectileFactory;
        private AuraZoneFactory _auraFactory;
        
        public AttackController ActiveController { get; private set; }

        [Inject]
        public void Construct(ProjectileFactory projectileFactory, AuraZoneFactory auraFactory)
        {
            _projectileFactory = projectileFactory;
            _auraFactory = auraFactory;
        }

        public ITowerBehavior CreateBehavior()
        {
            ActiveController = new AttackController(this, _projectileFactory, _auraFactory);
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
                var attack = facade.GetCurrentStats().GetModule<AttackStats>();
                if (attack != null) drawRange = attack.Range;
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
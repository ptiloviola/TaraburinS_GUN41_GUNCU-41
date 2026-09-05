using UnityEngine;
using System;
using Zenject;
using Gameplay.Towers.Behaviors.Aiming;

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

        public Transform LogicalRotator => _logicalRotator;
        public Transform FirePoint => _firePoint;
        public LayerMask EnemyMask => _enemyLayerMask;
        public float TurnSpeed => _turnSpeed;

        private IInstantiator _instantiator;
        
        // Ссылка на контроллер для отрисовки Gizmos
        public AttackController ActiveController { get; private set; }

        [Inject]
        public void Construct(IInstantiator instantiator)
        {
            // Zenject прокинет сюда инстанциатор при спавне башни
            _instantiator = instantiator;
        }

        public ITowerBehavior CreateBehavior()
        {
            // Передаем инстанциатор в контроллер
            ActiveController = new AttackController(this, _instantiator);
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

            // Теперь запрашиваем стратегию прицеливания напрямую из живого контроллера, а не ищем компонент
            if (Application.isPlaying && ActiveController != null)
            {
                ActiveController.AimStrategy?.DrawAimGizmo(_logicalRotator != null ? _logicalRotator : transform, drawRange);
            }
        }
    }
}
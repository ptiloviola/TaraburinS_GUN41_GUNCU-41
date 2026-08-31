using UnityEngine;

namespace Gameplay.Enemies.Visuals
{
    public class BallRoller : EnemyVisualsBase
    {
        [Header("Визуал")]
        [SerializeField] private Transform _visualMesh; 
        
        private float _radius;
        private Vector3 _lastPosition;
        private bool _isMoving;

        private const float DefaultRadius = 0.5f;

        protected override void Awake()
        {
            base.Awake(); // Обязательно вызываем метод базы для поиска Facade
            
            if (_visualMesh == null) _visualMesh = transform.Find("Visual");

            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            _radius = sphereCollider != null ? sphereCollider.radius * transform.localScale.x : DefaultRadius;
        }

        protected override void OnEnable()
        {
            base.OnEnable(); // Подписка на FSM происходит здесь
            
            _lastPosition = transform.position;
            _isMoving = false;

            if (_visualMesh != null)
            {
                _visualMesh.localRotation = Quaternion.identity;
            }
        }

        // --- РЕАКЦИИ НА СМЕНУ СОСТОЯНИЙ ---
        protected override void OnMoveStart() => _isMoving = true;
        protected override void OnStunned() => _isMoving = false;
        protected override void OnDeath() => _isMoving = false;
        protected override void OnReachedBase() => _isMoving = false;

        private void Update()
        {
            if (_visualMesh == null || !_isMoving) return;

            Vector3 movement = transform.position - _lastPosition;
            float distance = movement.magnitude;

            if (distance > 0.001f) 
            {
                Vector3 direction = movement.normalized;
                Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);

                float circumference = 2f * Mathf.PI * _radius;
                float angle = (distance / circumference) * 360f;

                _visualMesh.Rotate(rotationAxis, angle, Space.World);
            }

            _lastPosition = transform.position;
        }
    }
}
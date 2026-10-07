using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace Gameplay.Enemies.Visuals.Animators
{
    public class BallRollerAnimator : EnemyVisualsBase
    {
        [Header("Визуал")]
        [SerializeField] private Transform _visualMesh; 
        
        private float _radius;
        private Vector3 _lastPosition;
        private bool _isMoving;
        private Vector3 _initialScale;

        private const float DefaultRadius = 0.5f;

        protected override void Awake()
        {
            base.Awake();
            
            if (_visualMesh == null) 
            {
                _visualMesh = transform.Find("Visual");
            }
            if (_visualMesh != null) _initialScale = _visualMesh.localScale;

            SphereCollider sphereCollider = GetComponent<SphereCollider>();
            _radius = sphereCollider != null ? sphereCollider.radius * transform.localScale.x : DefaultRadius;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            _lastPosition = transform.position;
            _isMoving = false;

            if (_visualMesh != null)
            {
                _visualMesh.localRotation = Quaternion.identity;
                _visualMesh.localScale = _initialScale;
            }
        }


        protected override void OnMoveStart() => _isMoving = true;
        protected override void OnStunned() => _isMoving = false;
        protected override void OnReachedBase() => _isMoving = false;

        public override async UniTask PlayDeathAnimationAsync()
{
        _isMoving = false;
        
        if (_visualMesh != null)
        {
            await _visualMesh.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).AsyncWaitForCompletion();
        }
    }

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
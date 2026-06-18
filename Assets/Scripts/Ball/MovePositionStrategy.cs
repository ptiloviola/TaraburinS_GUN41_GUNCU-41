using UnityEngine;


namespace Bowling.Ball
{
    public class MovePositionStrategy : IThrowStrategy
    {
        private Rigidbody _rb;

        private Vector3 _direction;
        private float _force;
        private bool _isMoving = false;
        private float _frictionDecay = 0.98f;
        private float _movePositionMultiplier = 0.42f;

        private float _ballRadius = 0.5f;


        public Vector3 CurrentVelocity => _direction * _force;

        public MovePositionStrategy(PhysicsConfig config)
        {
            _frictionDecay = config.FrictionDecay;
            _movePositionMultiplier = config.MovePositionMultiplier;

        }

        public void Initialize(Transform transform, Rigidbody rb)
        {
            _rb = rb;
            if (transform.TryGetComponent<SphereCollider>(out SphereCollider collider))
            {
                _ballRadius = collider.radius * transform.localScale.x;
            }
        }
        public void ExecuteThrow(Vector3 direction, float force)
        {
            _direction = direction.normalized;
            _force = force;
            _isMoving = true;
        }

        public void HandleFixedUpdate()
        {
            if (_isMoving)
            {
                Vector3 step = _direction * (_force * _movePositionMultiplier * Time.fixedDeltaTime);
                _rb.MovePosition(_rb.position + step);
                _force *= _frictionDecay;

                Vector3 rotationAxis = Vector3.Cross(Vector3.up, _direction).normalized;
                
                float circumference = 2f * Mathf.PI * _ballRadius;
                float rotationAngle = (step.magnitude / circumference) * 360f;

                Quaternion stepRotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);
                _rb.MoveRotation(_rb.rotation * stepRotation);

                if (_force < 0.1f)
                {
                    ResetStrategy();
                }
            }
            
        }

        public void HandleUpdate()
        {

        }

        public void ResetStrategy()
        {
            _isMoving = false;
            _force = 0f;
        }

        public void StopMotorAndTransferPhysics()
        {
            if (!_isMoving) return;

            _isMoving = false;
            float linearSpeed = _force * _movePositionMultiplier;
            _rb.velocity = _direction * linearSpeed; 
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, _direction).normalized;
            _rb.angularVelocity = rotationAxis * (linearSpeed / _ballRadius);
            _force = 0f;
        }
    }
}


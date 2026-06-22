using UnityEngine;
using Bowling.BowlingPins;

namespace Bowling.Ball
{
    public class MovePositionStrategy : ITickableStrategy
    {
        private Rigidbody _rb;

        private Vector3 _direction;
        private float _force;
        private bool _isMotorActive = false;
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
            SphereCollider collider = transform.GetComponentInChildren<SphereCollider>();
            if (collider != null)
            {
                _ballRadius = collider.radius * collider.transform.lossyScale.x;
            }
            else
            {
                _ballRadius = 0.5f;
            }
        }
        public void ExecuteThrow(Vector3 direction, float force)
        {
            _rb.isKinematic = true;
            _direction = direction.normalized;
            _force = force;
            _isMotorActive = true;
        }

        public void FixedTick()
        {
            if (_isMotorActive)
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

        public void Tick()
        {

        }

        public void ResetStrategy()
        {
            _isMotorActive = false;
            _force = 0f;
        }

        public void HandleCollision(Collision collision)
        {
            if (!_isMotorActive) return;

            if (collision.gameObject.TryGetComponent<BowlingPin>(out BowlingPin pin))
            {
                Vector3 impactVector = CurrentVelocity * 2.0f;
                Rigidbody pinRb = pin.GetComponent<Rigidbody>();
                if (pinRb != null)
                {
                    pinRb.AddForce(impactVector, ForceMode.Impulse);
                }

                _isMotorActive = false;
                _rb.isKinematic = false;

                float linearSpeed = _force * _movePositionMultiplier;
                _rb.velocity = _direction * linearSpeed; 
                Vector3 rotationAxis = Vector3.Cross(Vector3.up, _direction).normalized;
                _rb.angularVelocity = rotationAxis * (linearSpeed / _ballRadius);
                
                _force = 0f;
            }
        }
    }
}


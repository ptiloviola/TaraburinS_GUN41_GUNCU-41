using UnityEngine;

namespace Bowling.Ball
{
    public class LinearVelocityStrategy : IThrowStrategy
    {

        private float _multiplier = 0.5f;


        private Rigidbody _rb;

        public LinearVelocityStrategy(PhysicsConfig config)
        {
            _multiplier = config.VelocityMultiplier;
        }

        public void Initialize(Transform transform, Rigidbody rb)
        {
            _rb = rb;
        }

        public void ExecuteThrow(Vector3 direction, float force)
        {
            Debug.Log($"direction = {direction}, force = {force}");
            _rb.velocity = direction * (force * _multiplier);

        }

        public void ResetStrategy()
        {
            
        }
        public void HandleCollision(Collision collision)
        {

        }
    }
}


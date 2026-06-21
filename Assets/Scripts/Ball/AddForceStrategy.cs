
using UnityEngine;


namespace Bowling.Ball
{
    public class AddForceStrategy : IThrowStrategy
    {
        private Rigidbody _rb;

        private float _addForceMultiplier = 1f;

        public AddForceStrategy(PhysicsConfig config)
        {
            _addForceMultiplier = config.AddForceMultiplier;
        }

        public void Initialize(Transform transform, Rigidbody rb)
        {
            _rb = rb;
            _rb.isKinematic = false;
        }
        public void ExecuteThrow(Vector3 direction, float force)
        {
            _rb.AddForce(direction.normalized * force * _addForceMultiplier, ForceMode.Impulse);
        }

        public void ResetStrategy()
        {
            
        }
        public void HandleCollision(Collision collision)
        {

        }
    }

}

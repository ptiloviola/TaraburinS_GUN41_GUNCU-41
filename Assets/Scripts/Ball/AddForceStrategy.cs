
using UnityEngine;


namespace Bowling.Ball
{
    public class AddForceStrategy : IThrowStrategy
    {
        private Rigidbody _rb;

        public void Initialize(Transform transform, Rigidbody rb)
        {
            _rb = rb;
            _rb.isKinematic = false;
        }
        public void ExecuteThrow(Vector3 direction, float force)
        {
            _rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        }

        public void HandleFixedUpdate()
        {

        }

        public void HandleUpdate()
        {

        }


    }

}

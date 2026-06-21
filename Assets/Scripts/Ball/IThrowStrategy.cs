using UnityEngine;


namespace Bowling.Ball
{
    public interface IThrowStrategy
    {
        void Initialize(Transform transform, Rigidbody rb);

        void ExecuteThrow(Vector3 direction, float force);

        void ResetStrategy();

        void HandleCollision(Collision collision);

    }
}



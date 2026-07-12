using UnityEngine;
namespace Infrastructure.Interfaces
{
    public interface IMover
    {
        float CurrentSpeed { get; }
        void SetDestination(Vector3 target);
        void Stop();
    }
}

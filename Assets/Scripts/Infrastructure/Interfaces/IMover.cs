using UnityEngine;
namespace Infrastructure.Interfaces
{
    // Абстракция движения. Системе ИИ плевать, КАК враг движется, главное — КУДА
    public interface IMover
    {
        float CurrentSpeed { get; }
        void SetDestination(Vector3 target);
        void Stop();
    }
}

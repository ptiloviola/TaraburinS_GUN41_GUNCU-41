using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VacuumSim.Robotics.Contracts
{
    public interface IVacuumMotor
    {
        void MoveForward(float speed);
        void Stop();
        // Теперь поворот требует времени и его можно прервать токеном
        UniTask RotateAsync(float angle, CancellationToken token);
        bool IsMoving { get; }
        Vector3 Position { get; } // Текущая позиция
        Vector3 Forward { get; }  // Куда смотрит "лицо" робота
        void SetSpeedMultiplier(float multiplier);
    }

}
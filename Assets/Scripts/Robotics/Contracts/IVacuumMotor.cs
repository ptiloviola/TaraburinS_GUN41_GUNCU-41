using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace VacuumSim.Robotics.Contracts
{
    public interface IVacuumMotor
    {
        void MoveForward(float speed);
        void Stop();
        UniTask RotateAsync(float angle, CancellationToken token);
        bool IsMoving { get; }
        Vector3 Position { get; }
        Vector3 Forward { get; } 
        void SetSpeedMultiplier(float multiplier);
    }

}
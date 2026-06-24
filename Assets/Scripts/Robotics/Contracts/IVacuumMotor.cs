using System.Threading;
using Cysharp.Threading.Tasks;

namespace VacuumSim.Robotics.Contracts
{
    public interface IVacuumMotor
    {
        void MoveForward(float speed);
        void Stop();
        // Теперь поворот требует времени и его можно прервать токеном
        UniTask RotateAsync(float angle, CancellationToken token);
        bool IsMoving { get; }
    }

}
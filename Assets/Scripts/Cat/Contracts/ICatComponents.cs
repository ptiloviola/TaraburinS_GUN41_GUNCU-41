using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VacuumSim.Pathfinding;

namespace VacuumSim.Cat.Contracts
{
    public interface ICatView
    {
        void PlayIdle();
        void PlayWalk();
        void PlaySitDown();
        void PlayStandUp();
    }

    public interface ICatObstacle
    {
        void UpdateObstaclePosition(Vector3 currentPosition);
        void SetObstacleActive(bool active);
        void ReleaseObstacle();
    }

    public interface ICatTrashProducer
    {
        void ProduceTrash(Vector3 position);
    }

    public interface ICatMotor
    {
        UniTask MoveAlongPathAsync(System.Collections.Generic.List<Node> path, float speed, float rotationSpeed, CancellationToken token);
    }
}
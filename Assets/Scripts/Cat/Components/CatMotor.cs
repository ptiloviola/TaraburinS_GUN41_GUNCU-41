using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VacuumSim.Pathfinding;
using VacuumSim.Cat.Contracts;

namespace VacuumSim.Cat.Components
{
    public class CatMotor : MonoBehaviour, ICatMotor
    {
        public async UniTask MoveAlongPathAsync(List<Node> path, float speed, float rotationSpeed, CancellationToken token)
        {
            foreach (Node node in path)
            {
                if (token.IsCancellationRequested) break;
                
                Vector3 targetPos = new Vector3(node.WorldPosition.x, transform.position.y, node.WorldPosition.z);
                
                while (Vector3.Distance(transform.position, targetPos) > 0.1f)
                {
                    Vector3 direction = (targetPos - transform.position).normalized;
                    if (direction.sqrMagnitude > 0.001f)
                    {
                        Quaternion lookRotation = Quaternion.LookRotation(direction);
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                    }

                    transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
        }
    }
}
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyRoamer : MonoBehaviour
{
    private NavMeshAgent _agent;
    public float roamRadius = 10f; // Радиус поиска новой точки

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        GoToRandomPoint();
    }

    private void Update()
    {
        // Если агент почти дошел до цели, ищем новую
        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            GoToRandomPoint();
        }
    }

    private void GoToRandomPoint()
    {
        // Берем случайную точку в сфере вокруг врага
        Vector3 randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += transform.position;
        
        // Ищем ближайшую точку на NavMesh
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, roamRadius, 1))
        {
            _agent.SetDestination(hit.position);
        }
    }
}
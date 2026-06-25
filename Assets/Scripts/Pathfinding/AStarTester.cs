using UnityEngine;
using VacuumSim.Pathfinding;

public class AStarTester : MonoBehaviour
{
    [Tooltip("Кто ищет путь (Пылесос)")]
    public Transform Seeker;
    
    [Tooltip("Цель (База)")]
    public Transform Target;
    
    [Tooltip("Ссылка на матрицу")]
    public PathfindingGrid Grid;

    private Pathfinder _pathfinder;

    private void Start()
    {
        // Пока без Zenject, просто инициализируем вручную для теста
        _pathfinder = new Pathfinder(Grid);
    }

    private void Update()
    {
        if (Seeker != null && Target != null && Grid != null)
        {
            // Каждый кадр просим алгоритм перестроить путь
            Grid.CurrentPath = _pathfinder.FindPath(Seeker.position, Target.position);
        }
    }
}

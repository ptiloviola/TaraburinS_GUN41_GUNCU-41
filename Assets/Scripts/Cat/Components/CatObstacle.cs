using UnityEngine;
using Zenject;
using VacuumSim.Pathfinding;
using Cysharp.Threading.Tasks;
using VacuumSim.Cat.Contracts;

namespace VacuumSim.Cat.Components
{
    public class CatObstacle : MonoBehaviour, ICatObstacle
    {
        [Inject] private PathfindingGrid _grid;
        
        private Node _lastOccupiedNode;
        private bool _isActive = true;
        
        // Ссылка на физический коллайдер кота
        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            
            // Прячемся от стартового сканирования сетки!
            if (_collider != null) _collider.enabled = false; 
        }

        private async void Start()
        {
            // Ждем 5 кадров, чтобы PathfindingGrid гарантированно собрал статичную карту комнаты
            await UniTask.DelayFrame(5);
            
            // Торжественно появляемся в физическом мире для сенсоров робота
            if (_collider != null) _collider.enabled = true;
        }

        public void UpdateObstaclePosition(Vector3 currentPosition)
        {
            if (!_isActive || _grid == null) return;

            Node currentNode = _grid.NodeFromWorldPoint(currentPosition);

            if (currentNode != _lastOccupiedNode)
            {
                ReleaseObstacle();

                if (currentNode != null && currentNode.IsWalkable)
                {
                    _lastOccupiedNode = currentNode;
                    _lastOccupiedNode.IsWalkable = false; 
                }
            }
        }

        public void SetObstacleActive(bool active)
        {
            _isActive = active;
            if (!_isActive) ReleaseObstacle();
        }

        public void ReleaseObstacle()
        {
            if (_lastOccupiedNode != null)
            {
                _lastOccupiedNode.IsWalkable = true;
                _lastOccupiedNode = null;
            }
        }

        private void OnDisable() => ReleaseObstacle();
    }
}
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using VacuumSim.Pathfinding;
using VacuumSim.Cat.Contracts;

namespace VacuumSim.Cat.Brain
{
    public class CatBrain : MonoBehaviour
    {
        [Inject] private PathfindingGrid _grid;
        [Inject] private Pathfinder _pathfinder;

        [Header("Настройки баланса кота")]
        [SerializeField] private float _moveSpeed = 1.2f;
        [SerializeField] private float _rotationSpeed = 6.0f;

        private ICatView _view;
        private ICatObstacle _obstacle;
        private ICatTrashProducer _trashProducer;
        private ICatMotor _motor;

        private CancellationTokenSource _cts;

        private void Awake()
        {
            _view = GetComponent<ICatView>();
            _obstacle = GetComponent<ICatObstacle>();
            _trashProducer = GetComponent<ICatTrashProducer>();
            _motor = GetComponent<ICatMotor>();
        }

        private void Start()
        {
            StartBrain();
        }

        public void StartBrain()
        {
            StopBrain();
            _cts = new CancellationTokenSource();
            CatLifeCycleAsync(_cts.Token).Forget();
        }

        public void StopBrain()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        private void Update()
        {
            _obstacle.UpdateObstaclePosition(transform.position);
        }

        private async UniTask CatLifeCycleAsync(CancellationToken token)
        {
            await UniTask.Delay(2000, cancellationToken: token);

            while (!token.IsCancellationRequested)
            {
                await WanderPhaseAsync(token);
                await RestPhaseAsync(token);
                await PoopPhaseAsync(token);
            }
        }

        private async UniTask WanderPhaseAsync(CancellationToken token)
        {
            _view.PlayWalk();

            _obstacle.SetObstacleActive(false);
            Node targetNode = GetRandomWalkableNode();
            List<Node> path = targetNode != null ? _pathfinder.FindPath(transform.position, targetNode.WorldPosition) : null;
            _obstacle.SetObstacleActive(true);

            if (path != null && path.Count > 0)
            {
                await _motor.MoveAlongPathAsync(path, _moveSpeed, _rotationSpeed, token);
            }
        }

        private async UniTask RestPhaseAsync(CancellationToken token)
        {
            _view.PlaySitDown();
            float restTime = Random.Range(4f, 7f);
            await UniTask.Delay(System.TimeSpan.FromSeconds(restTime), cancellationToken: token);
        }

        private async UniTask PoopPhaseAsync(CancellationToken token)
        {
            _view.PlayStandUp();
            await UniTask.Delay(1000, cancellationToken: token);

            _trashProducer.ProduceTrash(transform.position);

            _view.PlayIdle();
            await UniTask.Delay(1500, cancellationToken: token);
        }

        private Node GetRandomWalkableNode()
        {
            List<Node> walkableNodes = new List<Node>();
            for (int x = 0; x < _grid.GridSizeX; x++)
            {
                for (int y = 0; y < _grid.GridSizeY; y++)
                {
                    Node n = _grid.GetNodeFromIndices(x, y);
                    if (n.IsWalkable) walkableNodes.Add(n);
                }
            }
            return walkableNodes.Count > 0 ? walkableNodes[Random.Range(0, walkableNodes.Count)] : null;
        }

        private void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }
    }
}
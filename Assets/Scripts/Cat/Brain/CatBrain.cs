using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using VacuumSim.Pathfinding;
using VacuumSim.Trash; // Добавили пространство имен для мусора

namespace VacuumSim.Cat.Brain
{
    public class CatBrain : MonoBehaviour
    {
        [Inject] private PathfindingGrid _grid;
        [Inject] private Pathfinder _pathfinder;

        [Header("Настройки кота")]
        [SerializeField] private Animator _animator;
        [SerializeField] private float _moveSpeed = 1.2f;
        [SerializeField] private float _rotationSpeed = 6.0f;
        
        // ИСПРАВЛЕНИЕ 1: Теперь кот принимает серьезный тип мусора вместо сырого префаба
        [SerializeField] private TrashType _catTrashType; 

        private CancellationTokenSource _cts;
        private Node _lastOccupiedNode; // Ссылка на клетку, которую кот занял собой

        private void Start()
        {
            _cts = new CancellationTokenSource();
            CatLifeCycleAsync(_cts.Token).Forget();
        }

        // ИСПРАВЛЕНИЕ 2: Динамическое обновление препятствия каждый кадр
        private void Update()
        {
            if (_grid == null) return;

            // Определяем, на какой ячейке кот находится физически прямо сейчас
            Node currentNode = _grid.NodeFromWorldPoint(transform.position);

            if (currentNode != _lastOccupiedNode)
            {
                // Освобождаем старую клетку (робот снова может через нее ехать)
                if (_lastOccupiedNode != null)
                {
                    _lastOccupiedNode.IsWalkable = true;
                }

                // Занимаем новую клетку (делаем её стеной для робота)
                if (currentNode != null && currentNode.IsWalkable)
                {
                    _lastOccupiedNode = currentNode;
                    _lastOccupiedNode.IsWalkable = false; 
                }
            }
        }

        private async UniTask CatLifeCycleAsync(CancellationToken token)
        {
            await UniTask.Delay(2000, cancellationToken: token);

            while (!token.IsCancellationRequested)
            {
                await WanderAsync(token);
                await SitAndRestAsync(token);
                await PoopAndStandAsync(token);
            }
        }

        private async UniTask WanderAsync(CancellationToken token)
        {
            _animator.CrossFade("Walk", 0.2f);

            // ХИТРОСТЬ: Временно разблокируем свою клетку, чтобы алгоритмы поиска пути 
            // не выдали ошибку "Стартовая точка находится внутри стены"
            if (_lastOccupiedNode != null) _lastOccupiedNode.IsWalkable = true;

            Node targetNode = GetRandomWalkableNode();
            List<Node> path = null;

            if (targetNode != null)
            {
                path = _pathfinder.FindPath(transform.position, targetNode.WorldPosition);
            }

            // Сразу же возвращаем блок обратно, чтобы робот не успел проскочить
            if (_lastOccupiedNode != null) _lastOccupiedNode.IsWalkable = false;

            if (path == null || path.Count == 0) return;

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
                        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * _rotationSpeed);
                    }

                    transform.position = Vector3.MoveTowards(transform.position, targetPos, _moveSpeed * Time.deltaTime);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
        }

        private async UniTask SitAndRestAsync(CancellationToken token)
        {
            _animator.CrossFade("SitDown", 0.2f);
            float restTime = Random.Range(4f, 7f);
            await UniTask.Delay(System.TimeSpan.FromSeconds(restTime), cancellationToken: token);
        }

        private async UniTask PoopAndStandAsync(CancellationToken token)
        {
            _animator.CrossFade("StandUp", 0.2f);
            await UniTask.Delay(1000, cancellationToken: token);

            // Спавним мусор на основе настроек нашего ScriptableObject
            if (_catTrashType != null && _catTrashType.Prefab != null)
            {
                Vector3 spawnPos = new Vector3(transform.position.x, _grid.transform.position.y + 1f, transform.position.z);
                
                // Создаем визуальный объект на сцене
                Instantiate(_catTrashType.Prefab, spawnPos, Quaternion.identity);

                // Регистрируем загрязнение в системе
                Node currentNode = _grid.NodeFromWorldPoint(spawnPos);
                if (currentNode != null)
                {
                    currentNode.HasTrash = true;
                    currentNode.IsCleaned = false; 
                }
                
                Debug.Log($"<color=magenta>[Cat] Кот оставил мусор типа: {_catTrashType.Title}!</color>");
            }

            _animator.CrossFade("Idle", 0.2f);
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

            if (walkableNodes.Count > 0)
            {
                return walkableNodes[Random.Range(0, walkableNodes.Count)];
            }
            return null;
        }

        private void OnDisable()
        {
            // Обязательно освобождаем клетку, если кота выключили или уничтожили,
            // иначе на сетке останется "невидимая вечная стена".
            if (_lastOccupiedNode != null)
            {
                _lastOccupiedNode.IsWalkable = true;
            }
            
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }
    }
}
using UnityEngine;
using Zenject;
using VacuumSim.Trash;
using VacuumSim.Robotics.Configs;
using VacuumSim.Robotics.Signals;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Pathfinding;

namespace VacuumSim.Robotics.Components
{
    public class VacuumCollector : MonoBehaviour
    {
        [Header("Настройки физики")]
        [SerializeField] private LayerMask _trashMask;
        [SerializeField] private Transform _intakePoint; 

        [Header("Данные для редактора (Gizmos)")]
        [SerializeField] private VacuumConfig _config;

        private readonly Collider[] _hitBuffer = new Collider[10];

        private PathfindingGrid _grid;

        private SignalBus _signalBus;
        private IVacuumDustbin _dustbin;

        [Inject]
        public void Construct(VacuumConfig config, SignalBus signalBus,
            IVacuumDustbin dustbin, PathfindingGrid grid)
        {
            _config = config;
            _signalBus = signalBus;
            _dustbin = dustbin;
            _grid = grid;
        }

        private void FixedUpdate()
        {
            if (_dustbin.IsFull) return;
            int hitCount = Physics.OverlapSphereNonAlloc(
                _intakePoint.position, 
                _config.IntakeRadius, 
                _hitBuffer, 
                _trashMask
            );

            for (int i = 0; i < hitCount; i++)
            {
                if (_dustbin.IsFull) break;
                Collider hit = _hitBuffer[i];

                if (hit.TryGetComponent<TrashItem>(out var trash))
                {
                    Debug.Log($"[Collector] Всосали: {trash.Type.Title}!");
                    _signalBus.Fire(new TrashCollectedSignal { TrashData = trash.Type });
                    Destroy(hit.gameObject);
                    if (_grid != null)
                    {
                        Node node = _grid.NodeFromWorldPoint(transform.position);
                        if (node != null)
                        {
                            node.HasTrash = false;
                        }
                    }
                }
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_config == null || _intakePoint == null) return;
            Gizmos.color = new Color(0, 1, 0, 0.3f); 
            Gizmos.DrawSphere(_intakePoint.position, _config.IntakeRadius);
        }
        #endif
    }
}
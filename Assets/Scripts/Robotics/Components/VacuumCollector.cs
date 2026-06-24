using UnityEngine;
using Zenject;
using VacuumSim.Trash;
using VacuumSim.Robotics.Configs;
using VacuumSim.Robotics.Signals;
using VacuumSim.Robotics.Contracts;

namespace VacuumSim.Robotics.Components
{
    public class VacuumCollector : MonoBehaviour
    {
        [Header("Настройки физики")]
        [SerializeField] private LayerMask _trashMask;
        [SerializeField] private Transform _intakePoint; 

        [Header("Данные для редактора (Gizmos)")]
        [SerializeField] private VacuumConfig _config;

        // ОПТИМИЗАЦИЯ: Создаем фиксированный буфер в памяти ОДИН раз.
        // Пылесос вряд ли засосет больше 10 объектов за ОДИН физический кадр.
        private readonly Collider[] _hitBuffer = new Collider[10];

        private SignalBus _signalBus;
        private IVacuumDustbin _dustbin;

        [Inject]
        public void Construct(VacuumConfig config, SignalBus signalBus,
            IVacuumDustbin dustbin)
        {
            _config = config;
            _signalBus = signalBus;
            _dustbin = dustbin;
        }

        private void FixedUpdate()
        {
            if (_dustbin.IsFull) return;
            // NonAlloc не создает массив, а заполняет наш готовый _hitBuffer.
            // Он возвращает int — количество РЕАЛЬНО найденных объектов.
            int hitCount = Physics.OverlapSphereNonAlloc(
                _intakePoint.position, 
                _config.IntakeRadius, 
                _hitBuffer, 
                _trashMask
            );

            // Идем циклом for только по тем элементам, которые реально нашли
            for (int i = 0; i < hitCount; i++)
            {
                if (_dustbin.IsFull) break;
                Collider hit = _hitBuffer[i];

                if (hit.TryGetComponent<TrashItem>(out var trash))
                {
                    Debug.Log($"[Collector] Всосали: {trash.Type.Title}!");
                    _signalBus.Fire(new TrashCollectedSignal { TrashData = trash.Type });
                    Destroy(hit.gameObject);
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
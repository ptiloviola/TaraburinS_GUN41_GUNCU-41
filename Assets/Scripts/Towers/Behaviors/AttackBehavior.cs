using Gameplay.Towers.Data;
using UnityEngine;
using Gameplay.Towers.Data.Modules;
using Gameplay.Towers.Visuals;
using Unity.VisualScripting;

namespace Gameplay.Towers.Behaviors
{
    // Модуль является MonoBehaviour, значит мы можем легко повесить его на префаб
    // и привязать к нему точку вылета снаряда (FirePoint) прямо в инспекторе!
    public class AttackBehavior : MonoBehaviour, ITowerBehavior
    {

        [Header("Прицеливание")]
        [SerializeField] private Transform _logicalRotator; // Невидимая ось вращения
        [SerializeField] private Transform _firePoint;      // Точка вылета снаряда (внутри ротатора)
        [SerializeField] private LayerMask _enemyLayerMask;
        [SerializeField] private float _turnSpeed = 10f;    // Скорость поворота



        
        private TowerFacade _facade;
        private ITowerVisuals _visuals;




        private Transform _currentTarget;
        private float _cooldownTimer;

        public void Initialize(TowerFacade facade)
        {
            _facade = facade;
            _cooldownTimer = 0f;
            _visuals = GetComponentInChildren<ITowerVisuals>();

            _visuals?.Initialize();
            if (_logicalRotator == null) _logicalRotator = transform;
            if (_firePoint == null) _firePoint = _logicalRotator;

            // 2. ЗАПУСКАЕМ АНИМАЦИЮ ПОЯВЛЕНИЯ!
            // Теперь башня плавно вырастет из земли до размера (1,1,1)
            _visuals?.PlayBuildAnimation();
            
        }

        public void Tick()
        {
            AttackStats stats = _facade.GetCurrentStats().Attack;
            if (stats == null) return;

            _cooldownTimer -= Time.deltaTime;

            // 1. Поиск цели (если нет текущей, или она выключена/умерла, или ушла слишком далеко)
            if (!IsTargetValid(stats.Range))
            {
                FindClosestTarget(stats.Range);
            }

            // 2. Если цель есть — поворачиваемся и стреляем
            if (_currentTarget != null)
            {
                AimAtTarget();

                // Стреляем, если прошла перезарядка И дуло смотрит на врага
                if (_cooldownTimer <= 0f && IsFacingTarget())
                {
                    ExecuteShot();
                    _cooldownTimer = stats.Cooldown;
                }
            }
            
        }

        private bool IsTargetValid(float range)
        {
            if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy) return false;
            // Проверяем, не ушел ли враг за радиус поражения (используем квадраты дистанций для оптимизации)
            float sqrDistance = (_currentTarget.position - _logicalRotator.position).sqrMagnitude;
            return sqrDistance <= (range * range);
        }

        private void FindClosestTarget(float range)
        {
            _currentTarget = null;
            Collider[] hits = Physics.OverlapSphere(_logicalRotator.position, range, _enemyLayerMask);
            
            float closestSqrDistance = Mathf.Infinity;

            foreach (var hit in hits)
            {
                float sqrDistance = (hit.transform.position - _logicalRotator.position).sqrMagnitude;
                if (sqrDistance < closestSqrDistance)
                {
                    closestSqrDistance = sqrDistance;
                    _currentTarget = hit.transform;
                }
            }
        }

        private void AimAtTarget()
        {
            // Направляем вектор на врага, игнорируя высоту (чтобы башня крутилась только влево-вправо)
            Vector3 direction = _currentTarget.position - _logicalRotator.position;
            direction.y = 0f;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                _logicalRotator.rotation = Quaternion.Slerp(_logicalRotator.rotation, targetRotation, _turnSpeed * Time.deltaTime);
            }
        }

        private bool IsFacingTarget()
        {
            Vector3 directionToTarget = (_currentTarget.position - _logicalRotator.position).normalized;
            directionToTarget.y = 0f;
            
            // Dot Product > 0.99 означает, что угол между направлением пушки и врагом меньше ~8 градусов
            return Vector3.Dot(_logicalRotator.forward, directionToTarget) > 0.99f;
        }

        private void ExecuteShot()
        {
            Debug.Log($"<color=red>[AttackBehavior] Выстрел по {_currentTarget.name}!</color>");
            Debug.DrawRay(_firePoint.position, _logicalRotator.forward * 5f, Color.red, 0.2f);
            
            // Дергаем API анимации (сейчас сработает Dummy, потом — настоящая анимация)
            _visuals?.PlayShootAnimation();
        }

    }
}
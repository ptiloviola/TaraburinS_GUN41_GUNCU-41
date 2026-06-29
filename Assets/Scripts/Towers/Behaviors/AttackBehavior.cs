using UnityEngine;
using Gameplay.Towers.Data.Modules;
using Gameplay.Towers.Visuals;
using Gameplay.Towers.Behaviors.Weapons;
// НОВОЕ: Подключаем пространство имен стратегий прицеливания
using Gameplay.Towers.Behaviors.Aiming;
using Gameplay.Projectiles.Contracts;

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

        // Добавь это в начало класса, где объявляются переменные:
        private IAttackExecutor _attackExecutor;
        private IAimStrategy _aimStrategy; // НОВОЕ: Ссылка на стратегию прицеливания


        private TowerFacade _facade;
        private ITowerVisuals _visuals;

        private Transform _currentTarget;
        private float _cooldownTimer;

        private AttackStats _currentStats;

        private Collider[] _targetColliders = new Collider[20];

        public void Initialize(TowerFacade facade)
        {
            _facade = facade;
            _cooldownTimer = 0f;
            _currentStats = _facade.GetCurrentStats().Attack;
            _visuals = GetComponentInChildren<ITowerVisuals>();

            _visuals?.Initialize();
            if (_logicalRotator == null) _logicalRotator = transform;
            if (_firePoint == null) _firePoint = _logicalRotator;

            // 2. ЗАПУСКАЕМ АНИМАЦИЮ ПОЯВЛЕНИЯ!
            // Теперь башня плавно вырастет из земли до размера (1,1,1)
            _visuals?.PlayBuildAnimation();

            // Добавь это в метод Initialize():
            _attackExecutor = GetComponentInChildren<IAttackExecutor>();
            if (_attackExecutor == null)
            {
                Debug.LogError($"[AttackBehavior] На башне {gameObject.name} нет компонента IAttackExecutor (Оружия)!");
            }
            // НОВОЕ: Ищем стратегию прицеливания
            _aimStrategy = GetComponentInChildren<IAimStrategy>();
            if (_aimStrategy == null)
            {
                Debug.LogError($"[AttackBehavior] На башне {gameObject.name} нет компонента IAimStrategy (Прицеливания)!");
            }
            
        }

        public void Tick()
        {
            if (_currentStats == null) return;

            _cooldownTimer -= Time.deltaTime;

            // 1. Поиск цели (если нет текущей, или она выключена/умерла, или ушла слишком далеко)
            if (!IsTargetValid(_currentStats.Range))
            {
                FindClosestTarget(_currentStats.Range);
            }

            // 2. Если цель есть — поворачиваемся и стреляем
            if (_currentTarget != null)
            {
                AimAtTarget();

                // Стреляем, если прошла перезарядка И дуло смотрит на врага
                if (_cooldownTimer <= 0f && IsFacingTarget())
                {
                    // ИСПРАВЛЕНО: Передаем весь объект _currentStats, а не только Damage
                    ExecuteShot(_currentStats);
                    _cooldownTimer = _currentStats.Cooldown;
                }
            }
            
        }

        private bool IsTargetValid(float range)
        {
            if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy) return false;
            
            float sqrDistance = (_currentTarget.position - _logicalRotator.position).sqrMagnitude;
            if (sqrDistance > (range * range)) return false;

            // СБРОС ЦЕЛИ: Если враг вылетел за пределы нашего угла (например, резко взлетел вверх)
            if (_aimStrategy != null && !_aimStrategy.CanAimAt(_logicalRotator, _currentTarget)) return false;

            return true;
        }

        private void FindClosestTarget(float range)
        {
            _currentTarget = null;

            int hitsCount = Physics.OverlapSphereNonAlloc(_logicalRotator.position, range, _targetColliders, _enemyLayerMask);
            
            float closestSqrDistance = Mathf.Infinity;

            for (int i = 0; i < hitsCount; i++)
            {
                Collider hit = _targetColliders[i];
                float sqrDistance = (hit.transform.position - _logicalRotator.position).sqrMagnitude;
                
                // Сначала проверяем дистанцию
                if (sqrDistance < closestSqrDistance)
                {
                    // А ТЕПЕРЬ САМОЕ ГЛАВНОЕ: Проверяем, позволяет ли физический угол башни выстрелить туда
                    if (_aimStrategy == null || _aimStrategy.CanAimAt(_logicalRotator, hit.transform))
                    {
                        closestSqrDistance = sqrDistance;
                        _currentTarget = hit.transform;
                    }
                }
            }
        }

        // НОВОЕ: Делегируем поворот стратегии
        private void AimAtTarget()
        {
            _aimStrategy?.AimAtTarget(_logicalRotator, _currentTarget, _turnSpeed);
        }

        // НОВОЕ: Делегируем проверку угла стратегии
        private bool IsFacingTarget()
        {
            if (_aimStrategy == null) return false;
            return _aimStrategy.IsFacingTarget(_logicalRotator, _currentTarget);
        }

        private void ExecuteShot(AttackStats stats)
        {
            Debug.Log($"<color=red>[AttackBehavior] Выстрел по {_currentTarget.name}!</color>");
            Debug.DrawRay(_firePoint.position, _logicalRotator.forward * 5f, Color.red, 0.2f);
            
            // ИСПРАВЛЕНО: ЗАЩИТА ОТ NULL
            if (stats.PayloadStrategy == null)
            {
                Debug.LogError($"[AttackBehavior] КРИТИЧЕСКАЯ ОШИБКА: На башне {gameObject.name} не назначен PayloadStrategy! Закиньте ScriptableObject в поле AttackStats.");
                return;
            }
            // 1. Просим конфиг собрать нам посылку
            IProjectilePayload payload = stats.PayloadStrategy.CreatePayload(stats.Damage);
            
            _attackExecutor?.ExecuteAttack(_currentTarget, payload, _firePoint);

            // Передаем точные мировые координаты врага на момент выстрела
            _visuals?.PlayShootAnimation(_currentTarget.position);
        }

        // --- БЛОК ДЕБАГА И ВИЗУАЛИЗАЦИИ ---
        
        // Сохраняем радиус для отрисовки, так как Gizmos работает даже на паузе
        private float _debugRange = 5f; 

        private void OnDrawGizmos()
        {
            float drawRange = 5f; 
            if (Application.isPlaying && _facade != null && _facade.GetCurrentStats() != null)
            {
                drawRange = _facade.GetCurrentStats().Attack.Range;
            }

            Vector3 center = _logicalRotator != null ? _logicalRotator.position : transform.position;

            // Рисуем границы радара
            Gizmos.color = _currentTarget != null ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(center, drawRange);

            // ---> ВОТ ЭТОТ БЛОК ОТВЕЧАЕТ ЗА ЛУЧИ УГЛОВ <---
            if (Application.isPlaying && _aimStrategy != null)
            {
                _aimStrategy.DrawAimGizmo(_logicalRotator != null ? _logicalRotator : transform, drawRange);
            }
            else
            {
                // Для отрисовки прямо в редакторе (когда игра на паузе)
                GetComponent<IAimStrategy>()?.DrawAimGizmo(_logicalRotator != null ? _logicalRotator : transform, drawRange);
            }
            // ----------------------------------------------

            if (_currentTarget != null && _firePoint != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(_firePoint.position, _currentTarget.position);
            }
        }

    }
}
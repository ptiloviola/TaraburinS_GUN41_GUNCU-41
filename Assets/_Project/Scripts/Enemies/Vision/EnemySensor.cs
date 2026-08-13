using UnityEngine;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.Vision
{
    public class EnemySensor
    {
        private readonly EnemyBrain _brain;
        
        // Массив для OverlapSphereNonAlloc, чтобы не генерировать мусор (Garbage Collection)
        private readonly Collider[] _colliders = new Collider[2]; 
        
        public bool IsTargetVisible { get; private set; }

        public EnemySensor(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Tick()
        {
            IsTargetVisible = false;
            
            if (_brain.Target == null) return;

            // 1. Ищем объекты на слое TargetMask в радиусе VisionRadius
            int count = Physics.OverlapSphereNonAlloc(_brain.transform.position, _brain.Config.VisionRadius, _colliders, _brain.Config.TargetMask);
            
            if (count > 0)
            {
                Transform targetTransform = _colliders[0].transform;
                Vector3 dirToTarget = (targetTransform.position - _brain.transform.position).normalized;

                // 2. Проверяем, находится ли цель внутри конуса зрения
                if (Vector3.Angle(_brain.transform.forward, dirToTarget) < _brain.Config.ViewAngle / 2f)
                {
                    float distanceToTarget = Vector3.Distance(_brain.transform.position, targetTransform.position);
                    
                    // 3. Проверка видимости (Linecast/Raycast). 
                    // Поднимаем точку каста на 1.5 метра (уровень глаз), чтобы не стрелять лучом из пяток по полу
                    Vector3 eyePosition = _brain.transform.position + Vector3.up * 1.5f;
                    Vector3 targetEyePosition = targetTransform.position + Vector3.up * 1.5f;

                    if (!Physics.Linecast(eyePosition, targetEyePosition, _brain.Config.ObstacleMask))
                    {
                        // Видим игрока!
                        IsTargetVisible = true;
                        _brain.LastKnownTargetPosition = targetTransform.position;
                    }
                }
            }
        }
    }
}
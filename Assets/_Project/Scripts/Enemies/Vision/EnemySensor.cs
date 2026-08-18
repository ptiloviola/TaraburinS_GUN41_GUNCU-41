using System;
using UnityEngine;
using TpsShooter.Enemies.Core;
using TpsShooter.Environment; // Для GlobalAIEvents

namespace TpsShooter.Enemies.Vision
{
    public class EnemySensor : IDisposable
    {
        private readonly EnemyBrain _brain;
        private readonly Collider[] _colliders = new Collider[2]; 
        
        public bool IsTargetVisible { get; private set; }
        public event Action<Vector3> OnHeardNoise; // Сигнал для Мозга

        public EnemySensor(EnemyBrain brain)
        {
            _brain = brain;
            // Подписываемся на глобальный слух
            GlobalAIEvents.OnNoiseGenerated += HandleGlobalNoise;
        }

        public void Tick()
        {
            IsTargetVisible = false;
            if (_brain.Target == null) return;

            int count = Physics.OverlapSphereNonAlloc(_brain.transform.position, _brain.Config.VisionRadius, _colliders, _brain.Config.TargetMask);
            if (count > 0)
            {
                Transform targetTransform = _colliders[0].transform;
                Vector3 dirToTarget = (targetTransform.position - _brain.transform.position).normalized;

                if (Vector3.Angle(_brain.transform.forward, dirToTarget) < _brain.Config.ViewAngle / 2f)
                {
                    Vector3 eyePosition = _brain.transform.position + Vector3.up * 1.5f;
                    Vector3 targetEyePosition = targetTransform.position + Vector3.up * 1.5f;

                    if (!Physics.Linecast(eyePosition, targetEyePosition, _brain.Config.ObstacleMask))
                    {
                        IsTargetVisible = true;
                        _brain.LastKnownTargetPosition = targetTransform.position;
                    }
                }
            }
        }

        private void HandleGlobalNoise(Vector3 noisePosition, float volume)
        {
            // Если мы уже видим игрока, на слух не отвлекаемся
            if (IsTargetVisible) return;

            float distance = Vector3.Distance(_brain.transform.position, noisePosition);
            if (distance <= _brain.Config.HearingRadius)
            {
                // Слышим! Запоминаем точку и кричим мозгу
                _brain.LastKnownTargetPosition = noisePosition;
                OnHeardNoise?.Invoke(noisePosition);
            }
        }

        public void Dispose()
        {
            GlobalAIEvents.OnNoiseGenerated -= HandleGlobalNoise;
        }
    }
}
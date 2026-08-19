using System;
using UnityEngine;
using TpsShooter.Enemies.Core;
using TpsShooter.Environment; 

namespace TpsShooter.Enemies.Vision
{
    public class EnemySensor : IDisposable
    {
        private readonly EnemyBrain _brain;
        
        public bool IsTargetVisible { get; private set; }
        public event Action<Vector3> OnHeardNoise; 

        public EnemySensor(EnemyBrain brain)
        {
            _brain = brain;
            GlobalAIEvents.OnNoiseGenerated += HandleGlobalNoise;
        }

        public void Tick()
        {
            IsTargetVisible = false;
            
            // Если цели нет, выходим (но если она есть - идем дальше)
            if (_brain.Target == null) return;

            Transform targetTransform = _brain.Target.transform;
            Vector3 myPos = _brain.transform.position;
            Vector3 targetPos = targetTransform.position;

            float distanceToTarget = Vector3.Distance(myPos, targetPos);

            // Точки для глаз (поднимаем на 1.5 метра от пола)
            Vector3 eyePosition = myPos + Vector3.up * 1.5f;
            Vector3 targetEyePosition = targetPos + Vector3.up * 1.5f;
            Vector3 dirToTarget = (targetEyePosition - eyePosition).normalized;

            // 1. Проверка дистанции
            if (distanceToTarget <= _brain.Config.VisionRadius)
            {
                float angle = Vector3.Angle(_brain.transform.forward, dirToTarget);

                // 2. Проверка угла
                if (angle < _brain.Config.ViewAngle / 2f)
                {
                    // 3. Проверка препятствий (Raycast со смещением на 0.5 метра вперед, чтобы не попасть в самого себя)
                    if (Physics.Raycast(eyePosition + dirToTarget * 0.5f, dirToTarget, out RaycastHit hit, distanceToTarget, _brain.Config.ObstacleMask))
                    {
                        Debug.DrawLine(eyePosition, hit.point, Color.red);
                        // ЭТОТ ЛОГ СКАЖЕТ НАМ ВСЮ ПРАВДУ:
                        DevLogger.Log($"<color=red>[Sensor]</color> Не вижу! Врезался в: {hit.collider.gameObject.name} (Слой: {LayerMask.LayerToName(hit.collider.gameObject.layer)})");
                    }
                    else
                    {
                        Debug.DrawLine(eyePosition, targetEyePosition, Color.green);
                        IsTargetVisible = true;
                        _brain.LastKnownTargetPosition = targetPos;
                    }
                }
                else
                {
                    // Рядом, но не в зоне угла
                    Debug.DrawLine(eyePosition, targetEyePosition, Color.yellow);
                }
            }
            else
            {
                // Слишком далеко
                Debug.DrawLine(eyePosition, targetEyePosition, Color.gray);
            }
        }

        private void HandleGlobalNoise(Vector3 noisePosition, float volume)
        {
            if (IsTargetVisible) return;

            float distance = Vector3.Distance(_brain.transform.position, noisePosition);
            if (distance <= _brain.Config.HearingRadius)
            {
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
using System;
using UnityEngine;
using TpsShooter.Enemies.Core;
using TpsShooter.Environment; 

namespace TpsShooter.Enemies.Vision
{
    public class EnemySensor : IDisposable
    {
        private const float RaycastForwardOffset = 0.5f;
        private const int MaxTargets = 3;

        private readonly EnemyBrain _brain;
        private readonly Collider[] _targetColliders = new Collider[MaxTargets];
        
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

            Vector3 myPos = _brain.transform.position;
            Vector3 eyePosition = myPos + Vector3.up * _brain.Config.EyeHeight;

            int hits = Physics.OverlapSphereNonAlloc(myPos, _brain.Config.VisionRadius, _targetColliders, _brain.Config.TargetMask);

            for (int i = 0; i < hits; i++)
            {
                Transform targetTransform = _targetColliders[i].transform;
                Vector3 targetPos = targetTransform.position;
                Vector3 targetEyePosition = targetPos + Vector3.up * _brain.Config.EyeHeight;
                
                Vector3 dirToTarget = (targetEyePosition - eyePosition).normalized;
                float distanceToTarget = Vector3.Distance(myPos, targetPos);

                float angle = Vector3.Angle(_brain.transform.forward, dirToTarget);

                if (angle < _brain.Config.ViewAngle / 2f)
                {
                    if (Physics.Raycast(eyePosition + dirToTarget * RaycastForwardOffset, dirToTarget, out RaycastHit hit, distanceToTarget, _brain.Config.ObstacleMask))
                    {
#if UNITY_EDITOR
                        Debug.DrawLine(eyePosition, hit.point, Color.red);
#endif
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.DrawLine(eyePosition, targetEyePosition, Color.green);
#endif
                        IsTargetVisible = true;
                        _brain.LastKnownTargetPosition = targetPos;
                        
                        break; 
                    }
                }
                else
                {
#if UNITY_EDITOR

                    Debug.DrawLine(eyePosition, targetEyePosition, Color.yellow);
#endif
                }
            }


#if UNITY_EDITOR
            if (!IsTargetVisible && _brain.Target != null)
            {
                Debug.DrawLine(eyePosition, _brain.Target.transform.position + Vector3.up * _brain.Config.EyeHeight, Color.gray);
            }
#endif
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
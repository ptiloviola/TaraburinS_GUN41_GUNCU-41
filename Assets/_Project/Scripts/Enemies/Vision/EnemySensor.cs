using UnityEngine;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.Vision
{
    public class EnemySensor
    {
        private readonly EnemyBrain _brain;
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
    }
}
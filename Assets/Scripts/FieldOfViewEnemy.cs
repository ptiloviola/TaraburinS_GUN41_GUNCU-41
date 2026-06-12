using System.Collections;
using UnityEngine;

namespace HomeWork.FieldOfView
{
    public class FieldOfViewEnemy : MonoBehaviour
    {
        [SerializeField] private float _radius;
        [SerializeField, Range(0, 360)] private float _angle;
        [SerializeField] private float _viewOffset;
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private LayerMask _obstacleMask;

        [SerializeField] Player _player;

        private bool _canSeePlayer;
        public Vector3 CenterOfView => transform.position + new Vector3(0, _viewOffset, 0);

        private void Start()
        {
            StartCoroutine(FovRoutine());
        }

        private IEnumerator FovRoutine()
        {
            WaitForSeconds wait = new WaitForSeconds(0.2f);
            while (_player != null)
            {
                yield return wait;
                FielOfViewCheck();
            }
        }

        private Vector3 DirectionFromAngle(float angleInDegrees)
        {
            angleInDegrees += transform.eulerAngles.y;
            float radius = angleInDegrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radius), 0, Mathf.Cos(radius));
        }

        private void FielOfViewCheck()
        {
            Collider[] rangeChecks = Physics.OverlapSphere(transform.position, _radius, _targetMask);
            if (rangeChecks.Length > 0)
            {
                Transform target = rangeChecks[0].transform;
                Vector3 directionToTarget = (target.position - CenterOfView).normalized;
                if (Vector3.Angle(transform.forward, directionToTarget) < _angle/2)
                {
                    float distanceToTarget = Vector3.Distance(target.position, transform.position);
                    if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, _obstacleMask))
                    {
                        _canSeePlayer = true;
                    }
                    else
                    {
                        _canSeePlayer = false;
                    }
                }
                else
                {
                    _canSeePlayer = false;
                }
            }
            else if (_canSeePlayer)
            {
                _canSeePlayer = false;
            }
        }

        void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                FielOfViewCheck();
            }
            if (!_canSeePlayer)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = Color.red;
            }
            
            Gizmos.DrawWireSphere(CenterOfView, _radius);
            Vector3 leftDirection = DirectionFromAngle(-_angle/2);
            Vector3 rightDirection = DirectionFromAngle(_angle/2);

            Gizmos.DrawRay(CenterOfView, leftDirection * _radius);
            Gizmos.DrawRay(CenterOfView, rightDirection * _radius);
        }

    }
}



using System.Collections;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float _radius;
    [SerializeField, Range(0, 360)] private float _angle;

    [SerializeField] private Player _player;
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private LayerMask _obstacleMask;

    private bool _canSeePlayer;
    public bool CanSeePlayer => _canSeePlayer;
    public float Radius => _radius;
    public float Angle => _angle;

    public Player Player => _player;

    private void Start()
    {
        print("Start()");
        StartCoroutine(FovRoutine());
    }

    private IEnumerator FovRoutine()
    {
        print("FovRoutine()");
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while(_player != null)
        {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck()
    {
        print("FieldOfViewCheck()");
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, _radius, _targetMask);

        if (rangeChecks.Length > 0)
        {
            print("rangeChecks.Length");
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < _angle/2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
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

}

using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Targets & Offset")]
    [SerializeField] private Transform _target;
    private Vector3 _offset;
    [Header("Smoothing")]
    [SerializeField] private float _smoothTime = 0.3f;
    private Vector3 _velocity = Vector3.zero;

    void Start()
    {
        _offset = transform.position - _target.position;
    }

    private void LateUpdate()
    {
        if (_target == null) return;
        Vector3 desiredPosition = _target.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _velocity, _smoothTime);
    }
}

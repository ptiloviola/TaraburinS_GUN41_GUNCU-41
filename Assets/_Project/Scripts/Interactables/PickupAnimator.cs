using UnityEngine;

namespace TpsShooter.Interactables
{
    public class PickupAnimator : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 45f;
        [SerializeField] private float _bobAmplitude = 0.2f;
        [SerializeField] private float _bobFrequency = 1f;

        private Vector3 _startPos;

        private void Start()
        {
            _startPos = transform.position;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime, Space.World);
            
            float newY = _startPos.y + Mathf.Sin(Time.time * Mathf.PI * _bobFrequency) * _bobAmplitude;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
}
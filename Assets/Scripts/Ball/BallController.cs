using UnityEngine;


namespace Bowling.Ball
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : MonoBehaviour
    {
        private Rigidbody _rb;
        private IThrowStrategy _currentStrategy;

        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }

        public void SetStrategy(IThrowStrategy throwStrategy)
        {
            _currentStrategy = throwStrategy;
            _currentStrategy.Initialize(transform, _rb);
        }

        public void ThrowBall(Vector3 direction, float force)
        {
            if (_currentStrategy != null)
            {
                _currentStrategy.ExecuteThrow(direction, force);
            }
            else
            {
                Debug.LogWarning("Не выбрана стратегия броска");
            }
        }

        private void FixedUpdate()
        {
            _currentStrategy?.HandleFixedUpdate();
        }

        private void Update()
        {
            _currentStrategy?.HandleUpdate();
        }

        public void ResetBall()
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.WakeUp();
            transform.position = _startPosition;
            transform.rotation = _startRotation;
        }

    }
}


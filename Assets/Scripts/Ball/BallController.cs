using UnityEngine;

namespace Bowling.Ball
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : MonoBehaviour
    {
        private Rigidbody _rb;
        private IThrowStrategy _currentStrategy;
        private ITickableStrategy _tickableStrategy;

        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private GameObject _currentVisualChild;

        private Vector3 _lastPosition;
        private float _kinematicSpeed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }


        public void SetStrategy(IThrowStrategy strategy)
        {
            _currentStrategy = strategy;
            _tickableStrategy = strategy as ITickableStrategy;
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
            if (_rb != null && _rb.isKinematic)
            {
                _kinematicSpeed = Vector3.Distance(_rb.position, _lastPosition) / Time.fixedDeltaTime;
                _lastPosition = _rb.position;
            }
            _tickableStrategy?.FixedTick();
        }

        private void Update()
        {
            _tickableStrategy?.Tick();
            
        }

        public void ResetBall()
        {
            _currentStrategy?.ResetStrategy();
            if (_rb != null) _rb.isKinematic = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.position = _startPosition;
            _rb.rotation = _startRotation;
            _rb.WakeUp();
            _rb.isKinematic = false;
            _lastPosition = _rb.position; 
            _kinematicSpeed = 0f;
        }

        private void OnCollisionEnter(Collision collision)
        {
            _currentStrategy?.HandleCollision(collision);
            
        }

        public void ApplyBallPrefab(GameObject prefab, float mass)
        {
            if (_currentStrategy != null)
            {
                Destroy(_currentVisualChild);
            }

            

            _currentVisualChild = Instantiate(prefab, transform);
            _currentVisualChild.transform.localPosition = Vector3.zero;
            _currentVisualChild.transform.localRotation = Quaternion.identity;
            _currentVisualChild.transform.localScale = Vector3.one;
            if (_rb != null)
            {
                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
                _rb.mass = mass;
            }
            if (_currentStrategy != null)
            {
                _currentStrategy.Initialize(transform, _rb);
            }
        }

        public bool IsSettled()
        {
            float stopThreshold = 0.15f;
            if (_rb.isKinematic)
            {
                // Если шар в режиме MovePosition, смотрим на нашу самодельную скорость
                // (Если меньше 0.05 единиц в секунду — значит остановился)
                return _kinematicSpeed < stopThreshold; 
            }
            else
            {
                // Если шар в режиме AddForce, смотрим на нативную физику
                return _rb.velocity.magnitude < stopThreshold && _rb.angularVelocity.magnitude < stopThreshold;
            }
        }

    }
}


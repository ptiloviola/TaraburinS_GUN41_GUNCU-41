using UnityEngine;
using Bowling.BowlingPins;


namespace Bowling.Ball
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : MonoBehaviour
    {
        private Rigidbody _rb;
        private IThrowStrategy _currentStrategy;

        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private GameObject _currentVisualChild;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
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
            _currentStrategy?.ResetStrategy();
            _rb.isKinematic = true;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.position = _startPosition;
            _rb.rotation = _startRotation;
            _rb.WakeUp();
            _rb.isKinematic = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_currentStrategy is MovePositionStrategy moveStrategy)
            {
                if (collision.gameObject.TryGetComponent<BowlingPin>(out BowlingPin pin))
                {
                    
                    Vector3 impactVector = moveStrategy.CurrentVelocity * 2.0f;

                    Rigidbody pinRb = pin.GetComponent<Rigidbody>();
                    if (pinRb != null)
                    {
                        pinRb.AddForce(impactVector, ForceMode.Impulse);
                    }
                     
                }
                moveStrategy.StopMotorAndTransferPhysics();
            }
            
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
            _rb.mass = mass;
            if (_currentStrategy != null)
            {
                _currentStrategy.Initialize(transform, _rb);
            }
        }

        public bool IsSettled()
        {
            return _rb.velocity.magnitude < 0.05f && _rb.angularVelocity.magnitude < 0.05f;
        }

    }
}


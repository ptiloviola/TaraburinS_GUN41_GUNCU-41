using UnityEngine;


namespace Bowling.BowlingPins
{
    public class BowlingPin : MonoBehaviour
    {
        private Rigidbody _rb;
        private bool _isLyingDown = false;
        private Vector3 _startPosition;
        private Quaternion _startRotation;

        public bool IsFallen => _isLyingDown;

        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!_isLyingDown)
            {
                // хотелось сделать чистую проверку на то, что кегля упала и лежит(чтобы исключить случай, 
                // когда упав, она вернулась в стоячее положение), но не получилось поймать это
                //if ((Vector3.Angle(Vector3.up, transform.up) > 45) && (_rb.velocity.magnitude < 0.2f))
                if (Vector3.Angle(Vector3.up, transform.up) > 45)
                {
                    _isLyingDown = true;
                    Debug.Log($"Кегля {transform.gameObject.name} упала");
                }
            }
        }

        public void ResetPin()
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            transform.position = _startPosition;
            transform.rotation = _startRotation;
            _isLyingDown = false;
            _rb.WakeUp();
        }
    }
}


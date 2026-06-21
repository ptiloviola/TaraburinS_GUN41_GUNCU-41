using UnityEngine;


namespace Bowling.BowlingPins
{
    public class BowlingPin : MonoBehaviour
    {
        private Rigidbody _rb;
        private Vector3 _startPosition;
        private Quaternion _startRotation;



        void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _startPosition = transform.position;
            _startRotation = transform.rotation;
        }

        public bool IsFallen()
        {
            return Vector3.Angle(Vector3.up, transform.up) > 45f;
        }


        public void ResetPin()
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            transform.position = _startPosition;
            transform.rotation = _startRotation;
            _rb.Sleep();
        }
    }
}


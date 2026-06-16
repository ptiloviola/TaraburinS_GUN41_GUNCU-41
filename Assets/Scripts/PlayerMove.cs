using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float _speed = 5;
    [SerializeField] private float _horizontalTurnSensitivity = 10f;
    [SerializeField] private float _verticalTurnSensitivity = 10f;
    [SerializeField] private float _verticalMinAngle = -89;
    [SerializeField] private float _verticalMaxAngle = 89;
    [SerializeField] private float _jumpSpeed = 7f;

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _transform;
    [SerializeField] private Transform _cameraTransform;

    private Vector3 _verticalVelocity;
    private float _cameraAngle = 0f;


    private void Awake()
    {
        _transform = transform;
        _cameraAngle = _cameraTransform.localScale.x;
    }
    private void Update()
    {
        if (_characterController != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(_cameraTransform.right, Vector3.up).normalized;
            
            _cameraAngle -= Input.GetAxis("Mouse Y") * _verticalTurnSensitivity;
            _cameraAngle = Mathf.Clamp(_cameraAngle, _verticalMinAngle, _verticalMaxAngle);
            _cameraTransform.localEulerAngles = Vector3.right * _cameraAngle;

            _transform.Rotate(Vector3.up * _horizontalTurnSensitivity, Input.GetAxis("Mouse X"));
            


            Vector3 playerSpeed = forward * Input.GetAxis("Vertical") * _speed 
                    + right * Input.GetAxis("Horizontal") * _speed; 
            if (_characterController.isGrounded)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    _verticalVelocity = Vector3.up * _jumpSpeed;
                }
                else
                {
                    _verticalVelocity = Vector3.down;
                }
                
                _characterController.Move((playerSpeed + _verticalVelocity) * Time.deltaTime);
            }
            else
            {
                Vector3 horizontalVelocity = _characterController.velocity;
                horizontalVelocity.y = 0;
                _verticalVelocity += Physics.gravity * Time.deltaTime;

                _characterController.Move((_characterController.velocity + _verticalVelocity) * Time.deltaTime);
            }
        }
        
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.GetComponent<Rigidbody>())
        {
            hit.rigidbody.velocity = Vector3.up * 10;
        }
    
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_transform.position, Vector3.right + Vector3.forward + Vector3.up * _characterController.height);
    }

}

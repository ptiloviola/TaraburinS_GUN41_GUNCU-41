using UnityEngine;

public class PlayerRigidbody : MonoBehaviour
{
    [SerializeField] private float _speed;
    private Rigidbody _rigidbody;


    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Vector3 playerSpeed = new Vector3(Input.GetAxis("Horizontal") * _speed, 0, Input.GetAxis("Vertical") * _speed);
        _rigidbody.velocity = playerSpeed;
        _rigidbody.velocity += Physics.gravity; 
    }

}

using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 12f;

    private Rigidbody2D _rb;
    private Animator _animator;

    private float _horizontalInput;
    private bool _isJumpPressed;

    private bool _isGrounded;
    
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        
    }

    
    void Update()
    {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _isJumpPressed = true;
        }

        _animator.SetBool("isWalking", _horizontalInput != 0);

        FlipSprite();
    }

    void FixedUpdate()
    {
        Move();
        if (_isJumpPressed)
        {
            Jump();
        }
    }

    private void Move()
    {
        _rb.velocity = new Vector2(_horizontalInput * _moveSpeed, _rb.velocity.y);
    }

    private void Jump()
    {
        _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        _animator.SetBool("isJumping", true);
        _isGrounded = false;
        _isJumpPressed = false;
    }

    private void FlipSprite()
    {
        if (_horizontalInput > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _isGrounded = true;
        _animator.SetBool("isJumping", false);
    }



}

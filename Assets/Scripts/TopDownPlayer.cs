using UnityEngine;

public class TopDownPlayer : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Sprites for Directions")]
    [SerializeField] private Sprite[] _walkDown;
    [SerializeField] private Sprite[] _walkUp;
    [SerializeField] private Sprite[] _walkLeft;
    [SerializeField] private Sprite[] _walkRight;

    [Header("Animation Settings")]
    [SerializeField] private float _frameRate = 0.15f;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    private Vector2 _movementInput;
    private Sprite[] _currentAnimation;

    private float _animationTimer;
    private int _currentFrame;




    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentAnimation = _walkDown;
        
    }


    void Update()
    {
        _movementInput.x = Input.GetAxisRaw("Horizontal");
        _movementInput.y = Input.GetAxisRaw("Vertical");
        if (_movementInput.x > 0) _currentAnimation = _walkRight;
        if (_movementInput.x < 0) _currentAnimation = _walkLeft;
        if (_movementInput.y > 0) _currentAnimation = _walkUp;
        if (_movementInput.y < 0) _currentAnimation = _walkDown;
        AnimateSprite();
        
    }

    void FixedUpdate()
    {
        _rb.velocity = _movementInput.normalized * _moveSpeed;
        
    }

    private void AnimateSprite()
    {
        if (_movementInput == Vector2.zero)
        {
            _spriteRenderer.sprite = _currentAnimation[0];
            return;
        }

        _animationTimer += Time.deltaTime;
        if (_animationTimer >= _frameRate)
        {
            _animationTimer = 0f;
            _currentFrame++;
            if (_currentFrame >= _currentAnimation.Length)
            {
                _currentFrame = 1;
            }
            _spriteRenderer.sprite = _currentAnimation[_currentFrame];
        }
    }
}

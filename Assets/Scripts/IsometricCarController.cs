using UnityEngine;

public class IsometricCarController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 5f;
    [Header("Isometric Sprites (8 Directions)")]
    [SerializeField] private Sprite _spriteN;
    [SerializeField] private Sprite _spriteNE;
    [SerializeField] private Sprite _spriteE;
    [SerializeField] private Sprite _spriteSE;
    [SerializeField] private Sprite _spriteS;
    [SerializeField] private Sprite _spriteSW;
    [SerializeField] private Sprite _spriteW;
    [SerializeField] private Sprite _spriteNW;

    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _movementInput;

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteRenderer.sprite = _spriteE;
        
    }

    void Update()
    {
        float horizontal =_movementInput.x = Input.GetAxis("Horizontal");
        float vertical = _movementInput.y = Input.GetAxis("Vertical");
        _movementInput = new Vector2(horizontal, vertical);
        if (_movementInput != Vector2.zero)
        {
            UpdateCarSprite(horizontal, vertical);
        }
    }

    void FixedUpdate()
    {
        _rb.velocity = _movementInput.normalized * _moveSpeed;
    }


    private void UpdateCarSprite(float h, float v)
    {
        if (h > 0 && v > 0) _spriteRenderer.sprite = _spriteNE;
        else if (h > 0 && v < 0) _spriteRenderer.sprite = _spriteSE;
        else if (h < 0 && v < 0) _spriteRenderer.sprite = _spriteSW;
        else if (h < 0 && v > 0) _spriteRenderer.sprite = _spriteNW;
        else if (h == 0 && v > 0) _spriteRenderer.sprite = _spriteN;
        else if (h > 0 && v == 0) _spriteRenderer.sprite = _spriteE;
        else if (h == 0 && v < 0) _spriteRenderer.sprite = _spriteS;
        else if (h < 0 && v == 0) _spriteRenderer.sprite = _spriteW;
    }
}

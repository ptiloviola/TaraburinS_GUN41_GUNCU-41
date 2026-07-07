using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerFPS_NewInput : MonoBehaviour
{
    [Header("Настройки FPS")]
    public float walkSpeed = 5f;
    public float lookSensitivity = 0.2f;
    public Camera playerCam;

    [Header("Прицел (UI)")]
    public RectTransform crosshair; // Сюда перетащим нашу картинку
    public float normalSize = 20f;  // Обычный размер
    public float aimSize = 8f;      // Размер при прицеливании (зуме)
    public float fireExpansion = 30f; // Насколько сильно расширяется при выстреле

    private CharacterController _controller;
    private float _xRotation = 0f;
    private float _normalFOV;
    private float _currentCrosshairSize; // Текущий размер в кадрах

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; 
        _normalFOV = playerCam.fieldOfView;
        _currentCrosshairSize = normalSize;
    }

    private void Update()
    {
        if (Keyboard.current == null || Mouse.current == null) return;

        HandleLook();
        HandleMovement();
        HandleWeapon();
        UpdateCrosshair(); // Обновляем UI прицела
    }

    private void HandleLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        float mouseX = mouseDelta.x * lookSensitivity;
        float mouseY = mouseDelta.y * lookSensitivity;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        
        playerCam.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.wKey.isPressed) moveZ += 1f;
        if (Keyboard.current.sKey.isPressed) moveZ -= 1f;
        if (Keyboard.current.aKey.isPressed) moveX -= 1f;
        if (Keyboard.current.dKey.isPressed) moveX += 1f;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        _controller.Move(move.normalized * walkSpeed * Time.deltaTime);
    }

    private void HandleWeapon()
    {
        // Выстрел
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Резко "взрываем" размер прицела, имитируя отдачу/разброс
            _currentCrosshairSize += fireExpansion;
            
            Debug.Log("Выстрел!");

            Ray ray = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                if (hit.collider.TryGetComponent<EnemyRoamer>(out var enemy))
                {
                    Debug.Log("Попадание по врагу!"); 
                }
            }
        }

        // Прицеливание (Зум)
        if (Mouse.current.rightButton.isPressed)
        {
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, 40f, 10f * Time.deltaTime);
        }
        else
        {
            playerCam.fieldOfView = Mathf.Lerp(playerCam.fieldOfView, _normalFOV, 10f * Time.deltaTime);
        }

        if (Keyboard.current.rKey.wasPressedThisFrame) Debug.Log("Перезарядка!"); 
    }

    private void UpdateCrosshair()
    {
        if (crosshair == null) return;

        // 1. Определяем, к какому размеру прицел должен стремиться сейчас
        float targetSize = Mouse.current.rightButton.isPressed ? aimSize : normalSize;

        // 2. Плавно "сдуваем" или "расширяем" прицел к этому целевому размеру
        _currentCrosshairSize = Mathf.Lerp(_currentCrosshairSize, targetSize, 10f * Time.deltaTime);

        // 3. Применяем расчеты к UI картинке
        crosshair.sizeDelta = new Vector2(_currentCrosshairSize, _currentCrosshairSize);
    }
}
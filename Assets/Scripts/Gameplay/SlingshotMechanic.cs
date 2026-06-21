using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Bowling.Gameplay
{
    public class SlingshotMechanic : BaseThrowMechanic
    {
        [Header("Настройки физики")]
        [SerializeField] private float _maxForce = 50f;
        [SerializeField] private float _dragMultiplier = 10f;

        [Header("Визуализация (Резинка)")]
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private Transform _ballTransform;
        [SerializeField] private float _visualLineMultiplier = 3f;
        
        [Range(0f, 1f)]
        [SerializeField] private float _shakeThreshold = 0.6f;
        [SerializeField] private float _shakeAmplitude = 2f;
        [SerializeField] private float _shakeSpeed = 25f;

        private Vector3 _currentAimDirection;
        private Vector3 _startDragWorldPos;
        private bool _isDragging = false;
        
        private Camera _mainCamera;
        private Coroutine _drawRoutine;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        // 1. ПОДПИСКА НА СОБЫТИЯ
        public override void Initialize(BowlingInputActions inputActions)
        {
            base.Initialize(inputActions);
            _inputActions.Slingshot.Drag.started += OnDragStarted;
            _inputActions.Slingshot.Drag.canceled += OnDragCanceled;
        }

        // 2. ФАЗА: ИГРОК НАЖАЛ КНОПКУ
        private void OnDragStarted(InputAction.CallbackContext context)
        {
            // ЖЕЛЕЗОБЕТОННАЯ ЗАЩИТА: Игнорируем клик, если мы над кнопками меню!
            if (_isThrowExecuted || _isPointerOverUI) return;

            if (GetMouseWorldPosition(out Vector3 hitPos))
            {
                _startDragWorldPos = hitPos;
                _isDragging = true; // Засчитываем честное начало натяжения

                if (_lineRenderer != null)
                {
                    _lineRenderer.enabled = true;
                    Vector3 startPoint = _ballTransform != null ? _ballTransform.position : _startDragWorldPos;
                    startPoint.y = 0.5f; 
                    _lineRenderer.SetPosition(0, startPoint);
                    _lineRenderer.SetPosition(1, startPoint);
                }
                
                if (_drawRoutine != null) StopCoroutine(_drawRoutine);
                _drawRoutine = StartCoroutine(DrawSlingshotRoutine());
            }
        }

        // 3. ФАЗА: ИГРОК ОТПУСТИЛ КНОПКУ
        private void OnDragCanceled(InputAction.CallbackContext context)
        {
            // ЗАЩИТА ОТ БРОСКА-ПРИЗРАКА: Бросаем, только если было честное натяжение мимо UI
            if (!_isDragging) return; 

            if (GetMouseWorldPosition(out Vector3 hitPos))
            {
                Vector3 dragVector = _startDragWorldPos - hitPos;
                dragVector.y = 0;
                float rawForce = dragVector.magnitude * _dragMultiplier;
                float clampedForce = Mathf.Clamp(rawForce, 0, _maxForce);
                
                ExecuteThrow(_currentAimDirection, clampedForce);
            }
            
            _isDragging = false;
            if (_drawRoutine != null) StopCoroutine(_drawRoutine);
            if (_lineRenderer != null) _lineRenderer.enabled = false;
        }

        // 4. КОРУТИНА ОТРИСОВКИ (Спит, пока кнопка не зажата)
        private IEnumerator DrawSlingshotRoutine()
        {
            while (_isDragging)
            {
                if (GetMouseWorldPosition(out Vector3 currentHitPos))
                {
                    Vector3 dragVector = _startDragWorldPos - currentHitPos;
                    dragVector.y = 0;

                    float maxDragDistance = _maxForce / _dragMultiplier; 
                    Vector3 clampedDragVector = Vector3.ClampMagnitude(dragVector, maxDragDistance);
                    float tensionPercent = clampedDragVector.magnitude / maxDragDistance;
                    
                    Vector3 shakeOffset = Vector3.zero;
                    if (tensionPercent > _shakeThreshold)
                    {
                        float intensity = (tensionPercent - _shakeThreshold) / (1f - _shakeThreshold);
                        float noiseX = (Mathf.PerlinNoise(Time.time * _shakeSpeed, 0f) - 0.5f) * 2f;
                        float noiseZ = (Mathf.PerlinNoise(0f, Time.time * _shakeSpeed) - 0.5f) * 2f;
                        shakeOffset = new Vector3(noiseX, 0f, noiseZ) * (_shakeAmplitude * intensity);
                    }

                    Vector3 startPoint = _ballTransform != null ? _ballTransform.position : _startDragWorldPos;
                    startPoint.y = 0.5f;

                    Vector3 aimPoint = startPoint + (clampedDragVector * _visualLineMultiplier) + shakeOffset;
                    aimPoint.y = 0.5f;

                    _currentAimDirection = (aimPoint - startPoint).normalized;

                    if (_lineRenderer != null)
                    {
                        _lineRenderer.SetPosition(0, startPoint);
                        _lineRenderer.SetPosition(1, aimPoint);
                    }
                }
                yield return null; 
            }
        }

        private bool GetMouseWorldPosition(out Vector3 result)
        {
            Vector2 mousePos = _inputActions.Slingshot.Position.ReadValue<Vector2>();
            Ray ray = _mainCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                result = hit.point;
                return true;
            }
            result = Vector3.zero;
            return false;
        }

        public override void EnableMechanic()
        {
            this.enabled = true;
            if (_inputActions != null) _inputActions.Slingshot.Enable();
            ResetMechanic();
        }

        public override void DisableMechanic()
        {
            this.enabled = false;
            if (_inputActions != null) _inputActions.Slingshot.Disable();
            ResetMechanic();
        }

        private void OnDestroy()
        {
            if (_inputActions != null)
            {
                _inputActions.Slingshot.Drag.started -= OnDragStarted;
                _inputActions.Slingshot.Drag.canceled -= OnDragCanceled;
            }
        }
    }
}
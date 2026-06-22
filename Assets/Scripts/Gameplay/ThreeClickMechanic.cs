using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Bowling.Gameplay
{
    public class ThreeClickMechanic : BaseThrowMechanic
    {

        [Header("UI Ссылки")]
        [SerializeField] private Slider _powerSlider;
        [SerializeField] private Slider _accuracySlider;
        [SerializeField] private RectTransform _sweetSpotRect;
        [SerializeField] private GameObject _uiContainer;
        
        [Header("Настройки силы")]
        [SerializeField] private float _powerSliderSpeed = 50f;
        [SerializeField] private float _powerMultiplier = 15f;

        [Header("Настройки точности")]
        [SerializeField] private float _accuracySliderSpeed = 3f;
        [SerializeField] private float _minAccuracySpeed = 2f;
        [SerializeField] private float _maxAccuracySpeed = 8f;
        [SerializeField] private float _maxSweetSpotWidth = 200f;
        [SerializeField] private float _minSweetSpotWidth = 20f;
        [SerializeField] private float _sideDeviationMultiplier = 2f;
        
        

        private enum ClickState {Idle, SettingPower, SettingAccuracy, Finished}
        private ClickState _currentState = ClickState.Idle;

        
        private float _accuracyTimer = 0f;
        private float _powerTimer = 0f;
        private float _finalPower = 0f;
        private float _finalAccuracy = 0f;

        private Coroutine _animationRoutine;

        public override void Initialize(BowlingInputActions inputActions)
        {
            base.Initialize(inputActions);
            
            _inputActions.ThreeClick.Click.performed += OnClickReceived;
            
            Debug.Log("<color=green>[ThreeClick] УСПЕХ: Инициализация прошла, подписка оформлена!</color>");
        }

        private void OnClickReceived(InputAction.CallbackContext context)
        {
            if (!this.enabled || _isThrowExecuted || _isPointerOverUI) return;

            AdvanceState();
        }



    

        private void AdvanceState()
        {
            switch(_currentState)
            {
                case ClickState.Idle:
                    _currentState = ClickState.SettingPower;
                    if (_animationRoutine != null) StopCoroutine(_animationRoutine);
                    _animationRoutine = StartCoroutine(AnimateSlidersRoutine());
                    break;;
                case ClickState.SettingPower:
                    _finalPower = _powerSlider.value;
                    float powerPercent = _finalPower / 100f; 
        
                    _accuracySliderSpeed = Mathf.Lerp(_minAccuracySpeed, _maxAccuracySpeed, powerPercent);
                    
                    if (_sweetSpotRect != null)
                    {
                        float newWidth = Mathf.Lerp(_maxSweetSpotWidth, _minSweetSpotWidth, powerPercent);
                        _sweetSpotRect.sizeDelta = new Vector2(newWidth, _sweetSpotRect.sizeDelta.y);

                    }
                    _currentState = ClickState.SettingAccuracy;
                    break;
                case ClickState.SettingAccuracy:
                    _finalAccuracy = _accuracySlider.value;
                    float lateralShift = _finalAccuracy * _sideDeviationMultiplier;
                    Vector3 throwDir = new Vector3(lateralShift, 0f, 1f).normalized;
                    float finalPhysicalForce = _finalPower * _powerMultiplier;
                    _currentState = ClickState.Finished;
                    // Останавливаем корутину, ползунки замирают
                    if (_animationRoutine != null) StopCoroutine(_animationRoutine);
                    ExecuteThrow(throwDir, finalPhysicalForce);
                    break;
            }
        }

        private IEnumerator AnimateSlidersRoutine()
        {
            while (_currentState == ClickState.SettingPower || _currentState == ClickState.SettingAccuracy)
            {
                if (_currentState == ClickState.SettingPower)
                {
                    _powerTimer += Time.deltaTime * _powerSliderSpeed;
                    _powerSlider.value = Mathf.PingPong(_powerTimer, 100f);
                }
                else if (_currentState == ClickState.SettingAccuracy)
                {
                    _accuracyTimer += Time.deltaTime * _accuracySliderSpeed;
                    _accuracySlider.value = Mathf.PingPong(_accuracyTimer, 2f) - 1f;
                }
                
                yield return null; 
            }
        }

        public override void ResetMechanic()
        {
            base.ResetMechanic();
            _currentState = ClickState.Idle;
            _powerTimer = 0f;
            _accuracyTimer = 0f;

            if (_animationRoutine != null) StopCoroutine(_animationRoutine);
            if (_powerSlider != null) _powerSlider.value = 0f;
            if (_accuracySlider != null) _accuracySlider.value = 0f;

            if (_sweetSpotRect != null)
            {
                _sweetSpotRect.sizeDelta = new Vector2(_maxSweetSpotWidth, _sweetSpotRect.sizeDelta.y);
            }
        }

        public override void EnableMechanic()
        {
            this.enabled = true;
            if (_uiContainer != null) _uiContainer.SetActive(true);
            
            _inputActions.ThreeClick.Enable(); 
            
            Debug.Log($"<color=cyan>[ThreeClick] КАРТА ВВОДА ВКЛЮЧЕНА: {_inputActions.ThreeClick.enabled}</color>");
        }

        public override void DisableMechanic()
        {
            this.enabled = false;
            if (_uiContainer != null) _uiContainer.SetActive(false);
            if (_inputActions != null) _inputActions.ThreeClick.Disable();
            ResetMechanic();
        }

        private void OnDestroy()
        {
            if (_inputActions != null)
            {
                _inputActions.ThreeClick.Click.started -= OnClickReceived;
            }
        }



    }
}



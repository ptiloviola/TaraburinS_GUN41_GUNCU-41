using UnityEngine;
using UnityEngine.UI;

namespace Bowling.Gameplay
{
    public class ThreeClickMechanic : BaseThrowMechanic
    {

        [SerializeField] private Slider _powerSlider;
        [SerializeField] private float _powerSliderSpeed = 50f;
        [SerializeField] private Slider _accuracySlider;
        [SerializeField] private float _accuracySliderSpeed = 3f;
        
        private float _accuracyTimer = 0f;

        private enum ClickState {Idle, SettingPower, SettingAccuracy, Finished}
        private ClickState _currentState = ClickState.Idle;

        

        private float _powerTimer = 0f;
        private float _finalPower = 0f;
        private float _finalAccuracy = 0f;

        [SerializeField] private float _minAccuracySpeed = 2f;
        [SerializeField] private float _maxAccuracySpeed = 8f;

        [SerializeField] private RectTransform _sweetSpotRect;
        [SerializeField] private float _maxSweetSpotWidth = 200f;
        [SerializeField] private float _minSweetSpotWidth = 20f;

        [SerializeField] private float _powerMultiplier = 15f;
        [SerializeField] private float _sideDeviationMultiplier = 2f;

        [SerializeField] private GameObject _uiContainer;

        

        private void Update()
        {
            if (_isThrowExecuted) return;
            if (Input.GetMouseButtonDown(0))
            {
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    return; 
                }
                AdvanceState();
            }
            if (_currentState == ClickState.SettingPower)
            {
                _powerTimer += Time.deltaTime * _powerSliderSpeed;
                float currentPower = Mathf.PingPong(_powerTimer, 100f);
                _powerSlider.value = currentPower;

            }
            else if (_currentState == ClickState.SettingAccuracy)
            {
                _accuracyTimer += Time.deltaTime * _accuracySliderSpeed;
                float currentAccuracy = Mathf.PingPong(_accuracyTimer, 2f) - 1f;
                _accuracySlider.value = currentAccuracy;
                
            }
        }

        private void AdvanceState()
        {
            switch(_currentState)
            {
                case ClickState.Idle:
                    _currentState = ClickState.SettingPower;
                    break;
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
                    ExecuteThrow(throwDir, finalPhysicalForce);
                    _currentState = ClickState.Finished;
                    break;
            }
        }

        public override void ResetMechanic()
        {
            base.ResetMechanic();
            _currentState = ClickState.Idle;
            _powerTimer = 0f;
            _accuracyTimer = 0f;
            
            if (_powerSlider != null) _powerSlider.value = 0f;
            if (_accuracySlider != null) _accuracySlider.value = 0f;

            if (_sweetSpotRect != null)
            {
                _sweetSpotRect.sizeDelta = new Vector2(_maxSweetSpotWidth, _sweetSpotRect.sizeDelta.y);
            }
        }

        private void OnEnable()
        {
            if (_uiContainer != null) _uiContainer.SetActive(true);
        }
        private void OnDisable()
        {
            if (_uiContainer != null) _uiContainer.SetActive(false);
            ResetMechanic();
        }


    }
}



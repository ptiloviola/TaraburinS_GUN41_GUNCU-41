
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{

    [SerializeField]
    private GameObject _restartUI;

    [SerializeField]
    private Image _restartFillImage;
    
    [SerializeField]
    private float _timeToHold = 2f;

    private SceneController _sceneController;
    private Controls _controls;

    private bool _isHolding = false;
    private float _currentHoldingTime = 0f;

    [Inject]
    public void Construct(SceneController sceneController, Controls controls)
    {
        _sceneController = sceneController;
        _controls = controls;
    }

    private void OnEnable()
    {
        _controls.Game.Restart.started += OnRestartStarted;
        _controls.Game.Restart.canceled += OnRestartCanceling;
    }

    private void OnDisable()
    {
        _controls.Game.Restart.started -= OnRestartStarted;
        _controls.Game.Restart.canceled -= OnRestartCanceling;
    }

    private void OnRestartStarted(InputAction.CallbackContext context)
    {
        _isHolding = true;
        _currentHoldingTime = 0f;
        _restartFillImage.fillAmount = 0f;
        _restartUI.SetActive(true);
    }

    private void OnRestartCanceling(InputAction.CallbackContext context)
    {
        _isHolding = false;
        _restartFillImage.fillAmount = 0f;
        _restartUI.SetActive(false);
    }

    private void RestartLevel()
    {
        _sceneController.OpenMainScene();
        _sceneController.OpenGameScene();
    }

    private void Update()
    {
        if (_isHolding)
        {
            _currentHoldingTime += Time.deltaTime;
            _restartFillImage.fillAmount = _currentHoldingTime/ _timeToHold;
            if (_currentHoldingTime >= _timeToHold)
            {
                _isHolding = false;
                RestartLevel();
            }
        }
    }
}

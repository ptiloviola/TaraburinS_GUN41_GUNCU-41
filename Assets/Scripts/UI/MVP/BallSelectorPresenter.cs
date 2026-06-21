using UnityEngine;
using Bowling.Ball;

namespace Bowling.UI.MVP
{
    public class BallSelectorPresenter : MonoBehaviour
    {
        [Header("Связи MVP")]
        [SerializeField] private BallModel _model;
        [SerializeField] private BallSelectorView _view;
        
        [Header("Связь с игрой")]
        [SerializeField] private BallController _ballController;

        private void Start()
        {
            _view.OnBallClicked += HandleBallSelection;

            _view.SetupView(_model.AvailableBalls);

            if (_model.AvailableBalls.Length > 0)
            {
                HandleBallSelection(0);
            }
        }

        private void HandleBallSelection(int index)
        {
            BallConfig selectedConfig = _model.GetBall(index);
            
            if (selectedConfig != null && selectedConfig.BallPrefab != null)
            {
                _ballController.ApplyBallPrefab(selectedConfig.BallPrefab, selectedConfig.Mass);
                Debug.Log($"[Presenter] Выбран шар: {selectedConfig.Name}");
            }
        }

        private void OnDestroy()
        {
            if (_view != null)
            {
                _view.OnBallClicked -= HandleBallSelection;
            }
        }
    }
}
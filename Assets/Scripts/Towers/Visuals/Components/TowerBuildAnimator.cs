using UnityEngine;
using DG.Tweening;

namespace Gameplay.Towers.Visuals.Components
{
    public class TowerBuildAnimator : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private Transform _baseTransform;
        [SerializeField] private Transform _turretTransform;
        [Range(0.1f, 2f)] [SerializeField] private float _buildDuration = 0.5f;
        [SerializeField] private Ease _buildEase = Ease.OutBack;

        private TowerVisualsHub _hub;

        private void Awake()
        {
            _hub = GetComponentInParent<TowerVisualsHub>();
            ResetScaleToZero();
        }

        private void OnEnable() => _hub.OnBuild += HandleBuild;
        private void OnDisable() => _hub.OnBuild -= HandleBuild;

        private void ResetScaleToZero()
        {
            if (_baseTransform != null) _baseTransform.localScale = Vector3.zero;
            if (_turretTransform != null) _turretTransform.localScale = Vector3.zero;
        }

        private void HandleBuild()
        {
            ResetScaleToZero();
            Sequence buildSequence = DOTween.Sequence();

            if (_baseTransform != null)
            {
                buildSequence.Append(_baseTransform.DOScale(Vector3.one, _buildDuration).SetEase(_buildEase));
            }
            if (_turretTransform != null)
            {
                buildSequence.Insert(_buildDuration * 0.4f, _turretTransform.DOScale(Vector3.one, _buildDuration).SetEase(_buildEase));
            }
        }
    }
}
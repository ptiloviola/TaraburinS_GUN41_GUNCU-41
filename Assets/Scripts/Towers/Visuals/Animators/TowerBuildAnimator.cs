using UnityEngine;
using DG.Tweening;
using Gameplay.Towers.Data.Visuals;

namespace Gameplay.Towers.Visuals.Animators
{
    public class TowerBuildAnimator
    {
        private readonly Transform _base;
        private readonly Transform _turret;
        private readonly BuildVisualData _config;

        public TowerBuildAnimator(Transform baseTransform, Transform turretTransform, BuildVisualData config)
        {
            _base = baseTransform;
            _turret = turretTransform;
            _config = config;
            ResetScaleToZero();
        }

        private void ResetScaleToZero()
        {
            if (_base != null) _base.localScale = Vector3.zero;
            if (_turret != null) _turret.localScale = Vector3.zero;
        }

        public void PlayBuildAnimation()
        {
            ResetScaleToZero();
            Sequence buildSequence = DOTween.Sequence();

            if (_base != null)
                buildSequence.Append(_base.DOScale(Vector3.one, _config.Duration).SetEase(_config.EaseType));
            
            if (_turret != null)
                buildSequence.Insert(_config.Duration * 0.4f, _turret.DOScale(Vector3.one, _config.Duration).SetEase(_config.EaseType));
        }
    }
}
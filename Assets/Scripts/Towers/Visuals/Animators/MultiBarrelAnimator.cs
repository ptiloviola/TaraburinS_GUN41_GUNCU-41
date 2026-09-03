using UnityEngine;
using DG.Tweening;
using Gameplay.Towers.Data.Visuals;

namespace Gameplay.Towers.Visuals.Animators
{
    public class MultiBarrelAnimator
    {
        private readonly Transform[] _barrels;
        private readonly ParticleSystem[] _flashes;
        private readonly RecoilVisualData _config;

        private readonly Vector3[] _initialPositions;
        private readonly Sequence[] _sequences;
        private int _currentIndex = 0;

        public MultiBarrelAnimator(Transform[] barrels, ParticleSystem[] flashes, RecoilVisualData config)
        {
            _barrels = barrels;
            _flashes = flashes;
            _config = config;

            _initialPositions = new Vector3[_barrels.Length];
            _sequences = new Sequence[_barrels.Length];

            for (int i = 0; i < _barrels.Length; i++)
            {
                if (_barrels[i] != null) _initialPositions[i] = _barrels[i].localPosition;
            }
        }

        public void PlayRecoil()
        {
            if (_barrels == null || _barrels.Length == 0) return;

            Transform activeBarrel = _barrels[_currentIndex];
            Vector3 initialPos = _initialPositions[_currentIndex];
            ParticleSystem activeFlash = (_flashes != null && _currentIndex < _flashes.Length) ? _flashes[_currentIndex] : null;

            if (_sequences[_currentIndex] != null && _sequences[_currentIndex].IsActive())
                _sequences[_currentIndex].Complete();

            _sequences[_currentIndex] = DOTween.Sequence();
            
            if (activeBarrel != null)
            {
                float recoilX = initialPos.x - _config.Distance;
                float halfDuration = _config.Duration * 0.5f;

                _sequences[_currentIndex].Append(activeBarrel.DOLocalMoveX(recoilX, halfDuration).SetEase(_config.RecoilEase));
                _sequences[_currentIndex].Append(activeBarrel.DOLocalMoveX(initialPos.x, halfDuration).SetEase(_config.ReturnEase));
                
                if (activeFlash != null)
                    _sequences[_currentIndex].InsertCallback(0f, () => activeFlash.Play());
            }

            _currentIndex = (_currentIndex + 1) % _barrels.Length;
        }
    }
}
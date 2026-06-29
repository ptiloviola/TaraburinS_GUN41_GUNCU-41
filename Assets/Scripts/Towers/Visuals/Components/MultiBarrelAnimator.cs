using UnityEngine;
using DG.Tweening;

namespace Gameplay.Towers.Visuals.Components
{
    public class MultiBarrelAnimator : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private Transform[] _barrelTransforms;
        [SerializeField] private ParticleSystem[] _muzzleFlashes; // Бывшие steamParticles

        [Header("Анимация отдачи")]
        [Range(0.05f, 1f)] [SerializeField] private float _shootDuration = 0.2f;
        [Range(0.1f, 2f)] [SerializeField] private float _recoilDistance = 0.4f;
        [SerializeField] private Ease _recoilEase = Ease.OutQuad;
        [SerializeField] private Ease _returnEase = Ease.OutQuad;

        private TowerVisualsHub _hub;
        private Sequence[] _barrelSequences;
        private Vector3[] _initialBarrelLocalPositions;
        private int _currentBarrelIndex = 0;

        private void Awake()
        {
            _hub = GetComponentInParent<TowerVisualsHub>();
            InitializeBarrels();
        }

        private void OnEnable() => _hub.OnShoot += HandleShoot;
        private void OnDisable() => _hub.OnShoot -= HandleShoot;

        private void InitializeBarrels()
        {
            if (_barrelTransforms == null || _barrelTransforms.Length == 0) return;

            _initialBarrelLocalPositions = new Vector3[_barrelTransforms.Length];
            _barrelSequences = new Sequence[_barrelTransforms.Length];

            for (int i = 0; i < _barrelTransforms.Length; i++)
            {
                if (_barrelTransforms[i] != null)
                {
                    _initialBarrelLocalPositions[i] = _barrelTransforms[i].localPosition;
                }
            }
        }

        private void HandleShoot(Vector3 targetPos)
        {
            if (_barrelTransforms == null || _barrelTransforms.Length == 0) return;

            Transform activeBarrel = _barrelTransforms[_currentBarrelIndex];
            Vector3 initialPos = _initialBarrelLocalPositions[_currentBarrelIndex];

            ParticleSystem activeFlash = null;
            if (_muzzleFlashes != null && _currentBarrelIndex < _muzzleFlashes.Length)
            {
                activeFlash = _muzzleFlashes[_currentBarrelIndex];
            }

            if (_barrelSequences[_currentBarrelIndex] != null && _barrelSequences[_currentBarrelIndex].IsActive())
            {
                _barrelSequences[_currentBarrelIndex].Complete();
            }

            _barrelSequences[_currentBarrelIndex] = DOTween.Sequence();
            
            if (activeBarrel != null)
            {
                float recoilX = initialPos.x - _recoilDistance;
                _barrelSequences[_currentBarrelIndex].Append(activeBarrel.DOLocalMoveX(recoilX, _shootDuration * 0.25f).SetEase(_recoilEase));
                _barrelSequences[_currentBarrelIndex].Append(activeBarrel.DOLocalMoveX(initialPos.x, _shootDuration * 0.75f).SetEase(_returnEase));
                
                if (activeFlash != null)
                {
                    _barrelSequences[_currentBarrelIndex].InsertCallback(0f, () => activeFlash.Play());
                }
            }

            _currentBarrelIndex = (_currentBarrelIndex + 1) % _barrelTransforms.Length;
        }
    }
}
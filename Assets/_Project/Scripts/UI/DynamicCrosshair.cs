using UnityEngine;
using Zenject;
using TpsShooter.Player;

namespace TpsShooter.UI
{
    public class DynamicCrosshair : MonoBehaviour
    {
        [Header("Crosshair Parts")]
        [SerializeField] private RectTransform _topLine;
        [SerializeField] private RectTransform _bottomLine;
        [SerializeField] private RectTransform _leftLine;
        [SerializeField] private RectTransform _rightLine;

        [Header("Settings")]
        [SerializeField] private float _baseOffset = 10f;
        [SerializeField] private float _spreadMultiplier = 500f;
        [SerializeField] private float _smoothSpeed = 15f;

        private PlayerFacade _player;
        private float _currentVisualSpread;

        [Inject]
        public void Construct(PlayerFacade playerFacade)
        {
            _player = playerFacade;
        }

        private void Update()
        {
            float targetSpread = _player != null ? _player.CurrentWeaponSpread : 0f;

            _currentVisualSpread = Mathf.Lerp(_currentVisualSpread, targetSpread, Time.deltaTime * _smoothSpeed);

            float offset = _baseOffset + (_currentVisualSpread * _spreadMultiplier);

            if (_topLine != null) _topLine.anchoredPosition = new Vector2(0, offset);
            if (_bottomLine != null) _bottomLine.anchoredPosition = new Vector2(0, -offset);
            if (_leftLine != null) _leftLine.anchoredPosition = new Vector2(-offset, 0);
            if (_rightLine != null) _rightLine.anchoredPosition = new Vector2(offset, 0);
        }
    }
}
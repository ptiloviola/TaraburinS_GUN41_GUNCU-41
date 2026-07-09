using UnityEngine;
using DG.Tweening;
using Infrastructure.Interfaces;
namespace Player.UI
{
    public class CrosshairUiView : ICrosshairView
    {
        private readonly RectTransform _crosshairUi;
        private readonly PlayerConfig _config;

        public CrosshairUiView(RectTransform crosshairUi, PlayerConfig config)
        {
            _crosshairUi = crosshairUi;
            _config = config;
            
            if (_crosshairUi != null) _crosshairUi.sizeDelta = _config.hipReticleSize;
        }

        public void SetAimState(bool isAiming)
        {
            if (_crosshairUi == null) return;
            
            _crosshairUi.DOKill();
            Vector2 targetUiSize = isAiming ? _config.aimReticleSize : _config.hipReticleSize;
            _crosshairUi.DOSizeDelta(targetUiSize, 0.2f).SetEase(Ease.OutQuad);
        }
    }
}

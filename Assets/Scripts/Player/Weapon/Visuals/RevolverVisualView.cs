using UnityEngine;
using DG.Tweening;
using Infrastructure.Interfaces;
using Player.Weapon.Config;
namespace Player.Weapon.Visuals
{
    public class RevolverVisualView : IWeaponView
    {
        private readonly Transform _weaponRoot;
        private readonly Transform _visualsRoot;
        private readonly Transform _cylinder;
        private readonly Transform _hammer;
        private readonly Transform _trigger;
        private readonly RangedWeaponConfig _config;

        private Tween _bobbingTween;

        public RevolverVisualView(Transform weaponRoot, 
            Transform visualsRoot, 
            Transform cylinder, 
            Transform hammer, 
            Transform trigger, 
            RangedWeaponConfig config)
        {
            _weaponRoot = weaponRoot;
            _visualsRoot = visualsRoot;
            _cylinder = cylinder;
            _hammer = hammer;
            _trigger = trigger;
            _config = config;

            _weaponRoot.localPosition = _config.hipPosition;
            _weaponRoot.localRotation = Quaternion.identity;
            _visualsRoot.localPosition = Vector3.zero;
            _visualsRoot.localRotation = Quaternion.identity;

            InitializeBobbing();
        }

        private void InitializeBobbing()
        {
            _bobbingTween = _visualsRoot.DOLocalMoveY(-0.02f, 0.4f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine)
                .Pause();
        }

        public void PlayBobbing(bool isMoving, bool isAiming)
        {
            if (isMoving && !isAiming) _bobbingTween.Play();
            else
            {
                _bobbingTween.Pause();
                _visualsRoot.DOLocalMoveY(0f, 0.2f);
            }
        }

        public void SetAimState(bool isAiming)
        {
            _weaponRoot.DOKill();
            Vector3 targetPos = isAiming ? _config.aimPosition : _config.hipPosition;
            _weaponRoot.DOLocalMove(targetPos, 0.2f).SetEase(Ease.OutQuad);
        }

        public void PlayFireAnimation()
        {

            Sequence recoilSeq = DOTween.Sequence();
            recoilSeq.Append(_visualsRoot.DOLocalMove(_config.recoilKickback, _config.recoilDuration).SetEase(Ease.OutExpo));
            recoilSeq.Join(_visualsRoot.DOLocalRotate(_config.recoilRotation, _config.recoilDuration).SetEase(Ease.OutExpo));
            recoilSeq.Append(_visualsRoot.DOLocalMove(Vector3.zero, _config.recoilDuration * 2f).SetEase(Ease.InOutQuad));
            recoilSeq.Join(_visualsRoot.DOLocalRotate(Vector3.zero, _config.recoilDuration * 2f).SetEase(Ease.InOutQuad));

            Sequence mechSeq = DOTween.Sequence();
            mechSeq.Append(_trigger.DOLocalRotate(_config.triggerPullRotation, 0.05f, RotateMode.LocalAxisAdd));
            mechSeq.Join(_hammer.DOLocalRotate(_config.hammerStrikeRotation, 0.05f, RotateMode.LocalAxisAdd));
            mechSeq.Append(_cylinder.DOLocalRotate(_config.cylinderStepRotation, 0.1f, RotateMode.LocalAxisAdd).SetEase(Ease.OutBack));
            mechSeq.Join(_hammer.DOLocalRotate(-_config.hammerStrikeRotation, 0.15f, RotateMode.LocalAxisAdd));
            mechSeq.Join(_trigger.DOLocalRotate(-_config.triggerPullRotation, 0.15f, RotateMode.LocalAxisAdd));
        }

        public void PlayReloadAnimation(float duration)
        {
            Sequence reloadSeq = DOTween.Sequence();
            reloadSeq.Append(_weaponRoot.DOLocalMove(new Vector3(0, -0.5f, 0), duration * 0.3f).SetEase(Ease.InQuad));
            reloadSeq.Join(_weaponRoot.DOLocalRotate(new Vector3(45f, 0, 0), duration * 0.3f));
            reloadSeq.AppendInterval(duration * 0.4f);
            reloadSeq.Append(_weaponRoot.DOLocalMove(_config.hipPosition, duration * 0.3f).SetEase(Ease.OutBack));
            reloadSeq.Join(_weaponRoot.DOLocalRotate(Vector3.zero, duration * 0.3f).SetEase(Ease.OutBack));
        }
    }
}
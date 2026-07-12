using UnityEngine;
using DG.Tweening;
using Infrastructure.Interfaces;
using Services;
using Player.Weapon.Config;

namespace Player.Weapon.Effects
{
    public class PaintballImpactHandler : IImpactHandler
    {
        private readonly RangedWeaponConfig _config;
        private readonly AudioService _audioService;

        public PaintballImpactHandler(RangedWeaponConfig config, AudioService audioService)
        {
            _config = config;
            _audioService = audioService;
        }

        public void HandleImpact(Vector3 startPoint, Vector3 targetPoint, Collider targetCollider, Vector3 hitNormal)
        {
            if (_config.paintballPrefab == null) return;

            GameObject ball = Object.Instantiate(_config.paintballPrefab, startPoint, Quaternion.identity);

            ball.transform.DOJump(targetPoint, _config.jumpPower, 1, _config.bulletDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() => ProcessCollision(ball, targetCollider, targetPoint, hitNormal));
        }

        private void ProcessCollision(GameObject ball, Collider targetCollider, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (targetCollider == null)
            {
                Object.Destroy(ball);
                return;
            }

            Color paintColor = ball.GetComponent<Renderer>().material.color;
            IDamageable damageable = targetCollider.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(1, hitPoint);
                _audioService.PlaySoundAtPoint(_config.hitSound, hitPoint, randomizePitch: true);
                SpawnSplatParticles(hitPoint, hitNormal, targetCollider.transform.root, paintColor);
                Object.Destroy(ball);
            }
            else
            {
                ball.transform.SetParent(targetCollider.transform, true);
                ball.transform.up = hitNormal;
                ball.transform.DOScale(new Vector3(0.25f, 0.01f, 0.25f), 0.05f).SetEase(Ease.OutQuad);
                SpawnSplatParticles(hitPoint, hitNormal, targetCollider.transform, paintColor);
                Object.Destroy(ball, 2f);
            }
        }

        private void SpawnSplatParticles(Vector3 point, Vector3 normal, Transform parent, Color color)
        {
            int burstCount = 6;
            for (int i = 0; i < burstCount; i++)
            {
                GameObject drop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(drop.GetComponent<Collider>());
                
                drop.transform.position = point;
                drop.transform.localScale = Vector3.one * Random.Range(0.03f, 0.06f);
                drop.GetComponent<Renderer>().material.color = color;
                drop.transform.SetParent(parent);

                Vector3 randomDir = (normal + Random.insideUnitSphere * 0.6f).normalized;
                Vector3 targetPosition = point + randomDir * Random.Range(0.2f, 0.5f);

                drop.transform.DOMove(targetPosition, Random.Range(0.15f, 0.3f)).SetEase(Ease.OutQuad);
                drop.transform.DOScale(Vector3.zero, Random.Range(0.15f, 0.3f))
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => Object.Destroy(drop));
            }
        }
    }
}
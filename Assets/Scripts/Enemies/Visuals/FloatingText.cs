using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Threading;
using Zenject;

namespace Gameplay.Enemies.Visuals
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private TextMeshPro _text;
        [SerializeField] private float _duration = 1f;
        [SerializeField] private float _floatSpeed = 2f;
        [SerializeField] private float _scatterRadius = 0.5f;

        private CancellationTokenSource _cts;
        private IMemoryPool _pool;
        private Camera _mainCamera; // Кэшируем камеру

        private void Awake()
        {
            // Находим камеру один раз при создании префаба пулом
            _mainCamera = Camera.main;
        }

        public void Init(Vector3 spawnPosition, float amount, Color color, IMemoryPool pool)
        {
            _pool = pool;
            _text.text = amount.ToString("0");
            _text.color = color;

            Vector2 randomCircle = Random.insideUnitCircle * _scatterRadius;
            transform.position = spawnPosition + new Vector3(randomCircle.x, randomCircle.y, 0f);

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            
            AnimateTextAsync(_cts.Token).Forget();
        }

        private async UniTaskVoid AnimateTextAsync(CancellationToken token)
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Color startColor = _text.color;

            while (elapsed < _duration)
            {
                if (token.IsCancellationRequested) return;

                elapsed += Time.deltaTime;
                float progress = elapsed / _duration;

                // Движение вверх
                transform.position = startPos + Vector3.up * (_floatSpeed * progress);

                // ИДЕАЛЬНЫЙ BILLBOARD: Текст всегда параллелен плоскости камеры
                if (_mainCamera != null)
                {
                    transform.forward = _mainCamera.transform.forward;
                }

                // Растворение альфа-канала
                startColor.a = 1f - progress;
                _text.color = startColor;

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            Despawn();
        }

        private void Despawn()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _pool.Despawn(this);
        }

        public class Pool : MonoMemoryPool<FloatingText> { }
    }
}
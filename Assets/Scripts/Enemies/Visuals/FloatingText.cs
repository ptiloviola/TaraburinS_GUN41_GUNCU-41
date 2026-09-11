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
        private Camera _mainCamera; 

        private void Awake()
        {
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

            // Оборачиваем в try-catch для чистоты, хотя UniTask сам глушит OperationCanceledException
            try
            {
                while (elapsed < _duration)
                {
                    if (token.IsCancellationRequested) return;

                    elapsed += Time.deltaTime;
                    float progress = elapsed / _duration;

                    transform.position = startPos + Vector3.up * (_floatSpeed * progress);

                    if (_mainCamera != null)
                    {
                        transform.forward = _mainCamera.transform.forward;
                    }

                    startColor.a = 1f - progress;
                    _text.color = startColor;

                    // Если токен отменится во время Yield, вылетит OperationCanceledException
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                Despawn();
            }
            catch (System.OperationCanceledException)
            {
                // Задача была прервана (вышли из Play Mode или переиспользовали объект) - это нормально
            }
        }

        private void Despawn()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _pool?.Despawn(this);
        }

        // НОВОЕ: Спасительный метод при выходе из Play Mode
        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public class Pool : MonoMemoryPool<FloatingText> { }
    }
}
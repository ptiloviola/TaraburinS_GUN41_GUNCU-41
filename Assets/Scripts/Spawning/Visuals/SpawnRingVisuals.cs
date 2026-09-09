using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Spawning.Visuals
{
    public class SpawnRingVisuals : MonoBehaviour, ISpawnVisuals
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        [Header("Ссылки")]
        [SerializeField] private Renderer _ringRenderer; 

        [Header("Настройки колец")]
        [SerializeField] private Color _ringColor = new Color(1f, 0f, 0f, 0.8f);
        [SerializeField] private float _maxScale = 3f; 
        [SerializeField] private float _pingSpeed = 0.5f; 

        private MaterialPropertyBlock _propertyBlock;
        private CancellationTokenSource _effectCts;
        private Vector3 _initialScale;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();

            if (_ringRenderer != null)
            {
                SetRingAlpha(0f);
                _initialScale = _ringRenderer.transform.localScale;
                _ringRenderer.gameObject.SetActive(false);
            }
        }

        public void PlayWarningEffect(float duration)
        {
            _effectCts?.Cancel();
            _effectCts?.Dispose();
            _effectCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            
            WarningRoutineAsync(duration, _effectCts.Token).Forget();
        }

        private async UniTaskVoid WarningRoutineAsync(float duration, CancellationToken ct)
        {
            if (_ringRenderer == null) return;

            try
            {
                _ringRenderer.gameObject.SetActive(true);

                float totalElapsed = 0f;
                float currentPingTimer = 0f;

                while (totalElapsed < duration)
                {
                    totalElapsed += Time.deltaTime;
                    currentPingTimer += Time.deltaTime;

                    if (currentPingTimer >= _pingSpeed)
                    {
                        currentPingTimer = 0f;
                    }

                    float progress = currentPingTimer / _pingSpeed;

                    float currentScale = Mathf.Lerp(0f, _maxScale, progress);
                    _ringRenderer.transform.localScale = new Vector3(currentScale, _initialScale.y, currentScale);

                    SetRingAlpha(Mathf.Lerp(_ringColor.a, 0f, progress));

                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
            }
            finally
            {
                if (_ringRenderer != null) _ringRenderer.gameObject.SetActive(false);
            }
        }

        private void SetRingAlpha(float alpha)
        {
            _ringRenderer.GetPropertyBlock(_propertyBlock);
            Color c = _ringColor;
            c.a = alpha;
            _propertyBlock.SetColor(ColorPropertyId, c);
            _ringRenderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
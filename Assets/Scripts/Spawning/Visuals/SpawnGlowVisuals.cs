using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Gameplay.Spawning.Visuals
{
    public class SpawnGlowVisuals : MonoBehaviour, ISpawnVisuals
    {
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        [Header("Ссылки на компоненты")]
        [SerializeField] private Renderer _glowPillar; 
        [SerializeField] private Light _groundLight;   

        [Header("Настройки Огня")]
        [SerializeField] private Color _fireColor = new Color(0.8f, 0f, 0f, 0.6f);
        [SerializeField] private float _maxLightIntensity = 8f;
        [SerializeField] private float _flickerSpeed = 15f; 

        [Header("Настройки таймингов")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private float _maxNoiseOffset = 100f;

        private MaterialPropertyBlock _propertyBlock;
        private CancellationTokenSource _effectCts;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();

            if (_glowPillar != null)
            {
                SetPillarAlpha(0f);
                _glowPillar.gameObject.SetActive(false);
            }

            if (_groundLight != null)
            {
                _groundLight.color = _fireColor;
                _groundLight.intensity = 0f;
                _groundLight.gameObject.SetActive(false);
            }
        }

        public void PlayWarningEffect(float duration)
        {
            // Отменяем предыдущий эффект, если он еще играет
            _effectCts?.Cancel();
            _effectCts?.Dispose();
            
            // Создаем новый токен, привязанный к жизни этого GameObject
            _effectCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            
            WarningRoutineAsync(duration, _effectCts.Token).Forget();
        }

        private async UniTaskVoid WarningRoutineAsync(float duration, CancellationToken ct)
        {
            try
            {
                if (_glowPillar != null) _glowPillar.gameObject.SetActive(true);
                if (_groundLight != null) _groundLight.gameObject.SetActive(true);

                float elapsed = 0f;
                float randomOffset = Random.Range(0f, _maxNoiseOffset); 

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    
                    // 1. Логика Fade In / Fade Out
                    float fadeMultiplier = 1f;
                    if (elapsed < _fadeInDuration) 
                        fadeMultiplier = elapsed / _fadeInDuration;
                    else if (duration - elapsed < _fadeOutDuration) 
                        fadeMultiplier = Mathf.Max(0f, (duration - elapsed) / _fadeOutDuration);

                    // 2. Логика шума Перлина
                    float flicker = Mathf.PerlinNoise(randomOffset, Time.time * _flickerSpeed);
                    flicker = Mathf.Lerp(0.3f, 1f, flicker); 

                    float currentIntensity = flicker * fadeMultiplier;

                    // 3. Обновляем свет и прозрачность через PropertyBlock (Zero Allocation!)
                    if (_groundLight != null) _groundLight.intensity = _maxLightIntensity * currentIntensity;
                    if (_glowPillar != null) SetPillarAlpha(_fireColor.a * currentIntensity);

                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
            }
            finally
            {
                // Блок finally гарантирует, что визуализатор выключится, даже если сцену закрыли
                if (_glowPillar != null) _glowPillar.gameObject.SetActive(false);
                if (_groundLight != null) _groundLight.gameObject.SetActive(false);
            }
        }

        private void SetPillarAlpha(float alpha)
        {
            _glowPillar.GetPropertyBlock(_propertyBlock);
            Color c = _fireColor;
            c.a = alpha;
            _propertyBlock.SetColor(ColorPropertyId, c);
            _glowPillar.SetPropertyBlock(_propertyBlock);
        }
    }
}
using UnityEngine;
using Gameplay.Core;

namespace Gameplay.Enemies.Visuals
{
    // Меняем зависимость с HealthComponent на DamageReceiver
    [RequireComponent(typeof(DamageReceiver))]
    public class DamageFlashVisualizer : MonoBehaviour
    {
        [Header("Настройки вспышек")]
        [SerializeField] private DamageVisualSettings _settings;
      
        
        private Renderer[] _renderers;
        private Color[] _originalColors;
        
        private DamageReceiver _damageReceiver;
        private MaterialPropertyBlock _propBlock;
        
        private float _currentFlashTimer;
        private bool _isFlashing;
        private Color _currentFlashColor; // Храним цвет текущей вспышки

        private void Awake()
        {
            _damageReceiver = GetComponent<DamageReceiver>();
            _propBlock = new MaterialPropertyBlock();
            
            _renderers = GetComponentsInChildren<Renderer>(true);
            _originalColors = new Color[_renderers.Length];
            
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i].sharedMaterial != null)
                {
                    if (_renderers[i].sharedMaterial.HasProperty("_BaseColor"))
                        _originalColors[i] = _renderers[i].sharedMaterial.GetColor("_BaseColor");
                    else if (_renderers[i].sharedMaterial.HasProperty("_Color"))
                        _originalColors[i] = _renderers[i].sharedMaterial.GetColor("_Color");
                    else
                        _originalColors[i] = Color.white;
                }
            }
        }

        private void OnEnable()
        {
            _damageReceiver.OnHitReceived += PlayFlash;
            _currentFlashTimer = 0f;
            _isFlashing = false;
        }

        private void OnDisable()
        {
            _damageReceiver.OnHitReceived -= PlayFlash;
            ResetColor();
        }

        private void PlayFlash(DamagePayload payload)
        {
            // Динамически выбираем цвет на основе типа входящего урона
            _currentFlashColor = payload.Type switch
            {
                DamageType.Physical => _settings.GetColor(payload.Type),
                DamageType.Energy => _settings.GetColor(payload.Type),
                DamageType.Explosive => _settings.GetColor(payload.Type),
                _ => _settings.PhysicalColor
            };

            _currentFlashTimer = _settings.FlashDuration;
            _isFlashing = true;
        }

        private void Update()
        {
            if (!_isFlashing) return;

            _currentFlashTimer -= Time.deltaTime;
            
            if (_currentFlashTimer <= 0f)
            {
                _isFlashing = false;
                ResetColor();
                return;
            }

            float halfDuration = _settings.FlashDuration / 2f;
            float intensity = _currentFlashTimer > halfDuration 
                ? Mathf.InverseLerp(_settings.FlashDuration, halfDuration, _currentFlashTimer) 
                : Mathf.InverseLerp(0f, halfDuration, _currentFlashTimer);

            ApplyFlashIntensity(intensity);
        }

        private void ApplyFlashIntensity(float intensity)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                
                _renderers[i].GetPropertyBlock(_propBlock);
                Color blendedColor = Color.Lerp(_originalColors[i], _currentFlashColor, intensity);
                
                _propBlock.SetColor("_BaseColor", blendedColor);
                _propBlock.SetColor("_Color", blendedColor); 
                _renderers[i].SetPropertyBlock(_propBlock);
            }
        }

        private void ResetColor()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null) _renderers[i].SetPropertyBlock(null);
            }
        }
    }
}
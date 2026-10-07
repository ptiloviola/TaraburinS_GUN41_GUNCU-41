using UnityEngine;
using Gameplay.Combat;
using Gameplay.Enemies.Combat;

namespace Gameplay.Enemies.Visuals.DamageFX
{
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
        private Color _currentFlashColor; 

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
            _currentFlashColor = _settings != null ? _settings.GetColor(payload.Type) : Color.white;
            _currentFlashTimer = _settings != null ? _settings.FlashDuration : 0.15f;
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

            float duration = _settings != null ? _settings.FlashDuration : 0.15f;
            float halfDuration = duration / 2f;
            float intensity = _currentFlashTimer > halfDuration 
                ? Mathf.InverseLerp(duration, halfDuration, _currentFlashTimer) 
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
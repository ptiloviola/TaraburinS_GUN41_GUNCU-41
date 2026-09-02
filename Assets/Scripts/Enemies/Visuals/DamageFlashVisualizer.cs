using UnityEngine;
using Gameplay.Core;

namespace Gameplay.Enemies.Visuals
{
    [RequireComponent(typeof(HealthComponent))]
    public class DamageFlashVisualizer : MonoBehaviour
    {
        [Header("Настройки вспышки")]
        [SerializeField] private Color _flashColor = Color.white;
        [SerializeField] private float _flashDuration = 0.15f;
        
        // Массив больше не виден в инспекторе, скрипт соберет всё сам!
        private Renderer[] _renderers;
        private Color[] _originalColors;
        
        private HealthComponent _health;
        private MaterialPropertyBlock _propBlock;
        
        private float _currentFlashTimer;
        private bool _isFlashing;

        private void Awake()
        {
            _health = GetComponent<HealthComponent>();
            _propBlock = new MaterialPropertyBlock();
            
            // АВТОМАТИЗАЦИЯ: Сам ищет все рендереры внутри префаба (даже в детских объектах)
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
            _health.OnDamaged += PlayFlash;
            _currentFlashTimer = 0f;
            _isFlashing = false;
        }

        private void OnDisable()
        {
            _health.OnDamaged -= PlayFlash;
            ResetColor();
        }

        private void PlayFlash()
        {
            // Просто сбрасываем таймер. НИКАКИХ аллокаций памяти и Sequence!
            _currentFlashTimer = _flashDuration;
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

            // Высчитываем интенсивность от 1 до 0 (треугольный график вспышки)
            float halfDuration = _flashDuration / 2f;
            float intensity = _currentFlashTimer > halfDuration 
                ? Mathf.InverseLerp(_flashDuration, halfDuration, _currentFlashTimer) 
                : Mathf.InverseLerp(0f, halfDuration, _currentFlashTimer);

            ApplyFlashIntensity(intensity);
        }

        private void ApplyFlashIntensity(float intensity)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null) continue;
                
                _renderers[i].GetPropertyBlock(_propBlock);
                Color blendedColor = Color.Lerp(_originalColors[i], _flashColor, intensity);
                
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
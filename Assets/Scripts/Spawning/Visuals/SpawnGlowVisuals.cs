using System.Collections;
using UnityEngine;

namespace Gameplay.Spawning.Visuals
{
    public class SpawnGlowVisuals : MonoBehaviour, ISpawnVisuals
    {
        [Header("Ссылки на компоненты")]
        [Tooltip("Цилиндр, который будет служить столбом света")]
        [SerializeField] private Renderer _glowPillar; 
        [Tooltip("Источник света для окрашивания земли")]
        [SerializeField] private Light _groundLight;   

        [Header("Настройки Огня")]
        [SerializeField] private Color _fireColor = new Color(0.8f, 0f, 0f, 0.6f); // Кроваво-красный
        [SerializeField] private float _maxLightIntensity = 8f;
        [Tooltip("Скорость дрожания пламени")]
        [SerializeField] private float _flickerSpeed = 15f; 

        private Material _pillarMaterial;
        private Coroutine _warningRoutine;

        private void Awake()
        {
            // Подготавливаем столб света
            if (_glowPillar != null)
            {
                // Клонируем материал, чтобы не менять общий ассет проекта
                _pillarMaterial = _glowPillar.material; 
                _pillarMaterial.color = new Color(_fireColor.r, _fireColor.g, _fireColor.b, 0f);
                _glowPillar.gameObject.SetActive(false);
            }

            // Подготавливаем свет
            if (_groundLight != null)
            {
                _groundLight.color = _fireColor;
                _groundLight.intensity = 0f;
                _groundLight.gameObject.SetActive(false);
            }
        }

        public void PlayWarningEffect(float duration)
        {
            if (_warningRoutine != null) StopCoroutine(_warningRoutine);
            _warningRoutine = StartCoroutine(WarningRoutine(duration));
        }

        private IEnumerator WarningRoutine(float duration)
        {
            if (_glowPillar != null) _glowPillar.gameObject.SetActive(true);
            if (_groundLight != null) _groundLight.gameObject.SetActive(true);

            float elapsed = 0f;
            // Случайный сдвиг для шума Перлина, чтобы разные порталы мерцали вразнобой
            float randomOffset = Random.Range(0f, 100f); 

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                
                // 1. Логика появления и затухания (Fade In / Fade Out)
                float fadeMultiplier = 1f;
                if (elapsed < 0.5f) // Первые полсекунды разгораемся
                    fadeMultiplier = elapsed / 0.5f;
                else if (duration - elapsed < 0.3f) // Последние 0.3 секунды резко гаснем
                    fadeMultiplier = (duration - elapsed) / 0.3f;

                // 2. Логика дрожания огня (Шум Перлина дает плавные, но хаотичные скачки)
                float flicker = Mathf.PerlinNoise(randomOffset, Time.time * _flickerSpeed);
                flicker = Mathf.Lerp(0.3f, 1f, flicker); // Ограничиваем, чтобы свет не гас полностью

                float currentIntensity = flicker * fadeMultiplier;

                // 3. Применяем расчеты к свету
                if (_groundLight != null)
                {
                    _groundLight.intensity = _maxLightIntensity * currentIntensity;
                }

                // 4. Применяем расчеты к прозрачности столба (Alpha)
                if (_pillarMaterial != null)
                {
                    Color c = _pillarMaterial.color;
                    c.a = _fireColor.a * currentIntensity;
                    _pillarMaterial.color = c;
                }

                yield return null;
            }

            // Выключаем всё по завершении
            if (_glowPillar != null) _glowPillar.gameObject.SetActive(false);
            if (_groundLight != null) _groundLight.gameObject.SetActive(false);
        }
    }
}
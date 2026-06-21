using System.Collections;
using UnityEngine;
using TMPro;
using System;

namespace Bowling.UI
{
    [RequireComponent(typeof(TMP_Text), typeof(AudioSource))]
    public class StrikeEffect : MonoBehaviour
    {
        private TMP_Text _text;
        private AudioSource _audio;
        
        [Header("Настройки анимации")]
        [SerializeField] private float _animationDuration = 2f;
        [SerializeField] private Vector3 _startScale = new Vector3(0.5f, 0.5f, 0.5f);
        [SerializeField] private Vector3 _endScale = new Vector3(1.5f, 1.5f, 1.5f);

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _audio = GetComponent<AudioSource>();
        }

        public void PlayStrikeSpareEffect(string text)
        {
            gameObject.SetActive(true);
            StartCoroutine(AnimateText(text));
        }

        private IEnumerator AnimateText(string text)
        {
            _audio.Play();
            
            float time = 0f;
            _text.text = text;
            Color originalColor = _text.color;

            while (time < _animationDuration)
            {
                time += Time.deltaTime;
                float normalizedTime = time / _animationDuration;

                float lerpStep = Mathf.SmoothStep(0f, 1f, normalizedTime);

                transform.localScale = Vector3.Lerp(_startScale, _endScale, lerpStep);
                
                if (normalizedTime > 0.7f)
                {
                    float alpha = Mathf.Lerp(1f, 0f, (normalizedTime - 0.7f) / 0.3f);
                    _text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                }

                yield return null;
            }

            _text.color = originalColor;
            gameObject.SetActive(false);
        }
    }
}

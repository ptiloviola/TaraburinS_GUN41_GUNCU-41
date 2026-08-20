using UnityEngine;
using TpsShooter.Combat;
using Cysharp.Threading.Tasks;
using System;

namespace TpsShooter.Environment
{
    public class TestTarget : MonoBehaviour, IDamageable
    {
        private const float BlinkDuration = 0.1f;

        private Renderer _renderer;
        private Color _originalColor;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null) _originalColor = _renderer.material.color;
        }

        public void TakeDamage(float amount)
        {
            DevLogger.Log($"<color=orange>[HIT]</color> Попали по {gameObject.name}! Урон: {amount}");
            
            if (_renderer != null)
            {
                BlinkRedAsync().Forget();
            }
        }

        private async UniTaskVoid BlinkRedAsync()
        {
            _renderer.material.color = Color.red;
            await UniTask.Delay(TimeSpan.FromSeconds(BlinkDuration));
            
            if (_renderer != null) 
            {
                _renderer.material.color = _originalColor;
            }
        }
    }
}
using UnityEngine;
using TpsShooter.Combat;

namespace TpsShooter.Environment
{
    public class TestTarget : MonoBehaviour, IDamageable
    {
        private Renderer _renderer;
        private Color _originalColor;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            if (_renderer != null) _originalColor = _renderer.material.color;
        }

        public void TakeDamage(float amount)
        {
            Debug.Log($"<color=orange>[HIT]</color> Попали по {gameObject.name}! Урон: {amount}");
            
            if (_renderer != null)
            {
                // При попадании кубик на долю секунды будет мигать красным
                _renderer.material.color = Color.red;
                Invoke(nameof(ResetColor), 0.1f);
            }
        }

        private void ResetColor()
        {
            if (_renderer != null) _renderer.material.color = _originalColor;
        }
    }
}
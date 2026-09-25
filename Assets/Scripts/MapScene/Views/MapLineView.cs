using UnityEngine;

namespace Gameplay.MapScene.Views
{
    [RequireComponent(typeof(LineRenderer))]
    public class MapLineView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        
        [Header("Настройки цветов")]
        [SerializeField] private Color _lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _completedColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);

        private void Awake()
        {
            if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
        }

        public void Setup(Vector3 startPoint, Vector3 endPoint)
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, startPoint);
            _lineRenderer.SetPosition(1, endPoint);
        }

        public void SetState(NodeVisualState state)
        {
            Color targetColor = state switch
            {
                NodeVisualState.Locked => _lockedColor,
                NodeVisualState.Available => _availableColor,
                NodeVisualState.Completed => _completedColor,
                _ => _lockedColor
            };

            _lineRenderer.startColor = targetColor;
            _lineRenderer.endColor = targetColor;
        }
    }
}
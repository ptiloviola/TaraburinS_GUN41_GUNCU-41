using UnityEngine;

namespace Gameplay.MapScene.Views
{
    [RequireComponent(typeof(LineRenderer))]
    public class MapLineView : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        
        [Header("Базовые цвета")]
        [SerializeField] private Color _lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color _availableColor = new Color(1f, 1f, 1f, 0.8f);
        [SerializeField] private Color _completedColor = new Color(0.2f, 0.8f, 0.2f, 1f);

        private void Awake()
        {
            if (_lineRenderer == null) _lineRenderer = GetComponent<LineRenderer>();
            
            _lineRenderer.numCapVertices = 5;
            _lineRenderer.numCornerVertices = 5;
            _lineRenderer.sortingOrder = -10;
        }

        public void Setup(Vector3 startPoint, Vector3 endPoint)
        {
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, startPoint);
            _lineRenderer.SetPosition(1, endPoint);
        }

        public void SetState(NodeVisualState state)
        {
            Color finalColor = _lockedColor;

            switch (state)
            {
                case NodeVisualState.Locked:
                    finalColor = _lockedColor;
                    break;
                case NodeVisualState.Available:
                    finalColor = _availableColor;
                    break;
                case NodeVisualState.Completed:

                    finalColor = new Color(_completedColor.r * 2f, _completedColor.g * 2f, _completedColor.b * 2f, 1f);
                    break;
            }

            _lineRenderer.startColor = finalColor;
            _lineRenderer.endColor = finalColor;
        }
    }
}
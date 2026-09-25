using System;
using UnityEngine;
using TMPro;

namespace Gameplay.MapScene.Views
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class MapNodeView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _iconRenderer;
        [SerializeField] private TextMeshPro _nameText;
        
        [Header("Настройки цветов")]
        [SerializeField] private Color _lockedColor = Color.gray;
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _hoverColor = Color.yellow;

        private string _nodeId;
        private NodeVisualState _currentState;

        public string NodeId => _nodeId;
        public event Action<string> OnNodeClicked;

        public void Setup(string id, string displayName, Vector2 worldPosition, Sprite icon)
        {
            _nodeId = id;
            _nameText.text = displayName;
            transform.position = worldPosition;
            
            if (icon != null) _iconRenderer.sprite = icon;
        }

        public void SetState(NodeVisualState state)
        {
            _currentState = state;
            _nameText.fontStyle = FontStyles.Normal;

            switch (state)
            {
                case NodeVisualState.Locked:
                    _iconRenderer.color = _lockedColor;
                    break;
                case NodeVisualState.Available:
                    _iconRenderer.color = _availableColor;
                    break;
                case NodeVisualState.Completed:
                    _iconRenderer.color = _completedColor;
                    _nameText.fontStyle = FontStyles.Strikethrough;
                    break;
            }
        }

        public void OnPointerEnter()
        {
            if (_currentState == NodeVisualState.Available)
                _iconRenderer.color = _hoverColor;
        }

        public void OnPointerExit()
        {
            SetState(_currentState);
        }

        public void OnPointerClick()
        {
            OnNodeClicked?.Invoke(_nodeId);
        }
    }

    public enum NodeVisualState
    {
        Locked,
        Available,
        Completed
    }
}
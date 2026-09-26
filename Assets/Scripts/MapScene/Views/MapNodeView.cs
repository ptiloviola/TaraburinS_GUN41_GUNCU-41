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
        
        [Header("Настройки цветов иконки")]
        [SerializeField] private Color _lockedColor = Color.gray;
        [SerializeField] private Color _availableColor = Color.white;
        [SerializeField] private Color _completedColor = Color.green;
        [SerializeField] private Color _hoverColor = Color.yellow;

        [Header("Свечение (Outline)")]
        [SerializeField] private SpriteRenderer _outlineRenderer;
        
        private string _nodeId;
        private NodeVisualState _currentState;
        private bool _isStartNode;

        public string NodeId => _nodeId;
        public event Action<string> OnNodeClicked;

        public void Setup(string id, string displayName, Vector2 worldPosition, Sprite icon, Color glowColor, bool isStartNode)
        {
            _nodeId = id;
            _nameText.text = displayName;
            transform.position = worldPosition;
            _isStartNode = isStartNode;
            
            if (icon != null) 
            {
                _iconRenderer.sprite = icon;
                
                if (_outlineRenderer != null)
                {
                    _outlineRenderer.sprite = icon; 
                    _outlineRenderer.transform.localScale = Vector3.one;
                    
                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    _outlineRenderer.GetPropertyBlock(mpb);
                    mpb.SetColor("_GlowColor", glowColor);
                    _outlineRenderer.SetPropertyBlock(mpb);

                    _outlineRenderer.enabled = false; 
                }
            }
        }

        public void SetState(NodeVisualState state)
        {
            _currentState = state;
            _nameText.fontStyle = FontStyles.Normal; 

            if (_outlineRenderer != null) _outlineRenderer.enabled = false;

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
                    if (!_isStartNode) _nameText.fontStyle = FontStyles.Strikethrough; 
                    
                    if (_outlineRenderer != null) _outlineRenderer.enabled = true;
                    break;
            }
        }

        public void OnPointerEnter()
        {
            if (_currentState == NodeVisualState.Available)
            {
                _iconRenderer.color = _hoverColor;
                if (_outlineRenderer != null) _outlineRenderer.enabled = true;
            }
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
}
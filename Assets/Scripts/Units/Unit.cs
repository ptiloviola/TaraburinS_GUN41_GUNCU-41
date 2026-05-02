using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.iOS;

public class Unit : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{

    public Cell CurrentCell { set; get; }

    public event Action OnMoveEndCallback;

    [SerializeField]
    private float _moveSpeed = 5f;

    [SerializeField] private GameObject _selectionRing;


    private Coroutine _moveCoroutine;

    [Header("Piece Data")]
    [SerializeField]
    private Team _team;
    [SerializeField]
    private PieceType _pieceType;

    public Team Team => _team;
    public PieceType PieceType => _pieceType;
    
    private IMovementRule _movementRule;

    private void Start()
    {
        InitializeRule();
    }



    private void InitializeRule()
    {
        _movementRule = _pieceType switch
        {
            PieceType.Pawn => new PawnMovementRule(),

            _ => null
        };
    }

    public List<Cell> CalculateAvailableMoves(Battlefield battlefield)
    {
        if (_movementRule == null)
        {
            Debug.LogError($"<color=red>The piece {gameObject.name} has no move rule defined!</color>");
            return new List<Cell>();
        }
        return _movementRule.GetAvailableMoves(this, battlefield);
    }



    public void OnPointerClick(PointerEventData eventData)
    {
        if (CurrentCell != null)
        {
            CurrentCell.OnPointerClick(eventData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CurrentCell != null)
        {
            CurrentCell.OnPointerEnter(eventData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CurrentCell != null)
        {
            CurrentCell.OnPointerExit(eventData);
        }
    }

    public void Move(Cell targetCell)
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

        if (CurrentCell != null)
        {
            CurrentCell.Unit = null;
        }

        CurrentCell = targetCell;

        CurrentCell.Unit = this;

        Vector3 targetPosition = targetCell.transform.position;
        targetPosition.y = transform.position.y;
        _moveCoroutine = StartCoroutine(MoveRoutine(targetPosition));

    }

    private IEnumerator MoveRoutine(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, _moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        OnMoveEndCallback?.Invoke();
    }

    public void SetHighlight(bool isSelected)
    {
        if (_selectionRing != null)
        {
            _selectionRing.SetActive(isSelected);
        }
    }
}

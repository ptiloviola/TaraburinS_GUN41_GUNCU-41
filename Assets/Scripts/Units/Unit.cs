using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{

    public Cell CurrentCell { set; get; }

    public event Action OnMoveEndCallback;

    [SerializeField]
    private float _moveSpeed = 5f;

    [SerializeField] private GameObject _selectionRing;


    private Coroutine _moveCoroutine;



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

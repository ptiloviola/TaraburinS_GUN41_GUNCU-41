using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField]
    private MeshRenderer _focus;

    [SerializeField]
    private MeshRenderer _select;

    public Unit Unit { get; set; }

    private Dictionary<NeighbourType, Cell> _neighbours = new Dictionary<NeighbourType, Cell>();

    public event Action<Cell> OnPointerClickEvent;

    private bool _isHovered = false;

    public void SetSelect(Material material)
    {
        (_select.enabled, _select.sharedMaterial) = (true, material);
    }

    public void ResetSelect()
    {
        _select.enabled = false;
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        _focus.enabled = true;
         _isHovered = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnPointerClickEvent.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _focus.enabled = false;
        _isHovered = false;

    }

    public void AddNeighbour(NeighbourType type, Cell cell)
    {
        if (!_neighbours.ContainsKey(type))
        {
            _neighbours.Add(type, cell);
        }
        
    }


    private void OnDrawGizmos()
    {
        if (!_isHovered || _neighbours == null) return;

        Vector3 offset = Vector3.up * 1.0f;
        Gizmos.color = Color.yellow;

        foreach (var neighbour in _neighbours.Values)
        {
            if (neighbour != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position + offset, neighbour.transform.position + offset);
                Gizmos.DrawSphere(neighbour.transform.position + offset, 0.15f);
            }
        }
    }


}
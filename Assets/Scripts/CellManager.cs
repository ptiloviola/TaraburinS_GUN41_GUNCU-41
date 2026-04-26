
using System;
using Unity.VisualScripting;
using UnityEngine;

public class CellManager : MonoBehaviour
{
    private Cell[] _cells;
    public event Action<Cell> OnCellClicked;

    private void Awake()
    {
        _cells = FindObjectsOfType<Cell>();
        for (int i = 0; i < _cells.Length; i++)
        {
            _cells[i].OnPointerClickEvent += OnCellClicked;

        }
    }

}


using System;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "CellPaletteSettings", menuName = "Settings/CellPaletteSettings")]
public class CellPaletteSettings : ScriptableObject
{
    [field: SerializeField, Space(20f)]
    [field: Tooltip("Cell under selected unit")]

    public Material SelectCell { get; private set; }
    [field: SerializeField]
    [field: Tooltip("Cell allowing for movement")]
    public Material MoveCell { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Cell allowing for attack")]
    public Material AttackCell { get; private set;}

    [field: SerializeField]
    [field: Tooltip("Cell allowing for attack and move")]
    public Material MoveAndAttackCell { get; private set;}

}

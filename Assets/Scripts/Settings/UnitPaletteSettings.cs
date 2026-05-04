using UnityEngine;

[CreateAssetMenu(fileName = "UnitPaletteSettings", menuName = "Settings/UnitPaletteSettings")]
public class UnitPaletteSettings : ScriptableObject
{
    [field: SerializeField, Space(10f)]
    [field: Tooltip("White units material")]
    public Material WhiteUnitMaterial { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Black units material")]
    public Material BlackUnitMaterial { get; private set; }

}

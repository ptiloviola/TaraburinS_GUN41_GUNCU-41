using System;
using System.Collections.Generic;
using System.Linq;
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

    [field: SerializeField]
    [field: Tooltip("Meshes")]
    public List<PieceMeshMapping> MeshMapping;

    public Mesh GetMeshForPiece(PieceType type)
    {
        var mapping = MeshMapping.FirstOrDefault(m => m.Type == type);
        return mapping?.Mesh;
    }
    



}


[Serializable]
public class PieceMeshMapping
{
    public PieceType Type;
    public Mesh Mesh;
}

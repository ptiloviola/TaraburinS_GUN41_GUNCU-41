    using UnityEngine;

namespace Gameplay.Grid
{
    [System.Serializable]
    public struct GridCellData
    {
        public int elevation;
        public NodeType type;
    }

    [System.Serializable]
    public struct GridRow
    {
        public GridCellData[] columns;
    }

    [CreateAssetMenu(fileName = "NewGridConfig", menuName = "TD/Grid Config", order = 51)]
    public class GridConfig : ScriptableObject
    {
        [Header("Размеры сетки")]
        public int width = 5;
        public int height = 5;

        [Header("Карта уровня")]
        public GridRow[] rows;


        public GridCellData GetCellData(int x, int z)
        {
            if (rows != null && x < rows.Length && rows[x].columns != null && z < rows[x].columns.Length)
            {
                return rows[x].columns[z];
            }
            return new GridCellData { elevation = 0, type = NodeType.Ground };
        }

        public void SetCellData(int x, int z, GridCellData data)
        {
            if (rows != null && x < rows.Length && rows[x].columns != null && z < rows[x].columns.Length)
            {
                rows[x].columns[z] = data;
            }
        }
    }
}
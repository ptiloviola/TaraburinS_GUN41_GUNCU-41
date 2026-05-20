using UnityEngine;

namespace Gameplay.Grid
{
    [System.Serializable]
    public struct GridRow
    {
        public int[] columns; // Ячейки в одной строке
    }

    [CreateAssetMenu(fileName = "NewGridConfig", menuName = "TD/Grid Config", order = 51)]
    public class GridConfig : ScriptableObject
    {
        [Header("Размеры сетки")]
        public int width = 5;
        public int height = 5;

        [Header("Карта высот (Строки x Столбцы)")]
        public GridRow[] rows;

        // Метод-помощник, который безопасно выдает высоту для конкретной ячейки [x, z]
        public int GetElevation(int x, int z)
        {
            // Проверяем, что строка существует в конфиге
            if (rows != null && x < rows.Length)
            {
                // Проверяем, что столбец существует в этой строке
                if (rows[x].columns != null && z < rows[x].columns.Length)
                {
                    return rows[x].columns[z];
                }
            }
            return 0; // Возвращаем 0, если вышли за границы (защита от ошибок)
        }
    }
}
using Gameplay.Grid;
using UnityEngine;
using Zenject;

namespace Infrastructure.Levels
{
    public class GridDataInitializer
    {
        private readonly IGridService _gridService;
        private readonly GridConfig _gridConfig;
        private readonly GridSceneReferences _sceneReferences;

        public GridDataInitializer(
            IGridService gridService, 
            GridConfig gridConfig, 
            GridSceneReferences sceneReferences)
        {
            _gridService = gridService;
            _gridConfig = gridConfig;
            _sceneReferences = sceneReferences;
        }

        public void Initialize()
        {
            if (_gridConfig == null)
            {
                Gameplay.Tools.GameLogger.LogError("[GridDataInitializer] Конфиг сетки не передан в контейнер!");
                return;
            }

            int w = _gridConfig.width;
            int h = _gridConfig.height;

            int[,] elevationMap = new int[w, h];
            NodeType[,] typeMap = new NodeType[w, h];

            for (int x = 0; x < w; x++)
            {
                for (int z = 0; z < h; z++)
                {
                    GridCellData cellData = _gridConfig.GetCellData(x, z);
                    elevationMap[x, z] = cellData.elevation;
                    typeMap[x, z] = cellData.type;
                }
            }

            _gridService.InitializeGrid(
                w, 
                h, 
                elevationMap, 
                typeMap, 
                _sceneReferences.Spacing, 
                _sceneReferences.ElevationStep
            );
        }
    }
}
using Gameplay.Grid;
using UnityEngine;
using Unity.AI.Navigation;


namespace Infrastructure.Levels
{
    public class GridVisualBuilder
    {
        private readonly IGridService _gridService;
        private readonly GridSceneReferences _references;

        private const string CustomPathAreaName = "CustomPath";
        private const string CustomGroundAreaName = "CustomGround";
        private const int NotWalkableAreaIndex = 1;
        private const float BaseBlockThickness = 0.2f;

        public GridVisualBuilder(IGridService gridService, GridSceneReferences references)
        {
            _gridService = gridService;
            _references = references;
        }

        public void Initialize()
        {
            int pathAreaIndex = UnityEngine.AI.NavMesh.GetAreaFromName(CustomPathAreaName);
            int groundAreaIndex = UnityEngine.AI.NavMesh.GetAreaFromName(CustomGroundAreaName);

            if (pathAreaIndex == -1 || groundAreaIndex == -1)
            {
                Debug.LogError($"[GridVisualBuilder] ОШИБКА: Зоны {CustomPathAreaName} или {CustomGroundAreaName} не найдены!");
                pathAreaIndex = 0;
                groundAreaIndex = 0;
            }

            for (int x = 0; x < _gridService.Width; x++)
            {
                for (int z = 0; z < _gridService.Height; z++)
                {
                    GridNode node = _gridService.GetNode(new Vector2Int(x, z));
                    float addedHeight = node.Elevation * _references.ElevationStep;
                    Vector3 spawnPosition = new Vector3(x * _references.Spacing, addedHeight / 2f, z * _references.Spacing);

                    GameObject block = Object.Instantiate(
                        _references.CubePrefab, 
                        spawnPosition, 
                        Quaternion.identity, 
                        _references.GridParent
                    );
                    
                    block.name = $"Node_[{x},{z}]_Height_{node.Elevation}";
                    
                    Vector3 currentScale = block.transform.localScale;
                    block.transform.localScale = new Vector3(currentScale.x, BaseBlockThickness + addedHeight, currentScale.z);

                    SetupBlockVisuals(block, node, pathAreaIndex, groundAreaIndex);
                }
            }
        }

        private void SetupBlockVisuals(GameObject block, GridNode node, int pathAreaIndex, int groundAreaIndex)
        {

            if (!block.TryGetComponent(out Renderer blockRenderer))
            {
                blockRenderer = block.GetComponentInChildren<Renderer>();
            }
            
            if (!block.TryGetComponent(out NavMeshModifier modifier))
            {
                modifier = block.AddComponent<NavMeshModifier>();
            }
            
            modifier.overrideArea = true;
            GridTheme theme = _references.Theme;

            if (node.Type == NodeType.Path || node.Type == NodeType.Spawn || node.Type == NodeType.Base)
            {
                modifier.area = pathAreaIndex;
                if (theme != null && theme.GetMaterial(node.Type) != null)
                    blockRenderer.material = theme.GetMaterial(node.Type);
            }
            else if (node.Type == NodeType.Obstacle)
            {
                modifier.area = NotWalkableAreaIndex;
                if (theme != null && theme.obstacleMaterial != null)
                    blockRenderer.material = theme.obstacleMaterial;
            }
            else
            {
                modifier.area = groundAreaIndex;
                if (theme != null && theme.groundMaterial != null)
                    blockRenderer.material = theme.groundMaterial;
            }
        }
    }
}
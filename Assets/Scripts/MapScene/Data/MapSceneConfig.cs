using UnityEngine;
using Gameplay.MapScene.Views;

namespace Gameplay.MapScene.Data
{
    [CreateAssetMenu(fileName = "NewMapSceneConfig", menuName = "TD/Map/Scene Config")]
    public class MapSceneConfig : ScriptableObject
    {
        [Header("Префабы")]
        public MapNodeView NodePrefab;
        public MapLineView LinePrefab;

        [Header("Настройки сетки отрисовки")]
        public float LayerYSpacing = 3f;
        public float NodeXSpacing = 2.5f;
        public float StartYOffset = -4f;
        
        [Header("Шум (смещение узлов)")]
        public Vector2 PositionJitter = new Vector2(0.4f, 0.4f);
    }
}
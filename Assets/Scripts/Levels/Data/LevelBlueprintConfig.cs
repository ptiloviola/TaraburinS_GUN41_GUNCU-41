using UnityEngine;
using Gameplay.Grid;
using Gameplay.Spawning.Data;

namespace Gameplay.Levels.Data
{
    [CreateAssetMenu(fileName = "NewLevelBlueprint", menuName = "TD/Level Blueprint Config")]
    public class LevelBlueprintConfig : ScriptableObject
    {
        [Header("Идентификация")]
        public string LevelId = "Level_01";
        public string DisplayName = "Уровень 1";

        [Header("Стартовые ресурсы")]
        public int StartingLives = 20;
        public int StartingMoney = 100;
        public int FoundationQuota = 5;

        [Header("Данные уровня")]
        [Tooltip("Конфиг сетки (матрица высот и типов ячеек)")]
        public GridConfig GridConfig;
        
        [Tooltip("Конфиг волн (кто и когда нападает)")]
        public LevelWavesConfig WavesConfig;
    }
}
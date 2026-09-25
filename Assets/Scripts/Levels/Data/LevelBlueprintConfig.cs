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
        public int LevelIndex = 1;

        [Header("Визуал на Карте")]
        public Sprite MapIcon;
        public Color MapGlowColor = Color.cyan;
        
        [Header("Стартовые ресурсы")]
        public int StartingLives = 20;
        public int StartingMoney = 100;
        public int FoundationQuota = 5;

        [Header("Награды за прохождение")]
        public int RunCurrencyReward = 50;
        public int MetaCurrencyReward = 10;

        [Header("Данные уровня")]
        public GridConfig GridConfig;
        public LevelWavesConfig WavesConfig;

        
    }
}
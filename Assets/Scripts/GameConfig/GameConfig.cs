using UnityEngine;

namespace VacuumSim.GameConfigs
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "VacuumSim/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Условия игры (Game Rules)")]
        [Tooltip("Процент мусора, при котором наступает поражение (например, 0.2 = 20%)")]
        [SerializeField] private float _gameOverTrashPercent = 0.2f;

        [Header("Влияние Кота (Cat Penalties)")]
        [Tooltip("Множитель скорости мотора, когда кот на пылесосе (0.5 = в два раза медленнее)")]
        [SerializeField] private float _catSpeedMultiplier = 0.5f;
        
        [Tooltip("Множитель расхода батареи, когда кот на пылесосе (3.0 = жрет в 3 раза быстрее)")]
        [SerializeField] private float _catBatteryMultiplier = 3.0f;

        // Геттеры
        public float GameOverTrashPercent => _gameOverTrashPercent;
        public float CatSpeedMultiplier => _catSpeedMultiplier;
        public float CatBatteryMultiplier => _catBatteryMultiplier;
    }
}
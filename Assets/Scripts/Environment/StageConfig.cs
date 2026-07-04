using UnityEngine;

namespace MeatMushrooms.Environment
{
    [CreateAssetMenu(fileName = "StageConfig", menuName = "MeatMushrooms/Stage Config")]
    public class StageConfig : ScriptableObject
    {
        [Header("Стартовые размеры (Уровень 1)")]
        public float MapRadius = 25f;

        [Header("Стартовые препятствия")]
        public int TreeCount = 40;
        public int RockCount = 15;
        
        [Header("Стартовые враги")]
        public int WolfCount = 3;

        // --- НОВЫЙ БЛОК ДЛЯ ГЕЙМДИЗАЙНЕРА ---
        [Header("Прогрессия сложности (Прибавка за каждый уровень)")]
        public float RadiusIncrement = 5f;  // На сколько метров растет карта
        public int TreeIncrement = 10;      // Сколько деревьев добавляется
        public int RockIncrement = 3;       // Сколько камней добавляется
        public int WolfIncrement = 1;       // На сколько растет стая

        [Header("Префабы")]
        public GameObject PlayerPrefab;
        public GameObject WolfPrefab;
        public GameObject ExitPrefab;
        public GameObject[] TreePrefabs;
        public GameObject[] RockPrefabs;
    }
}
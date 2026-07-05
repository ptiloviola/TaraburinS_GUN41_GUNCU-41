using UnityEngine;

namespace MeatMushrooms.Environment
{
    public enum WolfSpawnLayout
    {
        CenterCircle,
        LineAcrossMap
    }

    [CreateAssetMenu(fileName = "StageConfig", menuName = "MeatMushrooms/Stage Config")]
    public class StageConfig : ScriptableObject
    {
        [Header("Стартовые параметры (Уровень 1)")]
        public float MapRadius = 25f;
        public int TreeCount = 40;
        public int RockCount = 15;
        public int WolfCount = 3;
        public int MushroomCount = 10;

        [Header("Прогрессия сложности (Прибавка за уровень)")]
        public float RadiusIncrement = 5f;
        public int TreeIncrement = 10;
        public int RockIncrement = 3;
        public int WolfIncrement = 1;
        public int MushroomIncrement = 4;

        [Header("Настройки ИИ")]
        public WolfSpawnLayout WolfLayout = WolfSpawnLayout.CenterCircle;

        [Header("Префабы")]
        public GameObject PlayerPrefab;
        public GameObject WolfPrefab;
        public GameObject ExitPrefab;
        public GameObject[] TreePrefabs;
        public GameObject[] RockPrefabs;
    }
}
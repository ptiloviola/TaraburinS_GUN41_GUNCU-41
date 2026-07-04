using UnityEngine;

namespace MeatMushrooms.Environment
{
    [CreateAssetMenu(fileName = "StageConfig", menuName = "MeatMushrooms/Stage Config")]
    public class StageConfig : ScriptableObject
    {
        [Header("Размеры поляны")]
        public float MapRadius = 25f;

        [Header("Препятствия")]
        public int TreeCount = 40;
        public int RockCount = 15;
        
        [Header("Волки (Задел на будущее)")]
        public int WolfCount = 3;

        [Header("Префабы")]
        public GameObject[] TreePrefabs;
        public GameObject[] RockPrefabs;

        [Header("Персонажи и Объекты")]
        public GameObject PlayerPrefab; // Префаб Шапочки
        public GameObject WolfPrefab;   // Префаб Волка
        public GameObject ExitPrefab;   // Префаб спасительного выхода (домик или просто зона)



    }
}
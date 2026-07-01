using UnityEngine;

namespace MeatMushrooms.Mushroom.Configs
{
    [CreateAssetMenu(fileName = "MushroomSpawnerConfig", menuName = "MeatMushrooms/Configs/Mushroom Spawner Config")]
    public class MushroomSpawnerConfig : ScriptableObject
    {
        [Header("Timing")]
        public float SpawnInterval = 5f; // Как часто пытаемся заспавнить
        
        [Header("Limits")]
        public int MaxMushroomsOnMap = 15; // Лимит, чтобы не забить всю поляну
        
        [Header("Placement")]
        public float SpawnAreaRadius = 20f; // Зона поиска случайной точки
        // Позже мы добавим сюда слой NavMesh, чтобы спавнер проверял доступность точки
    }
}
using UnityEngine;

namespace MeatMushrooms.Mushroom.Configs
{
    [CreateAssetMenu(fileName = "MushroomSpawnerConfig", menuName = "MeatMushrooms/Configs/Mushroom Spawner Config")]
    public class MushroomSpawnerConfig : ScriptableObject
    {
        [Header("Timing")]
        public float SpawnInterval = 5f;
        
        [Header("Limits")]
        public int MaxMushroomsOnMap = 15;
        
        [Header("Placement")]
        public float SpawnAreaRadius = 20f;
    }
}
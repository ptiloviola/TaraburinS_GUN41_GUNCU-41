using UnityEngine;

namespace MeatMushrooms.Mushroom.Configs
{
    [CreateAssetMenu(fileName = "MushroomConfig", menuName = "MeatMushrooms/Configs/Mushroom Config")]
    public class MushroomConfig : ScriptableObject
    {
        [Header("Visuals")]
        public GameObject Prefab;
        
        [Header("Aroma System")]
        public float MaxAromaRadius = 10f;
        public float AromaSpeed = 2f;
        
        [Header("Stats")]
        public float NutritionValue = 50f;
    }
}
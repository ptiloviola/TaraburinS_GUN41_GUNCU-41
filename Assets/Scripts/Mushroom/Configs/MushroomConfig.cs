using UnityEngine;

namespace MeatMushrooms.Mushroom.Configs
{
    [CreateAssetMenu(fileName = "MushroomConfig", menuName = "MeatMushrooms/Configs/Mushroom Config")]
    public class MushroomConfig : ScriptableObject
    {
        [Header("Visuals")]
        public GameObject Prefab;
        
        [Header("Aroma System")]
        public float MaxAromaRadius = 10f; // До куда доходит запах
        public float AromaSpeed = 2f;      // Как быстро запах распространяется
        
        [Header("Stats")]
        public float NutritionValue = 50f; // Насколько гриб утоляет голод
    }
}
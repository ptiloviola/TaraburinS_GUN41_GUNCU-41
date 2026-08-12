using UnityEngine;

namespace TpsShooter.Items.Configs
{

    [CreateAssetMenu(fileName = "HealthConfig", menuName = "TpsShooter/Items/Health")]
    public class HealthItemConfig : ItemConfig
    {
        public float HealAmount = 25f;
    }
}
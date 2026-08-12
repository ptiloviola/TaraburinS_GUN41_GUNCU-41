using UnityEngine;
using TpsShooter.Player.Inventory;

namespace TpsShooter.Items.Configs
{
    [CreateAssetMenu(fileName = "AmmoConfig", menuName = "TpsShooter/Items/Ammo")]
    public class AmmoItemConfig : ItemConfig
    {
        public AmmoType AmmoType;
        public int Amount = 30;
    }
}
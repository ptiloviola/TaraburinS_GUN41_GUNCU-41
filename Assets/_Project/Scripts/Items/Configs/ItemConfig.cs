using UnityEngine;

namespace TpsShooter.Items.Configs
{
    public abstract class ItemConfig : ScriptableObject
    {
        public string ItemName;
        public Sprite Icon;
        public GameObject Prefab;
        [Header("Audio")]
        [Tooltip("ID звука при подборе предмета")]
        public string PickupSoundId = "Item_Pickup";
    }
}
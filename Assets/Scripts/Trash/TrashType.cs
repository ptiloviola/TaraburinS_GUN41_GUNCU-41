using UnityEngine;

namespace VacuumSim.Trash
{
    [CreateAssetMenu(fileName = "NewTrashType", menuName = "VacuumSim/Trash Type")]
    public class TrashType : ScriptableObject
    {
        public string Title;
        public GameObject Prefab; // Визуальное представление
        public int Points;        // Сколько очков даем при всасывании
        public float FillAmount;  // На сколько % забивает бак (например, 0.1f)
    }
}
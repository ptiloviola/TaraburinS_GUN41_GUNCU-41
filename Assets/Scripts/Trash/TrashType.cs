using UnityEngine;

namespace VacuumSim.Trash
{
    [CreateAssetMenu(fileName = "NewTrashType", menuName = "VacuumSim/Trash Type")]
    public class TrashType : ScriptableObject
    {
        public string Title;
        public GameObject Prefab;
        public int Points;
        public float FillAmount;
    }
}
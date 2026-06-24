using UnityEngine;

namespace VacuumSim.Trash
{
    public class TrashItem : MonoBehaviour
    {
        [SerializeField] private TrashType _trashType;
        public TrashType Type => _trashType;

    }
}

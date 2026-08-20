using UnityEngine;

namespace TpsShooter.Interactables
{
    public interface IPickable
    {
        bool TryPickup(GameObject collector);
    }
}
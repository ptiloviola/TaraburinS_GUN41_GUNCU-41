using UnityEngine;

namespace TpsShooter.Interactables
{
    public interface IPickable
    {
        // Возвращает true, если предмет был успешно подобран (игрок его "поглотил")
        bool TryPickup(GameObject collector);
    }
}
using UnityEngine;

namespace Infrastructure.Interfaces
{
    public interface IDamageable
    {
        void TakeDamage(int amount, Vector3 hitPoint);
    }
}

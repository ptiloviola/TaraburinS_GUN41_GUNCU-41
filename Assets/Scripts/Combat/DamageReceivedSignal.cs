using UnityEngine;

namespace Gameplay.Combat
{
    public struct DamageReceivedSignal
    {
        public Vector3 Position;
        public DamagePayload Payload;

        public DamageReceivedSignal(Vector3 position, DamagePayload payload)
        {
            Position = position;
            Payload = payload;
        }
    }
}
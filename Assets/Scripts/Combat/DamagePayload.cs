namespace Gameplay.Combat
{
    public struct DamagePayload
    {
        public float Amount;
        public DamageType Type;

        public DamagePayload(float amount, DamageType type = DamageType.Physical)
        {
            Amount = amount;
            Type = type;
        }
    }

}
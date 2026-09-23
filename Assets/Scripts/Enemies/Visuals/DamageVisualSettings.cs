using UnityEngine;
using Gameplay.Combat;

namespace Gameplay.Enemies.Visuals
{
    [CreateAssetMenu(fileName = "DamageVisualSettings", menuName = "TD/Visuals/Damage Settings")]
    public class DamageVisualSettings : ScriptableObject
    {
        [Header("Цвета типов урона (HDR для яркости)")]
        [ColorUsage(true, true)] public Color PhysicalColor = Color.white;
        [ColorUsage(true, true)] public Color EnergyColor = Color.cyan;
        [ColorUsage(true, true)] public Color ExplosiveColor = new Color(1f, 0.5f, 0f);

        [Header("Настройки вспышки")]
        public float FlashDuration = 0.15f;

        public Color GetColor(DamageType type) => type switch
        {
            DamageType.Physical => PhysicalColor,
            DamageType.Energy => EnergyColor,
            DamageType.Explosive => ExplosiveColor,
            _ => PhysicalColor
        };
    }
}
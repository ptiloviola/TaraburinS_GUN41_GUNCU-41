using UnityEngine;

namespace Gameplay.Towers.Data.Visuals
{
    [CreateAssetMenu(fileName = "NewVisualSetup", menuName = "TD/Towers/Visuals/Tower Visual Setup")]
    public class TowerVisualSetup : ScriptableObject
    {
        [Header("Обязательные анимации")]
        public BuildVisualData Build;

        [Header("Опциональные модули")]
        public RotationVisualData Rotation;
        public RecoilVisualData Recoil;
        public PulseVisualData Pulse;
        // В будущем сюда легко добавятся:
        // public LaserVisualData Laser;
        // public BarracksVisualData BarracksFlags;
    }
}
using UnityEngine;
using DG.Tweening;

namespace Gameplay.Towers.Data.Visuals
{
    [System.Serializable]
    public class BuildVisualData
    {
        [Range(0.1f, 2f)] public float Duration = 0.5f;
        public Ease EaseType = Ease.OutBack;
    }

    [System.Serializable]
    public class RotationVisualData
    {
        public bool Enabled = true;
        [Range(1f, 50f)] public float Speed = 15f;
    }

    [System.Serializable]
    public class RecoilVisualData
    {
        public bool Enabled = true;
        [Range(0.05f, 1f)] public float Duration = 0.2f;
        [Range(0.1f, 2f)] public float Distance = 0.4f;
        public Ease RecoilEase = Ease.OutQuad;
        public Ease ReturnEase = Ease.OutQuad;
    }

    [System.Serializable]
    public class PulseVisualData
    {
        public bool Enabled = false;
        [Tooltip("Для магических кристаллов или аур")]
        public float PulseScale = 1.2f;
        public float PulseDuration = 1f;
    }
}
using System;
using UnityEngine;

namespace TpsShooter.Environment
{
    public static class GlobalAIEvents
    {
        public static event Action<Vector3, float> OnNoiseGenerated;

        public static void RaiseNoise(Vector3 position, float volumeRadius)
        {
            OnNoiseGenerated?.Invoke(position, volumeRadius);
        }
    }
}
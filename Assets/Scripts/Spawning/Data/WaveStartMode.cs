using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Spawning.Data
{
    // НОВОЕ: Режимы старта волны
    public enum WaveStartMode
    {
        TimeAfterPrevious, // Начнется по таймеру (классика)
        StrictClear        // Начнется только когда умрет последний враг на карте
    }
    
}

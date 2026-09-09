namespace Gameplay.Spawning.Data
{
    /// <summary>
    /// Режимы старта волны
    /// </summary>
    public enum WaveStartMode
    {
        TimeAfterPrevious, // Начнется по таймеру (классика)
        StrictClear        // Начнется только когда умрет последний враг на карте
    }
}
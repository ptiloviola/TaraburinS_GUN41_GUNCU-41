using UnityEngine;
using Gameplay.Towers.Data;
using System;


namespace Gameplay.Towers
{
// Этот скрипт висит на построенной башне на сцене.
public class TowerInstance : MonoBehaviour
{
    public TowerConfig Config { get; private set; }
    public int CurrentLevel { get; private set; }
    public Vector2Int GridPosition { get; private set; }

    // Событие для обновления UI, если башня прокачается
    public event Action OnLevelChanged;

    // Инициализация при постройке
    public void Initialize(TowerConfig config, Vector2Int gridPos)
    {
        Config = config;
        CurrentLevel = 0; // Всегда строим башню 0-го уровня
        GridPosition = gridPos;
    }

    // Удобный метод для получения текущих характеристик
    public TowerLevelData GetCurrentLevelData()
    {
        if (Config != null && Config.Levels.Count > CurrentLevel)
            {
                return Config.Levels[CurrentLevel];
            }
            return null;
    }
    // Удобный метод для проверки, можно ли прокачать
    public bool IsMaxLevel()
    {
        return Config != null && CurrentLevel >= Config.MaxLevel;
    }

    // ЗАГОТОВКА: Метод для будущего апгрейда
    public void Upgrade()
    {
        if (!IsMaxLevel())
        {
            CurrentLevel++;
            OnLevelChanged?.Invoke(); // Уведомляем UI об изменении уровня
            Debug.Log($"[TowerInstance] Башня {Config.DisplayName} улучшена до уровня {CurrentLevel}!");
            // Здесь в будущем мы будем обновлять визуал башни (VisualPrefab)
            // и передавать новые статы в логику стрельбы
        }
    }
}
}


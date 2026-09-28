using System.IO;
using UnityEngine;
using Gameplay.Core.Data;

namespace Gameplay.Core.Services
{
    public class SaveLoadService
    {
        private readonly string _saveFilePath;
        private PlayerProfileModel _cachedProfile;

        public SaveLoadService()
        {
            _saveFilePath = Path.Combine(Application.persistentDataPath, "player_save.json");
            Gameplay.Tools.GameLogger.Log($"<color=magenta>[SaveLoadService] Точный путь к файлу: {_saveFilePath}</color>");
        }

        public PlayerProfileModel LoadProfile()
        {
            if (_cachedProfile != null) return _cachedProfile;

            if (!File.Exists(_saveFilePath))
            {
                Gameplay.Tools.GameLogger.Log("<color=cyan>[SaveLoadService] Файл сохранения не найден. Создан новый профиль.</color>");
                _cachedProfile = new PlayerProfileModel();
                return _cachedProfile;
            }

            try
            {
                string json = File.ReadAllText(_saveFilePath);
                _cachedProfile = JsonUtility.FromJson<PlayerProfileModel>(json);
                Gameplay.Tools.GameLogger.Log("<color=green>[SaveLoadService] Прогресс успешно загружен.</color>");
            }
            catch (System.Exception ex)
            {
                Gameplay.Tools.GameLogger.LogError($"[SaveLoadService] Ошибка чтения сохранения: {ex.Message}. Создан новый профиль.");
                _cachedProfile = new PlayerProfileModel();
            }

            return _cachedProfile;
        }

        public void SaveProfile()
        {
            if (_cachedProfile == null) return;

            try
            {
                string json = JsonUtility.ToJson(_cachedProfile, true);
                File.WriteAllText(_saveFilePath, json);
                Gameplay.Tools.GameLogger.Log("<color=green>[SaveLoadService] Прогресс успешно сохранен.</color>");
            }
            catch (System.Exception ex)
            {
                Gameplay.Tools.GameLogger.LogError($"[SaveLoadService] Ошибка записи сохранения: {ex.Message}");
            }
        }
    }
}
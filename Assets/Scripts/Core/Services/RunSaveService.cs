using System.IO;
using UnityEngine;
using Gameplay.Campaign.Data;

namespace Gameplay.Core.Services
{
    public class RunSaveService
    {
        private readonly string _saveFilePath;

        public RunSaveService()
        {
            _saveFilePath = Path.Combine(Application.persistentDataPath, "current_run.json");
            Debug.Log($"<color=magenta>[RunSaveService] Путь к файлу забега: {_saveFilePath}</color>");
        }

        public bool HasSave()
        {
            return File.Exists(_saveFilePath);
        }

        public void SaveRun(RunSaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(_saveFilePath, json);
                Debug.Log("<color=green>[RunSaveService] Прогресс забега успешно сохранен.</color>");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RunSaveService] Ошибка записи забега: {ex.Message}");
            }
        }

        public RunSaveData LoadRun()
        {
            if (!HasSave()) return null;

            try
            {
                string json = File.ReadAllText(_saveFilePath);
                return JsonUtility.FromJson<RunSaveData>(json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[RunSaveService] Ошибка чтения забега: {ex.Message}. Файл поврежден.");
                return null;
            }
        }

        public void DeleteSave()
        {
            if (HasSave())
            {
                File.Delete(_saveFilePath);
                Debug.Log("<color=red>[RunSaveService] Файл сохранения забега УДАЛЕН (Permadeath / New Game).</color>");
            }
        }
    }
}
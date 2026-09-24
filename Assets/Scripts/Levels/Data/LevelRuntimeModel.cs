
namespace Gameplay.Levels.Data
{
    public class LevelRuntimeModel
    {
        public int StartingLives { get; private set; }
        public int StartingMoney { get; private set; }
        public int FoundationQuota { get; private set; }


        public LevelRuntimeModel(LevelBlueprintConfig blueprint)
        {
            if (blueprint != null)
            {
                StartingLives = blueprint.StartingLives;
                StartingMoney = blueprint.StartingMoney;
                FoundationQuota = blueprint.FoundationQuota;
                
                UnityEngine.Debug.Log($"<color=orange>[LevelRuntimeModel] Загружен {blueprint.name}. Базовые жизни: {StartingLives}, Базовые деньги: {StartingMoney}</color>");
            }
            else
            {
                UnityEngine.Debug.LogError("[LevelRuntimeModel] Блюпринт равен NULL!");
            }
        }
    }
}
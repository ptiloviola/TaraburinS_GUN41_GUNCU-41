using Gameplay.Levels.Data;

namespace Gameplay.Core.Data
{
    public class RunProgressModel
    {
        // Сюда мы кладем уровень перед тем, как загрузить BattleScene
        public LevelBlueprintConfig CurrentLevelBlueprint { get; set; }
        
        // В будущем здесь будет храниться текущий рекорд, прогресс мета-прокачки и т.д.
    }
}
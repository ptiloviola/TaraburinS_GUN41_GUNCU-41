namespace Gameplay.Spawning.Visuals
{
    // Отвечает за всю визуальную репрезентацию точки спавна (портала)
    public interface ISpawnVisuals
    {
        // Вызывается перед началом выхода отряда
        void PlayWarningEffect(float duration);
        
        // Заделы на будущее, которые ты описал:
        // void PlaySpawnAnimation(string enemyType);
        // void PlayBossWarning(float duration);
        // void SetIdleState();
    }
}

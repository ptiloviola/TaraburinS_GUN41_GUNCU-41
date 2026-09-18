using System.Threading;
using Cysharp.Threading.Tasks;

namespace Gameplay.Levels.States
{
    public interface ILevelState
    {
        // Теперь машина передает себя сама в момент старта стейта
        UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct);
        UniTask ExitAsync(CancellationToken ct);
    }
}
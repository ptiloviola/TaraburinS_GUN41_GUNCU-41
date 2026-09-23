using System.Threading;
using Cysharp.Threading.Tasks;

namespace Gameplay.Levels.States
{
    public interface ILevelState
    {
        UniTask EnterAsync(LevelStateMachine stateMachine, CancellationToken ct);
        UniTask ExitAsync(CancellationToken ct);
    }
}
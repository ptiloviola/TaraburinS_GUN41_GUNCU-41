using Cysharp.Threading.Tasks;
using System.Threading;

namespace Gameplay.Infrastructure.Services
{
    public interface ISceneLoaderService
    {
        UniTask LoadSceneAsync(string sceneName, CancellationToken ct = default);
    }
}
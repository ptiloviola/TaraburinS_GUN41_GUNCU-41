using Cysharp.Threading.Tasks;

namespace Gameplay.Infrastructure.Services
{
    public interface ISceneLoaderService
    {
        UniTask LoadSceneAsync(string sceneName);
    }
}
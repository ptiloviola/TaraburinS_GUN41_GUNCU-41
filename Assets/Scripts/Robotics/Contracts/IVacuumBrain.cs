using System.Threading;
using Cysharp.Threading.Tasks;

namespace VacuumSim.Robotics.Contracts
{
    public interface IVacuumBrain
    {
        // Запускает цикл уборки. Возвращает UniTask, чтобы мы могли отслеживать завершение.
        UniTask StartCleaningAsync(CancellationToken token);
    }
}

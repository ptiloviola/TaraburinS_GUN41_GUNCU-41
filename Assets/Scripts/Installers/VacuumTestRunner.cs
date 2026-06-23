using UnityEngine;
using Zenject;
using System.Threading;
using Cysharp.Threading.Tasks;
using VacuumSim.Robotics.Contracts;

namespace VacuumSim.Testing
{
    public class VacuumTestRunner : MonoBehaviour
    {
        private IVacuumBrain _brain;
        private CancellationTokenSource _cts;

        // Zenject сам вызовет этот метод и передаст сюда готовый мозг
        [Inject]
        public void Construct(IVacuumBrain brain)
        {
            _brain = brain;
        }

        private void Start()
        {
            _cts = new CancellationTokenSource();
            
            // Скорость задаем пока жестко для теста.
            // Вызываем Forget(), чтобы Unity не ругалась на не ожидаемую (unawaited) таску
            _brain.StartCleaningAsync(_cts.Token).Forget(); 
        }

        private void OnDestroy()
        {
            // Архитектурная гигиена: всегда отменяем асинхронные задачи при выходе
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }
    }
}

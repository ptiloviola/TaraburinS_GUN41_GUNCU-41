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

        [Inject]
        public void Construct(IVacuumBrain brain)
        {
            _brain = brain;
        }

        private void Start()
        {
            _cts = new CancellationTokenSource();
            
            _brain.StartCleaningAsync(_cts.Token).Forget(); 
        }

        private void OnDestroy()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }
    }
}

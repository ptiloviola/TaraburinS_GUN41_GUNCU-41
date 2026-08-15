using System;
using Zenject;
using TpsShooter.Environment;

namespace TpsShooter.UI.Presenters
{
    public class LevelFlowUIPresenter : IInitializable, IDisposable
    {
        private readonly LevelFlowManager _flowManager;
        private readonly LevelFlowUIView _view;

        public LevelFlowUIPresenter(LevelFlowManager flowManager, LevelFlowUIView view)
        {
            _flowManager = flowManager;
            _view = view;
        }

        public void Initialize()
        {
            _view.Hide(); // Прячем панель на старте
            _flowManager.OnVictoryTransition += HandleVictory;
        }

        public void Dispose()
        {
            _flowManager.OnVictoryTransition -= HandleVictory;
        }

        private void HandleVictory(int nextLevel)
        {
            _view.ShowVictoryMessage(nextLevel);
        }
    }
}
using UnityEngine;
using Zenject;
using Bowling.Gameplay;

namespace Bowling.UI
{
    public class EffectsPresenter : MonoBehaviour
    {
        [SerializeField] private StrikeAndSpareEffect _strikeEffect;
        private GameStateManager _gameStateManager;

        [Inject]
        public void Construct(GameStateManager gameStateManager)
        {
            _gameStateManager = gameStateManager;
            _gameStateManager.OnStrikeOrSpare += PlayEffect;
        }

        private void PlayEffect(string effectText)
        {
            if (_strikeEffect != null)
            {
                _strikeEffect.PlayStrikeSpareEffect(effectText);
            }
        }

        private void OnDestroy()
        {
            if (_gameStateManager != null)
            {
                _gameStateManager.OnStrikeOrSpare -= PlayEffect;
            }
        }
    }
}

using UnityEngine;
using TMPro;

namespace Gameplay.UI.Views
{
    public class BaseUIView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _textLivesComponent;
        [SerializeField] private TMP_Text _textBalanceComponent;

        public void SetLives(int lives)
        {
            _textLivesComponent.text = $"Жизни: {lives}";
        }

        public void SetBalance(int balance)
        {
            _textBalanceComponent.text = $"Баланс: {balance}$";
        }
    }
}
using TMPro;
using UnityEngine;


namespace Bowling.UI.MVP
{
    public class ScoreView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _mainScoreText;
        [SerializeField] private TMP_Text _bestScoreText;

        public void SetMainText(string text)
        {
            if (_mainScoreText != null) _mainScoreText.text = text;
        }

        public void SetBestScoreText(string text)
        {
            if (_bestScoreText != null) _bestScoreText.text = text;
        }

        public void AppendToMainText(string extraText)
        {
            if (_mainScoreText != null) 
            {
                _mainScoreText.text += extraText;
            }
        }
    }
}



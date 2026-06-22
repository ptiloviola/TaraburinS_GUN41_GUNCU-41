using UnityEngine;

namespace Bowling.UI.MVP
{
    public class ScoreModel : MonoBehaviour
    {
        public int CurrentFrame { get; private set; }
        public int CurrentThrow { get; private set; }
        public int CurrentScore { get; private set; }
        public int BestScore { get; private set; }

        public void UpdateData(int frame, int throwNum, int score)
        {
            CurrentFrame = frame;
            CurrentThrow = throwNum;
            CurrentScore = score;
            if (CurrentScore > BestScore)
            {
                BestScore = CurrentScore;
            }
        }

        public void ResetCurrentScore()
        {
            CurrentScore = 0;
        }
    }
}



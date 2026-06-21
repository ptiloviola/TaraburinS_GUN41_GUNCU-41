
using UnityEngine;

namespace Bowling.Gameplay
{
    public class BowlingScoreCalculator
    {
        private int[] _rolls = new int[21];
        private int _currentRollIndex = 0;
        public void AddRoll(int pinsKnockedDown)
        {
            if (_currentRollIndex < 21)
            {
                _rolls[_currentRollIndex] = pinsKnockedDown;
                _currentRollIndex++;
            }
        }

        public int CalculateTotalScore()
        {
            int score = 0;
            int cursor = 0;

            for (int frame = 0; frame < 10; frame++)
            {
                if (IsStrike(cursor))
                {
                    score += 10 + StrikeBonus(cursor);
                    cursor++;
                }
                else if (IsSpare(cursor))
                {
                    score += 10 + SpareBonus(cursor);
                    cursor += 2;
                }
                else
                {
                    score += SumOfBallsInFrame(cursor);
                    cursor += 2;
                }
            }
            return score;
        }

        public void ResetGame()
        {
            _rolls = new int[21];
            _currentRollIndex = 0;
        }
        private bool IsStrike(int cursor) => _rolls[cursor] == 10;
        private bool IsSpare(int cursor) => _rolls[cursor] + _rolls[cursor + 1] == 10;
        private int StrikeBonus(int cursor) => _rolls[cursor + 1] + _rolls[cursor + 2];
        private int SpareBonus(int cursor) => _rolls[cursor + 2];
        private int SumOfBallsInFrame(int cursor) => _rolls[cursor] + _rolls[cursor+1];

    }
}



using System;

namespace KidQuiz.Domain
{
    public static class ScoringRules
    {
        public const int BasePoints = 100;
        public const float MaxTimeBonus = 50f;

        public static int CalculatePoints(bool isCorrect, float secondsRemaining, float secondsPerQuestion)
        {
            if (!isCorrect)
            {
                return 0;
            }

            float timeRatio = secondsPerQuestion <= 0f
                ? 0f
                : Math.Clamp(secondsRemaining / secondsPerQuestion, 0f, 1f);

            float timeBonus = MaxTimeBonus * timeRatio;

            return (int)Math.Round(BasePoints + timeBonus);
        }
    }
}

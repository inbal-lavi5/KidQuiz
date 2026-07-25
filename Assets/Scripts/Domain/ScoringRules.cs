using System;

namespace KidQuiz.Domain
{
    public static class ScoringRules
    {
        public const int BasePoints = 100;
        public const float MaxTimeBonus = 50f;

        public const float DifficultyMultiplierEasy = 1f;
        public const float DifficultyMultiplierMedium = 1.5f;
        public const float DifficultyMultiplierHard = 2f;

        public static int CalculatePoints(bool isCorrect, Difficulty difficulty, float secondsRemaining, float secondsPerQuestion)
        {
            if (!isCorrect)
            {
                return 0;
            }

            float timeRatio = secondsPerQuestion <= 0f
                ? 0f
                : Math.Clamp(secondsRemaining / secondsPerQuestion, 0f, 1f);

            float timeBonus = MaxTimeBonus * timeRatio;
            float difficultyMultiplier = GetDifficultyMultiplier(difficulty);

            return (int)Math.Round((BasePoints + timeBonus) * difficultyMultiplier);
        }

        public static float GetDifficultyMultiplier(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => DifficultyMultiplierEasy,
                Difficulty.Medium => DifficultyMultiplierMedium,
                Difficulty.Hard => DifficultyMultiplierHard,
                _ => DifficultyMultiplierEasy
            };
        }
    }
}

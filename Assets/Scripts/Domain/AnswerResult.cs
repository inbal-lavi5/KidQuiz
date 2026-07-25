namespace KidQuiz.Domain
{
    public readonly struct AnswerResult
    {
        public bool WasCorrect { get; }
        public int PointsAwarded { get; }
        public string CorrectAnswer { get; }

        public AnswerResult(bool wasCorrect, int pointsAwarded, string correctAnswer)
        {
            WasCorrect = wasCorrect;
            PointsAwarded = pointsAwarded;
            CorrectAnswer = correctAnswer;
        }
    }
}

namespace KidQuiz.Presentation
{
    public readonly struct QuizResult
    {
        public int Score { get; }
        public int CorrectCount { get; }
        public int TotalQuestions { get; }

        public QuizResult(int score, int correctCount, int totalQuestions)
        {
            Score = score;
            CorrectCount = correctCount;
            TotalQuestions = totalQuestions;
        }
    }
}

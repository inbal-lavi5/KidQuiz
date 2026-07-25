namespace KidQuiz.Domain
{
    public readonly struct QuizRoundSettings
    {
        public int QuestionCount { get; }
        public Difficulty Difficulty { get; }
        public int CategoryId { get; }

        public QuizRoundSettings(int questionCount, Difficulty difficulty, int categoryId)
        {
            QuestionCount = questionCount;
            Difficulty = difficulty;
            CategoryId = categoryId;
        }
    }
}

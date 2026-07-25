namespace KidQuiz.Domain
{
    public readonly struct QuizRoundSettings
    {
        public int QuestionCount { get; }
        public int CategoryId { get; }

        public QuizRoundSettings(int questionCount, int categoryId)
        {
            QuestionCount = questionCount;
            CategoryId = categoryId;
        }
    }
}

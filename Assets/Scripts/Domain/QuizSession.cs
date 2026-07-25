using System;
using System.Collections.Generic;

namespace KidQuiz.Domain
{
    public sealed class QuizSession
    {
        private readonly List<Question> _questions;
        private readonly float _secondsPerQuestion;
        private readonly bool[] _answered;
        private int _currentIndex;

        public Question CurrentQuestion => IsComplete ? null : _questions[_currentIndex];
        public int Score { get; private set; }
        public int QuestionsAnswered { get; private set; }
        public bool IsComplete => _currentIndex >= _questions.Count;

        public QuizSession(IReadOnlyList<Question> questions, float secondsPerQuestion)
        {
            if (questions == null || questions.Count == 0)
            {
                throw new ArgumentException("QuizSession requires at least one question.", nameof(questions));
            }

            _questions = new List<Question>(questions);
            _secondsPerQuestion = secondsPerQuestion;
            _answered = new bool[_questions.Count];
        }

        public AnswerResult Submit(string answer, float secondsRemaining)
        {
            if (IsComplete)
            {
                throw new InvalidOperationException("Quiz is already complete.");
            }

            var question = _questions[_currentIndex];
            bool wasCorrect = question.IsCorrect(answer);

            if (_answered[_currentIndex])
            {
                return new AnswerResult(wasCorrect, 0, question.CorrectAnswer);
            }

            _answered[_currentIndex] = true;
            QuestionsAnswered++;

            int points = ScoringRules.CalculatePoints(wasCorrect, question.Difficulty, secondsRemaining, _secondsPerQuestion);
            Score += points;

            return new AnswerResult(wasCorrect, points, question.CorrectAnswer);
        }

        public void Advance()
        {
            if (!IsComplete)
            {
                _currentIndex++;
            }
        }
    }
}
